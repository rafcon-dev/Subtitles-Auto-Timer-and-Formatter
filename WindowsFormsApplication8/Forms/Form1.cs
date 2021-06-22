using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.IO;
using Subtitle_Synchronizer.Properties;

using System.Threading;


using System.Speech.Recognition;
using Microsoft.Speech.Recognition;

//using System.Speech.Recognition;

namespace Subtitle_Synchronizer
{
    public partial class Form1 : Form
    {
        public static class myGlobals
        {
            private static string myUnfixedSubsPath = string.Empty;

            public static string unfixedSubsPath
            {
                get { return myUnfixedSubsPath; }
                set { myUnfixedSubsPath = value; }
            }

            private static string myFixedSubsPath = string.Empty;

            public static string fixedSubsPath
            {
                get { return myFixedSubsPath; }
                set { myFixedSubsPath = value; }
            }

            private static string myVideoFilePath = string.Empty;

            public static string VideoFilePath
            {
                get { return myVideoFilePath; }
                set { myVideoFilePath = value; }
            }

            private static string myAudioFilePath = string.Empty;
            public static string audioFilePath
            {
                get { return myAudioFilePath; }
                set { myAudioFilePath = value; }
            }
        }

        popUp_Fixing alertFixing;

        private void generic_OnProgressUpdate(int currentStep, int totalSteps)
        {
            base.Invoke((Action)delegate
            {
                alertFixing.ProgressValue = (int)(((float)currentStep / (float)totalSteps) * 100);
                alertFixing.currentStep = currentStep;
                alertFixing.totalSteps = totalSteps;
            });
        }

        private void core_OnProgressUpdate(int currentStep, int totalSteps)
        {
            generic_OnProgressUpdate(currentStep, totalSteps);
        }

        private void unfixed_OnProgressUpdate(int currentStep, int totalSteps)
        {
            generic_OnProgressUpdate(currentStep, totalSteps);
        }

        private void fixed_OnProgressUpdate(int currentStep, int totalSteps)
        {
            generic_OnProgressUpdate(currentStep, totalSteps);
        }

        public void updateCurrentTask(popUp_Fixing.tasks currentTask)
        {
            base.Invoke((Action)delegate
            {
                alertFixing.currentTask = currentTask;
            });
        }

        private void fixed_OnProgressUpdate(float value)
        {
            base.Invoke((Action)delegate
            {
                alertFixing.ProgressValue = (int)(value * 100);
            });
        }


        public Form1()
        {
            InitializeComponent();

            richText_Unfixed.DragDrop += new DragEventHandler(richText_Unfixed_DragDrop);
            richText_Unfixed.AllowDrop = true;

            richText_Fixed.DragDrop += new DragEventHandler(richText_Fixed_DragDrop);
            richText_Fixed.AllowDrop = true;

            richText_Transcript.DragDrop += new DragEventHandler(richText_Transcript_DragDrop);
            richText_Transcript.AllowDrop = true;

            textBox_VideoFilePath.DragDrop += new DragEventHandler(tableLayoutPanel_VideoFile_DragDrop);
            textBox_VideoFilePath.DragEnter += new DragEventHandler(tableLayoutPanel_VideoFile_DragEnter);
            textBox_VideoFilePath.AllowDrop = true;

            textBox_AudioFilePath.DragDrop += new DragEventHandler(tableLayoutPane6_AudioFile_DragDrop);
            textBox_AudioFilePath.DragEnter += new DragEventHandler(tableLayoutPane6_AudioFile_DragEnter);
            textBox_AudioFilePath.AllowDrop = true;


            foreach (System.Speech.Recognition.RecognizerInfo ri in System.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers())
                comboBox_SystemSpeechLang.Items.Add(ri.Culture.ToString());

            foreach (Microsoft.Speech.Recognition.RecognizerInfo ri in Microsoft.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers())
                if (ri.Culture.ToString() != string.Empty)
                    comboBox_MsftSpeechLang.Items.Add(ri.Culture.ToString());

        }

        void richText_Unfixed_DragDrop(object sender, DragEventArgs e)
        {
            string newPath = FileStuff.richText_DragDrop(sender, e, richText_Unfixed);

            if (string.IsNullOrEmpty(newPath) == false)
                Form1.myGlobals.unfixedSubsPath = newPath;
        }

        void richText_Fixed_DragDrop(object sender, DragEventArgs e)
        {
            FileStuff.richText_DragDrop(sender, e, richText_Fixed);
        }

        void richText_Transcript_DragDrop(object sender, DragEventArgs e)
        {
            FileStuff.richText_DragDrop(sender, e, richText_Transcript);
        }

        void tableLayoutPanel_VideoFile_DragDrop(object sender, DragEventArgs e)
        {
            string newPath = FileStuff.textBox_DragDropFilePath(sender, e, textBox_VideoFilePath);

            if (string.IsNullOrEmpty(newPath) == false)
                Form1.myGlobals.VideoFilePath = newPath;
        }

        void tableLayoutPanel_VideoFile_DragEnter(object sender, DragEventArgs e)
        {

        }

        void tableLayoutPane6_AudioFile_DragDrop(object sender, DragEventArgs e)
        {
            FileStuff.textBox_DragDropFilePath(sender, e, textBox_AudioFilePath);
        }

        void tableLayoutPane6_AudioFile_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox_WorkingFolder.Text = AppConfigs.WorkingFolderPath;
            //checkBox_DetectActors.Checked = AppConfigs.CheckActors;

            checkBox_PPSubs.Checked = AppConfigs.PostProcessSubs;
            setAllPostProcessingSubsControlsEnabledStatus(checkBox_PPSubs.Checked);
            numericUpDown_NumberOfPasses.Value = AppConfigs.NumberOfPasses;
            numericUpDown_SingleLineMaxLen.Value = AppConfigs.singleLineMaxLen;
            numericUpDown_singleLineThreshold.Value = AppConfigs.singleLineThreshold;
            numericUpDown_SublineMaxLen.Value = AppConfigs.sublineMaxLen;
            numericUpDown_breakLinesSimilarityThreshold.Value = AppConfigs.breakLinesSimilarityThreshold;
            numericUpDown_ParagraphsSimilarityThreshold.Value = AppConfigs.paragraphSimilarityThreshold;
            numericUpDown_LongLastingLineThreshold.Value = AppConfigs.longLastingLinesTreshold;

            checkBox_SpeechAdjustTiming.Checked = AppConfigs.useSystemSpeechRecognition;

            checkBox_useWavFile.Checked = AppConfigs.useWavFileForSpeechRec;
            checkBox_UseSysSpeech.Checked = AppConfigs.useSystemSpeechRec;
            comboBox_SystemSpeechLang.Text = AppConfigs.systemSpeechCulture;
            checkBox_UseMsftSpeech.Checked = AppConfigs.useMicrosoftSpeechRec;
            comboBox_MsftSpeechLang.Text = AppConfigs.microsoftSpeechCulture;
            numericUpDown_speechRecMilisecsBefore.Value = AppConfigs.speechRecCutTimeMilisecsBefore;
            numericUpDown_SpeechRecMilisecsAfter.Value = AppConfigs.speechRecCutTimeMilisecsAfter;

            setAllSpeechControlsEnabledStatus
                (checkBox_SpeechAdjustTiming.Checked, checkBox_UseMsftSpeech.Checked, checkBox_SpeechAdjustTiming.Checked);

            checkBox_adjustToKeyframes.Checked = AppConfigs.useKeyFrames;
            setAllUseKeyframesControlsEnabledStatus(checkBox_adjustToKeyframes.Checked);
            textBox_videoFPS.Text = AppConfigs.framesPerSecond.ToString();
            numericUpDown_BegOfSublineBeforeKeyframes.Value = AppConfigs.keyframesStartBeforeMilis;
            numericUpDown_BegOfSublineAfterKeyframes.Value = AppConfigs.keyframesStartAfterMilis;
            numericUpDown_EndOfSublineBeforeKeyframes.Value = AppConfigs.keyframesEndBeforeMilis;
            numericUpDown_EndOfSublineAfterKeyframes.Value = AppConfigs.keyframesEndAfterMilis;

            checkBox_AddMiliseconds.Checked = AppConfigs.addMiliseconds;
            setAllAddMilisecondsControlsEnabledStatus(checkBox_AddMiliseconds.Checked);
            numericUpDown_LeadIn.Value = AppConfigs.LeadIn;
            numericUpDown_LeadOut.Value = AppConfigs.LeadOut;
            numericUpDown_ForcedLeadIn.Value = AppConfigs.forcedLeadIn;
            checkBox_AddLeadInToYoutubeTiming.Checked = AppConfigs.addLeadInToYoutube;
            checkBox_AddLeadOutToYoutubeTiming.Checked = AppConfigs.addLeadOutToYoutube;

            KeyPress += new KeyPressEventHandler(CheckEnter);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //AppConfigs.CheckActors = checkBox_DetectActors.Checked;
            AppConfigs.PostProcessSubs = checkBox_PPSubs.Checked;
            AppConfigs.WorkingFolderPath = textBox_WorkingFolder.Text;
            AppConfigs.NumberOfPasses = numericUpDown_NumberOfPasses.Value;
            AppConfigs.singleLineMaxLen = numericUpDown_SingleLineMaxLen.Value;
            AppConfigs.singleLineThreshold = numericUpDown_singleLineThreshold.Value;
            AppConfigs.sublineMaxLen = numericUpDown_SublineMaxLen.Value;
            AppConfigs.breakLinesSimilarityThreshold = numericUpDown_breakLinesSimilarityThreshold.Value;
            AppConfigs.paragraphSimilarityThreshold = numericUpDown_ParagraphsSimilarityThreshold.Value;
            AppConfigs.longLastingLinesTreshold = numericUpDown_LongLastingLineThreshold.Value;

            AppConfigs.useSystemSpeechRecognition = checkBox_SpeechAdjustTiming.Checked;
            AppConfigs.useWavFileForSpeechRec = checkBox_useWavFile.Checked;
            AppConfigs.useSystemSpeechRec = checkBox_UseSysSpeech.Checked;
            AppConfigs.systemSpeechCulture = comboBox_SystemSpeechLang.Text;
            AppConfigs.useMicrosoftSpeechRec = checkBox_UseMsftSpeech.Checked;
            AppConfigs.microsoftSpeechCulture = comboBox_MsftSpeechLang.Text;
            AppConfigs.speechRecCutTimeMilisecsBefore = numericUpDown_speechRecMilisecsBefore.Value;
            AppConfigs.speechRecCutTimeMilisecsAfter = numericUpDown_SpeechRecMilisecsAfter.Value;

            AppConfigs.useKeyFrames = checkBox_adjustToKeyframes.Checked;
            AppConfigs.framesPerSecond = Convert.ToDouble(textBox_videoFPS.Text);
            AppConfigs.keyframesStartBeforeMilis = numericUpDown_BegOfSublineBeforeKeyframes.Value;
            AppConfigs.keyframesStartAfterMilis = numericUpDown_BegOfSublineAfterKeyframes.Value;
            AppConfigs.keyframesEndBeforeMilis = numericUpDown_EndOfSublineBeforeKeyframes.Value;
            AppConfigs.keyframesEndAfterMilis = numericUpDown_EndOfSublineAfterKeyframes.Value;

            AppConfigs.addMiliseconds = checkBox_AddMiliseconds.Checked;
            AppConfigs.LeadIn = numericUpDown_LeadIn.Value;
            AppConfigs.LeadIn = numericUpDown_LeadOut.Value;
            AppConfigs.forcedLeadIn = numericUpDown_ForcedLeadIn.Value;
            AppConfigs.addLeadInToYoutube = checkBox_AddLeadInToYoutubeTiming.Checked;
            AppConfigs.addLeadOutToYoutube = checkBox_AddLeadOutToYoutubeTiming.Checked;
        }

        private void CheckEnter(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                //    button_FixSubtitles.PerformClick();
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string newFixedSubsPath = FileStuff.saveSubtitles(richText_Fixed.Text, myGlobals.unfixedSubsPath, true);
            if (newFixedSubsPath != null)
            {
                myGlobals.fixedSubsPath = newFixedSubsPath;

                if (!Directory.Exists(textBox_WorkingFolder.Text))
                    textBox_WorkingFolder.Text = Path.GetDirectoryName(newFixedSubsPath);
            }
        }

        private void button_LoadUnfixex_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a file.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Subtitles files|*.srt;*.txt|All files|*.*";
            openFileDialog1.Title = "Load Unfixed Subtitles File";

            bool workingFolderIsCorrect = Directory.Exists(textBox_WorkingFolder.Text);
            if (workingFolderIsCorrect)
                openFileDialog1.InitialDirectory = textBox_WorkingFolder.Text;
            else
                openFileDialog1.InitialDirectory = AppConfigs.WorkingFolderPath;

            // Show the Dialog. If the user clicked OK in the dialog and a valid filetype was selected, open it.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richText_Unfixed.Clear();
                richText_Unfixed.Text = System.IO.File.ReadAllText(openFileDialog1.FileName.ToString());
                myGlobals.unfixedSubsPath = openFileDialog1.FileName.ToString();

                if (!workingFolderIsCorrect)
                    textBox_WorkingFolder.Text = Path.GetDirectoryName(openFileDialog1.FileName);
            }
        }

        private void button_LoadTranscript_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a file.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "txt files|*.txt; | All files|*.*";
            openFileDialog1.Title = "Load Transcript File";

            bool workingFolderIsCorrect = Directory.Exists(textBox_WorkingFolder.Text);
            if (workingFolderIsCorrect)
                openFileDialog1.InitialDirectory = textBox_WorkingFolder.Text;
            else
                openFileDialog1.InitialDirectory = AppConfigs.WorkingFolderPath;

            // Show the Dialog. If the user clicked OK in the dialog and a valid filetype was selected, open it.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richText_Transcript.Text = System.IO.File.ReadAllText(openFileDialog1.FileName.ToString());

                if (!workingFolderIsCorrect)
                    textBox_WorkingFolder.Text = Path.GetDirectoryName(openFileDialog1.FileName);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button_BrowserFolder_Click(object sender, EventArgs e)
        {
            var fsd = new FolderSelect.FolderSelectDialog();
            fsd.Title = "Select the Working Folder";

            if (Directory.Exists(textBox_WorkingFolder.Text))
                fsd.InitialDirectory = textBox_WorkingFolder.Text;
            else
                fsd.InitialDirectory = AppConfigs.WorkingFolderPath;

            if (fsd.ShowDialog(IntPtr.Zero))
            {
                textBox_WorkingFolder.Text = fsd.FileName;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorker1.CancelAsync();
                // Close the AlertForm
                //alertFixing.Close();
            }
        }

        private void getCancellationStatus(object sender, CancelEventArgs e)
        {
            e.Cancel = backgroundWorker1.CancellationPending;
        }

        private void button_FixSubtitles_Click(object sender, EventArgs e)
        {
            core coreInstance = new core();

            coreInstance.unfixedBoxString = richText_Unfixed.Text;
            coreInstance.transcript.content = richText_Transcript.Text.eliminateDuplicatedNewLines();

            coreInstance.singleLineMaxLenght = Convert.ToInt32(numericUpDown_SingleLineMaxLen.Value);
            coreInstance.singleLineThreshold = Convert.ToInt32(numericUpDown_singleLineThreshold.Value);
            coreInstance.subLineMaxLenght = Convert.ToInt32(numericUpDown_SublineMaxLen.Value);
            coreInstance.breakLinesSimilarityThreshold = Convert.ToDouble(numericUpDown_breakLinesSimilarityThreshold.Value);
            coreInstance.paragraphSimilarityThreshold = Convert.ToDouble(numericUpDown_ParagraphsSimilarityThreshold.Value);
            coreInstance.longLastingLineThreshold = Convert.ToDouble(numericUpDown_LongLastingLineThreshold.Value);

            coreInstance.addLeadInAndOut = checkBox_AddMiliseconds.Checked;
            coreInstance.leadIn = Convert.ToInt32(numericUpDown_LeadIn.Value);
            coreInstance.leadOut = Convert.ToInt32(numericUpDown_LeadOut.Value);
            coreInstance.forcedLeadIn = Convert.ToInt32(numericUpDown_ForcedLeadIn.Value);
            coreInstance.addLeadInToYoutubeTiming = checkBox_AddLeadInToYoutubeTiming.Checked;
            coreInstance.addLeadOutToYoutubeTiming = checkBox_AddLeadOutToYoutubeTiming.Checked;

            coreInstance.keyFramesFilePath = textBox_KeyframesFilePath.Text;
            coreInstance.framesPerSecond = Convert.ToDouble(textBox_videoFPS.Text);
            coreInstance.keyframesStartBeforeMilis = Convert.ToInt32(numericUpDown_BegOfSublineBeforeKeyframes.Value);
            coreInstance.keyframesStartAfterMilis = Convert.ToInt32(numericUpDown_BegOfSublineAfterKeyframes.Value);
            coreInstance.keyframesEndBeforeMilis = Convert.ToInt32(numericUpDown_EndOfSublineBeforeKeyframes.Value);
            coreInstance.keyframesEndAfterMilis = Convert.ToInt32(numericUpDown_EndOfSublineAfterKeyframes.Value);

            coreInstance.allUnfixedSubtitles.OnProgressUpdate += unfixed_OnProgressUpdate;
            coreInstance.fixedSubtitles.OnProgressUpdate += fixed_OnProgressUpdate;
            coreInstance.OnProgressUpdate += core_OnProgressUpdate;

            checkForInvalidInputs();
            coreInstance.useMicrosoftSpeechRec = checkBox_UseMsftSpeech.Checked;
            coreInstance.microsoftSpeechCulture = comboBox_MsftSpeechLang.Text;
            coreInstance.useSystemSpeechRec = checkBox_UseSysSpeech.Checked;
            coreInstance.systemSpeechCulture = comboBox_SystemSpeechLang.Text;
            coreInstance.speechRecMilisBeforeToCut = Convert.ToInt32(numericUpDown_speechRecMilisecsBefore.Value);
            coreInstance.speechRecMilisAfterToCut = Convert.ToInt32(numericUpDown_SpeechRecMilisecsAfter.Value);

            coreInstance.wavFilePath = textBox_AudioFilePath.Text;
            coreInstance.videoFilePath = textBox_VideoFilePath.Text;
            coreInstance.useWavFile = checkBox_useWavFile.Checked;

            if (backgroundWorker1.IsBusy != true)
            {
                alertFixing = new popUp_Fixing();
                alertFixing.Canceled += new EventHandler<EventArgs>(buttonCancel_Click);
                coreInstance.CheckCancel += new EventHandler<CancelEventArgs>(getCancellationStatus);

                alertFixing.Show();
                alertFixing.ShowInTaskbar = true;
                backgroundWorker1.RunWorkerAsync(coreInstance);
            }

            richText_Fixed.Clear();

        }

        private void checkForInvalidInputs()
        {
            if (checkBox_SpeechAdjustTiming.Checked)
            {
                if (checkBox_UseSysSpeech.Checked && comboBox_SystemSpeechLang.Text == string.Empty)
                {
                    MessageBox.Show("A System Speech Language must be choosen! System Speech Recognition will be skipped.");
                    checkBox_UseSysSpeech.Checked = false;
                }

                if (checkBox_UseMsftSpeech.Checked && comboBox_MsftSpeechLang.Text == string.Empty)
                {
                    MessageBox.Show("A Microsoft Speech Language must be choosen! Microsoft Speech Recognition will be skipped.");
                    checkBox_UseMsftSpeech.Checked = false;
                }

                if ((comboBox_SystemSpeechLang.Text == string.Empty && comboBox_MsftSpeechLang.Text == string.Empty)
                    ||
                    (checkBox_UseSysSpeech.Checked == false && checkBox_UseMsftSpeech.Checked == false))
                {
                    MessageBox.Show("Neither System Speech Recognition or Microsoft Speec Recognition will run! Speech Recognition will be skipped.");
                    checkBox_SpeechAdjustTiming.Checked = false;

                }
                if (checkBox_useWavFile.Checked == true && textBox_AudioFilePath.Text == string.Empty)
                {
                    MessageBox.Show("A Wav File needs to be specified, or uncheck the use WaveFile Checkbox and provide a video file!\n"
                    + " Speech Recognition will be skipped");
                    checkBox_SpeechAdjustTiming.Checked = false;
                }

                if (checkBox_useWavFile.Checked == false && textBox_VideoFilePath.Text == string.Empty)
                {
                    MessageBox.Show("A Video File needs to be specified, or check the use WaveFile Checkbox and provide a wav file!\n"
                    + " Speech Recognition will be skipped");
                    checkBox_SpeechAdjustTiming.Checked = false;
                }
            }

            if (checkBox_adjustToKeyframes.Checked && !File.Exists(textBox_KeyframesFilePath.Text))
            {
                MessageBox.Show("Keyframe File path does not point to a valid file!\n" +
                "Keyframe fixing will be skipped.");
                checkBox_adjustToKeyframes.Checked = false;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker.CancellationPending == true)
            {
                e.Cancel = true;
                return;
            }
            Object[] arg = e.Argument as Object[];
            var coreInstance = e.Argument as core;

            // Perform a time consuming operation and report progress.

            if (coreInstance != null)
            {
                Stopwatch elapsedTimeclock = new Stopwatch();
                elapsedTimeclock.Start();

                updateCurrentTask(popUp_Fixing.tasks.parsingSubs);
                if (!coreInstance.getUnfixedSubtitles()) { e.Cancel = true; return; }

                if (worker.CancellationPending == true) { e.Cancel = true; return; }

                updateCurrentTask(popUp_Fixing.tasks.findingAnchors);
                coreInstance.allUnfixedSubtitles.findAllWordAnchors(coreInstance.transcript.content);

                if (worker.CancellationPending == true) { e.Cancel = true; return; }

                updateCurrentTask(popUp_Fixing.tasks.findingWithAnchors);
                coreInstance.createProvisorySubtitlesWithAnchors();

                if (worker.CancellationPending == true) { e.Cancel = true; return; }

                updateCurrentTask(popUp_Fixing.tasks.findingWithPermutations);
                //  coreInstance.createGrossSubtitlesPermutatingForward();
                if (worker.CancellationPending == true) { e.Cancel = true; return; }

                for (int i = 0; i < Convert.ToInt32(numericUpDown_NumberOfPasses.Value); i++)
                {
                    coreInstance.createFinelySubtitlesWithPermutations();
                    if (worker.CancellationPending == true) { e.Cancel = true; return; }
                }

                if (checkBox_PPSubs.Checked == true)
                {
                    updateCurrentTask(popUp_Fixing.tasks.postProcessing);
                    coreInstance.postProcessSubtitles();
                }

                if (worker.CancellationPending == true) { e.Cancel = true; return; }

                if (checkBox_SpeechAdjustTiming.Checked == true)
                {
                    updateCurrentTask(popUp_Fixing.tasks.correctTimingWithSpeechRec);
                    coreInstance.correctTimingWithSpeechRec();
                }

                if (worker.CancellationPending == true) { e.Cancel = true; return; }

                if (checkBox_adjustToKeyframes.Checked == true)
                {
                    //updateCurrentTask(popUp_Fixing.tasks.correctTimingWithSpeechRec);
                    coreInstance.adjustTimesWithKeyFrames();
                }

                coreInstance.finalPostProcess();
               // coreInstance.MarkTheShit();////////////////////////////////////////////////////////////////////////

                if (worker.CancellationPending == true) { e.Cancel = true; return; }

                elapsedTimeclock.Stop();
                coreInstance.elapsedCalculationTimeMiliseconds = elapsedTimeclock.ElapsedMilliseconds;

                e.Result = coreInstance;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            alertFixing.Message = "Fixing subtitles, please wait... " + e.ProgressPercentage.ToString();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error\n" + e.Error.Message);
            }
            else if (e.Cancelled == true)
            {
            }
            else
            {
                core fixedCore = e.Result as core;

                richText_Fixed.Clear();
                richText_Fixed.AppendText(subtitlesToString.theseLinesToSRT(fixedCore.fixedSubtitles.fixedSubtitlesLines));

                string elapsedTime = (fixedCore.elapsedCalculationTimeMiliseconds / 1000).ToString()
                    + "," +
                    (fixedCore.elapsedCalculationTimeMiliseconds % 1000).ToString();

                label_TimeElapsed.Text = "Calculation time: " + elapsedTime + " seconds";

                richTextBox_SpeechDebug.Clear();
                for (int i = 0; i < fixedCore.fixedSubtitles.notExactBegTimesHistory.Count; i++)
                {
                    richTextBox_SpeechDebug.AppendText("SysRec Iteration" + i.ToString() + ": ");
                    richTextBox_SpeechDebug.AppendText(fixedCore.fixedSubtitles.notExactBegTimesHistory[i].ToString() + "\n");
                }
            }

            alertFixing.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            richText_Unfixed.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richText_Transcript.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richText_Fixed.Clear();
        }

        private void textBox_WorkingFolder_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_OpenInAegisub_Click(object sender, EventArgs e)
        {
            if
                (
                aegisubExport.openInAegisub
                (textBox_VideoFilePath.Text, textBox_WorkingFolder.Text, myGlobals.fixedSubsPath, richText_Fixed.Text)
                )
                textBox_VideoFilePath.Text = myGlobals.VideoFilePath;
        }

        private void button_AegisubPath_Click(object sender, EventArgs e)
        {
            aegisubExport.setAegisubPath();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
        }

        //Set video file path
        private void button4_Click_2(object sender, EventArgs e)
        {
            string initialDirectory = "Select the Video File";

            bool workingFolderIsCorrect = Directory.Exists(textBox_WorkingFolder.Text);

            if (workingFolderIsCorrect)
                initialDirectory = textBox_WorkingFolder.Text;
            else
                initialDirectory = AppConfigs.WorkingFolderPath;

            string videoFilePath = aegisubExport.setVideoFilePath(initialDirectory);

            if (videoFilePath != null)
            {
              //  if (!workingFolderIsCorrect)
                    textBox_WorkingFolder.Text = Path.GetDirectoryName(videoFilePath);

                textBox_VideoFilePath.Text = videoFilePath;
            }
        }

        private void button4_Click_3(object sender, EventArgs e)
        {
        }

        private void button5_Click(object sender, EventArgs e)
        {
        }

        private void numericUpDown_SingleLineMaxLen_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown_singleLineThreshold_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button_ResetValues_Click(object sender, EventArgs e)
        {
            AppConfigs.resetConfigValuesToDefaults();

            textBox_WorkingFolder.Text = AppConfigs.WorkingFolderPath;
            //   checkBox_DetectActors.Checked = AppConfigs.CheckActors;
            checkBox_PPSubs.Checked = AppConfigs.PostProcessSubs;
            numericUpDown_NumberOfPasses.Value = AppConfigs.NumberOfPasses;
            numericUpDown_SingleLineMaxLen.Value = AppConfigs.singleLineMaxLen;
            numericUpDown_singleLineThreshold.Value = AppConfigs.singleLineThreshold;
            numericUpDown_SublineMaxLen.Value = AppConfigs.sublineMaxLen;
            numericUpDown_breakLinesSimilarityThreshold.Value = AppConfigs.breakLinesSimilarityThreshold;
            numericUpDown_ParagraphsSimilarityThreshold.Value = AppConfigs.paragraphSimilarityThreshold;
            numericUpDown_LongLastingLineThreshold.Value = AppConfigs.longLastingLinesTreshold;

            checkBox_SpeechAdjustTiming.Checked = AppConfigs.useSystemSpeechRecognition;
            
            
            
            checkBox_useWavFile.Checked = AppConfigs.useWavFileForSpeechRec;
            checkBox_UseSysSpeech.Checked = AppConfigs.useSystemSpeechRec;
            comboBox_SystemSpeechLang.Text = AppConfigs.systemSpeechCulture;
            checkBox_UseMsftSpeech.Checked = AppConfigs.useMicrosoftSpeechRec;
            comboBox_MsftSpeechLang.Text = AppConfigs.microsoftSpeechCulture;
            numericUpDown_speechRecMilisecsBefore.Value = AppConfigs.speechRecCutTimeMilisecsBefore;
            numericUpDown_SpeechRecMilisecsAfter.Value = AppConfigs.speechRecCutTimeMilisecsAfter;

            setAllSpeechControlsEnabledStatus
                (checkBox_SpeechAdjustTiming.Checked, checkBox_UseMsftSpeech.Checked, checkBox_SpeechAdjustTiming.Checked);

            checkBox_adjustToKeyframes.Checked = AppConfigs.useKeyFrames;
            setAllUseKeyframesControlsEnabledStatus(checkBox_adjustToKeyframes.Checked);
            textBox_videoFPS.Text = AppConfigs.framesPerSecond.ToString();
            numericUpDown_BegOfSublineBeforeKeyframes.Value = AppConfigs.keyframesStartBeforeMilis;
            numericUpDown_BegOfSublineAfterKeyframes.Value = AppConfigs.keyframesStartAfterMilis;
            numericUpDown_EndOfSublineBeforeKeyframes.Value = AppConfigs.keyframesEndBeforeMilis;
            numericUpDown_EndOfSublineAfterKeyframes.Value = AppConfigs.keyframesEndAfterMilis;

            AppConfigs.addMiliseconds = checkBox_AddMiliseconds.Checked;
            numericUpDown_LeadIn.Value = AppConfigs.LeadIn;
            numericUpDown_LeadOut.Value = AppConfigs.LeadOut;
            numericUpDown_ForcedLeadIn.Value = AppConfigs.forcedLeadIn;
            checkBox_AddLeadInToYoutubeTiming.Checked = AppConfigs.addLeadInToYoutube;
            checkBox_AddLeadOutToYoutubeTiming.Checked = AppConfigs.addLeadOutToYoutube;
        }

        private void numericUpDown_ParagraphsSimilarityThreshold_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_4(object sender, EventArgs e)
        {
            // SpeechStuff.FileShit(textBox_VideoFilePath.Text);
            // SpeechStuff.ConvertWavToMp3( textBox_VideoFilePath.Text, "c:\\jeux\\cona.wav");
            //   SpeechStuff.Mp3ToWav(textBox_VideoFilePath.Text, "c:\\jeux\\cona.wav");
            //  SpeechStuff.FileShit("c:\\jeux\\cona.wav");
            //  SpeechStuff.doTheStuff("c:\\jeux\\cona.wav");

            SpeechStuff speechStuffOBJ = new SpeechStuff("c:\\jeux\\cona.wav");

            // speechStuffOBJ.transcription("c:\\jeux\\cona.wav");

            // richText_Fixed.Text = speechStuffOBJ.CONACA;

            //  SpeechStuff.doTheStuff("c:\\jeux\\cona.wav");
        }

        private void label_TimeElapsed_Click(object sender, EventArgs e)
        {

        }


        private void button5_Click_1(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.GetType() == typeof(HelpForm))
                {
                    form.BringToFront();
                    return;
                }
            }

            HelpForm hf = new HelpForm();
            hf.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string initialDirectory = string.Empty;

            bool workingFolderIsCorrect = Directory.Exists(textBox_WorkingFolder.Text);

            if (workingFolderIsCorrect)
                initialDirectory = textBox_WorkingFolder.Text;
            else
                initialDirectory = AppConfigs.WorkingFolderPath;

            string audioFilePath = FileStuff.setAudioFilePath(initialDirectory);

            if (audioFilePath != null)
            {
                if (!workingFolderIsCorrect)
                    textBox_WorkingFolder.Text = Path.GetDirectoryName(audioFilePath);

                textBox_AudioFilePath.Text = audioFilePath;
            }
        }

        private void textBox_videoFPS_TextChanged(object sender, EventArgs e)
        {
            double parsedValue;

            if (!double.TryParse(textBox_videoFPS.Text, out parsedValue))
            {
                textBox_videoFPS.Text = "";
            }
        }

        private void button_BrowseKeyframesFile_Click(object sender, EventArgs e)
        {
            string initialDirectory = string.Empty;

            bool workingFolderIsCorrect = Directory.Exists(textBox_WorkingFolder.Text);

            if (workingFolderIsCorrect)
                initialDirectory = textBox_WorkingFolder.Text;
            else
                initialDirectory = AppConfigs.WorkingFolderPath;

            string keyFramesFilePath = FileStuff.setKeyframesFilePath(initialDirectory);

            if (keyFramesFilePath != null)
            {
                if (!workingFolderIsCorrect)
                    textBox_WorkingFolder.Text = Path.GetDirectoryName(keyFramesFilePath);

                textBox_KeyframesFilePath.Text = keyFramesFilePath;
            }
        }

        private void checkBox_PPSubs_CheckedChanged(object sender, EventArgs e)
        {
            setAllPostProcessingSubsControlsEnabledStatus(checkBox_PPSubs.Checked);
        }

        private void setAllPostProcessingSubsControlsEnabledStatus(bool setToTrue)
        {
            tableLayoutPanel_FormattingPPMaxLenghts.Enabled = setToTrue;
            tableLayoutPanel_FormattingPPMinimumSimilarityBetween.Enabled = setToTrue;
            tableLayoutPanel_FormattingPostProccessMaxDifferenceInDuration.Enabled = setToTrue;
        }

        private void checkBox_SpeechAdjustTiming_CheckedChanged(object sender, EventArgs e)
        {
            setAllSpeechControlsEnabledStatus
                (checkBox_SpeechAdjustTiming.Checked, checkBox_UseSysSpeech.Checked, checkBox_UseMsftSpeech.Checked);
        }

        private void checkBox_useSysSpeech_CheckedChanged(object sender, EventArgs e)
        {
            setLanguageControlsEnabledStatus(checkBox_UseSysSpeech.Checked, checkBox_UseMsftSpeech.Checked);
        }

        private void checkBox_UseMsftSpeech_CheckedChanged(object sender, EventArgs e)
        {
            setLanguageControlsEnabledStatus(checkBox_UseSysSpeech.Checked, checkBox_UseMsftSpeech.Checked);      
        }

        private void setLanguageControlsEnabledStatus(bool sysSpeechEnabled, bool msftSpeechEnabled)
        {
            comboBox_SystemSpeechLang.Enabled = sysSpeechEnabled;
            comboBox_MsftSpeechLang.Enabled = msftSpeechEnabled;
        }

        private void setAllSpeechControlsEnabledStatus(bool setToTrue, bool sysSpeechEnabled, bool msftSpeechEnabled)
        {
            checkBox_useWavFile.Enabled = setToTrue;
            checkBox_UseSysSpeech.Enabled = setToTrue;
            comboBox_SystemSpeechLang.Enabled = setToTrue;
            checkBox_UseMsftSpeech.Enabled = setToTrue;
            comboBox_MsftSpeechLang.Enabled = setToTrue;
            numericUpDown_speechRecMilisecsBefore.Enabled = setToTrue;
            numericUpDown_SpeechRecMilisecsAfter.Enabled = setToTrue;
            Label_SpeechRecMiliSecBefore.Enabled = setToTrue;
            Label_SpeechRecMiliSecAfter.Enabled = setToTrue;

            if(setToTrue)
            {
                setLanguageControlsEnabledStatus( sysSpeechEnabled,  msftSpeechEnabled);
            }
        }

        private void checkBox_AddMiliseconds_CheckedChanged(object sender, EventArgs e)
        {
            setAllAddMilisecondsControlsEnabledStatus(checkBox_AddMiliseconds.Checked);
        }

        private void setAllAddMilisecondsControlsEnabledStatus(bool setToTrue)
        {
            label_LeadIn.Enabled = setToTrue;
            label_LeadOut.Enabled = setToTrue;
            numericUpDown_LeadIn.Enabled = setToTrue;
            numericUpDown_LeadOut.Enabled = setToTrue;
            //label_AddToYoutubeTiming.Enabled = setToTrue;
            //checkBox_AddLeadInToYoutubeTiming.Enabled = setToTrue;
            checkBox_AddLeadOutToYoutubeTiming.Enabled = setToTrue;
        }

        private void checkBox_adjustToKeyframes_CheckedChanged(object sender, EventArgs e)
        {
            setAllUseKeyframesControlsEnabledStatus(checkBox_adjustToKeyframes.Checked);
        }

        private void setAllUseKeyframesControlsEnabledStatus(bool setToTrue)
        {
            label_KeyframesFramerate.Enabled = setToTrue;
            textBox_videoFPS.Enabled = setToTrue;

            label_KeyframesNearStartOfLine.Enabled = setToTrue;
            label_KeyframesNearStartBefore.Enabled = setToTrue;
            label_KeyframesNearStartAfter.Enabled = setToTrue;
            numericUpDown_BegOfSublineBeforeKeyframes.Enabled = setToTrue;
            numericUpDown_BegOfSublineAfterKeyframes.Enabled = setToTrue;

            label_KeyframesNearEndOfLine.Enabled = setToTrue;
            label_KeyframesNearEndBefore.Enabled = setToTrue;
            label_KeyframesNeaEndAfter.Enabled = setToTrue;
            numericUpDown_EndOfSublineBeforeKeyframes.Enabled = setToTrue;
            numericUpDown_EndOfSublineAfterKeyframes.Enabled = setToTrue;
        }

        private void tableLayoutPanel9_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel13_Paint(object sender, PaintEventArgs e)
        {

        }

        private void numericUpDown_LongLastingLineThreshold_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel14_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            subsToTranscriptFactory subsToTranscriptObj = new subsToTranscriptFactory(richText_Unfixed.Text);

            subsToTranscriptObj.convertToTranscript();

            richText_Transcript.Clear();
            richText_Transcript.Text = subsToTranscriptObj.transcript;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richText_Transcript.Text);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            VideoToWav.createWavFileFromVideo(textBox_VideoFilePath.Text);
            VideoToWav.deleteWavFile();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            richText_Transcript.Text = generalMethods.removeAllParenthesis(richText_Transcript.Text);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            allUnfixedSubtitles unfixedSubs = new allUnfixedSubtitles();
            allFixedSubtitles fixedSubs = new allFixedSubtitles();

            if (!unfixedSubs.getStringSubtitles(richText_Unfixed.Text))
                return;

            unfixedSubs.trimNewLinesAtEnd();

            fixedSubs.fixedSubtitlesLines = unfixedSubs.subtitlesLines;

            fixedSubs.addForcedLeadIn
                (Convert.ToInt32(numericUpDown_LeadIn.Value), 
                checkBox_AddLeadInToYoutubeTiming.Checked);

            richText_Fixed.Clear();
            richText_Fixed.AppendText(subtitlesToString.theseLinesToSRT(fixedSubs.fixedSubtitlesLines));
        }

        private void button4_Click_5(object sender, EventArgs e)
        {
            xVidApp.encodePassFileTroughCMD(textBox_VideoFilePath.Text);
        }
    }
}