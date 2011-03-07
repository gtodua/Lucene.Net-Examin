﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Examine.LuceneEngine
{
    public static class SerializableDictionaryExtensions
    {

        ///<summary>
        /// The encoding to use when saving the file
        ///</summary>
        internal static Encoding DefaultFileEncoding = Encoding.UTF8;

        public static SerializableDictionary<TKey, TValue> ToSerializableDictionary<TKey, TValue>(this Dictionary<TKey, TValue> d)
        {
            var sd = new SerializableDictionary<TKey, TValue>();
            d.ToList().ForEach(x =>
            {
                sd.Add(x.Key, x.Value);
            });
            return sd;
        }

        /// <summary>
        /// Returns a Dictionary from a type with a specified value to a dictionary that has a different type for it's value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TConvertVal"></typeparam>
        /// <param name="sd"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TConvertVal> ToDictionary<TKey, TValue, TConvertVal>(this SerializableDictionary<TKey, TValue> sd, Converter<TValue, TConvertVal> c)
        {
            var d = new Dictionary<TKey, TConvertVal>();
            sd.ToList().ForEach(x =>
            {
                d.Add(x.Key, c.Invoke(x.Value));
            });
            return d;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this SerializableDictionary<TKey, TValue> sd)
        {
            var d = new Dictionary<TKey, TValue>();
            sd.ToList().ForEach(x =>
            {
                d.Add(x.Key, x.Value);
            });
            return d;
        }

        private static void SaveToDisk<TKey, TValue>(this SerializableDictionary<TKey, TValue> sd, FileInfo fi)
        {
            var xs = new XmlSerializer(sd.GetType());
            var output = "";
            using (var sw = new StringWriter())
            {
                xs.Serialize(sw, sd);
                output = sw.ToString();
            }

            //write file using UTF8
            using (var fs = new FileStream(fi.FullName, FileMode.Create))
            {
                using (var w = new StreamWriter(fs, DefaultFileEncoding))
                {
                    w.Write(output);                    
                }
            }            
        }

        public static void SaveToDisk<TKey, TValue>(this Dictionary<TKey, TValue> d, FileInfo fi)
        {
            d.ToSerializableDictionary().SaveToDisk(fi);
        }

        public static void ReadFromDisk<TKey, TValue>(this SerializableDictionary<TKey, TValue> sd, FileInfo fi)
        {
            //get the dictionary object from the file data
            var xs = new XmlSerializer(typeof(SerializableDictionary<TKey, TValue>));
            var deserialized = new SerializableDictionary<TKey, TValue>();

            using (var fs = new FileStream(fi.FullName, FileMode.Open))
            {
                using (var r = new StreamReader(fs, DefaultFileEncoding))
                {
                    deserialized = xs.Deserialize(r) as SerializableDictionary<TKey, TValue>;
                }
            }
           
            sd.Clear();
            foreach (var x in deserialized)
            {
                sd.Add(x.Key, x.Value);
            }
        }
    }
}
