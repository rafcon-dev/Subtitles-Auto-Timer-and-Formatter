using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using OpenNLP;

namespace Subtitle_Synchronizer
{
    public class BreakLongLinesObject
    {
        string myRegexExpression;
        List<subLine> myAllNewProvisorySublines;

        subLine myLineToBreak;
        public BreakLongLinesObject()
        {
            regexExpression = phrasesGeneralMethods.regexExpression;
            allNewProvisorySublines = new List<subLine>();
            lineToBreak = new subLine();
        }
        string regexExpression
        {
            get { return myRegexExpression; }
            set { myRegexExpression = value; }
        }

        public List<subLine> allNewProvisorySublines
        {
            get { return myAllNewProvisorySublines; }
            set { myAllNewProvisorySublines = value; }
        }

        public subLine lineToBreak
        {
            get { return myLineToBreak; }
            set { myLineToBreak = value; }
        }

        public bool differenceBetweenLinesIsBiggerThan(double threshold)
        {
            List<int> allLenghts = new List<int>();
            foreach (subLine sl in allNewProvisorySublines)
            {
                allLenghts.Add(sl.lineContent.Length);
            }
            double dif = generalMethods.differencesnessBetweenValues(allLenghts);

            return (dif > threshold);
        }

        public bool differenceBetweenTwoStringsIsBiggerThan(double threshold, string s1, string s2)
        {
            double differenceBetweenStrings = Math.Abs(Convert.ToDouble(s1.Length - s2.Length));

            double combinedLenghtOfStrings = Convert.ToDouble(s1.Length + s2.Length);

            double ratio = differenceBetweenStrings / combinedLenghtOfStrings;

            return (ratio > threshold);

        }
        public void forceBreakAtParagraphs(int maxNumberOfParagraphsPerSubline, int singleLineMaxLenght)
        {
            if (maxNumberOfParagraphsPerSubline < 1)
                maxNumberOfParagraphsPerSubline = 1;

            string regexExpression = phrasesGeneralMethods.paragraphRegexExpression;

            listOfBlockStrings allNewBreakedStrings = new listOfBlockStrings(lineToBreak.lineContent, regexExpression);
            allNewBreakedStrings.AddOriginalStringInBlockForm();

            int firstLineIndex = 0;
            int lastLineIndex = firstLineIndex;

                while (allNewBreakedStrings.numberOfBlocksInLineIndex(firstLineIndex) > maxNumberOfParagraphsPerSubline
                    || allNewBreakedStrings.thereIsABlockThatIsLongerThan(singleLineMaxLenght,  0))
                {
                    if (allNewBreakedStrings.numberOfBlocksInLineIndex(0) <= 1)
                        break;

                    allNewBreakedStrings.permutateLastBlockFromLineIndex1ToBeginningOfTheNextLine(firstLineIndex);
                }
             //   lastLineIndex = allNewBreakedStrings.numberOfLines - 1;

            assignNewBreakedLines(allNewBreakedStrings);
        }

        
        public void breakLinesAtCenter(int maxLineLenght)
        {
            listOfBlockStrings allNewBreakedStrings = new listOfBlockStrings(lineToBreak.lineContent);
            if (lineToBreak.lineContent.Length > maxLineLenght)
            {
                allNewBreakedStrings.AddOriginalStringInBlockForm();
                allNewBreakedStrings.permutateLastBlockFromLineIndex1ToBeginningOfTheNextLine(0);

                if (allNewBreakedStrings.numberOfLines > 1) //if we created new lines
                {
                    //Clean up this in case it has something it shouldn't
                    allNewProvisorySublines = new List<subLine>();
                    for (int i = 0; i < allNewBreakedStrings.numberOfLines - 1; i++)
                    {
                        replaceAllNewProvisorySublinesAtIndexWithSublineList(i,
                            newTwoBreakedSublines(allNewBreakedStrings.stringAtIndex(i),
                            allNewBreakedStrings.stringAtIndex(i + 1),
                            lineToBreak));
                    }
                }
            }
        }

        public void breakLinesAtLastPhrase()
        {
            string regexExpression = phrasesGeneralMethods.regexExpression;

            listOfBlockStrings allNewBreakedStrings = new listOfBlockStrings(lineToBreak.lineContent, regexExpression);

            if (allNewBreakedStrings.onlyOneBlockFound)
                return;

            allNewBreakedStrings.AddOriginalStringInBlockForm();

            allNewBreakedStrings.permutateLastBlockFromLineIndex1ToBeginningOfTheNextLine(0);

            assignNewBreakedLines(allNewBreakedStrings);
        }

        public void forcedBreakLineAtParagraphs()
        {
            string regexExpression = phrasesGeneralMethods.paragraphRegexExpression;

            listOfBlockStrings allNewBreakedStrings = new listOfBlockStrings(lineToBreak.lineContent, regexExpression);

            if (allNewBreakedStrings.onlyOneBlockFound)
                return;

            allNewBreakedStrings.AddOriginalStringInBlockForm();
            allNewBreakedStrings.permutateLastBlockFromLineIndex1ToBeginningOfTheNextLine(0);

            assignNewBreakedLines(allNewBreakedStrings);
        }

        public void breakLineIntoTwoAtPhrases(int maxLineLenght, int maxLineLenghtThreshold, double differenceLenghtThreshold)
        {
            string regexExpression = phrasesGeneralMethods.regexExpression;
            breakIntoTwoWithRegex(regexExpression, maxLineLenght, maxLineLenghtThreshold, differenceLenghtThreshold);
        }

        public void breakLineIntoTwoAtCommas(int maxLineLenght, int maxLineLenghtThreshold, double differenceLenghtThreshold)
        {
            string regexExpression = @"[^\.\?\!\,]+[\.\?\!\,]+[^a-zA-Z0-9-\s]*";
            breakIntoTwoWithRegex(regexExpression, maxLineLenght, maxLineLenghtThreshold, differenceLenghtThreshold);
        }

        public void breakLineIntoTwoAtCenter(int maxLineLenght)
        {
            breakLinesAtCenter(maxLineLenght);
        }

        public void breakLinesAtExcessiveParagraph()
        {
            listOfBlockStrings allNewBreakedStrings = new listOfBlockStrings
                (lineToBreak.lineContent, phrasesGeneralMethods.paragraphRegexExpression);

            if (allNewBreakedStrings.onlyOneBlockFound)
                return;

            allNewBreakedStrings.AddOriginalStringInBlockForm();

            while (allNewBreakedStrings.numberOfBlocksInLineIndex(0) > 2)
            {
                allNewBreakedStrings.permutateLastBlockFromLineIndex1ToBeginningOfTheNextLine(0);
            }

            assignNewBreakedLines(allNewBreakedStrings);
        }
        public void breakLineAtParagraphs(int maxSubLineLenght)
        {
            string regexExpression = phrasesGeneralMethods.paragraphRegexExpression;
            breakLinesWithRegex(regexExpression, maxSubLineLenght);
        }

        public void breakLinesAtPhrases(int maxSubLineLenght)
        {
            string regexExpression = phrasesGeneralMethods.regexExpression;
            breakLinesWithRegex(regexExpression, maxSubLineLenght);
        }

        public void breakLinesAtCommas(int maxSubLineLenght)
        {
            string regexExpression = @"[^\.\?\!\,]+[\.\?\!\,]+[^a-zA-Z0-9-\s]*";
            breakLinesWithRegex(regexExpression, maxSubLineLenght);
        }

        public void breakLinesAtGrammar(int maxSubLineLenght)
        {
            string nModelPath = @".\ExpotsAndFiles\OpenNLP\nbin\";

            OpenNLP.Tools.SentenceDetect.MaximumEntropySentenceDetector mSentenceDetector;
            OpenNLP.Tools.Tokenize.EnglishMaximumEntropyTokenizer mTokenizer;
            OpenNLP.Tools.PosTagger.EnglishMaximumEntropyPosTagger mPosTagger;

        }

        void breakIntoTwoWithRegex
            (string regexExpression, int maxLineLenght, int maxLineLenghtThreshold, double differenceLenghtThreshold)
        {
            string commaRegexExpression = regexExpression;

            listOfBlockStrings allNewBreakedStrings = new listOfBlockStrings(lineToBreak.lineContent, commaRegexExpression);

            if (allNewBreakedStrings.onlyOneBlockFound)
                return;

            allNewBreakedStrings.AddOriginalStringInBlockForm();

            bool thereWasNoChange = false;
            int previousFirstLineLenght = allNewBreakedStrings.firstLineLenght;
            bool thisIsNotTheFirstCut = false;
            bool twoStringsAreTooDifferent = false;

            while (allNewBreakedStrings.firstLineLenght > maxLineLenght
                ||
                (allNewBreakedStrings.numberOfLines >= 2 && twoStringsAreTooDifferent))
            {
                if (allNewBreakedStrings.firstLineLenght > 1)
                    allNewBreakedStrings.permutateLastBlockFromLineIndex1ToBeginningOfTheNextLine(0);

                thereWasNoChange = (allNewBreakedStrings.firstLineLenght == previousFirstLineLenght);
                if (thereWasNoChange) break;

                previousFirstLineLenght = allNewBreakedStrings.firstLineLenght;

                //then this last cut is good enough

                twoStringsAreTooDifferent = differenceBetweenTwoStringsIsBiggerThan(differenceLenghtThreshold,
                allNewBreakedStrings.stringAtIndex(0), allNewBreakedStrings.stringAtIndex(1));

                if (allNewBreakedStrings.firstLineLenght <= maxLineLenght + maxLineLenghtThreshold
                    &&
                    twoStringsAreTooDifferent == false)
                    break;
            }
            thisIsNotTheFirstCut = true;
            assignNewBreakedLines(allNewBreakedStrings);
        }

        void breakLinesWithRegex(string regexExpression, int maxSublineLenght)
        {
            string commaRegexExpression = regexExpression;

            listOfBlockStrings allNewBreakedStrings = new listOfBlockStrings(lineToBreak.lineContent, commaRegexExpression);

            if (allNewBreakedStrings.onlyOneBlockFound)
                return;

            allNewBreakedStrings.AddOriginalStringInBlockForm();
            int previousFirstLineLenght = allNewBreakedStrings.firstLineLenght;

            int firstLineIndex = 0;
            bool thereWasNoChange = true;
            do
            {
                while (allNewBreakedStrings.lenghtOfStringAtIndex(firstLineIndex) > maxSublineLenght)
                {
                    if (allNewBreakedStrings.numberOfBlocksInLineIndex(firstLineIndex) > 1)
                        allNewBreakedStrings.permutateLastBlockFromLineIndex1ToBeginningOfTheNextLine(firstLineIndex);

                    thereWasNoChange = (allNewBreakedStrings.lenghtOfStringAtIndex(firstLineIndex) == previousFirstLineLenght);
                    if (thereWasNoChange) break;
                    previousFirstLineLenght = allNewBreakedStrings.lenghtOfStringAtIndex(firstLineIndex);
                }

                if (thereWasNoChange) break;
                firstLineIndex++;


            } while (allNewBreakedStrings.lastLineLenght > maxSublineLenght);

            assignNewBreakedLines(allNewBreakedStrings);

        }

        bool assignNewBreakedLines(listOfBlockStrings allNewBreakedStrings)
        {
            if (allNewBreakedStrings.numberOfLines > 1) //if we created new lines
            {
                //Clean up this in case it has something it shouldn't
                allNewProvisorySublines = new List<subLine>();
                allNewProvisorySublines.Add(lineToBreak);
                for (int i = 0; i < allNewBreakedStrings.numberOfLines - 1; i++)
                {
                    replaceAllNewProvisorySublinesAtIndexWithSublineList(i,
                        newTwoBreakedSublines(
                    allNewBreakedStrings.stringAtIndex(i),
                    allNewBreakedStrings.stringAtIndex(i + 1),
                    allNewProvisorySublines[i]));
                }
                return true;
            }
            else
                return false;
        }

        void replaceAllNewProvisorySublinesAtIndexWithSublineList(int index, List<subLine> subLineList)
        {
            if (index > allNewProvisorySublines.Count || index < 0)
                return;

            for (int i = 0; i < subLineList.Count; i++)
            {
                if (index + i < allNewProvisorySublines.Count)
                    allNewProvisorySublines[index + i] = subLineList[i];
                else
                    allNewProvisorySublines.Add(subLineList[i]);

                if (i + 1 < subLineList.Count)
                    allNewProvisorySublines.Add(subLineList[i + 1]);
            }
        }

        void getTheTime(subLine originalLineToBreak, subLine newFirstLine, subLine newSecondLine)
        {

            List<subTimePoint> allTimePointsOfThisLine = new List<subTimePoint>();
            List<int> allIndexes = new List<int>();

            allTimePointsOfThisLine.Add(originalLineToBreak.begTime);
            allIndexes.Add(0);

            foreach (lineStitch stitch in originalLineToBreak.allStitches)
                if (stitch.stitchIsNotCorrect == false)
                {
                    allTimePointsOfThisLine.Add(stitch.timeOfStitch);
                    allIndexes.Add(stitch.indexOfStitch);
                }
            allTimePointsOfThisLine.Add(originalLineToBreak.endTime);
            allIndexes.Add(originalLineToBreak.lineContent.Length - 1);

            int firstLineFirstTime = 0;

            int i = 0;
            for (; i < allTimePointsOfThisLine.Count; i++)
            {
                //get first exactTimePoint
                if (allTimePointsOfThisLine[i].isExact)
                {
                    firstLineFirstTime = allTimePointsOfThisLine[i].timeInMilisec;
                    break;
                }
            }

            if (i == allTimePointsOfThisLine.Count) //there aren't any exact time Points...
                firstLineFirstTime = allTimePointsOfThisLine[0].timeInMilisec;

            int firstLineSecondTimeIndexExact = 0;
            int firstLineSecondTimeIndexNOTExact = 0;
            for (; i < allTimePointsOfThisLine.Count; i++)
            {
                if (newFirstLine.lineContent.Length - 1 <= allIndexes[i]) //if the newFirstLine is inside this index
                {
                    if (allTimePointsOfThisLine[i].isExact)
                        firstLineSecondTimeIndexExact = i;
                    else
                        firstLineSecondTimeIndexNOTExact = i;
                }
            }

            // int firstLineDuration = (sl.totalMiliseconds * s.LenghtWithoutSpecialCharacters())
            //            / sl.lineContent.LenghtWithoutSpecialCharacters();


        }

        #region ValuesAssignment

        List<subLine> newTwoBreakedSublines(string newString1, string newString2, subLine lineWeAreBreaking)
        {
            subLine newSubLine1 = new subLine();
            subLine newSubLine2 = new subLine();

            assignLineContents(newSubLine1, newString1, newSubLine2, newString2);
            assignTimeValues(newSubLine1, newSubLine2, lineWeAreBreaking);
            assignIndexValues(newSubLine1, newSubLine2, lineWeAreBreaking);

            List<subLine> newBreakedSublines = new List<subLine>();

            newBreakedSublines.Add(newSubLine1);
            newBreakedSublines.Add(newSubLine2);

            return newBreakedSublines;
        }


        void assignLineContents(subLine newSubLine1, string newString1, subLine newSubLine2, string newString2)
        {
            newSubLine1.lineContent = newString1;
            newSubLine2.lineContent = newString2;
        }

        void assignTimeValues(subLine newsubLine1, subLine newsubLine2, subLine lineWeAreBreaking)
        {
            newsubLine1.begTime.assignTimeFromMilisec
                (lineWeAreBreaking.begTime.timeInMilisec, lineWeAreBreaking.begTime.isExact, lineWeAreBreaking.begTime.isYoutubeTiming);

            int firstLineDuration =
                generalMethods.durationOf_s_InRelationTo_sl(newsubLine1.lineContent, lineWeAreBreaking);

            newsubLine1.endTime.assignTimeFromMilisec(lineWeAreBreaking.begTime.timeInMilisec + firstLineDuration, false, false);

            newsubLine2.begTime.assignTimeFromMilisec(newsubLine1.endTime.timeInMilisec + 1, false, false);

            int secondLineDuration =
                generalMethods.durationOf_s_InRelationTo_sl(newsubLine2.lineContent, lineWeAreBreaking);

            newsubLine2.endTime.assignTimeFromMilisec(newsubLine2.begTime.timeInMilisec + secondLineDuration, false, false);
        }

        void assignIndexValues(subLine newsubLine1, subLine newsubLine2, subLine lineWeAreBreaking)
        {
            newsubLine1.lineIndex = lineWeAreBreaking.lineIndex;
            newsubLine2.lineIndex = newsubLine1.lineIndex + 1;
        }
        #endregion
    }
}