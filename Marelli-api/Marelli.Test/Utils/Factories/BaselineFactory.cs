using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class BaselineFactory
    {
        public static Baseline GetBaseline()
        {
            var project = ProjectFactory.GetProject();

            return new Baseline()
            {
                Id = 0,
                ProjectId = project.Id,
                Description = "test",
                FileName = "test.zip",
                Selected = true,
                UploadDate = DateTime.UtcNow
            };
        }
    }
}
