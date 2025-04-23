using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using BisTranslator.Windows;
using Microsoft.Extensions.DependencyInjection;
using BisTranslator.Services;
using BisTranslator.Translator;
using BisTranslator.Services.Chat;
using System.Linq;
using BisTranslator.Services.Actions;
using System.Threading.Tasks;
using System.Threading;
using ChatTwo.Movement;
using System.Reflection;
using Dalamud.Configuration;
using BisTranslator.external;
using System;
using System.Runtime.CompilerServices;
using ECommons.DalamudServices;
using System.Windows.Forms;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using BisTranslator.Context;
using ECommons;

namespace BisTranslator
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "BisTranslator";
        private const string CommandName = "/slutify";
        private readonly ServiceProvider _services;

        private IDalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        private Configuration _config { get; init; }

        private IPluginLog log { get;set; }

        private OverrideManager overrides { get; set; }

        private Thread? overlayThread;
        private Form? overlayForm;

        

        public Plugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager)
        {
            try
            {
                this.PluginInterface = pluginInterface;
                this.CommandManager = commandManager;
                //ExtractOverlay(pluginInterface);
                ECommonsMain.Init(pluginInterface, this);
                _services = ServiceHandler.CreateProvider(pluginInterface);
                log = _services.GetRequiredService<IPluginLog>();
                _services.GetRequiredService<WindowsService>();
                _config = _services.GetRequiredService<Configuration>();
                Translations.SetName(_config.Name);
                _config.Save();
                log.Debug($"Cofgi path: {pluginInterface.GetPluginConfigDirectory()}");

                _services.GetRequiredService<ChatManager>(); // Initialize the OnChatMessage
                _services.GetRequiredService<ChatReader>(); // Initialize the chat message detour
                _services.GetRequiredService<ActionManager>();
                overrides = _services.GetRequiredService<OverrideManager>();

                var client = _services.GetRequiredService<IClientState>();
                log.Debug($"client.IsLoggedIn: {client.IsLoggedIn} clientNull: {client == null}");
                if (client != null)
                {
                    client.Login += OnLogin;
                    //debug
                    if (client.IsLoggedIn) 
                    {
                        var framework = _services.GetRequiredService<IFramework>();
                        framework.RunOnFrameworkThread(() => OnLogin() );

                        //                        
                        _services.GetRequiredService<ContextUpdate>().UpdateTerritoryChanged();
                    }
                }

                if (_config.Overlay)
                {
                    RunOverlay();
                }

            }
            catch
            {
                Dispose();
                throw;
            }            

            //this.Configuration = this._pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            //this.Configuration.Initialize(this._pluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            //var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            //var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            

            this.CommandManager.AddHandler("/test", new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            
        }

        [STAThread]
        private void RunOverlay()
        {
            log.Debug($"[Overlay] Overlay start");
            overlayThread = new Thread(() => {
                overlayForm = new WebOverlay.OverlayForm();
                Application.Run(overlayForm);
            });
            overlayThread.SetApartmentState(ApartmentState.STA);
            overlayThread.Start();
            log.Debug($"[Overlay] Overlay end");
        }

        private void StopOverlay()
        {
            if (overlayForm != null && !overlayForm.IsDisposed)
            {
                overlayForm.Invoke((MethodInvoker)(() =>
                {
                    overlayForm.Close(); // Triggers Application.Run to exit
                }));

                overlayThread?.Join(); // Optional: wait for thread to end
            }
        }

        private void OnLogin()
        {
            //log.Debug($"OnLogin()");
            //StartOverlay(PluginInterface);
            overrides.Login();
        }

        public void Dispose()
        {
            overrides.ClearMoodle();
            StopOverlay();
            if (_config != null && _config.lockOnDisable)
            {
                var move = _services.GetRequiredService<MoveManager>();
                if (move != null)
                {
                    move.DisableMoving();
                    log.Warning("Movement disabled by Dispose");
                }
            }
            //this.CommandManager.RemoveHandler(CommandName);
            overrides.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            var move = _services.GetRequiredService<MoveManager>();
            if (move != null)
            {
                move.EnableMoving();
                log.Warning("Test command");
            }
            // in response to the slash command, just display our main ui
            //_mainWindow.IsOpen = true;
        }

        /*private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            _configWindow.IsOpen = true;
        }

        public void DrawMainUI()
        {
            _mainWindow.IsOpen = true;
        }*/
    }
}
