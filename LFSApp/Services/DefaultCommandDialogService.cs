using BLL.Helper;
using Core.Helper;
using DevExpress.Mvvm;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Core.Extensions;
using static Domain.Constants;

namespace LFSApp.Services
{
    public class DefaultCommandDialogService : Freezable
    {
        #region DependencyProperty
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(DefaultCommandDialogService), new PropertyMetadata(default));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DefaultCommandDialogService), new PropertyMetadata(default));

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(DefaultCommandDialogService), new PropertyMetadata(default));

        public string InitialDirectory
        {
            get { return (string)GetValue(InitialDirectoryProperty); }
            set { SetValue(InitialDirectoryProperty, value); }
        }

        public static readonly DependencyProperty InitialDirectoryProperty =
            DependencyProperty.Register("InitialDirectory", typeof(string), typeof(DefaultCommandDialogService), new PropertyMetadata(default));


        public bool IsSucceed
        {
            get { return (bool)GetValue(IsSucceedProperty); }
            set { SetValue(IsSucceedProperty, value); }
        }

        public static readonly DependencyProperty IsSucceedProperty =
            DependencyProperty.Register("IsSucceed", typeof(bool), typeof(DefaultCommandDialogService), new PropertyMetadata(default));

        #endregion

        #region Command
        public DelegateCommand OpenCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand<(string, MessageBoxImage)> ShowMessageCommand { get; }
        public DelegateCommand<string> AskCommand { get; }
        #endregion

        public DefaultCommandDialogService()
        {
            IsSucceed = false;
            Title = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Title ?? Application.ResourceAssembly.GetName().Name;
            Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            OpenCommand = new DelegateCommand(() => DispatcherHelper.DispatcherInvoke(() => SelectOpenFile()));
            SaveCommand = new DelegateCommand(() => DispatcherHelper.DispatcherInvoke(() => SetSaveFile()));
            ShowMessageCommand = new DelegateCommand<(string text, MessageBoxImage icon)>((param) => DispatcherHelper.DispatcherInvoke(() => ShowMessage(param.text, param.icon)));
            AskCommand = new DelegateCommand<string>((message) => DispatcherHelper.DispatcherInvoke(() => Ask(message)));
        }

        private bool SelectOpenFile()
        {
            IsSucceed = false;
            var openFileDialog = new OpenFileDialog()
            {
                Filter = Filter,
                InitialDirectory = (FilePath.CheckFile() ? Path.GetDirectoryName(FilePath) : null) ?? InitialDirectory,
                Title = Title + " - Open File"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                IsSucceed = true;
            }
            return IsSucceed;
        }

        private bool SetSaveFile()
        {
            IsSucceed = false;
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = Filter,
                Title = Title + " - Save File As",
                OverwritePrompt = false,
                InitialDirectory = (!FilePath.IsNone() ? Path.GetDirectoryName(FilePath) : null) ?? InitialDirectory,
                AddExtension = true,
                DefaultExt = "txt",
                FileName = $@"result{DateTime.Now.ToString(DefaultDateTimeStampFormat)}.txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                FilePath = saveFileDialog.FileName;
                IsSucceed = true;
            }
            return IsSucceed;
        }

        private void ShowMessage(string message, MessageBoxImage icon)
        {
            IsSucceed = false;
            MessageBox.Show(message, Title, MessageBoxButton.OK, icon);
            IsSucceed = true;
        }

        private bool Ask(string message)
        {
            return IsSucceed = MessageBox.Show(message, Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new DefaultCommandDialogService();
        }
    }
}
