﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;


namespace Kartverket.MetadataEditor.Models
{
    public class MemoryCacher
    {
        static readonly object addLock = new object();

        public object GetValue(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            return memoryCache.Get(key);
        }

        public void Set(string key, object value, DateTimeOffset absExpiration)
        {
            lock (addLock)
            {
                CacheItemPolicy policy =
                new CacheItemPolicy { AbsoluteExpiration = absExpiration, Priority = CacheItemPriority.NotRemovable };
                MemoryCache memoryCache = MemoryCache.Default;
                memoryCache.Set(key, value, policy);
            }
        }

        public void Delete(string key)
        {
            MemoryCache memoryCache = MemoryCache.Default;
            if (memoryCache.Contains(key))
            {
                memoryCache.Remove(key);
            }
        }

        public void DeleteAll()
        {
            List<string> cacheKeys = MemoryCache.Default.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                MemoryCache.Default.Remove(cacheKey);
            }
        }

    }
}