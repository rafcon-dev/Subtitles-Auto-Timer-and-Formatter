using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;

using System.Windows.Forms;

namespace Subtitle_Synchronizer
{
    public class core
    {
        string myUnfixedBoxString;
        string myTranscriptBoxString;

        allUnfixedSubtitles myAllUnfixedSubtitles = new allUnfixedSubtitles();
        allFixedSubtitles myFixedSubtitles = new allFixedSubtitles();

        transcript myTranscript = new transcript();

        public delegate void ProgressUpdate(int currentStep, int totalSteps);
        public event ProgressUpdate OnProgressUpdate;

        public event EventHandler<CancelEventArgs> CheckCancel;

        long myElapsedCalculationTimeMiliseconds;

        //defs. Assignements are overriden after, these are useless
        public int singleLineMaxLenght = 0;
        public int subLineMaxLenght = 0;
        public int singleLineThreshold = 0;
        public double paragraphSimilarityThreshold = 0.15;
        public double breakLinesSimilarityThreshold = 0.5;
        public double longLastingLineThreshold = 0.5;

        public bool useMicrosoftSpeechRec;
        public string microsoftSpeechCulture = string.Empty;
        public bool useSystemSpeechRec;
        public string systemSpeechCulture = string.Empty;
        public int speechRecMilisBeforeToCut;
        public int speechRecMilisAfterToCut;


        public string waveFilePath = string.Empty;

        public bool addLeadInAndOut;
        public int leadIn = 100;
        public int leadOut = 100;
        public int forcedLeadIn = 100;
        public bool addLeadInToYoutubeTiming = false;
        public bool addLeadOutToYoutubeTiming = true;

        public string keyFramesFilePath = string.Empty;
        public double framesPerSecond = 24.0;
        public int keyframesStartBeforeMilis = 100;
        public int keyframesStartAfterMilis = 100;
        public int keyframesEndBeforeMilis = 100;
        public int keyframesEndAfterMilis = 100;

        public popUp_Fixing alertFixing;

        public string wavFilePath = string.Empty;
        public string videoFilePath = string.Empty;
        public bool useWavFile = false;

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

        public core()
        {
            myUnfixedBoxString = String.Empty;
            myTranscriptBoxString = String.Empty;
        }

        public string unfixedBoxString
        {
            get { return myUnfixedBoxString; }
            set { myUnfixedBoxString = value; }
        }

        public string transcriptBoxTranscript
        {
            get { return myTranscriptBoxString; }
            set { myTranscriptBoxString = value; }
        }

        public allUnfixedSubtitles allUnfixedSubtitles
        {
            get { return myAllUnfixedSubtitles; }
            set { myAllUnfixedSubtitles = value; }
        }

        public allFixedSubtitles fixedSubtitles
        {
            get { return myFixedSubtitles; }
            set { myFixedSubtitles = value; }
        }

        public transcript transcript
        {
            get { return myTranscript; }
            set { myTranscript = value; }
        }

        public long elapsedCalculationTimeMiliseconds
        {
            get { return myElapsedCalculationTimeMiliseconds; }
            set { myElapsedCalculationTimeMiliseconds = value; }
        }
        public bool getUnfixedSubtitles()
        {
            return myAllUnfixedSubtitles.getStringSubtitles(myUnfixedBoxString);
        }

        public void getTranscript(string s)
        {
            myTranscript.getContent(s);
        }

        public void detectActors()
        {
            string text = myTranscript.content;

            int indexOfNewLine = 0;
            indexOfNewLine = text.IndexOf("\n");
        }

        private void reportProgress(int currentPosition, int totalCount, int frequency)
        {
            if (OnProgressUpdate != null)
            {
                if (currentPosition % frequency == 0) //only do this every thirty subtitles
                    OnProgressUpdate(currentPosition, totalCount);
            }
        }
        public void createProvisorySubtitlesWithAnchors()
        {
            int transcriptRunner = 0;
            string transcriptLine;

            int transcriptLenght = myTranscript.content.Length;

            //number of tries, positive and negative to find the smallest distance
            int tries = 5;

            //Create provisory subtitles

            int i = 0;

            int progressLineCounter = 0;

            foreach (subLine sl in myAllUnfixedSubtitles.subtitlesLines) ////////////ta tudo fodido, atençao
            {
                reportProgress(progressLineCounter, myAllUnfixedSubtitles.subtitlesLines.Count, 30);
                progressLineCounter++;

                //check for cancels
                if (onCheckCancel()) return;

                if (transcriptRunner < transcriptLenght)
                {
                    transcriptLine = generalMethods.bestMatchedSubstringWithAnchors(myTranscript.content, sl, tries, transcriptRunner);

                    myFixedSubtitles.fixedSubtitlesLines.Add(new subLine());
                    myFixedSubtitles.fixedSubtitlesLines[i].assignValuesFromSubLineAndString(
                        myAllUnfixedSubtitles.subtitlesLines[i], transcriptLine);

                    transcriptRunner += transcriptLine.Length;

                    i++;
                }
                else
                    break;
            }
        }

        public void createGrossSubtitlesPermutatingForward()
        {
            myFixedSubtitles.turnNullStringsEmpty();

            for (int j = 0; j < myFixedSubtitles.fixedSubtitlesLines.Count; j++)
            {
                reportProgress(j, myFixedSubtitles.fixedSubtitlesLines.Count, 5);

                if (onCheckCancel()) return; //if the cancel button was pressed

                //start fixing...
                permutationObject newLines = generalMethods.findBestMatchedSubLinesGrossly
                    (myAllUnfixedSubtitles.subtitlesLines, myFixedSubtitles, j, transcript.content);

                myFixedSubtitles.replaceLineContentsWithStringList(newLines.allNewSubitlesStrings());
            }
        }

        public void createFinelySubtitlesWithPermutations()
        {
            //a bit of postprocessing
            myFixedSubtitles.turnNullStringsEmpty();

            for (int j = 0; j < myFixedSubtitles.fixedSubtitlesLines.Count; j++)
            {
                reportProgress(j, myFixedSubtitles.fixedSubtitlesLines.Count, 1);

                if (onCheckCancel()) return; //if the cancel button was pressed

                //start fixing...
                permutationObject newLines = generalMethods.findBestMatchedSubLines
                    (myAllUnfixedSubtitles.subtitlesLines, myFixedSubtitles, j, transcript.content);
                List<string> subtitlesnew = newLines.allNewSubitlesStrings();
                myFixedSubtitles.replaceLineContentsWithStringList(subtitlesnew);
            }
        }

        public void postProcessSubtitles()
        {
            myFixedSubtitles.removeAllEmptySublines();

            myFixedSubtitles.joinSeparatedSentences();

            myFixedSubtitles.removeExtraNewLines();

            myFixedSubtitles.breakLinesThatTakeTooMuchTime(longLastingLineThreshold);
            myFixedSubtitles.separateLinesWithMoreThanTwoParagraphs(singleLineMaxLenght);
            myFixedSubtitles.breakLongLinesIntoSeparateSublines(subLineMaxLenght, breakLinesSimilarityThreshold);
            myFixedSubtitles.adjustNewLinesInEachSubline(singleLineMaxLenght, singleLineThreshold, paragraphSimilarityThreshold);

            myFixedSubtitles.recalculateLineIndexes();

            myFixedSubtitles.trimWhiteSpace();
        }

        public void correctTimingWithSpeechRec()
        {
            myFixedSubtitles.CheckCancel += CheckCancel;

            int localMilisBeforeToCut = speechRecMilisBeforeToCut;
            int localMilisAfterToCut = speechRecMilisAfterToCut;

            string locWavFilePath;
            if (useWavFile)
                locWavFilePath = waveFilePath;
            else
                locWavFilePath = VideoToWav.createWavFileFromVideo(videoFilePath);

            if (locWavFilePath == null)
            {
                MessageBox.Show("Couldn't create wav file from video file! Speech Recognition will abort!");
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                myFixedSubtitles.adjustTimeWithSpeechRecognition
                    (locWavFilePath, localMilisBeforeToCut, speechRecMilisAfterToCut, microsoftSpeechCulture, systemSpeechCulture,
                    useMicrosoftSpeechRec, useSystemSpeechRec);
                localMilisBeforeToCut += 300;
                localMilisAfterToCut += 300;
            }

            if (!useWavFile)
                VideoToWav.deleteWavFile();
        }

        public void adjustTimesWithKeyFrames()
        {
            myFixedSubtitles.adjustTimingToKeyframes(keyFramesFilePath, framesPerSecond,
                keyframesStartBeforeMilis, keyframesStartAfterMilis, keyframesEndBeforeMilis, keyframesEndAfterMilis);
        }

        public void addLeadInAndLeadOut()
        {
            myFixedSubtitles.addLeadInAndLeadOut(leadIn, leadOut, addLeadInToYoutubeTiming, addLeadOutToYoutubeTiming);
        }

        public void finalPostProcess()
        {
            if (addLeadInAndOut)
                addLeadInAndLeadOut();

            if (true)
                myFixedSubtitles.addForcedLeadIn(forcedLeadIn, addLeadInToYoutubeTiming);

            myFixedSubtitles.fixNegativeTimes();

            myFixedSubtitles.fixSmallTimes();

            myFixedSubtitles.makeNearSubtitlesContiguous();

            myFixedSubtitles.snapTimesToCentiseconds();

        }

        public void MarkTheShit()
        {
            myFixedSubtitles.markAllNotExactSubtitles();
        }

    }
}
