using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Subtitle_Synchronizer
{
    public static class VideoToWav
    {
        static string wavFileName = "subVideoAsWAV.wav";

        public static double getFPSFromVideo(string videoFilePath)
        {
            double defaultFps = 24.0;
            if (!File.Exists(videoFilePath))
                return defaultFps;

            Process process = new System.Diagnostics.Process();

            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            process.StartInfo.FileName = currentDirectory + @"ExportsAndFiles\VideoToWav\ffmpeg.exe";
            process.StartInfo.Arguments = "-i \"" + videoFilePath + "\"";

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;

            process.Start();
            string output = process.StandardError.ReadToEnd();

            process.WaitForExit();

            string regexExpression = @"[0-9]*[\,\.]?[0-9]* fps";
            Regex rg = new Regex(regexExpression);
            var matches = rg.Matches(output);

            if (matches.Count > 0)
            {
                string matchString = matches[0].ToString();
                int indexOfSpace = matchString.IndexOf(' ');
                string fps = matchString.Substring(0, indexOfSpace);

                fps.Replace(',', '.');
                double result;

                CultureInfo usCulture = new CultureInfo("en-US");
                NumberFormatInfo dbNumberFormat = usCulture.NumberFormat;

                double doubleFPS = double.Parse(fps, dbNumberFormat);

                return doubleFPS;
            }
            else
                return defaultFps;
        }

        public static string createWavFileFromVideo(string videoFilePath)
        {
            if (!File.Exists(videoFilePath))
                return null;

            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            startInfo.FileName = currentDirectory + @"ExportsAndFiles\VideoToWav\ffmpeg.exe";
            startInfo.Arguments = "-i \"" + videoFilePath + "\" \"" + wavFileName + "\"";
            process.StartInfo = startInfo;

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }

            if (File.Exists(currentDirectory + wavFileName))
                return currentDirectory + wavFileName;

            return null;
        }

        public static void deleteWavFile()
        {
            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(currentDirectory + wavFileName))
            {
                try
                {
                    File.Delete(currentDirectory + wavFileName);
                }
                catch (System.IO.IOException e)
                {

                    System.Windows.Forms.MessageBox.Show("Couldn't delete wav file! " + e.Message);
                    return;
                }
            }
        }
    }
}
