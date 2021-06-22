using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Globalization;
using System.Threading;

using System.Windows.Forms;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

using NAudio.Wave;

namespace Subtitle_Synchronizer
{
    class SystemSpeechRec : SpeechStuff
    {
        public SystemSpeechRec(string inputSoundFilePath)
            : base(inputSoundFilePath)
        {
        }

        public SystemSpeechRec(subLine subLineToFix, bool weWantToCorrectBegTime, string inputSoundFilePath,
   int miliSecondsBefore, int miliSecondsAfter, string speechCulture, List<string> allActors, typeOfSpeechRec typeOfSearch)
            : base(subLineToFix, weWantToCorrectBegTime, inputSoundFilePath,
            miliSecondsBefore, miliSecondsAfter, speechCulture, allActors, typeOfSearch)
        {
        }

        public void calculateTimesWithGrammarRecognition()
        {
            using (SpeechRecognitionEngine SR = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(_speechCulture)))
            {
                using (WaveFileReader reader = new WaveFileReader(_inputSoundFile))
                {
                    using (MemoryStream tempMemStream = new MemoryStream())
                    {
                        wavFilesStuff.TrimWavFile(reader, tempMemStream, _begTimeToCut, _endTimeToCut);
                        WaveStream wavSTRM = new WaveFileReader(tempMemStream);

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
                                wavFilesStuff.SystemSpeechAudioBitsPerSample(reader),
                                wavFilesStuff.SystemSpeechAudioChannel(reader)
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
                                SR.SpeechRecognitionRejected +=
                                    new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_RejectedSpeech);
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

                        _completed = false;

                        SR.RecognizeAsync();

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

      

        public void getTimesWithTranscription()
        {
            using (SpeechRecognitionEngine SR = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(_speechCulture)))
            {
                using (WaveFileReader reader = new WaveFileReader(_inputSoundFile))
                {
                    using (MemoryStream tempMemStream = new MemoryStream())
                    {
                        wavFilesStuff.TrimWavFile(reader, tempMemStream, _begTimeToCut, _endTimeToCut);
                        WaveStream wavSTRM = new WaveFileReader(tempMemStream);

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
                                wavFilesStuff.SystemSpeechAudioBitsPerSample(reader),
                                wavFilesStuff.SystemSpeechAudioChannel(reader)));
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                        DictationGrammar dg = new DictationGrammar();

                        SR.LoadGrammar(dg);

                        SR.LoadGrammar(new DictationGrammar());

                        SR.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>(sre_TranscriptionSpeechRecognized);

                        RecognitionResult result = SR.Recognize(new TimeSpan(0, 0, 1));

                        foreach (RecognizedWordUnit word in result.Words)
                        {

                        }
                    }
                }
            }
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            int matchBegTimeInMili = Convert.ToInt32(e.Result.Audio.AudioPosition.TotalMilliseconds);

            subTimePoint begTimeTocUT = new subTimePoint();
            begTimeTocUT.assignTimeFromMilisec(_begTimeToCut, false, false);
            subTimePoint tIMEbeforeCorrection = new subTimePoint();
            tIMEbeforeCorrection.assignTimeFromMilisec(matchBegTimeInMili + _begTimeToCut, false, false);

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
        }

        void sre_TranscriptionSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            int matchBegTimeInMili = Convert.ToInt32(e.Result.Audio.AudioPosition.TotalMilliseconds) + _begTimeToCut;

            float SECONDS = (float)matchBegTimeInMili / 1000f;

            //   MessageBox.Show("Transcription recognized: " + e.Result.Text + SECONDS.ToString() + 
            //       " conf: " +
            //      e.Result.Confidence );

            _allMatchesTimes.Add(matchBegTimeInMili);
        }

        void sre_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            //  MessageBox.Show("Speech recognition DONE");

            _completed = true;
        }

        void sre_RejectedSpeech(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            int matchBegTimeInMili = Convert.ToInt32(e.Result.Audio.AudioPosition.TotalMilliseconds) + _begTimeToCut;

            float SECONDS = (float)matchBegTimeInMili / 1000f;

            MessageBox.Show("Speech Rejected: " + e.Result.Text + SECONDS.ToString() +
                " conf: " +
               e.Result.Confidence + "\nOriginal Line: " + _subLineToFix.lineContent);

        }
    }
}
