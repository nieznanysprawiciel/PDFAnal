using System;
using System.Collections.Generic;
using System.IO;

namespace PDFAnal.Classification
{
	public class ClassifiedCollection
	{
		private Dictionary<string, ClassifiedPdfModel>   doneClassifications;

		public ClassifiedCollection()
		{
			doneClassifications = new Dictionary<string, ClassifiedPdfModel>();
		}

		public void AddPdfModel( string directory, string fileName, ClassifiedPdfModel newModel )
		{
			doneClassifications [ MakePath( directory, fileName ) ] = newModel;
		}

		private void AddPdfModel( string path, ClassifiedPdfModel newModel )
		{
			doneClassifications[ path ] = newModel;
		}

		public ClassifiedPdfModel GetPdfModel( string directory, string fileName )
		{
			string key = MakePath( directory, fileName );

			if( doneClassifications.ContainsKey( key ) )
				return doneClassifications[ key ];
			return null;
		}

		public string MakePath( string directory, string fileName )
		{
			string path = directory + "\\" + fileName;
			path = Path.GetFullPath( path );
			return path;
		}
	}
}
