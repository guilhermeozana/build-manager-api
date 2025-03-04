using Marelli.Domain.Entities;

namespace Marelli.Domain.Dtos
{
    public class JenkinsAllDataResponse
    {
        public List<BuildingState> BuildingStates { get; set; }
        public List<LogAndArtifact> LogAndArtifacts { get; set; }
        public List<FileVerify> FileVerifies { get; set; }
    }

}
