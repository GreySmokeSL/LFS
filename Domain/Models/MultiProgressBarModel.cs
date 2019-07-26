using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;

namespace Domain.Models
{
    public class MultiProgressBarModelDesign : MultiProgressBarModel<string> { }

    public class MultiProgressBarModel<T> : INotifyPropertyChanged
    {
        private T _mainTotal;
        private T _mainValue;
        private string _mainPrefix;
        private T _detailTotal;
        private T _detailValue;
        private string _detailPrefix;

        private bool _doRefresh;
        private Task _refreshTask;

        public bool DoRefresh
        {
            get => _doRefresh;
            set
            {
                _doRefresh = value;
                if (_doRefresh && _refreshTask == null)
                    _refreshTask = Task.Factory.StartNew(() =>
                    {
                        while (DoRefresh)
                        {
                            RefreshInfo();
                            Task.Delay(250);
                        }
                    }, TaskCreationOptions.LongRunning);
                else
                {
                    _refreshTask?.Wait();
                    _refreshTask = null;
                }
            }
        }

        /// <summary>
        /// The event that is fired when any child property changes its value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void NotifyPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private string GetInfo(string prefix, decimal? percent, T value, T total)
        {
            if (Comparer<T>.Default.Compare(default, total) == 0 || Comparer<T>.Default.Compare(value, total) > 0)
                return String.Empty;

            var percentText = percent == null ? "N/A" : $"{percent:0}%";
            return $"{prefix} {percentText} ({value}/{total})";
        }

        private decimal? GetPercent(T value, T total)
        {
            return Comparer<T>.Default.Compare(default, total) == 0
                ? (decimal?)null
                : 100.0m * (decimal)Convert.ChangeType(value, typeof(decimal)) /
                  (decimal)Convert.ChangeType(total, typeof(decimal));
        }

        private void RefreshInfo()
        {
            NotifyPropertyChanged(nameof(HasStage));
            NotifyPropertyChanged(nameof(HasDetail));
            NotifyPropertyChanged(nameof(HasMain));
            NotifyPropertyChanged(nameof(StageInfo));
            NotifyPropertyChanged(nameof(DetailInfo));
            NotifyPropertyChanged(nameof(MainInfo));
        }

        public void Reset()
        {
            StageInfo = default;
            MainTotal = default;
            MainValue = default;
            MainPrefix = default;
            DetailTotal = default;
            DetailValue = default;
            DetailPrefix = default;
        }

        #region Stage

        public void UpdateStage(string info, bool doReset)
        {
            if (doReset)
                Reset();
            StageInfo = info;
        }

        public string StageInfo { get; set; }

        public bool HasStage => !StageInfo.IsNone();

        #endregion Stage

        #region Main

        public void SetMain<TValue>(TValue total = default, TValue value = default, string prefix = default)
        {
            MainTotal = total.AdaptValue<T>();
            MainValue = value.AdaptValue<T>();
            MainPrefix = prefix;
        }

        public decimal? MainPercent => GetPercent(MainValue, MainTotal);
        public string MainInfo => GetInfo(MainPrefix, MainPercent, MainValue, MainTotal);
        public bool HasMain => !MainInfo.IsNone();

        public T MainTotal
        {
            get => _mainTotal;
            set
            {
                _mainTotal = value;
                NotifyPropertyChanged(nameof(MainTotal));
            }
        }

        public T MainValue
        {
            get => _mainValue;
            set
            {
                _mainValue = value;
                NotifyPropertyChanged(nameof(MainValue));
            }
        }

        public string MainPrefix
        {
            get => _mainPrefix;
            set
            {
                _mainPrefix = value;
                NotifyPropertyChanged(nameof(MainPrefix));
            }
        }

        #endregion Main

        #region Detail

        public void SetDetail<TValue>(TValue total = default, TValue value = default, string prefix = default)
        {
            DetailTotal = total.AdaptValue<T>(); ;
            DetailValue = value.AdaptValue<T>();
            DetailPrefix = prefix;
        }

        public decimal? DetailPercent => GetPercent(DetailValue, DetailTotal);
        public string DetailInfo => GetInfo(DetailPrefix, DetailPercent, DetailValue, DetailTotal);
        public bool HasDetail => !DetailInfo.IsNone();

        public T DetailTotal
        {
            get => _detailTotal;
            set
            {
                _detailTotal = value;
                NotifyPropertyChanged(nameof(DetailTotal));
            }
        }

        public T DetailValue
        {
            get => _detailValue;
            set
            {
                _detailValue = value;
                NotifyPropertyChanged(nameof(DetailValue));
            }
        }

        public string DetailPrefix
        {
            get => _detailPrefix;
            set
            {
                _detailPrefix = value;
                NotifyPropertyChanged(nameof(DetailPrefix));
            }
        }

        #endregion Detail
    }


}

