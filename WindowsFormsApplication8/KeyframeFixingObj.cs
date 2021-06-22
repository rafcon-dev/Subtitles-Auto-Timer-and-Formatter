using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Subtitle_Synchronizer
{
    class KeyframeFixingObj
    {
        string _keyFrameFilePath;
        double _framesPerSecond;

        List<int> _allKeyframesIndexes = new List<int>();
   //     List<subTimePoint> _allKeyframesTimePoints = new List<subTimePoint>();
        List<int> _allKeyframesMiliTimePoints = new List<int>();

        int _keyFrameSearchIndexBegOfLine = 0;
        int _keyFrameSearchIndexEndOfLine = 0;

        int _keyframesStartBeforeMilis;
        int _keyframesStartAfterMilis;
        int _keyframesEndBeforeMilis;
        int _keyframesEndAfterMilis;

        public KeyframeFixingObj(string keyFrameFilePath, double framesPerSecond,
            int keyframesStartBeforeMilis, int keyframesStartAfterMilis, int keyframesEndBeforeMilis, int keyframesEndAfterMilis)
        {
            _keyFrameFilePath = keyFrameFilePath;
            _framesPerSecond = framesPerSecond;

            _keyframesStartBeforeMilis = keyframesStartBeforeMilis;
            _keyframesStartAfterMilis = keyframesStartAfterMilis;
            _keyframesEndBeforeMilis = keyframesEndBeforeMilis;
            _keyframesEndAfterMilis = keyframesEndAfterMilis;
        }

        public subTimePoint frameIndexToSubTimePoint(int frameIndex)
        {
            subTimePoint result = new subTimePoint();
            result.assignTimeFromMilisec(frameNumberToMiliseconds(frameIndex), true, false);
            return result;
        }

        public subTimePoint frameIndexAndMilisecondsToTimePoint(int frameIndex, int miliseconds)
        {
            subTimePoint result = new subTimePoint();
            result.assignTimeFromMilisec(miliseconds, true, false);
            return result;
        }

        int frameNumberToMiliseconds(int frameNumber)
        {
            double milisecondsPerFrame = 1000 / _framesPerSecond;

            return Convert.ToInt32(milisecondsPerFrame * Convert.ToDouble(frameNumber));
        }

        void getAllKeyFrameIndexes()
        {

            if (!File.Exists(_keyFrameFilePath))
                return;
            StreamReader reader = File.OpenText(_keyFrameFilePath);

            string line;
            int lineIndex = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length > 0 && (line[0] == 'i' || line[0] == 'p' || line[0] == 'b'))
                {
                    if (line[0] == 'i')
                    {
                        _allKeyframesIndexes.Add(lineIndex);
                    }
                    lineIndex++;
                }
            }
        }

        public void getAllKeyframesTimePoints()
        {
            getAllKeyFrameIndexes();

            foreach (int keyIndex in _allKeyframesIndexes)
            {
                int miliseconds = frameNumberToMiliseconds(keyIndex);
                _allKeyframesMiliTimePoints.Add(miliseconds);
                //_allKeyframesTimePoints.Add(frameIndexAndMilisecondsToTimePoint(keyIndex, miliseconds));
            }
        }

        public void getNearestKeyframes(int timeInMilis, int searchStartIndex)
        {
            if (searchStartIndex < 0)
                searchStartIndex = 0;

            if (searchStartIndex > _allKeyframesMiliTimePoints.Count)
                searchStartIndex = _allKeyframesMiliTimePoints.Count; //yes, bypass the search

            int beforeKeyframeMilis = 0;
            int afterKeyframeMilis = 0;
            bool weFoundAKeyframeBefore = false;
            bool weFoundAKeyframeAfter = false;

            //boundary conditions
            if (searchStartIndex == 0 && timeInMilis < _allKeyframesMiliTimePoints[0])
            {
                afterKeyframeMilis = _allKeyframesMiliTimePoints[0];
                weFoundAKeyframeAfter = true;
            }
            else if (searchStartIndex == _allKeyframesIndexes.Count - 1 &&
                timeInMilis >= _allKeyframesMiliTimePoints[_allKeyframesMiliTimePoints.Count - 1])
            {
                beforeKeyframeMilis = _allKeyframesMiliTimePoints[_allKeyframesMiliTimePoints.Count - 1];
                weFoundAKeyframeBefore = true;
            }
            else
                //search the rest of the list
                for (int i = searchStartIndex; i < _allKeyframesMiliTimePoints.Count - 1; i++)
                {
                    if (timeInMilis >= _allKeyframesMiliTimePoints[i]
                        &&
                        timeInMilis < _allKeyframesMiliTimePoints[i + 1])
                    {
                        beforeKeyframeMilis = _allKeyframesMiliTimePoints[i];
                        afterKeyframeMilis = _allKeyframesMiliTimePoints[i + 1];
                        weFoundAKeyframeAfter = true;
                        weFoundAKeyframeBefore = true;
                        break;
                    }

                }
        }

        public void resetSearchPosition()
        {
            _keyFrameSearchIndexBegOfLine = 0;
            _keyFrameSearchIndexEndOfLine = 0;
        }

        public subTimePoint getNearestKeyFrameNearBeginning(subLine subLineToCorrect)
        {
            return getNearestKeyFrameTime
            (subLineToCorrect.begTime, _keyframesStartBeforeMilis, _keyframesStartAfterMilis, ref _keyFrameSearchIndexBegOfLine);
        }

        public subTimePoint getNearestKeyFrameNearEnd(subLine subLineToCorrect)
        {
            return getNearestKeyFrameTime
           (subLineToCorrect.endTime, _keyframesEndBeforeMilis, _keyframesEndAfterMilis, ref _keyFrameSearchIndexEndOfLine);
        }

        subTimePoint getNearestKeyFrameTime
           (subTimePoint timeToMatch, int milisecondsToBackThreshold, int milisecondsToFrontThrehsold, ref int searchStartIndex)
        {
            if (searchStartIndex > _allKeyframesMiliTimePoints.Count || _keyFrameSearchIndexBegOfLine < 0)
                return timeToMatch;

            List<int> allNewmMilisDiff = new List<int>();
            List<int> allNearestMilis = new List<int>();

            for (int i = searchStartIndex; i < _allKeyframesMiliTimePoints.Count; i++)
            {
                int keyFrameMili = _allKeyframesMiliTimePoints[i];

                if (keyFrameMili >= timeToMatch.timeInMilisec - milisecondsToBackThreshold &&
                    keyFrameMili <= timeToMatch.timeInMilisec + milisecondsToFrontThrehsold)
                {
                    //get the start Index For The Next Iterations
                    if (allNearestMilis.Count == 0)
                        searchStartIndex = i;

                    allNewmMilisDiff.Add(Math.Abs(keyFrameMili - timeToMatch.timeInMilisec));
                    allNearestMilis.Add(keyFrameMili);
                }
                else if (allNearestMilis.Count > 0) //we are no longer in the correct range
                    break;
            }

            if (allNewmMilisDiff.Count < 1)
                return timeToMatch;
            else
            {
                subTimePoint result = new subTimePoint();
                result.assignTimeFromMilisec(allNearestMilis[allNewmMilisDiff.indexOfMinimumValue()], true, false);
                return result;
            }
        }
    }
}