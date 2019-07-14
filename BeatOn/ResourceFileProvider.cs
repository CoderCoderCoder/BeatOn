using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets.AssetsChanger;

namespace BeatOn
{
    public class ResourceFileProvider : IFileProvider
    {
        private AssetManager _assetManager;
        public ResourceFileProvider(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public bool UseCombinedStream => false;

        public string SourceName => "AssetManager";

        public bool DirectoryExists(string path)
        {
            path = path.Trim('/');
            return _assetManager.List("").Any(x => x.StartsWith(path));
        }
        
        public bool FileExists(string filename)
        {
            filename = filename.TrimStart('/');
            return _assetManager.List("").Any(x => x == filename);
        }

        private static bool FilePatternMatch(string filename, string pattern)
        {
            Regex mask = new Regex(pattern.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
            return mask.IsMatch(filename);
        }

        public List<string> FindFiles(string pattern)
        {
            var found = new List<string>();
            foreach (var e in _assetManager.List(""))
            {
                if (FilePatternMatch(e, pattern))
                    found.Add(e);
            }
            return found;
        }

        public long GetFileSize(string filename)
        {
            filename = filename.TrimStart('/');
            using (var f = _assetManager.Open(filename))
            {
                return f.Length;
            }
        }

        public Stream GetReadStream(string filename, bool bypassCache = false)
        {
            filename = filename.TrimStart('/');
            return _assetManager.Open(filename);
        }

        public byte[] Read(string filename)
        {
            filename = filename.TrimStart('/');
            using (var f = _assetManager.Open(filename))
            {
                byte[] b = new byte[f.Length];
                f.Read(b, 0, (int)f.Length);
                return b;
            }
        }

        #region writes and changes not implemented for this provider
        public Stream GetWriteStream(string filename)
        {
            throw new NotImplementedException();
        }

        public void MkDir(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public void RmRfDir(string path)
        {
            throw new NotImplementedException();
        }

        public void Save(string toFile = null)
        {
            throw new NotImplementedException();
        }

        public void Write(string filename, byte[] data, bool overwrite = true, bool compressData = true)
        {
            throw new NotImplementedException();
        }

        public void WriteFile(string sourceFilename, string targetFilename, bool overwrite = true, bool compressData = true)
        {
            throw new NotImplementedException();
        }

        public void Delete(string filename)
        {
            throw new NotImplementedException();
        }

        public void DeleteFiles(string pattern)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ResourceFileProvider()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        #endregion
    }
}