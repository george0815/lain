using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class HeaderView : FrameView
    {

        static internal FrameView ASCIIHeader()
        {

            //Header
            var header = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Settings.Current.DisableASCII ? 5 : SettingsData.HeaderHeight,
                CanFocus = false,
                Border = new Border() { BorderStyle = BorderStyle.None }
            };

            // Wrap ASCII art in a FrameView to give it a border
            var logoFrame = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth, // fixed width for logo
                Height = SettingsData.HeaderHeight,
                Border = new Border() { BorderStyle = BorderStyle.Single }
            };

            var logo = new Label()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Text = Settings.Current.icons[0],

                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Settings.Current.LogoColor, Settings.Current.BackgroundColor), // text color, background color
                    Focus = Application.Driver.MakeAttribute(Settings.Current.LogoColor, Settings.Current.BackgroundColor)
                }
            };

            if (!Settings.Current.DisableASCII)
            {
                logoFrame.Add(logo);
                header.Add(logoFrame);
            }


            #region EXTRA HEADER INFO

            // Date
            var date = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = 1,
                Text = DateTime.Now.ToString("yyyy-MM-dd")
            };

            // Active torrents count
            var torrentCount = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = 3,
                Text = $"Active Torrents: {TorrentOperations.Managers!.Count}"
            };

            // Active torrents preview
            StringBuilder sb = new StringBuilder("", 30);
            int count = 0;

            if (TorrentOperations.Managers.Count() != 0)
            {
                for (int i = 0; i <= TorrentOperations.Managers.Count(); i++)
                {

                    if (TorrentOperations.Managers[i].State == TorrentState.Seeding || TorrentOperations.Managers[i].State == TorrentState.Downloading) { sb.Append(TorrentOperations.Managers[i].Torrent?.Name); }
                    if (count == 3) { sb.Append("..."); break; }
                    sb.Append("\n");

                }
            }
            else { sb.Append("No active torrents"); }

            var torrentPreview = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = 5,
                Text = sb.ToString()
            };

            // Port
            var portDisplay = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = 7,
                Text = $"Operating on port: {Settings.Current.Port}"
            };


            #region HOTKEY INFO


            header.Add(new Label($"Start: {Settings.Current.Controls.StartDownload}")
            {
                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.Green, Settings.Current.BackgroundColor), // text color, background color
                },
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30,
                Y = 1

            });
            header.Add(new Label($"Stop: {Settings.Current.Controls.StopDownload}")
            {
                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.Red, Settings.Current.BackgroundColor), // text color, background color
                },
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30,
                Y = 3
            });
            header.Add(new Label($"Start seeding: {Settings.Current.Controls.StartSeeding}")
            {
                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.BrightYellow, Settings.Current.BackgroundColor), // text color, background color
                },
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30,
                Y = 5
            });
            header.Add(new Label($"Stop seeding: {Settings.Current.Controls.StopSeeding}")
            {
                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.Blue, Settings.Current.BackgroundColor), // text color, background color
                },
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30,
                Y = 7
            });
            header.Add(new Label($"Delete: {Settings.Current.Controls.RemoveTorrent}")
            {
                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.Magenta, Settings.Current.BackgroundColor), // text color, background color
                },
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30,
                Y = 9
            });




            #endregion


            header.Add(date, torrentCount, torrentPreview, portDisplay);

            return header;

            #endregion
        }



    }
}
