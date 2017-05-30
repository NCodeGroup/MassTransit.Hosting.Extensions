#region Copyright Preamble
// 
//    Copyright © 2017 NCode Group
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MassTransit.Hosting.Extensions.Internal
{
    /// <summary>
    /// Provides an implementation of <see cref="IDictionary{TKey,TValue}"/>
    /// which uses the <typeparamref name="T"/> type for property keys and
    /// <see cref="IConfigurationProvider"/> for the values. This implementation
    /// doesn't cache anything and always defers to the configuration provider.
    /// </summary>
    /// <typeparam name="T">The type to reflect it's properties to use as the keys in the dictionary.</typeparam>
    public partial class ConfigurationProviderDictionaryAdapter<T> : IDictionary<string, string>, IDictionary
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ISet<string> _propertyNamesWithPrefix;
        private InnerCollection _keys;
        private InnerCollection _values;

        public ConfigurationProviderDictionaryAdapter(IConfigurationProvider configurationProvider, string prefix)
        {
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _propertyNamesWithPrefix = LoadPropertyNamesWithPrefix(prefix ?? string.Empty);
        }

        private InnerCollection InnerKeys => _keys ?? (_keys = new InnerCollection(this, GetKeysEnumerator, ContainsKey));

        private InnerCollection InnerValues => _values ?? (_values = new InnerCollection(this, GetValuesEnumerator, ContainsValue));

        public bool IsFixedSize => true;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public int Count => _propertyNamesWithPrefix.Count(ContainsKey);

        public bool IsReadOnly => true;

        private static ISet<string> LoadPropertyNamesWithPrefix(string prefix)
        {
            return new HashSet<string>(typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(prop => prop.CanRead && prop.GetIndexParameters().Length == 0)
                .Select(prop => prefix + prop.Name));
        }

        #region Enumerable Members

        public IEnumerator<TResult> GetEnumerator<TResult>(Func<string, string, TResult> projector)
        {
            foreach (var key in _propertyNamesWithPrefix)
            {
                if (TryGetValue(key, out string value))
                    yield return projector(key, value);
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return GetEnumerator((key, value) => new KeyValuePair<string, string>(key, value));
        }

        public IEnumerator<string> GetKeysEnumerator()
        {
            return GetEnumerator((key, value) => key);
        }

        public IEnumerator<string> GetValuesEnumerator()
        {
            return GetEnumerator((key, value) => value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDictionary<string, string> Members

        public ICollection<string> Keys => InnerKeys;

        public ICollection<string> Values => InnerValues;

        public string this[string key]
        {
            get => TryGetValue(key, out string value) ? value : null;
            set => throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return TryGetValue(item.Key, out string value) && item.Value == value;
        }

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out string _);
        }

        public bool ContainsValue(string value)
        {
            return _propertyNamesWithPrefix.Any(key => TryGetValue(key, out string item) && item == value);
        }

        public bool TryGetValue(string key, out string value)
        {
            if (!_propertyNamesWithPrefix.Contains(key))
            {
                value = null;
                return false;
            }
            return _configurationProvider.TryGetSetting(key, out value);
        }

        #endregion

        #region IDictionary Members

        ICollection IDictionary.Keys => InnerKeys;
        ICollection IDictionary.Values => InnerValues;

        object IDictionary.this[object key]
        {
            get => TryGetValue(key.ToString(), out string value) ? value : null;
            set => throw new NotSupportedException();
        }

        bool IDictionary.Contains(object key)
        {
            return ContainsKey(key.ToString());
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Not Supported Members

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public void Add(object key, object value)
        {
            throw new NotSupportedException();
        }

        public void Add(string key, string value)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public void Remove(object key)
        {
            throw new NotSupportedException();
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}