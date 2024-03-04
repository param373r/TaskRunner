
namespace ToDoWithAuth.Models.DTOs
{
    public class AvatarDto
    {
        public IFormFile ImageData { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
    }
}