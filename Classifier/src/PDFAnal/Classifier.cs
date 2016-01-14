using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
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

        //public Dictionary<string, Pair<Dictionary<SynSet, int>, int>> CategoriesNew { get; private set; }
        public List<Category> CategoriesNew { get; private set;  }

        public Classifier()
        {
            wordNetEngine = new WordNetEngine(@"..\..\resources", false);
            semanticSimilarityModel = new WordNetSimilarityModel(wordNetEngine);

            //CategoriesNew = new Dictionary<string, Pair<Dictionary<SynSet, int>, int>>();
            CategoriesNew = new List<Category>();
        }

        public void AddCategory(string name, string categoryDefiniction)
        {
            string[] wordArray = Split(categoryDefiniction);
            AddCategory(name, wordArray.ToList());
        }

        public void AddCategory(string name, List<string> wordList)
        {
            //  create stemming dictionary
            Dictionary<string, List<string>> stemmingDict = CreateStemmingDictionary(wordList);    //  <stemmedWord, list<word>>

            //  find synsets
            Dictionary<SynSet, int> resultingSynSetsDict = FindSynsets(stemmingDict);   //  <SynSet, wordsCount>

            //  count important words
            int importantWordsCont = 0;
            foreach (var entry in resultingSynSetsDict)
            {
                importantWordsCont += entry.Value;
            }

            //  add category
            //CategoriesNew[name] = new Pair<Dictionary<SynSet, int>, int>(resultingSynSetsDict, importantWordsCont);
            CategoriesNew.Add(new Category(name, importantWordsCont, resultingSynSetsDict));
        }

        public object Classify(Document document)
        {
            //  extract word list
            List<string> wordList = ExtractWords( document );

            if (wordList.Count == 0)
            {
                return null;
            }

            //  create stemming dictionary
            Dictionary<string, List<string>> stemmingDict = CreateStemmingDictionary( wordList );    //  <stemmedWord, list<word>>

            //  find synsets
            Dictionary<SynSet, int> resultingSynSetsDict = FindSynsets(stemmingDict) ;   //  <SynSet, wordsCount>

            // lets classify it finally
            return ClassifyFinally(resultingSynSetsDict);
        }

        private List<string> ExtractWords(Document document)
        {
            //  extract page list
            List<string> pageList = ExtractPageList(document);

            //  extract single words
            List<string> allWordsList = new List<string>();
            foreach (var pageContent in pageList)   // stem all words
            {
                string[] words = Split(pageContent);
                allWordsList.AddRange(words);
            }
            return allWordsList;

        }

        private Dictionary<string, List<string>> CreateStemmingDictionary(List<string> allWordsList)
        {
            //  create stemming dictionary
            int allWordsCount = 0;
            int nonStopWordsCount = 0;
            int stopWordCount = 0;
            int emptyStemsCount = 0;

            //stem
            Dictionary<string, List<string>> stemmingDict = new Dictionary<string, List<string>>();    //  <stemmedWord, list<word>>
            foreach (string word in allWordsList)
            {
                ++allWordsCount;

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

                //Utility.Log("word " + word + " has " + wordCount + " occurances and " + synSetsSet.Count + " synsets, so avg" + wordCountDivided + " for each synset");

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

        /*
        private SynSet ClassifyFinally(Dictionary<SynSet, int> resultingSynSetsDict)
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
        */

        private object ClassifyFinally(Dictionary<SynSet, int> documentSynSetsDict)
        {
            //  count doc words
            int docWordCount = 0;
            foreach (var x in documentSynSetsDict) {
                docWordCount += x.Value;
            }

            //Pair<string, double> bestCategory = null;

            //  sort document synsets decreasingly accordint to score
            List<KeyValuePair<SynSet, int>> documentTempList = documentSynSetsDict.ToList();
            documentTempList.Sort(
                delegate(KeyValuePair<SynSet, int> firstPair, KeyValuePair<SynSet, int> nextPair)
                {
                    return nextPair.Value.CompareTo(firstPair.Value);
                }
            );

            //Set<SynSet> objectSynSetsSet = new Set<SynSet>(resultingSynSetsDict.Keys.ToList());
            var resultList = new List<Pair<object, double>>();

            foreach (var category in CategoriesNew)
            {
                string categoryName = category.Name;

                /*
                Set<SynSet> categorySynSetsSet = new Set<SynSet>(category.Value.First.Keys.ToList());
                Set<SynSet> synSetsIntersection = categorySynSetsSet;
                synSetsIntersection.IntersectWith(objectSynSetsSet);
             
                int commonSynSetsCount = synSetsIntersection.Count;
                if (commonSynSetsCount == 0)
                {
                    continue;
                }
                Utility.Log(categoryName + " has " + commonSynSetsCount + " common synsets with this doc");

                //  gather synset data from category and document
                Dictionary<SynSet, Pair<int, int>> temp = new Dictionary<SynSet, Pair<int, int>>();
                foreach (var commonSynSet in synSetsIntersection)
                {
                    temp.Add(commonSynSet, new Pair<int, int>(category.Value.First[commonSynSet], resultingSynSetsDict[commonSynSet]));
                }
                

                //  use top k synsets
                int k = 4;
                Utility.Log("top " + k + ": ");
                List<KeyValuePair<SynSet, Pair<int, int>>> tempList = temp.ToList();
                tempList.Sort(
                    delegate(KeyValuePair<SynSet, Pair<int, int>> firstPair, KeyValuePair<SynSet, Pair<int, int>> nextPair)
                    {
                        
                        //Debug.Assert(firstPair.Value.First > 0 && firstPair.Value.Second > 0 && nextPair.Value.First > 0 && nextPair.Value.Second > 0);
                        //double firstCosineSimilarity = (double)(firstPair.Value.First * firstPair.Value.Second) / (firstPair.Value.First * firstPair.Value.Second);
                        //double nextCosineSimilarity = (double)(nextPair.Value.First * nextPair.Value.Second) / (nextPair.Value.First * nextPair.Value.Second);
                        //Debug.Assert(firstCosineSimilarity <= 1.0d && firstCosineSimilarity >= -1.0d && nextCosineSimilarity <= 1.0d && nextCosineSimilarity >= -1.0d);
                        //return nextCosineSimilarity.CompareTo(firstCosineSimilarity);
                        
                        return (nextPair.Value.First * nextPair.Value.Second).CompareTo(firstPair.Value.First * firstPair.Value.Second);
                    }
                );
                */


                //List<KeyValuePair<SynSet, int>> categoryTempList = category.SynSetDictionary.ToList();
                //categoryTempList.Sort(
                //    delegate(KeyValuePair<SynSet, int> firstPair, KeyValuePair<SynSet, int> nextPair)
                //    {
                //        return nextPair.Value.CompareTo(firstPair.Value);
                //    }
                //);
                List<KeyValuePair<SynSet, int>> categoryTempList = category.SynSetOrderedList;

                //  use top k synsets
                int k = 100;
                Utility.Log("top " + k + ": ");

                // compute category - document cosine similarity
                Set<SynSet> alreadyProcessedSynSets = new Set<SynSet>();    //  TODO reuse it with all categories

                double categoryDocumentCosineSimilarityNumerator = 0.0d;
                double categoryDocumentCosineSimilarityDenumerator1 = 0.0d;
                double categoryDocumentCosineSimilarityDenumerator2 = 0.0d;

                // category SynSets
                int count = 0;
                foreach (var categorySynSetInfo in categoryTempList)//category.Value.First)
                {
                    ++count;
                    if (count > k)
                    {
                        break;
                    }

                    var categorySynSet = categorySynSetInfo.Key;
                    int documentSynSetWordCount;
                    if (!documentSynSetsDict.TryGetValue(categorySynSet, out documentSynSetWordCount))
                    {
                        documentSynSetWordCount = 0;
                    }
                    var categorySynSetWordCount = categorySynSetInfo.Value;
                    int cateogryWordCount = category.WordCount;

                    //  compute cosine similarity
                    categoryDocumentCosineSimilarityNumerator += (double)(categorySynSetWordCount * documentSynSetWordCount) / (cateogryWordCount * docWordCount);
                    categoryDocumentCosineSimilarityDenumerator1 += (double)(categorySynSetWordCount * categorySynSetWordCount) / (cateogryWordCount * cateogryWordCount);
                    categoryDocumentCosineSimilarityDenumerator2 += (double)(documentSynSetWordCount * documentSynSetWordCount) / (docWordCount * docWordCount) ;

                    alreadyProcessedSynSets.Add(categorySynSet);

                    Utility.Log(categoryDocumentCosineSimilarityNumerator + " / " + categoryDocumentCosineSimilarityDenumerator1 + " * " + categoryDocumentCosineSimilarityDenumerator2);

                    //  log
                    Utility.Log("--- category synset " + count + " -->");
                    Utility.Log("\t" + categorySynSetWordCount + "/" + documentSynSetWordCount + " words " /* : [" + categoryDocumentCosineSimilarityNumerator + "/ sqrt(" + categoryDocumentCosineSimilarityDenumerator1 + ")sqrt(" + categoryDocumentCosineSimilarityDenumerator2 + ")]"*/);
                    Utility.Log("\tsynset: ");
                    foreach (var word in categorySynSet.Words)
                    {
                        Utility.Log("\t\t" + word);
                    }
                }

                //  document synSets
                count = 0;
                foreach ( var documentSynSetInfo in documentTempList )
                {
                    ++count;
                    if (count > k)
                    {
                        break;
                    }

                    var documentSynSet = documentSynSetInfo.Key;

                    if (alreadyProcessedSynSets.Contains(documentSynSet))
                    {
                        continue;
                    }

                    int categorySynSetWordCount;
                    if (!category.SynSetDictionary.TryGetValue(documentSynSet, out categorySynSetWordCount))
                    {
                        categorySynSetWordCount = 0;
                    }
                    int cateogryWordCount = category.WordCount;
                    var documentSynSetWordCount = documentSynSetInfo.Value;

                    //  compute cosine similarity
                    categoryDocumentCosineSimilarityNumerator += (double)(categorySynSetWordCount * documentSynSetWordCount) / (cateogryWordCount * docWordCount);
                    categoryDocumentCosineSimilarityDenumerator1 += (double)(categorySynSetWordCount * categorySynSetWordCount) / (cateogryWordCount * cateogryWordCount);
                    categoryDocumentCosineSimilarityDenumerator2 += (double)(documentSynSetWordCount * documentSynSetWordCount) / (docWordCount * docWordCount);

                    Utility.Log(categoryDocumentCosineSimilarityNumerator + " / " + categoryDocumentCosineSimilarityDenumerator1 + " * " + categoryDocumentCosineSimilarityDenumerator2);

                    alreadyProcessedSynSets.Add(documentSynSet);

                    //  log
                    Utility.Log("--- document synset " + count + " -->");
                    Utility.Log("\t" + categorySynSetWordCount + "/" + documentSynSetWordCount + " words " /* : [" + categoryDocumentCosineSimilarityNumerator + "/ sqrt(" + categoryDocumentCosineSimilarityDenumerator1 + ")sqrt(" + categoryDocumentCosineSimilarityDenumerator2 + ")]"*/);
                    Utility.Log("\tsynset: ");
                    foreach (var word in documentSynSet.Words)
                    {
                        Utility.Log("\t\t" + word);
                    }

                }

                double categoryDocumentCosineSimilarityDenominator = Math.Sqrt(categoryDocumentCosineSimilarityDenumerator1) * Math.Sqrt(categoryDocumentCosineSimilarityDenumerator2);
                Debug.Assert(categoryDocumentCosineSimilarityDenominator > 0.0d);
                double categoryDocumentCosineSimilarity = (double) categoryDocumentCosineSimilarityNumerator / categoryDocumentCosineSimilarityDenominator;

                Utility.Log(categoryName + " similarity:" + categoryDocumentCosineSimilarity);

                //  add category sim score to result list
                resultList.Add(new Pair<object, double>(categoryName, categoryDocumentCosineSimilarity));

                //  pick best category
                //if (bestCategory == null)
                //{
                //    bestCategory = new Pair<string, double>(categoryName, categoryDocumentCosineSimilarity);
                //}
                //else
                //{
                //    if (categoryDocumentCosineSimilarity > bestCategory.Second)
                //    {
                //        bestCategory.First = categoryName;
                //        bestCategory.Second = categoryDocumentCosineSimilarity;
                //    }
                //}

            }

            resultList.Sort(delegate(Pair<object, double> first, Pair<object, double> next) {
                return next.Second.CompareTo(first.Second);
            });

            return resultList;//   bestCategory.First;  //  return best category name
        }

        private List<string> ExtractPageList(Document document)
        {
            //  extract page list
            List<string> pageList = new List<string>();
            TextExtractor textExtractor = new TextExtractor();
            try {
                foreach (var page in document.Pages)
                {

                    var textStrings = textExtractor.Extract(page);
                    string pageContent = TextExtractor.ToString(textStrings);
                    //string[] ssize = content.Split(null);   //  splits by whitespace
                    pageList.Add(pageContent);
                }
            }
            catch (Exception e)
            {
                Utility.Log("Blad");
            }
            return pageList;
        }

        private string[] Split(string text)
        {
            return text.Split(new char[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
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
