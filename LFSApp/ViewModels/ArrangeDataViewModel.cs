using Domain;
using Domain.Enums;
using Domain.Models;
using System.Windows;

namespace LFSApp.ViewModels
{
    public class ArrangeDataViewModel : BaseViewModel<ViewModelBehavior>
    {
        public ArrangeDataViewModel(string sourceFile, string targetFile, int maxConcurrentCount, ushort blockSizeMB = Constants.DefaultReaderBlockSizeMB,
            int maxMarkerGroupLineCount = Constants.DefaultReaderMarkerGroupLineCount)
            : base(ViewModelBehavior.Arrange_Data)
        {
            SourceFile = sourceFile;
            TargetFile = targetFile;
            MaxConcurrentCount = maxConcurrentCount;
            BlockSizeMb = blockSizeMB;
            MaxMarkerGroupLineCount = maxMarkerGroupLineCount;
        }

        public override BaseDataModel<ViewModelBehavior> GetDataModel()
        {
            return new ArrangeDataModel(SourceFile, TargetFile, MaxConcurrentCount, BlockSizeMb, MaxMarkerGroupLineCount);
        }

        #region DependencyProperty

        public string SourceFile
        {
            get { return (string)GetValue(SourceFileProperty); }
            set { SetValue(SourceFileProperty, value); }
        }

        public static readonly DependencyProperty SourceFileProperty =
            DependencyProperty.Register("SourceFile", typeof(string), typeof(ArrangeDataViewModel), new PropertyMetadata(default));

        public string TargetFile
        {
            get { return (string)GetValue(TargetFileProperty); }
            set { SetValue(TargetFileProperty, value); }
        }

        public static readonly DependencyProperty TargetFileProperty =
            DependencyProperty.Register("TargetFile", typeof(string), typeof(ArrangeDataViewModel), new PropertyMetadata(default));

        public int MaxConcurrentCount
        {
            get { return (int)GetValue(MaxConcurrentCountProperty); }
            set { SetValue(MaxConcurrentCountProperty, value); }
        }

        public static readonly DependencyProperty MaxConcurrentCountProperty =
            DependencyProperty.Register("MaxConcurrentCount", typeof(int), typeof(ArrangeDataViewModel), new PropertyMetadata(default));

        public int MaxMarkerGroupLineCount
        {
            get { return (int)GetValue(MaxMarkerGroupLineCountProperty); }
            set { SetValue(MaxMarkerGroupLineCountProperty, value); }
        }

        public static readonly DependencyProperty MaxMarkerGroupLineCountProperty =
            DependencyProperty.Register("MaxMarkerGroupLineCount", typeof(int), typeof(ArrangeDataViewModel), new PropertyMetadata(default));

        public ushort BlockSizeMb
        {
            get { return (ushort)GetValue(BlockSizeMbProperty); }
            set { SetValue(BlockSizeMbProperty, value); }
        }

        public static readonly DependencyProperty BlockSizeMbProperty =
            DependencyProperty.Register("BlockSizeMb", typeof(ushort), typeof(ArrangeDataViewModel), new PropertyMetadata(default));

        #endregion DependencyProperty
    }
}