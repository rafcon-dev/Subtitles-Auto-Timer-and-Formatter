using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;

namespace Subtitle_Synchronizer
{
    public class allFixedSubtitles
    {
        List<subLine> _fixedSubtitlesLines;

        public List<int> notExactBegTimesHistory = new List<int>();
        public List<int> notExactEndTimesHistory = new List<int>();
        List<string> allTheActors = new List<string>();

        public delegate void ProgressUpdate(int currentStep, int totalSteps);
        public event ProgressUpdate OnProgressUpdate;
        public event EventHandler<CancelEventArgs> CheckCancel;

        public List<bool> allMarkersForSpeechRecTimings = new List<bool>();

        public allFixedSubtitles()
        {
            _fixedSubtitlesLines = new List<subLine>();
        }

        public List<subLine> fixedSubtitlesLines
        {
            get { return _fixedSubtitlesLines; }
            set { _fixedSubtitlesLines = value; }
        }

        private bool onCheckCancel()
        {
            EventHandler<CancelEventArgs> handler = CheckCancel;

            if (handler != null)
            {
                CancelEventArgs e = new CancelEventArgs();

                handler(this, e);

                return e.Cancel;
            }
            return false;
        }

        public void turnNullStringsEmpty()
        {
            foreach (subLine sl in _fixedSubtitlesLines)
            {
                string tempLineContent = sl.lineContent;
                generalMethods.ifNullTurnEmpty(ref tempLineContent);
                sl.lineContent = tempLineContent;
            }
        }

        public void joinSeparatedSentences()
        {
            string currentSentence = String.Empty;

            List<subLine> provisorySubs = _fixedSubtitlesLines;

            PhrasesPermutationObject phrasesPermObj = new PhrasesPermutationObject();

            if (provisorySubs.Count > 0)
                phrasesPermObj.secondLineRegexMatches = Regex.Matches(provisorySubs[0].lineContent, phrasesPermObj.regexExpression);

            for (int j = 1; j < provisorySubs.Count; j++)
            {
                phrasesPermObj.firstSubline = provisorySubs[j - 1];
                phrasesPermObj.secondSubline = provisorySubs[j];

                phrasesPermObj.permutateIncompletePhrase();

                if (phrasesPermObj.thereWereChanges)
                {
                    provisorySubs[j - 1] = phrasesPermObj.firstSubline;
                    provisorySubs[j] = phrasesPermObj.secondSubline;

                    if (phrasesPermObj.stitchIsInFirstLine)
                        provisorySubs[j - 1].allStitches.Add(phrasesPermObj.stitch);
                    else
                        provisorySubs[j].allStitches.Add(phrasesPermObj.stitch);
                }

                int new_j = j;

                if (string.IsNullOrEmpty(provisorySubs[j].lineContent))
                {
                    provisorySubs.RemoveAt(j);
                    new_j--;
                }

                if (string.IsNullOrEmpty(provisorySubs[j - 1].lineContent))
                {
                    provisorySubs.RemoveAt(j - 1);
                    new_j--;
                }

                j = new_j;
            }
        }

        public float averageCharactersPerSecond()
        {
            int totalMilisseconds = 0;
            int totalCharacters = 0;

            foreach (subLine sl in _fixedSubtitlesLines)
            {
                totalMilisseconds += sl.totalMiliseconds;
                totalCharacters += generalMethods.removeEverythingThatIsntAlphaNumbersOrSpaces(sl.lineContent).Length;
            }
            return (float)totalCharacters / (float)totalMilisseconds;
        }

        public void removeAllEmptySublines()
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count; )
            {
                if (_fixedSubtitlesLines[i].lineContent == string.Empty)
                {
                    _fixedSubtitlesLines.RemoveAt(i);
                    continue;
                }
                i++;
            }
        }

        public void removeExtraNewLines()
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                _fixedSubtitlesLines[i].lineContent = _fixedSubtitlesLines[i].lineContent.eliminateDuplicatedNewLines();
            }
        }

        public void trimWhiteSpace()
        {
            foreach (subLine sl in _fixedSubtitlesLines)
                sl.lineContent = sl.lineContent.Trim();
        }
        public void recalculateLineIndexes()
        {
            int i = 1;
            foreach (subLine sl in _fixedSubtitlesLines)
            {
                sl.lineIndex = i;
                i++;
            }
        }

        public void replaceLineContentsWithStringList(List<string> stringList)
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count && i < stringList.Count; i++)
            {
                _fixedSubtitlesLines[i].lineContent = stringList[i];
            }
        }

        public void fixOverlapping()
        {
            for (int i = 1; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].begTime.timeInMilisec < _fixedSubtitlesLines[i - 1].endTime.timeInMilisec) //is overlapping
                {
                    BreakLongLinesObject breakLongLinesObj = new BreakLongLinesObject();
                    breakLongLinesObj.lineToBreak = fixedSubtitlesLines[i];

                    breakLongLinesObj.breakLinesAtPhrases(2);

                    if (breakLongLinesObj.allNewProvisorySublines.Count >= 2)
                    {
                        _fixedSubtitlesLines[i] = breakLongLinesObj.allNewProvisorySublines[0];
                        for (int j = 1; j < breakLongLinesObj.allNewProvisorySublines.Count; j++)
                        {
                            _fixedSubtitlesLines.Insert(i + j, breakLongLinesObj.allNewProvisorySublines[j]);
                        }
                        i = i + breakLongLinesObj.allNewProvisorySublines.Count - 1;
                    }
                }
            }
        }

        public void breakLinesThatTakeTooMuchTime(double threshold)
        {
            double averageMilissecPerChar = generalMethods.averageMilisecondsPerCharacter(_fixedSubtitlesLines);

            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                double thisLineMilisecPerCharacter = _fixedSubtitlesLines[i].milisecPerCharacter;
                if (thisLineMilisecPerCharacter > averageMilissecPerChar)
                {
                    double fractionOfDifferenceBetweenThisTimeAndTheAverage = thisLineMilisecPerCharacter / averageMilissecPerChar - 1.0;
                    if (fractionOfDifferenceBetweenThisTimeAndTheAverage > threshold)
                    {
                        BreakLongLinesObject breakLongLinesObj = new BreakLongLinesObject();
                        breakLongLinesObj.lineToBreak = _fixedSubtitlesLines[i];

                        breakLongLinesObj.breakLinesAtLastPhrase();

                        if (breakLongLinesObj.allNewProvisorySublines.Count == 2) //because if we created more than two lines, we don't want them
                        {
                            _fixedSubtitlesLines[i] = breakLongLinesObj.allNewProvisorySublines[0];
                            _fixedSubtitlesLines.Insert(i + 1, breakLongLinesObj.allNewProvisorySublines[1]);
                        }
                    }
                }
            }
        }

        public void breakLongLinesIntoSeparateSublines(int subLineMaxLenght, double differenceThreshold)
        {
            breakLinesAtWithBreakMethod(subLineMaxLenght, differenceThreshold, breakMethods.breakAtParagraphs);
            breakLinesAtWithBreakMethod(subLineMaxLenght, differenceThreshold, breakMethods.breakAtPhrases);
            breakLinesAtWithBreakMethod(subLineMaxLenght, differenceThreshold, breakMethods.breakAtCommas);
            breakLinesAtWithBreakMethod(subLineMaxLenght, differenceThreshold, breakMethods.breakAtGrammar);
            breakLinesAtWithBreakMethod(subLineMaxLenght, differenceThreshold, breakMethods.breakAtCenter);
        }

        public void adjustNewLinesInEachSubline(int singleLineMaxLenght, int singleLineLenghtThreshold, double differenceThreshold)
        {
            List<bool> allThisLineHasBeenBrokenBefore = new List<bool>(new bool[_fixedSubtitlesLines.Count]);

            createParagraphsWithBreakMethod
                (singleLineMaxLenght, singleLineLenghtThreshold, differenceThreshold,
                breakMethods.breakAtParagraphs, allThisLineHasBeenBrokenBefore);

            createParagraphsWithBreakMethod
                (singleLineMaxLenght, singleLineLenghtThreshold, differenceThreshold,
                breakMethods.breakAtPhrases, allThisLineHasBeenBrokenBefore);

            createParagraphsWithBreakMethod
                (singleLineMaxLenght, singleLineLenghtThreshold, differenceThreshold,
                breakMethods.breakAtCommas, allThisLineHasBeenBrokenBefore);

            createParagraphsWithBreakMethod
                (singleLineMaxLenght, singleLineLenghtThreshold, differenceThreshold,
                breakMethods.breakAtCenter, allThisLineHasBeenBrokenBefore);
        }

        void createParagraphsWithBreakMethod
            (int maxLineLenght, int maxLineLenghtThreshold, double differenceThreshold,
            breakMethods breakingMethod, List<bool> allThisLineHasBeenBrokenBefore)
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                string linc = _fixedSubtitlesLines[i].lineContent;

                if (lineShouldBeBroken
                    (_fixedSubtitlesLines[i].lineContent, maxLineLenght, maxLineLenghtThreshold, allThisLineHasBeenBrokenBefore[i]))
                {
                    joinTheParagraphsInOneLine(_fixedSubtitlesLines[i].lineContent);

                    BreakLongLinesObject breakLongLinesObj = new BreakLongLinesObject();
                    breakLongLinesObj.lineToBreak = _fixedSubtitlesLines[i];

                    breakParagraphsWithProperMethod
                        (breakingMethod, breakLongLinesObj, maxLineLenght, maxLineLenghtThreshold, differenceThreshold);

                    //Assign
                    if (breakLongLinesObj.allNewProvisorySublines.Count >= 2 && weFoundAProperCombinationOfLines(breakLongLinesObj))
                    {
                        string firstLine = breakLongLinesObj.allNewProvisorySublines[0].lineContent.Trim() + "\n";
                        string secondLine = breakLongLinesObj.allNewProvisorySublines[1].lineContent.Trim();

                        _fixedSubtitlesLines[i].lineContent = firstLine + secondLine;

                        allThisLineHasBeenBrokenBefore[i] = true;
                    }
                }
            }
        }

        void breakParagraphsWithProperMethod
            (breakMethods breakingMethod, BreakLongLinesObject breakLongLinesObj,
            int maxLineLenght, int maxLineLenghtThreshold, double differenceThreshold)
        {
            switch (breakingMethod)
            {
                case breakMethods.breakAtParagraphs: breakLongLinesObj.forcedBreakLineAtParagraphs();
                    break;
                case breakMethods.breakAtPhrases:
                    breakLongLinesObj.breakLineIntoTwoAtPhrases(maxLineLenght, maxLineLenghtThreshold, differenceThreshold);
                    break;
                case breakMethods.breakAtCommas:
                    breakLongLinesObj.breakLineIntoTwoAtCommas(maxLineLenght, maxLineLenghtThreshold, differenceThreshold);
                    break;
                case breakMethods.breakAtCenter: breakLongLinesObj.breakLineIntoTwoAtCenter(maxLineLenght);
                    break;
            }
        }

        bool lineShouldBeBroken
            (string lineContent, int maxLineLenght, int maxLineLenghtThreshold, bool hasBeenBrokenBefore)
        {
            int firstLineLenght = lineContent.IndexOf('\n');

            if (firstLineLenght != -1)
            {
                if (hasBeenBrokenBefore)
                {
                    int secondLineLenght = lineContent.Length - firstLineLenght;

                    if (firstLineLenght > maxLineLenght + maxLineLenghtThreshold ||
                        secondLineLenght > maxLineLenght + maxLineLenghtThreshold)
                        return true;
                }
                else if (lineContent.Length > maxLineLenght)
                    return true;
            }
            else if (lineContent.Length > maxLineLenght)
                return true;

            return false;
        }

        void joinTheParagraphsInOneLine(string lineContent)
        {
            lineContent = lineContent.Replace("\n", "");
        }

        bool weFoundAProperCombinationOfLines(BreakLongLinesObject breakLongLinesObj)
        {
            return !string.IsNullOrEmpty(breakLongLinesObj.allNewProvisorySublines[0].lineContent);
        }

        enum breakMethods
        {
            breakAtParagraphs,
            breakAtPhrases,
            breakAtCommas,
            breakAtGrammar,
            breakAtCenter
        };

        public void separateLinesWithMoreThanTwoParagraphs(int singleLineMaxLenght)
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].lineContent.numberOfParagraphs() > 2)
                {
                    BreakLongLinesObject breakLongLinesObj = new BreakLongLinesObject();
                    breakLongLinesObj.lineToBreak = _fixedSubtitlesLines[i];

                    breakLongLinesObj.forceBreakAtParagraphs(2, singleLineMaxLenght);

                    if (breakLongLinesObj.allNewProvisorySublines.Count >= 2)
                    {
                        _fixedSubtitlesLines[i] = breakLongLinesObj.allNewProvisorySublines[0];

                        for (int j = 1; j < breakLongLinesObj.allNewProvisorySublines.Count; j++)
                        {
                            _fixedSubtitlesLines.Insert(i + j, breakLongLinesObj.allNewProvisorySublines[j]);
                        }

                        //i = i + breakLongLinesObj.allNewProvisorySublines.Count - 1;

                        if (i + 1 < fixedSubtitlesLines.Count)
                        {
                            if (_fixedSubtitlesLines[i].lineContent.numberOfParagraphs() < 2 &&
                                _fixedSubtitlesLines[i + 1].lineContent.numberOfParagraphs() < 2)
                            {
                                _fixedSubtitlesLines[i] = generalMethods.joinTwoSublines(fixedSubtitlesLines[i], fixedSubtitlesLines[i + 1]);
                                _fixedSubtitlesLines.RemoveAt(i + 1);
                            }
                        }
                    }
                }
            }
        }

        void breakLinesAtWithBreakMethod(int subLineMaxLenght, double differenceThreshold, breakMethods breakingMethod)
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].lineContent.Length > subLineMaxLenght)
                {
                    BreakLongLinesObject breakLongLinesObj = new BreakLongLinesObject();
                    breakLongLinesObj.lineToBreak = _fixedSubtitlesLines[i];

                    switch (breakingMethod)
                    {
                        case breakMethods.breakAtParagraphs: breakLongLinesObj.breakLineAtParagraphs(subLineMaxLenght); break;
                        case breakMethods.breakAtPhrases: breakLongLinesObj.breakLinesAtPhrases(subLineMaxLenght); break;
                        case breakMethods.breakAtCommas: breakLongLinesObj.breakLinesAtCommas(subLineMaxLenght); break;
                        case breakMethods.breakAtGrammar: breakLongLinesObj.breakLinesAtGrammar(subLineMaxLenght); break;
                        case breakMethods.breakAtCenter: breakLongLinesObj.breakLinesAtCenter(subLineMaxLenght); break;
                    }

                    if (breakLongLinesObj.allNewProvisorySublines.Count >= 2)
                    {
                        if (breakingMethod == breakMethods.breakAtCenter ||
                            !breakLongLinesObj.differenceBetweenLinesIsBiggerThan(differenceThreshold))
                        {
                            _fixedSubtitlesLines[i] = breakLongLinesObj.allNewProvisorySublines[0];
                            for (int j = 1; j < breakLongLinesObj.allNewProvisorySublines.Count; j++)
                            {
                                _fixedSubtitlesLines.Insert(i + j, breakLongLinesObj.allNewProvisorySublines[j]);
                            }
                            i = i + breakLongLinesObj.allNewProvisorySublines.Count - 1;
                        }
                    }
                }
            }
        }

        public void autoDetectActors(string actorDelimiter)
        {
            actorDelimiter = @"\:";

            string regexExpression = @"^.*" + actorDelimiter;

            Regex rg = new Regex(regexExpression);

            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                var matches = Regex.Matches(_fixedSubtitlesLines[i].lineContent, regexExpression);

                if (matches.Count > 0)
                    allTheActors.Add(matches[0].Value);
            }
        }

        private int numberOfNotExactBegTimes()
        {
            int counter = 0;
            foreach (subLine sl in _fixedSubtitlesLines)
            {
                if (sl.begTime.isExact == false)
                    counter++;
            }
            return counter;
        }

        private int numberOfNotExactEndTimes()
        {
            int counter = 0;
            foreach (subLine sl in _fixedSubtitlesLines)
            {
                if (sl.endTime.isExact == false)
                    counter++;
            }
            return counter;
        }

        private void reportProgress(int currentPosition, int totalCount, int frequency)
        {
            if (OnProgressUpdate != null)
            {
                if (currentPosition % frequency == 0) //only do this every five subtitles
                    OnProgressUpdate(currentPosition, totalCount);
            }
        }

        public void adjustTimeWithSpeechRecognition
            (string wavFilePath, int miliSecondsBeforeToCut, int miliSecondsAfterToCut, string microsoftSpeechCulture, string systemSpeechCulture,
            bool useMicrosoftSpeech, bool useSystemSpeech)
        {
            autoDetectActors(@"\:");

            notExactBegTimesHistory.Add(numberOfNotExactBegTimes());

            if (useMicrosoftSpeech)
            {
                generalSpeechRecAdjust
                    (wavFilePath, miliSecondsBeforeToCut, miliSecondsAfterToCut, microsoftSpeechCulture, SpeechStuff.speechRecEngine.MicrosoftSpeech,
                    SpeechStuff.typeOfSpeechRec.recWithSpaces, false);

                notExactBegTimesHistory.Add(numberOfNotExactBegTimes());

                //  generalSpeechRecAdjust
                //      (miliSecondsBeforeToCut, miliSecondsAfterToCut, microsoftSpeechCulture, SpeechStuff.speechRecEngine.MicrosoftSpeech,
                //       SpeechStuff.typeOfSpeechRec.recWithoutSpaces, false);

                notExactBegTimesHistory.Add(numberOfNotExactBegTimes());
            }

            if (useSystemSpeech)
            {
                generalSpeechRecAdjust
                    (wavFilePath, miliSecondsBeforeToCut, miliSecondsAfterToCut, systemSpeechCulture, SpeechStuff.speechRecEngine.systemSpeech,
                    SpeechStuff.typeOfSpeechRec.recWithSpaces, false);

                notExactBegTimesHistory.Add(numberOfNotExactBegTimes());

                //  generalSpeechRecAdjust
                //        (miliSecondsBeforeToCut, miliSecondsAfterToCut, systemSpeechCulture, SpeechStuff.speechRecEngine.systemSpeech,
                //        SpeechStuff.typeOfSpeechRec.recWithoutSpaces, false);

                notExactBegTimesHistory.Add(numberOfNotExactBegTimes());
            }
        }

        public void generalSpeechRecAdjust(string wavFilePath, int miliSecondsBeforeToCut, int miliSecondsAfterToCut, string speechCulture,
            SpeechStuff.speechRecEngine speechEngine, SpeechStuff.typeOfSpeechRec typeOfSearch, bool useHipothesizedSpeech)
        {
            //  Parallel.For(0, fixedSubtitlesLines.Count, i =>
            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                if (onCheckCancel())
                    return;
                reportProgress(i, _fixedSubtitlesLines.Count, 1);

                allMarkersForSpeechRecTimings.Add(false);
                if (_fixedSubtitlesLines[i].begTime.isExact == false)
                {
                    switch (speechEngine)
                    {
                        case SpeechStuff.speechRecEngine.systemSpeech:
                            SystemSpeechRecognitionAdjustTime(i, wavFilePath, miliSecondsBeforeToCut, miliSecondsAfterToCut,
                                speechCulture, typeOfSearch);
                            break;
                        case SpeechStuff.speechRecEngine.MicrosoftSpeech:
                            MicrosoftSpeechRecognitionAdjustTime(i, wavFilePath, miliSecondsBeforeToCut, miliSecondsAfterToCut,
                                speechCulture, typeOfSearch, useHipothesizedSpeech);
                            break;
                    }
                }
            }
            // });
        }

        void SystemSpeechRecognitionAdjustTime
            (int subLineIndex, string wavFilePath, int miliSecondsBeforeToCut, int miliSecondsAfterToCut,
            string speechCulture, SpeechStuff.typeOfSpeechRec typeOfSearch)
        {
            SystemSpeechRec speechStuffOBJ = new SystemSpeechRec
    (_fixedSubtitlesLines[subLineIndex], true, wavFilePath, miliSecondsBeforeToCut, miliSecondsAfterToCut,
    speechCulture, allTheActors, typeOfSearch);

            speechStuffOBJ.calculateTimesWithGrammarRecognition();

            if (speechStuffOBJ.weGotAMatch)
            {
                if (subLineIndex > 0)
                {
                    if (_fixedSubtitlesLines[subLineIndex - 1].endTime.isExact == false)
                    {
                        _fixedSubtitlesLines[subLineIndex - 1].endTime = newPreviousLineEndTime(subLineIndex, speechStuffOBJ.newBegTime);
                    }
                }
                _fixedSubtitlesLines[subLineIndex].begTime = speechStuffOBJ.newBegTime;

                allMarkersForSpeechRecTimings[subLineIndex] = false;
            }
        }

        public void MicrosoftSpeechRecognitionAdjustTime
            (int subLineIndex, string wavFilePath, int miliSecondsBeforeToCut, int miliSecondsAfterToCut,
            string speechCulture, SpeechStuff.typeOfSpeechRec typeOfSearch, bool useHipothesizedSpeech)
        {
            MicrosoftSpeechRec speechStuffOBJ = new MicrosoftSpeechRec
                (_fixedSubtitlesLines[subLineIndex], true, wavFilePath, miliSecondsBeforeToCut, miliSecondsAfterToCut,
                speechCulture, allTheActors, typeOfSearch);

            speechStuffOBJ.calculateTimesWithGrammarRecognition(useHipothesizedSpeech);

            if (speechStuffOBJ.weGotAMatch)
            {
                if (subLineIndex > 0)
                {
                    if (_fixedSubtitlesLines[subLineIndex - 1].endTime.isExact == false)
                    {
                        _fixedSubtitlesLines[subLineIndex - 1].endTime = newPreviousLineEndTime(subLineIndex, speechStuffOBJ.newBegTime);
                    }
                }
                _fixedSubtitlesLines[subLineIndex].begTime = speechStuffOBJ.newBegTime;

                allMarkersForSpeechRecTimings[subLineIndex] = false;
            }
        }

        subTimePoint newPreviousLineEndTime(int subLineIndex, subTimePoint newBegTime)
        {
            int originalDifferenceMiliseconds =
                _fixedSubtitlesLines[subLineIndex].begTime.timeInMilisec -
                _fixedSubtitlesLines[subLineIndex - 1].endTime.timeInMilisec;

            subTimePoint previousLineEndTime = new subTimePoint();

            previousLineEndTime.assignTimeFromMilisec(newBegTime.timeInMilisec - originalDifferenceMiliseconds, true, false);

            return previousLineEndTime;
        }

        public void markAllNotExactSubtitles()
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].begTime.isExact == false)
                    _fixedSubtitlesLines[i].lineContent = "+++++++" + _fixedSubtitlesLines[i].lineContent;
                if (_fixedSubtitlesLines[i].endTime.isExact == false)
                    _fixedSubtitlesLines[i].lineContent = _fixedSubtitlesLines[i].lineContent + " ******";
            }
        }

        public void correctUncertainTimesWithKeyframes(string keyframeFilePath, double fps,
            int keyframesStartBeforeMilis, int keyframesStartAfterMilis, int keyframesEndBeforeMilis, int keyframesEndAfterMilis)
        {

            KeyframeFixingObj kFObj = new KeyframeFixingObj(keyframeFilePath, fps,
                 keyframesStartBeforeMilis, keyframesStartAfterMilis, keyframesEndBeforeMilis, keyframesEndAfterMilis);

            kFObj.getAllKeyframesTimePoints();

            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].begTime.isExact == false)
                {

                }
            }
        }

        public void adjustTimingToKeyframes(string keyframeFilePath, double fps,
    int keyframesStartBeforeMilis, int keyframesStartAfterMilis, int keyframesEndBeforeMilis, int keyframesEndAfterMilis)
        {
            KeyframeFixingObj kFObj = new KeyframeFixingObj(keyframeFilePath, fps,
                 keyframesStartBeforeMilis, keyframesStartAfterMilis, keyframesEndBeforeMilis, keyframesEndAfterMilis);

            kFObj.getAllKeyframesTimePoints();

            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                _fixedSubtitlesLines[i].begTime = kFObj.getNearestKeyFrameNearBeginning(_fixedSubtitlesLines[i]);

                subTimePoint newEndTime = kFObj.getNearestKeyFrameNearEnd(_fixedSubtitlesLines[i]);
                if (newEndTime.timeInMilisec != _fixedSubtitlesLines[i].begTime.timeInMilisec)
                    _fixedSubtitlesLines[i].endTime = newEndTime;
            }
        }

        public void addLeadInAndLeadOut(int leadIn, int leadOut, bool addLeadInToYoutubeTiming, bool addLeadOutToYoutubeTiming)
        {
            addLeadIn(leadIn, addLeadInToYoutubeTiming);
            addLeadout(leadOut, addLeadOutToYoutubeTiming);
        }

        void addLeadIn(int leadIn, bool addLeanInToYoutubeTime)
        {
            if (_fixedSubtitlesLines.Count < 1)
                return;

            if (_fixedSubtitlesLines[0].begTime.isYoutubeTiming == false ||
                addLeanInToYoutubeTime && _fixedSubtitlesLines[0].begTime.isYoutubeTiming)
            {
                if (_fixedSubtitlesLines[0].begTime.timeInMilisec > leadIn)
                    _fixedSubtitlesLines[0].begTime.assignTimeFromMilisec
                        (_fixedSubtitlesLines[0].begTime.timeInMilisec - leadIn, _fixedSubtitlesLines[0].begTime.isExact, false);
                else
                    _fixedSubtitlesLines[0].begTime.assignTimeFromMilisec(0, _fixedSubtitlesLines[0].begTime.isExact, false);
            }
            for (int i = 1; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].begTime.isYoutubeTiming == false ||
                    addLeanInToYoutubeTime && _fixedSubtitlesLines[i].begTime.isYoutubeTiming)
                {
                    int dif = _fixedSubtitlesLines[i].begTime.timeInMilisec - _fixedSubtitlesLines[i - 1].endTime.timeInMilisec;

                    if (dif >= leadIn)
                        _fixedSubtitlesLines[i].begTime.assignTimeFromMilisec
                            (_fixedSubtitlesLines[i].begTime.timeInMilisec - leadIn, _fixedSubtitlesLines[i].begTime.isExact, false);
                    else
                        _fixedSubtitlesLines[i].begTime.assignTimeFromMilisec
                            (_fixedSubtitlesLines[i - 1].endTime.timeInMilisec, _fixedSubtitlesLines[i].begTime.isExact, false);
                }
            }
        }

        void addLeadout(int leadOut, bool addLeanInToYoutubeTime)
        {
            if (_fixedSubtitlesLines.Count < 1)
                return;

            for (int i = 0; i < _fixedSubtitlesLines.Count - 1; i++)
            {
                if (_fixedSubtitlesLines[i].endTime.isYoutubeTiming == false ||
                    addLeanInToYoutubeTime && _fixedSubtitlesLines[i].endTime.isYoutubeTiming)
                {
                    int dif = _fixedSubtitlesLines[i + 1].begTime.timeInMilisec - _fixedSubtitlesLines[i].endTime.timeInMilisec;

                    if (dif >= leadOut)
                        _fixedSubtitlesLines[i].endTime.assignTimeFromMilisec
                            (_fixedSubtitlesLines[i].endTime.timeInMilisec + leadOut, _fixedSubtitlesLines[i].endTime.isExact, false);
                    else
                        _fixedSubtitlesLines[i].endTime.assignTimeFromMilisec
                            (_fixedSubtitlesLines[i + 1].begTime.timeInMilisec, _fixedSubtitlesLines[i].endTime.isExact, false);
                }
            }

            int lastIndex = _fixedSubtitlesLines.Count - 1;
            if (_fixedSubtitlesLines[lastIndex].endTime.isYoutubeTiming == false ||
                addLeanInToYoutubeTime && _fixedSubtitlesLines[lastIndex].endTime.isYoutubeTiming)
            {
                _fixedSubtitlesLines[lastIndex].endTime.assignTimeFromMilisec
                    (_fixedSubtitlesLines[lastIndex].endTime.timeInMilisec +
                    leadOut, _fixedSubtitlesLines[lastIndex].endTime.isExact,
                    false);
            }
        }

        public void addForcedLeadIn(int leadIn, bool addLeadInToYoutubeTime)
        {
            if (_fixedSubtitlesLines.Count < 1)
                return;

            if (_fixedSubtitlesLines[0].begTime.isYoutubeTiming == false ||
                addLeadInToYoutubeTime && _fixedSubtitlesLines[0].begTime.isYoutubeTiming)
            {
                int newBegTime = _fixedSubtitlesLines[0].begTime.timeInMilisec - leadIn;
                if (newBegTime < 0) newBegTime = 0;

                _fixedSubtitlesLines[0].begTime.assignTimeFromMilisec(newBegTime, _fixedSubtitlesLines[0].begTime.isExact, false);
            }

            for (int i = 1; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].begTime.isYoutubeTiming == false ||
                    addLeadInToYoutubeTime && _fixedSubtitlesLines[i].begTime.isYoutubeTiming)
                {
                    int dif = _fixedSubtitlesLines[i].begTime.timeInMilisec - _fixedSubtitlesLines[i - 1].endTime.timeInMilisec;

                    _fixedSubtitlesLines[i].begTime.assignTimeFromMilisec
                           (_fixedSubtitlesLines[i].begTime.timeInMilisec - leadIn, _fixedSubtitlesLines[i].begTime.isExact, false);

                    int endTimeChange = leadIn - dif;
                    if (endTimeChange < 0) endTimeChange = 0;

                    _fixedSubtitlesLines[i - 1].endTime.assignTimeFromMilisec
                        (_fixedSubtitlesLines[i - 1].endTime.timeInMilisec - endTimeChange,
                        _fixedSubtitlesLines[i - 1].endTime.isExact, false);
                }
            }
        }

        void addForcedLeadOut(int leadOut, bool addLeadOutToYoutubeTiming)
        {

        }

        public void fixNegativeTimes()
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].begTime.timeInMilisec > _fixedSubtitlesLines[i].endTime.timeInMilisec)
                {
                    int begTimeMiliseconds = _fixedSubtitlesLines[i].begTime.timeInMilisec;
                    _fixedSubtitlesLines[i].begTime.assignTimeFromMilisec(_fixedSubtitlesLines[i].endTime.timeInMilisec, false, false);
                    _fixedSubtitlesLines[i].endTime.assignTimeFromMilisec(begTimeMiliseconds, false, false);
                }
            }
        }

        public void fixSmallTimes()
        {
            for (int i = 0; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].endTime.timeInMilisec - _fixedSubtitlesLines[i].begTime.timeInMilisec < 300)
                {
                    _fixedSubtitlesLines[i].endTime.assignTimeFromMilisec
                        (_fixedSubtitlesLines[i].endTime.timeInMilisec + 300, false, false);
                }
            }
        }

        public void makeNearSubtitlesContiguous()
        {
            for (int i = 1; i < _fixedSubtitlesLines.Count; i++)
            {
                if (_fixedSubtitlesLines[i].begTime.timeInMilisec - _fixedSubtitlesLines[i - 1].endTime.timeInMilisec < 50)
                {
                    _fixedSubtitlesLines[i - 1].endTime.assignTimeFromMilisec
                        (_fixedSubtitlesLines[i].begTime.timeInMilisec, false, false);
                }
            }
        }

        public void snapTimesToCentiseconds()
        {
            int newBegTime = generalMethods.roundINTToTen(_fixedSubtitlesLines[0].begTime.timeInMilisec);
            _fixedSubtitlesLines[0].begTime.assignTimeFromMilisec(newBegTime,
                _fixedSubtitlesLines[0].begTime.isExact,
                _fixedSubtitlesLines[0].begTime.isYoutubeTiming);

            for (int i = 1; i < _fixedSubtitlesLines.Count - 1; i++)
            {
                newBegTime = generalMethods.roundINTToTen(_fixedSubtitlesLines[i].begTime.timeInMilisec);

                int space = _fixedSubtitlesLines[i].begTime.timeInMilisec - _fixedSubtitlesLines[i - 1].endTime.timeInMilisec;

                int newPreviousEndTime = generalMethods.roundINTToTen(_fixedSubtitlesLines[i].begTime.timeInMilisec - space);              

                _fixedSubtitlesLines[i].begTime.assignTimeFromMilisec(newBegTime,
                    _fixedSubtitlesLines[i].begTime.isExact,
                    _fixedSubtitlesLines[i].begTime.isYoutubeTiming);

                _fixedSubtitlesLines[i - 1].endTime.assignTimeFromMilisec(newPreviousEndTime,
                    _fixedSubtitlesLines[i - 1].endTime.isExact,
                    _fixedSubtitlesLines[i - 1].endTime.isYoutubeTiming);
            }

            int newNEndTime = generalMethods.roundINTToTen(
                _fixedSubtitlesLines[_fixedSubtitlesLines.Count - 1].begTime.timeInMilisec);
            
            _fixedSubtitlesLines[_fixedSubtitlesLines.Count - 1].endTime.assignTimeFromMilisec(newNEndTime,
                _fixedSubtitlesLines[_fixedSubtitlesLines.Count - 1].endTime.isExact,
                _fixedSubtitlesLines[_fixedSubtitlesLines.Count - 1].endTime.isYoutubeTiming);
        }



    }
}