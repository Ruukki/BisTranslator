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

        public WindowsService(UiBuilder uiBuilder, MainWindow mainWindow, ConfigWindow configWindow, AbilitiesWindow abilitiesWindow)
        {
            _uiBuilder = uiBuilder;

            _configWindow = configWindow;
            _mainWindow = mainWindow;
            _abilitiesWindow = abilitiesWindow;

            WindowSystem.AddWindow(_configWindow);
            WindowSystem.AddWindow(_mainWindow);
            WindowSystem.AddWindow(_abilitiesWindow);

            _uiBuilder.Draw += WindowSystem.Draw;
            _uiBuilder.OpenConfigUi += _mainWindow.Toggle;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            _configWindow.Dispose();
            _mainWindow.Dispose();
            _abilitiesWindow.Dispose();

            _uiBuilder.Draw -= WindowSystem.Draw;
            _uiBuilder.OpenConfigUi -= _mainWindow.Toggle;
        }
    }
}
