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

namespace BeatOn
{
    public class DownloadStatusChangeArgs : EventArgs
    {
        
        public DownloadStatusChangeArgs(DownloadStatus status, string message = null)
        {
            Status = status;
            Message = message;
        }

        public DownloadStatusChangeArgs(int percentComplete)
        {
            UpdateType = DownloadStatusUpdateType.PercentCompleteChange;
            Status = DownloadStatus.Downloading;
            PercentComplete = percentComplete;
        }

        public int PercentComplete { get; private set; }

        public DownloadStatusUpdateType UpdateType { get; private set; }

        public DownloadStatus Status { get; private set; }

        public string Message { get; private set; }

        public enum DownloadStatusUpdateType
        {
            StatusChange,
            PercentCompleteChange
        }
    }
}