using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using org.pdfclown.files;
using org.pdfclown.documents;
using org.pdfclown.tools;
using org.pdfclown.documents.contents;
using System.Drawing;

namespace PDFAnal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
                Document document = file.Document;

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
				string content = TextExtractor.ToString( textStrings );

				ContentTextBox.Text += content + "\n\n";
            }

            //

        }

    }
}
