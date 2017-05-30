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

using System.Collections;
using Castle.Components.DictionaryAdapter;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class CastleObjectMapper : IObjectMapper
    {
        private readonly IDictionaryAdapterFactory _dictionaryAdapterFactory;

        // Dependencies:
        //  IDictionaryAdapterFactory : DictionaryAdapterFactory

        public CastleObjectMapper(IDictionaryAdapterFactory dictionaryAdapterFactory = null)
        {
            _dictionaryAdapterFactory = dictionaryAdapterFactory ?? new DictionaryAdapterFactory();
        }

        public virtual T MapObject<T>(string prefix, IDictionary dictionary)
        {
            PropertyDescriptor descriptor = null;
            if (!string.IsNullOrEmpty(prefix))
                descriptor = new PropertyDescriptor().AddBehaviors(new KeyPrefixAttribute(prefix));

            dictionary = dictionary ?? new Hashtable();

            var obj = (T) _dictionaryAdapterFactory.GetAdapter(typeof(T), dictionary, descriptor);
            return obj;
        }
    }
}