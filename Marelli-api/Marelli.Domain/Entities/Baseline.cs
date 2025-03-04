using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Marelli.Domain.Entities
{
    public class Baseline
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        [JsonIgnore]
        public Project? Project { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public bool Selected { get; set; }

    }
}
