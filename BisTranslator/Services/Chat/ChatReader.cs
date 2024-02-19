using BisTranslator.Utility;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BisTranslator.Services.Chat
{
    public unsafe class ChatReader
    {
        private readonly Configuration _config;                            // for config options
        //private readonly HistoryService _historyService;                    // for history service
        public virtual bool Ready { get; protected set; }       // see if ready
        public virtual bool Enabled { get; protected set; }     // set if enabled
        private nint processChatInputAddress;            // address of the chat input
        private HookWrapper<ProcessChatInputDelegate> processChatInputHook = null!;       // should be storing the chat message
        public static List<IHookWrapper> HookList = new();                   // list of hooks
        private unsafe delegate byte ProcessChatInputDelegate(nint uiModule, byte** message, nint a3);  // delegate for the chat input
        private readonly List<string> _configChannelsCommandsList;
        private readonly IPluginLog _pluginLog;
        private readonly IClientState _state;

        /// <summary> Initializes a new instance of the <see cref="ChatInputProcessor"/> class. </summary>
        public ChatReader(ISigScanner scanner, IGameInteropProvider interop, Configuration config, IPluginLog pluginLog, IClientState clientState)
        {
            // initialize interopfromattributes
            _config = config;
            _pluginLog = pluginLog;
            _state = clientState;
            //_configChannelsCommandsList = _config.Channels.GetChatChannelsListAliases();
            //_historyService = historyService;
            interop.InitializeFromAttributes(this);
            // try to get the chatinput address
            try
            {
                // try to get the chatinput address
                processChatInputAddress = scanner.ScanText("E8 ?? ?? ?? ?? FE 86 ?? ?? ?? ?? C7 86 ?? ?? ?? ?? ?? ?? ?? ??");
                Ready = true;
                _pluginLog.Debug($"[Chat Processor]: Input Address Found Sucessfully: {processChatInputAddress:X}");
            }
            catch
            {
                _pluginLog.Error($"[Chat Processor]: Failed to find input address!");
            }

            // if we get here, it means we can enable our scanner, because we found the address
            // but just as a failsafe, if we aren't ready, abort and return.
            if (!Ready)
            {
                _pluginLog.Error($"[Chat Processor]: You really REALLY shouldnt be seeing this!!!!");
                return;
            }
            //set up our hooks
            // first setup a temp storage to yoink from
            try
            {
                _pluginLog.Debug("[Chat Processor]: Setting up hooks");
                var h = interop.HookFromAddress(processChatInputAddress, new ProcessChatInputDelegate(ProcessChatInputDetour));
                _pluginLog.Debug("[Chat Processor]: Setting up hook wrapper");
                var wh = new HookWrapper<ProcessChatInputDelegate>(h); // make it a hook wrapper
                HookList.Add(wh); // add it to the hook list
                processChatInputHook = wh; // set the hook to the hook wrapper
                processChatInputHook?.Enable(); // enable the hook
                _pluginLog.Debug("[Chat Processor]: Hook setup complete");
                Enabled = true; // set enabled to true
            }
            catch
            {
                _pluginLog.Error("[Chat Processor]: Failed to setup hooks");
            }
        }

        /// <summary>
        /// Process the chatbox every time a new message is sent.
        /// <list type="bullet">
        /// <item><c>uiModule</c><param name="uiModule"> - The ui module.</param></item>
        /// <item><c>message</c><param name="message"> - The message.</param></item>
        /// <item><c>a3</c><param name="a3"> - The a3.</param></item>
        /// </list> </summary>
        /// <returns> The <see cref="byte"/>. </returns>
        /// <exception cref="Exception"> Thrown when an exception error condition of any kind occurs. </exception>
        private unsafe byte ProcessChatInputDetour(nint uiModule, byte** message, nint a3)
        {
            _pluginLog.Debug("[Chat Processor]: Detouring Chat Input Message");
            // try the following
            try
            {
                var bc = 0;
                int matchSequence = 0;
                for (var i = 0; i <= 500; i++)
                { // making sure command / message is within 500 characters
                    if (i + 5 < 500 && (*message)[i] == 0x02 && (*message)[i+1] == 0x2e) matchSequence += 2;
                    if ((*message)[i] == 0xf2 && matchSequence == 2) matchSequence++;
                    if ((*message)[i] == 0x03 && matchSequence == 3) matchSequence++;
                    //_pluginLog.Debug($"[Chat Processor]: XXX: {matchSequence}");

                    if (*(*message + i) != 0) continue; // if the message is empty, break
                    bc = i; // increment bc
                    break;
                }
                if (bc < 2 || bc > 500 || _config.BigPussy || matchSequence == 4)
                {
                    // if we satsify this condition it means our message is an invalid message so disregard it
                    return processChatInputHook.Original(uiModule, message, a3); // just send the message as invalid or whatever
                }

                /*
                StringBuilder hex = new StringBuilder(bc * 2);
                for (int i = 0; i < bc; i++)
                {
                    hex.AppendFormat("{0:x2} ", (*message)[i]);
                }
                _pluginLog.Debug($"[Chat Processor]: Message bytes: {hex}");
                _pluginLog.Debug($"[Chat Processor]: uiModule: {uiModule}");
                _pluginLog.Debug($"[Chat Processor]: a3: {a3}");
                */

                var inputString = Encoding.UTF8.GetString(*message, bc);
                var matchedCommand = "";
                _pluginLog.Debug($"[Chat Processor]: Detouring Message: {inputString}"); // see our message

                // first let's make sure its not a command
                #region
                //if (inputString.StartsWith("/"))
                //{
                //    // Check if command is not one of configured channels commands
                //    matchedCommand = _configChannelsCommandsList.FirstOrDefault(prefix => inputString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

                //    if (matchedCommand.IsNullOrEmpty())
                //    {
                //        // if it is isn't a command, we can just return the original message
                //        _pluginLog.Debug("[Chat Processor]: Ignoring Message as it is a command");
                //        return processChatInputHook.Original(uiModule, message, a3);
                //    }
                //    // if tell command is matched, need extra step to protect target name
                //    else if (matchedCommand.StartsWith("/tell") || matchedCommand.StartsWith("/t"))
                //    {
                //        /// Using /gag command on yourself sends /tell which should be caught by this
                //        /// Depends on <seealso cref="MsgEncoder.MessageEncoder"/> message to start like :"/tell {targetPlayer} *{playerPayload.PlayerName}"
                //        /// Since only outgoing tells are affected, {targetPlayer} and {playerPayload.PlayerName} will be the same
                //        var selfTellRegex = @"(?<=^|\s)/t(?:ell)?\s{1}(?<name>\S+\s{1}\S+)@\S+\s{1}\*\k<name>(?=\s|$)";
                //        if (!Regex.Match(inputString, selfTellRegex).Value.IsNullOrEmpty())
                //        {
                //            _pluginLog.Debug("[Chat Processor]: Ignoring Message as it is a self /gag command");
                //            return processChatInputHook.Original(uiModule, message, a3);
                //        }
                //        // Match any other outgoing tell to preserve target name
                //        var tellRegex = @"(?<=^|\s)/t(?:ell)?\s{1}\S+\s{1}\S+@\S+(?=\s|$)";
                //        matchedCommand = Regex.Match(inputString, tellRegex).Value;
                //    }
                //}
                #endregion

                // if our current channel is in our list of enabled channels AND we have enabled direct chat translation...
                if (/*_config.Channels.Contains(Data.ChatChannel.GetChatChannel())*/true)
                {
                    // if we satisfy this condition, it means we can try to attempt modifying the message.
                    _pluginLog.Debug($"[Chat Processor]: Modifying Message");
                    // we can try to attempt modifying the message.
                    try
                    {
                        _pluginLog.Debug($"[Chat Processor]: Input -> {inputString}, MatchedCommand -> {matchedCommand}");
                        // create the output translated text, cutting the command matched before to prevent it getting gargled
                        //var output = _messageGarbler.GarbleMessage(inputString.Substring(matchedCommand.Length), _config.GarbleLevel);
                        string output = inputString;
                        var gagSpeakRegex = @$"(?<=^|\s)/t(?:ell)?\s{{1}}(?<name>\S+\s{{1}}\S+)@\S+\s{{1}}\*{_state?.LocalPlayer?.Name.TextValue}(?=\s|$)";
                        if (Regex.Match(inputString, gagSpeakRegex).Value.IsNullOrEmpty())
                        {
                            output = Translator.Translations.Translate(inputString);
                        }
                        // adding command back to front
                        //output = matchedCommand + output;
                        _pluginLog.Debug($"[Chat Processor]: Output -> {output}");
                        //_historyService.AddTranslation(new Translation(inputString, output));
                        // create the new string
                        var newStr = output;
                        // if our new string is less than or equal to 500 characters, we can alias it
                        if (newStr.Length <= 500)
                        {
                            // log the sucessful alias
                            _pluginLog.Debug($"[Chat Processor]: New Packet Message: {newStr}");
                            // encode the new string
                            var bytes = Encoding.UTF8.GetBytes(newStr);
                            // allocate the memory
                            var mem1 = Marshal.AllocHGlobal(400);
                            var mem2 = Marshal.AllocHGlobal(bytes.Length + 30);
                            // copy and write the new memory into the allocated memory
                            Marshal.Copy(bytes, 0, mem2, bytes.Length);
                            Marshal.WriteByte(mem2 + bytes.Length, 0);
                            Marshal.WriteInt64(mem1, mem2.ToInt64());
                            Marshal.WriteInt64(mem1 + 8, 64);
                            Marshal.WriteInt64(mem1 + 8 + 8, bytes.Length + 1);
                            Marshal.WriteInt64(mem1 + 8 + 8 + 8, 0);
                            // properly send off the new message by setting it to r at the right pointer
                            var r = processChatInputHook.Original(uiModule, (byte**)mem1.ToPointer(), a3);
                            // free up the memory we used for assigning
                            Marshal.FreeHGlobal(mem1);
                            Marshal.FreeHGlobal(mem2);
                            // return the result of the alias
                            return r;
                        }
                        // if we reached this point, it means our message was longer than 500 character, inform the user!
                        _pluginLog.Error("[Chat Processor]: Message after was applied is too long!");
                        return 0; // fucking ABORT!
                    }
                    catch (Exception e)
                    { // if at any point we fail here, throw an exception.
                        _pluginLog.Error($"[Chat Processor]: Error sending message to chatbox: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            { // cant ever have enough safety!
                _pluginLog.Error($"[Chat Processor]: Error sending message to chatbox (secondary): {e.Message}");
            }
            // return the original message untranslated
            return processChatInputHook.Original(uiModule, message, a3);
        }

        // method to disable the hook
        protected void Disable()
        {
            processChatInputHook?.Disable();
            Enabled = false;
        }
        // method to dispose of the hook, self explanitory
        public void Dispose()
        {
            if (!Ready) return;

            foreach (var hook in HookList)
            {
                hook?.Disable();
                hook?.Dispose();
            }
            HookList.Clear();

            processChatInputHook?.Disable();
            processChatInputHook?.Dispose();
            Ready = false;
            Enabled = false;
        }
    }
}
