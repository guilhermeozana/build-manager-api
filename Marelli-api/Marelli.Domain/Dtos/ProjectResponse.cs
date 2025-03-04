using Marelli.Domain.Entities;

namespace Marelli.Domain.Dtos
{
    public class ProjectResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public GroupResponse Group { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
