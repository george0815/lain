using System;
using System.Drawing;
using Terminal.Gui;
using lain.helpers;

namespace lain.frameviews
{
    internal class PluginView : FrameView
    {




        public PluginView()
            : base(Resources.Ghidorah)
        {
            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();

            // Create a scroll view
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

            int y = 1; // starting Y position inside scroll view

        

            #region SEARCH ENGINE SETTINGS

            //Search results total limit
            scroll.Add(new Label(Resources.Searchtimeout) { X = 1, Y = y });
            var timeOutBtn = new TextField((Settings.Current.Timeout / 1000).ToString()) { X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 34), Y = y, Width = 10 };
            scroll.Add(timeOutBtn);
            y += 2;

            //Search results total limit
            scroll.Add(new Label(Resources.Searchresultslimit) { X = 1, Y = y });
            var torLimit = new TextField(Settings.Current.SearchResultsLimit.ToString()) { X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 34), Y = y, Width = 10 };
            scroll.Add(torLimit);
            y += 2;

            //Search results limit per source
            scroll.Add(new Label(Resources.Searchresultslimitpersource) { X = 1, Y = y });
            var torLimitPerSource = new TextField(Settings.Current.SearchResultsLimitPerSource.ToString()) { X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 34), Y = y, Width = 10 };
            scroll.Add(torLimitPerSource);
            y += 2;

            //categories to search
            scroll.Add(new Label(Resources.Categories) { X = 1, Y = y });
            var categories = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,

                Height = 1,
                Text = Settings.Current.Categories.Length > 1 ? Settings.Current.Categories[0] + "..." : Settings.Current.Categories[0]
            };


            categories.Clicked += () =>
            {
                // Open pick categories dialog
                string[] tmp = DialogHelpers.PickCategories([

                    Resources.Movies, Resources.Games,
                     Resources.TVshows, Resources.Applications,
                     Resources.Other


                ]);

                if (tmp.Length > 0)
                {
                    Settings.Current.Categories = tmp;
                    if (tmp.Length == 1)
                    {
                        categories.Text = tmp[0];
                    }
                    else if (tmp.Length > 1)
                    {
                        categories.Text = tmp[0] + "...";
                    }
                }


                else
                {
                    Settings.Current.Categories = [Resources.Movies, Resources.Games,
                     Resources.TVshows, Resources.Applications,
                     Resources.Other ];
                    categories.Text = $"{Resources.Movies}...";
                }

                // Force redraw the button *and its parent view*
                categories.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };
            scroll.Add(categories);
            y += 2;

            //Preferred search sources
            scroll.Add(new Label(Resources.Sources) { X = 1, Y = y });
            var sources = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,

                Height = 1,
                Text = Settings.Current.SearchSources.Length > 1 ? Settings.Current.SearchSources[0] + "..." : Settings.Current.SearchSources[0]
            };


            sources.Clicked += () =>
            {
                // Open your color picker
                string[] tmp = DialogHelpers.PickSources([
                    "kickasstorrents",
                        "thepiratebay",
                        "limetorrents",
                        "yts",
                        "x1337",
                        "torrentgalaxy"]);

                if (tmp.Length > 0)
                {
                    Settings.Current.SearchSources = tmp;
                    if (tmp.Length == 1)
                    {
                        sources.Text = tmp[0];
                    }
                    else if (tmp.Length > 1)
                    {
                        sources.Text = tmp[0] + "...";
                    }
                }
                else
                {
                    Settings.Current.SearchSources = [
                        "kickasstorrents",
                        "thepiratebay",
                        "limetorrents",
                        "yts",
                        "x1337",
                        "torrentgalaxy"];
                    sources.Text = "kickasstorrents...";
                }

                sources.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };
            scroll.Add(sources);
            y += 2;



            //sort by option
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
                // Open your color picker
                string tmp = DialogHelpers.PickSortCriteria([Resources.Name, Resources.Source, Resources.Size, Resources.Seeders]);
                Settings.Current.SortBy = tmp;

                // Update *the button's* text, not `Text = ...`
                sortBy.Text = tmp;

                if (String.IsNullOrEmpty(tmp))
                {
                    sortBy.Text = Resources.Source;
                    Settings.Current.SortBy = Resources.Source;
                }

                // Force redraw the button *and its parent view*
                sortBy.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };
            scroll.Add(sortBy);
            y += 2;


            //Use  qbittorent plugins
            var qbCheckbox = new CheckBox(Resources.Useqbittorrentplugins)
            {
                X = 1,
                Y = y,
                Checked = Settings.Current.UseQbittorrentPlugins
            };
            scroll.Add(qbCheckbox);
            y += 2;


            //check status 
            var checkStatusBtn = new Button(Resources.Checkstatus) { X = 1, Y = y };
            scroll.Add(checkStatusBtn);

            y += 2;

            #endregion

            // Save button
            var saveBtn = new Button(Resources.Save) { X = 1, Y = y };
            scroll.Add(saveBtn);


            // Set the content size so scroll bars work
            scroll.ContentSize = new Terminal.Gui.Size(Application.Top.Frame.Width - 2, y + 2);

            #region EVENT HANDLERS

            qbCheckbox.Toggled += (e) =>
            {
                try
                {
                    if (!e) { MessageBox.Query(Resources.Ghidorah, Resources.Importantifusing, Resources.OK); }
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery(Resources.FatalError, ex.Message, Resources.OK);
                }
            };  

            checkStatusBtn.Clicked += () =>
            {
                try
                {
                    
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery(Resources.FatalError, ex.Message, Resources.OK);
                }
            };


            saveBtn.Clicked += () =>
            {
                try
                {
                    
                    if (!int.TryParse(torLimit.Text.ToString(), out var torLim) || torLim > 100 || torLim < 0)
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidtorlimit, Resources.OK);
                        return;
                    }


                    if (!int.TryParse(torLimitPerSource.Text.ToString(), out var torLimPerSource) || torLimPerSource > 100 || torLimPerSource < 0)
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidtorlimitpersource, Resources.OK);
                        return;
                    }

                    if (!int.TryParse(timeOutBtn.Text.ToString(), out var timeOut) || timeOut > 1000 || timeOut < 0)
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidtimeout, Resources.OK);
                        return;
                    }



                    // Apply settings
                  
                    Settings.Current.SearchResultsLimit = torLim;
                    Settings.Current.SearchResultsLimitPerSource = torLimPerSource;
                    Settings.Current.Timeout = timeOut * 1000;



                    Settings.Save();

                    MessageBox.Query(Resources.Settings, Resources.Settingssavedsuccessfully, Resources.OK);
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