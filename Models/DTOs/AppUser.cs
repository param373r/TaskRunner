
namespace ToDoWithAuth.Models.DTOs
{
    public class AppUser
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Timezone { get; set; }
        public string Role { get; set; }
    }
}