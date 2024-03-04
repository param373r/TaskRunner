
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoWithAuth.Models
{
    public class TaskItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string TitleId { get; set; }

        [Required(ErrorMessage = "Title cannot be empty")]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }
        
        public bool MarkCompleted { get; set; }

        public TaskItem() {
            DueDate = null;
            CreationTime = DateTime.UtcNow;
            ModifiedTime = CreationTime;
            MarkCompleted = false;
        }
    }
}