using BallanceLauncher.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BallanceLauncher.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModDetailsPage : Page
    {
        private readonly BallanceMod _mod;

        public ModDetailsPage(BallanceMod mod)
        {
            _mod = mod;
            this.InitializeComponent();
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            await _mod.TryUpdateInfoAsync();

            ProgRing.Visibility = Visibility.Collapsed;

            if (_mod.Details == null || _mod.Details.Status != 0)
            {
                NotFound.Visibility = Visibility.Visible;
            }
            else
            {
                MainContent.Visibility = Visibility.Visible;
                ModName.Text = _mod.Details.Mod.Name;
                Author.Text = _mod.Details.Mod.Author;
                Version.Text = _mod.Details.Mod.Version;
                Description.Text = _mod.Details.Mod.Description;
                BMLVersion.Text = _mod.Details.Mod.BuildVersion;
            }
        }
    }
}
