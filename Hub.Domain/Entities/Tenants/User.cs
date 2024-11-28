//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;

//namespace Hub.Domain.Entities.Tenants
//{
//    [Table("Users")]
//    public class User
//    {
//        // Definindo a chave primária da tabela
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // A chave primária é gerada automaticamente
//        public int Id { get; set; }

//        // Propriedade para o nome do usuário
//        [Required] // Garantindo que o campo seja obrigatório
//        [MaxLength(100)] // Limitando o tamanho máximo da coluna
//        [Column("UserName")] // Definindo o nome da coluna no banco
//        public string UserName { get; set; }

//        // Propriedade para o email
//        [Required]
//        [EmailAddress] // Garantindo que o valor seja um email válido
//        [MaxLength(200)]
//        [Column("Email")]
//        public string Email { get; set; }

//        // Propriedade para a senha
//        [Required]
//        [MaxLength(256)] // O tamanho da senha pode variar, dependendo da implementação
//        [Column("PasswordHash")]
//        public string PasswordHash { get; set; }

//        // Propriedade de data de nascimento
//        [Column("DateOfBirth")]
//        public DateTime? DateOfBirth { get; set; }

//        // Propriedade para o status ativo do usuário
//        [Required]
//        [Column("IsActive")]
//        public bool IsActive { get; set; }

//        // Data de criação do usuário
//        [Column("CreatedAt")]
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//        // Data de atualização do usuário
//        [Column("UpdatedAt")]
//        public DateTime? UpdatedAt { get; set; }

//        // Soft delete (opcional, se necessário)
//        [Column("IsDeleted")]
//        public bool IsDeleted { get; set; } = false;

//        // Método para configurar um "soft delete" (se aplicável)
//        public void Delete()
//        {
//            IsDeleted = true;
//            UpdatedAt = DateTime.UtcNow;
//        }

//        // Método para reativar o usuário
//        public void Reactivate()
//        {
//            IsDeleted = false;
//            UpdatedAt = DateTime.UtcNow;
//        }
//    }
//}
