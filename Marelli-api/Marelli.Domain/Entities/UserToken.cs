using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marelli.Domain.Entities;

[Table("UserToken")]
public class UserToken
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Token { get; set; }

    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }

}