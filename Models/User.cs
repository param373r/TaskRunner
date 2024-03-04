using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToDoWithAuth.Models
{
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Tokens { get; set; }
        public string Timezone { get; set; }
        public Roles Role { get; set; }
        public string Avatar { get; set; }
        
        public User() {
            Tokens = new List<string>();
            Role = Roles.AppUser;
        }
    }

    public enum Roles {
        Admin,
        AppUser
    }

    public class Timezones {
        public List<string> timezones { get; set; }
    }

}