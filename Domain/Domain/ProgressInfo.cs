using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Domain.Domain
{
    public class ProgressInfo<T> : INotifyPropertyChanged where T : struct
    {
        private T _completed;
        private T _total;
        private string _description;

        public ProgressInfo(T completed = default, T total = default, string description = null)
        {
            Completed = completed;
            Total = total;
            Description = description;
        }

        public T Completed
        {
            get => _completed;
            set
            {
                _completed = value;
                NotifyPropertyChanged(nameof(Completed));
            }
        }

        public T Total
        {
            get => _total;
            set
            {
                _total = value;
                NotifyPropertyChanged(nameof(Total));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                NotifyPropertyChanged(nameof(Description));
            }
        }

        public void Reset()
        {
            Completed = default;
            Total = default;
            Description = default;
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
