using BLL.Helper;
using System.Windows;
using System.Windows.Media.Animation;

namespace LFSApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.InitDispatchGlobalException();
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 10 }
            );
        }

    }
}
