﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Examine.SearchCriteria;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.SearchCriteria
{
    public class LuceneQuery : IQuery
    {
        private readonly LuceneSearchCriteria _search;

        private readonly Occur _occurrence;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneQuery"/> class.
        /// </summary>
        /// <param name="search">The search.</param>
        /// <param name="occurrence">The occurance.</param>
        internal LuceneQuery(LuceneSearchCriteria search, Occur occurrence)
        {
            this._search = search;
            this._occurrence = occurrence;
        }

        /// <summary>
        /// Gets the boolean operation which this query method will be added as
        /// </summary>
        /// <value>The boolean operation.</value>
        public BooleanOperation BooleanOperation => _occurrence.ToBooleanOperation();


        #region ISearch Members

        public IBooleanOperation Field<T>(string fieldName, T fieldValue) where T : struct
        {
            return RangeQuery<T>(new[] { fieldName }, fieldValue, fieldValue);
        }

        public IBooleanOperation Field(string fieldName, string fieldValue)
        {
            return this._search.FieldInternal(fieldName, new ExamineValue(Examineness.Explicit, fieldValue), _occurrence);
        }

        public IBooleanOperation Field(string fieldName, IExamineValue fieldValue)
        {
            return this._search.FieldInternal(fieldName, fieldValue, _occurrence);
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this._search.GroupedAndInternal(fields.ToArray(), fieldVals.ToArray(), this._occurrence);
        }

        public IBooleanOperation GroupedAnd(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this._search.GroupedAndInternal(fields.ToArray(), query, this._occurrence);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this._search.GroupedOrInternal(fields.ToArray(), fieldVals.ToArray(), this._occurrence);
        }

        public IBooleanOperation GroupedOr(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this._search.GroupedOrInternal(fields.ToArray(), query, this._occurrence);
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this._search.GroupedNotInternal(fields.ToArray(), fieldVals.ToArray());
        }

        public IBooleanOperation GroupedNot(IEnumerable<string> fields, params IExamineValue[] query)
        {
            return this._search.GroupedNotInternal(fields.ToArray(), query);
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params string[] query)
        {
            var fieldVals = new List<IExamineValue>();
            foreach (var f in query)
            {
                fieldVals.Add(new ExamineValue(Examineness.Explicit, f));
            }
            return this._search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), fieldVals.ToArray(), _occurrence);
        }

        public IBooleanOperation GroupedFlexible(IEnumerable<string> fields, IEnumerable<BooleanOperation> operations, params IExamineValue[] query)
        {
            return this._search.GroupedFlexibleInternal(fields.ToArray(), operations.ToArray(), query, _occurrence);
        }

        public IBooleanOperation OrderBy(params SortableField[] fields)
        {
            return this._search.OrderBy(fields);
        }

        public IBooleanOperation OrderByDescending(params SortableField[] fields)
        {
            return this._search.OrderByDescending(fields);
        }

        public IBooleanOperation All()
        {
            return _search.All();
        }

        public IBooleanOperation ManagedQuery(string query, string[] fields = null)
        {
            return _search.ManagedQuery(query, fields);
        }

        public IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) where T : struct
        {
            return _search.RangeQuery(fields, min, max, minInclusive: minInclusive, maxInclusive: maxInclusive);
        }

        /// <summary>
        /// Creates an inner group query
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="defaultOp">The default operation is OR, generally a grouped query would have complex inner queries with an OR against another complex group query</param>
        /// <returns></returns>
        public IBooleanOperation Group(Func<IQuery, IBooleanOperation> inner, BooleanOperation defaultOp = BooleanOperation.Or)
        {
            var bo = new LuceneBooleanOperation(_search);
            bo.Op(inner, defaultOp);
            return bo;
        }

        public IBooleanOperation Id(string id)
        {
            return this._search.IdInternal(id, this._occurrence);
        }
        

        #endregion

    }
}
