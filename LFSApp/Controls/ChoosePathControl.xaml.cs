using DevExpress.Mvvm;
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

namespace LFSApp.Controls
{
    /// <summary>
    /// Interaction logic for ChoosePathControl.xaml
    /// </summary>
    public partial class ChoosePathControl : UserControl
    {
        public DelegateCommand ChoosePathCommand
        {
            get { return (DelegateCommand)GetValue(ChoosePathCommandProperty); }
            set { SetValue(ChoosePathCommandProperty, value); }
        }

        public static readonly DependencyProperty ChoosePathCommandProperty = DependencyProperty.Register("ChoosePathCommand", 
                typeof(DelegateCommand), typeof(ChoosePathControl), new PropertyMetadata(default));

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register("SelectedPath",
            typeof(string), typeof(ChoosePathControl), new PropertyMetadata(default));

        public ChoosePathControl()
        {
            InitializeComponent();
        }
    }
}
