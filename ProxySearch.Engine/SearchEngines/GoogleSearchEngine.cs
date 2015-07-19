using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProxySearch.Engine.SearchEngines.Google;

namespace ProxySearch.Engine.SearchEngines
{
    public class GoogleSearchEngine : ISearchEngine
    {
        private int linkNumber;
        private int allowedCount;

        private string queryString;
        private GoogleSearchOnPage searchOnPage;
        private ICaptchaWindow captchaWindow;

        public string Status
        {
            get 
            {
                return string.Format(Properties.Resources.DownloadingFormat, string.Format(queryString, linkNumber));
            }
        }

        public GoogleSearchEngine(int allowedCount, string keywords, ICaptchaWindow captchaWindow)
        {
            string[] escapedKeywords = keywords.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(item => Uri.EscapeDataString(item)).ToArray();
            StringBuilder builder = new StringBuilder();

            builder.Append("http://www.google.com.ua/search?q=");
            builder.Append(string.Join("+", escapedKeywords));
            builder.Append("&start={0}");

            queryString = builder.ToString();

            linkNumber = 0;

            this.allowedCount = allowedCount;
            this.captchaWindow = captchaWindow;

            searchOnPage = null;
        }

        public async Task<Uri> GetNext(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                if (allowedCount == 0)
                    return null;

                if (searchOnPage == null)
                {
                    allowedCount--;
                    searchOnPage = new GoogleSearchOnPage();

                    await searchOnPage.Initialize(new Uri(string.Format(queryString, linkNumber)), captchaWindow, linkNumber, cancellationTokenSource);
                }

                Uri res = searchOnPage.GetNext();

                if (res != null)
                    linkNumber++;
                else
                {
                    searchOnPage = null;
                    return await GetNext(cancellationTokenSource);
                }

                return res;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }
    }
}
