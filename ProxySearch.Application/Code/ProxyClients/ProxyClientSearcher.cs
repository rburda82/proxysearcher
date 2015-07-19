﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProxySearch.Console.Code.Interfaces;
using ProxySearch.Console.Code.Settings;
using ProxySearch.Console.Properties;

namespace ProxySearch.Console.Code.ProxyClients
{
    public class ProxyClientSearcher : IProxyClientSearcher
    {
        private Dictionary<string, List<IProxyClient>> allClients;

        public ProxyClientSearcher()
        {
            allClients = Assembly.GetExecutingAssembly().GetTypes()
                                  .Where(type => !type.IsAbstract)
                                  .Where(type => typeof(IProxyClient).IsAssignableFrom(type))
                                  .Select(type => (IProxyClient)Activator.CreateInstance(type))
                                  .Where(instance => instance.IsInstalled)
                                  .GroupBy(proxyClient => proxyClient.Type)
                                  .ToDictionary(group => group.Key, group => group.OrderBy(instance => instance.Order).ToList());
        }

        public List<IProxyClient> SelectedClients
        {
            get
            {
                string proxyType = Context.Get<AllSettings>().SelectedTabSettings.ProxyType;

                if (!allClients.ContainsKey(proxyType))
                {
                    return new List<IProxyClient>();
                }

                return allClients[proxyType];
            }
        }

        public List<IProxyClient> IEClients
        {
            get
            {
                return AllClients.Where(client => client.Name == Resources.InternetExplorer)
                               .ToList();
            }
        }

        public List<IProxyClient> AllClients
        {
            get
            {
                return allClients.SelectMany(pair => pair.Value).ToList();
            }
        }

        public IProxyClient GetInternetExplorerClientOrNull(string type)
        {
            return AllClients.Where(client => client.Type == type)
                             .SingleOrDefault(client => client.Name == Resources.InternetExplorer);
        }
    }
}
