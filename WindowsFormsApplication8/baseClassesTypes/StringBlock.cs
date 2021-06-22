using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Subtitle_Synchronizer
{

    public class listOfBlockStrings
    {
        List<stringInBlockForm> myAllBlockStrings;
        string myOriginalLine;
        string myRegexExpression;
        allBuildingStringBlocks myTheBlocksForAllLines;

        /// <summary>
        /// divides string in the middle
        /// </summary>
        /// <param name="originalLine"></param>
        public listOfBlockStrings(string originalLine)
        {
            myAllBlockStrings = new List<stringInBlockForm>();
            theBlocksForAllLines = new allBuildingStringBlocks(originalLine);
        }
        public listOfBlockStrings(string originalLine, string regexExpression)
        {
            myAllBlockStrings = new List<stringInBlockForm>();
            theBlocksForAllLines = new allBuildingStringBlocks(originalLine, regexExpression);
        }
        List<stringInBlockForm> allBlockStrings
        {
            get { return myAllBlockStrings; }
            set { myAllBlockStrings = value; }
        }

        public bool onlyOneBlockFound
        {
            get { return (theBlocksForAllLines.numberOfBlocks < 2); }
        }

        public int numberOfLines
        {
            get { return allBlockStrings.Count; }
        }
        string originalLine
        {
            get { return myOriginalLine; }
            set { originalLine = value; }
        }

        string regexExpression
        {
            get { return myRegexExpression; }
            set { regexExpression = value; }
        }

        allBuildingStringBlocks theBlocksForAllLines
        {
            get { return myTheBlocksForAllLines; }
            set { myTheBlocksForAllLines = value; }
        }

        public bool thereIsABlockThatIsLongerThan( int maxBlockLeght, int stringIndex)
        {
            if (allBlockStrings.Count <= 0 || stringIndex > allBlockStrings.Count)
                return false;

            return myAllBlockStrings[stringIndex].thereIsABlockLongerThan(maxBlockLeght);

        }

        public int numberOfBlocksInLineIndex(int index)
        {
            if (allBlockStrings.Count <= 0 || index > allBlockStrings.Count)
                return 0;

            return allBlockStrings[index].numberOfBlockInString;
        }

        public string stringAtIndex(int index)
        {
            return allBlockStrings[index].contentInStringForm;
        }

        public int lenghtOfStringAtIndex(int index) //faster than accessing stringAtIndex.Lenght
        {
            return allBlockStrings[index].lenghtOfTheString;
        }

        public int lastLineLenght
        {
            get
            {
                if (numberOfLines > 0)
                    return allBlockStrings[numberOfLines - 1].lenghtOfTheString;
                else return -1;
            }
        }

        public int firstLineLenght
        {
            get
            {
                if (numberOfLines > 0)
                    return allBlockStrings[0].lenghtOfTheString;
                else
                    return -1;
            }
        }

        public void AddOriginalStringInBlockForm()
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < theBlocksForAllLines.allBlocks.Count; i++)
            {
                indexes.Add(i);
            }
            allBlockStrings.Add(new stringInBlockForm(theBlocksForAllLines, indexes));
        }
        public void AddString(List<int> blockIndexes)
        {
            allBlockStrings.Add(new stringInBlockForm(theBlocksForAllLines, blockIndexes));
        }

        void AddNewEmtpyString()
        {
            List<int> indexes = new List<int>();
            allBlockStrings.Add(new stringInBlockForm(theBlocksForAllLines, indexes));
        }

        public void removeBlockStringAtIndex(int index)
        {
            allBlockStrings.RemoveAt(index);
        }
        public void permutateLastBlockFromLineIndex1ToBeginningOfTheNextLine
            (int index1)
        {
            if (allBlockStrings.Count <= index1 + 1)
                AddNewEmtpyString();

            allBlockStrings[index1 + 1].insertBlockAt
                (0, allBlockStrings[index1].indexAt(allBlockStrings[index1].numberOfBlockInString - 1));
            allBlockStrings[index1].removeLastBlock();
        }

        public void permutateFirstBlockFromLineIndex1ToEndOfPreviousLine
            (int index1)
        {
            if (index1 <= 0)
                return;
            if (numberOfBlocksInLineIndex(index1) == 0)
                return;

            allBlockStrings[index1 - 1].insertBlockAt
                (allBlockStrings[index1 - 1].numberOfBlockInString, allBlockStrings[index1].firstBlockIndex);

            allBlockStrings[index1].removeBlockAt(0);
        }
    }
    class stringInBlockForm
    {
        allBuildingStringBlocks myBuildingBlocks = new allBuildingStringBlocks();
        List<int> myAllBlockIndexes = new List<int>();

        public string contentInStringForm
        {
            get { return blocksToString(); }
        }

        public int lenghtOfTheString
        {
            get
            {
                int lenght = 0;
                foreach (int index in allBlockIndexes)
                {
                    lenght += buildingBlocks.allBlocks[index].content.Length;
                }
                return lenght;
            }
        }
        public int numberOfBlockInString
        {
            get { return allBlockIndexes.Count; }
        }
        allBuildingStringBlocks buildingBlocks
        {
            get { return myBuildingBlocks; }
            set { myBuildingBlocks = value; }
        }

        List<int> allBlockIndexes
        {
            get { return myAllBlockIndexes; }
            set { myAllBlockIndexes = value; }
        }

        public stringInBlockForm(string originalLine, bool areWeCuttingByRegex, string regexExpression)
        {
            buildingBlocks = new allBuildingStringBlocks(originalLine, regexExpression);
        }

        public stringInBlockForm(allBuildingStringBlocks allTheBlocks, List<int> allTheIndexes)
        {
            buildingBlocks = allTheBlocks;
            myAllBlockIndexes = new List<int>();
            foreach (int index in allTheIndexes)
                myAllBlockIndexes.Add(index);
        }

        public void removeLastBlock()
        {
            removeBlockAt(allBlockIndexes.Count - 1);
        }
        public void removeBlockAt(int index)
        {
            if (allBlockIndexes.Count >= 1)
            {
                allBlockIndexes.RemoveAt(index);
            }
        }

        public void insertBlockAt(int indexToInsert, int blockIndexInBuildingBlocks)
        {
            allBlockIndexes.Insert(indexToInsert, blockIndexInBuildingBlocks);
        }

        public int indexAt(int index)
        {
            return allBlockIndexes[index];
        }

        public int lastBlockIndex
        {
            get { return allBlockIndexes[numberOfBlockInString - 1]; }
        }

        public int firstBlockIndex
        {
            get { return allBlockIndexes[0]; }
        }

        //  allStringBlocks
        string blocksToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (int index in allBlockIndexes)
            {
                sb.Append(buildingBlocks.allBlocks[index].content);
            }
            return sb.ToString();
        }

        public bool thereIsABlockLongerThan(int maxBlockLenght)
        {
            foreach (int index in allBlockIndexes)
            {
                if (buildingBlocks.allBlocks[index].content.Length > maxBlockLenght)
                    return true;
            }
            return false;
        }
    }


    public class allBuildingStringBlocks
    {
        string myOriginalString;
        List<stringBlock> myAllBlocks;

        public allBuildingStringBlocks()
        {
            myAllBlocks = new List<stringBlock>();
            originalString = string.Empty;
        }

        /// <summary>
        /// Divides String In the middle
        /// </summary>
        /// <param name="originalS"></param>
        public allBuildingStringBlocks(string originalS)
        {
            originalString = originalS;
            allBlocks = createBlockListByDividingStringInTheMiddle();
        }
        public allBuildingStringBlocks(string originalS, string regexExpression)
        {
            originalString = originalS;
            allBlocks = createBlockListFromStringAndRegex(regexExpression);
        }

        public string originalString
        {
            get { return myOriginalString; }
            set { myOriginalString = value; }
        }
        public List<stringBlock> allBlocks
        {
            get { return myAllBlocks; }
            set { myAllBlocks = value; }
        }

        public int numberOfBlocks
        {
            get { return allBlocks.Count; }
        }

        List<stringBlock> createBlockListByDividingStringInTheMiddle()
        {
            List<stringBlock> blockList = new List<stringBlock>();

            if (originalString.Length <= 1)
                blockList.Add(new stringBlock(originalString, 0));
            else
            {
                int eachPartLenght = originalString.Length / 2;

                int previousPartLenght = eachPartLenght;

                string first = originalString.cutStringAtWord(eachPartLenght, 0, 0, true);
                string second = originalString.Substring(first.Length);

                blockList.Add(new stringBlock(first, 0));
                blockList.Add(new stringBlock(second, first.Length));
            }

            return blockList;
        }
        List<stringBlock> createBlockListFromStringAndRegex(string regexExpression)
        {
            var regexDivisions = Regex.Matches(originalString, regexExpression);
            List<stringBlock> blockList = new List<stringBlock>();

            foreach (Match mc in regexDivisions)
            {
                blockList.Add(new stringBlock(mc.Value, mc.Index));
            }
            return blockList;
        }

        public string buildStringThatIncludeStringIndexes(int startStringIndex, int endStringIndex)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < allBlocks.Count; i++)
            {
                if (allBlocks[i].index > endStringIndex)
                    break;

                if (allBlocks[i].index >= startStringIndex)
                    sb.Append(allBlocks[i].content);
            }
            return sb.ToString();
        }

        public string buildStringBetweenBlockIndexes(int startBlockIndex, int numberOfBlocksToUse)
        {

            if (startBlockIndex >= allBlocks.Count)
                return string.Empty;

            if (numberOfBlocksToUse < 0)
                numberOfBlocksToUse = 0;

            if (startBlockIndex < 0)
                startBlockIndex = 0;

            if (startBlockIndex + numberOfBlocksToUse > allBlocks.Count)
                numberOfBlocksToUse = allBlocks.Count - startBlockIndex;


            StringBuilder sb = new StringBuilder();

            for (int i = startBlockIndex; i < startBlockIndex + numberOfBlocksToUse; i++)
            {
                sb.Append(allBlocks[i].content);
            }
            return sb.ToString();
        }
    }
    public class stringBlock
    {
        string myContent;
        int myIndex;

        public stringBlock(string blockContent, int blockIndexInString)
        {
            content = blockContent;
            index = blockIndexInString;
        }

        public string content
        {
            get { return myContent; }
            set { myContent = value; }
        }

        public int index
        {
            get { return myIndex; }
            set { myIndex = value; }
        }

    }
}
