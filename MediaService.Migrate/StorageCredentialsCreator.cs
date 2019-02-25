using System.Configuration;
using Microsoft.WindowsAzure.Storage.Auth;

namespace MediaService.Migrate
{
   internal static class StorageCredentialsCreator
   {
      internal static StorageCredentials Create(string keyPrefix)
      {
         string storageAccountName = ConfigurationManager.AppSettings[keyPrefix + "StorageAccountName"];
         string storageAccountKey = ConfigurationManager.AppSettings[keyPrefix + "StorageAccountKey"];

         return new StorageCredentials(storageAccountName, storageAccountKey);
      }
   }
}