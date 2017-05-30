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
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MassTransit.Hosting.Extensions.Tests
{
    public class ResolvingSettingsProviderTests
    {
        [Fact]
        public void EndpointSettingsMissing()
        {
            var services = new ServiceCollection();
            services.AddTransient<ISettingsProvider, ResolvingSettingsProvider>();

            var serviceProvider = services.BuildServiceProvider();
            using ((IDisposable) serviceProvider)
            {
                var provider = serviceProvider.GetRequiredService<ISettingsProvider>();

                var exists = provider.TryGetSettings(out EndpointSettings settings);
                Assert.False(exists);
                Assert.Null(settings);
            }
        }

        [Fact]
        public void EndpointSettingsWithoutPrefix()
        {
            var services = new ServiceCollection();
            services.AddTransient<ISettingsProvider, ResolvingSettingsProvider>();

            var mockProvider = new Mock<ISettingsProvider<EndpointSettings>>(MockBehavior.Strict);

            EndpointSettings tempSettings = null;
            mockProvider
                .Setup(_ => _.TryGetSettings(It.IsAny<string>(), out tempSettings))
                .Returns(() => false);

            tempSettings = new EndpointOptions();
            mockProvider
                .Setup(_ => _.TryGetSettings(out tempSettings))
                .Returns(() => true);

            services.AddSingleton(mockProvider.Object);

            var serviceProvider = services.BuildServiceProvider();
            using ((IDisposable) serviceProvider)
            {
                var provider = serviceProvider.GetRequiredService<ISettingsProvider>();

                var exists = provider.TryGetSettings(out EndpointSettings settings);
                Assert.True(exists);
                Assert.Same(tempSettings, settings);

                exists = provider.TryGetSettings(null, out settings);
                Assert.False(exists);

                exists = provider.TryGetSettings(string.Empty, out settings);
                Assert.False(exists);

                exists = provider.TryGetSettings(Guid.NewGuid().ToString(), out settings);
                Assert.False(exists);
            }

            mockProvider.Verify();
        }

        [Fact]
        public void EndpointSettingsWithPrefix()
        {
            var services = new ServiceCollection();
            services.AddTransient<ISettingsProvider, ResolvingSettingsProvider>();

            var mockProvider = new Mock<ISettingsProvider<EndpointSettings>>(MockBehavior.Strict);

            EndpointSettings tempSettings = null;
            mockProvider
                .Setup(_ => _.TryGetSettings(out tempSettings))
                .Returns(() => false);

            var prefix = Guid.NewGuid().ToString();
            tempSettings = new EndpointOptions();
            mockProvider
                .Setup(_ => _.TryGetSettings(prefix, out tempSettings))
                .Returns(() => true);

            services.AddSingleton(mockProvider.Object);

            var serviceProvider = services.BuildServiceProvider();
            using ((IDisposable) serviceProvider)
            {
                var provider = serviceProvider.GetRequiredService<ISettingsProvider>();

                var exists = provider.TryGetSettings(prefix, out EndpointSettings settings);
                Assert.True(exists);
                Assert.Same(tempSettings, settings);

                exists = provider.TryGetSettings(out settings);
                Assert.False(exists);
            }

            mockProvider.Verify();
        }
    }
}