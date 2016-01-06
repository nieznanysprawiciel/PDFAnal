using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDFAnal
{
	/// <summary>
	/// Interaction logic for Progress.xaml
	/// </summary>
	public partial class Progress : Window
	{
		public bool end = false;

		public Progress()
		{
			InitializeComponent();
		}

		public void SetProgress( double value )
		{
			progressBar.Value = value;
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			if( !end )
				e.Cancel = true;
		}
	}
}
