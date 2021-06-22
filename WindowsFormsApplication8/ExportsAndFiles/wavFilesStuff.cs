using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Speech.AudioFormat;

using Microsoft.Speech.AudioFormat;
using System.Windows.Forms;

using NAudio.Wave;
using NAudio.Lame;
using NAudio.Utils;

using System.Threading;
namespace Subtitle_Synchronizer
{
    public static class wavFilesStuff
    {

        public static void TrimWavFile
            (WaveFileReader reader, MemoryStream tempMemStream, int begTimeMiliseconds, int endTimeMiliseconds)
        {
            if (begTimeMiliseconds < 0)
                begTimeMiliseconds = 0;

            long bytesPerSecond = Convert.ToInt64(reader.WaveFormat.AverageBytesPerSecond);

            long startPos = (begTimeMiliseconds * bytesPerSecond) / 1000;
            startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

            long endPos = (endTimeMiliseconds * bytesPerSecond) / 1000;
            endPos = endPos - endPos % reader.WaveFormat.BlockAlign;

            SubTrimWavFile(reader, tempMemStream, startPos, endPos);

            tempMemStream.Position = 0;
        }

        private static void SubTrimWavFile(WaveFileReader reader, MemoryStream memStreamToWriteTo, long startPos, long endPos)
        {
            memoryStreamHeaderStuffObj msobj = new memoryStreamHeaderStuffObj(memStreamToWriteTo, reader.WaveFormat);

            msobj.writeWavFileHeaderToMemoryStream();

            reader.Position = startPos;
            byte[] buffer = new byte[reader.WaveFormat.BlockAlign * 256]; //1024 if blockalign is 4
            while (reader.Position < endPos && reader.Position < reader.Length)
            {
                long Position = reader.Position;
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);

                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        msobj.Write(buffer, 0, bytesRead);
                    }
                }
            }
            msobj.updateHeader();
            memStreamToWriteTo = msobj.memoryStream;
        }

        public static System.Speech.AudioFormat.AudioChannel SystemSpeechAudioChannel(WaveFileReader reader)
        {
            if (reader.WaveFormat.Channels == 1)
                return System.Speech.AudioFormat.AudioChannel.Mono;
            else
                return System.Speech.AudioFormat.AudioChannel.Stereo;
        }

        public static System.Speech.AudioFormat.AudioBitsPerSample SystemSpeechAudioBitsPerSample(WaveFileReader reader)
        {
            if (reader.WaveFormat.BitsPerSample == 8)
                return System.Speech.AudioFormat.AudioBitsPerSample.Eight;
            else
                return System.Speech.AudioFormat.AudioBitsPerSample.Sixteen;
        }

        public static Microsoft.Speech.AudioFormat.AudioChannel MicrosoftSpeechAudioChannel(WaveFileReader reader)
        {
            if (reader.WaveFormat.Channels == 1)
                return Microsoft.Speech.AudioFormat.AudioChannel.Mono;
            else
                return Microsoft.Speech.AudioFormat.AudioChannel.Stereo;
        }

        public static Microsoft.Speech.AudioFormat.AudioBitsPerSample MicrosoftSpeechAudioBitsPerSample(WaveFileReader reader)
        {
            if (reader.WaveFormat.BitsPerSample == 8)
                return Microsoft.Speech.AudioFormat.AudioBitsPerSample.Eight;
            else
                return Microsoft.Speech.AudioFormat.AudioBitsPerSample.Sixteen;
        }
        public static byte[] ToArray(this Stream stream)
        {
            byte[] buffer = new byte[4096];
            int reader = 0;
            MemoryStream memoryStream = new MemoryStream();
            while ((reader = stream.Read(buffer, 0, buffer.Length)) != 0)
                memoryStream.Write(buffer, 0, reader);
            return memoryStream.ToArray();
        }

        // public static bool executeFFMPEGTroughCMD(string audioFilePath, string videoFilePath)
        //  {
        //      System.Diagnostics.Process process = new System.Diagnostics.Process();
        //      System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

        //     startInfo.FileName = AppConfigs.AegisubPath;
        //     startInfo.Arguments = fixedSubsPath + " " + videoFilePath;
        //     process.StartInfo = startInfo;

        //      return process.Start();
        //   }

        public static void Mp3ToWav(string mp3File, string outputFile)
        {
            using (Mp3FileReader reader = new Mp3FileReader(mp3File))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    WaveFileWriter.CreateWaveFile(outputFile, pcmStream);
                }
            }
        }

        public static List<int> FALSOaudioEnvelope(WaveStream wavSTRM, int stepsPerSecond)
        {
            if (wavSTRM.WaveFormat.BitsPerSample != 16)
                MessageBox.Show(
                    "Bits Per Sample of the Audio File is not equal to 16! Results will be unreliable!\n" +
                "Please encode the external wav file with 16 bits");

            int samplesPerGroup = wavSTRM.WaveFormat.SampleRate / stepsPerSecond * wavSTRM.WaveFormat.Channels;

            //So that the number of samplesPerGroup is a multiple of blockalign
            samplesPerGroup = samplesPerGroup + wavSTRM.WaveFormat.BlockAlign - (samplesPerGroup % wavSTRM.WaveFormat.BlockAlign);

            byte[] buffer = new byte[samplesPerGroup];

            List<int> allEnvelopeY = new List<int>();


            wavSTRM.Position = 0;

            int nOfBytesRead = 0;

              do
            {
                short high = 0;
                short low = 0;
                short sample = 0;
                
              //  nOfBytesRead = wavSTRM.Read(buffer, 0, samplesPerGroup);

                if (nOfBytesRead == 0)
                    break;
            } while (nOfBytesRead != 0);

            return allEnvelopeY;
        }
        public static List<int> audioEnvelope(WaveStream wavSTRM, int stepsPerSecond)
        {
            if (wavSTRM.WaveFormat.BitsPerSample != 16)
                MessageBox.Show(
                    "Bits Per Sample of the Audio File is not equal to 16! Results will be unreliable!\n" +
                "Please encode the external wav file with 16 bits");

            int samplesPerGroup = wavSTRM.WaveFormat.SampleRate / stepsPerSecond * wavSTRM.WaveFormat.BlockAlign;

            //So that the number of samplesPerGroup is a multiple of blockalign
            samplesPerGroup = samplesPerGroup + wavSTRM.WaveFormat.BlockAlign - (samplesPerGroup % wavSTRM.WaveFormat.BlockAlign);

            byte[] buffer = new byte[samplesPerGroup];

            List<int> allEnvelopeY = new List<int>();

           // AudioFileReader audio = new AudioFileReader(@"c:\arm.mp3");
             //   IWavePlayer player = new WaveOut(WaveCallbackInfo.FunctionCallback());
            //   player.Init(wavSTRM);
          //    player.Play();
          //  int aa = 0;
         //   while (aa < 10)
         //   {
         //       aa++;
         //       Thread.Sleep(1000);
         //   }
            long streamPositionAtBeginning = wavSTRM.Position;
            wavSTRM.Position = 0;

            int nOfBytesRead = 0;
            do
            {
                short high = 0;
              //  short low = 0;
                short sample = 0;
                
                nOfBytesRead = wavSTRM.Read(buffer, 0, samplesPerGroup);

                if (nOfBytesRead == 0)
                    break;

                for (int j = 0; j < nOfBytesRead; j += 2)
                {
                    sample = BitConverter.ToInt16(buffer, j);
                    
                   // if (sample < low && sample >= 0) low = sample;
                    if (sample > high) high = sample;
                }

                if (nOfBytesRead > 0)
                { 
                    allEnvelopeY.Add(Convert.ToInt32(high));
                }
            } while (nOfBytesRead != 0);

            wavSTRM.Position = streamPositionAtBeginning;
            return allEnvelopeY;
        }

        public static List<double> normalizedEnvelop(this List<int> envelope)
        {
            List<double> normalizedEnvelope = new List<double>();
            
            foreach (int value in envelope)
                normalizedEnvelope.Add(Convert.ToDouble(value) / Convert.ToDouble(Int16.MaxValue));
            
            return normalizedEnvelope;
        }
    }



    class memoryStreamHeaderStuffObj
    {
        long _factSampleCountPosition = 0;
        long _dataSizePosition = 0;
        long _dataChunckSize = 0;
        MemoryStream _ms;
        WaveFormat _wf;

        public memoryStreamHeaderStuffObj(MemoryStream memStreamToWriteTo, WaveFormat inputFileWaveFormat)
        {
            _ms = memStreamToWriteTo;
            _wf = inputFileWaveFormat;
        }

        public MemoryStream memoryStream
        {
            get { return _ms; }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (_ms.Length + count > UInt32.MaxValue)
                throw new ArgumentException("WAV file too large", "count");

            _ms.Write(buffer, offset, count);
            _dataChunckSize += count;

        }

        public void writeWavFileHeaderToMemoryStream()
        {
            writeFirstPartOfHeader();

            serializeWaveFormatToMemoryStream();

            writeFactChunkToMemoryStream();

            writeDataChunkHeaderToMemoryStream();
        }

        private void writeFirstPartOfHeader()
        {
            _ms.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);

            _ms.Write(BitConverter.GetBytes((Int32)0), 0, 4); // placeholder

            _ms.Write(Encoding.UTF8.GetBytes("WAVE"), 0, 4);

            _ms.Write(Encoding.UTF8.GetBytes("fmt "), 0, 4);
        }

        private void serializeWaveFormatToMemoryStream()
        {
            _ms.Write(BitConverter.GetBytes((Int32)(18 + _wf.ExtraSize)), 0, 4); // wave format length
            _ms.Write(BitConverter.GetBytes((short)_wf.Encoding), 0, 2);
            _ms.Write(BitConverter.GetBytes((short)_wf.Channels), 0, 2);
            _ms.Write(BitConverter.GetBytes((Int32)_wf.SampleRate), 0, 4);
            _ms.Write(BitConverter.GetBytes((Int32)_wf.AverageBytesPerSecond), 0, 4);
            _ms.Write(BitConverter.GetBytes((short)_wf.BlockAlign), 0, 2);
            _ms.Write(BitConverter.GetBytes((short)_wf.BitsPerSample), 0, 2);
            _ms.Write(BitConverter.GetBytes((short)_wf.ExtraSize), 0, 2);
        }

        private void writeFactChunkToMemoryStream()
        {
            if (hasFactChunk())
            {
                _ms.Write(Encoding.UTF8.GetBytes("fact"), 0, 4);
                _ms.Write(BitConverter.GetBytes((Int32)4), 0, 4);
                _factSampleCountPosition = _ms.Position;
                _ms.Write(BitConverter.GetBytes((Int32)0), 0, 4);
            }
        }

        private void writeDataChunkHeaderToMemoryStream()
        {
            _ms.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
            _dataSizePosition = _ms.Position;
            _ms.Write(BitConverter.GetBytes((Int32)0), 0, 4); //placeholder
        }

        private bool hasFactChunk()
        {
            return (_wf.Encoding != WaveFormatEncoding.Pcm &&
                _wf.BitsPerSample != 0);
        }

        public void updateHeader()
        {
            updateRiffChunk();
            updateFactChunk();
            updateDataChunk();
        }

        private void updateRiffChunk()
        {
            _ms.Seek(4, SeekOrigin.Begin);
            _ms.Write(BitConverter.GetBytes((UInt32)_ms.Length - 8), 0, 4);
        }

        private void updateFactChunk()
        {
            if (hasFactChunk())
            {
                int bitsPerSample = (_wf.BitsPerSample * _wf.Channels);
                if (bitsPerSample != 0)
                {
                    _ms.Seek((Int32)_factSampleCountPosition, SeekOrigin.Begin);

                    _ms.Write(BitConverter.GetBytes((Int32)((_dataChunckSize * 8) / bitsPerSample)), 0, 4);
                }
            }
        }
        private void updateDataChunk()
        {
            _ms.Seek((Int32)_dataSizePosition, SeekOrigin.Begin);
            _ms.Write(BitConverter.GetBytes((UInt32)_dataChunckSize), 0, 4);
        }
    }
}