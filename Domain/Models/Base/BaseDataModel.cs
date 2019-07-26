using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Domain.Models
{
    /// <summary>
    /// A base view model that fires Property Changed events as needed
    /// </summary>
    public class BaseDataModel<T> : INotifyPropertyChanged
    {
        public T Tag { get; }
        public string TargetFile { get; set; }

        public BaseDataModel(T value)
        {
            Tag = value;
        }

        /// <summary>
        /// The event that is fired when any child property changes its value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public virtual IEnumerable<string> Validate()
        {
            yield break;
        }
    }
}
