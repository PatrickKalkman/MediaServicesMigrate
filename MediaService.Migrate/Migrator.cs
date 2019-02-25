using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage.Auth;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MediaService.Migrate
{
   public static class Migrator
   {
      const string doneFileName = @"done.txt";
      const string mappingFileName = @"mapping.txt";

      public static void Migrate()
      {
         try
         {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting up");

            CloudMediaContext sourceContext = CloudMediaContextCreator.Create("Source");
            CloudMediaContext destinationContext = CloudMediaContextCreator.Create("Destination");

            StorageCredentials destinationStorageCredentials = StorageCredentialsCreator.Create("Destination");
            StorageCredentials sourceStorageCredentials = StorageCredentialsCreator.Create("Source");

            List<string> done = new List<string>();
            if (File.Exists(doneFileName))
            {
               done = File.ReadAllLines(doneFileName).ToList();
            }
            done.Add($"Started at {DateTime.Now}");

            // Get a reference to the source asset in the source context.
            var allSourceAssets = GetAllAssets(sourceContext);
            var allDestinationAssets =  GetAllAssets(destinationContext);

            Log.Information("Source {0}, Destination {1}", allSourceAssets.Count, allDestinationAssets.Count);

            List<string> mapping = new List<string>();
            if (File.Exists(mappingFileName))
            {
               mapping = File.ReadAllLines(mappingFileName).ToList();
            }
            mapping.Add($"Started at {DateTime.Now}");

            int totalNumberOfAssets = allSourceAssets.Count;
            int currentAsset = 1;
            foreach (IAsset sourceAsset in allSourceAssets)
            {
               Log.Information("Handling {0} of {1}, {2}", currentAsset, totalNumberOfAssets, sourceAsset.Name);
               currentAsset++;
               if (done.Contains(sourceAsset.Id))
               {
                  Log.Information("asset: {0} with id: {1} was already done, skipping asset", sourceAsset.Name, sourceAsset.Id);
                  continue;
               }

               // Create an empty destination asset in the destination context.
               IAsset destinationAsset = destinationContext.Assets.Create(sourceAsset.Name, AssetCreationOptions.None);

               mapping.Add($"asset: source: {sourceAsset.Id}, {sourceAsset.Name} / destination: {destinationAsset.Id.ToString()}, {destinationAsset.Name}");
               StoreMapping(mapping);

               // Copy all the files in the source asset to the destination asset using azcopy (copy containers)
               StorageContainerCopy.CopySourceToDestinationContainer(destinationStorageCredentials, sourceStorageCredentials, sourceAsset, destinationAsset);

               Log.Information("Copy finished, creating files");
               foreach (var sourceAssetFile in sourceAsset.AssetFiles)
               {
                  Log.Information($"Creating {sourceAssetFile.Name}");
                  IAssetFile destinationFile = destinationAsset.AssetFiles.Create(sourceAssetFile.Name);
                  destinationFile.IsPrimary = sourceAssetFile.IsPrimary;
                  destinationFile.MimeType = sourceAssetFile.MimeType;
                  destinationFile.ContentFileSize = sourceAssetFile.ContentFileSize;
                  destinationFile.Update();
               }

               // TODO: Handle dynamic encryption, create locators

               destinationAsset.Update();
               done.Add($"{sourceAsset.Id}");
               StoreDone(done);
            }
            Log.Information("Handled {0} assets", allSourceAssets.Count);
            Log.Information("Shutting down");
         }
         catch (Exception error)
         {
            Log.Error(error, "An error occurred during migration");
            throw;
         }

      }

      public static void StoreMapping(List<string> content)
      {
         File.WriteAllLines(mappingFileName, content);
      }

      public static void StoreDone(List<string> content)
      {
         File.WriteAllLines(doneFileName, content);
      }

      public static List<IAsset> GetAllAssets(CloudMediaContext cloudMediaContext)
      {
         List<IAsset> allAssets = new List<IAsset>();
         const int NumberOfAssetsToRetrieveInSingleCall = 1000;
         int assetCounter = 0;
         List<IAsset> retrievedAssets = null;
         do
         {
            retrievedAssets = cloudMediaContext.Assets.Skip(assetCounter).Take(NumberOfAssetsToRetrieveInSingleCall).ToList();
            allAssets.AddRange(retrievedAssets);
            assetCounter += NumberOfAssetsToRetrieveInSingleCall;
         } while (retrievedAssets.Count == NumberOfAssetsToRetrieveInSingleCall);

         return allAssets;
      }
   }
}
