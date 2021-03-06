using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Promenade.Models;
using Promenade.Services;
using Promenade.Services.Abstract;

namespace Promenade
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.AddMemoryCache();

            services.AddSingleton<ConcurrencyService>();
            services.AddSingleton<ContentService>();
            services.AddSingleton<GeoService>();
            services.AddSingleton<IDbService, MongoService>();
            services.AddSingleton<ISocialService, VkService>();
        }

        public void Configure(IApplicationBuilder app, IDbService dbService, ContentService contentService)
        {
            app.UseRouting();
            app.UseFileServer();
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            dbService.Init("promenade", type =>
            {
                if (type == typeof(User)) return "users";
                throw new ArgumentOutOfRangeException(nameof(type), $"No collection for type: {type.FullName}");
            });
            
            contentService.Init();
        }
    }
}