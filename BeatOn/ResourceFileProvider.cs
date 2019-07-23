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
using QuestomAssets;

namespace BeatOn
{
    public class ResourceFileProvider : IFileProvider
    {
        private AssetManager _assetManager;
        private string _rootPath;
        public ResourceFileProvider(AssetManager assetManager, string rootPath)
        {
            _assetManager = assetManager;
            _rootPath = rootPath;
        }

        public bool UseCombinedStream => false;

        public string SourceName => "AssetManager";

        public bool DirectoryExists(string path)
        {
            path = _rootPath.CombineFwdSlash(path).Trim('/');
            return _assetManager.List("").Any(x => x.StartsWith(path));
        }
        
        public bool FileExists(string filename)
        {
            filename = _rootPath.CombineFwdSlash(filename).TrimStart('/');
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
                if (FilePatternMatch(_rootPath.CombineFwdSlash(e), pattern))
                    found.Add(e);
            }
            return found;
        }

        public long GetFileSize(string filename)
        {
            filename = _rootPath.CombineFwdSlash(filename).TrimStart('/');
            using (var f = _assetManager.Open(filename))
            {
                return f.Length;
            }
        }

        public Stream GetReadStream(string filename, bool bypassCache = false)
        {
            filename = _rootPath.CombineFwdSlash(filename).TrimStart('/');
            return _assetManager.Open(filename);
        }

        public byte[] Read(string filename)
        {
            foreach (var test in _assetManager.List(_rootPath))
            {
                Log.LogMsg(test);
            }
            filename = _rootPath.CombineFwdSlash(filename).TrimStart('/');
            var fd = _assetManager.OpenFd(filename);
            int len = (int)fd.DeclaredLength;
            using (var f = _assetManager.Open(filename))
            {
                byte[] b = new byte[len];
                f.Read(b, 0, len);
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