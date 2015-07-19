using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ProxySearch.Engine.Checkers;
using ProxySearch.Engine.DownloaderContainers;
using ProxySearch.Engine.Error;
using ProxySearch.Engine.Extension;
using ProxySearch.Engine.GeoIP;
using ProxySearch.Engine.Parser;
using ProxySearch.Engine.Properties;
using ProxySearch.Engine.Proxies;
using ProxySearch.Engine.SearchEngines;
using ProxySearch.Engine.Socks;
using ProxySearch.Engine.Tasks;

namespace ProxySearch.Engine
{
    public class Application : IProxySearchFeedback, IErrorFeedback
    {
        ISearchEngine searchEngine;
        IProxyChecker checker;
        IHttpDownloaderContainer httpDownloaderContainer;
        IGeoIP geoIP;
        IProxyProvider proxyProvider;
        ITaskManager taskManager;

        internal static ISocksProxyTypeHashtable SocksProxyHashTable
        {
            get;
            private set;
        }

        public ObservableList<TaskData> Tasks
        {
            get
            {
                return taskManager.Tasks;
            }
        }

        static Application()
        {
            SocksProxyHashTable = new SocksProxyTypeHashtable();
        }

        public Application(ISearchEngine searchEngine,
                           IProxyChecker checker,
                           IHttpDownloaderContainer httpDownloaderContainer,
                           IGeoIP geoIP = null,
                           IProxyProvider proxyProvider = null,
                           ITaskManager taskManager = null)
        {
            this.searchEngine = searchEngine;
            this.checker = checker;
            this.httpDownloaderContainer = httpDownloaderContainer;

            this.proxyProvider = proxyProvider ?? new ProxyProvider();
            this.geoIP = geoIP ?? new TurnOffGeoIP();
            this.taskManager = taskManager ?? new TaskManager();
        }

        public void OnAliveProxy(ProxyInfo proxyInfo)
        {
            if (ProxyAlive != null)
                ProxyAlive(proxyInfo);
        }

        public void SetException(Exception exception)
        {
            if (OnError != null)
                OnError(exception);
        }

        public async Task SearchAsync()
        {
            await SearchAsync(new CancellationTokenSource());
        }

        public event Action<ProxyInfo> ProxyAlive;
        public event Action<Exception> OnError;

        public async Task SearchAsync(CancellationTokenSource cancellationTokenSource)
        {
            ManualResetEvent waitEvent = new ManualResetEvent(false);

            object[] objects = new object[] { checker, proxyProvider };

            foreach (object @object in objects)
            {
                IAsyncInitialization asyncInitialization = @object as IAsyncInitialization;

                if (asyncInitialization != null)
                    asyncInitialization.InitializeAsync(cancellationTokenSource, taskManager, httpDownloaderContainer, this, this, geoIP);
            }

            IEnumerable<ISearchEngine> searchEngines = searchEngine as IEnumerable<ISearchEngine>;

            if (searchEngines == null)
            {
                await SearchAsyncInternal(searchEngine, cancellationTokenSource);
            }
            else
            {
                List<Task> tasks = new List<Task>();

                foreach (ISearchEngine engine in searchEngines)
                {
                    tasks.Add(SearchAsyncInternal(engine, cancellationTokenSource));
                }

                await Task.WhenAll(tasks);
            }

            taskManager.OnCompleted += () =>
            {
                waitEvent.Set();
            };

            await waitEvent.AsTask();
        }

        private async Task SearchAsyncInternal(ISearchEngine searchEngine, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                using (TaskItem task = taskManager.Create(Resources.ProxySearching))
                {
                    while (true)
                    {
                        task.UpdateDetails(searchEngine.Status);

                        Uri uri = await searchEngine.GetNext(cancellationTokenSource);

                        if (uri == null || cancellationTokenSource.IsCancellationRequested)
                            return;

                        task.UpdateDetails(string.Format(Resources.DownloadingFormat, uri.ToString()));

                        string document = await GetDocumentAsyncOrNull(uri, cancellationTokenSource);

                        if (cancellationTokenSource.IsCancellationRequested)
                            return;

                        if (document == null)
                            continue;

                        List<Proxy> proxies = await proxyProvider.ParseProxiesAsync(uri, document);

                        if (proxies.Any())
                            checker.CheckAsync(proxies);
                    }
                }
            }
            catch (Exception e)
            {
                SetException(e);
            }
        }

        private async Task<string> GetDocumentAsyncOrNull(Uri uri, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                if (uri.IsFile)
                {
                    return await Task.FromResult<string>(File.ReadAllText(uri.LocalPath));
                }

                return await GetInternetDocumentAsyncOrNull(uri, cancellationTokenSource);
            }
            catch
            {
                return null;
            }
        }

        private static async Task<string> GetInternetDocumentAsyncOrNull(Uri uri, CancellationTokenSource cancellationTokenSource)
        {
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(uri, cancellationTokenSource.Token))
            {
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                return null;
            }
        }
    }
}
