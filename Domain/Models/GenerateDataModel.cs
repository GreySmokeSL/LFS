using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Extensions;

namespace Domain.Models
{
    public class GenerateDataModel : BaseDataModel<ViewModelBehavior>
    {
        public GenerateDataModel(string targetFile, uint maxRecordCount, int maxConcurrentCount, uint maxRecordPerTask, byte maxRepetitionForText, byte maxSentenceWordCount) : base(ViewModelBehavior.Generate_Data)
        {
            TargetFile = targetFile;
            MaxRecordCount = maxRecordCount;
            MaxConcurrentCount = maxConcurrentCount;
            MaxRecordPerTask = maxRecordPerTask;
            MaxRepetitionForText = maxRepetitionForText;
            MaxSentenceWordCount = maxSentenceWordCount;
        }

        public uint MaxRecordCount { get; }
        public uint MaxRecordPerTask { get; }
        public int MaxConcurrentCount { get; }
        public byte MaxRepetitionForText { get; }
        public byte MaxSentenceWordCount { get; }

        public override IEnumerable<string> Validate()
        {
            if (TargetFile.IsNone())
                yield return "file not set";
            else if (TargetFile.CheckFile())
                yield return "file already exists";

            if (MaxRecordCount < 1 )
                yield return "total line count must be more than zero";
            if (MaxRecordPerTask < 1)
                yield return "line count per task must be more than zero";
            if (MaxRecordPerTask > MaxRecordCount)
                yield return "line count per task must be less than total line count";
        }
    }
}