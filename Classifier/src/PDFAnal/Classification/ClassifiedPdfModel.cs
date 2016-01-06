using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace PDFAnal.Classification
{
	public class ClassifiedPdfModel
	{
		public string FileName { get; set; }
		public List<ClassifiedItem> Items { get; }


		public ClassifiedPdfModel()
		{
			Items = new List<ClassifiedItem>();
		}

		public void AddClassificationData( ClassifiedItem newItem )
		{
			Items.Add( newItem );
		}
	}
}
