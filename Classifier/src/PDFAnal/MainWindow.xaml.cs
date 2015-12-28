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
using System.Windows.Controls;
using System.IO;


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

        private BackgroundWorker classifyDocumentBackgroundWorker;
        private BackgroundWorker addPredefinedCategoriesBackgroundWorker;

        public MainWindow()
        {
            InitializeComponent();

            ProcessPDFBButton.IsEnabled = false;

            classifier = new Classifier();
            updateCategories(classifier.CategoriesNew.Keys.ToList<object>());
            
            /*
            //categories
            foreach( var category in classifier.Categories )
            {
                ListBoxCategories.Items.Add(category);
            }
            */

            //  create background workers
            classifyDocumentBackgroundWorker = new BackgroundWorker();
            classifyDocumentBackgroundWorker.DoWork += new DoWorkEventHandler(classifyDocumentBackgroundWorker_DoWork);
            //m_oWorker.ProgressChanged += new ProgressChangedEventHandler(m_oWorker_ProgressChanged);
            classifyDocumentBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(classifyDocumentBackgroundWorker_RunWorkerCompleted);
            classifyDocumentBackgroundWorker.WorkerReportsProgress = false;
            classifyDocumentBackgroundWorker.WorkerSupportsCancellation = false;

            addPredefinedCategoriesBackgroundWorker = new BackgroundWorker();
            addPredefinedCategoriesBackgroundWorker.DoWork += new DoWorkEventHandler(addPredefinedCategoriesBackgroundWorker_DoWork);
            //m_oWorker.ProgressChanged += new ProgressChangedEventHandler(m_oWorker_ProgressChanged);
            addPredefinedCategoriesBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(addPredefinedCategoriesBackgroundWorker_RunWorkerCompleted);
            addPredefinedCategoriesBackgroundWorker.WorkerReportsProgress = false;
            addPredefinedCategoriesBackgroundWorker.WorkerSupportsCancellation = false;
        }


        #region button clicks

        private void ProcessPDFBButton_Click(object sender, RoutedEventArgs e)
        {
            if (document == null)
            {
                LabelWait.Content = "Null doc";
                return;
            }
            SetViewEnabled(false);
            Utility.Log("Initiating analysis of document: " + DocumentNameLabel.Content);
            LabelWait.Content = "Analyzing document...";
            classifyDocumentBackgroundWorker.RunWorkerAsync();
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
                org.pdfclown.files.File file = new org.pdfclown.files.File(fileName);
                document = file.Document;
                DocumentNameLabel.Content = System.IO.Path.GetFileName(fileName);
                ProcessPDFBButton.IsEnabled = true;
            }
        }

        private void ButtonAddCategory_Click(object sender, RoutedEventArgs e)
        {
            //  open file dialog
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".txt";
            //  fileDialog.Filter = "PDF documents (.pdf)|*.pdf";
            fileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            Nullable<bool> result = fileDialog.ShowDialog();
            if (result == true)
            {
                string fileName = fileDialog.FileName;
                string categoryDefinition = System.IO.File.ReadAllText(fileName);
                string documentName = System.IO.Path.GetFileName(fileName);
                classifier.AddCategory(documentName, categoryDefinition);
            }

            //  update View
            updateCategories(classifier.CategoriesNew.Keys.ToList<object>());

        }

        private void ButtonLoadPredefinedCategories_Click(object sender, RoutedEventArgs e)
        {
            SetViewEnabled(false);
            addPredefinedCategoriesBackgroundWorker.RunWorkerAsync();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            classifier.Test();
        }

        #endregion

        #region background workers

        /// <summary>
        /// On completed do the appropriate task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void classifyDocumentBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            onClassificationFinish(e.Result);
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
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void classifyDocumentBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var category = classifier.Classify(document);
            e.Result = category;
        }

        void addPredefinedCategoriesBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //  update View
            updateCategories(classifier.CategoriesNew.Keys.ToList<object>());
            SetViewEnabled(true);
        }

        void addPredefinedCategoriesBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //  load categories from categories directory
            string[] filePaths = Directory.GetFiles(@"..\categories\", "*.txt");
            foreach (string filePath in filePaths)
            {
                string categoryDefinition = System.IO.File.ReadAllText(filePath);
                string documentName = System.IO.Path.GetFileName(filePath);
                classifier.AddCategory(documentName, categoryDefinition);
            }
        }

        #endregion

        #region view manipulations

        public void updateCategories(List<object> categories)
        {
            ListBoxCategories.Items.Clear();
            foreach (var category in categories)
            {
                ListBoxCategories.Items.Add(category);
            }
        }

        public void onClassificationFinish(Object category)
        {
            SetViewEnabled(true);
            LabelWait.Content = "";
            if (category != null)
            {
                LabelClassifiedAs.Content = "classified as:";
                LabelCategoryName.Content = category.ToString();
            }
            else
            {
                LabelClassifiedAs.Content = "classification failed";
            }

        }

        private void SetViewEnabled(bool enabled)
        {
            ProcessPDFBButton.IsEnabled = enabled;
            LoadPDFButton.IsEnabled = enabled;
            TestButton.IsEnabled = enabled;
            ButtonAddCategory.IsEnabled = enabled;
            ButtonLoadPredefinedCategories.IsEnabled = enabled;
        }
        #endregion

        /*
        //  straszne dziadostwo jezeli chodzi o parsowanie
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
