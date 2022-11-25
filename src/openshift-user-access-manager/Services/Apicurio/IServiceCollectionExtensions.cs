using Microsoft.Extensions.DependencyInjection.Extensions;

namespace UserAccessManager.Services.Apicurio
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures the specified <see cref="ISerializer"/>
        /// </summary>
        /// <typeparam name="TSerializer">The type of <see cref="ISerializer"/> to register</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the <see cref="ISerializer"/> to add</param>
        /// <returns>The configured <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddSerializer<TSerializer>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TSerializer : class, ISerializer
        {
           // services.TryAdd(new ServiceDescriptor(typeof(ISerializerProvider), typeof(SerializerProvider), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(TSerializer), typeof(TSerializer), lifetime));
            services.Add(new ServiceDescriptor(typeof(ISerializer), typeof(TSerializer), lifetime));
           
            if (typeof(ITextSerializer).IsAssignableFrom(typeof(TSerializer)))
                services.Add(new ServiceDescriptor(typeof(ITextSerializer), provider => (ITextSerializer)provider.GetRequiredService(typeof(TSerializer)), lifetime));
            //if (typeof(IJsonSerializer).IsAssignableFrom(typeof(TSerializer)))
            //    services.Add(new ServiceDescriptor(typeof(IJsonSerializer), provider => (IJsonSerializer)provider.GetRequiredService(typeof(TSerializer)), lifetime));
           
            return services;
        }
    }
}
