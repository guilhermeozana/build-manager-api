using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class GroupFactory
    {
        public static Group GetGroup()
        {
            return new Group()
            {
                Image = "my-image",
                Name = Guid.NewGuid().ToString(),
                CompanyImage = "company-image-1"
            };
        }

        public static GroupResponse GetGroupResponse()
        {
            return new GroupResponse()
            {
                Id = 0,
                Image = "my-image",
                Name = "Name",
                CompanyImage = "company-image-1",
                Projects = new List<Project>() { ProjectFactory.GetProject() },
                Users = new List<User>() { UserFactory.GetUser() }
            };
        }

        public static GroupRequest GetGroupRequest()
        {
            return new GroupRequest()
            {
                Id = 0,
                Image = "my-image",
                Name = Guid.NewGuid().ToString(),
                CompanyImage = "company-image-1",
                UserIds = new int[] { 1 }
            };
        }
    }
}
