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

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// Provides the ability to materialize objects (usually proxies) using the
    /// data from a dictionary for it's properties. This interface is just an
    /// abstraction for the builtin <c>GreenPipes</c> object converter
    /// implementations.
    /// </summary>
    public interface IObjectMapper
    {
        /// <summary>
        /// Used to materialize a proxy object using the data from a dictionary
        /// and a prefix for the keys.
        /// </summary>
        /// <typeparam name="T">The type of object to materialize.</typeparam>
        /// <param name="prefix">A prefix to use when loading keys from the dictionary.</param>
        /// <param name="dictionary">The dictionary containing the data for the object to materialize.</param>
        /// <returns>An object, usually a proxy, materialized from the dictionary data.</returns>
        T MapObject<T>(string prefix, IDictionary dictionary);
    }
}