using LFSApp.ViewModels;
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

namespace LFSApp.Views
{
    /// <summary>
    /// Interaction logic for ArrangeDataView.xaml
    /// </summary>
    public partial class ArrangeDataView : UserControl
    {
        public ArrangeDataView()
        {
            InitializeComponent();
        }

        private void View_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ArrangeDataViewModel vm)
                vm.Parent = this;
        }
    }
}
