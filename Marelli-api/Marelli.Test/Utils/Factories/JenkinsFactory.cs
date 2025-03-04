using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class JenkinsFactory
    {
        public static JenkinsAllDataResponse GetJenkinsAllDataResponse()
        {

            return new JenkinsAllDataResponse()
            {
                BuildingStates = new List<BuildingState>() { GetBuildingState() },
                LogAndArtifacts = new List<LogAndArtifact>() { GetLogAndArtifact() },
                FileVerifies = new List<FileVerify>() { GetFileVerify() },
            };
        }

        public static BuildingState GetBuildingState()
        {

            return new BuildingState()
            {
                UserId = 1,
                ProjectId = 1,
                BuildId = 1,
                JenkinsBuildLogFile = "",
                Date = DateTime.UtcNow,
                Starting = "In Progress",
                StartingDate = DateTime.UtcNow,
                Integrating = "Waiting",
                IntegratingDate = DateTime.UtcNow,
                ApplGen = "Waiting",
                ApplGenDate = DateTime.UtcNow,
                NvmGen = "Waiting",
                NvmGenDate = DateTime.MinValue,
                ParametersGen = "Waiting",
                ParametersGenDate = DateTime.MinValue,
                DiagnoseGen = "Waiting",
                DiagnoseGenDate = DateTime.MinValue,
                NetworkGen = "Waiting",
                NetworkGenDate = DateTime.MinValue,
                RteGen = "Waiting",
                RteGenDate = DateTime.MinValue,
                UpdateIds = "Waiting",
                UpdateIdsDate = DateTime.MinValue,
                Compiling = "Waiting",
                CompilingDate = DateTime.MinValue,
                Finished = "Waiting",
                FinishedDate = DateTime.MinValue,
                Download = "Waiting"
            };
        }

        public static LogAndArtifact GetLogAndArtifact()
        {
            return new LogAndArtifact()
            {
                Id = 0,
                ProjectId = 1,
                UserId = 1,
                BuildId = 1,
                FileLogS3BucketLocation = "test",
                FileOutS3BucketLocation = "test"
            };
        }

        public static FileVerify GetFileVerify()
        {
            return new FileVerify()
            {
                Id = 0,
                BuildId = 1,
                UserId = 1,
                ProjectId = 1,
                Filename = "test.zip",
                FileZipS3BucketLocation = "test",
                IsFileOk = true
            };
        }
    }
}
