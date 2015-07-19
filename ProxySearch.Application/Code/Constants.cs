﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySearch.Console.Code
{
    public static class Constants
    {
        public static class Working
        {
            static Working()
            {
                if (!System.IO.Directory.Exists(Directory))
                {
                    System.IO.Directory.CreateDirectory(Directory);
                }
            }

            public static readonly string Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ProxySearcher\\";
        }

        public static class SettingsStorage
        {
            public static readonly string Location = Working.Directory + "SettingsStorage.xml";
        }

        public static class UsedProxiesStorage
        {
            public static readonly string Location = Working.Directory + "UsedProxiesStorage.xml";
        }

        public static class BlackListStorage
        {
            public static readonly string Location = Working.Directory + "BlackList.xml";
        }

        public static class ProxySettingsStorage
        {
            public static readonly string Location = Working.Directory + "ProxySettingsStorage.xml";            
        }

        public static class DefaultExportFolder
        {
            private static readonly string Location = Working.Directory + "SearchResult";

            public static class Http
            {
                public static readonly string Location = DefaultExportFolder.Location + @"\Http";
            }

            public static class Socks
            {
                public static readonly string Location = DefaultExportFolder.Location + @"\Socks";
            }
        }

        public static class Browsers
        {
            public static readonly string BrowserPath64Bit = @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet\{0}\shell\open\command";
            public static readonly string BrowserPath32Bit = @"SOFTWARE\Clients\StartMenuInternet\{0}\shell\open\command";

            public static class Opera
            {
                public static readonly string Location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Opera\Opera\operaprefs.ini";
                public static readonly string Section = "Proxy";
            }
        }

        public static class RegistrySettings
        {
            public static readonly string Location = @"Software\Proxy Searcher";
            public static readonly string ClientId = @"ClientId";
        }

        public static class DefaultSettings
        {
            public static class MaxTasksCount
            {
                public static readonly int Value = 100;
            }
        }
    }
}
