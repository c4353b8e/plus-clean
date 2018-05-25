namespace Plus.Game.Cache.Process
{
    using System;
    using System.Linq;
    using System.Threading;
    using Core.Logging;
    using Users;
    using Users.Authenticator;

    internal sealed class ProcessComponent
    {
        private static readonly ILogger Logger = new Logger<ProcessComponent>();

        private Timer _timer;

        private bool _timerRunning;
        
        private bool _timerLagging;

        private bool _disabled;

        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);

        private static readonly int _runtimeInSec = 1200;

        public void Init()
        {
            _timer = new Timer(Run, null, _runtimeInSec * 1000, _runtimeInSec * 1000);
        }

        public void Run(object state)
        {
            try
            {
                if (_disabled)
                {
                    return;
                }

                if (_timerRunning)
                {
                    _timerLagging = true;
                    return;
                }

                _resetEvent.Reset();

                // BEGIN CODE
                var cacheList = Program.GameContext.GetCacheManager().GetUserCache().ToList();
                if (cacheList.Count > 0)
                {
                    foreach (var cache in cacheList)
                    {
                        try
                        {
                            if (cache == null)
                            {
                                continue;
                            }

                            if (cache.IsExpired())
                            {
                                Program.GameContext.GetCacheManager().TryRemoveUser(cache.Id, out _);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    }
                }

                var cachedUsers = HabboFactory.GetUsersCached().ToList();
                if (cachedUsers.Count > 0)
                {
                    foreach (var data in cachedUsers)
                    {
                        try
                        {
                            if (data == null)
                            {
                                continue;
                            }

                            Habbo temp = null;

                            if (data.CacheExpired())
                            {
                                HabboFactory.RemoveFromCache(data.Id, out temp);
                            }

                            if (temp != null)
                            {
                                temp.Dispose();
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    }
                }
                // END CODE

                // Reset the values
                _timerRunning = false;
                _timerLagging = false;

                _resetEvent.Set();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Dispose()
        {
            // Wait until any processing is complete first.
            try
            {
                _resetEvent.WaitOne(TimeSpan.FromMinutes(5));
            }
            catch { } // give up

            // Set the timer to disabled
            _disabled = true;

            // Dispose the timer to disable it.
            try
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }
            }
            catch { }

            // Remove reference to the timer.
            _timer = null;
        }
    }
}