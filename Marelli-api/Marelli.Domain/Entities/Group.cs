using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Marelli.Domain.Entities;

[Table("Group")]
public class Group
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string? Image { get; set; }
    public string? CompanyImage { get; set; }

    [JsonIgnore]
    public ICollection<Project>? Projects { get; set; } = new List<Project>();

    [JsonIgnore]
    public ICollection<UserGroup> UsersGroup { get; set; } = new List<UserGroup>();
}