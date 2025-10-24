
using System.ComponentModel.DataAnnotations;

namespace CMCS.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
