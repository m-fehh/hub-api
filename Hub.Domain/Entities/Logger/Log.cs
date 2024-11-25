using Hub.Shared.Enums.Infrastructure;
using Hub.Shared.Interfaces;
using Hub.Shared.Interfaces.Logger;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hub.Domain.Entities.Logger
{
    public class Log : BaseEntity, ILog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }

        [Required]
        public DateTime CreateDate { get; set; }

        [ForeignKey("CreateUserId")]
        public long? CreateUser { get; set; }

        [Required]
        public long ObjectId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ObjectName { get; set; }

        [Required]
        public ELogAction Action { get; set; }

        [Required]
        public ELogType LogType { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public virtual ISet<ILogField> Fields { get; set; } = new HashSet<ILogField>();

        [ForeignKey("FatherId")]
        public virtual ILogField Father { get; set; }
        public long? FatherId { get; set; }

        [ForeignKey("OwnerOrgStructId")]
        public long? OwnerOrgStructId { get; set; }

        public string IpAddress { get; set; }
    }
}
