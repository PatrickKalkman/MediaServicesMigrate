using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Serilog;
using System.Diagnostics;

namespace MediaService.Migrate
{
   public static class StorageContainerCopy
   {
      public static void CopySourceToDestinationContainer(StorageCredentials destinationStorageCredentials, StorageCredentials sourceStorageCredentials, IAsset sourceAsset, IAsset destinationAsset)
      {
         Log.Information("Starting copy from {0}", sourceAsset.Name);

         CloudBlobContainer sourceContainer = new CloudBlobContainer(sourceAsset.Uri, sourceStorageCredentials);
         CloudBlobContainer destinationContainer = new CloudBlobContainer(destinationAsset.Uri, destinationStorageCredentials);

         string sourceSasUri = SasCreator.GetContainerSasUri(sourceContainer);
         string destinationSasUri = SasCreator.GetContainerSasUri(destinationContainer);

         string arguments = $"cp \"{sourceSasUri}\" \"{destinationSasUri}\" --recursive=true";

         ProcessStartInfo startInfo = new ProcessStartInfo(@"azcopy.exe", arguments);
         var result = Process.Start(startInfo);
         result.WaitForExit();
      }
   }
}
