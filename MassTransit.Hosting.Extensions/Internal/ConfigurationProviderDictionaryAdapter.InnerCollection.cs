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

namespace MassTransit.Hosting.Extensions.Internal
{
    public partial class ConfigurationProviderDictionaryAdapter<T>
    {
        /// <summary>
        /// This class is used to delegate the implementation of the dictionary's
        /// <code>Keys</code> and <code>Values</code> properties.
        /// </summary>
        private class InnerCollection : ICollection<string>, ICollection
        {
            private readonly Func<string, bool> _containsFunc;
            private readonly Func<IEnumerator<string>> _getEnumeratorFunc;
            private readonly ConfigurationProviderDictionaryAdapter<T> _parent;

            public InnerCollection(ConfigurationProviderDictionaryAdapter<T> parent, Func<IEnumerator<string>> getEnumeratorFunc, Func<string, bool> containsFunc)
            {
                _parent = parent ?? throw new ArgumentNullException(nameof(parent));
                _getEnumeratorFunc = getEnumeratorFunc ?? throw new ArgumentNullException(nameof(getEnumeratorFunc));
                _containsFunc = containsFunc ?? throw new ArgumentNullException(nameof(containsFunc));
            }

            #region ICollection<string> Members

            public int Count => _parent.Count;
            public bool IsReadOnly => true;

            public IEnumerator<string> GetEnumerator()
            {
                return _getEnumeratorFunc();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public void Add(string item)
            {
                throw new NotSupportedException();
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            public bool Contains(string item)
            {
                return _containsFunc(item);
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            #endregion

            #region ICollection Members

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => _parent.SyncRoot;

            void ICollection.CopyTo(Array array, int index)
            {
                throw new NotSupportedException();
            }

            #endregion
        }
    }
}