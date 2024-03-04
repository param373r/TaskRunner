
namespace ToDoWithAuth.Models.DTOs
{
    public class AppTask
    {
        public string TitleId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}