using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using org.pdfclown.files;
using org.pdfclown.documents;
using org.pdfclown.tools;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.objects;
using System.Drawing;

namespace PDFAnal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Document document;
        private List<string> pageList = new List<string>();


        public MainWindow()
        {
            InitializeComponent();
        }

        private void ProcessPDFBButton_Click(object sender, RoutedEventArgs e)
        {
            Utility.log(pageList.Count.ToString());
            
            List<string> stemmedWords = new List<string>();
            foreach (var pageContent in pageList)   //    stem all words
            {
                string[] words = pageContent.Split(null);   //  splits by whitespace
                foreach (string word in words)
                {
                    //  lowercase the word - necessery for a stemmer
                    string stemmedWord = stem(word.ToLower());
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

        private string stem( string word )
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

    }
}
