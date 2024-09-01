using System;
using System.Linq;

namespace ASP_P15.Services
{
    public class FileNameGeneratorService : IFileNameGeneratorService
    {
        private static readonly char[] AllowedCharacters =
            "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        public string GenerateFileName(int length)
        {
            var random = new Random();
            return new string(Enumerable.Repeat(AllowedCharacters, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
