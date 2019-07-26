using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleWix
{
    public static class StringExtensions
    {
        /// <summary>
        /// Substring but let's you enter a negative start index. 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SubstringPlus (this string s, int startIndex, int length)
        {
            if (startIndex >= 0)
                return s.Substring(startIndex, length);
            else
            {
                int posIndex = s.Length + startIndex;
                return s.Substring(posIndex, length);
            }
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return String.IsNullOrEmpty(s);
        }


        public static string JsonPrettify(this string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }

        public static string MakeFileName(this string s)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            var newName = String.Join("_", s.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            return newName;
        }

        public static string MakeFolderName(this string s)
        {
            var invalids = System.IO.Path.GetInvalidPathChars();
            var newName = String.Join("_", s.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            return newName;
        }
        public static string GetMD5Hash(this string input)
        {
            // byte array representation of that string
            byte[] encodedPassword = new UTF8Encoding().GetBytes(input);

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

            // string representation (similar to UNIX format)
            string encoded = BitConverter.ToString(hash).Replace("-", string.Empty);// without dashes

            return encoded;
        }
        public static bool ValidateVersion(this string version)
        {
            var match = System.Text.RegularExpressions.Regex.Match(version, @"^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)\.(?<revision>\d+)$");
            if (match.Success)
            {
                int component;
                if (int.TryParse(match.Groups["major"].Value, out component) &&
                  int.TryParse(match.Groups["minor"].Value, out component) &&
                  int.TryParse(match.Groups["build"].Value, out component) &&
                  int.TryParse(match.Groups["revision"].Value, out component))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
