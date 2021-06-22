using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Subtitle_Synchronizer
{
    static class phrasesGeneralMethods
    {
        public static string regexExpression = @"[^\.\?\!]+[\.\?\!]+[^a-zA-Z0-9-\s]*";

        public static string paragraphRegexExpression = @"\s*\n*[^\n]+\n*";

     //   public static string paragraphRegexExpression = @".";
        public static void putLastDivisionOfs1InBeginningOfs2(ref string s1, ref string s2, string regexDivisionsExpression)
        {
            if (string.IsNullOrEmpty(s1))
                return;

            if (string.IsNullOrEmpty(s2))
                s2 = string.Empty;

            var regexDivisions = Regex.Matches(s1, regexDivisionsExpression);

            int indexOfLastRegex = regexDivisions.Count - 1;
            string lastDivision = string.Empty;

            if (regexDivisions.Count > 0)
            {
                s1 = s1.Substring(0, regexDivisions[indexOfLastRegex].Index);
                lastDivision = regexDivisions[indexOfLastRegex].Value;

                s2 = String.Concat(s2 + lastDivision);
            }
        }

        public static string getFirstLineCompletePhrase(string firstLine)
        {
            string firstLineCompletePhrase = string.Empty;

            var firstLineRegexMatches = Regex.Matches(firstLine, regexExpression);
            foreach (Match mc in firstLineRegexMatches)
            {
                firstLineCompletePhrase += mc.Value;
            }

            return firstLineCompletePhrase;
        }
        public static string getFirstLineIncompletePhrase(string firstLine)
        {
            string firstLineCompletePhrase = getFirstLineCompletePhrase(firstLine);
            string firstLineIncompletePhrase = string.Empty;

            if (firstLineCompletePhrase.Length < firstLine.Length)
                firstLineIncompletePhrase = firstLine.Substring(firstLineCompletePhrase.Length);

            return firstLineIncompletePhrase;
        }
    }
}
