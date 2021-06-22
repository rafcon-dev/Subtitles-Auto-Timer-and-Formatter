using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Synchronizer
{
    public  class wordAnchor
    {
        int mySubIndex;
        int myTransIndex;
        string myContent;
        int mySubIndexInIndividualLine;
        public wordAnchor()
        {
            mySubIndex = 0;
            myTransIndex = 0;
            myContent = "";
            mySubIndexInIndividualLine = 0;
            //metadata
            lineLength = 0;
            lineIndex = 0;
        }

        public string content
        {
            get { return myContent; }
            set { myContent = value; }
        }

        public int SubIndex
        {
            get { return mySubIndex; }
            set { mySubIndex = value; }
        }

        public int TransIndex
        {
            get { return myTransIndex; }
            set { myTransIndex = value; }
        }

        public int SubIndexInIndividualLine
        {
            get { return mySubIndexInIndividualLine; }
            set { mySubIndexInIndividualLine = value; }
        }
        //metadata
        private int myLineIndex;
        private int myLineLength;
        //the index of the start of the correspondent line in 
        private int myLineStartIndexInAllLines;
        public int lineIndex
        {
            get { return myLineIndex; }
            set { myLineIndex = value; }
        }
        public int lineLength
        {
            get { return myLineLength; }
            set { myLineLength = value; }
        }

    }
}
