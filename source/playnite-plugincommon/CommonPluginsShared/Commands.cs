using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace CommonPluginsShared
{
    public static class Commands
    {
        [Obsolete("Use CommonPlayniteShared.Commands.NavigateUrlCommand", true)]
        public static RelayCommand<object> NavigateUrl => new RelayCommand<object>((url) =>
        {
            try
            {
                string targetUrl = null;
                if (url is string stringUrl)
                {
                    targetUrl = stringUrl;
                }
                else if (url is Uri uriUrl)
                {
                    targetUrl = uriUrl.AbsoluteUri;
                }

                if (!string.IsNullOrWhiteSpace(targetUrl) &&
                    Uri.TryCreate(targetUrl, UriKind.Absolute, out Uri validatedUri) &&
                    (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = validatedUri.AbsoluteUri,
                        UseShellExecute = true
                    });
                }
                else
                {
                    throw new Exception("Unsupported or unsafe URL format.");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, "Failed to open url.");
            }
        });

        public static RelayCommand<object> RestartRequired => new RelayCommand<object>((sender) =>
        {
            try
            {
                Window WinParent = UI.FindParent<Window>((FrameworkElement)sender);
                if (WinParent.DataContext?.GetType().GetProperty("IsRestartRequired") != null)
                {
                    ((dynamic)WinParent.DataContext).IsRestartRequired = true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        });

        public static RelayCommand<Guid> GoToGame => new RelayCommand<Guid>((id) =>
        {
            API.Instance.MainView.SelectGame(id);
            API.Instance.MainView.SwitchToLibraryView();
        });
    }
}
