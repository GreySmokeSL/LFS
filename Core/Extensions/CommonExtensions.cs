using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Core.Helper;

namespace Core.Extensions
{
    public static class CommonExtensions
    {
        public static void DisposeSequence<T>(this IEnumerable<T> source)
        {
            foreach (var disposableObject in source.OfType<IDisposable>())
                disposableObject.Dispose();
        }

        public static T AdaptValue<T>(this object value) => (T)Convert.ChangeType(value, typeof(T));

        public static T ResultIfValue<T>(this T value, T rejectValue, T result) where T : struct
        {
            return value.Equals(rejectValue) ? result : value;
        }

        public static T ValueOrFunc<T>(this T value, T rejectValue, Func<T> func) where T : struct
        {
            return value.Equals(rejectValue) ? func() : value;
        }
        
        public static long GetObjectSize<T>(this T o)
        {
            using (var s = new MemoryStream())
            {
                new BinaryFormatter().Serialize(s, o);
                return s.Length;
            }
        }

        #region string ext
        public static IEnumerable<string> SplitWithDelimiter(this string source, string delimiter, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return source.Split(new[] { delimiter }, options);
        }

        public static IEnumerable<string> SplitBuffer(this char[] ca, char predlm = '\r', char dlm = '\n')
        {
            var offset = 0;
            var ieol = -1;
            do
            {
                ieol = Array.IndexOf(ca, dlm, offset);
                if (ieol > offset && ca[ieol - 1] == predlm)
                {
                    yield return new string(ca, offset, ieol - 1 - offset);
                    offset = ieol + 1;
                }
            } while (ieol > -1 && offset < ca.Length);
        }

        public static int GetPartCount(this char[] ca, char dlm = '\n')
        {
            return Array.FindAll(ca, c => c == dlm).Length;
        }

        public static byte ToByte(this char c)
        {
            return Convert.ToByte(c);
        }
        
        public static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public static bool IsNone(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        /// <summary>
        /// Check if file exists and not empty and match extention
        /// </summary>
        public static bool CheckFile(this string strPath, IEnumerable<string> allowedExtensions = null)
        {
            if (strPath.IsNone() || (!strPath.Contains(@"\\") && !strPath.Contains(@":\")))
                return false;

            return new FileInfo(strPath).CheckFile(allowedExtensions);
        }

        public static bool CheckFile(this FileInfo fi, IEnumerable<string> allowedExtensions = null)
        {
            return fi.Exists && fi.Length > 0 &&
                   (allowedExtensions.IsEmpty() ||
                    allowedExtensions.Any(e => fi.Extension.Equals(e, StringComparison.InvariantCultureIgnoreCase)));
        }

        #endregion string ext
    }
}
