using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Subtitle_Synchronizer
{
    public static class xVidApp
    {

        private static string createAVSFile(string videoFilePath)
        {
            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            string dllFilename = currentDirectory + @"ExportsAndFiles\xVidEncoder\LSMASHSource.dll";
            string firstLine = "LoadPlugin(\"" + dllFilename + "\")\n";

            string secondLine = "LSMASHVideoSource(\"" + videoFilePath + "\")\n";

            string output = firstLine + secondLine;
            output = output.Replace("\n", Environment.NewLine);

            string outputDirectory = Path.GetDirectoryName(videoFilePath);
            string outputFileName = Path.GetFileNameWithoutExtension(videoFilePath) + @".avs";
            string outputFilePath = outputDirectory + "\\" + outputFileName;

            System.IO.File.WriteAllText(outputFilePath, output);

            return outputFilePath;
        }

        public static bool encodePassFileTroughCMD(string videoFilePath)
        {
            if (!File.Exists(videoFilePath))
                return false;

            string AVSFilePath = createAVSFile(videoFilePath);
            if (AVSFilePath == null)
                return false;

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            startInfo.FileName = currentDirectory + @"ExportsAndFiles\xVidEncoder\xvid_encraw.exe";
            string outputDirectory = Path.GetDirectoryName(videoFilePath);
            string outputFileName = Path.GetFileNameWithoutExtension(videoFilePath) + @".stats";
            string outputFilePath = outputDirectory + "\\" + outputFileName;

            // string otherArguments = "-smoother 0 -max_key_interval 250 -nopacked -vhqmode 4 -qpel -notrellis -max_bframes 1 -bvhq -bquant_ratio 162 -bquant_offset 0 -threads 1";
            string otherArguments = "";
            startInfo.Arguments = "-i " + "\"" + AVSFilePath + "\"" + otherArguments + @" -pass1 " + "\"" + outputFilePath + "\"";
            process.StartInfo = startInfo;

       //     process.StartInfo.UseShellExecute = false;
        //    process.StartInfo.RedirectStandardError = true;
        //    process.StartInfo.RedirectStandardOutput = true;

            process.Start();
         //   string outputerr = process.StandardError.ReadToEnd();
         //   string outputstd = process.StandardOutput.ReadToEnd();
          //  process.WaitForExit();

            return true;
        }
    }
}