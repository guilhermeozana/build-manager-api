using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Marelli.Business.Exceptions;
using Marelli.Business.IClients;
using Marelli.Business.IServices;
using Marelli.Business.Utils;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace Marelli.Business.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IAmazonS3 _s3Client;
        private readonly IJenkinsRepository _jenkinsRepository;
        private readonly IConfiguration _configuration;
        private readonly IBuildTableRowRepository _buildTableRowRepository;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IAwsClient _awsClient;
        private readonly ITransferUtility _transferUtility;
        private readonly IBaselineService _baselineService;
        private bool error = false;

        public FileService(IConfiguration configuration, IWebHostEnvironment hostingEnvironment, IJenkinsRepository jenkinsRepository, IBuildTableRowRepository buildTableRowRepository, IUserService userService, IProjectService projectService, IAwsClient awsClient, IBaselineService baselineService)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _jenkinsRepository = jenkinsRepository;
            _buildTableRowRepository = buildTableRowRepository;
            _userService = userService;
            _projectService = projectService;
            _awsClient = awsClient;
            _s3Client = awsClient.GetAmazonS3Client();
            _transferUtility = awsClient.GetTransferUtility();
            _baselineService = baselineService;
        }

        public async Task<BuildTableRow> UploadZip(IFormFile file, int userId, int projectId)
        {
            error = false;
            var currentDate = DateTime.UtcNow;
            var currentTimestamp = currentDate.ToString("yyyyMMddHHmmssffff");
            var fileName = $"{projectId}_{userId}_{currentTimestamp}";
            var buildTable = new BuildTableRow();

            if (file == null || file.Length == 0)
            {
                throw new FileNotFoundException("File is empty or null.");
            }

            var bucketExists = await _awsClient.DoesS3BucketExistAsync(_s3Client, _configuration["AWS:BucketName"]);

            if (!bucketExists)
            {
                throw new NoSuchBucketException("S3 bucket does not exist.");
            }

            try
            {
                var user = await _userService.GetUserById(userId);
                var project = await _projectService.GetProjectById(projectId);

                var lastUploaded = await _buildTableRowRepository.GetLastUploadedByUserAsync(userId);

                var build = new BuildTableRow()
                {
                    UserId = userId,
                    ProjectId = projectId,
                    ProjectName = project.Name,
                    Developer = user.Name,
                    Date = currentDate,
                    Status = "Build",
                    FileName = file.FileName,
                    Md5Hash = FileUtils.CalculateMD5(file),
                    TagName = currentTimestamp,
                    TagDescription = "",
                    Deleted = false,
                    SendNotification = lastUploaded != null ? lastUploaded.SendNotification : true
                };

                buildTable = await _buildTableRowRepository.SaveBuildTableAsync(build);

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    await _awsClient.UploadS3File(_transferUtility, memoryStream, _configuration["AWS:BucketName"], $"{projectId}/{userId}/{buildTable.Id}/staging/{fileName}.zip");
                }
            }
            catch (AmazonS3Exception ioex)
            {
                error = true;
                throw new AmazonS3Exception("Error while uploading file: " + ioex.Message);
            }
            catch (Exception ex)
            {
                error = true;
                throw new Exception($"Internal Error: {ex.Message}");
            }
            finally
            {
                if (error && buildTable.Id > 0)
                {
                    var updatedBuildTable = buildTable;
                    updatedBuildTable.Status = "Failed";

                    await _buildTableRowRepository.UpdateBuildTableAsync(buildTable.Id, buildTable, updatedBuildTable);
                }
            }

            return buildTable;

        }

        public async Task RenameZip(string oldDirectory, string oldKey, string newDirectory, string newKey)
        {
            try
            {
                var metadata = await _s3Client.GetObjectMetadataAsync(_configuration["AWS:BucketName"], $"{oldDirectory}/{oldKey}.zip");

                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _configuration["AWS:BucketName"],
                    SourceKey = $"{oldDirectory}/{oldKey}.zip",
                    DestinationBucket = _configuration["AWS:BucketName"],
                    DestinationKey = $"{newDirectory}/{newKey}.zip",
                };

                await _s3Client.CopyObjectAsync(copyRequest);

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _configuration["AWS:BucketName"],
                    Key = oldKey
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro while renaming file: {ex.Message}");
            }
        }

        public async Task<FileResponse> DownloadZip(int buildId)
        {
            var logAndArtifact = await _jenkinsRepository.GetLogAndArtifactByBuildId(buildId);

            if (logAndArtifact == null || logAndArtifact.FileOutS3BucketLocation.IsNullOrEmpty())
            {
                throw new NotFoundException("The log file is not available right now. Please try again later.");
            }

            var objectKey = logAndArtifact.FileOutS3BucketLocation;

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _configuration["AWS:BucketName"],
                Key = objectKey,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            string url = _s3Client.GetPreSignedURL(request);

            var fileName = Regex.Match(objectKey, @"jenkins-(\d+_\d+_\d+)-1-build-artifact.zip").Groups[1].Value + ".zip";

            return new FileResponse
            {
                Name = fileName,
                MimeType = "application/zip",
                Url = url,
            };
        }

        public async Task RemoveZip(int userId, int projectId, int buildId)
        {
            var currentDate = DateTime.UtcNow;
            var buildTable = await _buildTableRowRepository.GetBuildTableRowAsync(buildId);

            if (buildTable == null)
            {
                throw new NotFoundException("Build table row not found.");
            }

            var timestamp = buildTable.Date.ToString("yyyyMMddHHmmssffff");
            var fileName = $"{userId}_{projectId}_{timestamp}";

            var bucketExists = await _awsClient.DoesS3BucketExistAsync(_s3Client, _configuration["AWS:BucketName"]);
            if (!bucketExists)
            {
                throw new NoSuchBucketException("S3 bucket does not exist.");
            }

            var fileKey = $"{projectId}/{userId}/{buildTable.Id}/staging/{fileName}.zip";
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _configuration["AWS:BucketName"],
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);

            await _buildTableRowRepository.DeleteBuildTableAsync(buildTable);
        }

        public async Task<Baseline> UploadBaseline(IFormFile file, int projectId, string description)
        {
            error = false;
            var uploadDate = DateTime.UtcNow;
            var uploadTimestamp = uploadDate.ToString("yyyyMMddHHmmssffff");
            var fileName = "baseline_ipc_328b_mcu.zip";
            var baseline = new Baseline();

            if (file == null || file.Length == 0)
            {
                throw new FileNotFoundException("File is empty or null.");
            }

            var bucketExists = await _awsClient.DoesS3BucketExistAsync(_s3Client, _configuration["AWS:BucketName"]);

            if (!bucketExists)
            {
                throw new NoSuchBucketException("S3 bucket does not exist.");
            }

            try
            {
                var project = await _projectService.GetProjectById(projectId);

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    baseline = new Baseline()
                    {
                        ProjectId = projectId,
                        Project = project,
                        Description = description,
                        FileName = file.FileName,
                        UploadDate = uploadDate,
                        Selected = true
                    };

                    baseline = await _baselineService.SaveBaseline(baseline);

                    await _awsClient.UploadS3File(_transferUtility, memoryStream, _configuration["AWS:BucketName"], $"{projectId}/baseline/{baseline.Id}/{fileName}");
                }
            }
            catch (AmazonS3Exception ioex)
            {
                error = true;
                throw new AmazonS3Exception("Error while uploading file: " + ioex.Message);
            }
            catch (Exception ex)
            {
                error = true;
                throw new Exception($"Internal Error: {ex.Message}");
            }
            finally
            {
                if (error && baseline.Id > 0)
                {
                    await _baselineService.DeleteBaseline(baseline.Id);
                }
            }

            return baseline;
        }

        public async Task<FileResponse> DownloadBaseline(int baselineId)
        {
            var fileName = "baseline_ipc_328b_mcu.zip";
            var baseline = await _baselineService.GetBaseline(baselineId);

            var objectKey = $"{baseline.ProjectId}/baseline/{baseline.Id}/{fileName}";

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _configuration["AWS:BucketName"],
                Key = objectKey,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            string url = _s3Client.GetPreSignedURL(request);

            return new FileResponse
            {
                Name = baseline.FileName,
                MimeType = "application/zip",
                Url = url,
            };
        }

        public async Task RemoveBaseline(int baselineId)
        {
            var currentDate = DateTime.UtcNow;
            var baseline = await _baselineService.GetBaseline(baselineId);
            var fileName = "baseline_ipc_328b_mcu.zip";

            var bucketExists = await _awsClient.DoesS3BucketExistAsync(_s3Client, _configuration["AWS:BucketName"]);
            if (!bucketExists)
            {
                throw new NoSuchBucketException("S3 bucket does not exist.");
            }

            var fileKey = $"{baseline.ProjectId}/baseline/{baseline.Id}/{fileName}.zip";
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _configuration["AWS:BucketName"],
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);

            await _baselineService.DeleteBaseline(baseline.Id);
        }

        public async Task<FileResponse> DownloadLogs(int buildId)
        {
            var logAndArtifact = await _jenkinsRepository.GetLogAndArtifactByBuildId(buildId);

            if (logAndArtifact == null || logAndArtifact.FileLogS3BucketLocation.IsNullOrEmpty())
            {
                throw new NotFoundException("The log file is not available right now. Please try again later.");
            }

            var objectKey = logAndArtifact.FileLogS3BucketLocation;

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _configuration["AWS:BucketName"],
                Key = objectKey,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            string url = _s3Client.GetPreSignedURL(request);

            return new FileResponse
            {
                Name = "compilation.log",
                MimeType = "application/text",
                Url = url,
            };

        }

    }
}
