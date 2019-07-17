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
        public event PropertyChangedEventHandler PropertyChanged;

        public BeatOnConfig()
        {
        }

        private void PropChanged(string name)
        {
            _isCommitted = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));            
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

        private SyncConfig _syncConfig;
        public SyncConfig SyncConfig
        {
            get
            {
                return _syncConfig;
            }
            set
            {
                _syncConfig = value;
                PropChanged(nameof(SyncConfig));
            }
        }

        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropChanged(nameof(Config));
        }
    }
}