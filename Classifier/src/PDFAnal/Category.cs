using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LAIR.ResourceAPIs.WordNet;

namespace PDFAnal
{
    public class Category
    {
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

    }
}
