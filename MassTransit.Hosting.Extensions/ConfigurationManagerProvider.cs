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
    public interface IConfigurationSectionProvider : IConfigurationProvider
    {
        object GetSection(string sectionName);
    }

    public interface IConfigurationManagerProvider : IConfigurationSectionProvider
    {
        string DefaultSectionName { get; set; }
    }

    public class ConfigurationManagerProvider : IConfigurationManagerProvider
    {
        private const string AppSettingsSectionName = "appSettings";
        private const string ConnectionStringsSectionName = "connectionStrings";

        private string _defaultSectionName;

        public virtual string DefaultSectionName
        {
            get => _defaultSectionName ?? (_defaultSectionName = AppSettingsSectionName);
            set => _defaultSectionName = value;
        }

        public virtual object GetSection(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }

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

        public virtual bool TryGetNameValueCollectionSection(string section, out NameValueCollection collection)
        {
            collection = GetSection(section) as NameValueCollection;
            return collection != null;
        }
    }
}