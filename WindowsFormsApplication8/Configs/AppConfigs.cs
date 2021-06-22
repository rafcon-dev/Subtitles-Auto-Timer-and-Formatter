using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Forms;
using System.IO;

using System.Speech.Recognition;
using System.Globalization;

namespace Subtitle_Synchronizer
{
    //How to add new settings
    //Add a default value for the setting
    //Add a configkey
    //Add a string version of the configkey in the method ToStringKey
    //Add a property, using as template any of the others properties
    //Add a line in the method resetConfigValuesToDefaults pertaining to the new setting
    //Add a line in the method generalDefaultFromConfigKey pertaining to the new setting
    //In the Form where the setting is inputted:
    //Add a line in the form_Load method, reading the setting from AppConfigs(this class)
    //Add a line in the form_Closing method, writing the setting to AppConfigs(this class)
    //Add a line to the button_ResetValues, if it exists in the said form

    class AppConfigs
    {
        //add default values here
        static string myDefaultAegisubPath = string.Empty;
        static bool myDefaultCheckActors = false;
        static bool myDefaultPostProcessSubtitles = true;
        static decimal myDefaultNumberOfPasses = 2M;
        static decimal myDefaultSingleLineMaxLen = 40M;
        static decimal myDefaultSingleLineThreshold = 5M;
        static decimal myDefaultSublineMaxLen = 85M;
        static decimal myDefaultBreakLinesSimilarityThreshold = 0.7M;
        static decimal myDefaultParagraphSimilarityThreshold = 0.15M;
        static decimal myDefaultLongLastingLinesTreshold = 0.3M;

        static bool myDefaultUseSpeechRecognition = true;
        static bool myDefaultUseWavFileForSpeechRec = false;
        static bool myDefaultUseSystemSpeech = true;
        static string myDefaultSystemSpeechCulture = string.Empty;
        static bool myDefaultUseMicrosoftSpeech = true;
        static string myDefaultMicrosoftSPeechCulture = string.Empty;
        static decimal myDefaultspeechRecCutTimeMilisecsBefore = 200M;
        static decimal myDefaultspeechRecCutTimeMilisecsAfter = 4000M;

        static string myDefaultWorkingFolderPath = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);

        static bool myDefaultUseKeyframes = true;
        static double myDefaultFramesPerSecond = 24;
        static decimal myDefaultKeyframesStartBeforeMilis = 250M;
        static decimal myDefaultKeyframesStartAfterMilis = 300M;
        static decimal myDefaultKeyframesEndBeforeMilis = 500M;
        static decimal myDefaultKeyframesEndAfterMilis = 500M;

        static bool myDefaultAddMiliseconds = true;
        static decimal myDefaultLeadIn = 150M;
        static decimal myDefaultLeadOut = 300M;
        static decimal myDefaultForcedLeadIn = 200M;
        static bool myDefaultAddLeadInToYoutube = false;
        static bool myDefaultAddLeadOutToYoutube = true;

        //Add configkeys here
        enum configKeys
        {
            AegisubPath,
            WorkingFolderPath,
            CheckActors,
            PostProcessSubs,
            NumberOfPasses,
            singleLineMaxLen,
            singleLineThreshold,
            sublineMaxLen,
            breakLinesSimilarityThreshold,
            paragraphSimilarityThreshold,
            longLastingLinesTreshold,

            useSpeechRecognition,
            useWavFileForSpeechRec,
            useSystemSpeechRec,
            systemSpeechCulture,
            useMicrosoftSpeechRec,
            microsoftSpeechCulture,
            speechRecCutTimeMilisecsBefore,
            speechRecCutTimeMilisecsAfter,

            useKeyFrames,
            framesPerSecond,
            keyframesStartBeforeMilis,
            keyframesStartAfterMilis,
            keyframesEndBeforeMilis,
            keyframesEndAfterMilis,

            addMiliseconds,
            leadIn,
            leadOut,
            forcedLeadIn,
            addLeadInToYoutube,
            addLeadOutToYoutube
        };

        //Add a string version of the configkey here
        static string ToStringKey(configKeys key)
        {
            switch (key)
            {
                case configKeys.AegisubPath:
                    return "AegisubPath";
                case configKeys.WorkingFolderPath:
                    return "WorkingFolderPath";
                case configKeys.CheckActors:
                    return "CheckActors";
                case configKeys.PostProcessSubs:
                    return "PostProcessSubs";
                case configKeys.NumberOfPasses:
                    return "NumberOfPasses";
                case configKeys.singleLineMaxLen:
                    return "singleLineMaxLen";
                case configKeys.sublineMaxLen:
                    return "sublineMaxLen";
                case configKeys.breakLinesSimilarityThreshold:
                    return "breakLinesSimilarityThreshold";
                case configKeys.paragraphSimilarityThreshold:
                    return "paragraphSimilarityThreshold";
                case configKeys.longLastingLinesTreshold:
                    return "longLastingLinesTreshold";

                case configKeys.useSpeechRecognition:
                    return "useSpeechRecognition";
                case configKeys.useWavFileForSpeechRec:
                    return "useWavFileForSpeechRec";
                case configKeys.useSystemSpeechRec:
                    return "useSystemSpeechRec";
                case configKeys.systemSpeechCulture:
                    return "systemSpeechCulture";
                case configKeys.useMicrosoftSpeechRec:
                    return "useMicrosoftSpeechRec";
                case configKeys.microsoftSpeechCulture:
                    return "microsoftSpeechCulture";
                case configKeys.speechRecCutTimeMilisecsBefore:
                    return "speechRecCutTimeMilisecsBefore";
                case configKeys.speechRecCutTimeMilisecsAfter:
                    return "speechRecCutTimeMilisecsAfter";

                case configKeys.useKeyFrames:
                    return "useKeyFrames";
                case configKeys.framesPerSecond:
                    return "framesPerSecond";
                case configKeys.keyframesStartBeforeMilis:
                    return "keyframesStartBeforeMilis";
                case configKeys.keyframesStartAfterMilis:
                    return "keyframesStartAfterMilis";
                case configKeys.keyframesEndBeforeMilis:
                    return "keyframesEndBeforeMilis";
                case configKeys.keyframesEndAfterMilis:
                    return "keyframesEndAfterMilis";

                case configKeys.addMiliseconds:
                    return "AddMiliseconds";
                case configKeys.leadIn:
                    return "leadIn";
                case configKeys.leadOut:
                    return "leadOut";
                case configKeys.forcedLeadIn:
                    return "forcedLeadIn";
                case configKeys.addLeadInToYoutube:
                    return "addLeadInToYoutube";
                case configKeys.addLeadOutToYoutube:
                    return "addLeadOutToYoutube";

                default:
                    return "Invalid key";
            }
        }

        public static string AegisubPath
        {
            get
            {
                var returnOBJ = ReadSetting(ToStringKey(configKeys.AegisubPath));
                if (returnOBJ == null)
                    return myDefaultAegisubPath;
                return Convert.ToString(returnOBJ);
            }
            set { SaveAppSetting(configKeys.AegisubPath, value); }
        }

        public static string WorkingFolderPath
        {
            get
            {
                var returnOBJ = ReadSetting(ToStringKey(configKeys.WorkingFolderPath));

                if (returnOBJ == null)
                    return myDefaultWorkingFolderPath;

                if (!(Directory.Exists(Convert.ToString(returnOBJ))))
                    return myDefaultWorkingFolderPath;

                return Convert.ToString(returnOBJ);
            }
            set { SaveAppSetting(configKeys.WorkingFolderPath, value); }
        }

        public static bool CheckActors
        {
            get { return generalBoolGet(configKeys.CheckActors); }
            set { SaveAppSetting(configKeys.CheckActors, Convert.ToString(value)); }
        }

        public static bool PostProcessSubs
        {
            get { return generalBoolGet(configKeys.PostProcessSubs); }
            set { SaveAppSetting(configKeys.PostProcessSubs, Convert.ToString(value)); }
        }

        public static decimal NumberOfPasses
        {
            get { return generalDecimalGet(configKeys.NumberOfPasses); }
            set { SaveAppSetting(configKeys.NumberOfPasses, Convert.ToString(value)); }
        }

        public static decimal singleLineMaxLen
        {
            get { return generalDecimalGet(configKeys.singleLineMaxLen); }
            set { SaveAppSetting(configKeys.singleLineMaxLen, Convert.ToString(value)); }
        }

        public static decimal singleLineThreshold
        {
            get { return generalDecimalGet(configKeys.singleLineThreshold); }
            set { SaveAppSetting(configKeys.singleLineThreshold, Convert.ToString(value)); }
        }
        public static decimal sublineMaxLen
        {
            get { return generalDecimalGet(configKeys.sublineMaxLen); }
            set { SaveAppSetting(configKeys.sublineMaxLen, Convert.ToString(value)); }
        }

        public static decimal breakLinesSimilarityThreshold
        {
            get { return generalDecimalGet(configKeys.breakLinesSimilarityThreshold); }
            set { SaveAppSetting(configKeys.breakLinesSimilarityThreshold, Convert.ToString(value)); }
        }

        public static decimal paragraphSimilarityThreshold
        {
            get { return generalDecimalGet(configKeys.paragraphSimilarityThreshold); }
            set { SaveAppSetting(configKeys.paragraphSimilarityThreshold, Convert.ToString(value)); }
        }

        public static decimal longLastingLinesTreshold
        {
            get { return generalDecimalGet(configKeys.longLastingLinesTreshold); }
            set { SaveAppSetting(configKeys.longLastingLinesTreshold, Convert.ToString(value)); }
        }

        public static bool useSystemSpeechRecognition
        {
            get { return generalBoolGet(configKeys.useSpeechRecognition); }
            set { SaveAppSetting(configKeys.useSpeechRecognition, Convert.ToString(value)); }
        }

        public static bool useSystemSpeechRec
        {
            get { return generalBoolGet(configKeys.useSystemSpeechRec); }
            set { SaveAppSetting(configKeys.useSystemSpeechRec, Convert.ToString(value)); }
        }

        public static string systemSpeechCulture
        {
            get { return generalSpeechCulture(configKeys.systemSpeechCulture); }
            set { SaveAppSetting(configKeys.systemSpeechCulture, value); }
        }

        public static bool useMicrosoftSpeechRec
        {
            get { return generalBoolGet(configKeys.useMicrosoftSpeechRec); }
            set { SaveAppSetting(configKeys.useMicrosoftSpeechRec, Convert.ToString(value)); }
        }

        public static string microsoftSpeechCulture
        {
            get { return generalSpeechCulture(configKeys.microsoftSpeechCulture); }
            set { SaveAppSetting(configKeys.microsoftSpeechCulture, value); }
        }

        public static bool useWavFileForSpeechRec
        {
            get { return generalBoolGet(configKeys.useWavFileForSpeechRec); }
            set { SaveAppSetting(configKeys.useWavFileForSpeechRec, Convert.ToString(value)); }
        }

        public static decimal speechRecCutTimeMilisecsBefore
        {
            get { return generalDecimalGet(configKeys.speechRecCutTimeMilisecsBefore); }
            set { SaveAppSetting(configKeys.speechRecCutTimeMilisecsBefore, Convert.ToString(value)); }
        }

        public static decimal speechRecCutTimeMilisecsAfter
        {
            get { return generalDecimalGet(configKeys.speechRecCutTimeMilisecsAfter); }
            set { SaveAppSetting(configKeys.speechRecCutTimeMilisecsAfter, Convert.ToString(value)); }
        }
        public static bool useKeyFrames
        {
            get { return generalBoolGet(configKeys.useKeyFrames); }
            set { SaveAppSetting(configKeys.useKeyFrames, Convert.ToString(value)); }
        }

        public static double framesPerSecond
        {
            get { return generalDoubleGet(configKeys.framesPerSecond); }
            set { SaveAppSetting(configKeys.framesPerSecond, Convert.ToString(value)); }
        }

        public static decimal keyframesStartBeforeMilis
        {
            get { return generalDecimalGet(configKeys.keyframesStartBeforeMilis); }
            set { SaveAppSetting(configKeys.keyframesStartBeforeMilis, Convert.ToString(value)); }
        }

        public static decimal keyframesStartAfterMilis
        {
            get { return generalDecimalGet(configKeys.keyframesStartAfterMilis); }
            set { SaveAppSetting(configKeys.keyframesStartAfterMilis, Convert.ToString(value)); }
        }
        public static decimal keyframesEndBeforeMilis
        {
            get { return generalDecimalGet(configKeys.keyframesEndBeforeMilis); }
            set { SaveAppSetting(configKeys.keyframesEndBeforeMilis, Convert.ToString(value)); }
        }
        public static decimal keyframesEndAfterMilis
        {
            get { return generalDecimalGet(configKeys.keyframesEndAfterMilis); }
            set { SaveAppSetting(configKeys.keyframesEndAfterMilis, Convert.ToString(value)); }
        }

        public static bool addMiliseconds
        {
            get { return generalBoolGet(configKeys.addMiliseconds); }
            set { SaveAppSetting(configKeys.addMiliseconds, Convert.ToString(value)); }
        }

        public static decimal LeadIn
        {
            get { return generalDecimalGet(configKeys.leadIn); }
            set { SaveAppSetting(configKeys.leadIn, Convert.ToString(value)); }
        }

        public static decimal LeadOut
        {
            get { return generalDecimalGet(configKeys.leadOut); }
            set { SaveAppSetting(configKeys.leadOut, Convert.ToString(value)); }
        }

        public static decimal forcedLeadIn
        {
            get { return generalDecimalGet(configKeys.forcedLeadIn); }
            set { SaveAppSetting(configKeys.forcedLeadIn, Convert.ToString(value)); }
        }

        public static bool addLeadInToYoutube
        {
            get { return generalBoolGet(configKeys.addLeadInToYoutube); }
            set { SaveAppSetting(configKeys.addLeadInToYoutube, Convert.ToString(value)); }
        }

        public static bool addLeadOutToYoutube
        {
            get { return generalBoolGet(configKeys.addLeadOutToYoutube); }
            set { SaveAppSetting(configKeys.addLeadOutToYoutube, Convert.ToString(value)); }
        }

        static bool generalBoolGet(configKeys configKey)
        {
            var returnOBJ = ReadSetting(ToStringKey(configKey));
            if (returnOBJ == null)
                return (bool)generalDefaultFromConfigKey(configKey);
            return Convert.ToBoolean(returnOBJ);
        }

        static decimal generalDecimalGet(configKeys configKey)
        {
            var returnOBJ = ReadSetting(ToStringKey(configKey));
            if (returnOBJ == null)
                return (decimal)generalDefaultFromConfigKey(configKey);
            return Convert.ToDecimal(returnOBJ);
        }

        static double generalDoubleGet(configKeys configKey)
        {
            var returnOBJ = ReadSetting(ToStringKey(configKey));
            if (returnOBJ == null)
                return (double)generalDefaultFromConfigKey(configKey);
            return Convert.ToDouble(returnOBJ);
        }

        static string generalSpeechCulture(configKeys configKey)
        {
            var returnOBJ = ReadSetting(ToStringKey(configKey));

            if (returnOBJ == null)
                return (string)generalDefaultFromConfigKey(configKey);

            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(Convert.ToString(returnOBJ));
                return culture.ToString();
            }
            catch (CultureNotFoundException) { return String.Empty; }
        }

        static object ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                object result = appSettings[key] ?? null;
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error reading app settings", "Error!");
                return "Error reading app settings";
            }
        }

        static void SaveAppSetting(configKeys configKey, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[ToStringKey(configKey)] == null)
                {
                    settings.Add(ToStringKey(configKey), value);
                }
                else
                {
                    settings[ToStringKey(configKey)].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error reading app settings", "Error!");
            }
        }

        static object generalDefaultFromConfigKey(configKeys configKey)
        {
            switch (configKey)
            {
                case configKeys.AegisubPath:
                    return myDefaultAegisubPath;
                case configKeys.WorkingFolderPath:
                    return myDefaultWorkingFolderPath;
                case configKeys.CheckActors:
                    return myDefaultCheckActors;
                case configKeys.PostProcessSubs:
                    return myDefaultPostProcessSubtitles;
                case configKeys.NumberOfPasses:
                    return myDefaultNumberOfPasses;
                case configKeys.singleLineMaxLen:
                    return myDefaultSingleLineMaxLen;
                case configKeys.sublineMaxLen:
                    return myDefaultSublineMaxLen;
                case configKeys.singleLineThreshold:
                    return myDefaultSingleLineThreshold;
                case configKeys.breakLinesSimilarityThreshold:
                    return myDefaultBreakLinesSimilarityThreshold;
                case configKeys.paragraphSimilarityThreshold:
                    return myDefaultParagraphSimilarityThreshold;
                case configKeys.longLastingLinesTreshold:
                    return myDefaultLongLastingLinesTreshold;
                case configKeys.useSpeechRecognition:
                    return myDefaultUseSpeechRecognition;
                case configKeys.useWavFileForSpeechRec:
                    return myDefaultUseWavFileForSpeechRec;
                case configKeys.useSystemSpeechRec:
                    return myDefaultUseSystemSpeech;
                case configKeys.systemSpeechCulture:
                    return myDefaultSystemSpeechCulture;
                case configKeys.useMicrosoftSpeechRec:
                    return myDefaultUseMicrosoftSpeech;
                case configKeys.microsoftSpeechCulture:
                    return myDefaultMicrosoftSPeechCulture;
                case configKeys.speechRecCutTimeMilisecsBefore:
                    return myDefaultspeechRecCutTimeMilisecsBefore;
                case configKeys.speechRecCutTimeMilisecsAfter:
                    return myDefaultspeechRecCutTimeMilisecsAfter;

                case configKeys.useKeyFrames:
                    return myDefaultUseKeyframes;
                case configKeys.framesPerSecond:
                    return myDefaultFramesPerSecond;
                case configKeys.keyframesStartBeforeMilis:
                    return myDefaultKeyframesStartBeforeMilis;
                case configKeys.keyframesStartAfterMilis:
                    return myDefaultKeyframesStartAfterMilis;
                case configKeys.keyframesEndBeforeMilis:
                    return myDefaultKeyframesEndBeforeMilis;
                case configKeys.keyframesEndAfterMilis:
                    return myDefaultKeyframesEndAfterMilis;

                case configKeys.addMiliseconds:
                    return myDefaultAddMiliseconds;
                case configKeys.leadIn:
                    return myDefaultLeadIn;
                case configKeys.leadOut:
                    return myDefaultLeadOut;
                case configKeys.forcedLeadIn:
                    return myDefaultForcedLeadIn;
                case configKeys.addLeadInToYoutube:
                    return myDefaultAddLeadInToYoutube;
                case configKeys.addLeadOutToYoutube:
                    return myDefaultAddLeadOutToYoutube;

                default:
                    return null;
            }
        }

        public static void resetConfigValuesToDefaults()
        {
            AegisubPath = myDefaultAegisubPath;
            WorkingFolderPath = myDefaultWorkingFolderPath;
            CheckActors = myDefaultCheckActors;
            PostProcessSubs = myDefaultPostProcessSubtitles;
            NumberOfPasses = myDefaultNumberOfPasses;
            singleLineMaxLen = myDefaultSingleLineMaxLen;
            singleLineThreshold = myDefaultSingleLineThreshold;
            sublineMaxLen = myDefaultSublineMaxLen;
            breakLinesSimilarityThreshold = myDefaultBreakLinesSimilarityThreshold;
            paragraphSimilarityThreshold = myDefaultParagraphSimilarityThreshold;
            longLastingLinesTreshold = myDefaultLongLastingLinesTreshold;

            useSystemSpeechRecognition = myDefaultUseSpeechRecognition;
            useWavFileForSpeechRec = myDefaultUseWavFileForSpeechRec;
            useSystemSpeechRec = myDefaultUseSystemSpeech;
            systemSpeechCulture = myDefaultSystemSpeechCulture;
            useMicrosoftSpeechRec = myDefaultUseMicrosoftSpeech;
            microsoftSpeechCulture = myDefaultMicrosoftSPeechCulture;
            speechRecCutTimeMilisecsBefore = myDefaultspeechRecCutTimeMilisecsBefore;
            speechRecCutTimeMilisecsAfter = myDefaultspeechRecCutTimeMilisecsAfter;

            useKeyFrames = myDefaultUseKeyframes;
            framesPerSecond = myDefaultFramesPerSecond;
            keyframesStartBeforeMilis = myDefaultKeyframesStartBeforeMilis;
            keyframesStartAfterMilis = myDefaultKeyframesStartAfterMilis;
            keyframesEndBeforeMilis = myDefaultKeyframesEndBeforeMilis;
            keyframesEndAfterMilis = myDefaultKeyframesEndAfterMilis;

            addMiliseconds = myDefaultAddMiliseconds;
            LeadIn = myDefaultLeadIn;
            LeadOut = myDefaultLeadOut;
            forcedLeadIn = myDefaultForcedLeadIn;
            addLeadInToYoutube = myDefaultAddLeadInToYoutube;
            addLeadOutToYoutube = myDefaultAddLeadOutToYoutube;
        }
    }
}
