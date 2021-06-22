using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Synchronizer
{
    public class PermutationMethodsObject
    {
        int _distanceOfSecondsSoFar;
        int _triesCounterSoFar;
        string _tempSub1;
        string _tempSub2;
        string _originaltempSub1;
        string _originaltempSub2;
        string _unfixedFirst;
        string _unfixedSecond;

        bool _permutateWithOneMoreSubline;

        int _numberOfObligatoryTries;

        int _lineOffsetOfSub2RelativeToSub1;

        wordPermutationLinePair _minimumDistanceMatchPair;
        wordPermutationLinePair _pairForNextIteration;

        bool _weOnlyHavesubLineInThisObject; //for boundary conditions

        List<int> _allDistancesOfPossiblePermutations;

        int _distanceToOriginalOfSecondBeforePermutations;
        public PermutationMethodsObject(int leveshteinDistanceOfPreviousSeconds, int obligatoryTriesSoFar,
            string tempSub1, string tempSub2, string unfixedFirst, string unfixedSecond, int numberOfObligatoryTries,
            int lineOffsetOfSub2RelativeToSub1, List<int> allDistanceOfPossiblePermutationsSoFar)
        {
            _distanceOfSecondsSoFar = leveshteinDistanceOfPreviousSeconds;
            _triesCounterSoFar = obligatoryTriesSoFar;
            _tempSub1 = tempSub1;
            _tempSub2 = tempSub2;
            _unfixedFirst = unfixedFirst;
            _unfixedSecond = unfixedSecond;

            _permutateWithOneMoreSubline = false;
            _numberOfObligatoryTries = numberOfObligatoryTries;
            _lineOffsetOfSub2RelativeToSub1 = lineOffsetOfSub2RelativeToSub1;

            _weOnlyHavesubLineInThisObject = false;

            _allDistancesOfPossiblePermutations = allDistanceOfPossiblePermutationsSoFar;
            _distanceToOriginalOfSecondBeforePermutations = 0;

            _originaltempSub1 = tempSub1;
            _originaltempSub2 = tempSub2;
        }

        public PermutationMethodsObject(string tempSub1, string tempSub2, string unfixedFirst, int numberOfObligatoryTries)
        {
            _distanceOfSecondsSoFar = 0;
            _triesCounterSoFar = 0;
            _tempSub1 = tempSub1;
            _tempSub2 = tempSub2;
            _unfixedFirst = unfixedFirst;
            _unfixedSecond = string.Empty;

            _permutateWithOneMoreSubline = false;
            _numberOfObligatoryTries = numberOfObligatoryTries;

            _weOnlyHavesubLineInThisObject = false;

            _allDistancesOfPossiblePermutations = new List<int>();
            _distanceToOriginalOfSecondBeforePermutations = 0;
        }

        public PermutationMethodsObject(string tempSub1, string unfixedFirst)
        {
            _distanceOfSecondsSoFar = 0;
            _triesCounterSoFar = 0;
            _tempSub1 = tempSub1;
            _tempSub2 = tempSub2;
            _unfixedFirst = unfixedFirst;
            _unfixedSecond = string.Empty;

            _permutateWithOneMoreSubline = false;
            _numberOfObligatoryTries = 0;

            _minimumDistanceMatchPair = new wordPermutationLinePair(
                tempSub1,
                string.Empty,
                generalMethods.levenshteinDistanceIgnorePontuationAndCase(tempSub1, unfixedFirst),
                0);
            _weOnlyHavesubLineInThisObject = true;

            _allDistancesOfPossiblePermutations = new List<int>();
            _distanceToOriginalOfSecondBeforePermutations = 0;
        }

        public int distanceOfSecondsSoFar
        { get { return _distanceOfSecondsSoFar; } }

        public int triesCounterSoFar
        { get { return _triesCounterSoFar; } }

        public string tempSub1
        { get { return _tempSub1; } }

        public string tempSub2
        { get { return _tempSub2; } }

        public bool permutateWithOneMoreSubline
        { get { return _permutateWithOneMoreSubline; } }

        public wordPermutationLinePair minimumDistanceMatchPair
        { get { return _minimumDistanceMatchPair; } }

        public wordPermutationLinePair matchPairForNextIteration
        { get { return _pairForNextIteration; } }

        public int distanceToOriginalOfSecondBeforePermutations
        { get { return _distanceToOriginalOfSecondBeforePermutations; } }

        public List<string> bestMatchedStringList()
        {
            if (_weOnlyHavesubLineInThisObject)
            {
                List<string> oneLineList = new List<string>();
                oneLineList.Add(_minimumDistanceMatchPair.line1);
                return oneLineList;
            }
            else
                return _minimumDistanceMatchPair.toStringList();
        }

        public int bestMatchedTotalDistance
        { get { return _minimumDistanceMatchPair.totaldistanceBetweenAllLinesAndOriginals; } }

        public List<int> allDistancesOfPossiblePermutations
        { get { return _allDistancesOfPossiblePermutations; } }

        public int numberOfLinesOfBestMatched
        {
            get
            {
                if (_weOnlyHavesubLineInThisObject)
                    return 1;
                else
                    return _minimumDistanceMatchPair.numberOfLinesBetweenThePair + 2;
            }
        }

        void putLastWordOfs1InBeginningOfs2(ref string s1, ref string s2)
        {
            putLastWordOfs2InBeginningOfs1(ref s2, ref s1);
        }

        void putFirstWordOfs1InEndOfs2(ref string s1, ref string s2)
        {
            putFirstWordOfs2InEndOfs1(ref s2, ref s1);
        }

        void putLastWordOfs2InBeginningOfs1(ref string s1, ref string s2)
        {
            if (string.IsNullOrEmpty(s2))
                return;

            int index = s2.indexOfStartOfLastWord();

            s1 = String.Concat(s2.Substring(index), s1);

            s2 = s2.Substring(0, index);
        }

        void putFirstWordOfs2InEndOfs1(ref string s1, ref string s2)
        {
            if (string.IsNullOrEmpty(s2))
                return;

            int index = s2.indexOfEndOfFirstWord();

            s1 = String.Concat(s1, s2.Substring(0, index + 1));

            if (index + 1 == s2.Length)
                s2 = string.Empty;
            else
                s2 = s2.Substring(index + 1);
        }

        public void closestFirstSendAllExcessToSecond()
        {
            int distance1;

            List<wordPermutationLinePair> allPossibleLinePairs = new List<wordPermutationLinePair>();

            distance1 = generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub1, _unfixedFirst);

            int iterationCounter = -1;

            while (weHaveWordsToPermutateInLine1Available()
                &&
                (allPossibleLinePairs.Count < _numberOfObligatoryTries // while we still havent tried 5 extra options
                ||
                iterationCounter < 1
                ||
                distance1 < allPossibleLinePairs[iterationCounter].totaldistanceBetweenAllLinesAndOriginals))
            {
                allPossibleLinePairs.Add(new wordPermutationLinePair(_tempSub1, _tempSub2, distance1, 0));

                putLastWordOfs1InBeginningOfs2(ref _tempSub1, ref _tempSub2);

                distance1 = generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub1, _unfixedFirst);

                iterationCounter++;
            }

            if (allPossibleLinePairs.Count > 0)
                _minimumDistanceMatchPair = allPossibleLinePairs[allPossibleLinePairs.indexOfLowestDistance()];
            else
                _minimumDistanceMatchPair = new wordPermutationLinePair(tempSub1, tempSub2, 0, 0);
        }

        enum typesOFWordPermutation
        {
            beginningOfS1GoingToEndOfS2,
            endOfS1GoingToBeginningOfS2,
            endOfS2GoingToBeginningOfS1,
            beginningOfS2GoingToEndOfS1
        }

        public void closestStringsByFirstInvadingSecondWhichIsInTheBack()
        {
            findClosestStringsByPermutationFirstWithSecond(typesOFWordPermutation.beginningOfS1GoingToEndOfS2);
        }

        /// <summary>
        /// Modifies subLine first and second to those that minimize the distances between them and
        /// the unfixed lines, by putting the first words of the second string in the end of first string
        /// Returns the distance between the second string and the respective original
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="unfixedFirst"></param>
        /// <param name="unfixedSecond"></param>
        public void closestStringsByFirstGettingInvadedBySecondWhichIsInTheBack()
        {
            findClosestStringsByPermutationFirstWithSecond(typesOFWordPermutation.endOfS2GoingToBeginningOfS1);
        }

        /// <summary>
        /// Modifies subLine first and second to those that minimize the distances between them and
        /// the unfixed lines, by putting the last words of the first string in the beginning of second string
        /// Returns the distance between the second string and the respective original
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="unfixedFirst"></param>
        /// <param name="unfixedSecond"></param>
        public void closestStringsByFirstGettingInvadedBySecondWhichIsInFront()
        {
            findClosestStringsByPermutationFirstWithSecond(typesOFWordPermutation.beginningOfS2GoingToEndOfS1);
        }

        public void closestStringsByFirstInvadingSecondWhichIsInFront()
        {
            findClosestStringsByPermutationFirstWithSecond(typesOFWordPermutation.endOfS1GoingToBeginningOfS2);
        }

        void permutateWithTypeOfWordPermutation(ref string s1, ref string s2, typesOFWordPermutation typeOfPerm)
        {
            switch (typeOfPerm)
            {
                case typesOFWordPermutation.beginningOfS1GoingToEndOfS2:
                    putFirstWordOfs1InEndOfs2(ref s1, ref s2);
                    break;
                case typesOFWordPermutation.endOfS1GoingToBeginningOfS2:
                    putLastWordOfs1InBeginningOfs2(ref s1, ref s2);
                    break;
                case typesOFWordPermutation.endOfS2GoingToBeginningOfS1:
                    putLastWordOfs2InBeginningOfs1(ref s1, ref s2);
                    break;
                case typesOFWordPermutation.beginningOfS2GoingToEndOfS1:
                    putFirstWordOfs2InEndOfs1(ref s1, ref s2);
                    break;
            }
        }

        void findClosestStringsByPermutationFirstWithSecond(typesOFWordPermutation typeOfWordPermutation)
        {
            List<wordPermutationLinePair> allPossibleLinePairs = new List<wordPermutationLinePair>();

            _distanceToOriginalOfSecondBeforePermutations = 
                generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub2, _unfixedSecond);

            int distance1_2 = generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub1, _unfixedFirst)
                        + _distanceToOriginalOfSecondBeforePermutations
                        + _distanceOfSecondsSoFar;

            allPossibleLinePairs.Add(new wordPermutationLinePair
                    (_tempSub1, _tempSub2, distance1_2, _lineOffsetOfSub2RelativeToSub1));

            _allDistancesOfPossiblePermutations.Add(distance1_2);

            int iterationCounter = -1;

            while (
                weHaveWordsToPermutateAvailable(typeOfWordPermutation)
                &&
                (
                allPossibleLinePairs.Count + triesCounterSoFar < _numberOfObligatoryTries // while we still havent tried 5 extra options
                ||
                iterationCounter < 0
                ||
                (_allDistancesOfPossiblePermutations.Count >= 1
                &&
                distance1_2 <= 
                _allDistancesOfPossiblePermutations[_allDistancesOfPossiblePermutations.Count - 2]
                )
                ||
                (_allDistancesOfPossiblePermutations.Count >= 2
                &&
                distance1_2 <=
                _allDistancesOfPossiblePermutations[_allDistancesOfPossiblePermutations.Count - 3])
                ))
            {
                permutateWithTypeOfWordPermutation(ref _tempSub1, ref _tempSub2, typeOfWordPermutation);

                distance1_2 = generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub1, _unfixedFirst)
                            + generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub2, _unfixedSecond)
                            + _distanceOfSecondsSoFar;

                allPossibleLinePairs.Add(new wordPermutationLinePair
                    (_tempSub1, _tempSub2, distance1_2, _lineOffsetOfSub2RelativeToSub1));

                _allDistancesOfPossiblePermutations.Add(distance1_2);

                iterationCounter++;
            }

            if (allPossibleLinePairs.Count > 0)
            {
                _minimumDistanceMatchPair = allPossibleLinePairs[allPossibleLinePairs.indexOfLowestDistance()];
                _pairForNextIteration = allPossibleLinePairs[allPossibleLinePairs.Count - 1];
            }
            else
            {
                _minimumDistanceMatchPair = new wordPermutationLinePair(tempSub1, tempSub2,
                    generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub1, _unfixedFirst)
                            + generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub2, _unfixedSecond)
                            + _distanceOfSecondsSoFar,
                            _lineOffsetOfSub2RelativeToSub1);

                _pairForNextIteration = _minimumDistanceMatchPair;
            }

            _distanceOfSecondsSoFar += generalMethods.levenshteinDistanceIgnorePontuationAndCase(_tempSub2, _unfixedSecond);

            _triesCounterSoFar += allPossibleLinePairs.Count;

            _permutateWithOneMoreSubline = weNeedAnotherIteration
                (typeOfWordPermutation, _triesCounterSoFar, allPossibleLinePairs);
        }

        bool weHaveWordsToPermutateInLine1Available()
        {
            return !string.IsNullOrWhiteSpace(_tempSub1);
        }

        bool weHaveWordsToPermutateAvailable(typesOFWordPermutation typeOfWordPermutation)
        {
            if (typeOfWordPermutation == typesOFWordPermutation.beginningOfS1GoingToEndOfS2
                || typeOfWordPermutation == typesOFWordPermutation.endOfS1GoingToBeginningOfS2)
                return !string.IsNullOrWhiteSpace(_tempSub1);
            else
                return !string.IsNullOrWhiteSpace(_tempSub2);
        }

        bool weNeedAnotherIteration
            (typesOFWordPermutation typeOfWordPermutation,
            int obligatoryTriesBeforeThisLastPermutation,
            List<wordPermutationLinePair> allLinePairs)
        {
            int indexOfLast = _allDistancesOfPossiblePermutations.Count - 1;

            //if we are emptying s1, theres no other string to go to
            if (typeOfWordPermutation == typesOFWordPermutation.beginningOfS1GoingToEndOfS2
                || typeOfWordPermutation == typesOFWordPermutation.endOfS1GoingToBeginningOfS2)
                return false;

            else if (allLinePairs.Count + obligatoryTriesBeforeThisLastPermutation < _numberOfObligatoryTries)
                return true;

            else if
                (_allDistancesOfPossiblePermutations.Count > 1
                &&
                _allDistancesOfPossiblePermutations[indexOfLast] <= _allDistancesOfPossiblePermutations[indexOfLast - 1])
                return true;

            else if
                (_allDistancesOfPossiblePermutations.Count > 2
                &&
                _allDistancesOfPossiblePermutations[indexOfLast] <= _allDistancesOfPossiblePermutations[indexOfLast - 2])
                return true;

            else if
                (allLinePairs.Count == 0 && !weHaveWordsToPermutateAvailable(typeOfWordPermutation)) //because this line apparently was empty
                return true;

            else
                return false;
        }
    }
}
