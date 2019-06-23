using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using QuestomAssets;
using System.Threading;
using System.IO;

namespace BeatOn
{
    public class FileLogger : ILog, IDisposable
    {
        private const int LOG_WRITER_INTERVAL_MS = 5000;
        private const int MAX_LOGFILE_SIZE = 20000000;
        private Timer _saveTimer;
        private string _logFile;
        public FileLogger(string logFile)
        {
            _logFile = logFile;
            CheckLogSize();
            _saveTimer = new Timer((o) =>
            {
                WriteLog();
            }, null, LOG_WRITER_INTERVAL_MS, LOG_WRITER_INTERVAL_MS);
        }
        StringBuilder _builder = new StringBuilder();
        

        private void CheckLogSize()
        {
            if (File.Exists(_logFile))
            {
                if (new FileInfo(_logFile).Length > MAX_LOGFILE_SIZE)
                {
                    try
                    {
                        File.Delete(_logFile);
                    }
                    catch (Exception ex)
                    {
                        Log.LogErr("Log file is too big and tried to delete it, but couldn't.", ex);
                    }
                }
            }
        }

        private void WriteLog()
        {
            if (!Monitor.TryEnter(_saveTimer))
            {
                Log.LogErr("FileLogger: Save timer is ticking while it's already running!");
                return;
            }
            try
            {
                lock (_builder)
                {
                    if (_builder.Length < 1)
                        return;

                    using (StreamWriter sw = new StreamWriter(_logFile, true))
                    {
                        sw.Write(_builder.ToString());
                    }
                    _builder.Clear();
                }
            }
            catch (Exception ex)
            {
                Log.LogErr("FileLogger: Unable to save log!", ex);
            }
            finally
            {
                Monitor.Exit(_saveTimer);
            }
        }

        public void LogErr(string message, Exception ex)
        {
            lock (_builder)
            {
                _builder.Append($"{message} {ex.Message} {ex.StackTrace} {ex?.InnerException?.Message} {ex?.InnerException?.StackTrace}\n");
            }
        }

        public void LogErr(string message, params object[] args)
        {
            lock (_builder)
            {
                _builder.Append($"{String.Format(message, args)}\n");
            }
        }

        public void LogMsg(string message, params object[] args)
        {
            lock (_builder)
            {
                string logStr = message;
                if (args.Length > 0 && !(args[0] is object[] && ((object[])args[0]).Length < 1))
                {
                    try
                    {
                        logStr = string.Format(logStr, args);
                    }
                    catch
                    { }
                }
               _builder.Append($"{logStr}\n");
            }
        }        

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_saveTimer != null)
                    {
                        _saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        _saveTimer.Dispose();
                        _saveTimer = null;
                    }
                    try
                    {
                        WriteLog();
                    }
                    catch
                    { }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FileLogger()
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
    }
}