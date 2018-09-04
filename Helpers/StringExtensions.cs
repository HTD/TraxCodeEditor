using System;

namespace Woof.SystemEx {

    public static class StringExtensions {

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current
        /// instance are replaced with another specified string.
        /// </summary>
        /// <param name="text">This string.</param>
        /// <param name="search">The string to be replaced.</param>
        /// <param name="replacement">The string to replace all occurrences of search.</param>
        /// <param name="comparison">One of the enumeration values that specifies the rules for the search.</param>
        /// <returns></returns>
        public static string Replace(this string text, string search, string replacement, StringComparison comparison) {
            int index = 0, length1 = search.Length, length2 = replacement.Length;
            while ((index = text.IndexOf(search, index, comparison)) >= 0) {
                text = text.Remove(index, length1).Insert(index, replacement);
                index += length2;
            }
            return text;
        }

    }

}