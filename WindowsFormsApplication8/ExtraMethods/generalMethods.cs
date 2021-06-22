using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Subtitle_Synchronizer
{
    static class generalMethods
    {
        public static int roundINTToTen(int intToRound)
        {
            double timeInDouble = Convert.ToDouble(intToRound);
            timeInDouble = timeInDouble / 10;
            timeInDouble = Math.Round(timeInDouble);
            return Convert.ToInt32(timeInDouble * 10.0);
        }

        public static string removeAllParenthesis(string s)
        {
            s = s.Replace("\r\n", "\n");
            s = s.Replace("\r", "\n"); //for mac linebreaks

            StringBuilder sb = new StringBuilder();
            int parBegIndex = 0;
            int parEndIndex = 0;
            for (int i = 0; i < s.Length; i++)
            {
                bool weHadParentheses = false;
                int begParIndex = 0;
                if (s[i] == '(')
                {
                    begParIndex = i;
                    while (i < s.Length && s[i] != ')')
                        i++;
                    i++;
                    weHadParentheses = true;
                }

                //remove whitespace
                if (weHadParentheses && i < s.Length)
                {
                    if (i > 0 && sb.Length > 0 && char.IsWhiteSpace(sb[sb.Length - 1]))
                    {
                        if (s[i] == '\n')
                            sb.Remove(sb.Length - 1, 1);
                        else
                            continue;
                    }
                }
                if (i < s.Length)
                    sb.Append(s[i]);
            }
            return sb.ToString();
        }

        public static double period(int frequency)
        {
            return 1 / Convert.ToDouble(frequency);
        }
        public static subLine joinTwoSublines(subLine subLine1, subLine subLine2)
        {
            subLine joinedSub = new subLine();

            joinedSub.lineContent = subLine1.lineContent + subLine2.lineContent;
            joinedSub.lineIndex = subLine1.lineIndex;
            joinedSub.begTime = subLine1.begTime;
            joinedSub.endTime = subLine2.endTime;

            return joinedSub;
        }

        public static double differencesnessBetweenValues(List<int> values)
        {
            int sumOfValues = 0;
            foreach (int val in values)
                sumOfValues += val;

            double average = Convert.ToDouble(sumOfValues) / Convert.ToDouble(values.Count);

            double sumOfDifferences = 0.0;
            foreach (int val in values)
                sumOfDifferences += Math.Abs(Convert.ToDouble(val) - average);

            return sumOfDifferences / Convert.ToDouble(sumOfValues);
        }

        /// <summary>
        /// Returns the duration of time of s relative to the average characters per second of sl
        /// </summary>
        /// <param name="s"></param>
        /// <param name="sl"></param>
        /// <returns>
        /// </returns>
        ///
        public static int durationOf_s_InRelationTo_sl(string s, subLine sl)
        {

            return (sl.totalMiliseconds * s.LenghtWithoutSpecialCharacters())
                        / sl.lineContent.LenghtWithoutSpecialCharacters();
        }

        public static void ifNullTurnEmpty(ref string s)
        {
            if (String.IsNullOrEmpty(s))
                s = string.Empty;
        }

        public static bool s1IsLessThanHalfOfS1PLUSS2(string s1, string s2)
        {
            return 0.5f > ((float)s1.Length / (float)(s1.Length + s2.Length));
        }

        public static double averageMilisecondsPerCharacter(List<subLine> subLineList)
        {
            bool assertCondition = (subLineList.Count == 0);
            if (assertCondition)
            { Debug.Assert(assertCondition, "subLineList shouldn't be empty!"); return -1; }

            int sumMili = 0;
            int sumCharacters = 0;

            foreach (subLine sl in subLineList)
            {
                sumMili += sl.endTime.timeInMilisec - sl.begTime.timeInMilisec;
                sumCharacters += sl.lineContent.Length;
            }

            return (sumCharacters == 0) ? Convert.ToDouble(sumMili) : Convert.ToDouble(sumMili) / Convert.ToDouble(sumCharacters);
        }

        #region WordsStuff
        //returns true if the word that starts at wordIndex is the last one in string s
        public static bool wordAtIndexIsLastInString(int wordIndex, string s)
        {
            //Begin method
            if (wordAtIndexLength(wordIndex, s) + wordIndex == s.TrimEnd().Length)
                return true;
            return false;
        }

        //returns the Length of the word that starts at wordIndex, in string s
        public static int wordAtIndexLength(int wordIndex, string s)
        {
            //Check for method input errors
            bool assertCondition;

            assertCondition = (wordIndex < 0 || wordIndex > s.Length - 1);
            if (assertCondition)
            { Debug.Assert(assertCondition, "wordIndex is invalid"); return 0; }

            assertCondition = string.IsNullOrEmpty(s);
            if (assertCondition)
            { Debug.Assert(assertCondition, "Input string s shouldn't be null or empty"); return 0; }

            //Method
            char[] terminationCharacters = new char[] { '\n', '\t', ' ', '\r' };

            int terminationIndex = s.IndexOfAny(terminationCharacters, wordIndex);

            if (-1 == terminationIndex)
                return s.Length - wordIndex;
            return terminationIndex - wordIndex - 1;
        }

        #endregion

        public static permutationObject findBestMatchedSubLinesGrossly
            (List<subLine> allUnfixedSublines, allFixedSubtitles allProvisorySubtitles, int currentIndex,
            string transcript)
        {
            permutationObject permutationObject = new permutationObject();

            permutationObject.setAllProvisorySubtitlesStringsFromFixedSubs(allProvisorySubtitles);

            permutationObject.startIndex = currentIndex;

            permutationObject.grossFindBestMatchedStartingFromIndex(currentIndex, allUnfixedSublines);

            return permutationObject;
        }

        public static permutationObject findBestMatchedSubLines
               (List<subLine> allUnfixedSublines, allFixedSubtitles allProvisorySubtitles, int currentIndex,
            string transcript)
        {
            ConcurrentQueue<permutationObject> basePermutationObjectsQueue = new ConcurrentQueue<permutationObject>();

            for (int i = 0; i < 2; i++)
                basePermutationObjectsQueue.Enqueue(new permutationObject());
            Parallel.For(0, 2, i =>
          //  for (int i = 0; i < 2; i++)
            {
                basePermutationObjectsQueue.ElementAt(i).goesBackwards = true;
                basePermutationObjectsQueue.ElementAt(i).isInvaded = (i == 0) ? true : false;

                basePermutationObjectsQueue.ElementAt(i).
                    setAllProvisorySubtitlesStringsFromFixedSubs(allProvisorySubtitles);

                basePermutationObjectsQueue.ElementAt(i).startIndex = currentIndex;
                  });
         //   }
           // foreach (permutationObject permObj in basePermutationObjectsQueue)
             Parallel.ForEach(basePermutationObjectsQueue, permObj =>
            {
                // Parallel.For(0, 2, i =>
                //  {
                //      basePermutationObjectsQueue.ElementAt(i).findBestMatchedStartingFromIndex(currentIndex, allUnfixedSublines);
                permObj.fineFindBestMatchedStartingFromIndex(currentIndex, allUnfixedSublines);
                //    });
           // }
                    });

            ConcurrentQueue<permutationObject> levelTwoPermutationObjectsQueue = new ConcurrentQueue<permutationObject>();

            for (int i = 0; i < 4; i++)
                levelTwoPermutationObjectsQueue.Enqueue(new permutationObject());
             Parallel.For(0, 4, i =>
           // for (int i = 0; i < 4; i++)
            {
                levelTwoPermutationObjectsQueue.ElementAt(i).goesBackwards = false;
                levelTwoPermutationObjectsQueue.ElementAt(i).isInvaded = (i % 2 == 0) ? true : false;

                levelTwoPermutationObjectsQueue.ElementAt(i).
                    setAllProvisorySubtitlesStringsFromStringList(
                    basePermutationObjectsQueue.ElementAt(i / 2).allNewSubitlesStrings());

                //    levelTwoPermutationObjectsQueue.ElementAt(i).distanceBetweenNewSubLinesAndUnfixed =
                //     basePermutationObjectsQueue.ElementAt(i / 2).distanceBetweenNewSubLinesAndUnfixed;

                levelTwoPermutationObjectsQueue.ElementAt(i).startIndex = currentIndex;
                 });
          //  }

            Parallel.ForEach(levelTwoPermutationObjectsQueue, permObj =>
              {
            //   Parallel.For(0, 4, i =>

          //  foreach (permutationObject permObj in levelTwoPermutationObjectsQueue)
                //  levelTwoPermutationObjectsQueue.ElementAt(i).findBestMatchedStartingFromIndex(currentIndex, allUnfixedSublines);
                permObj.fineFindBestMatchedStartingFromIndex(currentIndex, allUnfixedSublines);
            //});
                });

            int minimumDistanceIndex = indexOfMinimumDistanceQueue(
               levelTwoPermutationObjectsQueue, allProvisorySubtitles.fixedSubtitlesLines, currentIndex);

            return levelTwoPermutationObjectsQueue.ElementAt(minimumDistanceIndex);
        }

        public static int indexOfMinimumDistanceQueue
    (ConcurrentQueue<permutationObject> permutationObjectsQueue, List<subLine> allProvisorySublines, int indexToStart)
        {
            if (permutationObjectsQueue.Count < 1 || allProvisorySublines.Count < 1
                || indexToStart < 0 || indexToStart >= allProvisorySublines.Count)
                return 0;

            List<int> allTotalDistances = new List<int>();

            int maximumNumberOfNewStrings = permutationObjectsQueue.maxNumberOfLinesMatched();

            foreach (permutationObject permObj in permutationObjectsQueue)
            {
                allTotalDistances.Add(permObj.totalDistanceBetweenNewSublinesAndUnfixed);

                for (int j = permObj.numberOfLinesOfBestMatched; j < maximumNumberOfNewStrings; j++)
                {
                    if (permObj.goesBackwards &&
                        indexToStart - j >= 0 &&
                        indexToStart - j < allProvisorySublines.Count)
                        allTotalDistances[allTotalDistances.Count - 1] +=
                            allProvisorySublines[indexToStart - j].distanceBetweenThisLineAndTheOriginalRespectiveOne;

                    else if (indexToStart + j < allProvisorySublines.Count &&
                        indexToStart + j < allProvisorySublines.Count)
                        allTotalDistances[allTotalDistances.Count - 1] +=
                            allProvisorySublines[indexToStart + j].distanceBetweenThisLineAndTheOriginalRespectiveOne;
                }
            }
            return allTotalDistances.indexOfMinimumValue();
        }

        public static int indexOfMinimumDistanceOfBestMatchedMinDistance
            (List<PermutationMethodsObject> allPermMethObj, List<subLine> originalSublines, int indexToStart, bool goesBackwards)
        {
            if (allPermMethObj.Count < 1 || originalSublines.Count < 1
                || indexToStart < 0 || indexToStart >= originalSublines.Count)
                return 0;

            List<int> allTotalDistances = new List<int>();

            int maximumNumberOfNewStrings = allPermMethObj.maxNumberOfLinesOfMatched();
            for (int i = 0; i < allPermMethObj.Count(); i++)
            {
                allTotalDistances.Add(allPermMethObj[i].bestMatchedTotalDistance);

                int nOfLinesAlreadyIncludedInBestMatchedTotalDistance =
                    allPermMethObj[i].numberOfLinesOfBestMatched;

                for (int j = nOfLinesAlreadyIncludedInBestMatchedTotalDistance; j <= maximumNumberOfNewStrings; j++)
                {
                    if (goesBackwards && indexToStart - j >= 0 && indexToStart - j < originalSublines.Count)
                        allTotalDistances[i] += originalSublines[indexToStart - j].distanceBetweenThisLineAndTheOriginalRespectiveOne;
                    else if (indexToStart + j < originalSublines.Count && indexToStart + j < originalSublines.Count)
                        allTotalDistances[i] += originalSublines[indexToStart + j].distanceBetweenThisLineAndTheOriginalRespectiveOne;
                }
            }
            return allTotalDistances.indexOfMinimumValue();
        }

        /// <summary>
        /// //////////////////////////REMOVEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
        /// </summary>
        /// <param name="transcript"></param>
        /// <param name="unfixedSubLine"></param>
        /// <param name="cutSteps"></param>
        /// <param name="averageTranscriptPosition"></param>
        /// <returns></returns>
        public static string bestMatchedSubstringWithAnchors
            (string transcript, subLine unfixedSubLine, int cutSteps, int averageTranscriptPosition)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string s = String.Empty;

            int i = 0, j = 0;

            int localTranscriptPosition = averageTranscriptPosition;

            int numberOfAnchorsInLine = unfixedSubLine.wordAnchors.Count;

            foreach (wordAnchor wA in unfixedSubLine.wordAnchors)
            {
                //get the string part before the anchor
                s = transcript.cutStringAtWord(wA.TransIndex, 0, 0, true);

                //since wordanchor.subindex doesnt account for the cases when word has other characters than letters before it:
                int indexOfRealStartOfWordanchorWord = s.Length;

                s = s + transcript.Substring(indexOfRealStartOfWordanchorWord).cutStringAtWord(
                    0, 0, 0, false);

                //remove the beginning of the string, that doesn't belong to this line
                if (s.Length >= localTranscriptPosition)
                    s = s.Remove(0, localTranscriptPosition);
                else
                    s = String.Empty;

                sb.Append(s);
                s = sb.ToString();
                localTranscriptPosition = averageTranscriptPosition + s.Length;

                j++;
            }
            i++;

            return sb.ToString();
        }
        #region LEVESHTEIN STUFF

        //Leveshtein stuff

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int minimumOf(ref int b, ref int a, ref int c)
        {
            if (a < b && a < c)
                return a;
            if (a < b)
                return c;
            if (b < c)
                return b;
            return c;
        }

        public static int levenshteinDistance(string hori, string verti)
        {
            int i, j, jMOne, iMOne, horiLen, vertiLen;

            if (String.IsNullOrEmpty(hori))
                if (String.IsNullOrEmpty(verti))
                    return 0;
                else
                    return verti.Length;
            else
                if (String.IsNullOrEmpty(verti))
                    return hori.Length;

            horiLen = hori.Length;
            int horiLenPOne = horiLen + 1;

            vertiLen = verti.Length;

            char[] horiChar = hori.ToCharArray();
            char[] vertiChar = verti.ToCharArray();

            int[] d = new int[(horiLenPOne) * (vertiLen + 1)];

            for (i = 0; i <= horiLen; i++)
                d[i] = i;

            for (j = 0; j <= vertiLen; j++)
                d[(j * (horiLenPOne)) + 0] = j;

            for (i = 1; i <= horiLen; i++)
            {
                iMOne = i - 1;

                for (j = 1; j <= vertiLen; j++)
                {
                    jMOne = j - 1;

                    if (horiChar[iMOne] == vertiChar[jMOne])
                        d[j * (horiLenPOne) + i] = d[(jMOne) * (horiLenPOne) + iMOne];
                    else
                    {
                        d[j * (horiLenPOne) + i] = minimumOf(
                        ref d[(j) * (horiLenPOne) + iMOne], //Deletion
                        ref d[(jMOne) * (horiLenPOne) + i], //Insertion
                        ref d[(jMOne) * (horiLenPOne) + iMOne] //Substitution
                        )
                        + 1;
                    }
                }
            }

            return d[(j - 1) * (horiLenPOne) + i - 1];
        }

        public static char[] removeEverythingThatIsntAlphaNumbersOrSpaces(string s)
        {
            char[] result = new char[s.Length];
            int j = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsLetterOrDigit(s, i))
                {
                    result[j] = s[i];
                    j++;
                }
            }
            Array.Resize<char>(ref result, j);
            return result;
        }

        public static int levenshteinDistanceIgnorePontuationAndCase(string hori, string verti)
        {
            if (String.IsNullOrEmpty(hori))
                if (String.IsNullOrEmpty(verti))
                    return 0;
                else
                    return verti.Length;
            else
                if (String.IsNullOrEmpty(verti))
                    return hori.Length;

            int i, j, jMOne, iMOne;

            //  char[] horiChar = removeEverythingThatIsntAlphaNumbersOrSpaces(hori);
            //char[] vertiChar = removeEverythingThatIsntAlphaNumbersOrSpaces(verti);
            string lowerCasehori = hori.ToLower();
            string lowerCaseverti = verti.ToLower();

            char[] horiChar = lowerCasehori.ToCharArray();
            char[] vertiChar = lowerCaseverti.ToCharArray();
            int horiLen = horiChar.Length;
            int horiLenPOne = horiLen + 1;
            int vertiLen = vertiChar.Length;

            int[] d = new int[(horiLenPOne) * (vertiLen + 1)];

            for (i = 0; i <= horiLen; i++)
                d[i] = i;

            for (j = 0; j <= vertiLen; j++)
                d[(j * (horiLenPOne)) + 0] = j;

            for (i = 1; i <= horiLen; i++)
            {
                iMOne = i - 1;

                for (j = 1; j <= vertiLen; j++)
                {
                    jMOne = j - 1;

                    if (horiChar[iMOne] == vertiChar[jMOne])
                        d[j * (horiLenPOne) + i] = d[(jMOne) * (horiLenPOne) + iMOne];
                    else
                    {
                        d[j * (horiLenPOne) + i] = minimumOf(
                        ref d[(j) * (horiLenPOne) + iMOne], //Deletion
                        ref d[(jMOne) * (horiLenPOne) + i], //Insertion
                        ref d[(jMOne) * (horiLenPOne) + iMOne] //Substitution
                        )
                        + 1;
                    }
                }
            }

            return d[(j - 1) * (horiLenPOne) + i - 1];
        }

        public static int levenshteinDistanceWithThreshold(string hori, string verti, int threshold)
        {
            int i, j, jMOne, iMOne, horiLen, vertiLen;

            if (String.IsNullOrEmpty(hori))
                if (String.IsNullOrEmpty(verti))
                    return 0;
                else
                    return verti.Length;
            else
                if (String.IsNullOrEmpty(verti))
                    return hori.Length;

            horiLen = hori.Length;
            int horiLenPOne = horiLen + 1;

            vertiLen = verti.Length;

            char[] horiChar = hori.ToCharArray();
            char[] vertiChar = verti.ToCharArray();

            int[] d = new int[(horiLenPOne) * (vertiLen + 1)];

            for (i = 0; i <= horiLen; i++)
                d[i] = i;

            for (j = 0; j <= vertiLen; j++)
                d[(j * (horiLenPOne)) + 0] = j;

            int minimumOfThisLine = 0;

            for (i = 1; i <= horiLen; i++)
            {
                iMOne = i - 1;

                if (horiChar[iMOne] == vertiChar[0])
                    d[1 * (horiLenPOne) + i] = d[(0) * (horiLenPOne) + iMOne];
                else
                {
                    d[1 * (horiLenPOne) + i] = minimumOf(
                    ref d[(1) * (horiLenPOne) + iMOne], //Deletion
                    ref d[(0) * (horiLenPOne) + i], //Insertion
                    ref d[(0) * (horiLenPOne) + iMOne] //Substitution
                    )
                    + 1;
                }
                minimumOfThisLine = d[1 * (horiLenPOne) + i];

                for (j = 1; j <= vertiLen; j++)
                {
                    jMOne = j - 1;

                    if (horiChar[iMOne] == vertiChar[jMOne])
                        d[j * (horiLenPOne) + i] = d[(jMOne) * (horiLenPOne) + iMOne];
                    else
                    {
                        d[j * (horiLenPOne) + i] = minimumOf(
                        ref d[(j) * (horiLenPOne) + iMOne], //Deletion
                        ref d[(jMOne) * (horiLenPOne) + i], //Insertion
                        ref d[(jMOne) * (horiLenPOne) + iMOne] //Substitution
                        )
                        + 1;
                    }

                    if (d[j * (horiLenPOne) + i] < minimumOfThisLine)
                        minimumOfThisLine = d[j * (horiLenPOne) + i];

                    if (minimumOfThisLine > threshold)
                        return minimumOfThisLine;
                }
            }
            return d[(j - 1) * (horiLenPOne) + i - 1];
        }
        #endregion
    }
}
