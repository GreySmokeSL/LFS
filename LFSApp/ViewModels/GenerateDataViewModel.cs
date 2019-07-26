using Domain.Enums;
using Domain.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LFSApp.ViewModels
{
    public class GenerateDataViewModel : BaseViewModel<ViewModelBehavior>
    {
        public const uint MaxRecordPerTaskDefaultValue = 512000;
        public const byte MaxRepetitionForTextDefaultValue = 10;
        public const byte MaxSentenceWordCountDefaultValue = 3;

        
        public GenerateDataViewModel(string targetFile, uint maxRecordCount, int maxConcurrentCount,
            uint maxRecordPerTask = MaxRecordPerTaskDefaultValue,
            byte maxRepetitionForText = MaxRepetitionForTextDefaultValue,
            byte maxSentenceWordCount = MaxSentenceWordCountDefaultValue)
            : base(ViewModelBehavior.Generate_Data)
        {
            TargetFile = targetFile;
            MaxRecordCount = maxRecordCount;
            MaxConcurrentCount = maxConcurrentCount;
            MaxRecordPerTask = maxRecordPerTask;
            MaxRepetitionForText = maxRepetitionForText;
            MaxSentenceWordCount = maxSentenceWordCount;
        }

        public override BaseDataModel<ViewModelBehavior> GetDataModel()
        {
            return new GenerateDataModel(TargetFile, MaxRecordCount, MaxConcurrentCount, MaxRecordPerTask, MaxRepetitionForText, MaxSentenceWordCount);
        }

        #region DependencyProperty

        public string TargetFile
        {
            get { return (string)GetValue(TargetFileProperty); }
            set { SetValue(TargetFileProperty, value); }
        }

        public static readonly DependencyProperty TargetFileProperty =
            DependencyProperty.Register("TargetFile", typeof(string), typeof(GenerateDataViewModel), new PropertyMetadata(default));

        public uint MaxRecordCount
        {
            get { return (uint)GetValue(MaxRecordCountProperty); }
            set { SetValue(MaxRecordCountProperty, value); }
        }

        public static readonly DependencyProperty MaxRecordCountProperty =
            DependencyProperty.Register("MaxRecordCount", typeof(uint), typeof(GenerateDataViewModel), new PropertyMetadata(default));

        public int MaxConcurrentCount
        {
            get { return (int)GetValue(MaxConcurrentCountProperty); }
            set { SetValue(MaxConcurrentCountProperty, value); }
        }

        public static readonly DependencyProperty MaxConcurrentCountProperty =
            DependencyProperty.Register("MaxConcurrentCount", typeof(int), typeof(GenerateDataViewModel), new PropertyMetadata(default));

        public uint MaxRecordPerTask
        {
            get { return (uint)GetValue(MaxRecordPerTaskProperty); }
            set { SetValue(MaxRecordPerTaskProperty, value); }
        }

        public static readonly DependencyProperty MaxRecordPerTaskProperty =
            DependencyProperty.Register("MaxRecordPerTask", typeof(uint), typeof(GenerateDataViewModel), new PropertyMetadata(default));

        public byte MaxRepetitionForText
        {
            get { return (byte)GetValue(MaxRepetitionForTextProperty); }
            set { SetValue(MaxRepetitionForTextProperty, value); }
        }

        public static readonly DependencyProperty MaxRepetitionForTextProperty =
            DependencyProperty.Register("MaxRepetitionForText", typeof(byte), typeof(GenerateDataViewModel), new PropertyMetadata(default));

        public byte MaxSentenceWordCount
        {
            get { return (byte)GetValue(MaxSentenceWordCountProperty); }
            set { SetValue(MaxSentenceWordCountProperty, value); }
        }

        public static readonly DependencyProperty MaxSentenceWordCountProperty =
            DependencyProperty.Register("MaxSentenceWordCount", typeof(byte), typeof(GenerateDataViewModel), new PropertyMetadata(default));

        #endregion DependencyProperty

    }
}