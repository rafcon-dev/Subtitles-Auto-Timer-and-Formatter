using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Subtitle_Synchronizer
{
    public  class transcript
    {
        string myContent;

        public string content
        {
            get { return myContent; }
            set { myContent = value; }
        }

        public void getContent(string s)
        {
            myContent = s;
            cleanWhiteSpace();
        }

        private void cleanWhiteSpace()
        {
            myContent = Regex.Replace(myContent, @"\s{2,}", String.Empty);
        }


    }
}
