using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class BuildTableRowFactory
    {
        public static BuildTableRow GetBuildTable()
        {
            return new BuildTableRow()
            {
                ProjectId = 1,
                UserId = 1,
                Date = DateTime.UtcNow,
                Developer = "test",
                FileName = Guid.NewGuid().ToString(),
                Md5Hash = "test",
                ProjectName = "test",
                SendNotification = true,
                Status = "Build",
                TagDescription = "test",
                TagName = "test"
            };
        }
    }
}
