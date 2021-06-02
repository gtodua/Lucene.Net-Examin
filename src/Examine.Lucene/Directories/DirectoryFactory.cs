using System;
using System.IO;
using System.Threading;
using Examine.Lucene.Providers;
using Directory = Lucene.Net.Store.Directory;

namespace Examine.Lucene.Directories
{
    public class GenericDirectoryFactory : IDirectoryFactory
    {
        private readonly Func<string, Directory> _factory;
        private bool _disposedValue;
        private Directory _directory;

        public GenericDirectoryFactory(Func<string, Directory> factory) => _factory = factory;

        public Directory CreateDirectory(LuceneIndex index)
            => LazyInitializer.EnsureInitialized(ref _directory, () => _factory(index.Name));

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _directory?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
