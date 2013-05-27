﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UmbracoExamine.Config;
using UmbracoExamine;
using Examine.Test.DataServices;
using System.Threading;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Examine.Providers;
using System.Collections.Specialized;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Search;

namespace Examine.Test
{
    /// <summary>
    /// Used internally by test classes to initialize a new index from the template
    /// </summary>
    internal static class IndexInitializer
    {
		public static UmbracoContentIndexer GetUmbracoIndexer(Lucene.Net.Store.Directory luceneDir)
        {
	        
            var i = new UmbracoContentIndexer(new IndexCriteria(
                                                         new[]
                                                             {
                                                                 new TestIndexField { Name = "id", EnableSorting = true, Type = "Number" }, 
                                                                 new TestIndexField { Name = "nodeName", EnableSorting = true },
                                                                 new TestIndexField { Name = "updateDate", EnableSorting = true, Type = "DateTime" }, 
                                                                 new TestIndexField { Name = "writerName" }, 
                                                                 new TestIndexField { Name = "path" }, 
                                                                 new TestIndexField { Name = "nodeTypeAlias" }, 
                                                                 new TestIndexField { Name = "parentID" },
                                                                 new TestIndexField { Name = "sortOrder", EnableSorting = true, Type = "Number"},
                                                             },
                                                         new[]
                                                             {
                                                                 new TestIndexField { Name = "headerText" }, 
                                                                 new TestIndexField { Name = "bodyText" },
                                                                 new TestIndexField { Name = "metaDescription" }, 
                                                                 new TestIndexField { Name = "metaKeywords" }, 
                                                                 new TestIndexField { Name = "bodyTextColOne" }, 
                                                                 new TestIndexField { Name = "bodyTextColTwo" }, 
                                                                 new TestIndexField { Name = "xmlStorageTest" },
                                                                 new TestIndexField { Name = "umbracoNaviHide" }
                                                             },
                                                         new[]
                                                             {
                                                                 "CWS_Home", 
                                                                 "CWS_Textpage",
                                                                 "CWS_TextpageTwoCol", 
                                                                 "CWS_NewsEventsList", 
                                                                 "CWS_NewsItem", 
                                                                 "CWS_Gallery", 
                                                                 "CWS_EventItem", 
                                                                 "Image", 
                                                             },
                                                         new string[] { },
                                                         -1),
														 luceneDir, //custom lucene directory
                                                         new TestDataService(),
                                                         new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
                                                         false);

            //i.IndexSecondsInterval = 1;

            i.IndexingError += IndexingError;

            return i;
        }
		public static UmbracoExamineSearcher GetUmbracoSearcher(Lucene.Net.Store.Directory luceneDir)
        {

			return new UmbracoExamineSearcher(luceneDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
        }
		public static SimpleDataIndexer GetSimpleIndexer(Lucene.Net.Store.Directory luceneDir)
        {
            var i = new SimpleDataIndexer(new IndexCriteria(
                                                         new IIndexField[] { },
                                                         new[]
                                                             {
                                                                 new TestIndexField { Name = "Author" }, 
                                                                 new TestIndexField { Name = "DateCreated", EnableSorting = true, Type = "DateTime"  },
                                                                 new TestIndexField { Name = "Title" }, 
                                                                 new TestIndexField { Name = "Photographer" }, 
                                                                 new TestIndexField { Name = "YearCreated", Type = "Date.Year" }, 
                                                                 new TestIndexField { Name = "MonthCreated", Type = "Date.Month" }, 
                                                                 new TestIndexField { Name = "DayCreated", Type = "Date.Day" },
                                                                 new TestIndexField { Name = "HourCreated", Type = "Date.Hour" },
                                                                 new TestIndexField { Name = "MinuteCreated", Type = "Date.Minute" },
                                                                 new TestIndexField { Name = "SomeNumber", Type = "Number" },
                                                                 new TestIndexField { Name = "SomeFloat", Type = "Float" },
                                                                 new TestIndexField { Name = "SomeDouble", Type = "Double" },
                                                                 new TestIndexField { Name = "SomeLong", Type = "Long" }
                                                             },
                                                         new string[] { },
                                                         new string[] { },
                                                         -1),
														 luceneDir,
                                                         new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
                                                         new TestSimpleDataProvider(),
                                                         new[] { "Documents", "Pictures" }, 
                                                         false);
            i.IndexingError += IndexingError;

            return i;
        }
		public static LuceneSearcher GetLuceneSearcher(Lucene.Net.Store.Directory luceneDir)
        {
			return new LuceneSearcher(luceneDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
        }
		
		public static MultiIndexSearcher GetMultiSearcher(Lucene.Net.Store.Directory pdfDir, Lucene.Net.Store.Directory simpleDir, Lucene.Net.Store.Directory conventionDir, Lucene.Net.Store.Directory cwsDir)
        {
            var i = new MultiIndexSearcher(new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
            return i;
        }

        
        internal static void IndexingError(object sender, IndexingErrorEventArgs e)
        {
            throw new ApplicationException(e.Message, e.InnerException);
        }

     
    }
}
