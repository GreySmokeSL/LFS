using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Core.Extensions;
using Core.Helper;
using BLL.Log;
using BLL.Processors;
using DevExpress.Mvvm;
using Domain.Domain;
using Domain.Enums;
using Domain.Models;
using LFSApp.Services;
using BLL.Helper;
using System.Windows.Controls;
using DevExpress.Mvvm.Native;

namespace LFSApp.ViewModels
{
    public class MainViewModel : BaseViewModel<ViewModelBehavior>
    {
        public BaseViewModel<ViewModelBehavior> CurrentBehavior => Models?.FirstOrDefault(m => SelectedButton?.Behavior == m.Tag);
        public ILogger Logger { get; set; }
        public AsyncCommand ProcessCommand { get; set; }
        public DelegateCommand TerminateCommand { get; set; }
        public DelegateCommand<string> InsertLogCommand { get; set; }
        public DelegateCommand ClearLogCommand { get; set; }
        public MultiProgressBarModel<long> ProgressModel { get; } = new MultiProgressBarModel<long>();

        public List<ButtonInfo> Buttons { get; set; } = EnumHelper.GetEnumItems<ViewModelBehavior>()
            .Select(i => new ButtonInfo { Behavior = i.Key, Title = i.Value }).ToList();

        public ButtonInfo PreviewDataButton => Buttons.FirstOrDefault(x => x.Behavior == ViewModelBehavior.View_Data);

        public ObservableCollection<BaseViewModel<ViewModelBehavior>> Models { get; private set; } =
            new ObservableCollection<BaseViewModel<ViewModelBehavior>>();

        public ArrangeDataViewModel ArrangeModel => Models.OfType<ArrangeDataViewModel>().FirstOrDefault();
        public ViewDataViewModel PreviewModel => Models.OfType<ViewDataViewModel>().FirstOrDefault();

        private ButtonInfo _selectedButton = null;

        public ButtonInfo SelectedButton
        {
            get { return _selectedButton; }
            set
            {
                if (value != null && _selectedButton != value)
                {
                    _selectedButton = value;
                    NotifyPropertyChanged(nameof(SelectedButton));
                    NotifyPropertyChanged(nameof(CurrentBehavior));
                    NotifyPropertyChanged(nameof(IsProcessingAllowed));
                }
            }
        }

        public List<string> LogLines { get; set; } = new List<string>();

        public bool IsProcessExecuting => ProcessCommand?.IsExecuting ?? false;
        public bool IsProcessingAllowed => new[] { ViewModelBehavior.Generate_Data, ViewModelBehavior.Arrange_Data }.Contains(CurrentBehavior?.Tag ?? ViewModelBehavior.None);
        private CancellationToken ProcCancToken => ProcessCommand?.CancellationTokenSource?.Token ?? CancellationToken.None;

        public MainViewModel() : base(ViewModelBehavior.Main)
        {
            ProcessCommand = new AsyncCommand(() =>
                    Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    NotifyPropertyChanged(nameof(IsProcessExecuting));
                                    ProcessData().Wait();
                                }
                                catch (Exception ex)
                                {
                                    Logger?.LogException($"ProcessData failed for {CurrentBehavior?.Tag ?? ViewModelBehavior.None}", ex);
                                }
                                finally
                                {
                                    NotifyPropertyChanged(nameof(IsProcessExecuting));
                                }
                            },
                            ProcCancToken)
                        .ContinueWith(x => Log($"Terminate by user"), TaskContinuationOptions.OnlyOnCanceled),
                () => IsProcessingAllowed && !IsProcessExecuting,
                false);
            TerminateCommand = ProcessCommand.CancelCommand;
        }

        private async Task ProcessData()
        {
            if (CurrentBehavior == null)
                return;

            var validationResult = new List<string>();
            var dataModel = DispatcherHelper.DispatcherInvoke(() =>
            {
                var dm = CurrentBehavior.GetDataModel();
                if (CurrentBehavior.Parent != null)
                    validationResult.AddRange(CurrentBehavior.Parent.GetErrors());
                validationResult.AddRange(dm.Validate());
                return dm;
            });

            if (!validationResult.IsEmpty())
            {
                TerminateCommand.Execute(null);
                ShowValidationResult(validationResult);
                return;
            }

            switch (CurrentBehavior)
            {
                case GenerateDataViewModel gdvm:
                    {
                        await GenerateFakeDataAsync(dataModel as GenerateDataModel);
                    }
                    break;
                case ArrangeDataViewModel advm:
                    {
                        await TransferSortDataAsync(dataModel as ArrangeDataModel);
                    }
                    break;
                case MainViewModel mvm:
                case ViewDataViewModel vdvm:
                case null:
                    break;
                default:
                    InsertLogCommand.Execute($"Unknown type of behavior: {CurrentBehavior.GetType()}, Tag={CurrentBehavior.Tag}");
                    break;
            }
            
            //switch to view
            if (dataModel?.TargetFile?.CheckFile() ?? false)
            {
                DispatcherHelper.DispatcherInvoke(() =>
                {
                    if (ArrangeModel != null && dataModel is GenerateDataModel)
                        ArrangeModel.SourceFile = dataModel.TargetFile;
                    PreviewResult(dataModel.TargetFile);
                });
            }
        }

        private async Task<ulong> GenerateFakeDataAsync(GenerateDataModel gdvm)
        {
            if (gdvm == null)
                throw new ArgumentException(nameof(gdvm));

            ProgressModel.Reset();
            ProgressModel.StageInfo = "Data generation in progress...";
            
            return await new FakeDataGenerator<long>(ProgressModel, Logger).GenerateDataToFileAsync(gdvm, ProcCancToken);
        }

        private async Task<ulong> TransferSortDataAsync(ArrangeDataModel advm)
        {
            if (advm == null)
                throw new ArgumentException(nameof(advm));

            ProgressModel.UpdateStage("Data arrangement in progress...", true);
          
            return await new DataProcessor<long>(ProgressModel, Logger).TransferSortedDataToFileAsync(advm, ProcCancToken);
        }

        private void Log(string message) => Logger?.Log(message);

        private void PreviewResult(string targetFile)
        {
            if (File.Exists(targetFile) && PreviewDataButton != null && PreviewModel != null)
            {
                SelectedButton = PreviewDataButton;
                PreviewModel.TargetFile = targetFile;
            }
        }
    }
}