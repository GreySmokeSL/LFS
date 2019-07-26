using System;
using System.Collections.Generic;
using System.Windows;
using BLL.Log;
using DevExpress.Mvvm;
using Domain.Enums;
using LFSApp.ViewModels;
using BLL.Helper;

namespace LFSApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var mvm = new MainViewModel();

            mvm.ClearLogCommand = new DelegateCommand(() => DispatcherHelper.DispatcherInvoke(() => lbLog.Items.Clear()), () => !lbLog.Items.IsEmpty);
            mvm.InsertLogCommand = new DelegateCommand<string>((text) => DispatcherHelper.DispatcherInvoke(() =>
            {
                lbLog.Items.Insert(0, text);
                lbLog.SelectedIndex = 0;
            }));

            mvm.Models.Add(new GenerateDataViewModel("", 10000000, Environment.ProcessorCount));
            mvm.Models.Add(new ArrangeDataViewModel("", "", Environment.ProcessorCount));
            mvm.Models.Add(new ViewDataViewModel());

            Logger.Instance.DefaultLogTarget = LogTarget.UI;
            Logger.Instance.UILogAction = text => mvm.InsertLogCommand.Execute(text);
            mvm.Logger = Logger.Instance;

            this.DataContext = mvm;
        }
    }
}
