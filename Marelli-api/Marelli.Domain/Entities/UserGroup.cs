using System.ComponentModel.DataAnnotations.Schema;

namespace Marelli.Domain.Entities;

[Table("UserGroup")]
public class UserGroup
{
    public int UserId { get; set; }

    public int GroupId { get; set; }

}