using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Marelli.Domain.Entities;

[Table("Project")]
public class Project
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string? Description { get; set; }

    public string? Image { get; set; }

    [ForeignKey("Group")]
    public int GroupId { get; set; }

    [JsonIgnore]
    public Group? Group { get; set; }
    public ICollection<UserProject> UsersProject { get; set; } = new List<UserProject>();

}