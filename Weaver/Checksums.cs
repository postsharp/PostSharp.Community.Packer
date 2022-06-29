using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PostSharp.Community.Packer.Weaver
{
    public class Checksums
    {
        private readonly Dictionary<string, string> checksums =
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> AllChecksums
            => checksums;

        public static string CalculateChecksum(string filename)
        {
            var result = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(filename)) return result;
                if (!File.Exists(filename)) return result;

                using var fs = new FileStream(
                    filename, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite
                );
                result = CalculateChecksum(fs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                result = string.Empty;
            }

            return result;
        }

        public static string CalculateChecksum(Stream stream)
        {
            var result = string.Empty;

            if (stream == null) return result;

            try
            {
                using var bs = new BufferedStream(stream);
                using var sha1 = new SHA1CryptoServiceProvider();
                var hash = sha1.ComputeHash(bs);
                var formatted = new StringBuilder(2 * hash.Length);
                foreach (var b in hash) formatted.AppendFormat("{0:X2}", b);

                result = formatted.ToString();
            }
            catch (Exception ex)
            {
                // dump all the exception info to the log
                Console.WriteLine(ex);

                result = string.Empty;
            }

            return result;
        }

        public void Add(string resourceName, string checksum)
            => checksums.Add(resourceName, checksum);

        public bool ContainsKey(string resourceName)
            => checksums.ContainsKey(resourceName);
    }
}