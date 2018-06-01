﻿using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RockLib.Configuration;
using Xunit;

namespace RockLib.Logging.AspNetCore.Tests
{
    public class AspNetExtensionsTests
    {
        [Fact]
        public void UseRockLibExtension1ThrowsOnNullBuilder()
        {
            Action action = () => ((IWebHostBuilder)null).UseRockLib();

            action.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: builder");
        }

        [Fact]
        public void UseRockLibExtension1AddsLoggerAndProvider()
        {
            if (!Config.IsLocked)
            {
                var dummy = Config.Root;
            }

            var actualLogger = LoggerFactory.GetInstance("SomeRockLibName");

            var serviceDescriptors = new List<ServiceDescriptor>();

            var servicesCollectionMock = new Mock<IServiceCollection>();
            servicesCollectionMock
                .Setup(scm => scm.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd => serviceDescriptors.Add(sd));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(m => m.GetService(typeof(ILogger))).Returns(actualLogger);

            var fakeBuilder = new FakeWebHostBuilder()
            {
                ServiceCollection = servicesCollectionMock.Object
            };

            fakeBuilder.UseRockLib("SomeRockLibName");

            servicesCollectionMock.Verify(lfm => lfm.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));

            // The first thing we happen to register is the RockLib.Logging.Logger
            var logger = (ILogger)serviceDescriptors[0].ImplementationFactory.Invoke(null);
            logger.Should().BeSameAs(actualLogger);

            // The second thing we happen to register is the RockLib.Logging.AspNetCore.RockLibLoggerProvider
            var provider = (RockLibLoggerProvider)serviceDescriptors[1].ImplementationFactory.Invoke(serviceProviderMock.Object);
            provider.Logger.Should().BeSameAs(actualLogger);
        }

        [Fact]
        public void UseRockLibExtension2ThrowsOnNullBuilder()
        {
            var actualLogger = new Mock<ILogger>().Object;

            Action action = () => ((IWebHostBuilder)null).UseRockLib(actualLogger);

            action.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: builder");
        }

        [Fact]
        public void UseRockLibExtension2ThrowsOnNullLogger()
        {
            var webHostBuilder = new Mock<IWebHostBuilder>().Object;

            Action action = () => webHostBuilder.UseRockLib((ILogger)null);

            action.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: logger");
        }

        [Fact]
        public void UseRockLibExtension2AddsLoggerAndProvider()
        {
            var actualLogger = new Mock<ILogger>().Object;

            var serviceDescriptors = new List<ServiceDescriptor>();

            var servicesCollectionMock = new Mock<IServiceCollection>();
            servicesCollectionMock
                .Setup(scm => scm.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd => serviceDescriptors.Add(sd));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(m => m.GetService(typeof(ILogger))).Returns(actualLogger);

            var fakeBuilder = new FakeWebHostBuilder()
            {
                ServiceCollection = servicesCollectionMock.Object
            };

            fakeBuilder.UseRockLib(actualLogger);

            servicesCollectionMock.Verify(lfm => lfm.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));

            // The first thing we happen to register is the RockLib.Logging.Logger
            var logger = (ILogger)serviceDescriptors[0].ImplementationInstance;
            logger.Should().BeSameAs(actualLogger);

            // The second thing we happen to register is the RockLib.Logging.AspNetCore.RockLibLoggerProvider
            var provider = (RockLibLoggerProvider)serviceDescriptors[1].ImplementationFactory.Invoke(serviceProviderMock.Object);
            provider.Logger.Should().BeSameAs(actualLogger);
        }

        private class FakeWebHostBuilder : IWebHostBuilder
        {
            public IServiceCollection ServiceCollection { get; set; }

            public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
            {
                configureServices(ServiceCollection);
                return this;
            }

            #region UnusedImplementations
            public IWebHost Build()
            {
                throw new NotImplementedException();
            }

            public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
            {
                throw new NotImplementedException();
            }

            public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
            {
                throw new NotImplementedException();
            }

            public string GetSetting(string key)
            {
                throw new NotImplementedException();
            }

            public IWebHostBuilder UseSetting(string key, string value)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
    }
}
