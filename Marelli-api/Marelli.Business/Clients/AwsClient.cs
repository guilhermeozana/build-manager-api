using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.SecretsManager;
using Marelli.Business.Hubs;
using Marelli.Business.IClients;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace Marelli.Business.Clients
{
    public class AwsClient : IAwsClient
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<UploadProgressHub> _hubContext;


        public AwsClient(IConfiguration configuration, IHubContext<UploadProgressHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }

        //S3

        public virtual IAmazonS3 GetAmazonS3Client()
        {
            //return new AmazonS3Client(RegionEndpoint.GetBySystemName(_configuration["AWS:Region"]));
            return new AmazonS3Client(_configuration["AWS:AccessKeyId"], _configuration["AWS:SecretAccessKey"], RegionEndpoint.GetBySystemName(_configuration["AWS:Region"]));
        }

        public ITransferUtility GetTransferUtility()
        {
            return new TransferUtility(GetAmazonS3Client());
        }

        public async Task<bool> DoesS3BucketExistAsync(IAmazonS3 s3Client, string bucketName)
        {
            return await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
        }

        public async Task UploadS3File(ITransferUtility transferUtility, Stream stream, string bucketName, string key, CancellationToken cancellationToken = default)
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                BucketName = bucketName,
                Key = key,
                ContentType = "application/zip"
            };

            uploadRequest.UploadProgressEvent += (sender, args) =>
            {
                var progressPercentage = (args.TransferredBytes * 100) / args.TotalBytes;

                Task.Run(async () =>
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveProgress", progressPercentage);
                });
            };

            await transferUtility.UploadAsync(uploadRequest, cancellationToken);
        }

        // Secrets Manager

        public IAmazonSecretsManager GetAmazonSecretsManagerClient()
        {
            //return new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_configuration["AWS:Region"]));
            return new AmazonSecretsManagerClient(_configuration["AWS:AccessKeyId"], _configuration["AWS:SecretAccessKey"], RegionEndpoint.GetBySystemName(_configuration["AWS:Region"]));
        }




    }
}