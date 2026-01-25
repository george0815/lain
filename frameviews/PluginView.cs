using lain.helpers;
using System;
using System.Drawing;
using System.Net.NetworkInformation;
using Terminal.Gui;

namespace lain.frameviews
{
    /// <summary>
    /// Configuration view for Ghidorah-based search plugins.
    ///
    /// This view exposes search-related settings such as:
    /// - Timeouts and result limits
    /// - Categories and preferred sources
    /// - Sorting criteria
    /// - Optional qBittorrent plugin integration
    ///
    /// All changes are applied to Settings.Current and persisted explicitly
    /// via the Save button.
    /// </summary>
    internal class PluginView : FrameView
    {
        public PluginView()
            : base(Resources.Ghidorah)
        {
            // Position and size the frame consistently with other views.
            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();

            // ScrollView allows the settings list to grow vertically
            // without breaking layout on smaller terminals.
            var scroll = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ShowVerticalScrollIndicator = true,
                ShowHorizontalScrollIndicator = false,
            };

            Add(scroll);

            // Vertical layout cursor inside the scroll view.
            int y = 1;

            #region SEARCH ENGINE SETTINGS

            // -------------------
            // Ghidorah search configuration
            // -------------------

            // Search timeout (displayed in seconds, stored internally in ms).
            scroll.Add(new Label(Resources.Searchtimeout) { X = 1, Y = y });
            var timeOutBtn = new TextField((Settings.Current.Timeout / 1000).ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 34),
                Y = y,
                Width = 10
            };
            scroll.Add(timeOutBtn);
            y += 2;

            // Total search results limit across all sources.
            scroll.Add(new Label(Resources.Searchresultslimit) { X = 1, Y = y });
            var torLimit = new TextField(Settings.Current.SearchResultsLimit.ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 34),
                Y = y,
                Width = 10
            };
            scroll.Add(torLimit);
            y += 2;

            // Per-source search results limit.
            scroll.Add(new Label(Resources.Searchresultslimitpersource) { X = 1, Y = y });
            var torLimitPerSource = new TextField(Settings.Current.SearchResultsLimitPerSource.ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 34),
                Y = y,
                Width = 10
            };
            scroll.Add(torLimitPerSource);
            y += 2;

            // Categories selector (compact button with dialog-based picker).
            scroll.Add(new Label(Resources.Categories) { X = 1, Y = y });
            var categories = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,

                // Display only the first category to keep UI compact.
                Text = Settings.Current.Categories.Length > 1
                    ? Settings.Current.Categories[0] + "..."
                    : Settings.Current.Categories[0]
            };

            categories.Clicked += () =>
            {
                // Open category picker dialog.
                string[] tmp = DialogHelpers.PickCategories([
                    Resources.Movies,
                    Resources.Games,
                    Resources.TVshows,
                    Resources.Applications,
                    Resources.Other
                ]);

                // Apply selection or fall back to defaults if nothing selected.
                if (tmp.Length > 0)
                {
                    Settings.Current.Categories = tmp;
                    categories.Text = tmp.Length == 1 ? tmp[0] : tmp[0] + "...";
                }
                else
                {
                    Settings.Current.Categories = [
                        Resources.Movies,
                        Resources.Games,
                        Resources.TVshows,
                        Resources.Applications,
                        Resources.Other
                    ];
                    categories.Text = $"{Resources.Movies}...";
                }

                // Force redraw of the button and its container.
                categories.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(categories);
            y += 2;

            // Preferred search sources selector.
            scroll.Add(new Label(Resources.Sources) { X = 1, Y = y });
            var sources = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,

                // Compact representation of selected sources.
                Text = Settings.Current.SearchSources.Length > 1
                    ? Settings.Current.SearchSources[0] + "..."
                    : Settings.Current.SearchSources[0]
            };

            sources.Clicked += () =>
            {
                // Choose between qBittorrent plugins or default sources.
                string[] tmp = DialogHelpers.PickSources(
                    Settings.Current.UseQbittorrentPlugins
                        ? Ghidorah.QbSources
                        : Settings.Current.DefaultSources
                );

                if (tmp.Length > 0)
                {
                    Settings.Current.SearchSources = tmp;
                    sources.Text = tmp.Length == 1 ? tmp[0] : tmp[0] + "...";
                }
                else
                {
                    Settings.Current.SearchSources =
                        Settings.Current.UseQbittorrentPlugins
                            ? Ghidorah.QbSources
                            : Settings.Current.DefaultSources;

                    sources.Text = $"{Settings.Current.SearchSources[0]}...";
                }

                sources.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(sources);
            y += 2;

            // Sorting criteria selector.
            scroll.Add(new Label(Resources.Sortby) { X = 1, Y = y });
            var sortBy = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,
                Text = Settings.Current.SortBy
            };

            sortBy.Clicked += () =>
            {
                // Open sorting criteria picker.
                string tmp = DialogHelpers.PickSortCriteria([
                    Resources.Name,
                    Resources.Source,
                    Resources.Size,
                    Resources.Seeders
                ]);

                // Apply selected value or fall back to default.
                Settings.Current.SortBy = tmp;
                sortBy.Text = String.IsNullOrEmpty(tmp) ? Resources.Source : tmp;

                if (String.IsNullOrEmpty(tmp))
                    Settings.Current.SortBy = Resources.Source;

                sortBy.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(sortBy);
            y += 2;

            // Toggle for using qBittorrent search plugins.
            var qbCheckbox = new CheckBox(Resources.Useqbittorrentplugins)
            {
                X = 1,
                Y = y,
                Checked = Settings.Current.UseQbittorrentPlugins
            };
            scroll.Add(qbCheckbox);
            y += 2;

            // Button to check plugin availability/status.
            var checkStatusBtn = new Button(Resources.Checkstatus) { X = 1, Y = y };
            scroll.Add(checkStatusBtn);
            y += 2;

            #endregion

            // Persist settings button.
            var saveBtn = new Button(Resources.Save) { X = 1, Y = y };
            scroll.Add(saveBtn);

            // Inform ScrollView of total virtual content size.
            scroll.ContentSize = new Terminal.Gui.Size(
                Application.Top.Frame.Width - 2,
                y + 2
            );

            #region EVENT HANDLERS

            // -------------------
            // Settings logic and validation
            // -------------------

            qbCheckbox.Toggled += (e) =>
            {
                try
                {
                    // Toggle logic includes validation of available qB plugins.
                    if (!e)
                    {
                        if (Ghidorah.QbSources == null || Ghidorah.QbSources.Length == 0)
                        {
                            MessageBox.ErrorQuery(
                                Resources.Error,
                                Resources.Noqbittorrentpluginsfound,
                                Resources.OK
                            );

                            qbCheckbox.Checked = false;
                            Settings.Current.UseQbittorrentPlugins = false;
                        }
                        else
                        {
                            MessageBox.Query(
                                Resources.Ghidorah,
                                Resources.Importantifusing,
                                Resources.OK
                            );

                            Settings.Current.UseQbittorrentPlugins = true;
                        }
                    }
                    else
                    {
                        Settings.Current.UseQbittorrentPlugins = false;
                    }

                    // Immediately update dependent UI state.
                    Application.MainLoop.Invoke(() =>
                    {
                        Settings.Current.SearchSources =
                            Settings.Current.UseQbittorrentPlugins
                                ? Ghidorah.QbSources!
                                : Settings.Current.DefaultSources!;

                        sources.Text =
                            Settings.Current.UseQbittorrentPlugins
                                ? $"{Ghidorah.QbSources![0]}..."
                                : $"{Settings.Current.DefaultSources[0]}...";

                        sources.SetNeedsDisplay();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery(Resources.FatalError, ex.Message, Resources.OK);
                }
            };

            checkStatusBtn.Clicked += () =>
            {
                // Immediately reflect pending state in UI.
                Application.MainLoop.Invoke(() =>
                {
                    checkStatusBtn.Text = $"{Resources.Checkingstatus}...";
                    checkStatusBtn.SetNeedsDisplay();
                });

                Task.Run(() =>
                {
                    try
                    {
                        var result = Ghidorah.CheckStatusPlugins(true);

                        Application.MainLoop.Invoke(() =>
                        {
                            MessageBox.Query(Resources.Ghidorah, result, Resources.OK);
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            MessageBox.ErrorQuery(Resources.FatalError, ex.Message, Resources.OK);
                        });
                    }
                    finally
                    {
                        // Restore button state regardless of outcome.
                        Application.MainLoop.Invoke(() =>
                        {
                            checkStatusBtn.Text = Resources.Checkstatus;
                            checkStatusBtn.SetNeedsDisplay();
                        });
                    }
                });
            };

            saveBtn.Clicked += () =>
            {
                try
                {
                    // Validate numeric inputs before persisting.
                    if (!int.TryParse(torLimit.Text.ToString(), out var torLim) || torLim > 100 || torLim < 0)
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidtorlimit, Resources.OK);
                        return;
                    }

                    if (!int.TryParse(torLimitPerSource.Text.ToString(), out var torLimPerSource)
                        || torLimPerSource > 100
                        || torLimPerSource < 0)
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidtorlimitpersource, Resources.OK);
                        return;
                    }

                    if (!int.TryParse(timeOutBtn.Text.ToString(), out var timeOut)
                        || timeOut > 1000
                        || timeOut < 0)
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidtimeout, Resources.OK);
                        return;
                    }

                    if (qbCheckbox.Checked &&
                        (Ghidorah.QbSources == null || Ghidorah.QbSources.Length == 0))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Noqbittorrentpluginsfound, Resources.OK);
                        return;
                    }

                    // Apply validated settings.
                    Settings.Current.SearchResultsLimit = torLim;
                    Settings.Current.SearchResultsLimitPerSource = torLimPerSource;
                    Settings.Current.Timeout = timeOut * 1000;
                    Settings.Current.UseQbittorrentPlugins = qbCheckbox.Checked;

                    Settings.Save();

                    MessageBox.Query(
                        Resources.Settings,
                        Resources.Settingssavedsuccessfully,
                        Resources.OK
                    );

                    Log.Write(Resources.Settingssaved);
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery(Resources.FatalError, ex.Message, Resources.OK);
                }
            };

            #endregion
        }
    }
}
