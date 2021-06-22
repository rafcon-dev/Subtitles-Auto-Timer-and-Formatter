using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Subtitle_Synchronizer
{
    public class allUnfixedSubtitles
    {
        List<subLine> mySubtitlesLines = new List<subLine>();
        string mySubtitlesAsString;

        List<wordAnchor> allUnfixedWords = new List<wordAnchor>();
        List<wordAnchor> allWordAnchors = new List<wordAnchor>();

        public delegate void ProgressUpdate(int currentStep, int totalSteps);
        public event ProgressUpdate OnProgressUpdate;

        private void allWordAnchorsToIndividualSublines()
        {
            foreach (wordAnchor wa in allWordAnchors)
            {
                wordAnchor tempAnchor = new wordAnchor();

                tempAnchor = wa;
                tempAnchor.SubIndex = wa.SubIndexInIndividualLine;
                mySubtitlesLines[wa.lineIndex].wordAnchors.Add(tempAnchor);
            }
        }

        private void lineContentsToWords()
        {
            string regexPattern = @"[^\s]+";
            int cumulativeIndex = 0;
            int lineIndexCounter = 0;
            foreach (subLine sl in mySubtitlesLines)
            {
                var matches = Regex.Matches(sl.lineContent, regexPattern);
                foreach (Match mc in matches)
                {
                    allUnfixedWords.Add(new wordAnchor()
                    {
                        SubIndex = mc.Index + cumulativeIndex,
                        content = mc.Value,
                        lineLength = sl.lineContent.Length,
                        lineIndex = lineIndexCounter,
                        SubIndexInIndividualLine = mc.Index
                    });
                }
                cumulativeIndex += sl.lineContent.Length - 1;
                lineIndexCounter++;
            }
        }
        /// <summary>
        /// Returns zero-based number of the Line that includes textIndex; indexOfStartLine is zero based
        /// </summary>
        /// <param name="textIndex"></param>
        /// <param name="indexOfStartLine"></param>
        /// <returns></returns>


        public bool getStringSubtitles(string subtitlesAsString)
        {
            mySubtitlesAsString = subtitlesAsString;
            if (!assignStringToSubtitles())
                return false;

            fixOverlapping(); //fix overlapping subtitles

            // lineContentsToString();
            lineContentsToWords();
            return true;
        }


        public List<subLine> subtitlesLines
        {
            get { return mySubtitlesLines; }
        }

        public string subtitlesAsString
        {
            get { return mySubtitlesAsString; }
            set { mySubtitlesAsString = value; }
        }

        private int skipNewlines(int endIndex)
        {
            while (mySubtitlesAsString[endIndex] == '\n' && endIndex < mySubtitlesAsString.Length - 1)
                endIndex++;
            return endIndex;
        }

        /// <summary>
        /// returns true if the assignment was successfull
        /// </summary>
        /// <returns></returns>

        private void reportProgress(int currentPosition, int totalCount)
        {
            if (OnProgressUpdate != null)
            {
                if (currentPosition % 30 == 0) //only do this every five subtitles
                    OnProgressUpdate(currentPosition, totalCount);
            }
        }

        public bool assignStringToSubtitles()
        {
            int subtitlesTotalLength = mySubtitlesAsString.Length;

            mySubtitlesAsString = mySubtitlesAsString.Insert(subtitlesTotalLength, "\n\n");
            subtitlesTotalLength += 2;

            int endIndex = mySubtitlesAsString.IndexOf("\n\n") + 1;

            int i = 0;
            int startIndex = 0, errorLine = 0;
            while (endIndex > 0)
            {
                subtitlesLines.Add(new subLine());

                //Skip extra newlines between individual subtitles REMOVE????????????????????????????
                endIndex = skipNewlines(endIndex);

                //try to assign the subtitle line values
                string temps = mySubtitlesAsString.Substring(startIndex, endIndex - startIndex + 1);
                if (!subtitlesLines[i].getValuesFromLineString(
                    temps))
                {
                    //get the subtitle Line where the format error is located
                    if (i > 0)
                        errorLine = subtitlesLines[i - 1].lineIndex + 1;
                    else
                        errorLine = 1;
                    //Throw exception with the formation error line

                    MessageBox.Show(String.Format("A format error seems to exist at around subtitle number {0}. Execution will stop.",
                        errorLine.ToString()), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;

                }

                reportProgress(endIndex, subtitlesTotalLength);

                //prepare variables for next cycle
                startIndex = endIndex;
                endIndex = mySubtitlesAsString.IndexOf("\n\n", startIndex);

                i++;
            }
            return true;
        }
        public void findAllWordAnchors(string transcript)
        {
            int tempTransIndex, tempSubIndex;

            int transcriptRunner = 0;

            wordAnchor previousAnchor = new wordAnchor();
            int lengthToSearch = 0;
            int amountToAdd = 0;

            int progressIterationCounter = 0;

            foreach (var w in allUnfixedWords)
            {
                //report progress
                progressIterationCounter++;
                if (OnProgressUpdate != null)
                {
                    if (progressIterationCounter % 100 == 0) //only do this every hundred words
                        OnProgressUpdate(progressIterationCounter, allUnfixedWords.Count);
                }
                /////////////

                //cuidado com as strings feitas de whitespace
                if (String.IsNullOrEmpty(w.content))
                    continue;

                int currentLineLength = w.lineLength;

                lengthToSearch = currentLineLength + amountToAdd;
                tempTransIndex = transcript.findFullWord
                    (transcriptRunner, lengthToSearch * 2, w.content);

                //if nothing was found in the transcript
                if (tempTransIndex == -1)
                {
                    amountToAdd += w.content.Length;
                    //transcriptRunner += w.content.Length;
                    continue;
                }

                tempSubIndex = w.SubIndex;

                //if this anchor is very far from where it should be, ignore it
                // if (tempTransIndex - previousAnchor.TransIndex >
                //    tempSubIndex - previousAnchor.SubIndex + currentLineLength / 2)
                //    continue;

                //if this anchor is not the word immediately after the previous
                if (tempTransIndex - previousAnchor.TransIndex > previousAnchor.content.Length + 2)
                {

                }

                //if this anchor is immediately after the previous
                if (tempTransIndex - previousAnchor.TransIndex <= previousAnchor.content.Length + 2)
                {
                    transcriptRunner = tempTransIndex + w.content.Length;
                    amountToAdd = 0;
                }

                //remove invalid previous anchors
                if (allWordAnchors.Count > 1)
                    while (tempTransIndex < allWordAnchors[allWordAnchors.Count - 1].TransIndex)
                    {
                        allWordAnchors.RemoveAt(allWordAnchors.Count - 1); //remove the wrong anchors
                        // unfixedRunner = allWordAnchors[allWordAnchors.Count].TransIndex + 1;
                    }

                //transcriptRunner = tempTransIndex + (w.content.Length / 2) + 1;///////////////////////////////////////////////////////////
                //basta chegar ate acima de metade da palavra para a ignorar
                // unfixedRunner += w.content.Length + 1;

                allWordAnchors.Add(new wordAnchor()
                {
                    SubIndex = tempSubIndex,
                    TransIndex = tempTransIndex,
                    content = w.content,
                    lineIndex = w.lineIndex,
                    lineLength = w.lineLength,
                    SubIndexInIndividualLine = w.SubIndexInIndividualLine
                });

                //save this anchor to memory
                previousAnchor.SubIndex = tempSubIndex;
                previousAnchor.TransIndex = tempTransIndex;
                previousAnchor.content = w.content;
                previousAnchor.lineIndex = w.lineIndex;
                previousAnchor.lineLength = w.lineLength;
            }
            //postProcess wordAnchors

            //Assign these wordAnchors to each individual subline
            allWordAnchorsToIndividualSublines();

        }

        public void fixOverlapping()
        {
            for (int i = 1; i < subtitlesLines.Count; i++)
            {
                if (subtitlesLines[i].begTime.timeInMilisec < subtitlesLines[i - 1].endTime.timeInMilisec) //is overlapping
                {
                    subtitlesLines[i - 1].endTime.assignTimeFromMilisec(subtitlesLines[i].begTime.timeInMilisec, true, false);
                }
            }
        }

        public void trimNewLinesAtEnd()
        {
            for (int i =0; i < mySubtitlesLines.Count; i++)
            {
                mySubtitlesLines[i].lineContent = mySubtitlesLines[i].lineContent.TrimEnd('\n');
            }
        }
    }
}
