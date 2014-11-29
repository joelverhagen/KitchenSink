using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapcode.KitchenSink.Support
{
    public static class Base64Url
    {
        private const char PaddingCharacter = '=';

        private static readonly IDictionary<char, char> DecodeDictionary = new Dictionary<char, char>
        {
            {'-', '+'},
            {'_', '/'}
        };

        private static readonly IDictionary<char, char> EncodeDictionary;

        static Base64Url()
        {
            EncodeDictionary = DecodeDictionary.ToDictionary(p => p.Value, p => p.Key);
        }

        public static byte[] Decode(string base64)
        {
            Guard.ArgumentNotNull(base64, "base64");

            base64 = base64.TrimEnd(PaddingCharacter);
            base64 = Map(base64, DecodeDictionary);
            while (base64.Length%4 != 0)
            {
                base64 += PaddingCharacter;
            }

            return Convert.FromBase64String(base64);
        }

        public static string Encode(byte[] bytes)
        {
            Guard.ArgumentNotNull(bytes, "bytes");

            string base64 = Convert.ToBase64String(bytes);
            base64 = Map(base64, EncodeDictionary);
            base64 = base64.Trim(PaddingCharacter);
            return base64;
        }

        private static string Map(string base64, IDictionary<char, char> dictionary)
        {
            var builder = new StringBuilder(base64.Length);
            foreach (char existingC in base64)
            {
                char mappedC;
                builder.Append(dictionary.TryGetValue(existingC, out mappedC) ? mappedC : existingC);
            }

            return builder.ToString();
        }
    }
}