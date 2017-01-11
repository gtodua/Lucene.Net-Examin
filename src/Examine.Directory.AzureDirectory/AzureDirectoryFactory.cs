using System;
using System.Configuration;
using System.IO;
using System.Web;
using Examine.Directory.Sync;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Store;
using Microsoft.WindowsAzure.Storage;

namespace Examine.Directory.AzureDirectory
{
    /// <summary>
    /// The azure umbraco content indexer.
    /// </summary>
    public class AzureDirectoryFactory : EnvironmentTempLocationDirectoryFactory
    {
        /// <summary>
        /// Get/set the config storage key
        /// </summary>
        public static string ConfigStorageKey = "examine:AzureStorageConnString";

        /// <summary>
        /// Get/set the config container key
        /// </summary>
        public static string ConfigContainerKey = "examine:AzureStorageContainer";

        /// <summary>
        /// Return the AzureDirectory.
        /// It stores the master index in Blob storage.
        /// Only a master server can write to it.
        /// For each slave server, the blob storage index files are synced to the local machine.
        /// </summary>
        /// <param name="indexer">
        /// The indexer.
        /// </param>
        /// <param name="luceneIndexFolder">
        /// The lucene index folder.
        /// </param>
        /// <returns>
        /// The <see cref="Lucene.Net.Store.Directory"/>.
        /// </returns>
        public override Lucene.Net.Store.Directory CreateDirectory(LuceneIndexer indexer, string luceneIndexFolder)
        {
            var indexFolder = new DirectoryInfo(luceneIndexFolder);
            var tempFolder = GetLocalStorageDirectory(indexFolder);

            return new AzureDirectory(
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings[ConfigStorageKey]),
                ConfigurationManager.AppSettings[ConfigContainerKey],
                new SimpleFSDirectory(tempFolder),
                rootFolder: indexer.IndexSetName);
        }
        
    }
}