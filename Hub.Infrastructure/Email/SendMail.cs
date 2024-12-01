using System.Net.Mail;
using System.Net;
using SendGrid;
using SendGrid.Helpers.Mail;
using MimeKit;
using MailKit.Security;
using Hub.Infrastructure.Email.Interfaces;
using Hub.Infrastructure.Email.Models;
using Hub.Infrastructure.Extensions;
using Hub.Infrastructure.Autofac;
using Hub.Infrastructure.Logger;

namespace Hub.Infrastructure.Email
{
    public class SendMail : ISendMail
    {
        private readonly IMailConfigProvider _mailConfigProvider;

        public SendMail(IMailConfigProvider mailConfigProvider)
        {
            _mailConfigProvider = mailConfigProvider;
        }

        public async Task<HttpStatusCode?> Send(SendMailVM model)
        {
            HttpStatusCode? response = null;

            try
            {
                var config = _mailConfigProvider.GetConfig(model.ConfigKey);

                if (config.EmailSendType == EmailSendType.SENDGRID)
                {
                    if (!config.IsSendGridValid())
                    {
                        throw new BusinessException($"{Engine.Get("CheckSendGridSettings")}, {config}");
                    }

                    response = await SendBySendGrid(model, config);
                }
                else if (config.EmailSendType == EmailSendType.SMTP)
                {
                    if (!config.IsSMTPValid())
                    {
                        throw new BusinessException($"{Engine.Get("CheckSMTPSettings")}, {config}");
                    }

                    SendBySMTP(model, config);

                    response = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                LogErrorSendMail(ex);
            }

            return response;
        }

        private void LogErrorSendMail(Exception ex)
        {
            Singleton<Log4NetManager>.Instance.Error("Sch.SendEmail", ex.Message, ex);
        }

        private async Task<HttpStatusCode?> SendBySendGrid(SendMailVM model, MailConfig config)
        {
            var client = new SendGridClient(config.SendGridApiKey);
            var from = new EmailAddress(config.Email, model.EmailDisplay ?? config.EmailDisplay);
            var listTo = model.ListTo.Select(s => new EmailAddress { Email = s }).ToList();
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, listTo, model.Subject, "", model.Body);

            if (!string.IsNullOrEmpty(model.MessageId))
            {
                msg.AddCustomArg("MessageId", model.MessageId);
                msg.AddCustomArg("MessageOrigin", model.MessageOrigin);
            }

            if (model.Attachment != null)
            {
                var file = Convert.ToBase64String(ConvertAttachment(model.Attachment.Attachment));
                msg.AddAttachment(model.Attachment.FileName, file);
            }

            Response response = null;

            try
            {
                response = await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                var listToString = string.Join(",", listTo.Select(s => s.Email).ToArray());

                Singleton<Log4NetManager>.Instance.Error("Sch.SendEmail", $"Para: {listToString} Retorno: {ex.Message}", ex);
            }

            if (response?.StatusCode != HttpStatusCode.OK &&
                response?.StatusCode != HttpStatusCode.Accepted)
            {
                var body = await response.Body.ReadAsStringAsync();

                var listToString = string.Join(",", listTo.Select(s => s.Email).ToArray());

                Singleton<Log4NetManager>.Instance.Error("Sch.SendEmail", $"Para: {listToString} Retorno: {body}");
            }

            return response?.StatusCode;
        }

        private byte[] ConvertAttachment(Stream attachment)
        {
            byte[] byteArray = new byte[16 * 1024];
            using (MemoryStream mStream = new MemoryStream())
            {
                int bit;
                while ((bit = attachment.Read(byteArray, 0, byteArray.Length)) > 0)
                {
                    mStream.Write(byteArray, 0, bit);
                }
                return mStream.ToArray();
            }
        }

        private void SendBySMTP(SendMailVM model, MailConfig config)
        {
            var useMailKit = Engine.AppSettings["UseSMTPMailKit"];

            if ("true".Equals(useMailKit, StringComparison.InvariantCultureIgnoreCase))
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(model.EmailDisplay ?? config.EmailDisplay, config.Email));

                MimePart attachment = null;
                if (model.Attachment != null)
                {
                    attachment = new MimePart(model.Attachment.FileType)
                    {
                        Content = new MimeContent(model.Attachment.Attachment),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Default,
                        FileName = Path.GetFileName(model.Attachment.FileName)
                    };
                }

                if (model.ListTo != null)
                {
                    foreach (var item in model.ListTo)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            message.To.Add(new MailboxAddress(item, item));
                        }
                    }
                }

                if (model.ListCC != null)
                {
                    foreach (var item in model.ListCC)
                    {
                        if (!string.IsNullOrEmpty(item.Trim()))
                        {
                            message.Cc.Add(new MailboxAddress(item, item));
                        }
                    }
                }

                // create our message text, just like before (except don't set it as the message.Body)
                var body = new TextPart("html")
                {
                    Text = model.Body
                };

                var multipart = new Multipart("mixed");
                multipart.Add(body);

                if (attachment != null)
                    multipart.Add(attachment);


                message.Subject = model.Subject;
                message.Body = multipart;

                if (model.AsksConfirmation)
                    message.Headers.Add("Disposition-Notification-To", config.Email);

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.CheckCertificateRevocation = false;

                    var port = config.EmailSMTPPort > 0 ? config.EmailSMTPPort : 587;

                    if (config.EnableTLS)
                    {
                        client.Connect(config.EmailSMTP, port, SecureSocketOptions.StartTls);
                    }
                    else
                    {
                        client.Connect(config.EmailSMTP, port, config.EnableSsl);
                    }

                    // Note: only needed if the SMTP server requires authentication
                    if (!string.IsNullOrEmpty(config.Password))
                    {
                        var username = config.EmailUsername;
                        if (string.IsNullOrEmpty(username)) username = config.Email;

                        client.Authenticate(username, config.Password);
                    }

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            else
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(config.Email, model.EmailDisplay ?? config.EmailDisplay);

                    if (model.Attachment != null)
                        mail.Attachments.Add(new System.Net.Mail.Attachment(model.Attachment.Attachment, model.Attachment.FileName, model.Attachment.FileType));

                    if (model.ListTo != null)
                    {
                        foreach (var item in model.ListTo)
                        {
                            if (!string.IsNullOrEmpty(item.Trim()))
                            {
                                mail.To.Add(item);
                            }
                        }
                    }

                    if (model.ListCC != null)
                    {
                        foreach (var item in model.ListCC)
                        {
                            if (!string.IsNullOrEmpty(item.Trim()))
                            {
                                mail.CC.Add(item);
                            }
                        }
                    }

                    if (model.ListCCO != null)
                    {
                        foreach (var item in model.ListCCO)
                        {
                            if (!string.IsNullOrEmpty(item.Trim()))
                            {
                                mail.Bcc.Add(item);
                            }
                        }
                    }

                    mail.Subject = model.Subject;
                    mail.Body = model.Body;
                    mail.IsBodyHtml = true;

                    if (model.AsksConfirmation) mail.Headers.Add("Disposition-Notification-To", config.Email);

                    SmtpClient smtp;

                    if (config.EmailSMTPPort > 0)
                    {
                        smtp = new SmtpClient(config.EmailSMTP, config.EmailSMTPPort);
                    }
                    else
                    {
                        smtp = new SmtpClient(config.EmailSMTP);
                    }

                    if (!string.IsNullOrEmpty(config.Password))
                    {
                        var username = config.EmailUsername;

                        if (string.IsNullOrEmpty(username)) username = config.Email;

                        smtp.Credentials = new NetworkCredential(username, config.Password);
                    }

                    smtp.EnableSsl = config.EnableSsl || config.EnableTLS;

                    smtp.Send(mail);
                }
            }
        }
    }
}
