using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Buttplug;
using Buttplug.Client;
using Buttplug.Client.Connectors;
using Buttplug.Client.Connectors.WebsocketConnector;
using Dalamud.Plugin.Services;

namespace BisTranslator.Services
{
    public class PlugService : IDisposable
    {
        private readonly ButtplugClient _client = new ButtplugClient("BisTranslator");
        private ButtplugWebsocketConnector _connector = new ButtplugWebsocketConnector(new Uri("ws://localhost:12345"));
        private double _stepSize;
        private ButtplugClientDevice? _device;
        private readonly IPluginLog _log;
        public bool Connected { get { return _client.Connected; } }
        public PlugService(IPluginLog log)
        {
            _log = log;
        }

        public async void ConnectAsync()
        {
            try
            {
                if (!_client.Connected)
                {
                    await _client.ConnectAsync(_connector);
                }

                if (_client.Connected)
                {
                    _device = _client.Devices.FirstOrDefault() ?? null;
                    if (_device != null)
                    {
                        if (_device.VibrateAttributes.Count > 0)
                        {
                            _stepSize = 1.0 / _device.VibrateAttributes.First().StepCount;
                        }
                    }
                    else
                    {
                        await _client.DisconnectAsync();
                    }
                }
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        public async void DisconnectAsync()
        {
            try
            {
                if (_client.Connected)
                {
                    await _client.DisconnectAsync();
                }
                _connector?.Dispose();
                _connector = new ButtplugWebsocketConnector(new Uri("ws://localhost:12345"));
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        public async void Vibrate(int seconds, double strength)
        {
            try
            {
                if (strength < 0 || strength > 1)
                {
                    _log.Information($"Strength must be between 0 and 1: {strength}");
                }
                if (_client.Connected && _device != null && seconds > 0)
                {
                    await _device.VibrateAsync(strength);
                    Thread.Sleep(seconds * 1000);
                    await _device.Stop();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        public void Dispose()
        {
            _client?.DisconnectAsync();
            _client?.Dispose();
            _connector?.Dispose();
        }

    }
}
