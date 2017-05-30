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
using System.Collections.Specialized;
using Moq;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class ConfigurationManagerProviderTests
    {
        [Fact]
        public void DefaultSectionName_IsAppSettings()
        {
            var provider = new ConfigurationManagerProvider();

            Assert.Equal("appSettings", provider.DefaultSectionName);
        }

        [Fact]
        public void DefaultSectionName_WhenNull_ThenNotNull()
        {
            var provider = new ConfigurationManagerProvider();

            Assert.NotNull(provider.DefaultSectionName);
            provider.DefaultSectionName = null;
            Assert.NotNull(provider.DefaultSectionName);
        }

        [Fact]
        public void TryGetSetting_UsesAppSettings()
        {
            var testKey = Guid.NewGuid().ToString();
            var testValue = Guid.NewGuid().ToString();

            NameValueCollection ignored;
            var mockProvider = new Mock<ConfigurationManagerProvider> {CallBase = true};
            mockProvider
                .SetupGet(_ => _.DefaultSectionName)
                .CallBase()
                .Verifiable();
            mockProvider
                .Setup(_ => _.TryGetNameValueCollectionSection("appSettings", out ignored))
                .CallBase()
                .Verifiable();
            mockProvider
                .Setup(_ => _.GetSection("appSettings"))
                .Returns(() => new NameValueCollection {{testKey, testValue}})
                .Verifiable();

            var provider = mockProvider.Object;
            var successFlag = provider.TryGetSetting(testKey, out string value);
            Assert.True(successFlag);
            Assert.Equal(testValue, value);

            var randomKey = Guid.NewGuid().ToString();
            successFlag = provider.TryGetSetting(randomKey, out value);
            Assert.False(successFlag);
            Assert.Null(value);

            mockProvider.Verify();
        }
    }
}