﻿using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProxySearch.Engine.Proxies;

namespace ProxySearch.Engine.DownloaderContainers
{
    public class HttpDownloader<HttpClientHandlerType> : IHttpDownloader where HttpClientHandlerType : HttpClientHandler, new()
    {
        public async Task<string> GetContentOrNull(string url, Proxy proxy)
        {
            return await GetContentOrNull(url, proxy, null);
        }

        public async Task<string> GetContentOrNull(string url, Proxy proxy, CancellationTokenSource cancellationToken)
        {
            return await GetContentOrNull(url, proxy, cancellationToken, () => { }, length => { }, length => { });
        }

        public async Task<string> GetContentOrNull(string url, Proxy proxy, CancellationTokenSource cancellationToken, Action begin, Action<int> firstTime, Action<int> end)
        {
            cancellationToken = cancellationToken ?? new CancellationTokenSource();

            try
            {
                using (HttpClientHandlerType handler = new HttpClientHandlerType())
                {
                    handler.Proxy = proxy == null ? null : new WebProxy(proxy.Address.ToString(), proxy.Port);

                    begin();

                    using (HttpClient client = new HttpClient(handler))
                    using (HttpResponseMessage response = await client.GetAsync(url, cancellationToken.Token))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        string content = await response.Content.ReadAsStringAsync();

                        firstTime(content.Length);
                        end(content.Length);

                        return content;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
