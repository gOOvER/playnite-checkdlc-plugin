using CheckDlc.Services;
using CommonPluginsShared;
using Playnite.SDK;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using CheckDlc.Clients;
using CheckDlc.Models;
using System.Collections.Generic;
using CommonPluginsStores.Gog;
using CommonPluginsStores.Gog.Models;
using CommonPluginsStores.Models;
using CommonPluginsStores.Origin;
using CommonPluginsStores.Steam;

namespace CheckDlc.Views
{
    public partial class CheckDlcSettingsView : UserControl
    {
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;


        public CheckDlcSettingsView()
        {
            InitializeComponent();

            SteamPanel.StoreApi = CheckDlc.SteamApi;
            EpicPanel.StoreApi = CheckDlc.EpicApi;
            GogPanel.StoreApi = CheckDlc.GogApi;

            // List features
            PART_FeatureDlc.ItemsSource = API.Instance.Database.Features.OrderBy(x => x.Name);

            // List GOG currencies
            if (CheckDlc.GogApi != null)
            {
                List<StoreCurrency> dataGog = CheckDlc.GogApi.GetCurrencies();
                PART_GogCurrency.ItemsSource = dataGog?.OrderBy(x => x.currency).ToList();

                try
                {
                    int idx = ((List<StoreCurrency>)PART_GogCurrency.ItemsSource)?.FindIndex(x => x.currency == PluginDatabase.PluginSettings.Settings.GogCurrency?.currency) ?? -1;
                    PART_GogCurrency.SelectedIndex = idx;
                }
                catch { }
            }

            // List Origin currencies
            try
            {
                OriginApi originApi = new OriginApi(PluginDatabase.PluginName);
                List<StoreCurrency> dataOrigin = originApi.GetCurrencies();
                PART_OriginCurrency.ItemsSource = dataOrigin?.OrderBy(x => x.currency).ToList();

                int idx = ((List<StoreCurrency>)PART_OriginCurrency.ItemsSource)?.FindIndex(x => x.country == PluginDatabase.PluginSettings.Settings.OriginCurrency?.country) ?? -1;
                PART_OriginCurrency.SelectedIndex = idx;
            }
            catch { }

            SteamPanel.Visibility = (PluginDatabase.PluginSettings.Settings.PluginState.SteamIsEnabled && CheckDlc.SteamApi != null) ? Visibility.Visible : Visibility.Collapsed;
            EpicPanel.Visibility = (PluginDatabase.PluginSettings.Settings.PluginState.EpicIsEnabled && CheckDlc.EpicApi != null) ? Visibility.Visible : Visibility.Collapsed;
            GogPanel.Visibility = (PluginDatabase.PluginSettings.Settings.PluginState.GogIsEnabled && CheckDlc.GogApi != null) ? Visibility.Visible : Visibility.Collapsed;
        }


        #region Tag
        private void ButtonAddTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.AddTagAllGame();
        }

        private void ButtonRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.RemoveTagAllGame();
        }
        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).Tag is string url &&
                Uri.TryCreate(url, UriKind.Absolute, out Uri uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            }
        }


        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(((FrameworkElement)sender).Tag?.ToString(), out int index) &&
                    PART_IgnoredList.ItemsSource is ObservableCollection<string> list &&
                    index >= 0 && index < list.Count)
                {
                    list.RemoveAt(index);
                    PART_IgnoredList.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true);
            }
        }

        private void Button_Click_Remove2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(((FrameworkElement)sender).Tag?.ToString(), out int index) &&
                    PART_ManuallyOwnedList.ItemsSource is ObservableCollection<string> list &&
                    index >= 0 && index < list.Count)
                {
                    list.RemoveAt(index);
                    PART_ManuallyOwnedList.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true);
            }
        }
    }
}