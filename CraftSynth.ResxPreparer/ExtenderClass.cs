using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CraftSynth.ResxPreparer
{
    public static class ExtenderClass
    {
        public static bool IsNull<T>(this T? nullableObject) where T : struct
        {
            return !nullableObject.HasValue;
        }

        public static string ToNonNullString<T>(this T? nullableObject) where T : struct
        {
            return ToNonNullString<T>(nullableObject, string.Empty);
        }

        public static string ToNonNullString<T>(this T? nullableObject, string nullCaseString) where T : struct
        {
            if (nullableObject.HasValue)
            {
                return nullableObject.Value.ToString();
            }
            else
            {
                return nullCaseString;
            }
        }

        public static string ToNonNullString(this DateTime? nullableObject)
        {
            return ToNonNullString(nullableObject, string.Empty, null);
        }

        public static string ToNonNullString(this DateTime? nullableObject, string nullCaseString)
        {
            return ToNonNullString(nullableObject, nullCaseString, null);
        }

        public static string ToNonNullString(this DateTime? nullableObject, string nullCaseString, string format)
        {
            if (nullableObject.HasValue)
            {
                if (format == null)
                {
                    return nullableObject.Value.ToString();
                }
                else
                {
                    return nullableObject.Value.ToString(format);
                }
            }
            else
            {
                return nullCaseString;
            }
        }

        public static string ToNonNullString(this string nullableObject)
        {
            return ToNonNullString(nullableObject, string.Empty);
        }

        public static string ToNonNullString(this string nullableObject, string nullCaseString)
        {
            if (nullableObject != null)
            {
                return nullableObject;
            }
            else
            {
                return nullCaseString;
            }
        }

        public static bool IsNullOrWhiteSpace(this string nullableObject)
        {
            return string.IsNullOrWhiteSpace(nullableObject);
        }

        public static bool IsNOTNullOrWhiteSpace(this string nullableObject)
        {
            return !string.IsNullOrWhiteSpace(nullableObject);
        }

        public static string EnclosedWithPercentSignMakeStartsWithClause(this string nullableObject)
        {
            var result = nullableObject;

            if (nullableObject.IsNOTNullOrWhiteSpace())
            {
                if (!nullableObject.EndsWith("%"))
                {
                    result = result + "%";
                }
            }

            return result;
        }

        public static string EnclosedWithPercentSign(this string nullableObject)
        {
            string result = nullableObject;

            if (nullableObject.IsNOTNullOrWhiteSpace())
            {
                if (!nullableObject.StartsWith("%"))
                {
                    result = "%" + result;
                }

                if (!nullableObject.EndsWith("%"))
                {
                    result = result + "%";
                }
            }

            return result;
        }

        /// <summary>
        /// If instance is not null or white space returns instance + stringAppend;
        /// otherwse returns instance.
        /// </summary>
        /// <param name="nullableObject"></param>
        /// <param name="stringToPrepend"></param>
        /// <returns></returns>
        public static string AppendIfNotNullOrWhiteSpace(this string nullableObject, string stringToAppend)
        {
            string result = nullableObject;
            if (nullableObject.IsNOTNullOrWhiteSpace())
            {
                result = result + stringToAppend;
            }
            return result;
        }

        /// <summary>
        /// If instance is not null or white space returns stringToPrepend + instance;
        /// otherwse returns instance.
        /// </summary>
        /// <param name="nullableObject"></param>
        /// <param name="stringToPrepend"></param>
        /// <returns></returns>
        public static string PrependIfNotNullOrWhiteSpace(this string nullableObject, string stringToPrepend)
        {
            string result = nullableObject;
            if (nullableObject.IsNOTNullOrWhiteSpace())
            {
                result = stringToPrepend + result;
            }
            return result;
        }

        public static List<string> ParseCSV(this string nullableString, char[] separator)
        {
            return ParseCSV<string>(nullableString, separator, new char[] { ' ' }, false, null, null);
        }

        /// <summary>
        /// Splits string instance on every comma sign. 
        /// Does trimming all the way. 
        /// Returns list if resuting parts which does not include empty or whitespace items.
        /// On any error returns null.
        /// </summary>
        /// <param name="nullableString"></param>
        /// <returns></returns>
        public static List<string> ParseCSV(this string nullableString)
        {
            return ParseCSV<string>(nullableString, new char[] { ',' }, new char[] { ' ' }, false, null, null);
        }

        public static List<T> ParseCSV<T>(this string nullableString, char[] separator, char[] trimChars, bool includeEmptyOrWhiteSpaceItems, List<T> nullCaseResult, List<T> errorCaseResult)
        {
            List<T> r = null;

            if (nullableString == null)
            {
                r = nullCaseResult;
            }
            else
            {
                try
                {
                    string v = nullableString;
                    if (trimChars != null)
                    {
                        v = v.Trim(trimChars);
                    }

                    string[] parts = nullableString.Split(separator, StringSplitOptions.None);
                    foreach (var part in parts)
                    {
                        string p = part;
                        if (trimChars != null)
                        {
                            p = p.Trim(trimChars);
                        }

                        if (!string.IsNullOrWhiteSpace(p) || includeEmptyOrWhiteSpaceItems)
                        {
                            if (r == null)
                            {
                                r = new List<T>();
                            }

                            T pAsT = (T)Convert.ChangeType((object)p, typeof(T));
                            r.Add(pAsT);
                        }
                    }
                }
                catch (Exception)
                {
                    r = errorCaseResult;
                }
            }

            return r;
        }

        /// <summary>
        /// Converts list of values to comma-separated-values (CSV) string. Empty and whitespace items are not included in resulting string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToCSV<T>(this List<T> list)
        {
            return ToCSV<T>(list, false);
        }

        /// <summary>
        /// Converts list of values to comma-separated-values (CSV) string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="includeEmptyOrWhiteSpaceItems"></param>
        /// <returns></returns>
        public static string ToCSV<T>(this List<T> list, bool includeEmptyOrWhiteSpaceItems)
        {
            return ToCSV<T>(list, ",", new char[] { ' ' }, includeEmptyOrWhiteSpaceItems, null, null);
        }

        /// <summary>
        /// Creates the CSV from a generic list.
        /// </summary>
        public static string ToCSV<T>(this List<T> list, string separator, char[] trimChars, bool includeEmptyOrWhiteSpaceItems, string nullCaseResult, string errorCaseResult)
        {
            string r = null;

            if (list == null)
            {
                r = nullCaseResult;
            }
            else
            {
                try
                {
                    StringBuilder sb = new StringBuilder(string.Empty);
                    foreach (var v in list)
                    {
                        string item = string.Format("{0}", v);

                        if (trimChars != null)
                        {
                            item = item.Trim(trimChars);
                        }

                        if (!item.IsNullOrWhiteSpace() ||
                        item.IsNullOrWhiteSpace() && includeEmptyOrWhiteSpaceItems)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(separator);
                            }
                            sb.Append(item);
                        }
                    }
                    r = sb.ToString();
                }
                catch (Exception)
                {
                    r = errorCaseResult;
                }
            }

            return r;
        }

        public static string FirstXChars(this string s, int count, string endingIfTrimmed)
        {
            string r = string.Empty;

            if (s.ToNonNullString().Length >= count)
            {

                r = s.ToNonNullString().Substring(0, count - endingIfTrimmed.ToNonNullString().Length).ToNonNullString() + endingIfTrimmed.ToNonNullString();
            }
            else
            {
                r = s.ToNonNullString();
            }

            return r;
        }

        /// <summary>
        /// Returns ending of string.
        /// If count is bigger than character count in string whole string is returned. 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="count"></param>
        /// <param name="startingPartIfTrimmed">Can be used as '...' to indicate that result was trimmed at the beginning.</param>
        /// <returns></returns>
        public static string LastXChars(this string s, int count, string startingPartIfTrimmed)
        {
            string r = s.ToNonNullString();
            startingPartIfTrimmed = startingPartIfTrimmed.ToNonNullString();

            if (r.Length >= count)
            {
                if (startingPartIfTrimmed.Length > count)
                {
                    throw new ArgumentException("Length of startingPartIfTrimmed must be smaller than count.");
                }
                r = r.Substring(r.Length - count, count);
                r = r.Remove(0, startingPartIfTrimmed.Length);
                r = startingPartIfTrimmed + r;
            }

            return r;
        }

        public static string GetSubstringAfter(this string s, string startMarker)
        {
            string r = null;

            if (s != null && !string.IsNullOrEmpty(startMarker))
            {
                int startMarkerIndex = s.IndexOf(startMarker);
                int endMarkerIndex = s.Length;

                if (startMarkerIndex >= 0 && endMarkerIndex >= 0)
                {
                    startMarkerIndex = startMarkerIndex + startMarker.Length;

                    r = s.Substring(startMarkerIndex, endMarkerIndex - startMarkerIndex);
                }
            }

            return r;
        }

        public static string GetSubstringAfterLastOccurence(this string s, string marker)
        {
            string r = null;

            if (s != null && !string.IsNullOrEmpty(marker))
            {
                int startMarkerIndex = s.LastIndexOf(marker);
                int endMarkerIndex = s.Length;

                if (startMarkerIndex >= 0 && endMarkerIndex >= 0)
                {
                    startMarkerIndex = startMarkerIndex + marker.Length;

                    r = s.Substring(startMarkerIndex, endMarkerIndex - startMarkerIndex);
                }
            }

            return r;
        }

        public static string GetSubstring(this string s, string startMarker, string endMarker)
        {
            string r = null;

            if (s != null && !string.IsNullOrEmpty(startMarker) && !string.IsNullOrEmpty(endMarker))
            {
                int startMarkerIndex = s.IndexOf(startMarker);
                int endMarkerIndex = s.IndexOf(endMarker);

                if (startMarkerIndex >= 0 && endMarkerIndex >= 0)
                {
                    startMarkerIndex = startMarkerIndex + startMarker.Length;

                    r = s.Substring(startMarkerIndex, endMarkerIndex - startMarkerIndex);
                }
            }

            return r;
        }

        public static string RemoveSubstring(this string s, string startMarker, string endMarker)
        {
            string r = null;

            if (s != null && !string.IsNullOrEmpty(startMarker) && !string.IsNullOrEmpty(endMarker))
            {
                int startMarkerIndex = s.IndexOf(startMarker);
                int endMarkerIndex = s.IndexOf(endMarker);

                if (startMarkerIndex >= 0 && endMarkerIndex >= 0)
                {
                    startMarkerIndex = startMarkerIndex + startMarker.Length;

                    r = s.Remove(startMarkerIndex, endMarkerIndex - startMarkerIndex);
                }
            }

            return r;
        }

        /// <summary>
        /// Gets Description from enum value ie [Description("MasterCard")] returns MasterCard
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Description(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static int GetValueFromEnumDescription(string description, Type enumType)
        {
            foreach (Enum value in enumType.GetEnumValues())
            {
                if (value.Description() == description)
                {
                    return Int32.Parse(value.ToString("d"));
                }
            }
            throw new Exception("Description isn't defined! desc: " + description);
        }

        public static string GetDigitsAfterDecimalPoint(this decimal n, int numberOfDigits = 2)
        {
            int result = Decimal.ToInt32((Decimal.Round(n, 2) - Decimal.Floor(n) )* 100);
            return result > 9 ? result.ToString() : "0" + result;
        }

        public static string GetDigitsBeforeDecimalPoint(this decimal n)
        {
            //need to floor n first?
            return Decimal.ToInt32(n).ToString();
        }

        public static string RemoveSpaces(this string s)
        {
            if (s == null)
            {
                return s;
            }
            else
            {
                return s.Replace(" ", "");
            }
        }

        public static List<string> DistinctStrings(this List<string> items)
        {
            var r = new List<string>();
            foreach (var item in items)
            {
                if(!r.Contains(item))
                {
                    r.Add(item);
                }
            }
            return r;
        }

        public static string PrepandHiddenHtmlZeros(this string number, int length)
        {
            string r = number;
            string zeros = string.Empty;

            while (r.Length + zeros.Length < length)
            {
                zeros += " ";
            }
            //zeros = string.Format("<span style=\"visibility:hidden\">{0}</span>", zeros);
            return zeros + number;
        }

        public static bool IsAlphaNumeric(this string strToCheck)
        {
            Regex objAlphaNumericPattern = new Regex("[^a-zA-Z0-9]");
            return !objAlphaNumericPattern.IsMatch(strToCheck);
        }

        /// <summary>
        /// String length check is performed. I.e. “test”. SafeLeftSubstring(50) returns “test”
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string SafeLeftSubstring(this String data, int length)
        {
            return String.IsNullOrEmpty(data) ? data : data.Substring(0, data.Length > length ? length : data.Length);
        }

        public static string ToProperStringDisplayValue(this decimal value, int decimalPlaces = 2)
        {
            var d = (double) value;

            d = d * Math.Pow(10, decimalPlaces);
            d = Math.Truncate(d);
            d = d / Math.Pow(10, decimalPlaces);

            return string.Format("{0:N" + Math.Abs(decimalPlaces) + "}", d);
        }

        /// <summary>
        /// Gets property name from expression without magic strings. 
        /// Examle: ExtenderClass.GetPropertyName(()=>Model.AdvertisementImage)  
        /// "Extended" expressions such model.Propery.PropertyA are not supported.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(System.Linq.Expressions.Expression<Func<T>> expression)
        {
            var body = expression.Body as System.Linq.Expressions.MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("'expression' should be a member expression");
            }

            return body.Member.Name;
        }
    }
}
