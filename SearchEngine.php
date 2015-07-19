<!DOCTYPE html>
<html>
    <head>
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta http-equiv="Pragma" content="no-cache">
        <meta http-equiv="Expires" content="-1">
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
        <link rel="shortcut icon" href="/Images/favicon.ico">

        <title>Free Proxy Search Engine</title>
        <link href="CSS/style.css" rel="stylesheet" type="text/css">
      
        <script type="text/javascript">
            var _gaq = _gaq || [];
            _gaq.push(['_setAccount', 'UA-39209364-1']);
            _gaq.push(['_trackPageview']);

            (function () {
                var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
                ga.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'stats.g.doubleclick.net/dc.js';
                var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
             })();
         </script>
         <script src="Javascript/jquery-1.9.1.min.js"></script>
         <script type="text/javascript" src="/Libraries/syntaxhighlighter/scripts/shCore.js"></script>
	  <script type="text/javascript" src="/Libraries/syntaxhighlighter/scripts/shBrushJScript.js"></script>
	  <link type="text/css" rel="stylesheet" href="/Libraries/syntaxhighlighter/styles/shCoreDefault.css"/>
	  <script type="text/javascript">SyntaxHighlighter.all();</script>
    </head>
    <body>
       <div class="head-element">
         <div class="head-content">
            <h2 class="looking-for-proxies-easily">Looking for proxies easily
            </h2>
            <h2 class="find-avaliable-proxies-in-one-click">Find available proxies in one click
            </h2>
            <h2 class="hide-your-ip-from-everyone">Hide your IP from everyone
            </h2>
            <h2 class="do-it-for-free">Use Proxy Searcher for free
            </h2>
         </div>
       </div>

       <div class="body-element">
           <a href="/"><img src="Images/world64x64.jpg" class="program-logo" /></a>
           <h1 class="body-header">Free proxy search engine</h1>
            
           <div class="separator"></div>

           <div class="left-panel normal-text">

 	          <h3>Introduction</h3>
           
       	    <div class="normal-text">
			<a href="/">Proxy Searcher</a> uses its own public search engine which based on .NET 4.5 technology. In order to use it you need to perform following steps:
           	    </div>
	           <ul>
			<li>
              	    Find the latest version of .NET library called ProxySearch.Engine.dll. You may download it here <a href="/Proxy Search Engine v4.4.zip">ProxySearch.Engine v4.4</a>.
                  	Also the latest version of ProxySearch.Engine.dll may be found at installation folder of <a href="/">Proxy Searcher</a>. 
	              </li>
			<li>
	                  Use examples below in order to use proxy search engine.
 	             </li>
      		      <li>
	                  If you need more examples please leave your request at <a href="https://sourceforge.net/p/proxysearcher/discussion/?source=navbar">any forum</a>.
 	             </li>
	           </ul>
                  
                  Proxy search engine doesn't support backward compatibility, therefore when you upgrade engine you may notice that your solution is not compilable anymore.
                  In that case please check examples on this page again.
 	     </div>

           <div class="right-panel-advertising engine-advertising">
 		<!-- Begin BidVertiser code -->
		<SCRIPT LANGUAGE="JavaScript1.1" SRC="http://bdv.bidvertiser.com/BidVertiser.dbm?pid=638376&bid=1592441" type="text/javascript"></SCRIPT>
		<noscript><a href="http://www.bidvertiser.com/bdv/BidVertiser/bdv_xml_feed.dbm">search feed</a></noscript>
		<!-- End BidVertiser code --> 
           </div>

           <div class="clear" />

           <h3>Examples</h3>
           <div class="normal-text">
		Following example demonstrates how to search http proxies based on parallel search engine and show them on console application. 
           </div>
    <div class="code-element">
<pre style="margin:0em; overflow:auto; background-color:#ffffff;"><code style="font-family:Consolas,&quot;Courier New&quot;,Courier,Monospace; font-size:10pt; color:#000000;"><span style="color:#0000ff;">using</span> System;
<span style="color:#0000ff;">using</span> System.Net.Http;
<span style="color:#0000ff;">using</span> System.Net.Http.Handlers;
<span style="color:#0000ff;">using</span> ProxySearch.Engine;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.Checkers;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.DownloaderContainers;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.Proxies;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.Proxies.Http;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.ProxyDetailsProvider;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.SearchEngines;

<span style="color:#0000ff;">namespace</span> ProxySearch.CommandLine
{
    <span style="color:#0000ff;">public</span> <span style="color:#0000ff;">class</span> Program
    {
        <span style="color:#0000ff;">static</span> <span style="color:#0000ff;">void</span> Main(<span style="color:#0000ff;">string</span>[] args)
        {
            <span style="color:#0000ff;">var</span> url = <span style="color:#a31515;">"http://proxysearcher.sourceforge.net/ProxyList.php?type=http&filtered=true&limit=1000"</span>;
            ISearchEngine searchEngine = <span style="color:#0000ff;">new</span> ParallelSearchEngine(<span style="color:#0000ff;">new</span> UrlListSearchEngine(url),
                                                                  <span style="color:#0000ff;">new</span> GoogleSearchEngine(40, <span style="color:#a31515;">"http proxy list"</span>, <span style="color:#0000ff;">null</span>));

            IProxyChecker checker =
                <span style="color:#0000ff;">new</span> ProxyCheckerByUrl&lt;HttpProxyDetailsProvider&gt;(<span style="color:#a31515;">"http://wikipedia.org/"</span>, 0.9);
            IHttpDownloaderContainer httpDownloaderContainer =
                <span style="color:#0000ff;">new</span> HttpDownloaderContainer&lt;HttpClientHandler, ProgressMessageHandler&gt;();

            Application application = <span style="color:#0000ff;">new</span> Application(searchEngine, checker, httpDownloaderContainer);

            application.ProxyAlive += application_ProxyAlive;
            application.OnError += exception =&gt; Console.WriteLine(exception.Message);

            application.SearchAsync().GetAwaiter().GetResult();
        }

        <span style="color:#0000ff;">static</span> <span style="color:#0000ff;">void</span> application_ProxyAlive(ProxyInfo proxyInfo)
        {
            <span style="color:#0000ff;">switch</span> ((HttpProxyTypes)Enum.Parse(<span style="color:#0000ff;">typeof</span>(HttpProxyTypes), proxyInfo.Details.Details.Type))
            {
                <span style="color:#0000ff;">case</span> HttpProxyTypes.Anonymous:
                <span style="color:#0000ff;">case</span> HttpProxyTypes.HighAnonymous:
                    Console.WriteLine(proxyInfo.AddressPort);
                    <span style="color:#0000ff;">break</span>;
                <span style="color:#0000ff;">default</span>:
                    <span style="color:#0000ff;">break</span>;
            }
        }
    }
}</code></pre>
    </div>

    <div class="normal-text">
    	Following example demonstrates how to search socks proxies based on parallel search engine and show them on console application. 
    </div>
    <div class="code-element">
<pre style="margin:0em; overflow:auto; background-color:#ffffff;"><code style="font-family:Consolas,&quot;Courier New&quot;,Courier,Monospace; font-size:10pt; color:#000000;"><span style="color:#0000ff;">using</span> System;
<span style="color:#0000ff;">using</span> ProxySearch.Engine;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.Checkers;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.DownloaderContainers;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.Proxies;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.Proxies.Socks;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.ProxyDetailsProvider;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.SearchEngines;
<span style="color:#0000ff;">using</span> ProxySearch.Engine.Socks;

<span style="color:#0000ff;">namespace</span> ProxySearch.CommandLine
{
    <span style="color:#0000ff;">public</span> <span style="color:#0000ff;">class</span> Program
    {
        <span style="color:#0000ff;">static</span> <span style="color:#0000ff;">void</span> Main(<span style="color:#0000ff;">string</span>[] args)
        {
            <span style="color:#0000ff;">var</span> url = <span style="color:#a31515;">"http://proxysearcher.sourceforge.net/ProxyList.php?type=socks&filtered=true&limit=1000"</span>;
            ISearchEngine searchEngine = <span style="color:#0000ff;">new</span> ParallelSearchEngine(<span style="color:#0000ff;">new</span> UrlListSearchEngine(url),
                                                                  <span style="color:#0000ff;">new</span> GoogleSearchEngine(40, <span style="color:#a31515;">"socks proxy list"</span>, <span style="color:#0000ff;">null</span>));

            IProxyChecker checker = <span style="color:#0000ff;">new</span> ProxyCheckerByUrl&lt;SocksProxyDetailsProvider&gt;(<span style="color:#a31515;">"http://www.wikipedia.org/"</span>, 0.9);
            IHttpDownloaderContainer httpDownloaderContainer =
                <span style="color:#0000ff;">new</span> HttpDownloaderContainer&lt;SocksHttpClientHandler, SocksProgressMessageHandler&gt;();

            Application application = <span style="color:#0000ff;">new</span> Application(searchEngine, checker, httpDownloaderContainer);

            application.ProxyAlive += application_ProxyAlive;
            application.OnError += exception =&gt; Console.WriteLine(exception.Message);

            application.SearchAsync().GetAwaiter().GetResult();
        }

        <span style="color:#0000ff;">static</span> <span style="color:#0000ff;">void</span> application_ProxyAlive(ProxyInfo proxyInfo)
        {
            <span style="color:#0000ff;">switch</span> ((SocksProxyTypes)Enum.Parse(<span style="color:#0000ff;">typeof</span>(SocksProxyTypes), proxyInfo.Details.Details.Type))
            {
                <span style="color:#0000ff;">case</span> SocksProxyTypes.Socks4:
                <span style="color:#0000ff;">case</span> SocksProxyTypes.Socks5:
                    Console.WriteLine(proxyInfo.AddressPort);
                    <span style="color:#0000ff;">break</span>;
                <span style="color:#0000ff;">default</span>:
                    <span style="color:#0000ff;">break</span>;
            }
        }
    }
}</code></pre>
    </div>

    <div class="separator"></div>
    </body>
</html>


