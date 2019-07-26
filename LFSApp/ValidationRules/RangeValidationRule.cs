using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;

namespace LFSApp.ValidationRules
{
    public class RangeValidationRule<T> : ValidationRule where T : struct
    {
        public T Minimum { get; set; } = default;
        public T Maximum { get; set; } = default;

        public string Source { get; set; } = string.Empty;

        public RangeValidationRule()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (Comparer<T>.Default.Compare(Minimum, Maximum) > 0)
                    return new ValidationResult(false, $"{Source}invalid range: {Minimum}-{Maximum}");

                var tVal = (T)Convert.ChangeType(value, typeof(T));
                if (Comparer<T>.Default.Compare(tVal, Minimum) >= 0
                    && Comparer<T>.Default.Compare(tVal, Maximum) <= 0)
                    return new ValidationResult(true, null);
            }
            catch (Exception)
            {
            }

            return new ValidationResult(false, $"{Source}invalid value {value}, allowed range: {Minimum}-{Maximum}");
        }
    }

    public class RangeValidationRuleULong : RangeValidationRule<ulong>
    {
        public RangeValidationRuleULong()
        {
            Minimum = ulong.MinValue;
            Maximum = ulong.MaxValue;
        }
    }

    public class RangeValidationRuleUInt : RangeValidationRule<uint>
    {
        public RangeValidationRuleUInt()
        {
            Minimum = uint.MinValue;
            Maximum = uint.MaxValue;
        }
    }
}
