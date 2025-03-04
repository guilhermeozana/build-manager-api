using Marelli.Domain.Entities;

namespace Marelli.Domain.Dtos
{
    public class GroupResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Image { get; set; }
        public string? CompanyImage { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<Project> Projects { get; set; }
    }
}
