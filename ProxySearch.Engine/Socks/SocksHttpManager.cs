﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProxySearch.Engine.Extended;
using ProxySearch.Engine.Properties;
using ProxySearch.Engine.Proxies.Socks;

namespace ProxySearch.Engine.Socks
{
    public class SocksHttpManager
    {
        private static readonly HttpStatusCode[] redirectCodes = new HttpStatusCode[]
        {
            HttpStatusCode.Ambiguous,
            HttpStatusCode.Moved,
            HttpStatusCode.Redirect,
            HttpStatusCode.RedirectMethod,
            HttpStatusCode.RedirectKeepVerb
        };

        public async Task<HttpResponseMessage> GetResponse(SocksHttpManagerParameters parameters)
        {
            return await HandleRedirects(parameters, async uri =>
            {
                switch (parameters.ProxyType)
                {
                    case SocksProxyTypes.CannotVerify:
                    case SocksProxyTypes.ChangesContent:
                    case SocksProxyTypes.Unchecked:
                        try
                        {
                            return await ReadHttpResponseMessage(parameters, uri, async (stream, remoteEP, cancellationToken) =>
                            {
                                await new SocksRequest().V4(stream, remoteEP, cancellationToken);
                                Application.SocksProxyHashTable.Add(GetProxyUri(parameters, uri).ToString(), SocksProxyTypes.Socks4);
                            });
                        }
                        catch (SocksRequestFailedException)
                        {
                        }

                        return await ReadHttpResponseMessage(parameters, uri, async (stream, remoteEP, cancellationToken) =>
                        {
                            await new SocksRequest().V5(stream, remoteEP, cancellationToken);
                            Application.SocksProxyHashTable.Add(GetProxyUri(parameters, uri).ToString(), SocksProxyTypes.Socks5);
                        });
                    case SocksProxyTypes.Socks4:
                        return await ReadHttpResponseMessage(parameters, uri, new SocksRequest().V4);
                    case SocksProxyTypes.Socks5:
                        return await ReadHttpResponseMessage(parameters, uri, new SocksRequest().V5);
                    default:
                        throw new InvalidOperationException(string.Format("Value '{0}' is not expected at SocksHttpManager.GetResponse", parameters.ProxyType));
                }
            });
        }

        private async Task<HttpResponseMessage> ReadHttpResponseMessage(SocksHttpManagerParameters parameters, Uri uri, Func<NetworkStream, Uri, CancellationToken, Task> sendRequest)
        {
            using (TcpClientExtended tcpClient = new TcpClientExtended())
            {
                Uri proxyUri = GetProxyUri(parameters, uri);

                await tcpClient.ConnectAsync(proxyUri.Host, proxyUri.Port, parameters.CancellationToken);

                await sendRequest(tcpClient.GetStream(), uri, parameters.CancellationToken);

                StringBuilder responseBuilder = new StringBuilder();

                byte[] requestBytes = Encoding.UTF8.GetBytes(BuildHttpRequestMessage(parameters.Request));
                await tcpClient.GetStream().WriteAsync(requestBytes, 0, requestBytes.Length, parameters.CancellationToken);
                FireEventProgress(parameters.ReportRequestProgress, requestBytes.Length, requestBytes.Length);

                long? total = null;
                var buffer = new byte[1024];
                var bytesReceived = await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length, parameters.CancellationToken);

                while (bytesReceived > 0)
                {
                    responseBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesReceived));

                    if (parameters.ReportResponseProgress != null && !total.HasValue)
                    {
                        total = GetContentLength(responseBuilder.ToString());
                    }

                    if (total.HasValue)
                    {
                        FireEventProgress(parameters.ReportResponseProgress, responseBuilder.Length, total);
                    }

                    bytesReceived = await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length, parameters.CancellationToken);
                }

                HttpResponseMessage result = BuildHttpResponseMessage(responseBuilder.ToString());

                FireEventProgress(parameters.ReportResponseProgress, responseBuilder.Length, responseBuilder.Length);
                return result;
            }
        }

        private async Task<HttpResponseMessage> HandleRedirects(SocksHttpManagerParameters parameters, Func<Uri, Task<HttpResponseMessage>> getResponse)
        {
            HttpResponseMessage response = await getResponse(parameters.Request.RequestUri);
            int redirectsCount = 0;

            while (parameters.Handler.AllowAutoRedirect && redirectCodes.Contains(response.StatusCode))
            {
                if (redirectsCount > parameters.Handler.MaxAutomaticRedirections)
                {
                    throw new InvalidOperationException(Resources.TooManyRedirectsWasRequestedByServer);
                }

                response = await getResponse(response.Headers.Location);
                redirectsCount++;
            }

            return response;
        }

        private long? GetContentLength(string partialContent)
        {
            try
            {
                HttpResponseMessage response = BuildHttpResponseMessage(partialContent);

                if (!response.Content.Headers.Contains("Content-Length"))
                    return null;

                List<string> values = response.Content.Headers.GetValues("Content-Length").ToList();

                if (values.Count != 1)
                {
                    return null;
                }

                long result = 0;

                if (!long.TryParse(values[0], out result))
                    return null;

                return result;

            }
            catch
            {
                return null;
            }
        }

        private HttpResponseMessage BuildHttpResponseMessage(string response)
        {
            string[] lines = response.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            string content = string.Join(Environment.NewLine, lines.SkipWhile(item => item != "").Skip(1));
            HttpResponseMessage result = new HttpResponseMessage
            {
                StatusCode = GetStatusCode(lines[0]),
                Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(content)))
            };

            foreach (string header in lines.Skip(1).TakeWhile(item => item != ""))
            {
                string[] headerEntry = header.Split(new[] { ':' });
                string value = string.Join(":", headerEntry.Skip(1).ToArray()).Trim();

                try
                {
                    result.Headers.Add(headerEntry[0], value);
                }
                catch (InvalidOperationException)
                {
                    result.Content.Headers.Add(headerEntry[0], value);
                }
            }

            return result;
        }

        private HttpStatusCode GetStatusCode(string firstLine)
        {
            string[] words = firstLine.Split(' ');

            if (words.Length != 3)
            {
                throw new ArgumentException(Resources.InvalidHttpResponseFromSocksProxy);
            }

            return (HttpStatusCode)int.Parse(words[1]);
        }

        private string BuildHttpRequestMessage(HttpRequestMessage request)
        {
            var message = new StringBuilder();

            message.AppendFormat("{0} {1} HTTP/{2}", request.Method, request.RequestUri.PathAndQuery, request.Version).AppendLine();
            message.AppendFormat("Host: {0}", request.RequestUri.Host).AppendLine();

            foreach (var header in request.Headers)
            {
                message.AppendFormat("{0}: {1}", header.Key, header.Value).AppendLine();
            }

            return message.AppendLine().ToString();
        }

        private void FireEventProgress(Action<int, long?> eventHandler, int transfer, long? total)
        {
            if (eventHandler != null)
            {
                eventHandler(transfer, total);
            }
        }

        private Uri GetProxyUri(SocksHttpManagerParameters parameters, Uri uri)
        {
            return parameters.Handler.Proxy.GetProxy(uri);
        }
    }
}
