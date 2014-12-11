﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Lucene.Net.Store;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Used to retrieve/track the same lucene directory instance for a given DirectoryInfo object
    /// </summary>
    [SecuritySafeCritical]
    public sealed class DirectoryTracker
    {
        private static readonly DirectoryTracker Instance = new DirectoryTracker();
       
        private readonly ConcurrentDictionary<string, Lucene.Net.Store.Directory> _directories = new ConcurrentDictionary<string, Lucene.Net.Store.Directory>();
   
        public static DirectoryTracker Current
        {
            get { return Instance; }
        }

        public Lucene.Net.Store.Directory GetDirectory(DirectoryInfo dir)
        {
            var resolved = _directories.GetOrAdd(dir.FullName, s => new SimpleFSDirectory(dir));
            return resolved;
        }
    }
}
