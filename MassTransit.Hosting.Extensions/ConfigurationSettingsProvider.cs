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
using MassTransit.Hosting.Extensions.Internal;

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// Provides an implementation of <see cref="ISettingsProvider{T}"/> that
    /// will retrieve it's settings from <see cref="IConfigurationProvider"/>.
    /// This class should be registered in the dependency container as an open
    /// generic.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ISettings"/> to retrieve.</typeparam>
    public class ConfigurationSettingsProvider<T> : ISettingsProvider<T>
        where T : ISettings
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IObjectMapper _objectMapper;

        public ConfigurationSettingsProvider(IConfigurationProvider configurationProvider, IObjectMapper objectMapper)
        {
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _objectMapper = objectMapper ?? throw new ArgumentNullException(nameof(objectMapper));
        }

        /// <inheritdoc />
        public virtual bool TryGetSettings(out T settings)
        {
            return TryGetSettings(null, out settings);
        }

        /// <inheritdoc />
        public virtual bool TryGetSettings(string prefix, out T settings)
        {
            var dictionary = new ConfigurationProviderDictionaryAdapter<T>(_configurationProvider, prefix);
            if (dictionary.Count == 0)
            {
                settings = default(T);
                return false;
            }
            settings = _objectMapper.MapObject<T>(prefix, dictionary);
            return true;
        }
    }
}