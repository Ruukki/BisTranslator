using BisTranslator.Services.Actions;
using BisTranslator.Services.Chat;
using BisTranslator.Windows;
using ChatTwo.Movement;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using GagSpeak.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Services
{
    public static class ServiceHandler
    {
        public static ServiceProvider CreateProvider(DalamudPluginInterface pi)
        {
            // Create a service collection (see Dalamud.cs, if confused about AddDalamud, that is what AddDalamud(pi) pulls from)
            var services = new ServiceCollection()
                .AddDalamud(pi)
                .AddMeta(pi)
                .AddMovement()
                .AddChat()
                .AddExtras()
                .AddAction()
                //.AddApi()
                .AddUi();
            // return the built services provider in the form of a instanced service collection
            return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        }

        private static IServiceCollection AddDalamud(this IServiceCollection services, DalamudPluginInterface pi)
        {
            // Add the dalamudservices to the service collection
            new DalamudServices(pi).AddServices(services);
            return services;
        }

        private static IServiceCollection AddMeta(this IServiceCollection services, DalamudPluginInterface pi)
        {
            Configuration? config = pi.GetPluginConfig() as Configuration;
            if (config != null)
            {
                config.LoadInterface(pi);
                return services.AddSingleton<Configuration>(config);
            }
            return services.AddSingleton<Configuration>();
        }

        private static IServiceCollection AddChat(this IServiceCollection services)
        => services.AddSingleton<ChatManager>()
             .AddSingleton<MessageSender>(_ => { var sigService = _.GetRequiredService<ISigScanner>(); return new MessageSender(sigService); })
             .AddSingleton<ChatReader>(_ => {
                 // this shit is all a bit wild but its nessisary to handle our danger file stuff correctly. Until you learn more about signatures, i dont advise
                 // you to try and replicate this. However, when you do, just know this is how to correctly integrate them into a service collection structure
                 var sigService = _.GetRequiredService<ISigScanner>();
                 var interop = _.GetRequiredService<IGameInteropProvider>();
                 var config = _.GetRequiredService<Configuration>();
                 var logger = _.GetRequiredService<IPluginLog>();
                 //var historyService = _.GetRequiredService<HistoryService>();
                 return new ChatReader(sigService, interop, config, logger);
             })
             /*.AddSingleton<MessageEncoder>()
             .AddSingleton<MessageDecoder>()
             .AddSingleton<MessageResultLogic>()
             .AddSingleton<GagManager>()*/;

        private static IServiceCollection AddMovement(this IServiceCollection services)
        => services.AddSingleton<MoveManager>()
             .AddSingleton<MoveMemory>(_ => { var interop = _.GetRequiredService<IGameInteropProvider>(); return new MoveMemory(interop); });

        private static IServiceCollection AddAction(this IServiceCollection services)
        => services.AddSingleton<ActionManager>();

        private static IServiceCollection AddExtras(this IServiceCollection services)
        => services.AddSingleton<PlugService>()
            .AddSingleton<OverrideManager>();

        private static IServiceCollection AddUi(this IServiceCollection services)
        => services.AddSingleton<WindowsService>()
            .AddSingleton<MainWindow>()
            .AddSingleton<ConfigWindow>()
            .AddSingleton<AbilitiesWindow>()
            .AddSingleton<Widget>();

        /*private static IServiceCollection AddApi(this IServiceCollection services)
        => services.AddSingleton<CommandManager>();*/
    }
}
