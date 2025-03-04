using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marelli.Domain.Entities;

[Table("User")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Required]
    [StringLength(255)]
    public string Email { get; set; }
    [StringLength(255)]
    public string? Password { get; set; }
    [Required]
    public string Role { get; set; }
    public bool? NewUser { get; set; }
    public string? Name { get; set; }
}