﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Manages the collection of <see cref="IFieldValueTypeFactory"/>
    /// </summary>
    public class ValueTypeFactoryCollection
    {
        public bool TryGetFactory(string valueTypeName, out IFieldValueTypeFactory fieldValueTypeFactory)
        {
            return _valueTypeFactories.TryGetValue(valueTypeName, out fieldValueTypeFactory);
        }

        public bool TryAdd(string valueTypeName, IFieldValueTypeFactory fieldValueTypeFactory)
        {
            return _valueTypeFactories.TryAdd(valueTypeName, fieldValueTypeFactory);
        }

        public bool TryAdd(string valueTypeName, Func<string, IIndexValueType> fieldValueTypeFactory)
        {
            return _valueTypeFactories.TryAdd(valueTypeName, new DelegateFieldValueTypeFactory(fieldValueTypeFactory));
        }

        /// <summary>
        /// Replace any value type factory with the specified one, if one doesn't exist then it is added
        /// </summary>
        /// <param name="valueTypeName"></param>
        /// <param name="fieldValueTypeFactory"></param>
        public void Replace(string valueTypeName, IFieldValueTypeFactory fieldValueTypeFactory)
        {
            _valueTypeFactories.AddOrUpdate(valueTypeName, fieldValueTypeFactory, (s, factory) => fieldValueTypeFactory);
        }

        /// <summary>
        /// Returns the <see cref="IFieldValueTypeFactory"/> by name, if it's not found an exception is thrown
        /// </summary>
        /// <param name="valueTypeName"></param>
        /// <returns></returns>
        public IFieldValueTypeFactory GetRequiredFactory(string valueTypeName)
        {
            if (!TryGetFactory(valueTypeName, out var fieldValueTypeFactory))
                throw new InvalidOperationException($"The required {typeof(IFieldValueTypeFactory).Name} was not found with name {valueTypeName}");

            return fieldValueTypeFactory;
        }

        private readonly ConcurrentDictionary<string, IFieldValueTypeFactory> _valueTypeFactories = new ConcurrentDictionary<string, IFieldValueTypeFactory>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Returns the default index value types that is used in normal construction of an indexer
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, IFieldValueTypeFactory> DefaultValueTypes
            => DefaultValueTypesInternal.ToDictionary(x => x.Key, x => (IFieldValueTypeFactory)new DelegateFieldValueTypeFactory(x.Value));

        //define the defaults
        private static readonly IReadOnlyDictionary<string, Func<string, IIndexValueType>> DefaultValueTypesInternal
            = new Dictionary<string, Func<string, IIndexValueType>>(StringComparer.InvariantCultureIgnoreCase) //case insensitive
            {
                {"number", name => new Int32Type(name)},
                {FieldDefinitionTypes.Integer, name => new Int32Type(name)},
                {FieldDefinitionTypes.Float, name => new SingleType(name)},
                {FieldDefinitionTypes.Double, name => new DoubleType(name)},
                {FieldDefinitionTypes.Long, name => new Int64Type(name)},
                {"date", name => new DateTimeType(name, DateTools.Resolution.MILLISECOND)},
                {FieldDefinitionTypes.DateTime, name => new DateTimeType(name, DateTools.Resolution.MILLISECOND)},
                {FieldDefinitionTypes.DateYear, name => new DateTimeType(name, DateTools.Resolution.YEAR)},
                {FieldDefinitionTypes.DateMonth, name => new DateTimeType(name, DateTools.Resolution.MONTH)},
                {FieldDefinitionTypes.DateDay, name => new DateTimeType(name, DateTools.Resolution.DAY)},
                {FieldDefinitionTypes.DateHour, name => new DateTimeType(name, DateTools.Resolution.HOUR)},
                {FieldDefinitionTypes.DateMinute, name => new DateTimeType(name, DateTools.Resolution.MINUTE)},
                {FieldDefinitionTypes.Raw, name => new RawStringType(name)},
                {FieldDefinitionTypes.FullText, name => new FullTextType(name)},
                {FieldDefinitionTypes.FullTextSortable, name => new FullTextType(name, null, true)},
                {FieldDefinitionTypes.InvariantCultureIgnoreCase, name => new GenericAnalyzerValueType(name, new CultureInvariantWhitespaceAnalyzer())},
                {FieldDefinitionTypes.EmailAddress, name => new GenericAnalyzerValueType(name, new EmailAddressAnalyzer())}
            };
    }
}