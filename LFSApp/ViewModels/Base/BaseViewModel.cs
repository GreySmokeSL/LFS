using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Core.Helper;
using Domain.Models;
using LFSApp.Services;

namespace LFSApp.ViewModels
{
    /// <summary>
    /// A base view model that fires Property Changed events as needed
    /// </summary>
    public class BaseViewModel<T> : DependencyObject, INotifyPropertyChanged
    {
        public static readonly int ParallelConcurrentCountDefaultValue = Environment.ProcessorCount;
        public static readonly string NL = Environment.NewLine;
        public DefaultCommandDialogService DCDServ { get; private set; } = new DefaultCommandDialogService();

        public T Tag { get; }
        public ContentControl Parent { get; set; }

        public BaseViewModel(T value)
        {
            Tag = value;
        }

        public virtual BaseDataModel<T> GetDataModel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The event that is fired when any child property changes its value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public async void ShowValidationResult(List<string> validationResult)
        {
            await Task.Run(() => DCDServ.ShowMessageCommand.Execute((
                $"Validation errors:{validationResult.Join(NL + "   - ", true)}.{NL}{NL}Correct values and try again.",
                MessageBoxImage.Error)));

        }
    }
}
