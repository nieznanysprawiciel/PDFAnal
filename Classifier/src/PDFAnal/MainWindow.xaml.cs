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
using System.ComponentModel;

using LAIR.ResourceAPIs.WordNet;
using LAIR.Collections.Generic;

namespace PDFAnal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Document document;
        private Classifier classifier;

        BackgroundWorker m_oWorker;

        public MainWindow()
        {
            InitializeComponent();

            classifier = new Classifier();
            
            //categories
            foreach( var category in classifier.Categories )
            {
                ListBoxCategories.Items.Add(category);
            }

            ProcessPDFBButton.IsEnabled = false;

            m_oWorker = new BackgroundWorker();

            // Create a background worker thread that ReportsProgress &
            // SupportsCancellation
            // Hook up the appropriate events.
            m_oWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
            //m_oWorker.ProgressChanged += new ProgressChangedEventHandler(m_oWorker_ProgressChanged);
            m_oWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    (m_oWorker_RunWorkerCompleted);
            m_oWorker.WorkerReportsProgress = false;
            m_oWorker.WorkerSupportsCancellation = false;
        }

        private void ProcessPDFBButton_Click(object sender, RoutedEventArgs e)
        {
            SetViewEnabled(false);
            Utility.Log("Initiating analysis of document: " + DocumentNameLabel.Content);
            LabelWait.Content = "Analyzing document...";
            m_oWorker.RunWorkerAsync();
        }

        public void onClassificationFinish(SynSet category)
        {
            SetViewEnabled(true);
            LabelWait.Content = "";
            LabelClassifiedAs.Content = "classified as:";
            LabelCategoryName.Content = category.ToString();
        }

        private void LoadPDFButton_Click(object sender, RoutedEventArgs e)
        {
            //  clear labels
            LabelCategoryName.Content = "";
            LabelClassifiedAs.Content = "";

            //  open file dialog
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
                DocumentNameLabel.Content = System.IO.Path.GetFileName(fileName);
                ProcessPDFBButton.IsEnabled = true;
            }
        }

        private void SetViewEnabled(bool enabled)
        {
            ProcessPDFBButton.IsEnabled = enabled;
            LoadPDFButton.IsEnabled = enabled;
            TestButton.IsEnabled = enabled;
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            classifier.Test();
        }

        /// <summary>
        /// On completed do the appropriate task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            onClassificationFinish((SynSet)e.Result);
        }

        /// <summary>
        /// Notification is performed here to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void m_oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //}

        /// <summary>
        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SynSet category = classifier.Classify(document);
            e.Result = category;
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
    }
}
