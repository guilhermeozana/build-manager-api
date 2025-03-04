using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marelli.Domain.Entities
{
    [Table("BuildTableRow")]
    public class BuildTableRow
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }
        public int UserId { get; set; }
        public string Developer { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string TagName { get; set; }
        public string TagDescription { get; set; }
        public string FileName { get; set; }
        public string Md5Hash { get; set; }
        public bool SendNotification { get; set; }
        public bool Deleted { get; set; }
    }
}
