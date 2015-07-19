﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using ProxySearch.Console.Code.GoogleAnalytics;
using ProxySearch.Console.Code.GoogleAnalytics.Timing;
using ProxySearch.Console.Code.Interfaces;
using ProxySearch.Console.Code.Settings;
using ProxySearch.Console.Properties;
using ProxySearch.Engine.Proxies;
using ProxySearch.Engine.Proxies.Http;
using ProxySearch.Engine.Proxies.Socks;
using ProxySearch.Engine.Tasks;

namespace ProxySearch.Console.Code
{
    public class ProxySearchFeedback
    {
        private StreamWriter stream = null;
        private bool isTimingNotificationSent = false;
        private bool isProxyFound = false;

        private ExportSettings ExportSettings
        {
            get
            {
                return Context.Get<AllSettings>().ExportSettings;
            }
        }

        public ProxySearchFeedback()
        {
            ExportAllowed = true;

            Context.Get<ITaskManager>().OnCompleted += () =>
            {
                if (Context.Get<CancellationTokenSource>().IsCancellationRequested)
                {
                    OnSearchCancelled();
                }
                else
                {
                    OnSearchFinished();
                }
            };
        }

        public bool ExportAllowed
        {
            get;
            set;
        }

        public void OnAliveProxy(ProxyInfo proxyInfo)
        {
            if (proxyInfo.Details != null && proxyInfo.Details.Details != null &&
                Context.Get<AllSettings>().IgnoredHttpProxyTypes.Any(item => item.ToString() == proxyInfo.Details.Details.Type))
                return;

            if (ExportAllowed && ExportSettings.ExportSearchResult)
            {
                lock (this)
                {
                    if (stream == null)
                    {
                        stream = CreateFile(GetDirectory(proxyInfo.Details.Details));
                    }

                    stream.WriteLine(proxyInfo.ToString(ExportSettings.ExportCountry, ExportSettings.ExportProxyType));
                }
            }

            Context.Get<ISearchResult>().Add(proxyInfo);
            isProxyFound = true;

            if (!isTimingNotificationSent)
            {
                isTimingNotificationSent = true;
                Context.Get<IGA>().EndTrackTiming(TimingCategory.SearchSpeed, TimingVariable.TimeForGetFirstProxy, GAResources.FirstProxyFound);
            }
        }

        private void OnSearchFinished()
        {
            Context.Get<IActionInvoker>().Finished(!isProxyFound);
            CloseFile();
            Context.Get<IGA>().TrackEventAsync(EventType.General, Resources.SearchFinished);

            if (!isTimingNotificationSent)
            {
                isTimingNotificationSent = true;
                Context.Get<IGA>()
                       .EndTrackTiming(TimingCategory.SearchSpeed, TimingVariable.TimeForGetFirstProxy, GAResources.SearchFinishedAndNoProxiesWereFound);
            }
        }

        private void OnSearchCancelled()
        {
            Context.Get<IActionInvoker>().Cancelled(!isProxyFound);
            CloseFile();
            Context.Get<IGA>().TrackEventAsync(EventType.General, Resources.SearchCancelled);

            if (!isTimingNotificationSent)
            {
                isTimingNotificationSent = true;
                Context.Get<IGA>()
                       .EndTrackTiming(TimingCategory.SearchSpeed, TimingVariable.TimeForGetFirstProxy, GAResources.SearchCancelledAndNoProxiesWereFound);
            }
        }

        private void CloseFile()
        {
            lock (this)
            {
                if (stream != null && stream.BaseStream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        private string GetDirectory(ProxyTypeDetails details)
        {
            if (details is HttpProxyDetails)
            {
                return Context.Get<AllSettings>().ExportSettings.HttpExportFolder;
            }

            if (details is SocksProxyDetails)
            {
                return Context.Get<AllSettings>().ExportSettings.SocksExportFolder;
            }

            throw new NotSupportedException();
        }

        private StreamWriter CreateFile(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string fileName = string.Format(@"{0}\Search Results {1}.txt", directory, DateTime.Now.ToString("HH.mm.ss dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture));
            return new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                AutoFlush = true
            };
        }
    }
}
