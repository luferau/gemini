/*
 * Original source code from the Wide framework:
 * https://github.com/chandramouleswaran/Wide
 * 
 * Used in Gemini with kind permission of the author.
 *
 * Original licence follows:
 *
 * Copyright (c) 2013 Chandramouleswaran Ravichandran
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Gemini.Framework.Services;

namespace Gemini.Framework.Themes
{
    [Export(typeof(IThemeManager))]
    public class ThemeManager : IThemeManager
    {
        public event EventHandler CurrentThemeChanged;

        /// <summary>
        /// Raised for every theme resource dictionary that could not be loaded, so a host
        /// application can record it next to the rest of its startup diagnostics.
        /// </summary>
        public event EventHandler<ThemeResourceFailureEventArgs> ResourceLoadFailed;

        private readonly SettingsPropertyChangedEventManager<Properties.Settings> _settingsEventManager =
            new SettingsPropertyChangedEventManager<Properties.Settings>(Properties.Settings.Default);

        private ResourceDictionary _applicationResourceDictionary;

        public List<ITheme> Themes
        {
            get; private set;
        }

        public ITheme CurrentTheme { get; private set; }

        [ImportingConstructor]
        public ThemeManager([ImportMany] ITheme[] themes)
        {
            Themes = new List<ITheme>(themes);
            _settingsEventManager.AddListener(s => s.ThemeName, value => SetCurrentTheme(value));
        }

        public bool SetCurrentTheme(string name)
        {
            var theme = Themes.FirstOrDefault(x => x.GetType().Name == name);
            if (theme == null)
                return false;

            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null)
                return false;

            CurrentTheme = theme;

            if (_applicationResourceDictionary == null)
            {
                _applicationResourceDictionary = new ResourceDictionary();
                Application.Current.Resources.MergedDictionaries.Add(_applicationResourceDictionary);
            }
            _applicationResourceDictionary.BeginInit();
            _applicationResourceDictionary.MergedDictionaries.Clear();

            var windowResourceDictionary = mainWindow.Resources.MergedDictionaries[0];
            windowResourceDictionary.BeginInit();
            windowResourceDictionary.MergedDictionaries.Clear();

            var loadedAny = false;

            foreach (var uri in theme.ApplicationResources)
                loadedAny |= TryAddResourceDictionary(_applicationResourceDictionary, uri);

            foreach (var uri in theme.MainWindowResources)
                loadedAny |= TryAddResourceDictionary(windowResourceDictionary, uri);

            _applicationResourceDictionary.EndInit();
            windowResourceDictionary.EndInit();

            RaiseCurrentThemeChanged(EventArgs.Empty);

            return loadedAny;
        }

        /// <summary>
        /// Merges one dictionary and reports the ones that cannot load. A theme pulls dictionaries
        /// from other assemblies by pack:// URI, so a single missing or version-mismatched assembly
        /// would otherwise abort theme application - and with it the shell's OnViewLoaded.
        /// </summary>
        private bool TryAddResourceDictionary(ResourceDictionary target, Uri uri)
        {
            try
            {
                target.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
                return true;
            }
            catch (Exception error)
            {
                ReportResourceFailure(uri, error);
                return false;
            }
        }

        private void ReportResourceFailure(Uri uri, Exception error)
        {
            Trace.WriteLine("Theme resource could not be loaded: " + uri + " (" + error.GetType().Name + ": " + error.Message + ")");

            var handler = ResourceLoadFailed;
            if (handler != null)
                handler(this, new ThemeResourceFailureEventArgs(uri, error));
        }

        private void RaiseCurrentThemeChanged(EventArgs args)
        {
            var handler = CurrentThemeChanged;
            if (handler != null)
                handler(this, args);
        }
    }
}