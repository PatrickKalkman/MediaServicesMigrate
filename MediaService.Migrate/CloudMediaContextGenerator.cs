using System;
using System.Configuration;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace MediaService.Migrate
{
   /// <summary>
   /// This class is responsible for creating the Azure Media Services Cloud Media Context
   /// </summary>
   internal static class CloudMediaContextCreator
   {
      private const string SourceAzureTenantNameKey = "AzureTenantName";
      private const string SourceApiClientIdKey = "ApiClientIdKey";
      private const string SourceApiSecretKey = "ApiSecretKey";
      private const string SourceMediaServicesApiUrlKey = "MediaServicesApiUrl";

      internal static CloudMediaContext Create(string keyPrefix)
      {
         string tenantName = ConfigurationManager.AppSettings[keyPrefix + SourceAzureTenantNameKey];
         string clientId = ConfigurationManager.AppSettings[keyPrefix + SourceApiClientIdKey];
         string secret = ConfigurationManager.AppSettings[keyPrefix + SourceApiSecretKey];
         string mediaServicesApiUrl = ConfigurationManager.AppSettings[keyPrefix + SourceMediaServicesApiUrlKey];

         var tokenCredentials = new AzureAdTokenCredentials(
            tenantName,
            new AzureAdClientSymmetricKey(clientId, secret),
            AzureEnvironments.AzureCloudEnvironment);

         var tokenProvider = new AzureAdTokenProvider(tokenCredentials);

         return new CloudMediaContext(new Uri(mediaServicesApiUrl), tokenProvider);
      }
   }
}