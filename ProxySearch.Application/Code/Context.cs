﻿using System;
using System.Collections.Generic;
using ProxySearch.Console.Properties;

namespace ProxySearch.Console.Code
{
    public static class Context
    {
        private static Dictionary<Type, object> Objects
        {
            get;
            set;
        }

        static Context()
        {
            Objects = new Dictionary<Type, object>();
        }

        public static void Set<T>(T obj)
        {
            if (Objects.ContainsKey(typeof(T)))
            {
                Objects[typeof(T)] = obj;
            }
            else
            {
                Objects.Add(typeof(T), obj);
            }
        }

        public static T Get<T>()
        {
            if (!Objects.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException(string.Format(Resources.ObjectOfTypeIsNotSetYet, typeof(T).FullName));
            }

            return (T)Objects[typeof(T)];
        }

        public static bool IsSet<T>()
        {
            return Objects.ContainsKey(typeof(T));
        }
    }
}
