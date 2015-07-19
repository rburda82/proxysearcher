using ProxySearch.Console.Properties;

namespace ProxySearch.Console.Code.ProxyClients.Opera
{
    public class HttpOperaClient : OperaClientBase
    {
        public HttpOperaClient()
            : base(Resources.HttpProxyType)
        {
        }
    }
}
