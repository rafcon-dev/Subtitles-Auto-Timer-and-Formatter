using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Subtitle_Synchronizer
{
    class PhrasesPermutationObject
    {
        List<subLine> myAllNewProvisorySublines;

        subLine myFirstSubline;
        subLine mySecondSubline;

        string myFirstLineIncompletePhrase;

        string myRegexExp;
        MatchCollection mySecondLineRegexMatches;

        bool myThereWereChanges;
        bool myStitchIsInFirstLine;
        lineStitch myStitch;
        public PhrasesPermutationObject()
        {
            firstSubline = new subLine();
            secondSubline = new subLine();
            myAllNewProvisorySublines = new List<subLine>();
            regexExpression = phrasesGeneralMethods.regexExpression;
            stitch = new lineStitch();
            thereWereChanges = false;
            stitchIsInFirstLine = false;

            //   previousRegexMatches = new MatchCollection();
            //  secondLineRegexMatches = new MatchCollection();
        }

        public lineStitch stitch
        {
            get { return myStitch; }
            set { myStitch = value; }
        }

        public string regexExpression
        {
            get { return myRegexExp; }
            set { myRegexExp = value; }
        }

        public string firstLineIncompletePhrase
        {
            get { return myFirstLineIncompletePhrase; }
            set { myFirstLineIncompletePhrase = value; }
        }

        public subLine firstSubline
        {
            get { return myFirstSubline; }
            set { myFirstSubline = value; }
        }

        public subLine secondSubline
        {
            get { return mySecondSubline; }
            set { mySecondSubline = value; }
        }
        public MatchCollection secondLineRegexMatches
        {
            get { return mySecondLineRegexMatches; }
            set { mySecondLineRegexMatches = value; }
        }

        public bool thereWereChanges
        {
            get { return myThereWereChanges; }
            set { myThereWereChanges = value; }
        }

        public bool stitchIsInFirstLine
        {
            get { return myStitchIsInFirstLine; }
            set { myStitchIsInFirstLine = value; }
        }

        public void permutateIncompletePhrase()
        {
            string provFirstLine = firstSubline.lineContent;
            string provSecondLine = secondSubline.lineContent;

            subTimePoint provFirstLineEndTime = new subTimePoint();
            subTimePoint provSecondLineBegTime = new subTimePoint();

            secondLineRegexMatches = Regex.Matches(provSecondLine, regexExpression);

            string secondLinefirstPhrase = secondLineRegexMatches.Count == 0 ? provSecondLine : secondLineRegexMatches[0].Value;

            //get first line incompletePhrase
            string firstLineIncompletePhrase = phrasesGeneralMethods.getFirstLineIncompletePhrase(provFirstLine);

            if (!string.IsNullOrEmpty(firstLineIncompletePhrase))
            {
                if (generalMethods.s1IsLessThanHalfOfS1PLUSS2(firstLineIncompletePhrase, secondLinefirstPhrase))
                {
                    provSecondLine = firstLineIncompletePhrase + provSecondLine;

                    provFirstLine = provFirstLine.Substring
                        (0, provFirstLine.Length - firstLineIncompletePhrase.Length);

                    int firstLineIncompletePhraseDuration = 
                        generalMethods.durationOf_s_InRelationTo_sl(firstLineIncompletePhrase, firstSubline);

                    if (firstLineIncompletePhrase.Length == firstSubline.lineContent.Length) //we have the whole line
                    {
                        provSecondLineBegTime = firstSubline.begTime;
                        provSecondLineBegTime.isExact = true;
                    }
                    else
                    {
                        provSecondLineBegTime.assignTimeFromMilisec
                            (secondSubline.begTime.timeInMilisec - firstLineIncompletePhraseDuration,
                            false, false);

                        provFirstLineEndTime.assignTimeFromMilisec
                            (firstSubline.endTime.timeInMilisec - firstLineIncompletePhraseDuration,
                            false, false);
                    }

                    stitch.setStitch(firstSubline.endTime, firstLineIncompletePhrase.Length);
                    stitchIsInFirstLine = false;
                }
                else
                {
                    provSecondLine = provSecondLine.Substring(secondLinefirstPhrase.Length);

                    stitch.setStitch(secondSubline.begTime, provFirstLine.Length);
                    stitchIsInFirstLine = true;

                    provFirstLine = provFirstLine + secondLinefirstPhrase;

                    int secondLinefirstPhraseDuration = 
                        generalMethods.durationOf_s_InRelationTo_sl(secondLinefirstPhrase, secondSubline);

                    if (secondLinefirstPhrase.Length == secondSubline.lineContent.Length)
                    {
                        provFirstLineEndTime = secondSubline.endTime;
                        provFirstLineEndTime.isExact = true;
                    }
                    else
                    {
                        provSecondLineBegTime.assignTimeFromMilisec
                            (secondSubline.begTime.timeInMilisec + secondLinefirstPhraseDuration,
                            false, false);

                        provFirstLineEndTime.assignTimeFromMilisec
                            (firstSubline.endTime.timeInMilisec + secondLinefirstPhraseDuration,
                            false, false);
                    }
                }

                thereWereChanges = true;

                firstSubline.lineContent = provFirstLine;
                firstSubline.endTime = provFirstLineEndTime;

                secondSubline.lineContent = provSecondLine;
                secondSubline.begTime = provSecondLineBegTime;
            }
            return;
        }
    }
}