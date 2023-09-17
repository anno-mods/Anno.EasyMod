using Anno.EasyMod.Accountdata;
using Anno.EasyMod.Metadata;
using Anno.EasyMod.ModioWrapper;
using Anno.EasyMod.Mods;
using Anno.EasyMod.Mods.LocalMods;
using Anno.EasyMod.Mods.ModioMods;
using Anno.EasyMod.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Anno.EasyMod.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureEasyMod(this IServiceCollection services)
        {
            services.AddScoped<IModinfoLoader, ModinfoLoader>();

            services.AddScoped<IModFactory<LocalMod>, LocalModFactory>();
            services.AddScoped<ILocalModCollectionFactory, LocalModCollectionFactory>();
            services.AddTransient<CollectionBuilder>(sp => new CollectionBuilder(
                    sp.GetRequiredService<ILocalModCollectionFactory>(),
                    sp.GetService<IModioModCollectionFactory>()
                ));
        }

        public static void ConfigureModio(this IServiceCollection services, IModioAuthenticator authenticator)
        {
            services.AddScoped<IModioModFactory, ModioModFactory>();
            services.AddScoped<IModioModCollectionFactory, ModioModCollectionFactory>();
            services.AddSingleton<IModioAuthenticator>(authenticator);
            services.AddSingleton<IModioClientProvider, ModioClientProvider>();
            services.AddSingleton<AccountdataAccess>();
        }
    }
}
