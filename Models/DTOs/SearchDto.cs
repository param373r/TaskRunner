
namespace ToDoWithAuth.Models.DTOs
{
    public class SearchDto
    {
        public string SearchString { get; set; }
        public bool? IsComplete { get; set; }
        public bool? IsModified { get; set; }
        public bool? IsDue { get; set; }
        // public int step { get; set; }
        // public int page { get; set; }
    }
}