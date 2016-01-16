using System;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;
using System.IO;

namespace PDFAnal.pdfManager
{
	class Manager
	{
		private string					pythonPath;
		private FileListModel			fileModel;

		private FileSystemWatcher       directoryWatcher;
		private string                  watcherDirectory;

		public Manager()
		{
			fileModel = new FileListModel();
			pythonPath = GetPythonPath();
			directoryWatcher = null;
		}

		private void InitFileSystemWatcher( string path )
		{
			// Disable last watcher (I don't know, if it isn't useless.
			if( directoryWatcher != null )
				directoryWatcher.EnableRaisingEvents = false;

			directoryWatcher = new FileSystemWatcher();
			directoryWatcher.Path = path;
			directoryWatcher.IncludeSubdirectories = false;
			directoryWatcher.Filter = "*.pdf";
			directoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

			directoryWatcher.Changed += new FileSystemEventHandler( OnDirectoryChanged );
			directoryWatcher.Created += new FileSystemEventHandler( OnDirectoryChanged );
			directoryWatcher.Deleted += new FileSystemEventHandler( OnDirectoryChanged );
			directoryWatcher.Renamed += new RenamedEventHandler( OnDirectoryRenamed );
			directoryWatcher.Error += new ErrorEventHandler( OnError );

			directoryWatcher.EnableRaisingEvents = true;
		}

		private void OnError( object source, ErrorEventArgs e )
		{
			fileModel.SetNewDirectory( watcherDirectory );
		}

		private void ChangeWatcherDirectory( string path )
		{
			directoryWatcher.Path = path;
		}

		private void OnDirectoryChanged( object sender, FileSystemEventArgs e )
		{
			fileModel.SetNewDirectory( watcherDirectory );
		}

		void OnDirectoryRenamed( object sender, RenamedEventArgs e )
		{
			watcherDirectory = e.Name;
			fileModel.SetNewDirectory( watcherDirectory );
		}

		// Call in background thread
		public void LoadFromWeb( string outputPath )
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
			pythonProcess.StartInfo.Arguments = Environment.CurrentDirectory + "\\PDFLoader\\InvokeLoading.py " + outputPath;
			pythonProcess.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
			pythonProcess.Start();

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


		public void				SetPDFsDirectory( string directory )
		{
			watcherDirectory = directory;
			if ( directoryWatcher == null )
				InitFileSystemWatcher( watcherDirectory );
			else
				ChangeWatcherDirectory( watcherDirectory );
			fileModel.SetNewDirectory( watcherDirectory );
		}

		public FileListModel	GetFileModelContext() { return fileModel; }
	}
}
