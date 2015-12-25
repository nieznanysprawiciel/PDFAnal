using System;
using System.Collections.Generic;
using System.Text;
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
        private List<string> pageList;

        private Set<SynSet> categories;

        public MainWindow()
        {
            InitializeComponent();

            wordNetEngine = new WordNetEngine(@"..\..\resources", false);
            semanticSimilarityModel = new WordNetSimilarityModel(wordNetEngine);

            pageList = new List<string>();
            categories = new Set<SynSet>();
            SynSet categoryTelecommuncationSynset = wordNetEngine.GetSynSet("Noun:6282431");    //  {telecommuncation, telecom}
            SynSet categoryMathSynset = wordNetEngine.GetSynSet("Noun:6009822");    //  mathematics, math, maths
            categories.Add(categoryTelecommuncationSynset);
            categories.Add(categoryMathSynset);
        }

        private void ProcessPDFBButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, List<string>> stemmingDict = new Dictionary<string, List<string>>();    //  <stemmedWord, list<word>>
            //Dictionary<string, int> stemmedWordsOccuranceCountDict = new Dictionary<string, int>();    //  <stemmedWord, occuranceCount>
            int allWordsCount = 0;
            int nonStopWordsCount = 0;
            int stopWordCount = 0;
            int emptyStemsCount = 0;

            //Dictionary<SynSet, Set<string>> resultingSynSetsDict = new Dictionary<SynSet, Set<string>>();   //  <SynSet, Set<stemmedWord>>
            //int stemmedWordsThatHasSynSetsCount = 0;

            foreach (var pageContent in pageList)   // stem all words
            {
                string[] words = pageContent.Split(new char[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
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
                    
                    //  check if this stem has already happened before and count it
                    List<string> wordList;
                    bool stemmedWordAlreadyProcessed = stemmingDict.TryGetValue(stemmedWord, out wordList); //stemmedWordsOccuranceCountDict.TryGetValue(stemmedWord, out occCount);
                    if ( stemmedWordAlreadyProcessed )
                    {
                        //  register this word in existing stemming dictionary entry
                        wordList.Add(wordLower);
                    }
                    else
                    {
                        //  create new entry in stemming dictionary 
                        List<string> newWordList = new List<string>();
                        newWordList.Add(wordLower);
                        stemmingDict[stemmedWord] = newWordList;
                    }
                    
                    // find synset if new stemmed word
                    /*
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
                    */
                }
            }

            /*
            foreach (var z in stemmingDict)
            {
                Utility.Log("\t" + z.Key);
                foreach (var nonStemmedWord in z.Value)
                {
                    Utility.Log("\t\t" + nonStemmedWord);
                }
            }
            */

            Dictionary<SynSet, int> resultingSynSetsDict = new Dictionary<SynSet, int>();   //  <SynSet, wordsCount>
            int stemmedWordsThatHasSynSetsCount = 0;
            try {
                Set<SynSet> synSetSet;
                foreach (var stemmingDictionaryEntry in stemmingDict )
                {
                    Debug.Assert(stemmingDictionaryEntry.Value.Count > 0);
                    var stemmedWord = stemmingDictionaryEntry.Key;
                    synSetSet = wordNetEngine.GetSynSets(stemmedWord, WordNetEngine.POS.Noun);

                    if (!(synSetSet.Count == 0))
                    {
                        stemmedWordsThatHasSynSetsCount++;

                        //  relate synSet with this stemmed word ( add / modify entry in Dictionary<SynSet, Set<string>> resultingSynSetsDict )
                        foreach (SynSet synSet in synSetSet)
                        {
                            int wordCount;
                            if (resultingSynSetsDict.TryGetValue(synSet, out wordCount))
                            {
                                wordCount += stemmingDictionaryEntry.Value.Count;
                                resultingSynSetsDict[synSet] = wordCount;
                            }
                            else
                            {
                                resultingSynSetsDict[synSet] = stemmingDictionaryEntry.Value.Count;
                            }
                        }
                    }
                    else //  no synsets found for this stemmed word
                    {
                        //lets try with not-stemmed word
                        List<string> nonStemmedWordsSet = stemmingDict[stemmedWord];
                        //group by and count
                        var q = from x in nonStemmedWordsSet
                                group x by x into g
                                let count = g.Count()
                                select new { Value = g.Key, Count = count };
                        foreach (var nonStemmedWordCountKeyValue in q)
                        {
                            var nonStemmedWord = nonStemmedWordCountKeyValue.Value;
                            synSetSet = wordNetEngine.GetSynSets(nonStemmedWord, WordNetEngine.POS.Noun);

                            //XXX
                            //  relate synSet with this stemmed word ( add / modify entry in Dictionary<SynSet, Set<string>> resultingSynSetsDict )
                            foreach (SynSet synSet in synSetSet)
                            {
                                int pair;
                                if (resultingSynSetsDict.TryGetValue(synSet, out pair))
                                {
                                    pair += nonStemmedWordCountKeyValue.Count;
                                    resultingSynSetsDict[synSet] = pair;
                                }
                                else
                                {
                                    resultingSynSetsDict[synSet] = nonStemmedWordCountKeyValue.Count;
                                }
                            }
                            //~~XXX
                        }                  
                    }
                }
            } catch (Exception ex) { Debug.Assert(false, ex.ToString()); }
            

            Utility.Log( "all words count: " + allWordsCount );
            Utility.Log( "non stopwords count: " + nonStopWordsCount );
            Utility.Log( "stopwords count: " + stopWordCount );
            Utility.Log( "empty stems count: " + emptyStemsCount);
            Utility.Log( "stemmed words that has SynSets count: " + stemmedWordsThatHasSynSetsCount );
            Utility.Log( "synsets count: " + resultingSynSetsDict.Count );
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
            int k = 30;
            Utility.Log("top " + k + ": ");
            var resultingSynSetsList = resultingSynSetsDict.ToList();
            resultingSynSetsList.Sort(
                delegate(KeyValuePair<SynSet, int> firstPair, KeyValuePair<SynSet, int> nextPair)
                {
                    return nextPair.Value.CompareTo(firstPair.Value);
                }
            );
            for (int i = 0; i < Math.Min(k, resultingSynSetsList.Count); ++i)
            {
                var keyValuePair = resultingSynSetsList[i];
                Utility.Log("----->");
                Utility.Log("\t" + keyValuePair.Value + " words" );
                Utility.Log("\tsynset: ");
                foreach ( var word in keyValuePair.Key.Words )
                {
                    Utility.Log("\t\t" + word);
                }
            }

            //  lets try to classify the document
            //Dictionary<string, int> tempStemmedWordsOccuranceCountDict = new Dictionary<string, int>( stemmedWordsOccuranceCountDict );    //  <stemmedWord, occuranceCount>
            ////Dictionary<SynSet, Set<string>> tempResultingSynSetsDict = new Dictionary<SynSet, Set<string>>( resultingSynSetsDict );   //  <SynSet, Set<stemmedWord>>
            List<WordNetEngine.SynSetRelation> synSetRelations = new List<WordNetEngine.SynSetRelation>();
            synSetRelations.Add( WordNetEngine.SynSetRelation.Hypernym );
            Dictionary<SynSet, double> classificationResultDict = new Dictionary<SynSet, double>(); //  <categorySynSet, mysimiliartyBetwCatSynSetAndDoc>
            foreach ( var category in categories )
            {
                string categoryText = Utility.Words(category);
                double mySimilarity = 0.0d;
                for ( int i = 0 ; i < Math.Min( k, resultingSynSetsList.Count ) ; ++i )
                {
                    SynSet synSetFromDocument = resultingSynSetsList[i].Key;
                    // get the LCS along the similarity relations
                    SynSet lcs = category.GetClosestMutuallyReachableSynset(synSetFromDocument, synSetRelations);
                    if ( ! (lcs == null) )
                    {
                        // get depth of synsets
                        int lcsDepth = lcs.GetDepth(synSetRelations) + 1;
                        int categoryDepth = category.GetShortestPathTo(lcs, synSetRelations).Count - 1 + lcsDepth;
                        int synSetFromDocumentDepth = synSetFromDocument.GetShortestPathTo(lcs, synSetRelations).Count - 1 + lcsDepth;

                        //  count words from this documentsynset
                        int wordCount = resultingSynSetsDict[synSetFromDocument];

                        // get similarity
                        double synSetsSimilarity = 2 * lcsDepth / (double)(categoryDepth + synSetFromDocumentDepth);
                        mySimilarity += synSetsSimilarity * wordCount;

                        //  log
                        Utility.Log(categoryText + "->" + Utility.Words(synSetFromDocument));
                        Utility.Log("\tlcs[" + lcsDepth + "]:" + Utility.Words(lcs));
                        Utility.Log("\t\t synSets similarity:" + synSetsSimilarity);
                        Utility.Log("\tcategory[" + categoryDepth + "] docSynSet[" + synSetFromDocumentDepth + "]");
                    }




                    /*
                    SynSet synSetFromDocument = resultingSynSetsList[i].Key;
                    var shortestPath = category.GetShortestPathTo(synSetFromDocument, synsetRelations);
                    if ( shortestPath == null )
                    {
                        continue;
                    } 
                    int pathLength = shortestPath.Count;
                    Debug.Assert( pathLength != 0 );  //  TODO check this one, it assumes that path from node A to node A is List[] { node A }
                    int wordCount = 0;
                    foreach ( string word in tempResultingSynSetsDict[synSetFromDocument] )
                    {
                        int stemmedWordCount;
                        if ( tempStemmedWordsOccuranceCountDict.TryGetValue( word, out stemmedWordCount ) )
                        {
                            wordCount += stemmedWordCount;
                            tempStemmedWordsOccuranceCountDict.Remove( word );
                        }
                    }
                    mySimilarity += ( double )wordCount / pathLength;
                    */
                }
                classificationResultDict.Add( category, mySimilarity );
            }
            Utility.Log( "results:" );
            foreach ( var classificationResult in classificationResultDict )
            {
                SynSet category = classificationResult.Key;
                Utility.Log( "\t" + Utility.Words(category) + ":" + classificationResult.Value );
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

            Utility.Log(automationWord + " synsets:" + automationSynSetSet.Count);
            Utility.Log(mechanizationWord + " synsets:" + mechanizationSynSetSet.Count);
            Utility.Log("sets union: " + synSetsUnionSet.Count);
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

            Utility.Log( "done" );
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
            Utility.Log(word + " -> " + stemmedWord);
            return stemmedWord;
        }

        public class Pair<T1, T2>
        {
            public T1 First { get; set; }
            public T2 Second { get; set; }
        }

    }
}
