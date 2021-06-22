using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Synchronizer
{
    class subsToTranscriptFactory
    {
        allUnfixedSubtitles subsObj = new allUnfixedSubtitles();

        string _transcript;

        public string transcript
        { get { return _transcript; } }

        public subsToTranscriptFactory( string subtitlesAsString)
        {
            subsObj.getStringSubtitles(subtitlesAsString);
        }

        public void convertToTranscript()
        {
            StringBuilder sb = new StringBuilder();

            foreach(subLine sl in subsObj.subtitlesLines)
            {
                string stringToAdd = sl.lineContent;
                stringToAdd = stringToAdd.Replace(Environment.NewLine, "\n");
                stringToAdd.TrimEnd('\n');
                sb.Append(stringToAdd);
            }

            sb.Replace('\n', ' ');
            sb.Replace("  ", " ");
            _transcript = sb.ToString();
        }
    }
}
