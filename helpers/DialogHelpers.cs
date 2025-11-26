using Terminal.Gui;
using System;
using System.IO;

public static class DialogHelpers
{
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
                throw new Exception("Invalid path.");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir); 

            return path;
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery(40, 10, "Error", ex.Message, "OK");
            return null;
        }
    }


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
                else{MessageBox.ErrorQuery("Invalid File", "Selected file does not exist.", "OK");}
            
            }
            else
            {
                // For create mode, just return the path (will be created later)
                return path;
            }
        }

        return null;
    }


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

            MessageBox.ErrorQuery("Invalid Folder", "Selected folder does not exist.", "OK");
        }

        return null;
    }



}
