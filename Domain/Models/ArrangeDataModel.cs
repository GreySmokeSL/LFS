using Core.Extensions;
using Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using static Domain.Constants;

namespace Domain.Models
{
    public class ArrangeDataModel : BaseDataModel<ViewModelBehavior>
    {
        public ArrangeDataModel(string sourceFile, string targetFile, int maxConcurrentCount, ushort blockSizeMB, int maxMarkerGroupLineCount) : base(ViewModelBehavior.Arrange_Data)
        {
            SourceFile = sourceFile;
            TargetFile = targetFile;
            MaxConcurrentCount = maxConcurrentCount;
            BlockSizeMb = blockSizeMB;
            MaxMarkerGroupLineCount = maxMarkerGroupLineCount;
        }

        public string SourceFile { get; }
        public int MaxConcurrentCount { get; }
        public ushort BlockSizeMb { get; }
        public int MaxMarkerGroupLineCount { get; }

        public override IEnumerable<string> Validate()
        {
            if (SourceFile.IsNone())
                yield return "source file not selected";
            else if (!SourceFile.CheckFile())
                yield return "source file does not exists";

            if (TargetFile.IsNone())
                yield return "target file not set";
            else if (TargetFile.CheckFile())
                yield return "target file already exists";
        }
    }
}