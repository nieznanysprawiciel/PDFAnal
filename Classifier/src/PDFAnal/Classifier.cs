using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using org.pdfclown.files;
using org.pdfclown.documents;
using org.pdfclown.tools;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.objects;

using LAIR.ResourceAPIs.WordNet;
using LAIR.Collections.Generic;

namespace PDFAnal
{
    class Classifier
    {
        private WordNetEngine wordNetEngine;
        private WordNetSimilarityModel semanticSimilarityModel;

        public Set<SynSet> Categories { get; private set; }

        public Classifier()
        {
            wordNetEngine = new WordNetEngine(@"..\..\resources", false);
            semanticSimilarityModel = new WordNetSimilarityModel(wordNetEngine);

            Categories = new Set<SynSet>();
            SynSet categoryTelecommuncationSynset = wordNetEngine.GetSynSet("Noun:6282431");    //  {telecommuncation, telecom}
            SynSet categoryMathSynset = wordNetEngine.GetSynSet("Noun:6009822");    //  mathematics, math, maths
            /*
             data_mining Noun:13476407
            algorithm Noun:5855965
            electronics Noun:6108876
            energy Noun:11472496
            multimedia Noun:6272397	
            security.security measures Noun:824977
             */
            SynSet dataMiningSynSet = wordNetEngine.GetSynSet("Noun:13476407");
            SynSet algorithmSynSet = wordNetEngine.GetSynSet("Noun:5855965");
            SynSet electronicsSynSet = wordNetEngine.GetSynSet("Noun:6108876");
            SynSet energySynSet = wordNetEngine.GetSynSet("Noun:11472496");
            SynSet multimediaSynSet = wordNetEngine.GetSynSet("Noun:6272397");
            SynSet securitySynSet = wordNetEngine.GetSynSet("Noun:824977");
            SynSet sculptureSynSet = wordNetEngine.GetSynSet("Noun:939472");
            SynSet literatureSynSet = wordNetEngine.GetSynSet("Noun:6179204");

            Categories.Add(categoryTelecommuncationSynset);
            Categories.Add(categoryMathSynset);

            Categories.Add(dataMiningSynSet);
            Categories.Add(algorithmSynSet);
            Categories.Add(electronicsSynSet);
            Categories.Add(energySynSet);
            Categories.Add(multimediaSynSet);
            Categories.Add(securitySynSet);
            Categories.Add(sculptureSynSet);
            Categories.Add(literatureSynSet);
        }

        public SynSet Classify(Document document)
        {
            //  create stemming dictionary
            Dictionary<string, List<string>> stemmingDict = CreateStemmingDictionary( document );    //  <stemmedWord, list<word>>

            //  find synsets
            Dictionary<SynSet, int> resultingSynSetsDict = FindSynsets(stemmingDict) ;   //  <SynSet, wordsCount>

            // lets classify it finally
            return Classify(resultingSynSetsDict);
        }

        private Dictionary<string, List<string>> CreateStemmingDictionary(Document document)
        {
            //  extract page list
            List<string> pageList = ExtractPageList(document);

            //  create stemming dictionary
            int allWordsCount = 0;
            int nonStopWordsCount = 0;
            int stopWordCount = 0;
            int emptyStemsCount = 0;
            Dictionary<string, List<string>> stemmingDict = new Dictionary<string, List<string>>();    //  <stemmedWord, list<word>>
            foreach (var pageContent in pageList)   // stem all words
            {
                string[] words = pageContent.Split(new char[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
                allWordsCount += words.Length;
                foreach (string word in words)
                {
                    //  lowercase the word - necessery for a stopword check and stemmer
                    string wordLower = word.ToLower();
                    //  check if word is a stopword
                    if (StopWord.IsStopWord(wordLower))
                    {
                        stopWordCount++;
                        continue;
                    }
                    nonStopWordsCount++;

                    //  stem word
                    string stemmedWord = Stemmer.Stem(wordLower);
                    if (stemmedWord.Length == 0 || stemmedWord.Equals(" "))
                    {
                        emptyStemsCount++;
                        continue;
                    }

                    //  check if this stem has already happened before and count it
                    List<string> wordList;
                    bool stemmedWordAlreadyProcessed = stemmingDict.TryGetValue(stemmedWord, out wordList); //stemmedWordsOccuranceCountDict.TryGetValue(stemmedWord, out occCount);
                    if (stemmedWordAlreadyProcessed)
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
                }
            }
            Utility.Log("all words count: " + allWordsCount);
            Utility.Log("non stopwords count: " + nonStopWordsCount);
            Utility.Log("stopwords count: " + stopWordCount);
            Utility.Log("empty stems count: " + emptyStemsCount);
            return stemmingDict;
        }

        private Dictionary<SynSet, int> FindSynsets(Dictionary<string, List<string>> stemmingDict )
        {
            //Dictionary<SynSet, int> synSetsWordsCountDict = new Dictionary<SynSet, int>();   //  <SynSet, wordsCount>
            Dictionary<string, Pair<int, Set<SynSet>>> wordSynSetDict = new Dictionary<string, Pair<int, Set<SynSet>>>();
            int stemmedWordsThatHasSynSetsCount = 0;
            try
            {
                Set<SynSet> synSetSet;
                foreach (var stemmingDictionaryEntry in stemmingDict)
                {
                    Debug.Assert(stemmingDictionaryEntry.Value.Count > 0);
                    var stemmedWord = stemmingDictionaryEntry.Key;
                    synSetSet = wordNetEngine.GetSynSets(stemmedWord, WordNetEngine.POS.Noun);

                    if ( synSetSet.Count > 0 )
                    {
                        stemmedWordsThatHasSynSetsCount++;

                        //  wordSynSetDict
                        /*
                         * environment -> environ
                         * environmental -> environment
                         */
                        //wordSynSetDict[stemmedWord] = new Pair<int, Set<SynSet>>(stemmingDictionaryEntry.Value.Count, synSetSet);
                        Pair<int, Set<SynSet>> pair;
                        if (wordSynSetDict.TryGetValue(stemmedWord, out pair))
                        {
                            pair.Second.ThrowExceptionOnDuplicateAdd = false;
                            pair.Second.AddRange(synSetSet);    //  TODO isnt that just adding same set here?
                            pair.First += stemmingDictionaryEntry.Value.Count;
                            wordSynSetDict[stemmedWord] = pair;
                        }
                        else
                        {
                            synSetSet.ThrowExceptionOnDuplicateAdd = false;
                            wordSynSetDict[stemmedWord] = new Pair<int, Set<SynSet>>(stemmingDictionaryEntry.Value.Count, synSetSet);
                        }




                        /*
                        //  synSetsWordsCountDict
                        foreach (SynSet synSet in synSetSet)
                        {
                            int wordCount;
                            if (synSetsWordsCountDict.TryGetValue(synSet, out wordCount))
                            {
                                wordCount += stemmingDictionaryEntry.Value.Count;
                                synSetsWordsCountDict[synSet] = wordCount;
                            }
                            else
                            {
                                synSetsWordsCountDict[synSet] = stemmingDictionaryEntry.Value.Count;
                            }
                        }
                        */
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

                            if ( synSetSet.Count > 0 )
                            {
                                //  wordSynSetDictZ
                                /*
                                high-dimensional -> dimension
                                dimension -> dimens
                                */
                                //wordSynSetDict[nonStemmedWord] = new Pair<int, Set<SynSet>>(nonStemmedWordCountKeyValue.Count, synSetSet);
                                Pair<int, Set<SynSet>> pair;
                                if (wordSynSetDict.TryGetValue(stemmedWord, out pair))
                                {
                                    pair.Second.ThrowExceptionOnDuplicateAdd = false;
                                    pair.Second.AddRange(synSetSet);    //  TODO isnt that just adding same set here?
                                    pair.First += stemmingDictionaryEntry.Value.Count;
                                    wordSynSetDict[stemmedWord] = pair;
                                }
                                else
                                {
                                    synSetSet.ThrowExceptionOnDuplicateAdd = false;
                                    wordSynSetDict[stemmedWord] = new Pair<int, Set<SynSet>>(stemmingDictionaryEntry.Value.Count, synSetSet);
                                }

                            }


                            /*
                            //  synSetsWordsCountDict
                            foreach (SynSet synSet in synSetSet)
                            {
                                int wordCount;
                                if (synSetsWordsCountDict.TryGetValue(synSet, out wordCount))
                                {
                                    wordCount += nonStemmedWordCountKeyValue.Count;
                                    synSetsWordsCountDict[synSet] = wordCount;
                                }
                                else
                                {
                                    synSetsWordsCountDict[synSet] = nonStemmedWordCountKeyValue.Count;
                                }
                            }
                            */
                        }
                    }
                }
            }
            catch (Exception ex) { Debug.Assert(false, ex.ToString()); }    //  TODO show some info window

            /*
            foreach ( var wordSynSetDictEntry in wordSynSetDict )
            {
                var word = wordSynSetDictEntry.Key;
                var wordCount = wordSynSetDictEntry.Value.First;
                var synSetsSet = wordSynSetDictEntry.Value.Second;
                Utility.Log("\t" + word);
                int i = 0;
                foreach (var synSet in synSetsSet)
                {
                    ++i;
                    foreach( var singleWord in synSet.Words )
                    {
                        Utility.Log("\t\t" + i + ":" + singleWord);
                    }
                    Utility.Log("");
                }
            }
            */

            Dictionary<SynSet, int> synSetsWordsCountDict = new Dictionary<SynSet, int>();   //  <SynSet, wordsCount>
            foreach (var wordSynSetDictEntry in wordSynSetDict)
            {
                var word = wordSynSetDictEntry.Key;
                var wordCount = wordSynSetDictEntry.Value.First;
                var synSetsSet = wordSynSetDictEntry.Value.Second;

                Debug.Assert(synSetsSet.Count > 0);
                int wordCountDivided = (int)Math.Ceiling( (double)wordCount / synSetsSet.Count );

                Utility.Log("word " + word + " has " + wordCount + " occurancses and " + synSetsSet.Count + " synsets, so avg" + wordCountDivided + " for each synset");

                foreach ( var synSet in synSetsSet ) {
                    int synSetWordsCount;
                    if (synSetsWordsCountDict.TryGetValue(synSet, out synSetWordsCount))
                    {
                        synSetWordsCount += wordCountDivided;
                        synSetsWordsCountDict[synSet] = synSetWordsCount;
                    }
                    else
                    {
                        synSetsWordsCountDict[synSet] = wordCountDivided;
                    }
                }
            }
            
            
            Utility.Log("stemmed words that has SynSets count: " + stemmedWordsThatHasSynSetsCount);
            Utility.Log("synsets count: " + synSetsWordsCountDict.Count);
            return synSetsWordsCountDict;
        }

        private SynSet Classify(Dictionary<SynSet, int> resultingSynSetsDict)
        {

            //  use top k synsets (that has most words in document)
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
                Utility.Log("\t" + keyValuePair.Value + " words");
                Utility.Log("\tsynset: ");
                foreach (var word in keyValuePair.Key.Words)
                {
                    Utility.Log("\t\t" + word);
                }
            }

            //  lets try to classify the document
            List<WordNetEngine.SynSetRelation> synSetRelations = new List<WordNetEngine.SynSetRelation>();
            synSetRelations.Add(WordNetEngine.SynSetRelation.Hypernym);
            Dictionary<SynSet, double> classificationResultDict = new Dictionary<SynSet, double>(); //  <categorySynSet, mysimiliartyBetwCatSynSetAndDoc>
            Pair<SynSet, double> bestCategory = null;
            foreach (var category in Categories)
            {
                string categoryText = Utility.Words(category);
                double mySimilarity = 0.0d;
                for (int i = 0; i < Math.Min(k, resultingSynSetsList.Count); ++i)
                {
                    SynSet synSetFromDocument = resultingSynSetsList[i].Key;
                    // get the LCS along the similarity relations
                    SynSet lcs = category.GetClosestMutuallyReachableSynset(synSetFromDocument, synSetRelations);
                    if (!(lcs == null))
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
                }

                //  pick best category
                if (bestCategory == null)
                {
                    bestCategory = new Pair<SynSet, double>(category, mySimilarity);
                }
                else
                {
                    if (mySimilarity > bestCategory.Second)
                    {
                        bestCategory = new Pair<SynSet, double>(category, mySimilarity);
                    }
                }
                
                classificationResultDict.Add(category, mySimilarity);
            }
            Utility.Log("results:");
            foreach (var classificationResult in classificationResultDict)
            {
                SynSet category = classificationResult.Key;
                Utility.Log("\t" + Utility.Words(category) + ":" + classificationResult.Value);
            }
            return bestCategory.First;
        }

        private List<string> ExtractPageList(Document document)
        {
            //  extract page list
            List<string> pageList = new List<string>();
            TextExtractor textExtractor = new TextExtractor();
            foreach (var page in document.Pages)
            {
                var textStrings = textExtractor.Extract(page);
                string pageContent = TextExtractor.ToString(textStrings);
                //string[] ssize = content.Split(null);   //  splits by whitespace
                pageList.Add(pageContent);
            }
            return pageList;
        }

        public void Test()
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

            SynSet telecommuncationSynSet = wordNetEngine.GetSynSet("Noun:6282431");
            SynSet telephoneSynSet = wordNetEngine.GetSynSet("Noun:6282943");
            List<WordNetEngine.SynSetRelation> synSetRelations = new List<WordNetEngine.SynSetRelation>();
            synSetRelations.Add(WordNetEngine.SynSetRelation.Hypernym);
            SynSet closestReachableSynSet = telecommuncationSynSet.GetClosestMutuallyReachableSynset(telephoneSynSet, synSetRelations);
            Utility.Log("closest:");
            foreach (var word in closestReachableSynSet.Words)
            {
                Utility.Log("\t" + word);
            }
        }

    }
}
