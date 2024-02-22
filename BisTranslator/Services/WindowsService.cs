using BisTranslator.Windows;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BisTranslator.Services
{
    public class WindowsService
    {
        public WindowSystem WindowSystem = new("BisTranslator");
        private readonly UiBuilder _uiBuilder;

        private ConfigWindow _configWindow { get; init; }
        private MainWindow _mainWindow { get; init; }
        private AbilitiesWindow _abilitiesWindow { get; init; }
        private Widget _widgetWindow { get; init; }
        private OverlayWindow _overlayWindow { get; init; }

        public WindowsService(UiBuilder uiBuilder, MainWindow mainWindow, ConfigWindow configWindow, AbilitiesWindow abilitiesWindow, Widget widgetWindow, OverlayWindow overlayWindow)
        {
            _uiBuilder = uiBuilder;

            _configWindow = configWindow;
            _mainWindow = mainWindow;
            _abilitiesWindow = abilitiesWindow;
            _widgetWindow = widgetWindow;
            _overlayWindow = overlayWindow;

            WindowSystem.AddWindow(_configWindow);
            WindowSystem.AddWindow(_mainWindow);
            WindowSystem.AddWindow(_abilitiesWindow);
            WindowSystem.AddWindow(_widgetWindow);
            WindowSystem.AddWindow(_overlayWindow);

            _uiBuilder.Draw += WindowSystem.Draw;
            _uiBuilder.OpenConfigUi += _mainWindow.Toggle;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            _configWindow.Dispose();
            _mainWindow.Dispose();
            _abilitiesWindow.Dispose();
            _widgetWindow.Dispose();
            _overlayWindow.Dispose();

            _uiBuilder.Draw -= WindowSystem.Draw;
            _uiBuilder.OpenConfigUi -= _mainWindow.Toggle;
        }
    }
}
