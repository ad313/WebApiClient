﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using WebApiClientCore;
using WebApiClientCore.ResponseCaches;
using WebApiClientCore.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 提供HttpApi相关扩展
    /// </summary>
    public static class HttpApiExtensions
    {
        /// <summary>
        /// 添加HttpApi代理类到服务
        /// </summary>
        /// <typeparam name="THttpApi"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpApi<THttpApi>(this IServiceCollection services) where THttpApi : class
        {
            services.AddOptions();
            services.AddMemoryCache();
            services.TryAddSingleton<IXmlSerializer, XmlSerializer>();
            services.TryAddSingleton<IJsonSerializer, JsonSerializer>();
            services.TryAddSingleton<IKeyValueSerializer, KeyValueSerializer>();
            services.TryAddSingleton<IResponseCacheProvider, ResponseCacheProvider>();

            var name = typeof(THttpApi).FullName;
            return services
                .AddHttpClient(name)
                .AddTypedClient((client, serviceProvider) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptionsMonitor<HttpApiOptions>>().Get(name);
                    return HttpApi.Create<THttpApi>(client, serviceProvider, options);
                });
        }

        /// <summary>
        /// 添加HttpApi代理类到服务
        /// </summary>
        /// <typeparam name="THttpApi"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureOptions">配置选项</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpApi<THttpApi>(this IServiceCollection services, Action<HttpApiOptions> configureOptions) where THttpApi : class
        {
            services
                .AddOptions<HttpApiOptions>(typeof(THttpApi).FullName)
                .Configure(configureOptions);

            return services.AddHttpApi<THttpApi>();
        }

        /// <summary>
        /// 添加HttpApi代理类到服务
        /// </summary>
        /// <typeparam name="THttpApi"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureOptions">配置选项</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpApi<THttpApi>(this IServiceCollection services, Action<HttpApiOptions, IServiceProvider> configureOptions) where THttpApi : class
        {
            services
                .AddOptions<HttpApiOptions>(typeof(THttpApi).FullName)
                .Configure(configureOptions);

            return services.AddHttpApi<THttpApi>();
        }


        /// <summary>
        /// 添加HttpApi代理类到服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="httpApiType">接口类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpApi(this IServiceCollection services, Type httpApiType)
        {
            return services.CreateHttpApiBuilder(httpApiType).AddHttpApi();
        }

        /// <summary>
        /// 添加HttpApi代理类到服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="httpApiType">接口类型</param>
        /// <param name="configureOptions">配置选项</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpApi(this IServiceCollection services, Type httpApiType, Action<HttpApiOptions> configureOptions)
        {
            services
                .AddOptions<HttpApiOptions>(httpApiType.FullName)
                .Configure(configureOptions);

            return services.CreateHttpApiBuilder(httpApiType).AddHttpApi();
        }

        /// <summary>
        /// 添加HttpApi代理类到服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="httpApiType">接口类型</param>
        /// <param name="configureOptions">配置选项</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IHttpClientBuilder AddHttpApi(this IServiceCollection services, Type httpApiType, Action<HttpApiOptions, IServiceProvider> configureOptions)
        {
            services
                .AddOptions<HttpApiOptions>(httpApiType.FullName)
                .Configure(configureOptions);

            return services.CreateHttpApiBuilder(httpApiType).AddHttpApi();
        }

        /// <summary>
        /// 创建HttpApiBuilder
        /// </summary>
        /// <param name="services"></param>
        /// <param name="httpApiType">接口类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        private static IHttpApiBuilder CreateHttpApiBuilder(this IServiceCollection services, Type httpApiType)
        {
            if (httpApiType == null)
            {
                throw new ArgumentNullException(nameof(httpApiType));
            }

            var builderType = typeof(HttpApiBuilder<>).MakeGenericType(httpApiType);
            return builderType.CreateInstance<IHttpApiBuilder>(services);
        }

        /// <summary>
        /// 定义httpApi的Builder的行为
        /// </summary>
        private interface IHttpApiBuilder
        {
            /// <summary>
            /// 添加HttpApi代理类到服务
            /// </summary>
            /// <returns></returns>
            IHttpClientBuilder AddHttpApi();
        }

        /// <summary>
        /// httpApi的Builder
        /// </summary>
        /// <typeparam name="THttpApi"></typeparam>
        private class HttpApiBuilder<THttpApi> : IHttpApiBuilder where THttpApi : class
        {
            private readonly IServiceCollection services;

            /// <summary>
            /// httpApi的Builder
            /// </summary>
            /// <param name="services"></param>
            public HttpApiBuilder(IServiceCollection services)
            {
                this.services = services;
            }

            /// <summary>
            /// 添加HttpApi代理类到服务
            /// </summary> 
            /// <returns></returns>
            public IHttpClientBuilder AddHttpApi()
            {
                return this.services.AddHttpApi<THttpApi>();
            }
        }
    }
}
