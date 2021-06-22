using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace Subtitle_Synchronizer
{
    public static class MyExtensionMethods
    {
        public static int maxNumberOfLinesMatched(this ConcurrentQueue<permutationObject> allPermObjs)
        {
            if (allPermObjs.Count < 1)
                return 0;

            int maximum = allPermObjs.ElementAt(0).numberOfLinesOfBestMatched;

            for (int i = 0; i < allPermObjs.Count; i++)
            {
                if (allPermObjs.ElementAt(i).numberOfLinesOfBestMatched > maximum)
                    maximum = allPermObjs.ElementAt(i).numberOfLinesOfBestMatched;
            }
            return maximum;
        }
        public static int maxNumberOfLinesOfMatched(this List<PermutationMethodsObject> allPermMethObj)
        {
            if (allPermMethObj.Count < 1)
                return 0;

            int maximum = allPermMethObj[0].numberOfLinesOfBestMatched;

            for (int i = 0; i < allPermMethObj.Count; i++)
            {
                if (allPermMethObj[i].numberOfLinesOfBestMatched > maximum)
                    maximum = allPermMethObj[i].numberOfLinesOfBestMatched;
            }
            return maximum;
        }
        public static int indexOfLowestDistance(this  List<wordPermutationLinePair> allLinePairs)
        {
            List<int> allDistances = new List<int>();

            foreach (wordPermutationLinePair pair in allLinePairs)
            {
                allDistances.Add(pair.totaldistanceBetweenAllLinesAndOriginals);
            }

            return allDistances.indexOfMinimumValue();
        }

        //only works for ordered complete lists, dont use anywhere else, very hacky
        public static int indexOfLowestDistance(this List<PermutationMethodsObject> allPermutationMethodsObjects)
        {
            List<int> allDistances = new List<int>();

            int maxNumberOfLines = 0;
            foreach (PermutationMethodsObject perMethObj in allPermutationMethodsObjects)
            {
                if (perMethObj.numberOfLinesOfBestMatched > maxNumberOfLines)
                    maxNumberOfLines = perMethObj.numberOfLinesOfBestMatched;
            }

            for(int i = 0; i < allPermutationMethodsObjects.Count ; i++)
            {
                int distance = 0;
                if(allPermutationMethodsObjects[i].numberOfLinesOfBestMatched < maxNumberOfLines)
                {
                    for (int j = allPermutationMethodsObjects[i].numberOfLinesOfBestMatched - 1; j < maxNumberOfLines - 1; j++)
                    {
                        if (j < 0)
                            break;
                        distance += allPermutationMethodsObjects[j].distanceToOriginalOfSecondBeforePermutations;
                    }
                }
                distance += allPermutationMethodsObjects[i].bestMatchedTotalDistance;
                allDistances.Add(distance);
            }
            return allDistances.indexOfMinimumValue();
        }

        public static int indexOfMinimumValue(this List<int> intList)
        {
            if (intList.Count < 1)
                return -1;

            int smallest = intList[0];
            int index = 0;

            for (int i = 1; i < intList.Count; i++)
            {
                if (intList[i] < smallest)
                {
                    smallest = intList[i];
                    index = i;
                }
            }
            return index;
        }

        /// <summary>
        /// Normalizes a int list, so that all values are in relation to baseToNormalize to. For example, if the max value in a list is
        /// 5000, and baseToNormalizeTo is 20, it will turn 5000 into 20 and so on
        /// </summary>
        /// <param name="listToNormalize"></param>
        /// <param name="baseToNormalizeTo"></param>
        /// <returns></returns>
        public static List<int> normalizeIntList(this List<int> listToNormalize, int baseToNormalizeTo)
        {
            List<int> normalizedList = new List<int>();

            double max = Convert.ToDouble(listToNormalize.Max());

            for (int i = 0; i < listToNormalize.Count; i++)
            {
                normalizedList.Add(Convert.ToInt32(Convert.ToDouble(listToNormalize[i]) * Convert.ToDouble(baseToNormalizeTo) / max));
            }
            return normalizedList;
        }

        public static List<double> firstDerivative(this List<double> intList, double dX)
        {
            List<double> firstDerivativeFunction = new List<double>();

            for (int i = 1; i < intList.Count; i++)
            {
                double pontualDerivative = (intList[i] - intList[i - 1]) / dX;
                firstDerivativeFunction.Add(pontualDerivative);
            }

            firstDerivativeFunction.Add(0.0);

            return firstDerivativeFunction;
        }

        public static string removeActorNameFromBeginningOfString(this string s, List<string> listOfActors)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string actorName in listOfActors)
            {
                if (s.IndexOf(actorName) == 0)
                {
                    if (actorName.Length >= s.Length)
                        return string.Empty;
                    sb.Append(s.Substring(actorName.Length));
                    return sb.ToString();
                }
            }
            return s;
        }

        public static string keepOnlyNumbersLettersSpacesAndFullStops(this string s)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in s)
            {
                if (isValidCharacter(c))
                    sb.Append(c);
                else
                    if (c == '\n')
                        sb.Append(' ');
            }
            return sb.ToString();
        }

        private static bool isValidCharacter(char c)
        {
            return (c >= '0' && c <= '9')
                || (c >= 'A' && c <= 'z')
                    || c == '.' || c == ' ';
        }
        public static int numberOfParagraphs(this string s)
        {
            s = s.eliminateDuplicatedNewLines();

            if (s.Length < 1)
                return 0;

            int numberOfParagraphsAlreadyInTheLine = s.Length - s.Replace("\n", "").Length;

            if (s[s.Length - 1] == '\n')
                numberOfParagraphsAlreadyInTheLine--;

            if (s[0] == '\n')
                numberOfParagraphsAlreadyInTheLine--;

            return numberOfParagraphsAlreadyInTheLine + 1;

        }

        public static string eliminateDuplicatedNewLines(this string s)
        {
            while (s.IndexOf("\n\n") != -1)
            {
                s = s.Replace("\n\n", "\n");
            }
            return s;
        }

        public static int LenghtWithoutSpecialCharacters(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;

            int lenght = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsLetterOrDigit(s, i) || char.IsWhiteSpace(s, i))
                {
                    lenght++;
                }
            }

            return lenght;
        }
        public static string cutStringWordStart(this string s, int centerIndex, int charactersOffset)
        {

            Debug.Assert(centerIndex >= 0, "centerIndex is less than zero dafuq");
            if (centerIndex < 0)
                return string.Empty;
            Debug.Assert(centerIndex <= s.Length - 1, "CenterIndex is bigger than it should...");
            if (centerIndex >= s.Length - 1)
                return string.Empty;

            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] terminationCharacters = new char[] { '\n', '\t', ' ', '\r' };

            int startIndex = s.LastIndexOfAny(terminationCharacters, centerIndex);

            if (startIndex == -1)
                return string.Empty;

            if (startIndex == -1 || startIndex + charactersOffset > s.Length)
                return s;

            if (startIndex + charactersOffset <= 0)
                return String.Empty;

            return s.Substring(0, startIndex + 1 + charactersOffset); ;
        }
        public static string cutStringWordEnd(this string s, int centerIndex, int charactersOffset)
        {

            if (string.IsNullOrEmpty(s))
                return string.Empty;

            if (centerIndex < 0 || centerIndex >= s.Length - 1)
                return string.Empty;

            char[] terminationCharacters = new char[] { '\n', '\t', ' ', '\r' };

            int endIndex = s.IndexOfAny(terminationCharacters, centerIndex);

            // if we are over the end of the string
            if (endIndex == -1 || endIndex + charactersOffset > s.Length)
                return s;

            if (endIndex + charactersOffset <= 0)
                return String.Empty;

            //Cut at the nearest word of the center index, rounded to the front of the index
            return s.Substring(0, endIndex + charactersOffset);
        }

        public static int findFullWord(this string s, int startIndex, int lengthToSearch, string wordToFind)
        {
            if (lengthToSearch + startIndex > s.Length)
                lengthToSearch = s.Length - startIndex;

            if (lengthToSearch < 0 || startIndex >= s.Length)
                return -1;
            string escapedWordToFind = Regex.Escape(wordToFind);
            string regexExpression = @"(?i)\b" + escapedWordToFind + @"\b";
            Regex r = new Regex(regexExpression);

            var match = r.Match(s, startIndex, lengthToSearch);

            if (match.Success)
            {
                return match.Index;
            }
            return -1;
        }

        public static int indexOfStartOfLastWord(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;

            List<char> terminationCharacters = new List<char> { '\n', '\t', ' ', '\r' };
            int i = s.Length - 1;

            //go back in white space until a word is found
            while (i >= 0 && i < s.Length && terminationCharacters.IndexOf(s[i]) != -1)
                i--;

            return 1 + s.Substring(0, i + 1).LastIndexOfAny(terminationCharacters.ToArray());
        }

        public static int indexOfEndOfFirstWord(this string s)
        {
            List<char> terminationCharacters = new List<char> { '\n', '\t', ' ', '\r' };
            int i = 0;

            if (string.IsNullOrEmpty(s))
                return 0;

            //go forward in white space until word is found
            while (i >= 0 && i < s.Length && terminationCharacters.IndexOf(s[i]) != -1)
                i++;

            int endIndex = s.IndexOfAny(terminationCharacters.ToArray(), i);

            if (endIndex == -1)
                return s.Length - 1;

            return endIndex - 1;
        }

        public static string firstWord(this string s)
        {
            return s.Substring(0, s.indexOfEndOfFirstWord() + 1);
        }

        public static string lastWordAndWhiteSpaceAfter(this string s)
        {
            return s.Substring(s.indexOfStartOfLastWord());
        }
        /// <summary>
        /// Returns the word it by centerIndex
        /// </summary>
        /// <param name="s"></param>
        /// <param name="centerIndex"></param>
        /// <param name="wordOffset"></param>
        /// <returns></returns>
        public static string wordAtIndex(this string s, int centerIndex, int wordOffset)
        {
            string temp = String.Empty;
            List<char> terminationCharacters = new List<char> { '\n', '\t', ' ', '\r' };

            string foundWord = String.Empty;

            int i = centerIndex;
            int wordOffsetCounter = 0;

            int j = 0;

            if (i >= 0 && i < s.Length && terminationCharacters.IndexOf(s[i]) != -1)
                foundWord = String.Empty;

            if (wordOffset < 0)
            {
                while (wordOffsetCounter <= wordOffset)
                {
                    j = i;

                    while (i >= 0 && i < s.Length && terminationCharacters.IndexOf(s[i]) == -1)
                        i--;
                    wordOffsetCounter--;
                }
            }

            while (j >= 0 && j < s.Length && terminationCharacters.IndexOf(s[j]) == -1)
                j++;

            if (i < 0)
                i = 0;
            else if (i > s.Length)
                i = s.Length;

            if (j < 0)
                j = 0;
            else if (j > s.Length)
                j = s.Length;

            foundWord = s.Substring(i, j - i + 1);

            return foundWord;
        }

        /// <summary>
        /// return a string that is offseted by wordOffset 
        /// number of words from the startIndex
        /// </summary>
        public static string offSetWords(this string s, int startIndex, int wordOffset)
        {
            string result = s;
            int endIndex;

            char[] terminationCharacters = new char[] { '\n', '\t', ' ', '\r' };

            int absWordOffset = Math.Abs(wordOffset);

            for (int i = 0; i < absWordOffset; i++)
            {
                if (result.Length < s.Length && wordOffset > 0)
                {
                    endIndex = s.IndexOfAny(terminationCharacters, result.Length + 1);
                    if (endIndex == -1)
                    {
                        endIndex = s.Length - 1;
                        i = absWordOffset; //to break later
                    }
                    result = s.Substring(0, endIndex);
                }
                else if (result.Length >= 1 && wordOffset < 0)
                {
                    endIndex = s.LastIndexOfAny(terminationCharacters, result.Length - 1);

                    if (endIndex == -1)
                        return "";

                    result = s.Substring(0, endIndex);
                }
            }
            return result;
        }
        /// <summary>
        /// Returns a string that is cut at centerIndex, that can be offset by
        /// <para>wordOffset number of words. If cutAtStartOfWord is true,</para>
        /// <para>the cut will be performed the start of the word hit. Else, at the end.</para>
        /// </summary>
        public static string cutStringAtWord
            (this string s, int centerIndex, int charactersOffset, int wordOffset, bool cutAtStartOfWord)
        {
            string result;

            if (cutAtStartOfWord)
                result = s.cutStringWordStart(centerIndex, charactersOffset);
            else
                result = s.cutStringWordEnd(centerIndex, charactersOffset);

            if (wordOffset != 0)
                return result.offSetWords(centerIndex, wordOffset);

            return result;
        }
    }
}
