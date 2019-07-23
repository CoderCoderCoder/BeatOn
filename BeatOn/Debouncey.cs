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
using System.Threading;

namespace BeatOn
{
    public class Debouncey<T>
    {
        private object _timerLock = new object();
        private Timer _timer;
        private object _lastSender;
        private T _lastObj;
        public void EventRaised(object sender, T a)
        {
            lock (_timerLock)
            {
                _lastSender = sender;
                _lastObj = a;
                if (_waitForStop && _timer != null)
                {
                    try
                    {
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer.Dispose();
                    }
                    catch
                    {

                    }
                    _timer = null;
                }
                if (_timer == null)
                {
                    _timer = new Timer((o) =>
                    {
                        lock (_timerLock)
                        {
                            try
                            {
                                Debounced?.Invoke(_lastSender, _lastObj);
                            }
                            finally
                            {
                                try
                                {
                                    if (_timer != null)
                                        _timer.Dispose();
                                }
                                catch { }
                                _timer = null;
                            }
                        }
                    }, null, _debounceMs, Timeout.Infinite);
                }
            }
        }

        public event EventHandler<T> Debounced;
        private int _debounceMs;
        private bool _waitForStop;

        /// <summary>
        /// Creates an instance of Debouncey
        /// </summary>
        /// <param name="debounceMs">The MS to debounce</param>
        /// <param name="waitForStop">If true, will wait for messages to stop before triggering the event.  If false, will always send an event (if triggered) at least once every debounceMs</param>
        public Debouncey(int debounceMs, bool waitForStop)
        {
            _debounceMs = debounceMs;
            _waitForStop = waitForStop;
        }
    }
}