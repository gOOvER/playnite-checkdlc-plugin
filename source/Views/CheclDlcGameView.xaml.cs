using CheckDlc.Models;
using CheckDlc.Services;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using CommonPluginsShared;
using CommonPluginsShared.Converters;
using System.Globalization;

namespace CheckDlc.Views
{
    /// <summary>
    /// Logique d'interaction pour CheclDlcGameView.xaml
    /// </summary>
    public partial class CheclDlcGameView : UserControl
    {
        private CheckDlc Plugin { get; }
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;

        private Game GameContext { get; set; }


        public CheclDlcGameView(CheckDlc plugin, Game gameContext)
        {
            Plugin = plugin;

            InitializeComponent();

            GameContext = gameContext;
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked);

            PART_PriceNotification.Visibility = (PluginDatabase.Get(gameContext, true)?.IsManual ?? false) ? Visibility.Collapsed : Visibility.Visible;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).Tag is string url &&
                Uri.TryCreate(url, UriKind.Absolute, out Uri uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            }
        }


        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            PART_Dlcs.ItemsSource = null;
            PluginDatabase.Refresh(GameContext.Id);
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked);
        }


        private void ToggleButtonPriceNotification_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            GameDlc data = PluginDatabase.GetOnlyCache(GameContext);
            if (data != null)
            {
                data.PriceNotification = (bool)tb.IsChecked;
                PluginDatabase.Update(data);
            }
        }


        #region Filter
        private void PART_TgHide_Click(object sender, RoutedEventArgs e)
        {
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked);
        }

        private void PART_TgFree_Click(object sender, RoutedEventArgs e)
        {
            PART_TgHide.IsChecked = false;
            PART_TgHidden.IsChecked = false;
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked);
        }

        private void PART_TgHidden_Click(object sender, RoutedEventArgs e)
        {
            PART_TgFree.IsChecked = false;
            PART_TgHide.IsChecked = false;
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked);
        }


        private void Filter(bool hiddenOwned, bool onlyFree, bool showHidden)
        {
            PART_Dlcs.ItemsSource = null;

            GameDlc gameDlc = PluginDatabase.Get(GameContext, true);
            if (gameDlc?.Items == null || gameDlc.Count == 0)
            {
                return;
            }

            PART_PriceNotification.IsChecked = gameDlc.PriceNotification;

            IEnumerable<Dlc> query = gameDlc.Items;
            if (hiddenOwned)
            {
                query = query.Where(x => !x.IsOwned);
            }
            if (!showHidden)
            {
                query = query.Where(x => !x.IsHidden);
            }
            else
            {
                query = query.Where(x => x.IsHidden);
            }
            if (onlyFree)
            {
                query = query.Where(x => x.IsFree);
            }

            List<Dlc> data = query.OrderBy(x => x.Name).ToList();

            PART_Dlcs.ItemsSource = data;
            PART_TotalFoundCount.Text = data.Count.ToString();
            PART_TotalOwnedCount.Text = gameDlc.Items.Where(x => x.IsOwned).Count().ToString();
            PART_TotalHiddenCount.Text = gameDlc.Items.Where(x => x.IsHidden).Count().ToString();
            PART_DataDate.Text = new LocalDateTimeConverter().Convert(gameDlc.DateLastRefresh, null, null, CultureInfo.CurrentCulture).ToString();
        }
        #endregion


        private void Part_Ignore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string id = ((Button)sender).Tag.ToString();
                if (PluginDatabase.PluginSettings.Settings.IgnoredList.Contains(id))
                {
                    _ = PluginDatabase.PluginSettings.Settings.IgnoredList.Remove(id);
                }
                else
                {
                    PluginDatabase.PluginSettings.Settings.IgnoredList.Add(id);
                }
                Plugin.SavePluginSettings(PluginDatabase.PluginSettings.Settings);
                Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }

        private void Part_Owned_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string id = ((Button)sender).Tag.ToString();
                if (PluginDatabase.PluginSettings.Settings.ManuallyOwneds.Contains(id))
                {
                    _ = PluginDatabase.PluginSettings.Settings.ManuallyOwneds.Remove(id);
                }
                else
                {
                    PluginDatabase.PluginSettings.Settings.ManuallyOwneds.Add(id);
                }
                Plugin.SavePluginSettings(PluginDatabase.PluginSettings.Settings);
                Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }
    }
}
