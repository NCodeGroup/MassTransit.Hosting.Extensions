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
    /// Represents a <see cref="ICandidateSettingsProvider"/> that will attempt
    /// to retrieve settings from <see cref="IConfigurationProvider"/>.
    /// </summary>
    public interface IConfigurationSettingsProvider : ICandidateSettingsProvider
    {
        // nothing
    }

    /// <summary>
    /// Provides an implementation of <see cref="ICandidateSettingsProvider"/> that
    /// will attempt to retrieve settings from <see cref="IConfigurationProvider"/>.
    /// </summary>
    public class ConfigurationSettingsProvider : IConfigurationSettingsProvider
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IObjectMapper _objectMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSettingsProvider"/> class.
        /// </summary>
        /// <param name="configurationProvider">The <see cref="IConfigurationProvider"/> to load configuration settings from.</param>
        /// <param name="objectMapper">The <see cref="IObjectMapper"/> to materialize objects from configuration settings.</param>
        public ConfigurationSettingsProvider(IConfigurationProvider configurationProvider, IObjectMapper objectMapper)
        {
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _objectMapper = objectMapper ?? throw new ArgumentNullException(nameof(objectMapper));
        }

        /// <inheritdoc />
        public virtual bool TryGetSettings<T>(string prefix, out T settings) where T : ISettings
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

        /// <inheritdoc />
        public virtual bool TryGetSettings<T>(out T settings) where T : ISettings
        {
            return TryGetSettings(null, out settings);
        }

    }
}