using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;

namespace SearchEngine_.utils;

public class AmazonS3
{
    
    public class S3Uploader
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
    
        public S3Uploader(IAmazonS3 s3Client, string bucketName)
        {
            _s3Client = s3Client;
            _bucketName = bucketName;
        }
    
        public async Task UploadFileAsync(string filePath, string keyName)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var putObjectRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = keyName,
                        InputStream = fileStream
                    };
                    await _s3Client.PutObjectAsync(putObjectRequest);
                    System.Console.WriteLine($"Successfully uploaded {keyName} to {_bucketName}");
                }
            }
            catch (AmazonS3Exception e)
            {
                System.Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when writing an object");
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Unknown encountered on server. Message:'{e.Message}' when writing an object");
            }
        }
    }
}