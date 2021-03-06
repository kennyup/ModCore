﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModCore.Abstraction.DataAccess;
using ModCore.Abstraction.Plugins;
using ModCore.Core.Controllers;
using ModCore.Core.Plugins;
using ModCore.Models.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModCore.Core.HelperExtensions
{
    public static class MvcExtensions
    {

        public static IApplicationBuilder UseMvcWithPlugin(
         this IApplicationBuilder app,
         Action<IRouteBuilder> configureRoutes)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configureRoutes == null)
            {
                throw new ArgumentNullException(nameof(configureRoutes));
            }

            // Verify if AddMvc was done before calling UseMvc
            // We use the MvcMarkerService to make sure if all the services were added.
            if (app.ApplicationServices.GetService(typeof(MvcMarkerService)) == null)
            {
                throw new InvalidOperationException("UseMvc called before AddMvc was done.");
            }

            var routes = new PluginRouteBuilder(app)
            {
                DefaultHandler = app.ApplicationServices.GetRequiredService<MvcRouteHandler>(),
                PluginManager = app.ApplicationServices.GetRequiredService<IPluginManager>(),
            };

            if (app.ApplicationServices.GetService(typeof(IPluginManager)) == null)
            {
                throw new InvalidOperationException("Plugin manager was not set up correctly");
            }

            var pluginManager = app.ApplicationServices.GetService<IPluginManager>();

            configureRoutes(routes);

            //Disable for now.
            //routes.Routes.Insert(0, AttributeRouting.CreateAttributeMegaRoute(app.ApplicationServices));

            return app.UseRouter(routes.Build());
        }

        public static IServiceCollection AddPlugins(this IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<IControllerActivator, ValidateControllerActivator>();
            services.AddTransient<IPluginLog, PluginLogger>();
            services.AddTransient<IPluginSettingsManager, PluginSettingsManager>();
            services.AddTransient<IAssemblyManager, PluginAssemblyManager>();
            services.AddTransient<IRouteBuilder, PluginRouteBuilder>();
            services.AddSingleton<IActionDescriptorCollectionProvider, PluginActionDescriptorCollectionProvider>();


            mvcBuilder.AddRazorOptions(a => a.ViewLocationExpanders.Add(new PluginViewLocationExpander()));


            return services;
        }

        public static IServiceCollection AddPluginManager(this IServiceCollection services, IConfigurationRoot configRoot,IHostingEnvironment env)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IPluginManager, PluginManager>(srcProvider =>
            {
                var assbly = srcProvider.GetRequiredService<IAssemblyManager>();
                var repos = srcProvider.GetRequiredService<IDataRepository<InstalledPlugin>>();
                var appMgr = srcProvider.GetRequiredService<ApplicationPartManager>();

                return new PluginManager(assbly, configRoot, env, repos, appMgr);
            });

            return services;
        }


    }
}
