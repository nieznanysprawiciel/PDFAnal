using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using Microsoft.Win32;
using org.pdfclown.files;
using org.pdfclown.documents;
using org.pdfclown.tools;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.objects;
using System.Drawing;
using System.Diagnostics;

using LAIR.ResourceAPIs.WordNet;
using LAIR.Collections.Generic;

namespace PDFAnal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WordNetEngine wordNetEngine;
        private WordNetSimilarityModel semanticSimilarityModel;

        private Document document;
        private List<string> pageList = new List<string>();
        

        public MainWindow()
        {
            InitializeComponent();

            wordNetEngine = new WordNetEngine(@"..\..\resources", false);
            semanticSimilarityModel = new WordNetSimilarityModel(wordNetEngine);
        }

        private void ProcessPDFBButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> stemmingDict = new Dictionary<string, string>();    //  <word,stemmedWord>
            Dictionary<string, int> stemmedWordsOccuranceCountDict = new Dictionary<string, int>();    //  <stemmedWord, occuranceCount>
            Dictionary<SynSet, Set<string>> resultingSynSetsDict = new Dictionary<SynSet, Set<string>>();   //  <SynSet, Set<stemmedWord>>
            int allWordsCount = 0;
            int nonStopWordsCount = 0;
            int stopWordCount = 0;
            int stemmedWordsThatHasSynSetsCount = 0;
            int emptyStemsCount = 0;

            foreach (var pageContent in pageList)   // stem all words
            {
                string[] words = pageContent.Split(new char[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);   //  splits by whitespace
                allWordsCount += words.Length;
                foreach (string word in words)
                {
                    //  lowercase the word - necessery for a stopword check and stemmer
                    string wordLower = word.ToLower();
                    //  check if word is a stopword
                    if(StopWord.IsStopWord(wordLower)) {
                        stopWordCount++;
                        continue;
                    }
                    nonStopWordsCount++;
                    
                    //  stem word
                    string stemmedWord = Stem(wordLower);
                    if ( stemmedWord.Length == 0 || stemmedWord.Equals(" "))
                    {
                        emptyStemsCount++;
                        continue;
                    }
                    //  register this particular stemming
                    stemmingDict[wordLower] = stemmedWord;
                    //  check if this stem has already happened before and count it
                    int occCount;
                    bool stemmedWordAlreadyProcessed = stemmedWordsOccuranceCountDict.TryGetValue(stemmedWord, out occCount);
                    if ( stemmedWordAlreadyProcessed )
                    {
                        stemmedWordsOccuranceCountDict[stemmedWord] = occCount + 1;
                    }
                    else
                    {
                        stemmedWordsOccuranceCountDict[stemmedWord] = 1;
                    }

                    // find synset if new stemmed word
                    Set<SynSet> synSetSet = new Set<SynSet>();
                    if (!stemmedWordAlreadyProcessed)
                    {
                        try { synSetSet = wordNetEngine.GetSynSets(stemmedWord, WordNetEngine.POS.Noun); }
                        catch (Exception ex) { Debug.Assert(false, ex.ToString());  }

                        if ( synSetSet.Count == 0)  //  no synsets found for this stemmed word
                        {
                            //lets try with not-stemmed word
                            try { synSetSet = wordNetEngine.GetSynSets(wordLower, WordNetEngine.POS.Noun); }
                            catch (Exception ex) { Debug.Assert(false, ex.ToString()); }
                        }

                        if ( ! ( synSetSet.Count == 0 ) )
                        {
                            stemmedWordsThatHasSynSetsCount++;
                        }

                        //  relate synSet with this stemmed word ( add / modify entry in Dictionary<SynSet, Set<string>> resultingSynSetsDict )
                        foreach (SynSet synSet in synSetSet)
                        {
                            Set<string> pair;
                            if (resultingSynSetsDict.TryGetValue(synSet, out pair))
                            {
                                pair.Add(stemmedWord);
                                resultingSynSetsDict[synSet] = pair;
                            }
                            else
                            {
                                pair = new Set<string>();
                                pair.Add(stemmedWord);
                                resultingSynSetsDict[synSet] = pair;
                            }
                        }
                    }
                }
            }


            //  compute synsets' usage - no of words related to each synset
            Dictionary<SynSet, Tuple<int, int>> resultingSynSetsWordsCountDict = new Dictionary<SynSet, Tuple<int, int>>(); //  <synset, Tuple<wordsCount, uniqueStemsCount>>
            foreach ( var synsetData in resultingSynSetsDict )    //  for each found synset
            {
                int wordsCount = 0;
                foreach ( var stemmedWord in synsetData.Value )   //  for each stemmed word related to this synset
                {
                    wordsCount += stemmedWordsOccuranceCountDict[stemmedWord];
                }
                Debug.Assert( wordsCount >= synsetData.Value.Count );
                /*bool assert = false;
                if ( wordsCount < synsetData.Value.Count )
                {
                    assert = true;
                }*/
                resultingSynSetsWordsCountDict[synsetData.Key] = new Tuple<int, int>(wordsCount, synsetData.Value.Count);
            }


            Utility.log( "all words count: " + allWordsCount );
            Utility.log( "non stopwords count: " + nonStopWordsCount );
            Utility.log( "stopwords count: " + stopWordCount );
            Utility.log( "empty stems count: " + emptyStemsCount);
            Utility.log( "stemmed words that has SynSets count: " + stemmedWordsThatHasSynSetsCount );
            Utility.log( "synsets count: " + resultingSynSetsDict.Count );
            /*Set<string> bestPair = null;
            SynSet bestSynSet = null;
            foreach ( var keyValuePair in resultingSynSetsDict ) {

                var pair = keyValuePair.Value;
                if ( bestPair == null )
                {
                    bestPair = pair;
                    bestSynSet = keyValuePair.Key;
                }
                else
                {
                    if (pair.Count > bestPair.Count)
                    {
                        bestPair = pair;
                        bestSynSet = keyValuePair.Key;
                    }
                }
            }
            //Utility.log( "best synset was chosen to be assigned for " + bestPair.Second + " words and " + bestPair.First.Count + " stemmed words" );
            Utility.log( "best synset: " );
            foreach (var word in bestSynSet.Words)
            {
                Utility.log("\t" + word);
            }*/

            //  TOP k
            int k = 20;
            Utility.log("top " + k + ": ");
            var resultingSynSetsList = resultingSynSetsWordsCountDict.ToList();
            resultingSynSetsList.Sort(
                delegate(KeyValuePair<SynSet, Tuple<int, int>> firstPair, KeyValuePair<SynSet, Tuple<int, int>> nextPair)
                {
                    return nextPair.Value.Item1.CompareTo(firstPair.Value.Item1);
                }
            );
            for (int i = 0; i < Math.Min(k, resultingSynSetsList.Count); ++i)
            {
                var keyValuePair = resultingSynSetsList[i];
                Utility.log("----->");
                Utility.log("\t" + keyValuePair.Value.Item1 + " words and " + keyValuePair.Value.Item2 + " unique stemmed words related to this synset");
                Utility.log("\tsynset: ");
                foreach ( var word in keyValuePair.Key.Words )
                {
                    Utility.log("\t\t" + word);
                }
            }

        }

        private void LoadPDFButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".pdf";
            fileDialog.Filter = "PDF documents (.pdf)|*.pdf";
			fileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

			Nullable<bool> result = fileDialog.ShowDialog();
            if( result == true )
            {
                string fileName = fileDialog.FileName;
                File file = new File( fileName );
                document = file.Document;

                GetInterestingThings( document );
            }
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            string automationWord = "automation";
            string mechanizationWord = "mechanization";
            Set<SynSet> automationSynSetSet = wordNetEngine.GetSynSets(automationWord, null);
            Set<SynSet> mechanizationSynSetSet = wordNetEngine.GetSynSets(mechanizationWord, null);
            Set<SynSet> synSetsUnionSet = new Set<SynSet>();
            synSetsUnionSet.ThrowExceptionOnDuplicateAdd = false;
            synSetsUnionSet.AddRange(automationSynSetSet);
            synSetsUnionSet.AddRange(mechanizationSynSetSet);

            Utility.log(automationWord + " synsets:" + automationSynSetSet.Count);
            Utility.log(mechanizationWord + " synsets:" + mechanizationSynSetSet.Count);
            Utility.log("sets union: " + synSetsUnionSet.Count);
        }

        private void GetInterestingThings( Document document )
        {
			TextExtractor textExtractor = new TextExtractor();
			ContentTextBox.Text = "";

			foreach ( var page in document.Pages )
            {
				var textStrings = textExtractor.Extract( page );
				string pageContent = TextExtractor.ToString( textStrings );
                //string[] ssize = content.Split(null);   //  splits by whitespace
                pageList.Add(pageContent);
				ContentTextBox.Text += pageContent + "\n\n";
            }

            Utility.log( "done" );
        }

/*        //  straszne dziadostwo jezeli chodzi o parsowanie
        //extract(new ContentScanner(page));
        private void extract(ContentScanner level)
        {
            if(level == null)
            return;

            while(level.MoveNext())
            {
              ContentObject content = level.Current;
              if( content is ShowText )
              {
                var font = level.State.Font;
                // Extract the current text chunk, decoding it!
                Utility.log(font.Decode(((ShowText)content).Text));
              }
              else if( content is Text || content is ContainerObject )
              {
                // Scan the inner level!
                extract(level.ChildLevel);
              }
            }
        }
 */

        private string Stem( string word )
        {
            char[] w = new char[501];
            Stemmer s = new Stemmer();
            char[] wordCharsArray = (word + " ").ToCharArray();
            for (int i = 0; i < wordCharsArray.Length; ++i)
            {
                char ch = wordCharsArray[i];
                if (Char.IsLetter((char)ch))
                {
                    int j = 0;
                    while (true)
                    {
                        ch = Char.ToLower((char)ch);
                        w[j] = (char)ch;
                        if (j < 500)
                            j++;
                        ++i;
                        if (i == wordCharsArray.Length)
                        {
                            break;
                        }
                        ch = wordCharsArray[i];
                        if (!Char.IsLetter((char)ch))
                        {
                            // to test add(char ch) 
                            for (int c = 0; c < j; c++)
                                s.add(w[c]);
                            // or, to test add(char[] w, int j) 
                            // s.add(w, j); 
                            s.stem();

                            String u;

                            // and now, to test toString() :
                            u = s.ToString();

                            // to test getResultBuffer(), getResultLength() : 
                            // u = new String(s.getResultBuffer(), 0, s.getResultLength()); 

                            Console.Write(u);
                            break;
                        }
                    }
                }
            }   //  for
            string stemmedWord = s.ToString();
            Utility.log(word + " -> " + stemmedWord);
            return stemmedWord;
        }

        public class Pair<T1, T2>
        {
            public T1 First { get; set; }
            public T2 Second { get; set; }
        }

    }
}
