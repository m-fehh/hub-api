namespace Hub.Infrastructure.Email.Models
{
    public class MailConfig
    {
        public string EmailDisplay { get; set; }

        public bool EnableSsl { get; set; }

        public int EmailSMTPPort { get; set; }

        /// <summary>
        /// SMTP Server Address
        /// </summary>
        public string EmailSMTP { get; set; }

        /// <summary>
        /// Email Sender (FROM)
        /// </summary>
        public string Email { get; set; }

        public string EmailUsername { get; set; }

        public string Password { get; set; }

        public EmailSendType EmailSendType { get; set; }

        public string SendGridApiKey { get; set; }

        public string EmailDisplayTemplate { get; set; }

        public bool EnableTLS { get; set; }

        /// <summary>
        /// Indicates whether SMTP Email settings are valid or not.
        /// </summary>
        /// <returns></returns>
        public bool IsSMTPValid()
        {
            return EmailSendType == EmailSendType.SMTP
                && !(string.IsNullOrEmpty(EmailDisplay) &&
                string.IsNullOrEmpty(EmailSMTP) &&
                string.IsNullOrEmpty(Email) &&
                string.IsNullOrEmpty(EmailUsername) &&
                string.IsNullOrEmpty(Password));
        }

        /// <summary>
        /// Indicates whether SendGrid Email settings are valid or not.
        /// </summary>
        /// <returns></returns>
        public bool IsSendGridValid()
        {
            return EmailSendType == EmailSendType.SENDGRID &&
                !(string.IsNullOrEmpty(SendGridApiKey) &&
                string.IsNullOrEmpty(EmailDisplay) &&
                string.IsNullOrEmpty(Email));
        }

        public override string ToString()
        {
            if (EmailSendType == EmailSendType.SMTP)
            {
                return $"EmailSendType: {Enum.GetName(EmailSendType)}, " +
                    $"EmailSMTP: {EmailSMTP}, " +
                    $"EmailSMTPPort: {EmailSMTPPort}, " +
                    $"EnableSsl: {EnableSsl}, " +
                    $"EnableTLS: {EnableTLS}, " +
                    $"EmailDisplay: {EmailDisplay}, " +
                    $"Email: {Email}, " +
                    $"EmailUsername: {EmailUsername}, " +
                    $"Password: {Password}";
            }
            else if (EmailSendType == EmailSendType.SENDGRID)
            {
                return $"EmailSendType: {Enum.GetName(EmailSendType)}, " +
                    $"SendGridApiKey: {SendGridApiKey}, " +
                    $"EmailDisplay: {EmailDisplay}, " +
                    $"Email: {Email}";
            }
            else
            {
                return string.Empty;
            }
        }

    }

    public enum EmailSendType
    {
        /// <summary>
        /// SMTP
        /// </summary>
        SMTP = 1,

        /// <summary>
        /// SendGrid
        /// </summary>
        SENDGRID = 2
    }
}
