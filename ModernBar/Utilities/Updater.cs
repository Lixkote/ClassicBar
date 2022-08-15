﻿using ManagedShell.Common.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace ModernBar.Utilities
{
    public class Updater : IDisposable
    {
        public bool IsUpdateAvailable { get; private set; }

        public readonly string DownloadUrl = "https://github.com/dremin/ModernBar/releases";

        private readonly HttpClient httpClient = new HttpClient();

        private readonly string _versionUrl = "https://dremin.github.io/updates/ModernBar.json";
        private readonly int _initialInterval = 10000;
        private readonly int _updateInterval = 86400000;
        
        private Version _currentVersion;
        private System.Timers.Timer updateCheck;

        public Updater()
        {
            _currentVersion = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version;

            updateCheck = new System.Timers.Timer(_initialInterval);
            updateCheck.Elapsed += UpdateCheck_Elapsed;
            updateCheck.AutoReset = true;
            updateCheck.Start();
        }

        private async void UpdateCheck_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // initial interval is shortly after startup
            // don't check again until the desired time
            updateCheck.Interval = _updateInterval;

            IsUpdateAvailable = await CheckForUpdate();

            if (!IsUpdateAvailable)
            {
                return;
            }

            // if an update has been found, stop checking
            updateCheck.Stop();
        }

        public void Dispose()
        {
            updateCheck?.Stop();
        }

        private async Task<bool> CheckForUpdate()
        {
            try
            {
                VersionInfo versionInfo = await httpClient.GetFromJsonAsync<VersionInfo>(_versionUrl);

                if (Version.TryParse(versionInfo.Version, out Version newVersion))
                {
                    if (newVersion > _currentVersion)
                    {
                        return true;
                    }
                }
                else
                {
                    ShellLogger.Info($"Updater: Unable to parse new version");
                }

            }
            catch (Exception e)
            {
                ShellLogger.Info($"Updater: Unable to check for updates: {e.Message}");
            }

            return false;
        }
    }

    public class VersionInfo
    {
        public string Version { get; set; }
    }
}
