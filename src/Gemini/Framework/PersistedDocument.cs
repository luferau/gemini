using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Gemini.Framework
{
    public abstract class PersistedDocument : Document, IPersistedDocument
    {
        private bool _isDirty;

        public bool IsNew { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (value == _isDirty)
                    return;

                _isDirty = value;
                NotifyOfPropertyChange(() => IsDirty);
                UpdateDisplayName();
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            if (!IsDirty)
            {
                callback(true);
                DoClose();
                return;
            }

            var result = MessageBox.Show($"Document {FileName} has unsaved changes." +
                                         $"{Environment.NewLine}Do you want to save changes before close?",
                                         "Confirm", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Cancel:
                    callback(false);
                    break;
                case MessageBoxResult.Yes:
                    DoSave(FilePath);
                    DoClose();
                    callback(true);
                    break;
                case MessageBoxResult.No:
                    DoClose();
                    callback(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateDisplayName()
        {
            DisplayName = IsDirty ? FileName + "*" : FileName;
        }

        public async Task New(string fileName)
        {
            FileName = fileName;
            FilePath = fileName;
            UpdateDisplayName();

            IsNew = true;
            IsDirty = false;

            await DoNew(fileName);
        }

        protected abstract Task DoNew(string fileName);

        public async Task Load(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            UpdateDisplayName();

            IsNew = false;
            IsDirty = false;

            await DoLoad(filePath);
        }

        protected abstract Task DoLoad(string filePath);

        public async Task Save(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            UpdateDisplayName();

            await DoSave(filePath);

            IsDirty = false;
            IsNew = false;
        }

        protected abstract Task DoSave(string filePath);

        protected abstract Task DoClose();
    }
}