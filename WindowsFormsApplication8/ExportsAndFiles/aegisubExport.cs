using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Subtitle_Synchronizer.Properties;
using System.Windows.Forms;
using System.IO;

namespace Subtitle_Synchronizer
{
    public static class aegisubExport
    {
        /// <summary>
        /// Returns true if we opened the project in Aegisub successfully
        /// </summary>
        /// <param name="videoFilePath"></param>
        /// <param name="workingFolderPath"></param>
        /// <param name="fixedSubsPath"></param>
        /// <param name="fixedSubtitlesInText"></param>
        /// <returns></returns>
        public static bool openInAegisub(string videoFilePath, string workingFolderPath, string fixedSubsPath, 
            string fixedSubtitlesInText)
        {
            while (!File.Exists(AppConfigs.AegisubPath))
            {
                if (MessageBox.Show("The executable in memory does not point to a valid file. Please select a valid file.",
                    "Invalid file", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)
                    == DialogResult.Cancel)
                    return false;

                if (aegisubExport.setAegisubPath() == false)
                    return false;
            }

            string newVideoFilePath = videoFilePath;
            while (!File.Exists(newVideoFilePath))
            {
                newVideoFilePath = setVideoFilePath(workingFolderPath);
                if ( newVideoFilePath == null)
                    return false; 
            }
            Form1.myGlobals.VideoFilePath = newVideoFilePath;

            //save the new subs file
                string newFixedSubsPath = FileStuff.saveSubtitles(fixedSubtitlesInText, Form1.myGlobals.unfixedSubsPath, false);
                if (newFixedSubsPath != null)
                    Form1.myGlobals.fixedSubsPath = newFixedSubsPath;
                else
                    return false;

            if (File.Exists(newVideoFilePath) && File.Exists(Form1.myGlobals.fixedSubsPath))
                return aegisubExport.executeAegisubTroughCMD(Form1.myGlobals.fixedSubsPath, newVideoFilePath);
            else
                MessageBox.Show("Some information to open this project in Aegisub is missing!");
            return false;
        }

        public static bool executeAegisubTroughCMD(string fixedSubsPath, string videoFilePath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.FileName = AppConfigs.AegisubPath;
            startInfo.Arguments = "\"" + fixedSubsPath + "\" \"" + videoFilePath + "\"";
            process.StartInfo = startInfo;

            return process.Start();
        }

        public static bool setAegisubPath()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Executable files|*.exe;|All files|*.*";
            openFileDialog1.Title = "Select the Aegisub executable";
            openFileDialog1.InitialDirectory = Environment.GetEnvironmentVariable("PROGRAMFILES");

            // Show the Dialog. If the user clicked OK in the dialog and a valid filetype was selected, open it.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                AppConfigs.AegisubPath = openFileDialog1.FileName.ToString();
                return true;
            }
            return false;
        }

        /// <summary>
        /// returns the path of the video File
        /// </summary>
        /// <param name="initialPath"></param>
        /// <returns></returns>
        public static string setVideoFilePath(string initialPath)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Video files|*.avi; *.mp4; *.wmv; *.mkv; *.flv; |All files|*.*";
            openFileDialog1.Title = "Select the correspondent Video file";

            if (Directory.Exists(initialPath))
                openFileDialog1.InitialDirectory = initialPath;
            else
                openFileDialog1.InitialDirectory = AppConfigs.WorkingFolderPath;

            string resultString = null;
            // Show the Dialog. If the user clicked OK in the dialog and a valid filetype was selected, open it.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                resultString = openFileDialog1.FileName.ToString();
                return resultString;
            }
            return resultString;
        }
    }
}
