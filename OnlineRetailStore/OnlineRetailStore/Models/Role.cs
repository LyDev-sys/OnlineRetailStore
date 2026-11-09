using System.ComponentModel.DataAnnotations;

namespace OnlineRetailStore.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, MaxLength(50)]
        public string RoleName { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<User> Users { get; set; }
    }
}
