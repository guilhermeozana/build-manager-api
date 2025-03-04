using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class BuildLogFactory
    {
        public static BuildLog GetBuildLog()
        {
            return new BuildLog()
            {
                Id = 0,
                ProjectId = 0,
                UserId = 0,
                BuildId = 0,
                Date = DateTime.UtcNow,
                Status = "Build",
                Details = "test",
                Type = "INFO"
            };
        }
    }
}
