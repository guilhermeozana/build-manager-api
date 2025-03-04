using System.ComponentModel.DataAnnotations.Schema;

namespace Marelli.Domain.Entities;

[Table("UserProject")]
public class UserProject
{
    public int UserId { get; set; }

    public int ProjectId { get; set; }

}