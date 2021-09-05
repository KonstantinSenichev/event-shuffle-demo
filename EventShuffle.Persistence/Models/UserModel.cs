using System.ComponentModel.DataAnnotations;

namespace EventShuffle.Persistence.Models
{
    public class UserModel
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }

        // Add more user properties here if needed
    }
}
