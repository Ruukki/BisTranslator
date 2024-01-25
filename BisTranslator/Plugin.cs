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
using Glamourer;
using Glamourer.Api;
using Glamourer.Automation;
using FFXIVClientStructs.Havok;
using Glamourer.State;
using Glamourer.Interop;
using Glamourer.Services;
using Penumbra.GameData.Actors;
using Glamourer.Unlocks;
using Glamourer.Events;
using Penumbra.GameData.DataContainers;
using Glamourer.Designs;
using Newtonsoft.Json.Linq;
using Penumbra.GameData.Data;

namespace BisTranslator
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "BisTranslator";
        private const string CommandName = "/slutify";
        private readonly ServiceProvider _services;

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        private Configuration _config { get; init; }
        

        

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            try
            {
                _services = ServiceHandler.CreateProvider(pluginInterface);
                var log = _services.GetRequiredService<IPluginLog>();
                _services.GetRequiredService<WindowsService>();
                _config = _services.GetRequiredService<Configuration>();
                Translations.SetName(_config.Name);
                _config.Save();
                log.Debug($"Cofgi path: {pluginInterface.GetPluginConfigDirectory()}");

                _services.GetRequiredService<ChatManager>(); // Initialize the OnChatMessage
                _services.GetRequiredService<ChatReader>(); // Initialize the chat message detour
                _services.GetRequiredService<ActionManager>();
                var overrides = _services.GetRequiredService<OverrideManager>();

                var client = _services.GetRequiredService<IClientState>();
                log.Debug($"client.IsLoggedIn: {client.IsLoggedIn}");
                if (client != null && client.IsLoggedIn)
                {
                    overrides.Login();
                    var sub = Glamourer.Api.GlamourerIpc.GetAllCustomizationSubscriber(pluginInterface);
                    var result = sub.Invoke("Miki Kiki");
                    log.Debug($"Result: {result}");
                    string decomp;
                    var bytes = Glamourer.Utility.CompressExtensions.DecompressToString(System.Convert.FromBase64String(result), out decomp);
                    var jo = JObject.Parse(decomp);
                    var neckId = jo["Equipment"]?["Neck"]?["ItemId"]?.ToString();
                    //log.Debug($"Decomp: {decomp}");
                    log.Debug($"Neck: {neckId}");
                }

            }
            catch
            {
                Dispose();
                throw;
            }

            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            //this.Configuration = this._pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            //this.Configuration.Initialize(this._pluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            //var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            //var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            

            /*this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });*/

            
        }

        public void Dispose()
        {
            if (_config != null && _config.lockOnDisable)
            {
                var move = _services.GetRequiredService<MoveManager>();
                if (move != null)
                {
                    move.DisableMoving();
                }
            }
            //this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
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
