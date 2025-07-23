using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAAL
{
    public class FileFolderPicker
    {
        private readonly Window _parentWindow;

        public FileFolderPicker(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        public async Task PickFile(Action<string> output)
        {
            var files = await _parentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Pick a file...",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.All }
            });

            if (files is { Count: > 0 })
            {
                output?.Invoke(files[0].Path.LocalPath);
            }
        }

        public async Task PickFolder(Action<string> output)
        {
            var folder = await _parentWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Pick a folder...",
                AllowMultiple = false,
            });

            if (folder is { Count: > 0 })
            {
                output?.Invoke(folder[0].Path.LocalPath);
            }
        }
    }
}
