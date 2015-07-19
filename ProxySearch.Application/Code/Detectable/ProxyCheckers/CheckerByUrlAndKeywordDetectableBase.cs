﻿using System.Collections.Generic;
using ProxySearch.Console.Controls;
using ProxySearch.Console.Properties;
using ProxySearch.Engine.Checkers;
using ProxySearch.Engine.ProxyDetailsProvider;

namespace ProxySearch.Console.Code.Detectable.ProxyCheckers
{
    public abstract class CheckerByUrlAndKeywordDetectableBase<ProxyDetailsProviderType> :
                 DetectableBase<IProxyChecker, ProxyCheckerByUrlAndKeywords<ProxyDetailsProviderType>, CheckerByUrlAndKeywordsControl>
                 where ProxyDetailsProviderType : IProxyDetailsProvider, new()
    {
        public CheckerByUrlAndKeywordDetectableBase(string proxyType, string url, string keywords)
            : base(Resources.ProxyCheckerByUrlAndKeywords, Resources.ProxyCheckerByUrlAndKeywordsDescription, 1, proxyType, new List<object>
                {
                    url,
                    keywords,
                    Constants.DefaultSettings.MaxTasksCount.Value
                })
        {
        }
    }
}