using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Globalization;
using System.Threading;

using NAudio.Wave;

//using Microsoft.CognitiveServices.Speech;


using Microsoft.Speech.Recognition;

using System.Drawing;

namespace Subtitle_Synchronizer
{

    class MicrosoftSpeechRec : SpeechStuff
    {
        List<int> allhypothesizedTimeMatches = new List<int>();
        List<string> allhypothesizedStrings = new List<string>();

        public MicrosoftSpeechRec(string inputSoundFilePath)
            : base(inputSoundFilePath)
        {
        }

        public MicrosoftSpeechRec(subLine subLineToFix, bool weWantToCorrectBegTime, string inputSoundFilePath,
           int miliSecondsBefore, int miliSecondsAfter, string speechCulture, List<string> allActors, typeOfSpeechRec typeOfSearch
            )
            : base(subLineToFix, weWantToCorrectBegTime, inputSoundFilePath,
            miliSecondsBefore, miliSecondsAfter, speechCulture, allActors, typeOfSearch)
        {
        }

        public int getTimeOfBeginningOfSoundBlock
            (List<int> soundEnvelope, int begTimeInMilis)
        {
            SoundBlockTimingFactory soundBlockTimingFctry = new SoundBlockTimingFactory
            (soundEnvelope, _stepsPerSecondForEnvelope, begTimeInMilis, 4.7, 0.0);

            soundBlockTimingFctry.findIndexAndTimeOfBeginningOfSoundBlock();

            return soundBlockTimingFctry.correctBlockBegMilis;
        }

        public void calculateTimesWithGrammarRecognition(bool useHipothesizedSpeech)
        {
            using (SpeechRecognitionEngine SR = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(_speechCulture)))
            {
                using (WaveFileReader reader = new WaveFileReader(_inputSoundFile))
                {
                    using (MemoryStream tempMemStream = new MemoryStream())
                    {
                        wavFilesStuff.TrimWavFile(reader, tempMemStream, _begTimeToCut, _endTimeToCut);
                        WaveStream wavSTRM = new WaveFileReader(tempMemStream);
                        
                        _currentAudioEnvelope = wavFilesStuff.audioEnvelope(wavSTRM, _stepsPerSecondForEnvelope);

                      //  long initPosition = wavSTRM.Position;
                     //   string path = @"c:\jeux\CORTADO.wav";
                     //   WaveFileWriter.CreateWaveFile(path, wavSTRM);
                     //   wavSTRM.Position = initPosition;
                        //  AudioFileReader audio = new AudioFileReader(@"c:\arm.mp3");
                        //    IWavePlayer player = new WaveOut(WaveCallbackInfo.FunctionCallback());
                        //   player.Init(wavSTRM);
                        //  player.Play();
                        int i = 0;
                        //  while (i < 40)
                        //{
                        //     i++;
                        //    Thread.Sleep(500);
                        // }   

                        SR.SetInputToAudioStream(
                            wavSTRM,
                            new SpeechAudioFormatInfo(reader.WaveFormat.SampleRate,
                                wavFilesStuff.MicrosoftSpeechAudioBitsPerSample(reader),
                                wavFilesStuff.MicrosoftSpeechAudioChannel(reader)
                                ));

                        Choices phraseToMatch = new Choices();

                        string phrase = string.Empty;

                        phrase = _subLineToFix.lineContent.keepOnlyNumbersLettersSpacesAndFullStops();
                        phrase = phrase.removeActorNameFromBeginningOfString(_allActors);

                        switch (_typeOfSearch)
                        {
                            case typeOfSpeechRec.recWithSpaces:
                                break;
                            case typeOfSpeechRec.recWithSpacesAndRejected:
                                break;
                            case typeOfSpeechRec.recWithoutSpaces:
                                phrase = phrase.Replace(" ", "");
                                break;
                        }
                        phraseToMatch.Add(new string[] { phrase });

                        GrammarBuilder gb = new GrammarBuilder();
                        gb.Append(phraseToMatch);

                        Thread.CurrentThread.CurrentCulture = new CultureInfo(_speechCulture);
                        gb.Culture = Thread.CurrentThread.CurrentCulture;

                        // Create the Grammar instance.
                        Grammar g = new Grammar(gb);

                        SR.LoadGrammar(g);

                        SR.SpeechRecognized +=
                            new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);

                        SR.RecognizeCompleted +=
                            new EventHandler<RecognizeCompletedEventArgs>(sre_RecognizeCompleted);

                        if (useHipothesizedSpeech)
                            SR.SpeechHypothesized +=
                                new EventHandler<SpeechHypothesizedEventArgs>(sre_hypothesizedRecognized);

                        _completed = false;

                        SR.RecognizeAsync();

                      //  SR.Recognize();

                        while (!_completed)
                            Thread.Sleep(2);

                        if (_allMatchesTimes.Count > 0)
                        {
                            _newBegTime.assignTimeFromMilisec(_allMatchesTimes[0], true, false);
                            _weGotAMatch = true;
                        }
                    }
                }
            }
        }

        void sre_hypothesizedRecognized(object sender, SpeechHypothesizedEventArgs e)
        {
            int matchBegTimeInMili = Convert.ToInt32(e.Result.Audio.AudioPosition.TotalMilliseconds);
            subTimePoint tIMEbeforeCorrection = new subTimePoint();
            tIMEbeforeCorrection.assignTimeFromMilisec(matchBegTimeInMili + _begTimeToCut, false, false);
            allhypothesizedTimeMatches.Add(matchBegTimeInMili);
            allhypothesizedStrings.Add(e.Result.Text);
            
        }
        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            int matchBegTimeInMili = Convert.ToInt32(e.Result.Audio.AudioPosition.TotalMilliseconds);

            subTimePoint begTimeTocUT = new subTimePoint();
            begTimeTocUT.assignTimeFromMilisec(_begTimeToCut, false, false);
            subTimePoint tIMEbeforeCorrection = new subTimePoint();
            tIMEbeforeCorrection.assignTimeFromMilisec(matchBegTimeInMili + _begTimeToCut, false, false);

           // matchBegTimeInMili = getTimeOfBeginningOfSoundBlock(_currentAudioEnvelope, matchBegTimeInMili) + _begTimeToCut;
            matchBegTimeInMili = matchBegTimeInMili + _begTimeToCut;

            subTimePoint timeAfterCorrection = new subTimePoint();
            timeAfterCorrection.assignTimeFromMilisec(matchBegTimeInMili, false, false);

            int matchDurationTimeMili = Convert.ToInt32(e.Result.Audio.Duration.TotalMilliseconds);

            float SECONDS = (float)matchBegTimeInMili / 1000f;

            float durationSeconds = (float)matchDurationTimeMili / 1000f;

        //    MessageBox.Show("Speech recognized: " + e.Result.Text + SECONDS.ToString() +
         //       " conf: " +
         //      e.Result.Confidence + "\nDuration: " + durationSeconds.ToString());


            _allMatchesTimes.Add(matchBegTimeInMili);
            _allMatchesDurations.Add(matchDurationTimeMili); //////////////////////check duration with the new begDuration

            //refresh the picture box
          
        }

        void sre_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
           //   MessageBox.Show("Speech recognition DONE");
            _completed = true;
        }
    }
}
