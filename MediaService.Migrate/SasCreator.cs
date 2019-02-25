using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace MediaService.Migrate
{
   public static class SasCreator
   {
      public static string GetContainerSasUri(CloudBlobContainer container)
      {
         //Set the expiry time and permissions for the container.
         //In this case no start time is specified, so the shared access signature becomes valid immediately.
         SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
         sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(10);
         sasConstraints.Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read;

         //Generate the shared access signature on the container, setting the constraints directly on the signature.
         string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

         //Return the URI string for the container, including the SAS token.
         return container.Uri + sasContainerToken;
      }
   }
}
