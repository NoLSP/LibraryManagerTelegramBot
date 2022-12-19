using Flurl;
using System;

namespace SpecialLibraryBot.DeviantArtApi
{
    public class PagedRestBase<T> : RestBase where T : class
    {
        private readonly int _maxOffset;
        private readonly int _maxLimit;

        public PagedRestBase(int maxOffset, int maxLimit)
        {
            _maxLimit = maxLimit;
            _maxOffset = maxOffset;
        }

        public T WithMaxLimit()
        {
            _uri = _uri.SetQueryParam("limit", _maxLimit);
            return this as T;
        }

        public T WithPageOffset(int offset)
        {
            if (offset < 0 || offset > _maxOffset)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, $"Offset must be between 0 and {_maxOffset}, inclusive.");
            }

            _uri = _uri.SetQueryParam("offset", offset);
            return this as T;
        }

        public T WithPageLimit(int limit)
        {
            if (limit < 1 || limit > _maxLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), limit, $"Limit must be between 0 and {_maxLimit}, inclusive.");
            }

            _uri = _uri.SetQueryParam("limit", limit);
            return this as T;
        }
    }
}
