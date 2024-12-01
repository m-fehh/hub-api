namespace Hub.Infrastructure.Email.Models
{
    public class MailAttachment
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public Stream Attachment { get; set; }

        public MailAttachment()
        {
        }

        public MailAttachment(Stream content, string fileName)
        {
            FileName = fileName;
            Attachment = content;
        }

        public MailAttachment(Stream content, string fileName, string fileType) : this(content, fileName)
        {
            FileType = fileType;
        }
    }
}
