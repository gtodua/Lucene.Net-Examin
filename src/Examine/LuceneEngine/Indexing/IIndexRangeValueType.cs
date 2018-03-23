﻿using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public interface IIndexRangeValueType<T> where T : struct
    {
        Query GetQuery(T? lower, T? upper, bool lowerInclusive = true, bool upperInclusive = true);
    }
}