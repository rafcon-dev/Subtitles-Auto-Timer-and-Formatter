using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace Subtitle_Synchronizer
{
    public class SoundBlockTimingFactory
    {
        List<int> _envelope;
        List<double> _bluredNormalizedEnvelope;
        List<double> _derivativeOfEnvelope;

        int _stepsPerSecond;
        int _timeToStartBackwardsSearch;
        int _startBackwardsSearchIndex;
        double _derivativeThreshold1;
        double _derivativeThreshold2;

        //outputs
        int _correctBlockIndex;
        int _correctBlockBegMilis;

        public SoundBlockTimingFactory
            (List<int> soundEnvelope, int stepsPerSecond, int timeToStartBackwardsSearch,
            double derivativeThreshold1, double derivativeThreshold2)
        {
            _envelope = soundEnvelope;
            _stepsPerSecond = stepsPerSecond;
            _timeToStartBackwardsSearch = timeToStartBackwardsSearch;
            _derivativeThreshold1 = derivativeThreshold1;
            _derivativeThreshold2 = derivativeThreshold2;

            _startBackwardsSearchIndex = Convert.ToInt32(Math.Floor(
                Convert.ToDouble(stepsPerSecond) * (Convert.ToDouble(_timeToStartBackwardsSearch) / 1000d)));

            _bluredNormalizedEnvelope = OneDimensionGaussianBlur.blurSigma1(_envelope.normalizedEnvelop());
            _derivativeOfEnvelope = _bluredNormalizedEnvelope.firstDerivative(generalMethods.period(stepsPerSecond));
        }

        public int correctBlockIndex
        {
            get { return _correctBlockIndex; }
        }
        public int correctBlockBegMilis
        {
            get { return _correctBlockBegMilis; }
        }

        public void findIndexAndTimeOfBeginningOfSoundBlock()
        {
            List<int> AllSoundBlocks = findAllSoundBlocks();

            _correctBlockIndex = getCorrectSoundBlockIndex(AllSoundBlocks);
            _correctBlockBegMilis = timeInMilis(_correctBlockIndex);
        }

        public List<int> findAllSoundBlocks()
        {
            bool weAreGettingOutOfBlock = false;

            List<int> AllSoundBlockBegIndex = new List<int>();

            for (int i = _startBackwardsSearchIndex; i >= 0; i--)
            {
                if (_derivativeOfEnvelope[i] > _derivativeThreshold1)
                {
                    weAreGettingOutOfBlock = true;
                }
                else if (weAreGettingOutOfBlock && _derivativeOfEnvelope[i] < _derivativeThreshold2)
                {
                    AllSoundBlockBegIndex.Add(i);
                }
            }
            return AllSoundBlockBegIndex;
        }

        int timeInMilis(int index)
        {
            return 1000 / _stepsPerSecond * index;
        }

        int getCorrectSoundBlockIndex(List<int> allSoundBlocksBegIndexes)
        {
            int blockIndex = 0;

            for (int i = 1; i < allSoundBlocksBegIndexes.Count - 1; i++)
            {
                if (timeInMilis(allSoundBlocksBegIndexes[blockIndex]) - timeInMilis(allSoundBlocksBegIndexes[i]) > 200)
                    break;
                else if (timeInMilis(allSoundBlocksBegIndexes[i - 1]) - timeInMilis(allSoundBlocksBegIndexes[i]) < 20)
                    continue;
                else if (Convert.ToDouble(_envelope[allSoundBlocksBegIndexes[i]])
                    < 1.2 * Convert.ToDouble(_envelope[allSoundBlocksBegIndexes[blockIndex]]))
                    blockIndex = i;
                else
                    continue;
            }
            return blockIndex;
        }
    }
}
