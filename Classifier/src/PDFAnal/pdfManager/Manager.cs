using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;

namespace PDFAnal.pdfManager
{
	class Manager
	{
		private string			pythonPath;
		private FileListModel   fileModel;

		public Manager()
		{
			fileModel = new FileListModel();
			pythonPath = GetPythonPath();
		}

		public void LoadFromWeb()
		{
			if ( pythonPath == null )
			{
				MessageBox.Show( "Could not find python path in system register!" );
				return;
			}

			Process pythonProcess = new Process();
			pythonProcess.StartInfo.FileName = pythonPath;
			pythonProcess.StartInfo.RedirectStandardOutput = false;
			pythonProcess.StartInfo.UseShellExecute = true;
			pythonProcess.StartInfo.Arguments = Environment.CurrentDirectory + "\\PDFLoader\\pdfLoader.py";
			pythonProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
			pythonProcess.Start();

			//StreamReader stream = pythonProcess.StandardOutput;
			//String output = stream.ReadToEnd();
			//Console.WriteLine( output );

			pythonProcess.WaitForExit();
		}

		// Get python path from registry.
		private string GetPythonPath()
		{
			const string regKey = "Python";
			try
			{
				RegistryKey registryKey = Registry.LocalMachine;
				RegistryKey subKey = registryKey.OpenSubKey("SOFTWARE");
				if ( subKey == null )
					return null;

				RegistryKey microsoftKey = subKey.OpenSubKey("Microsoft");
				if ( microsoftKey == null )
					return null;

				RegistryKey windows = microsoftKey.OpenSubKey( "Windows" );
				if ( windows == null )
					return null;

				RegistryKey currentVersion = windows.OpenSubKey( "CurrentVersion" );
				if ( currentVersion == null )
					return null;

				RegistryKey appPaths = currentVersion.OpenSubKey( "App Paths" );
				if ( appPaths == null )
					return null;

				string[] subkeyNames = appPaths.GetSubKeyNames();		// Get all keys
				int index = -1;
				for ( int i = 0; i < subkeyNames.Length; i++ )
				{
					if ( subkeyNames[i].Contains( "Python.exe" ) )
					{
						index = i;
						break;
					}
				}
				if ( index < 0 )
					return null;

				// Key exists
				RegistryKey pythonKey = appPaths.OpenSubKey( subkeyNames[ index ] );
				object pathObject = pythonKey.GetValue( null );								/// Gets Default field of register
				return pathObject.ToString();
			}
			catch ( Exception e )
			{
				MessageBox.Show( e + "\r\nReading registry " + regKey.ToUpper() );
				return null;
			}
		}


		public void				SetPDFsDirectory( string directory ) { fileModel.SetNewDirectory( directory ); }
		public FileListModel	GetFileModelContext() { return fileModel; }
	}
}
