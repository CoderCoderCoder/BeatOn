using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuestomAssets.Models;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;

namespace BeatOn.ClientModels
{
    public class BeatOnConfig : INotifyPropertyChanged
    {
        public event EventHandler ConfigChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public BeatOnConfig()
        {
        }

        object _debounceLock = new object();
        CancellationTokenSource _tokenSource;
        private void PropChanged(string name)
        {
            _isCommitted = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));            
            lock (_debounceLock)
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel(true);
                    _tokenSource = null;
                }
                _tokenSource = new CancellationTokenSource();
                var task = Task.Delay(10, _tokenSource.Token);
                task.ContinueWith((t) =>
                {
                    try
                    {
                        t.Wait();
                        lock (_debounceLock)
                        {
                            ConfigChanged?.Invoke(this, new EventArgs());
                            _tokenSource = null;
                        }
                    }
                    catch (System.OperationCanceledException)
                    {
                    }
                });
               // task.Start();
            }
        }

        private bool _isCommitted;
        public bool IsCommitted
        {
            get => _isCommitted;
            set
            {
                if (_isCommitted != value)
                    PropChanged(nameof(IsCommitted));
                _isCommitted = value;
            }
        }

        private BeatSaberQuestomConfig _config;

        public BeatSaberQuestomConfig Config {
            get => _config;
            set
            {
                //I'm not going to compare this, maybe i'm just lazy, maybe i'll have to write a comparer soon anyways
                if (_config != value)
                {
                    PropChanged(nameof(Config));
                    if (_config != null)
                        _config.PropertyChanged -= Config_PropertyChanged;

                    if (value != null)
                        value.PropertyChanged += Config_PropertyChanged;
                }
                _config = value;
            }
        }

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropChanged(nameof(Config));
        }
    }
}