using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ChatTwo.Movement;
using System.Threading;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace BisTranslator.Services.Chat
{
    public class ChatManager
    {
        private readonly IChatGui _clientChat;
        private readonly Configuration _config;
        private readonly IClientState _clientState;
        private readonly IObjectTable _objectTable;
        private readonly MessageSender _messageSender;
        private readonly IPluginLog _log;
        private readonly MoveManager _moveManager;
        //private readonly MessageDecoder _messageDecoder;
        //private readonly MessageResultLogic _msgResultLogic;
        private readonly IFramework _framework;
        private Queue<string> messageQueue = new Queue<string>();
        private Stopwatch messageTimer = new Stopwatch();
        public ChatManager(IChatGui clientChat, Configuration config, IClientState clientState, IObjectTable objectTable,
    MessageSender messageSender, /*MessageDecoder messageDecoder, MessageResultLogic messageResultLogic,*/ IFramework framework, IPluginLog log, MoveManager moveManager)
        {
            _clientChat = clientChat;
            _config = config;
            _clientState = clientState;
            _objectTable = objectTable;
            _messageSender = messageSender;
            //_messageDecoder = messageDecoder;
            //_msgResultLogic = messageResultLogic;
            _framework = framework;
            _log = log;
            _moveManager = moveManager;

            // begin our framework check
            _framework.Update += framework_Update;
            // Begin our OnChatMessage Detection
            _clientChat.CheckMessageHandled += Chat_OnCheckMessageHandled;
            _clientChat.ChatMessage += Chat_OnChatMessage;
        }

        public void Dispose()
        {
            _framework.Update -= framework_Update;
            _clientChat.CheckMessageHandled -= Chat_OnCheckMessageHandled;
            _clientChat.ChatMessage -= Chat_OnChatMessage;
        }

        /// <summary> 
        /// This function will determine if we hide an incoming message or not. By default, this handles the hiding of all outgoing encoded tells
        /// <list type="bullet">
        /// <item><c>type</c><param name="type"> - The type of message that was sent.</param></item>
        /// <item><c>senderid</c><param name="senderid"> - The id of the sender.</param></item>
        /// <item><c>sender</c><param name="sender"> - The name of the sender.</param></item>
        /// <item><c>message</c><param name="message"> - The message that was sent.</param></item>
        /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message was handled.</param></item>
        /// </list> </summary>
        private void Chat_OnCheckMessageHandled(XivChatType type, uint senderid, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            var chatTypes = new[] { XivChatType.TellIncoming, XivChatType.TellOutgoing };
            // if the message is a outgoing tell
            if (true/*type == XivChatType.TellOutgoing*/)
            {
                _log.Debug($"Chat_OnCheckMessageHandled {message.TextValue}");
                var match = Regex.Match(message.TextValue, _config.CommandRegex);
                if (match.Success)
                {
                    _log.Debug($"match.Success {match.Groups[2].Value}");
                    _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddUiForeground(31).AddText($"{sender.TextValue}").AddUiForegroundOff()
                        .AddText($" forced you to {match.Groups[2].Value}.")
                        .AddItalicsOff().BuiltString);
                    MoveManager.DisableMoving();
                    Task.Run(() => {
                        Thread.Sleep(100);
                        _messageSender.SendMessage($"/{match.Groups[2].Value} motion");
                        Thread.Sleep(5000);
                        MoveManager.EnableMoving();
                    });
                    // if it does, hide it from the chat log
                    isHandled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// This function will determine what to do with an incoming message. By default, this handles the hiding of all incoming encoded tells
        /// <list type="bullet">
        /// <item><c>type</c><param name="type"> - The type of message that was sent.</param></item>
        /// <item><c>senderid</c><param name="senderid"> - The id of the sender.</param></item>
        /// <item><c>sender</c><param name="sender"> - The name of the sender.</param></item>
        /// <item><c>message</c><param name="message"> - The message that was sent.</param></item>
        /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message was handled.</param></item>
        /// </list> </summary>
        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString chatmessage, ref bool isHandled)
        {
            // create some new SeStrings for the message and the new line
            var fmessage = new SeString(new List<Payload>());
            var nline = new SeString(new List<Payload>());
            nline.Payloads.Add(new TextPayload("\n"));
            // make payload for the player
            PlayerPayload playerPayload;
            //removes special characters in party listings [https://na.finalfantasyxiv.com/lodestone/character/10080203/blog/2891974/]
            List<char> toRemove = new() {
            '','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','',
        };
            // convert the sender from SeString to String
            var sanitized = sender.ToString();
            // loop through each character in the toRemove list
            foreach (var c in toRemove) { sanitized = sanitized.Replace(c.ToString(), string.Empty); } // remove all special characters

            // if the sender is the local player, set the player payload to the local player 
            if (sanitized == _clientState.LocalPlayer?.Name.TextValue)
            {
                playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
                if (type == XivChatType.CustomEmote)
                {
                    var playerName = new SeString(new List<Payload>());
                    playerName.Payloads.Add(new TextPayload(_clientState.LocalPlayer.Name.TextValue));
                    fmessage.Append(playerName);
                }
            }
            // if the sender is not the local player, set the player payload to the sender
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type, dont care didnt ask.
            else
            {
                if (type == XivChatType.StandardEmote)
                {
                    playerPayload = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload ??
                                    chatmessage.Payloads.FirstOrDefault(x => x is PlayerPayload) as PlayerPayload;
                }
                else
                {
                    playerPayload = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload;
                    if (type == XivChatType.CustomEmote && playerPayload != null)
                    {
                        fmessage.Append(playerPayload.PlayerName);
                    }
                }
            }
#pragma warning restore CS8600 // let us see if we have any others
            // append the chat message to the new formatted message 
            fmessage.Append(chatmessage);
            var isEmoteType = type is XivChatType.CustomEmote or XivChatType.StandardEmote;
            if (isEmoteType)
            {
                fmessage.Payloads.Insert(0, new EmphasisItalicPayload(true));
                fmessage.Payloads.Add(new EmphasisItalicPayload(false));
            }
            // set the player name to the player payload, otherwise set it to the local player
            var pName = playerPayload == default(PlayerPayload) ? _clientState.LocalPlayer?.Name.TextValue : playerPayload.PlayerName;
            var sName = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload; // get the player payload from the sender 
            var senderName = sName?.PlayerName != null ? sName.PlayerName : pName;
            // if the message is an incoming tell
            #region TellIncomind
            //if (type == XivChatType.TellIncoming)
            //{
            //    if (senderName == null) { GagSpeak.Log.Error("senderName is null"); return; } // removes the possibly null reference warning

            //    switch (true)
            //    {
            //        // Logic commented on first case, left out on rest. All cases are the same, just with different conditions.
            //        case var _ when _config.friendsOnly && _config.partyOnly && _config.whitelistOnly: //  all 3 options are checked
            //                                                                                           // If a message is from a friend, or a party member, or a whitelisted player, it will become true,
            //                                                                                           // however, to make sure that we meet a condition that causes this to exit, we put a !() infront, to say
            //                                                                                           // they were a player outside of these parameters while the parameters were checked.
            //            if (!(IsFriend(senderName) || IsPartyMember(senderName) || IsWhitelistedPlayer(senderName))) { return; }
            //            break;

            //        case var _ when _config.friendsOnly && _config.partyOnly && !_config.whitelistOnly: // When both friend and party are checked
            //            if (!(IsFriend(senderName) || IsPartyMember(senderName))) { return; }
            //            break;

            //        case var _ when _config.friendsOnly && _config.whitelistOnly && !_config.partyOnly: // When both friend and whitelist are checked
            //            if (!(IsFriend(senderName) || IsWhitelistedPlayer(senderName))) { return; }
            //            break;

            //        case var _ when _config.partyOnly && _config.whitelistOnly && !_config.friendsOnly: // When both party and whitelist are checked
            //            if (!(IsPartyMember(senderName) || IsWhitelistedPlayer(senderName))) { return; }
            //            break;

            //        case var _ when _config.friendsOnly && !_config.partyOnly && !_config.whitelistOnly: // When only friend is checked
            //            if (!(IsFriend(senderName))) { return; }
            //            break;

            //        case var _ when _config.partyOnly && !_config.friendsOnly && !_config.whitelistOnly: // When only party is checked
            //            if (!(IsPartyMember(senderName))) { return; }
            //            break;

            //        case var _ when _config.whitelistOnly && !_config.friendsOnly && !_config.partyOnly: // When only whitelist is checked
            //            if (!(IsWhitelistedPlayer(senderName))) { return; }
            //            break;

            //        default: // None of the filters were checked, so just accept the message anyways because it works for everyone.
            //            break;
            //    }

            //    ////// Once we have reached this point, we know we have recieved a tell, and that it is from one of our filtered players. //////
            //    GagSpeak.Log.Debug($"[Chat Manager]: Recieved tell from: {senderName}");

            //    // if the incoming tell is an encoded message, lets check if we are in dom mode before accepting changes
            //    int encodedMsgIndex = 0; // get a index to know which encoded msg it is, if any
            //    if (MessageDictionary.EncodedMsgDictionary(chatmessage.TextValue, ref encodedMsgIndex))
            //    {
            //        // if in dom mode, back out, none of this will have any significance
            //        if (_config.InDomMode && encodedMsgIndex > 0 && encodedMsgIndex <= 8)
            //        {
            //            GagSpeak.Log.Debug("[Chat Manager]: Encoded Command Ineffective Due to Dominant Status");
            //            isHandled = true;
            //            return;
            //        }
            //        // if our encodedmsgindex is > 1 && < 6, we have a encoded message via command
            //        else if (encodedMsgIndex > 0 && encodedMsgIndex <= 8)
            //        {
            //            List<string> decodedCommandMsg = _messageDecoder.DecodeMsgToList(fmessage.ToString(), encodedMsgIndex);
            //            // function that will determine what happens to the player as a result of the tell.
            //            if (_msgResultLogic.CommandMsgResLogic(fmessage.ToString(), decodedCommandMsg, isHandled, _clientChat, _config))
            //            {
            //                isHandled = true; // logic sucessfully parsed, so hide from chat
            //            }
            //            _config.Save(); // save our config

            //            // for now at least, anything beyond 7 is a whitelist exchange message
            //        }
            //        else if (encodedMsgIndex > 8)
            //        {
            //            List<string> decodedWhitelistMsg = _messageDecoder.DecodeMsgToList(fmessage.ToString(), encodedMsgIndex);
            //            // function that will determine what happens to the player as a result of the tell.
            //            if (_msgResultLogic.WhitelistMsgResLogic(fmessage.ToString(), decodedWhitelistMsg, isHandled, _clientChat, _config))
            //            {
            //                isHandled = true; // logic sucessfully parsed, so hide from chat
            //            }
            //            isHandled = true;
            //            return;
            //        }
            //    } // skipped to this point if not encoded message
            //} // skips directly to here if not a tell
            #endregion
        }

        /// <summary>
        /// Will search through the senders friend list to see if they are a friend or not.
        /// <list type="bullet">
        /// <item><c>nameInput</c><param name="nameInput"> - The name who you want to see if they are in your friend list or not</param></item>
        /// </list></summary>
        /// <returns> True if they are a friend, false if they are not. </returns>
        private bool IsFriend(string nameInput)
        {
            // Check if it is possible for the client to grab the local player name, if so by default set to true.
            if (nameInput == _clientState.LocalPlayer?.Name.TextValue) return true;
            // after, scan through each object in the object table
            foreach (var t in _objectTable)
            {
                // If the object is a player character (us), we found ourselves, so conmtinue on..
                if (!(t is PlayerCharacter pc)) continue;
                // If the player characters name matches the list of names from local players 
                if (pc.Name.TextValue == nameInput)
                {
                    // See if they have a status of being a friend, if so return true, otherwise return false.
                    return pc.StatusFlags.HasFlag(StatusFlags.Friend);
                }
            }
            return false;
        }

        /// <summary>
        /// Will search through the senders party list to see if they are a party member or not.
        /// <list type="bullet">
        /// <item><c>nameInput</c><param name="nameInput"> - The name who you want to see if they are in your party list or not</param></item>
        /// </list></summary>
        /// <returns> True if they are a party member, false if they are not. </returns>
        private bool IsPartyMember(string nameInput)
        {
            if (nameInput == _clientState.LocalPlayer?.Name.TextValue) return true;
            foreach (var t in _objectTable)
            {
                if (!(t is PlayerCharacter pc)) continue;
                if (pc.Name.TextValue == nameInput)
                    return pc.StatusFlags.HasFlag(StatusFlags.PartyMember);
            }
            return false;
        }

        /// <summary>
        /// Will search through the senders party list to see if they are a party member or not.
        /// <list type="bullet">
        /// <item><c>nameInput</c><param name="nameInput"> - The name who you want to see if they are in your party list or not</param></item>
        /// </list></summary>
        /// <returns> True if they are a party member, false if they are not. </returns>
        private bool IsWhitelistedPlayer(string nameInput)
        {
            //// Check if it is possible for the client to grab the local player name, if so by default set to true.
            //if (nameInput == _clientState.LocalPlayer?.Name.TextValue)
            //{
            //    return true;
            //}
            //foreach (var t in _objectTable)
            //{
            //    if (!(t is PlayerCharacter pc)) continue;
            //    if (pc.Name.TextValue == nameInput)
            //    {
            //        foreach (var whitelistChar in _config.Whitelist)
            //        {
            //            if (whitelistChar.name.Contains(nameInput))
            //            {
            //                return true;
            //            }
            //        }
            //    }
            //}
            return false;
        }

        /// <summary>
        /// will send a real message to the chat, server side, USE WITH CAUTION
        /// <list type="bullet">
        /// <item><c>message</c><param name="message"> - The message to be sent to the server</param></item>
        /// </list></summary>
        public void SendRealMessage(string message)
        {
            //try
            //{
            //    _realChatInteraction.SendMessage(message);
            //}
            //catch
            //{
            //    GagSpeak.Log.Error($"[Chat Manager]: Failed to send message: {message}");
            //}
        }

        /// <summary>
        /// This function will handle the framework update, and will send messages to the server if there are any in the queue.
        /// <list type="bullet">
        /// <item><c>framework</c><param name="framework"> - The framework to be updated.</param></item>
        /// </list></summary>
        private void framework_Update(IFramework framework)
        {
            if (_config != null /*&& _config.Enabled*/)
            {
                try
                {
                    if (messageQueue.Count > 0 && _messageSender != null)
                    {
                        if (!messageTimer.IsRunning)
                        {
                            messageTimer.Start();
                        }
                        else
                        {
                            if (messageTimer.ElapsedMilliseconds > 1000)
                            {
                                try
                                {
                                    _messageSender.SendMessage(messageQueue.Dequeue());
                                }
                                catch (Exception e)
                                {
                                    //GagSpeak.Log.Warning($"{e},{e.Message}");
                                }
                                messageTimer.Restart();
                            }
                        }
                    }
                }
                catch
                {
                    //GagSpeak.Log.Error($"[Chat Manager]: Failed to process Framework Update!");
                }
            }
        }
    }


}
