using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marelli.Domain.Entities
{
    public class News
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        [StringLength(255)]
        public string Description { get; set; }
    }
}
