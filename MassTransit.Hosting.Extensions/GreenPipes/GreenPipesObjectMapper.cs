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
using GreenPipes.Internals.Mapping;

namespace MassTransit.Hosting.Extensions.GreenPipes
{
    /// <summary>
    /// An implementation of <see cref="IObjectMapper"/> that will use the builtin
    /// <c>GreenPipes</c> obect converter cache.
    /// </summary>
    public class GreenPipesObjectMapper : IObjectMapper
    {
        private readonly IObjectConverterCache _objectConverterCache;

        // Dependencies:
        //  IObjectConverterCache : DynamicObjectConverterCache
        //  IImplementationBuilder : DynamicImplementationBuilder

        public GreenPipesObjectMapper(IObjectConverterCache objectConverterCache)
        {
            _objectConverterCache = objectConverterCache ?? throw new ArgumentNullException(nameof(objectConverterCache));
        }

        /// <inheritdoc />
        public virtual T MapObject<T>(string prefix, IDictionary dictionary)
        {
            dictionary = dictionary ?? new Hashtable();
            var provider = new DictionaryObjectValueProvider(dictionary, prefix);

            var converter = _objectConverterCache.GetConverter(typeof(T));
            var obj = (T)converter.GetObject(provider);
            return obj;
        }
    }
}