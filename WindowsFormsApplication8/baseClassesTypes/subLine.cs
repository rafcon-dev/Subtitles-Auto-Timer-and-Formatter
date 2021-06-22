using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Subtitle_Synchronizer
{
    public class lineStitch
    {
        subTimePoint myTimeOfStitch;
        int myIndexOfStitch;
        bool myStitchIsNotCorrect;

        public lineStitch()
        {
            timeOfStitch = new subTimePoint();
            indexOfStitch = 0;
            myStitchIsNotCorrect = true;
        }
        public bool stitchIsNotCorrect
        {
            get { return myStitchIsNotCorrect; }
            set { myStitchIsNotCorrect = value; }
        }

        public int indexOfStitch
        {
            get { return myIndexOfStitch; }
            set { myIndexOfStitch = value; }
        }

        public subTimePoint timeOfStitch
        {
            get { return myTimeOfStitch; }
            set { myTimeOfStitch = value; }
        }

        public void setStitch(subTimePoint stitchTimePoint, int stitchIndex)
        {
            timeOfStitch = stitchTimePoint;
            indexOfStitch = stitchIndex;
            myStitchIsNotCorrect = false;
        }
    }

    public class subLine
    {
        public int lineIndex;
        public subTimePoint begTime = new subTimePoint();
        public subTimePoint endTime = new subTimePoint();
        string myLineContent;
        public List<wordAnchor> wordAnchors = new List<wordAnchor>();

        public int distanceBetweenThisLineAndTheOriginalRespectiveOne;

        public List<lineStitch> allStitches = new List<lineStitch>();

        public void addNewStitch(subTimePoint stitchTime, int stitchIndex)
        {
            allStitches.Add(new lineStitch()
            {
                timeOfStitch = stitchTime,
                indexOfStitch = stitchIndex,
                stitchIsNotCorrect = false,
            }); 
        }

        void markAllStitchesAsIncorrect()
        {
            foreach (lineStitch lS in allStitches)
                lS.stitchIsNotCorrect = true;
        }
        public string lineContent
        {
            get { return myLineContent; }
            set
            {
                myLineContent = value;
                markAllStitchesAsIncorrect();
            }
        }

        public subLine()
        {
        }

        public int totalMiliseconds
        {
            get { return endTime.timeInMilisec - begTime.timeInMilisec; }
        }
        public bool durationIsLessThan500ms
        {
            get { return (totalMiliseconds < 500); }
        }

        public double milisecPerCharacter
        {
            get
            {
                return Convert.ToDouble(totalMiliseconds) / Convert.ToDouble(lineContent.Length);
            }
        }

        public float charactersPerSecond
        {
            get
            {
                int totalCharacters = 0;
                for (int i = 0; i < lineContent.Length; i++)
                {
                    if (char.IsLetterOrDigit(lineContent[i]))
                        totalCharacters++;
                }
                //int totalMiliSeconds = endTime.totalMiliseconds() - begTime.totalMiliseconds();
                return (float)totalCharacters / ((float)totalMiliseconds / 1000);
            }
        }

        public bool getValuesFromLineString(string subtitleLine)
        {
            string expr;

            expr = @"(\d+)\n(\d{2}):(\d{2}):(\d{2}),(\d{3}) --> (\d{2}):(\d{2}):(\d{2}),(\d{3})\n((.+\n){0,3})\n{1,}";

            MatchCollection mc = Regex.Matches(subtitleLine, expr);

            if (mc.Count < 1)
                return false;

            foreach (Match m in mc)
            {
                lineIndex = int.Parse(m.Groups[1].Value);

                begTime.assignTime(
                    int.Parse(m.Groups[2].Value),
                    int.Parse(m.Groups[3].Value),
                    int.Parse(m.Groups[4].Value),
                    int.Parse(m.Groups[5].Value),
                    true,
                    true
                    );
                endTime.assignTime(
                   int.Parse(m.Groups[6].Value),
                   int.Parse(m.Groups[7].Value),
                   int.Parse(m.Groups[8].Value),
                   int.Parse(m.Groups[9].Value),
                   true,
                   true
                   );

                lineContent = m.Groups[10].Value;
            }
            return true;
        }

        public bool copyIndBegEndFrom(subLine sl)
        {
            bool assertCondition = (sl == null);
            if (assertCondition)
            { Debug.Assert(assertCondition, "Invalid parameters..."); return false; }

            lineIndex = sl.lineIndex;
            begTime = sl.begTime;
            endTime = sl.endTime;
            return true;
        }

        public bool assignValuesFromSubLineAndString(subLine sl, string s)
        {
            if (sl == null || s == null)
                return false;

            copyIndBegEndFrom(sl);
            lineContent = s;
            distanceBetweenThisLineAndTheOriginalRespectiveOne =
                generalMethods.levenshteinDistanceIgnorePontuationAndCase(s, sl.lineContent);
            return true;
        }
    }
}
