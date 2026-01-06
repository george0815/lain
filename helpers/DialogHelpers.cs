using Terminal.Gui;
using System;
using System.IO;
using lain;
using lain.frameviews;

public static class DialogHelpers
{

    // Allows multiple selection of categories
    public static string[] PickCategories(string[] categories)
    {
        var dlg = new Dialog(Resources.Choosecategories, 60, 12);
        var selected = new HashSet<string>();

        int y = 0;
        foreach (var cat in categories)
        {
            var btn = new Button($" [ ] {cat} ")
            {
                X = 1,
                Y = y++
            };

            btn.Clicked += () =>
            {
                if (selected.Contains(cat))
                {
                    selected.Remove(cat);
                    btn.Text = $" [ ] {cat} ";
                }
                else
                {
                    selected.Add(cat);
                    btn.Text = $" [x] {cat} ";
                }
            };

            dlg.Add(btn);
        }

        var okBtn = new Button(Resources.OK)
        {
            IsDefault = true
        };
        okBtn.Clicked += () => Application.RequestStop();

        var cancelBtn = new Button(Resources.Cancel);
        cancelBtn.Clicked += () =>
        {
            selected.Clear();
            Application.RequestStop();
        };

        dlg.AddButton(okBtn);
        dlg.AddButton(cancelBtn);


        Application.Run(dlg);
        return selected.ToArray();
    }


    // Allows multiple selection of sources
    public static string[] PickSources(string[] sources)
    {
        var dlg = new Dialog(Resources.Choosesources, 60, 12);
        var selected = new HashSet<string>();

        int y = 0;
        foreach (var src in sources)
        {
            var btn = new Button($" [ ] {src} ")
            {
                X = 1,
                Y = y++
            };

            btn.Clicked += () =>
            {
                if (selected.Contains(src))
                {
                    selected.Remove(src);
                    btn.Text = $" [ ] {src} ";
                }
                else
                {
                    selected.Add(src);
                    btn.Text = $" [x] {src} ";
                }
            };

            dlg.Add(btn);
        }

        var okBtn = new Button(Resources.OK)
        {
            IsDefault = true
        };
        okBtn.Clicked += () => Application.RequestStop();

        var cancelBtn = new Button(Resources.Cancel);
        cancelBtn.Clicked += () =>
        {
            selected.Clear();
            Application.RequestStop();
        };

        dlg.AddButton(okBtn);
        dlg.AddButton(cancelBtn);


        Application.Run(dlg);
        return selected.ToArray();
    }

    // Allows single selection of sort criteria
    public static string PickSortCriteria(string[] criteria)
    {
        var dlg = new Dialog("Sort by: ", 40, 10);
        string result = criteria.FirstOrDefault() ?? "";

        int y = 0;
        Button? active = null;

        foreach (var c in criteria)
        {
            var btn = new Button($" ( ) {c} ")
            {
                X = 1,
                Y = y++
            };

            btn.Clicked += () =>
            {
                if (active != null)
                    active.Text = active.Text.Replace("(x)", "( )");

                btn.Text = $" (x) {c} ";
                active = btn;
                result = c;
            };

            dlg.Add(btn);

            if (active == null)
            {
                active = btn;
                btn.Text = $" (x) {c} ";
                result = c;
            }
        }

        var okBtn = new Button(Resources.OK)
        {
            IsDefault = true
        };
        okBtn.Clicked += () => Application.RequestStop();

        var cancelBtn = new Button(Resources.Cancel);
        cancelBtn.Clicked += () =>
        {
            result = "";
            Application.RequestStop();
        };

        dlg.AddButton(okBtn);
        dlg.AddButton(cancelBtn);


        Application.Run(dlg);
        return result;
    }


    // Allows single color selection from grid
    public static string PickColorGrid()
    {
        var colors = SettingsView.colors;
        var dlg = new Dialog(Resources.ChooseColor, 50, 8);

        dlg.ColorScheme = new ColorScheme()
        {
            Normal = Application.Driver.MakeAttribute(Settings.Current.TextColor, Color.Black), // text, background
            Focus = Application.Driver.MakeAttribute(Settings.Current.FocusTextColor, Color.Black), // focused element
            HotNormal = Application.Driver.MakeAttribute(Settings.Current.HotTextColor, Color.Black), //hotkey text, background
            HotFocus = Application.Driver.MakeAttribute(Settings.Current.FocusTextColor, Color.Black), // focused hotkey text, background
        };

        string result = Resources.Black;

        int x = 0, y = 0;
        foreach (var c in colors)
        {


            var textColor = c.Value;
            if (c.Value == Color.Black) { 
                textColor = Color.White; 
            }

            var box = new Button($" {c.Key} ")
            {
                X = x * 15,
                Y = y,
                ColorScheme = new ColorScheme()
                {
                    Normal = new Terminal.Gui.Attribute(textColor, Color.Black),
                    Focus = Application.Driver.MakeAttribute(textColor, Color.Black), // focused element
                    HotNormal = Application.Driver.MakeAttribute(textColor, Color.Black), //hotkey text, background
                    HotFocus = Application.Driver.MakeAttribute(textColor, Color.Black), // focused hotkey text
                }
            };

            box.Clicked += () =>
            {
                result = c.Key;
                Application.RequestStop();
            };

            dlg.Add(box);

            x++;
            if (x == 3) { x = 0; y++; }
        }

        Application.Run(dlg);
        return result;
    }

    // Save file dialog with validation 
    public static string? ShowSaveFileDialog(string title, string message, string[] allowedExtensions,
                                             string defaultFileName = "")
    {
        var dialog = new SaveDialog(title, message)
        {
            CanCreateDirectories = true,
            AllowedFileTypes = allowedExtensions,
            FilePath = defaultFileName
        };

        Application.Run(dialog);

        // User pressed ESC/cancel
        if (dialog.Canceled || string.IsNullOrWhiteSpace(dialog.FilePath?.ToString()))
            return null;

        string path = dialog.FilePath!.ToString();

        try
        {
            // Validate directory exists or can be created
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(dir))
                throw new Exception(Resources.Invalidfile_folderpath);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir); 

            return path;
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery(40, 10, Resources.Error, ex.Message, Resources.OK);
            return null;
        }
    }

    // Open file dialog with validation
    public static string? ShowFileDialog(string title, string message, string[] allowedExtensions, bool create)
    {
        var dialog = new OpenDialog(title, message)
        {
            CanChooseFiles = true,
            CanChooseDirectories = false,
            AllowedFileTypes = allowedExtensions,
            AllowsMultipleSelection = false
        };

        Application.Run(dialog);

        if (!dialog.Canceled && dialog.FilePaths.Count > 0)
        {
            var path = dialog.FilePath.ToString();

            if (!create)
            {

                if (File.Exists(path)) { return path; }
                else{MessageBox.ErrorQuery(Resources.InvalidFile, Resources.Selectedfiledoesnotexist, Resources.OK);}
            
            }
            else
            {
                // For create mode, just return the path (will be created later)
                return path;
            }
        }

        return null;
    }

    // Truncate string to max length
    internal static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }


    // Open folder dialog with validation
    public static string? ShowFolderDialog(string title, string message)
    {
        var dialog = new OpenDialog(title, message)
        {
            CanChooseFiles = false,
            CanChooseDirectories = true,
            AllowsMultipleSelection = false
        };

        Application.Run(dialog);

        if (!dialog.Canceled && dialog.FilePaths.Count > 0)
        {
            var path = dialog.FilePath.ToString();

            if (Directory.Exists(path))
                return path;

            MessageBox.ErrorQuery(Resources.InvalidFolder, Resources.Selectedfolderdoesnotexist, Resources.OK);
        }

        return null;
    }



}
