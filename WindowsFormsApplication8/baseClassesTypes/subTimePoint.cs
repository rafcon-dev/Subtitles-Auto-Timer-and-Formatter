using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Synchronizer
{
    public class subTimePoint
    {
        int _hour;
        int _minute;
        int _second;
        int _milisecond;

        bool _isExact;

        int _equivalentTimeInMiliseconds;

        bool _isYoutubeTime;

        public int hour { get { return _hour; } }
        public int minute { get { return _minute; } }
        public int second { get { return _second; } }
        public int milisecond { get { return _milisecond; } }

        public bool isExact
        {
            get { return _isExact; }
            set { _isExact = value; }
        }

        public bool isYoutubeTiming
        {
            get { return _isYoutubeTime; }
            set { _isYoutubeTime = value; }
        }
        public subTimePoint()
        {
            _hour = 0;
            _minute = 0;
            _second = 0;
            _milisecond = 0;
            _isExact = false;
            _equivalentTimeInMiliseconds = 0;
        }

        public int timeInMilisec { get { return _equivalentTimeInMiliseconds; } }

        public void assignTimeFromMilisec(int milis, bool isExactTime, bool isYoutubeTime)
        {
            int remainer = milis;

            _hour = remainer / (1000 * 60 * 60);
            remainer = remainer - hour * (1000 * 60 * 60);

            _minute = remainer / (1000 * 60);
            remainer = remainer - minute * (1000 * 60);

            _second = remainer / 1000;
            remainer = remainer - second * 1000;

            _milisecond = remainer;

            _isExact = isExactTime;

            _isYoutubeTime = isYoutubeTime;

            _equivalentTimeInMiliseconds = milis;
        }

        public void assignTime(int hours, int minutes, int seconds, int miliseconds, bool isExactTime, bool isYoutubeTime)
        {
            _hour = hours;
            _minute = minutes;
            _second = seconds;
            _milisecond = miliseconds;
            _isExact = isExactTime;
            _isYoutubeTime = isYoutubeTime;

            _equivalentTimeInMiliseconds = milisecondsFromTime(hours, minutes, seconds, miliseconds);
        }

        int milisecondsFromTime(int hours, int minutes, int seconds, int miliseconds)
        {
            int result = hours;
            result = result * 60 + minutes;
            result = result * 60 + seconds;
            result = result * 1000 + miliseconds;
            return result;
        }
    }
}