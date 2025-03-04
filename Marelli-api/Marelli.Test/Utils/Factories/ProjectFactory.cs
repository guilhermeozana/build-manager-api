using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class ProjectFactory
    {
        public static Project GetProject()
        {
            return new Project()
            {
                Image = "my-image",
                Name = Guid.NewGuid().ToString(),
                Description = "Description",
                GroupId = 1,
                Group = GroupFactory.GetGroup(),
                UsersProject = new List<UserProject>() { UserProjectFactory.GetUserProject() }
            };
        }

        public static ProjectResponse GetProjectResponse()
        {
            return new ProjectResponse()
            {
                Id = 0,
                Image = "my-image",
                Name = "Name",
                Description = "Description",
                Group = GroupFactory.GetGroupResponse(),
                Users = new List<User>() { UserFactory.GetUser() }
            };
        }
    }
}
