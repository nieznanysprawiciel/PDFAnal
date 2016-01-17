using System.Collections.Generic;
using System.IO;
using System.ComponentModel;

namespace PDFAnal.pdfManager
{
	class FileListModel : INotifyPropertyChanged
	{
		private List<FileItem> folderContent;
		public event PropertyChangedEventHandler PropertyChanged;

		#region.Functions

		// Constructor
		public FileListModel()
		{
			folderContent = new List<FileItem>();
		}

		public void SetNewDirectory( string directory )
		{
			DirectoryInfo dirInfo = new DirectoryInfo( directory );
			FileInfo[] info = dirInfo.GetFiles("*.pdf");

			var newContent = new List<FileItem>();

			foreach ( var File in info )
				newContent.Add( new FileItem( File.Name ) );
			DirContent = newContent;
		}


		public List<FileItem> DirContent
		{
			get { return folderContent; }
			set
			{
				folderContent = value;
				OnPropertyChanged( "DirContent" );
			}
		}



		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged( string name )
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if ( handler != null )
			{
				handler( this, new PropertyChangedEventArgs( name ) );
			}
		}

		#endregion
	}
}
