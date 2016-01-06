using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public void AddPdfModel( string path, ClassifiedPdfModel newModel )
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

		private string MakePath( string directory, string fileName )
		{		return directory + "\\" + fileName;		}
	}
}
