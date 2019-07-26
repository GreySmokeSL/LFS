using Core.Extensions;
using Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using static Domain.Constants;

namespace Domain.Models
{
    public class ViewDataModel : BaseDataModel<ViewModelBehavior>
    {
        public ViewDataModel(string targetFile) : base(ViewModelBehavior.View_Data)
        {
            TargetFile = targetFile;
        }

        public override IEnumerable<string> Validate()
        {
            if (TargetFile.IsNone())
                yield return "target file not set";
            else if (!TargetFile.CheckFile())
                yield return "target file does not exists";
        }

    }
}