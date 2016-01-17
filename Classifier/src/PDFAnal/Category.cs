using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using LAIR.ResourceAPIs.WordNet;

namespace PDFAnal
{
    public class Category
    {
        public static string STRING_REPRESENTATION_FIRST_LINE_KEY = "name";
        public static string STRING_REPRESENTATION_SECOND_LINE_KEY = "wordcount";

        public string Name { get; private set; }
        public int WordCount { get; private set; }
        public Dictionary<SynSet, int> SynSetDictionary { get; private set; }
        public List<KeyValuePair<SynSet, int>> SynSetOrderedList { get; private set; }


        public Category(String name, int wordCount, Dictionary<SynSet, int> synSetDictionary)
        {
            Name = name;
            WordCount = wordCount;
            SynSetDictionary = synSetDictionary;

            SynSetOrderedList = synSetDictionary.ToList();
            SynSetOrderedList.Sort(
                delegate(KeyValuePair<SynSet, int> firstPair, KeyValuePair<SynSet, int> nextPair)
                {
                    return nextPair.Value.CompareTo(firstPair.Value);
                }
            );
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(  STRING_REPRESENTATION_FIRST_LINE_KEY + "=" + Name + Environment.NewLine + 
                                                        STRING_REPRESENTATION_SECOND_LINE_KEY + "=" + WordCount);
	        foreach (var keyValuePair in SynSetOrderedList)
	        {
                builder.Append(Environment.NewLine + keyValuePair.Key.ID + "|" + keyValuePair.Value);
	        }
            return  builder.ToString();
        }

        public static Category FromString(WordNetEngine wordNetEngine, String text)
        {
            string categoryName;
            int wordCount;
            Dictionary<SynSet, int> synSetDictionary;

            //  split into lines
            var lines = Regex.Split(text, "\r\n|\r|\n");
            
            //  first line "name=.."
            var firstLineSplit = lines[0].Split(new char[] {'='}, StringSplitOptions.RemoveEmptyEntries);
            if (firstLineSplit.Length != 2)
            {
                return null;
            }
            if (firstLineSplit[0] != STRING_REPRESENTATION_FIRST_LINE_KEY)
            {
                return null;
            }
            categoryName = firstLineSplit[1];

            //  second line "wordcount=.."
            var secondLineSplit = lines[1].Split(new char[] {'='}, StringSplitOptions.RemoveEmptyEntries);
            if (secondLineSplit.Length != 2)
            {
                return null;
            }
            if (secondLineSplit[0] != STRING_REPRESENTATION_SECOND_LINE_KEY || !Int32.TryParse( secondLineSplit[1], out wordCount))
            {
                return null;
            }
        
            synSetDictionary = new Dictionary<SynSet,int>();

            //  rest of the lines - dictionary
            for(int i = 2 ; i < lines.Length ; ++i)
            {
                var lineSplit = lines[i].Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                if (lineSplit.Length != 2)
                {
                    break;
                }

                //  synset word count
                int synSetWordCount;
                if (!Int32.TryParse(lineSplit[1], out synSetWordCount))
                {
                    return null;
                }

                //  synset
                var synSet = wordNetEngine.GetSynSet(lineSplit[0]);
                if (synSet == null)
                {
                    return null;
                }

                synSetDictionary.Add(synSet, synSetWordCount);
            }

            Utility.Log("loaded " + categoryName + " (" + wordCount + ") with " + synSetDictionary.Count + " synsets");

            return new Category(categoryName, wordCount, synSetDictionary);
        }

    }
}
