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

using System.Collections.Specialized;
using System.Configuration;

namespace MassTransit.Hosting.Extensions
{
    /// <summary>
    /// An extension to <see cref="IConfigurationProvider"/> that allows to
    /// load additional configuration sections other than <see cref="NameValueCollection"/>.
    /// </summary>
    public interface IConfigurationSectionProvider : IConfigurationProvider
    {
        /// <summary>
        /// Retrieves a specified configuration section for the current
        /// application's configuration.
        /// </summary>
        /// <param name="sectionName">The configuration section path and name.</param>
        /// <returns>The specified <see cref="ConfigurationSection"/> object,
        /// or <c>null</c> if the section does not exist.</returns>
        object GetSection(string sectionName);
    }

    /// <summary>
    /// An extension to <see cref="IConfigurationSectionProvider"/> that allows
    /// to change the name of the default section name to be other than <code>appSettings</code>.
    /// </summary>
    public interface IConfigurationManagerProvider : IConfigurationSectionProvider
    {
        /// <summary>
        /// Specifies the name of the section to use when loading settings.
        /// The default is initialized with <c>appSettings</c>.
        /// </summary>
        string DefaultSectionName { get; set; }
    }

    /// <summary>
    /// Provides a default implementation of <see cref="IConfigurationManagerProvider"/>
    /// that will load its settings from the builtin <see cref="ConfigurationManager"/>
    /// class from Microsoft.
    /// </summary>
    public class ConfigurationManagerProvider : IConfigurationManagerProvider
    {
        private const string AppSettingsSectionName = "appSettings";
        private const string ConnectionStringsSectionName = "connectionStrings";

        private string _defaultSectionName;

        /// <inheritdoc />
        public virtual string DefaultSectionName
        {
            get => _defaultSectionName ?? (_defaultSectionName = AppSettingsSectionName);
            set => _defaultSectionName = value;
        }

        /// <inheritdoc />
        public virtual object GetSection(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }

        /// <inheritdoc />
        public virtual bool TryGetSetting(string name, out string value)
        {
            if (!TryGetNameValueCollectionSection(DefaultSectionName, out NameValueCollection collection))
            {
                value = null;
                return false;
            }

            value = collection.Get(name);
            return value != null;
        }

        /// <inheritdoc />
        public virtual bool TryGetConnectionString(string name, out string connectionString, out string providerName)
        {
            var section = GetSection(ConnectionStringsSectionName) as ConnectionStringsSection;
            if (section == null)
            {
                connectionString = null;
                providerName = null;
                return false;
            }

            var item = section.ConnectionStrings[name];
            if (item == null)
            {
                connectionString = null;
                providerName = null;
                return false;
            }

            connectionString = item.ConnectionString;
            providerName = item.ProviderName;
            return true;
        }

        /// <inheritdoc />
        public virtual bool TryGetNameValueCollectionSection(string section, out NameValueCollection collection)
        {
            collection = GetSection(section) as NameValueCollection;
            return collection != null;
        }
    }
}