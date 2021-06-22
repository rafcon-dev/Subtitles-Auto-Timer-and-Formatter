using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace Subtitle_Synchronizer
{
    public class permutationObject
    {
        PermutationMethodsObject _bestMatchedPermmMethObj;
        int myStartIndex;
        int myDistanceBetweenNewSECONDSSublinesAndUnfixed;

        int _obligatoryTriesWhenPermutatingWords;
        bool myGoesBackwards;
        bool myIsInvaded;

        int _distanceOfNewModifiedStringsToTheirOrignals;

        List<string> _allProvisorySubtitlesStrings;

        public permutationObject()
        {
            myGoesBackwards = false;
            myStartIndex = 0;
            myDistanceBetweenNewSECONDSSublinesAndUnfixed = 0;
            _obligatoryTriesWhenPermutatingWords = 25;
            _allProvisorySubtitlesStrings = new List<string>();

            myGoesBackwards = false;
            myIsInvaded = false;
        }

        public int totalDistanceBetweenNewSublinesAndUnfixed
        {
            get
            { return _bestMatchedPermmMethObj.bestMatchedTotalDistance; }
        }

        public int startIndex
        {
            get { return myStartIndex; }
            set { myStartIndex = value; }
        }

        public bool goesBackwards
        {
            get { return myGoesBackwards; }
            set { myGoesBackwards = value; }
        }

        public bool isInvaded
        {
            get { return myIsInvaded; }
            set { myIsInvaded = value; }
        }

        public List<string> allNewSubitlesStrings()
        {
            return fullStringListWithNewSublines();
        }

        public int numberOfLinesOfBestMatched
        {
            get { return _bestMatchedPermmMethObj.numberOfLinesOfBestMatched; }
        }

        public void setAllProvisorySubtitlesStringsFromFixedSubs(allFixedSubtitles fixSubs)
        {
            for (int j = 0; j < fixSubs.fixedSubtitlesLines.Count; j++)
            {
                _allProvisorySubtitlesStrings.Add(
                    fixSubs.fixedSubtitlesLines[j].lineContent);
            }
        }

        public void setAllProvisorySubtitlesStringsFromStringList(List<string> stringList)
        {
            for (int j = 0; j < stringList.Count; j++)
            {
                _allProvisorySubtitlesStrings.Add(stringList[j]);
            }
        }

        List<string> fullStringListWithNewSublines()
        {
            List<string> allNewStrings = new List<string>();
            if (myStartIndex >= _allProvisorySubtitlesStrings.Count)
                return allNewStrings;

            foreach (string s in _allProvisorySubtitlesStrings)
            {
                allNewStrings.Add(s);
            }

            List<string> bestMatchedStrings = _bestMatchedPermmMethObj.bestMatchedStringList();
            int j = myStartIndex;
            int amountToAdd = goesBackwards ? -1 : 1;

            for (int i = 0; i < bestMatchedStrings.Count; i++)
            {
                if (j < 0)
                {
                    allNewStrings.Insert(0, bestMatchedStrings[i]);
                    j++;
                }

                else if (j >= allNewStrings.Count)
                    allNewStrings.Add(bestMatchedStrings[i]);

                else
                    allNewStrings[j] = bestMatchedStrings[i];

                j += amountToAdd;
            }

            return allNewStrings;
        }

        //ONLY GOING FORWARD////////////////////////////////////////////////////////////////////////

        //find the best matched subline, ignoring the adjecent ones, sending the excess forwards
        public void grossFindBestMatchedStartingFromIndex
            (int startSublineIndex, List<subLine> allUnfixedSublines)
        {
            if (startSublineIndex + 1 < 0 || startSublineIndex + 1 >= _allProvisorySubtitlesStrings.Count)
            {
                _bestMatchedPermmMethObj = new PermutationMethodsObject
                   (_allProvisorySubtitlesStrings[startSublineIndex], allUnfixedSublines[startSublineIndex].lineContent);
                return;
            }

            string tempString1 = _allProvisorySubtitlesStrings[startSublineIndex];

            string tempString2 = _allProvisorySubtitlesStrings[startSublineIndex + 1];

            PermutationMethodsObject permMethodObj = new PermutationMethodsObject(
                _allProvisorySubtitlesStrings[startSublineIndex],
                _allProvisorySubtitlesStrings[startSublineIndex + 1],
                allUnfixedSublines[startSublineIndex].lineContent,
                _obligatoryTriesWhenPermutatingWords);

            permMethodObj.closestFirstSendAllExcessToSecond();

            _bestMatchedPermmMethObj = permMethodObj;
        }

        /// <summary>
        /// Returns a list of strings that correspond to those that best mimic the originals, taking into account the adjacent
        /// Processes one line (of index startSublineIndex) and goes
        /// from line to line until the distance is minimized
        /// </summary>
        /// <param name="startSublineIndex"></param>
        /// <param name="allUnfixedSublines"></param>
        /// <returns></returns>
        /// 
        public void fineFindBestMatchedStartingFromIndex
            (int startSublineIndex, List<subLine> allUnfixedSublines)
        {
            // List<string> allNewSublinesStrings = new List<string>();

            List<PermutationMethodsObject> allPossibleLinePermutations = new List<PermutationMethodsObject>();

            List<int> allDistancesForNextIteration = new List<int>();

            int leveshteinDistanceSoFarBetweenSeconds = myDistanceBetweenNewSECONDSSublinesAndUnfixed;

            string tempString1;
            string tempString2;

            tempString1 = _allProvisorySubtitlesStrings[startSublineIndex];

            int distanceToMove = myGoesBackwards ? -1 : 1;
            int j = distanceToMove;

            int obligatoryTriesSoFar = 0;

            bool checkOneMoreSubline = false;
            do
            {
                if (startSublineIndex + j < 0 || startSublineIndex + j >= _allProvisorySubtitlesStrings.Count)
                    break;

                tempString2 = _allProvisorySubtitlesStrings[startSublineIndex + j];

                PermutationMethodsObject permMethodObj = new PermutationMethodsObject
                    (leveshteinDistanceSoFarBetweenSeconds, obligatoryTriesSoFar,
                    tempString1, tempString2,
                    allUnfixedSublines[startSublineIndex].lineContent, allUnfixedSublines[startSublineIndex + j].lineContent,
                    _obligatoryTriesWhenPermutatingWords, Math.Abs(j) - 1, allDistancesForNextIteration);

                if (myGoesBackwards)
                {
                    if (isInvaded)
                        permMethodObj.closestStringsByFirstGettingInvadedBySecondWhichIsInTheBack();
                    else
                        permMethodObj.closestStringsByFirstInvadingSecondWhichIsInTheBack();
                }
                else
                {
                    if (isInvaded)
                        permMethodObj.closestStringsByFirstGettingInvadedBySecondWhichIsInFront();
                    else
                        permMethodObj.closestStringsByFirstInvadingSecondWhichIsInFront();
                }

                allPossibleLinePermutations.Add(permMethodObj);

                j += distanceToMove;

                leveshteinDistanceSoFarBetweenSeconds = permMethodObj.distanceOfSecondsSoFar;
                obligatoryTriesSoFar = permMethodObj.triesCounterSoFar;

                tempString1 = permMethodObj.matchPairForNextIteration.line1;
                tempString2 = permMethodObj.matchPairForNextIteration.line2;

                allDistancesForNextIteration=permMethodObj.allDistancesOfPossiblePermutations;

                checkOneMoreSubline = permMethodObj.permutateWithOneMoreSubline;
            } while (checkOneMoreSubline); //while the second string is being emptied, ie, now permutate with the string before or after it

            if (allPossibleLinePermutations.Count > 0)
            {
                _bestMatchedPermmMethObj = allPossibleLinePermutations
                    [allPossibleLinePermutations.indexOfLowestDistance()];
            }
            else
            {
                _bestMatchedPermmMethObj = new PermutationMethodsObject
                    (tempString1, allUnfixedSublines[startSublineIndex].lineContent);
            }
        }
        void getBestMatchedPermmObj(List<PermutationMethodsObject> allPossibleLinePermutations)
        {

        }
    }
}