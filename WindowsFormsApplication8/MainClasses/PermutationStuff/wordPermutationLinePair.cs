using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Synchronizer
{

    public class wordPermutationLinePair
    {
        string _line1;
        string _line2;
        int _totaldistanceBetweenAllLinesAndOriginals;
        int _numberOfLinesBetweenThePair;

        public wordPermutationLinePair()
        {
            _line1 = string.Empty;
            _line2 = string.Empty;
            _totaldistanceBetweenAllLinesAndOriginals = 0;
        }

        public wordPermutationLinePair(string line1, string line2, int distanceBetweenLinesAndOriginals, int numberOfLinesBetweenThePair)
        {
            _line1 = line1;
            _line2 = line2;
            _totaldistanceBetweenAllLinesAndOriginals = distanceBetweenLinesAndOriginals;
            _numberOfLinesBetweenThePair = numberOfLinesBetweenThePair;
        }

        public string line1
        { get { return _line1; } }

        public string line2
        { get { return _line2; } }

        public int totaldistanceBetweenAllLinesAndOriginals
        { get { return _totaldistanceBetweenAllLinesAndOriginals; } }

        public int numberOfLinesBetweenThePair
        { get { return _numberOfLinesBetweenThePair; } }

        public List<string> toStringList()
        {
            List<string> result = new List<string>();

            result.Add(_line1);

            for (int i = 1; i <= _numberOfLinesBetweenThePair; i++)
            {
                result.Add(string.Empty);
            }

            result.Add(_line2);
            return result;
        }
    }
}
