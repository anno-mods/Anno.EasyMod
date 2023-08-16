using Anno.EasyMod.Metadata;
using Anno.EasyMod.Mods;
using Anno.EasyMod.Mods.LocalMods;
using Microsoft.Extensions.DependencyInjection;

namespace Anno.EasyMod.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureEasyMod(this IServiceCollection services)
        {
            services.AddScoped<IModFactory<LocalMod>, LocalModFactory>();
            services.AddScoped<IModinfoLoader, ModinfoLoader>(); 
        
        }
    }
}
