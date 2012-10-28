using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageImporter
{
    /// <summary>
    /// Staticka trieda s funkciami
    /// </summary>
    public static class Functions
    {
        public static string ExtractFileName(string pathAndName)
        {
            if (!pathAndName.Contains('\\'))
                return pathAndName;

            return pathAndName.Substring(pathAndName.LastIndexOf('\\') + 1);
        }
    }
}
