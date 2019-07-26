using System;
using System.Collections.Generic;
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
using LFSApp.ViewModels;

namespace LFSApp.Views
{
    /// <summary>
    /// Interaction logic for GenerateDataView.xaml
    /// </summary>
    public partial class GenerateDataView : UserControl
    {
        public GenerateDataView()
        {
            InitializeComponent();
        }

        private void View_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is GenerateDataViewModel vm)
                vm.Parent = this;
        }
    }
}
