﻿using System;
using System.IO;
using Examine.LuceneEngine.Providers;
using Examine.Test.DataServices;
using Lucene.Net.Analysis.Standard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using UmbracoExamine;

namespace Examine.Test.Search
{
    [TestClass]
    public class DataTypeTests
    {
        /// <summary>
        /// Test range query with a DateTime structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Date_Range_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("DateCreated", _reIndexDateTime, DateTime.Now, true, true).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("DateCreated", _reIndexDateTime.AddDays(-1), _reIndexDateTime.AddSeconds(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Year structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Date_Range_Year_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();

            //Shouldn't there be results for searching the current year? 2010 -> 2010 ?
            var filter = criteria.Range("YearCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Year).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("YearCreated", DateTime.Now.AddYears(-2), DateTime.Now.AddYears(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Month structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Date_Range_Month_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("MonthCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Month).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("MonthCreated", _reIndexDateTime.AddMonths(-2), _reIndexDateTime.AddMonths(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Day structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Date_Range_Day_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("DayCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Day).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("DayCreated", _reIndexDateTime.AddDays(-2), _reIndexDateTime.AddDays(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Hour structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Date_Range_Hour_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("HourCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Hour).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("HourCreated", _reIndexDateTime.AddHours(-2), _reIndexDateTime.AddHours(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Date.Minute structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Date_Range_Minute_SimpleIndexSet()
        {
            ////Arrange

            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("MinuteCreated", _reIndexDateTime, DateTime.Now, true, true, SearchCriteria.DateResolution.Minute).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("MinuteCreated", _reIndexDateTime.AddMinutes(-20), _reIndexDateTime.AddMinutes(-1), true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }


        /// <summary>
        /// test range query with a Number structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Number_Range_SimpleIndexSet()
        {

            ////Arrange
            //all numbers should be between 0 and 100 based on the data source
            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeNumber", 0, 100, true, true).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeNumber", 101, 200, true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Float structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Float_Range_SimpleIndexSet()
        {
            ////Arrange            

            //all numbers should be between 0 and 100 based on the data source
            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeFloat", 0f, 100f, true, true).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeFloat", 101f, 200f, true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Double structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Double_Range_SimpleIndexSet()
        {
            ////Arrange            

            //all numbers should be between 0 and 100 based on the data source
            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeDouble", 0d, 100d, true, true).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeDouble", 101d, 200d, true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        /// <summary>
        /// test range query with a Double structure
        /// </summary>
        [TestMethod]
        public void DataTypesTests_Long_Range_SimpleIndexSet()
        {
            ////Arrange            

            //all numbers should be between 0 and 100 based on the data source
            var criteria = _searcher.CreateSearchCriteria();
            var filter = criteria.Range("SomeLong", 0L, 100L, true, true).Compile();

            var criteriaNotFound = _searcher.CreateSearchCriteria();
            var filterNotFound = criteriaNotFound.Range("SomeLong", 101L, 200L, true, true).Compile();

            ////Act
            var results = _searcher.Search(filter);
            var resultsNotFound = _searcher.Search(filterNotFound);

            ////Assert
            Assert.IsTrue(results.TotalItemCount > 0);
            Assert.IsTrue(resultsNotFound.TotalItemCount == 0);
        }

        private static ISearcher _searcher;
        private static IIndexer _indexer;
        private static DateTime _reIndexDateTime;

        [TestInitialize()]
        public void Initialize()
        {

            var newIndexFolder = new DirectoryInfo(Path.Combine("App_Data\\SimpleIndexSet", Guid.NewGuid().ToString()));
            _indexer = IndexInitializer.GetSimpleIndexer(newIndexFolder);
            
            _reIndexDateTime = DateTime.Now;
            Thread.Sleep(1000);

            _indexer.RebuildIndex();

            _searcher = IndexInitializer.GetLuceneSearcher(newIndexFolder);
        }
    }
}
