using System.Collections;

namespace SFTemplateGenerator.Processor.Shares
{
    public class Context : IReadOnlyDictionary<string, object>
    {
        private readonly Dictionary<string, object> _map;

        private Context()
        {
            _map = new Dictionary<string, object>();
        }

        public dynamic this[string key] => _map[key];

        public IEnumerable<string> Keys => _map.Keys;

        public IEnumerable<object> Values => _map.Values;

        public int Count => _map.Count;

        public static Context Create(IReadOnlyDictionary<string, object> map)
        {
            Context context = new Context();

            foreach (var key in map.Keys)
            {
                if (context._map.ContainsKey(key))
                {
                    context._map[key] = map[key];
                }
                else
                {
                    context._map.Add(key, map[key]);
                }
            }

            return context;
        }

        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        public bool TryGetValue(string key, out object value)
        {
            return _map.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
