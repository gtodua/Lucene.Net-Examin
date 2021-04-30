﻿using System;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Examine.LuceneEngine.Search
{

    /// <summary>
    /// We use this to get at the protected methods directly since the new version makes them not public
    /// </summary>
    public class CustomMultiFieldQueryParser : MultiFieldQueryParser
    {

        public CustomMultiFieldQueryParser(Lucene.Net.Util.LuceneVersion matchVersion, string[] fields, Analyzer analyzer) : base(matchVersion, fields, analyzer)
        {
        }

        public virtual Query GetFuzzyQueryInternal(string field, string termStr, float minSimilarity)
        {
            if (string.IsNullOrWhiteSpace(termStr)) throw new System.ArgumentException($"'{nameof(termStr)}' cannot be null or whitespace", nameof(termStr));

            return GetFuzzyQuery(field, termStr, minSimilarity);
        }
        /// <summary>
        /// Override to provide support for numerical range query parsing
        /// </summary>
        /// <param name="field"></param>
        /// <param name="part1"></param>
        /// <param name="part2"></param>
        /// <param name="startInclusive"></param>
        /// <returns></returns>
        /// <remarks>
        /// By Default the lucene query parser only deals with strings and the result is a TermRangeQuery, however for numerics it needs to be a
        /// NumericRangeQuery. We can override this method to provide that behavior.
        /// 
        /// In previous releases people were complaining that this wouldn't work and this is why. The answer came from here https://stackoverflow.com/questions/5026185/how-do-i-make-the-queryparser-in-lucene-handle-numeric-ranges
        /// 
        /// TODO: We could go further and override the field query and check if it is a numeric field, if so then we can automatically generate a numeric range query for the single digit too.
        /// </remarks>
        protected override Query GetRangeQuery(string field, string part1, string part2, bool startInclusive,bool endInclusive)
        {
            return base.GetRangeQuery(field, part1, part2, startInclusive,endInclusive);
        }

        public virtual Query GetWildcardQueryInternal(string field, string termStr)
        {
            if (string.IsNullOrWhiteSpace(termStr)) throw new System.ArgumentException($"'{nameof(termStr)}' cannot be null or whitespace", nameof(termStr));
            
            return GetWildcardQuery(field, termStr);
        }
     
        public virtual Query GetFieldQueryInternal(string field, string queryText, int slop)
        {
            if (string.IsNullOrWhiteSpace(queryText)) throw new System.ArgumentException($"'{nameof(queryText)}' cannot be null or whitespace", nameof(queryText));

            return GetFieldQuery(field, queryText,slop);
        }
        
        public Query GetFieldQueryInternal(string field, string queryText,
            bool quoted)
        {
            if (string.IsNullOrWhiteSpace(queryText)) throw new System.ArgumentException($"'{nameof(queryText)}' cannot be null or whitespace", nameof(queryText));

            return GetFieldQuery(field, queryText,true);
        }
    }
}
