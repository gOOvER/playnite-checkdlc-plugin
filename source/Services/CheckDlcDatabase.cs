using CheckDlc.Clients;
using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using static CommonPluginsShared.PlayniteTools;
using System.Threading;

namespace CheckDlc.Services
{
    public class CheckDlcDatabase : PluginDatabaseObject<CheckDlcSettingsViewModel, CheckDlcCollection, GameDlc, Dlc>
    {
        public bool SettingsOpen { get; set; } = false;


        public CheckDlcDatabase(CheckDlcSettingsViewModel PluginSettings, string PluginUserDataPath) : base(PluginSettings, "CheckDlc", PluginUserDataPath)
        {
            TagBefore = "[DLC]";
        }


        protected override bool LoadDatabase()
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                Database = new CheckDlcCollection(Paths.PluginDatabasePath);
                Database.SetGameInfo<Dlc>();

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                Logger.Info($"LoadDatabase with {Database.Count} items - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}");
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
                return false;
            }

            return true;
        }


        public override GameDlc Get(Guid id, bool onlyCache = false, bool force = false)
        {
            GameDlc gameDlc = base.GetOnlyCache(id);

            // Get from web
            if ((gameDlc == null && !onlyCache) || force)
            {
                gameDlc = GetWeb(id);
                AddOrUpdate(gameDlc);
            }

            if (gameDlc == null)
            {
                Game game = API.Instance.Database.Games.Get(id);
                if (game != null)
                {
                    gameDlc = GetDefault(game);
                    AddOrUpdate(gameDlc);
                }
            }

            return gameDlc;
        }

        public override GameDlc GetWeb(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            GameDlc gameDlc = GetDefault(game);
            try
            {
                //Thread.Sleep(100);
                List<Dlc> dlcs = new List<Dlc>();
                ExternalPlugin pluginType = PlayniteTools.GetPluginType(game.PluginId);
                switch (pluginType)
                {
                    case ExternalPlugin.SteamLibrary:
                        if (PluginSettings.Settings.PluginState.SteamIsEnabled)
                        {
                            SteamDlc steamDlc = new SteamDlc();
                            dlcs = steamDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.GogLibrary:
                    case ExternalPlugin.GogOssLibrary:
                        if (PluginSettings.Settings.PluginState.GogIsEnabled)
                        {
                            GogDlc gogDlc = new GogDlc();
                            dlcs = gogDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.EpicLibrary:
                    case ExternalPlugin.LegendaryLibrary:
                        if (PluginSettings.Settings.PluginState.EpicIsEnabled)
                        {
                            EpicDlc epicDlc = new EpicDlc();
                            dlcs = epicDlc.GetGameDlc(game);
                        }
                        break;


                    case ExternalPlugin.OriginLibrary:
                        if (PluginSettings.Settings.PluginState.OriginIsEnabled)
                        {
                            OriginDlc originDlc = new OriginDlc();
                            dlcs = originDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.PSNLibrary:
                        if (PluginSettings.Settings.PluginState.PsnIsEnabled)
                        {
                            PsnDlc psnDlc = new PsnDlc();
                            dlcs = psnDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.NintendoLibrary:
                        if (PluginSettings.Settings.PluginState.NintendosEnabled)
                        {
                            NintendoDlc nintendoDlc = new NintendoDlc();
                            dlcs = nintendoDlc.GetGameDlc(game);
                        }
                        break;

                    case ExternalPlugin.None:
                    case ExternalPlugin.BattleNetLibrary:
                    case ExternalPlugin.XboxLibrary:
                    case ExternalPlugin.IndiegalaLibrary:
                    case ExternalPlugin.AmazonGamesLibrary:
                    case ExternalPlugin.BethesdaLibrary:
                    case ExternalPlugin.HumbleLibrary:
                    case ExternalPlugin.ItchioLibrary:
                    case ExternalPlugin.RockstarLibrary:
                    case ExternalPlugin.TwitchLibrary:
                    case ExternalPlugin.OculusLibrary:
                    case ExternalPlugin.RiotLibrary:
                    case ExternalPlugin.UplayLibrary:
                    case ExternalPlugin.SuccessStory:
                    case ExternalPlugin.CheckDlc:
                    case ExternalPlugin.EmuLibrary:

                    default:
                        break;
                }

                gameDlc.Items = dlcs;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return gameDlc;
        }

        private GameDlc GetManual(Guid id, uint appId)
        {
            Game game = API.Instance.Database.Games.Get(id);
            GameDlc gameDlc = GetDefault(game);

            try
            {
                SteamDlc steamDlc = new SteamDlc();
                gameDlc.IsManual = true;
                gameDlc.AppId = appId;
                gameDlc.Items = steamDlc.GetGameDlc(appId);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
            }

            return gameDlc;
        }


        public override void SetThemesResources(Game game)
        {
            GameDlc gameDlc = Get(game, true);
            PluginSettings.Settings.HasData = gameDlc?.HasData ?? false;
            PluginSettings.Settings.ListDlcs = new List<Dlc>();

            if (PluginSettings.Settings.HasData)
            {
                PluginSettings.Settings.ListDlcs = gameDlc.Items;
            }
        }

        public override void Refresh(IEnumerable<Guid> ids, string message)
        {
            if (ids == null)
            {
                return;
            }

            // Filter out unsupported libraries upfront unless manually configured
            List<Guid> validIds = ids.Where(id =>
            {
                Game g = API.Instance.Database.Games.Get(id);
                if (g == null)
                {
                    return false;
                }
                if (CheckDlc.SupportedLibrary.Contains(g.PluginId))
                {
                    return true;
                }
                GameDlc loaded = Get(id, true);
                return loaded?.IsManual == true;
            }).ToList();

            if (validIds.Count == 0)
            {
                Logger.Info("Refresh: no supported games to refresh.");
                return;
            }

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"{PluginName} - {message}")
            {
                Cancelable = true,
                IsIndeterminate = validIds.Count == 1
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                API.Instance.Database.BeginBufferUpdate();
                Database.BeginBufferUpdate();

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                a.ProgressMaxValue = validIds.Count;

                foreach (Guid id in validIds)
                {
                    Game game = API.Instance.Database.Games.Get(id);
                    a.Text = $"{PluginName} - {message}"
                        + (validIds.Count == 1 ? string.Empty : "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}")
                        + "\n" + game?.Name + (game?.Source == null ? string.Empty : $" ({game?.Source.Name})");

                    if (a.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        RefreshNoLoader(id);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, true, PluginName);
                    }

                    a.CurrentProgressValue++;
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                Logger.Info($"Task Refresh(){(a.CancelToken.IsCancellationRequested ? " canceled" : string.Empty)} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{validIds.Count} items");

                Database.EndBufferUpdate();
                API.Instance.Database.EndBufferUpdate();
            }, globalProgressOptions);
        }

        public override void RefreshNoLoader(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            Logger.Info($"RefreshNoLoader({game?.Name} - {game?.Id})");

            if (game == null)
            {
                return;
            }

            GameDlc loadedItem = Get(id, true);
            if (loadedItem == null)
            {
                return;
            }

            if (CheckDlc.SupportedLibrary.Contains(game.PluginId) && !loadedItem.IsManual)
            {
                GameDlc webItem = GetWeb(id);
                if (webItem != null)
                {
                    webItem.PriceNotification = loadedItem.PriceNotification;
                    if (!ReferenceEquals(loadedItem, webItem))
                    {
                        Update(webItem);
                    }
                    ActionAfterRefresh(webItem);
                }
                else
                {
                    ActionAfterRefresh(loadedItem);
                }
            }
            else if (loadedItem.IsManual)
            {
                GameDlc webItem = GetManual(id, loadedItem.AppId);
                if (webItem != null)
                {
                    if (!ReferenceEquals(loadedItem, webItem))
                    {
                        Update(webItem);
                    }
                    ActionAfterRefresh(webItem);
                }
                else
                {
                    ActionAfterRefresh(loadedItem);
                }
            }
            else
            {
                Logger.Warn($"The plugin does not support the library {PlayniteTools.GetSourceByPluginId(game.PluginId)}");
            }
        }

        public override void ActionAfterRefresh(GameDlc item)
        {
            Game game = API.Instance.Database.Games.Get(item.Id);
            if ((item?.HasData ?? false) && PluginSettings.Settings.DlcFeature != null)
            {
                if (game.FeatureIds != null)
                {
                    _ = game.FeatureIds.AddMissing(PluginSettings.Settings.DlcFeature.Id);
                }
                else
                {
                    game.FeatureIds = new List<Guid> { PluginSettings.Settings.DlcFeature.Id };
                }
                API.Instance.Database.Games.Update(game);
            }
            else
            {
                if (PluginSettings.Settings.DlcFeature?.Id != null && game.FeatureIds?.Find(x => x == PluginSettings.Settings.DlcFeature?.Id) != null)
                {
                    _ = game.FeatureIds.Remove(PluginSettings.Settings.DlcFeature.Id);
                    API.Instance.Database.Games.Update(game);
                }
            }
        }


        public override void AddTag(Game game)
        {
            GameDlc item = Get(game, true);
            if (item.HasData)
            {
                try
                {
                    Guid? TagId = FindGoodPluginTags(string.Empty);
                    if (TagId != null)
                    {
                        if (game.TagIds == null)
                        {
                            game.TagIds = new List<Guid>();
                        }
                        if (!game.TagIds.Contains((Guid)TagId))
                        {
                            game.TagIds.Add((Guid)TagId);
                        }
                    }

                    if (PluginSettings.Settings.EnableTagAllDlc && item.HasAllDlc)
                    {
                        TagId = FindGoodPluginTags("100%");
                        if (TagId != null)
                        {
                            if (game.TagIds == null)
                            {
                                game.TagIds = new List<Guid>();
                            }
                            if (!game.TagIds.Contains((Guid)TagId))
                            {
                                game.TagIds.Add((Guid)TagId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Tag insert error with {game.Name}", true, PluginName, string.Format(ResourceProvider.GetString("LOCCommonNotificationTagError"), game.Name));
                    return;
                }
            }
            else if (TagMissing)
            {
                Guid noDataTag = (Guid)AddNoDataTag();
                if (game.TagIds == null)
                {
                    game.TagIds = new List<Guid>();
                }
                if (!game.TagIds.Contains(noDataTag))
                {
                    game.TagIds.Add(noDataTag);
                }
            }

            API.Instance.MainView.UIDispatcher?.Invoke(() =>
            {
                API.Instance.Database.Games.Update(game);
                game.OnPropertyChanged();
            });
        }


        private static string EscapeCsv(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return string.Empty;
            }

            // CSV/Excel formula injection prevention: if first character is =, +, -, @, prefix with '
            if (val.Length > 0 && (val[0] == '=' || val[0] == '+' || val[0] == '-' || val[0] == '@'))
            {
                val = "'" + val;
            }

            return val.Replace("\"", "\"\"");
        }

        internal override string GetCsvData(GlobalProgressActionArgs a, bool minimum)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("\"Game name\";\"Platform\";\"Dlc name\";\"Price\";\"Is owned\";\"Is owned manually\";\"Is hidden\";\"Dlc link\";\"Is manual added\";");

            if (Database.Items != null)
            {
                foreach (var x in Database.Items)
                {
                    if (x.Value?.Items == null)
                    {
                        continue;
                    }

                    foreach (var y in x.Value.Items)
                    {
                        if (a.CancelToken.IsCancellationRequested)
                        {
                            return sb.ToString();
                        }

                        a.Text = $"{PluginName} - {ResourceProvider.GetString("LOCCommonExtracting")}"
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + x.Value.Game?.Name + (x.Value.Game?.Source == null ? string.Empty : $" ({x.Value.Game?.Source.Name})");

                        sb.AppendLine();
                        string gameName = EscapeCsv(x.Value.Name);
                        string platform = EscapeCsv(x.Value.Source?.Name ?? x.Value.Platforms?.FirstOrDefault()?.Name ?? "Playnite");
                        string dlcName = EscapeCsv(y.Name);
                        string price = EscapeCsv(y.Price);
                        string isOwned = y.IsOwned ? "X" : string.Empty;
                        string isManualOwned = y.IsManualOwned ? "X" : string.Empty;
                        string isHidden = y.IsHidden ? "X" : string.Empty;
                        string link = EscapeCsv(y.Link);
                        string isManual = x.Value.IsManual ? "X" : string.Empty;

                        sb.Append($"\"{gameName}\";\"{platform}\";\"{dlcName}\";\"{price}\";\"{isOwned}\";\"{isManualOwned}\";\"{isHidden}\";\"{link}\";\"{isManual}\";");

                        a.CurrentProgressValue++;
                    }
                }
            }

            return sb.ToString();
        }
    }
}
