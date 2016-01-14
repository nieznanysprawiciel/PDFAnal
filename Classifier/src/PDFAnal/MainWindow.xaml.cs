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
using PDFAnal.pdfManager;
using PDFAnal.Classification;

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
		private ClassifiedCollection classifiedDocuments;

        private BackgroundWorker classifyDocumentBackgroundWorker;
		private BackgroundWorker classifyAllDocumentBackgroundWorker;
		private BackgroundWorker addPredefinedCategoriesBackgroundWorker;
		private BackgroundWorker pdfsLoadingBackgroundWorker;
		private Progress progressWindow;

		private bool predefinedCategoriesLoaded = false;

		private Manager				PDFs;

        public MainWindow()
        {
            InitializeComponent();

			//  gui
			//ProcessPDFBButton.IsEnabled = false;
			progressWindow = new Progress();
			progressWindow.SetProgress( 0.0 );
			HideProgress();
			progressWindow.Show();		// Show hidden window :P


			string[] predefinedCategoriesFilePathsArr = predefinedCategoriesFilePaths();
            int predefCatCount = predefinedCategoriesFilePathsArr.Length;
            ButtonLoadPredefinedCategories.Content = ButtonLoadPredefinedCategories.Content + " (" + predefCatCount + ")";


            classifier = new Classifier();
            updateCategories(classifier.CategoriesNew);
            
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
			//classifyDocumentBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressFunction);
            classifyDocumentBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(classifyDocumentBackgroundWorker_RunWorkerCompleted);
            classifyDocumentBackgroundWorker.WorkerReportsProgress = false;
            classifyDocumentBackgroundWorker.WorkerSupportsCancellation = false;

			classifyAllDocumentBackgroundWorker = new BackgroundWorker();
			classifyAllDocumentBackgroundWorker.DoWork += new DoWorkEventHandler( DoWorkClassifyAll );
			classifyAllDocumentBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler( ProgressClassifyAll );
			classifyAllDocumentBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( WorkCompletedClassifyAll );
			classifyAllDocumentBackgroundWorker.WorkerReportsProgress = true;
			classifyAllDocumentBackgroundWorker.WorkerSupportsCancellation = false;

			addPredefinedCategoriesBackgroundWorker = new BackgroundWorker();
            addPredefinedCategoriesBackgroundWorker.DoWork += new DoWorkEventHandler(addPredefinedCategoriesBackgroundWorker_DoWork);
			addPredefinedCategoriesBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler( ProgressFunction );
            addPredefinedCategoriesBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(addPredefinedCategoriesBackgroundWorker_RunWorkerCompleted);
            addPredefinedCategoriesBackgroundWorker.WorkerReportsProgress = true;
            addPredefinedCategoriesBackgroundWorker.WorkerSupportsCancellation = false;

			/// Init PDFs loading controls
			pdfsDirectoryTextBox.Text = Environment.CurrentDirectory + "\\Data\\";
			PDFs = new pdfManager.Manager();
			PDFs.SetPDFsDirectory( pdfsDirectoryTextBox.Text );

			directoryContentListBox.DataContext = PDFs.GetFileModelContext();

			// PDFs loading background worker
			pdfsLoadingBackgroundWorker = new BackgroundWorker();
			pdfsLoadingBackgroundWorker.WorkerReportsProgress = false;
			pdfsLoadingBackgroundWorker.WorkerSupportsCancellation = false;
			pdfsLoadingBackgroundWorker.DoWork += new DoWorkEventHandler( DoWorkLoadPDFsWorker );
			pdfsLoadingBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( WorkCompletedLoadPDFsWorker );

			// Collection of classified documents data
			classifiedDocuments = new ClassifiedCollection();
		}

		#region progress

		private void ProgressFunction( object sender, ProgressChangedEventArgs e )
		{
			progressWindow.SetProgress( e.ProgressPercentage );
		}

		private void ShowProgressIndeterminate()
		{
			progressWindow.progressBar.IsIndeterminate = true;
			progressWindow.Visibility = Visibility.Visible;
			progressWindow.Show();
			progressWindow.Focus();
		}

		private void ShowProgressPercentage()
		{
			progressWindow.SetProgress( 0.0 );
			progressWindow.progressBar.IsIndeterminate = false;
			progressWindow.Visibility = Visibility.Visible;
			progressWindow.Show();
			progressWindow.Focus();
		}

		private void HideProgress()
		{
			progressWindow.Visibility = Visibility.Hidden;
		}

		#endregion


		#region button clicks

		private void ProcessPDFBButton_Click(object sender, RoutedEventArgs e)
        {
			LoadPDFButton_Click( sender, e );

			if ( document == null )
				return;

			SetViewEnabled(false);
            Utility.Log("Initiating analysis of document: " + DocumentNameLabel.Content);
            
            LabelClassifiedAs.Content = "Analyzing document...";

			ShowProgressIndeterminate();
			classifyDocumentBackgroundWorker.RunWorkerAsync();
        }

		private void ProcessAllPDFs_Click( object sender, RoutedEventArgs e )
		{
			SetViewEnabled( false );
			LabelClassifiedAs.Content = "Analyzing documents...";

			ShowProgressPercentage();

			string directory = pdfsDirectoryTextBox.Text;
			classifyAllDocumentBackgroundWorker.RunWorkerAsync( directory );
		}

		private void LoadPDFButton_Click(object sender, RoutedEventArgs e)
        {
            LabelClassifiedAs.Content = "";

			var selectedItem = directoryContentListBox.SelectedItem as PDFAnal.pdfManager.FileItem;
			if ( selectedItem == null )
				return;

			LoadPDF( pdfsDirectoryTextBox.Text, selectedItem.Name );
        }

		private void LoadPDF( string directory, string fileName )
		{
			string fullPath = directory + "\\" + fileName;

			org.pdfclown.files.File file = new org.pdfclown.files.File( fullPath );
			document = file.Document;
			DocumentNameLabel.Content = fileName;
			ProcessPDFBButton.IsEnabled = true;
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
                documentName = documentName.Replace(".txt", "");
                classifier.AddCategory(documentName, categoryDefinition);
            }

            //  update View
            updateCategories(classifier.CategoriesNew);

        }

        private void ButtonLoadPredefinedCategories_Click(object sender, RoutedEventArgs e)
        {
            SetViewEnabled(false);
			ShowProgressPercentage();

			addPredefinedCategoriesBackgroundWorker.RunWorkerAsync();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            classifier.Test();
        }

        #endregion

        #region background workers
		void DoWorkClassifyAll( object sender, DoWorkEventArgs e )
		{
			BackgroundWorker worker = sender as BackgroundWorker;

			string directory = e.Argument as string;
			var dirInfo = new DirectoryInfo( directory );
			FileInfo[] infos = dirInfo.GetFiles( "*.pdf" );

			var numPDFs = infos.Length;
			int progress = 0;

			foreach ( var fileInfo in infos )
			{
				string fullPath = directory + "\\" + fileInfo.Name;

				org.pdfclown.files.File file = new org.pdfclown.files.File( fullPath );
				document = file.Document;
				if ( document == null )
					continue;

				var category = classifier.Classify( document );
				if ( category != null )
				{
					ClassifiedPdfModel newModel = new ClassifiedPdfModel();

					int i = 0;
					foreach ( var classResult in (List<Pair<object, double>>)category )
					{
						++i;
						ClassifiedItem newItem = new ClassifiedItem();
						newItem.Category = classResult.First as string;
						newItem.Compatibility = classResult.Second.ToString();

						newModel.AddClassificationData( newItem );
					}

					classifiedDocuments.AddPdfModel( directory, fileInfo.Name, newModel );
				}

				worker.ReportProgress( (int)( 100 * progress++ / (double)numPDFs ) );
			}
		}

		void WorkCompletedClassifyAll( object sender, RunWorkerCompletedEventArgs e )
		{
			LabelClassifiedAs.Content = "classified as:";
			SetViewEnabled( true );
			HideProgress();
		}

		// Ta funkcja duplikuje ProgressFunction. Myslałem, że będzie tu inna funkcjonalność....
		private void ProgressClassifyAll( object sender, ProgressChangedEventArgs e )
		{
			progressWindow.SetProgress( e.ProgressPercentage );
		}


		/// <summary>
		/// On completed do the appropriate task
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void classifyDocumentBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
			var selectedItem = directoryContentListBox.SelectedItem as FileItem;

			onClassificationFinish(e.Result, selectedItem.Name);
			HideProgress();
        }


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
            updateCategories(classifier.CategoriesNew);
            predefinedCategoriesLoaded = true;
            SetViewEnabled(true);
			HideProgress();
        }

        void addPredefinedCategoriesBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
			BackgroundWorker worker = sender as BackgroundWorker;

			//  load categories from categories directory
			string[] filePaths = predefinedCategoriesFilePaths();
			var numProgresses = filePaths.Length * 2;
			double progress = 0;

            foreach (string filePath in filePaths)
            {
                string categoryDefinition = System.IO.File.ReadAllText(filePath);
                string documentName = System.IO.Path.GetFileName(filePath);
                documentName = documentName.Replace(".txt", "");

				worker.ReportProgress( (int)( 100 * progress++ / (double)numProgresses ) );

				classifier.AddCategory(documentName, categoryDefinition);

				worker.ReportProgress( (int)(100 * progress++ / (double)numProgresses) );
            }
        }

        private string[] predefinedCategoriesFilePaths()
        {
            return Directory.GetFiles(@"..\categories\", "*.txt");
        }

		private void DoWorkLoadPDFsWorker( object sender, DoWorkEventArgs e )
		{
			PDFs.LoadFromWeb( e.Argument as string );
		}

		private void WorkCompletedLoadPDFsWorker( object sender, RunWorkerCompletedEventArgs e )
		{
			loadRemotePDFsButton.IsEnabled = true;
		}

		#endregion

		#region view manipulations

		public void updateCategories(List<Category> categories)
        {
            ListBoxCategories.Items.Clear();
            foreach (var category in categories)
            {
                ListBoxCategories.Items.Add(category.Name);
            }
        }

        public void onClassificationFinish(Object category, string name)
        {
            SetViewEnabled(true);
            //LabelWait.Content = "";
            if (category != null)
            {
                LabelClassifiedAs.Content = "classified as:";

				ClassifiedPdfModel newModel = new ClassifiedPdfModel();

                int i = 0;
                foreach (var classResult in (List<Pair<object, double>>)category)
                {
                    ++i;
					ClassifiedItem newItem = new ClassifiedItem();
					newItem.Category = classResult.First as string;
					newItem.Compatibility = classResult.Second.ToString();

					newModel.AddClassificationData( newItem );
					//ListBoxClassificationResult.Items.Add(i + ". " + classResult.First + "\t\t\t\t" + classResult.Second);
                }

				classifiedDocuments.AddPdfModel( pdfsDirectoryTextBox.Text, name, newModel );
				ListBoxClassificationResult.DataContext = newModel;
			}
            else
            {
                LabelClassifiedAs.Content = "classification failed";
            }

        }

        private void SetViewEnabled(bool enabled)
        {
            ProcessPDFBButton.IsEnabled = enabled;
			ProcessAllPDFs.IsEnabled = enabled;
            TestButton.IsEnabled = enabled;
            ButtonAddCategory.IsEnabled = enabled;
            ButtonLoadPredefinedCategories.IsEnabled = enabled && !predefinedCategoriesLoaded;

			directoryContentListBox.IsEnabled = enabled;
			pdfsDirectoryTextBox.IsEnabled = enabled;
			changeDirectoryButton.IsEnabled = enabled;
		}
        #endregion

        private void LoadPDFsFromWeb( object sender, RoutedEventArgs e )
        {
			loadRemotePDFsButton.IsEnabled = false;
			pdfsLoadingBackgroundWorker.RunWorkerAsync( pdfsDirectoryTextBox.Text );
		}

		private void ChangeDirectory( object sender, RoutedEventArgs e )
		{
			System.Windows.Forms.FolderBrowserDialog openFolder = new System.Windows.Forms.FolderBrowserDialog();
			openFolder.SelectedPath = pdfsDirectoryTextBox.Text;

			var result = openFolder.ShowDialog();
			if ( result == System.Windows.Forms.DialogResult.OK )
			{
				pdfsDirectoryTextBox.Text = openFolder.SelectedPath;
				PDFs.SetPDFsDirectory( openFolder.SelectedPath );
			}
		}

		private void directoryContentListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			var selectedItem = directoryContentListBox.SelectedItem as FileItem;

			string fileName = selectedItem.Name;
			string directory = pdfsDirectoryTextBox.Text;

			// May be null, but that's no problem.
			var dataModel = classifiedDocuments.GetPdfModel( directory, fileName );
			ListBoxClassificationResult.DataContext = dataModel;
			DocumentNameLabel.Content = fileName;
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			progressWindow.end = true;
			progressWindow.Close();
		}

	}
}
