using System.Collections.Generic;
using System.IO;
using Caliburn.Micro;
using Gemini.Framework.Results;
using Microsoft.Win32;

namespace Gemini.Modules.Inspector.Inspectors
{
    public class FilePath
    {
        public string FullName { get; set; }

        public bool OpenFolderDialog { get; set; }

        public FilePath()
        {
            FullName = "";
            OpenFolderDialog = false;
        }

        public FilePath(string fullName, bool openFolderDialog = false)
        {
            FullName = fullName;
            OpenFolderDialog = openFolderDialog;
        }
    }

    public class FilePathEditorViewModel : EditorBase<FilePath>, ILabelledInspector
    {
        public IEnumerable<IResult> Choose()
        {
            var fileDialog = new OpenFileDialog();

            if (Value.OpenFolderDialog)
            {
                fileDialog.ValidateNames = false;
                fileDialog.CheckFileExists = false;
                fileDialog.CheckPathExists = true;

                fileDialog.FileName = "Folder Selection Mode";
            }

            yield return Show.CommonDialog(fileDialog);

            // Get selected filename
            var fileName = fileDialog.FileName;

            // Get selected folder
            if (Value.OpenFolderDialog)
            {
                if (fileName != null &&
                    (fileName.EndsWith("Folder Selection Mode") || !File.Exists(fileName)) &&
                    !Directory.Exists(fileName))
                {
                    fileName = Path.GetDirectoryName(fileName);
                }
            }
            
            Value = new FilePath(fileName, Value.OpenFolderDialog);
        }
    }
}
