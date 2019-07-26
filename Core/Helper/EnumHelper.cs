using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper
{
    public static class EnumHelper
    {
        public static IEnumerable<EnumValueType> EnumToEnumerable<EnumValueType>() where EnumValueType : struct
        {
            var aEnumType = typeof(EnumValueType);
            if (!aEnumType.IsEnum)
                throw new ArgumentException($"{aEnumType.FullName} is not Enum argument type");

            return Enum.GetValues(typeof(EnumValueType)).Cast<EnumValueType>();
        }

        public static Dictionary<EnumValueType, string> GetEnumItems<EnumValueType>(Func<EnumValueType, string> getNameFunc = null, bool onlyValuable = true)
            where EnumValueType : struct, IConvertible
        {
            var result = EnumToEnumerable<EnumValueType>()
                .Where(x => !onlyValuable || Convert.ToInt16(x) > 0)
                .ToDictionary(x => x, v => getNameFunc?.Invoke(v) ?? v.ToString(CultureInfo.InvariantCulture).Replace('_', ' '));

            return result;
        }
    }
}
