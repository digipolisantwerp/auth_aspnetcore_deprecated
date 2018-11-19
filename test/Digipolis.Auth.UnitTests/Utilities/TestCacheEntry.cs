using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Digipolis.Auth.UnitTests.Utilities
{
    public class TestCacheEntry : ICacheEntry
    {
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        public IList<IChangeToken> ExpirationTokens { get; set; }

        public object Key { get; set; }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }

        public CacheItemPriority Priority { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        public object Value { get; set; }

        public long? Size
        {
            get => 0;
            set { }
        }

        public void Dispose()
        {
        }
    }
}
