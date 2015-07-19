﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ProxySearch.Engine.Checkers;
using ProxySearch.Engine.DownloaderContainers;
using ProxySearch.Engine.Error;
using ProxySearch.Engine.GeoIP;
using ProxySearch.Engine.Properties;
using ProxySearch.Engine.Proxies;
using ProxySearch.Engine.Proxies.Http;
using ProxySearch.Engine.Tasks;

namespace ProxySearch.Engine.ProxyDetailsProvider
{
    public class HttpProxyDetailsProvider : ProxyDetailsProviderBase, IAsyncInitialization
    {
        private Task initializatinTask = null;
        private IPAddress myIPAddress = null;
        private IHttpDownloaderContainer httpDownloaderContainer;

        public void InitializeAsync(CancellationTokenSource cancellationTokenSource,
                                    ITaskManager taskManager,
                                    IHttpDownloaderContainer httpDownloaderContainer,
                                    IErrorFeedback errorFeedback,
                                    IProxySearchFeedback proxySearchFeedback,
                                    IGeoIP geoIP)
        {
            this.httpDownloaderContainer = httpDownloaderContainer;
            TaskItem taskItem = taskManager.Create(Resources.ConfigureProviderOfProxyDetails);

            taskItem.UpdateDetails(Resources.DetermineCurrentIPAddress);

            initializatinTask = httpDownloaderContainer.HttpDownloader
                                                       .GetContentOrNull(MyIPUrl, null, cancellationTokenSource)
                                                       .ContinueWith(task =>
                                                       {
                                                           try
                                                           {
                                                               if (task.Result != null)
                                                               {
                                                                   IPAddress.TryParse(task.Result.Trim(), out myIPAddress);
                                                               }
                                                           }
                                                           finally
                                                           {
                                                               taskItem.Dispose();
                                                           }
                                                       });
        }

        public override async Task<ProxyTypeDetails> GetProxyDetails(Proxy proxy, TaskItem task, CancellationTokenSource cancellationToken)
        {
            task.UpdateDetails(string.Format(Resources.WaitUntilProxyDetailsProviderConfiguredFormat, proxy.AddressPort), Tasks.TaskStatus.Slow);
            await initializatinTask;

            task.UpdateDetails(string.Format(Resources.ProxyDeterminingProxyType, proxy.AddressPort), Tasks.TaskStatus.Normal);

            string result = await httpDownloaderContainer.HttpDownloader.GetContentOrNull(GetProxyTypeDetectorUrl(proxy, myIPAddress, Resources.HttpProxyType),
                                                                                                          proxy,
                                                                                                          cancellationToken);

            if (result == null)
                return new HttpProxyDetails(HttpProxyTypes.CannotVerify, null);

            string[] values = result.Split(',');

            HttpProxyTypes proxyType;
            IPAddress outgoingIPAddress;

            if (values.Length != 2 || !Enum.TryParse(values[0], out proxyType) || !IPAddress.TryParse(values[1], out outgoingIPAddress))
                return new HttpProxyDetails(HttpProxyTypes.ChangesContent, null);

            return new HttpProxyDetails(proxyType, outgoingIPAddress);
        }

        public override ProxyTypeDetails GetUncheckedProxyDetails()
        {
            return new HttpProxyDetails(HttpProxyTypes.Unchecked, null);
        }
    }
}
