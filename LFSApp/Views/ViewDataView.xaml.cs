using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Core.Helper;
using DevExpress.Mvvm;
using LFSApp.ViewModels;
using Xceed.Wpf.Toolkit;

namespace LFSApp.Views
{
    /// <summary>
    /// Interaction logic for ViewDataView.xaml
    /// </summary>
    public partial class ViewDataView : UserControl
    {
        public ViewDataView()
        {
            InitializeComponent();
        }

        private void SpinPage_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                fdr.Focus();
        }

        private void SpinPage_OnSpinned(object sender, SpinEventArgs e)
        {
            fdr.Focus();
        }

        private void TxtCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                fdr.Focus();
        }

        private void View_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ViewDataViewModel vm)
                vm.Parent = this;
        }
    }
}
