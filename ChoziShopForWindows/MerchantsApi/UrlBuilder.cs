using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.MerchantsApi
{
    public class UrlBuilder
    {
        public readonly string _baseUrl;
        private readonly List<string> _pathSegments = new();
        private readonly Dictionary<string, string> _queryParams = new();

        private readonly StringBuilder _urlBuilder;
        private bool _hasQuery = false;

        public UrlBuilder(string baseUrl)
        {
            _urlBuilder = new StringBuilder(baseUrl.TrimEnd('/'));
        }

        public UrlBuilder AddPath(string segment)
        {
            _urlBuilder.Append('/')
                .Append(Uri.EscapeDataString(segment));
            return this;
        }

        public UrlBuilder AddPath(int segment)
        {
            AddPath(segment.ToString());
            return this;
        }

        public UrlBuilder AddQuery(string key, string value)
        {
            _urlBuilder.Append(_hasQuery ? '&' : '?');
            _hasQuery = true;

            _urlBuilder.Append(Uri.EscapeDataString(key))
                .Append("=")
                .Append(Uri.EscapeDataString(value));
            return this;
        }

        public UrlBuilder AddQuery(string key, int value)
        {
            return AddQuery(key, value.ToString());
        }

        public UrlBuilder AddQuery(string key, bool value)
        {
            return AddQuery(key, value.ToString().ToLower());
        }

        public UrlBuilder AddQuery(string key, double value)
        {
            return AddQuery(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public UrlBuilder AddQueryArray(string key, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                AddQuery(key, value);
            }
            return this;
        }

        public UrlBuilder AddQueryIf(string key, string value, bool condition)
        {
            if (condition) AddQuery(key, value); return this;
        }

        public string Build()
        {
            return _urlBuilder.ToString();
        }
    }
}
