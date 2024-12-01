namespace Hub.Infrastructure.Email.Models
{
    public class SendMailVM
    {
        public string ConfigKey { get; set; }
        public List<string> ListTo { get; set; }
        public List<string> ListCC { get; set; }
        public List<string> ListCCO { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool AsksConfirmation { get; set; }
        public MailAttachment Attachment { get; set; }
        public string EmailDisplay { get; set; }

        #region For SendGrid

        public string MessageOrigin { get; set; }
        public string MessageId { get; set; }

        #endregion

        public SendMailVM()
        {
        }

        public SendMailVM(string subject, string body, params string[] recipients)
        {
            Subject = subject;
            Body = body;
            ListTo = recipients.ToList();
        }
    }
}
