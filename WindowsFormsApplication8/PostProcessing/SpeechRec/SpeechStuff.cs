using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Threading;
using System.Globalization;

//using Yeti.MMedia;
//using Yeti.WMFSdk;

using NAudio.Wave;
using NAudio.Lame;

using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace Subtitle_Synchronizer
{

    public class SpeechStuff
    {
        protected bool _completed = false;

        protected string _inputSoundFile = string.Empty;

        protected subLine _subLineToFix;

        protected subTimePoint _newBegTime;
        protected subTimePoint _newEndTime;

        protected int _miliSecondsBefore;
        protected int _miliSecondsAfter;

        protected int _begTimeToCut;
        protected int _endTimeToCut;

        protected bool _weAreGoingToCorrectBegTime;

        protected typeOfSpeechRec _typeOfSearch;

        protected string _speechCulture;

        protected bool _weGotAMatch;

        protected List<int> _allMatchesTimes = new List<int>();

        protected List<int> _allMatchesDurations = new List<int>();

        protected List<int> _currentAudioEnvelope = new List<int>();
        protected int _stepsPerSecondForEnvelope = 100;

      //  protected PictureBox _picBox = new PictureBox();

       // protected List<int> _allRejectedMatchesTimes = new List<int>();

        protected List<string> _allActors = new List<string>();
        public subTimePoint newBegTime
        {
            get { return _newBegTime; }
        }

        public subTimePoint newEndTime
        {
            get { return _newEndTime; }
        }

        public bool weGotAMatch
        {
            get { return _weGotAMatch; }
        }

        public SpeechStuff(string inputSoundFilePath)
        {
            _inputSoundFile = inputSoundFilePath;

            _miliSecondsBefore = 5000;
            _miliSecondsAfter = 15000;

            _weGotAMatch = false;
        }

        /// <summary>
        /// miliSecondsBack - number of miliseconds before the incorret timing where we want to start doing the speech recognition
        /// miliSecondsAfter - number of miliseconds after the incorrect timing where we want the speech recognition to stop
        /// </summary>
        /// <param name="subLineToFix"></param>
        /// <param name="correctBegTime"></param>
        /// <param name="inputSoundFilePath"></param>
        /// <param name="miliSecondsBack"></param>
        /// <param name="miliSecondsAfter"></param>
        public SpeechStuff
            (subLine subLineToFix, bool weWantToCorrectBegTime, string inputSoundFilePath,
            int miliSecondsBefore, int miliSecondsAfter, string speechCulture,
            List<string> allActors, typeOfSpeechRec typeOfSearch)
        {
            _subLineToFix = subLineToFix;

            _weAreGoingToCorrectBegTime = weWantToCorrectBegTime;

            _typeOfSearch = typeOfSearch;

            _inputSoundFile = inputSoundFilePath;

            _miliSecondsBefore = miliSecondsBefore;
            _miliSecondsAfter = miliSecondsAfter;

            _begTimeToCut = _subLineToFix.begTime.timeInMilisec - _miliSecondsBefore;
            _endTimeToCut = _subLineToFix.endTime.timeInMilisec + _miliSecondsAfter;

            _speechCulture = speechCulture;

            _newBegTime = new subTimePoint();
            _newEndTime = new subTimePoint();

            _completed = false;
            _weGotAMatch = false;

            _allActors = allActors;

        //    _picBox = picBox;
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            int matchBegTimeInMili = Convert.ToInt32(e.Result.Audio.AudioPosition.TotalMilliseconds) + _begTimeToCut;

            float SECONDS = (float)matchBegTimeInMili / 1000f;

            // MessageBox.Show("Speech recognized: " + e.Result.Text + SECONDS.ToString() +
            //     " conf: " +
            //    e.Result.Confidence);

            _allMatchesTimes.Add(matchBegTimeInMili);
        }

        void sre_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            //  MessageBox.Show("Speech recognition DONE");
            _completed = true;
        }

        void sre_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            //  MessageBox.Show("HA ALGUEM A FALAR AO MENOS: " + e.AudioPosition.ToString());
        }

        void recognizer_AudioStateChanged(object sender, AudioStateChangedEventArgs e)
        {
            //  MessageBox.Show("Estado é " + e.AudioState);

        }

        public enum typeOfSpeechRec
        {
            recWithSpaces,
            recWithoutSpaces,
            recWithSpacesAndRejected
        }

        public enum speechRecEngine
        {
            systemSpeech,
            MicrosoftSpeech
        }

    }
}
