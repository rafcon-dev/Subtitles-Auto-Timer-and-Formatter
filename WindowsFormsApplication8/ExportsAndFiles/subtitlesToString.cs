using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Synchronizer
{
    public static class subtitlesToString 
    {


        public static string ToTXT(this string s)
        {
            return s.Replace("\n", Environment.NewLine);
        }

        public static string ToSRT(this string s)
        {
            allUnfixedSubtitles allFixedSubtitles = new allUnfixedSubtitles();
            if (!allFixedSubtitles.getStringSubtitles(s))
                return null;
            return subtitlesToString.theseLinesToSRT(allFixedSubtitles.subtitlesLines)
                .Replace("\n", Environment.NewLine); ;
        }
        
        public static string theseLinesToSRT(List<subLine> slList)
        {
            StringBuilder sBuilder = new StringBuilder();

            foreach (subLine sl in slList)
            {
                sBuilder.Append(thisLineToSRT(sl));
            }
            string result = sBuilder.ToString();

      //      int i = result.Length;

     //       result = result.TrimEnd();

     //       result = result + "\n\n";

            return result;
        }
        public static string thisLineToSRT(subLine sl)
        {
            StringBuilder sBuilder = new StringBuilder();

            sBuilder
                .Append(sl.lineIndex).Append("\n")

                .AppendFormat("{0:00}:{1:00}:{2:00},{3:000}",
                sl.begTime.hour, sl.begTime.minute, sl.begTime.second, sl.begTime.milisecond)

                .Append(" --> ")

                .AppendFormat("{0:00}:{1:00}:{2:00},{3:000}\n",
                sl.endTime.hour, sl.endTime.minute, sl.endTime.second, sl.endTime.milisecond)

                .Append(sl.lineContent).
                Append("\n\n");

            return sBuilder.ToString();
        }
    }
}
