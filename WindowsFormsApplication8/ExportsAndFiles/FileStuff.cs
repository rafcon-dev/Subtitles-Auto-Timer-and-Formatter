using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Subtitle_Synchronizer
{
    static class FileStuff
    {
        public static string richText_DragDrop(object sender, DragEventArgs e, RichTextBox rtb)
        {
            object filename = e.Data.GetData("FileDrop");
            if (filename != null)
            {
                var list = filename as string[];

                if (list != null && !string.IsNullOrWhiteSpace(list[0]))
                {
                    rtb.Text = System.IO.File.ReadAllText(list[0]);
                    return list[0];
                }
                return null;
            }
            return null;
        }

        public static string textBox_DragDropFilePath(object sender, DragEventArgs e, TextBox txtBox)
        {
            object filename = e.Data.GetData("FileDrop");
            if (filename != null)
            {
                var list = filename as string[];

                if (list != null && !string.IsNullOrWhiteSpace(list[0]))
                {
                    txtBox.Text = list[0];
                    return list[0];
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// returns the path of the savedsubstrings
        /// </summary>
        /// <param name="subsToSaveInText"></param>
        /// <param name="unfixedSubsPath"></param>
        /// <returns></returns>
        public static string saveSubtitles(string subsToSaveInText, string unfixedSubsPath, bool askForUserInput)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            DialogResult resultDiag = DialogResult.Cancel;

            bool localAskForUserInput = askForUserInput;

            var extension = ".srt";
            string filenameToSave = string.Empty;

            if (localAskForUserInput == false)
            {
                if (File.Exists(unfixedSubsPath))
                {
                    filenameToSave = unfixedSubsPath.Substring(0, unfixedSubsPath.LastIndexOf("."));
                    filenameToSave = filenameToSave + "_beforeAegi";
                }
                else
                    localAskForUserInput = true;
            }

            if (localAskForUserInput == true)
            {
                saveFileDialog1.Filter = "srt files|*.srt|txt files|*.txt";
                saveFileDialog1.Title = "Save Fixed Subtitles";
                saveFileDialog1.RestoreDirectory = true;

                if (File.Exists(unfixedSubsPath))
                    saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(unfixedSubsPath) + "_beforeAegi";
                else
                    saveFileDialog1.FileName = "_beforeAegi";

                resultDiag = saveFileDialog1.ShowDialog();

                if (resultDiag == DialogResult.OK)
                {
                    filenameToSave = saveFileDialog1.FileName.ToString();
                    extension = Path.GetExtension(saveFileDialog1.FileName.ToString());
                }
                else
                    return null;
            }

            if (resultDiag == DialogResult.OK || localAskForUserInput == false)
            {
                string sToSave;

                switch (extension.ToLower())
                {
                    case ".txt":
                        if (localAskForUserInput == false) filenameToSave += ".txt";
                        System.IO.File.WriteAllText(filenameToSave, subsToSaveInText.ToTXT());
                        return filenameToSave;

                    case ".srt":
                        sToSave = subsToSaveInText.ToSRT();
                        if (sToSave == null)
                            return null;
                        if (localAskForUserInput == false) filenameToSave += ".srt";
                        System.IO.File.WriteAllText(filenameToSave, sToSave);
                        return filenameToSave;

                    default: MessageBox.Show("Invalid filetype selected!");
                        return null;
                }
            }
            return null;
        }

        public static string setAudioFilePath(string initialPath)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Audio files|*.wav; *.mp3; |All files|*.*";
            openFileDialog1.Title = "Select the correspondent Audio file";

            if (Directory.Exists(initialPath))
                openFileDialog1.InitialDirectory = initialPath;
            else
                openFileDialog1.InitialDirectory = AppConfigs.WorkingFolderPath;

            string resultString;

            // Show the Dialog. If the user clicked OK in the dialog and a valid filetype was selected, open it.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                resultString = openFileDialog1.FileName.ToString();
                return resultString;
            }
            return null;
        }

        public static string setKeyframesFilePath(string initialPath)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Keyframes files|*.pass; *.stats; |All files|*.*";
            openFileDialog1.Title = "Select the correspondent Keyframes file";

            if (Directory.Exists(initialPath))
                openFileDialog1.InitialDirectory = initialPath;
            else
                openFileDialog1.InitialDirectory = AppConfigs.WorkingFolderPath;

            string resultString;

            // Show the Dialog. If the user clicked OK in the dialog and a valid filetype was selected, open it.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                resultString = openFileDialog1.FileName.ToString();
                return resultString;
            }
            return null;
        }
    }
}
