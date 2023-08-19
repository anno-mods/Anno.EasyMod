using Anno.EasyMod.Metadata;
using Anno.EasyMod.Mods;
using Anno.EasyMod.Mods.LocalMods;
using Anno.EasyMod.Mods.ModioMods;
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
            services.AddScoped<IModioModFactory, ModioModFactory>();
            services.AddScoped<IModioModCollectionFactory, ModioModCollectionFactory>();
        }

        public static void ConfigureModio(this IServiceCollection services, Modio.Client client)
        {
            services.AddSingleton<Modio.Client>(client);
        }
    }
}
