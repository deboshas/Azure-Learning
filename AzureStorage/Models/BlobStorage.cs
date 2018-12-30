using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace FileUploader.Models
{
    public class BlobStorage : IStorage
    {
        private readonly AzureStorageConfig storageConfig;

        public BlobStorage(IOptions<AzureStorageConfig> storageConfig)
        {
            this.storageConfig = storageConfig.Value;
        }

        public Task Initialize()
        {
            // Add Initialize code here
            var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            var cloudblobClinet = storageAccount.CreateCloudBlobClient();
            var cloudblobConatiner = cloudblobClinet.GetContainerReference(storageConfig.FileContainerName);
            return cloudblobConatiner.CreateIfNotExistsAsync();
        }

        public Task Save(Stream fileStream, string name)
        {
            // Add Save code here
            var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            var cloudblobClinet = storageAccount.CreateCloudBlobClient();
            var cloudblobConatiner = cloudblobClinet.GetContainerReference(storageConfig.FileContainerName);
            var blobReference = cloudblobConatiner.GetBlockBlobReference(name);
             blobReference.UploadFromStreamAsync(fileStream);
            return null;


        }

        public async Task<IEnumerable<string>> GetNames()
        {
            var names = new List<string>();

            var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            var cloudblobClinet = storageAccount.CreateCloudBlobClient();
            var cloudblobConatiner = cloudblobClinet.GetContainerReference(storageConfig.FileContainerName);

            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;

            do
            {
                resultSegment = await cloudblobConatiner.ListBlobsSegmentedAsync(continuationToken);
                names.AddRange(resultSegment.Results.OfType<ICloudBlob>().Select(b => b.Name));
                continuationToken = resultSegment.ContinuationToken;

            } while (continuationToken != null);
            return names;


        }

        public  Task<Stream> Load(string name)
        {
            // Add Load code here
            var storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            var cloudblobClinet = storageAccount.CreateCloudBlobClient();
            var cloudblobConatiner = cloudblobClinet.GetContainerReference(storageConfig.FileContainerName);
            var blobReference = cloudblobConatiner.GetBlobReference(name);
            return blobReference.OpenReadAsync();
        }
    }
}