using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.SecretsManager;

namespace Marelli.Business.IClients
{
    public interface IAwsClient
    {
        //S3
        public IAmazonS3 GetAmazonS3Client();

        public ITransferUtility GetTransferUtility();

        public Task<bool> DoesS3BucketExistAsync(IAmazonS3 s3Client, string bucketName);

        public Task UploadS3File(ITransferUtility transferUtility, Stream stream, string bucketName, string key, CancellationToken cancellationToken = default);

        //Secrets Manager
        public IAmazonSecretsManager GetAmazonSecretsManagerClient();

    }
}
