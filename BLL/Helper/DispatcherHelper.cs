using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using BLL.Log;

namespace BLL.Helper
{
    public static class DispatcherHelper
    {
        //AppDomain.CurrentDomain.UnhandledException From all threads in the AppDomain.
        //Dispatcher.UnhandledException From a single specific UI dispatcher thread.
        //Application.Current.DispatcherUnhandledException From the main UI dispatcher thread in your WPF application.
        //TaskScheduler.UnobservedTaskException from within each AppDomain that uses a task scheduler for asynchronous operations.
        public static void InitDispatchGlobalException(this Application application)
        {
            application.DispatcherUnhandledException += (o, args) =>
            {
                Logger.Instance.LogException("Unhandled Application Exception", args.Exception);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (o, args) =>
                Logger.Instance.LogException("Unhandled Domain Exception", args.ExceptionObject as Exception);

#if DEBUG
            PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingErrorTraceListener());
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
#endif
        }

        public static void DispatcherInvoke(Action act)
        {
            Application.Current.Dispatcher.Invoke(() => act());
        }

        public static T DispatcherInvoke<T>(Func<T> func)
        {
            return Application.Current.Dispatcher.Invoke(() => func());
        }

        public static IEnumerable<string> GetErrors(this DependencyObject parent)
        {
            if (parent != null)
                foreach (var child in LogicalTreeHelper.GetChildren(parent))
                    if (child is FrameworkElement element)
                    {
                        if (Validation.GetHasError(element))
                            foreach (var error in Validation.GetErrors(element))
                                yield return $"{error.ErrorContent}";

                        foreach (var err in GetErrors(element))
                            yield return err;
                    }
        }
    }
}
