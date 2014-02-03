using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using AppLocaleLib;
using ini_dotnet;

namespace WizardGui
{
	public partial class App
	{
		public IniParser IniParser { get; private set; }
		private bool checkAppLocalePresence = true;

		public static string EntryDirectory
		{
			get { return Path.GetDirectoryName(EntryPath); }
		}

		public static string EntryPath
		{
			get { return Assembly.GetEntryAssembly().Location; }
		}

		private void LoadConfig()
		{
			if (!IniParser.Root.HasSection("Application"))
				return;

			var applicationSection = IniParser.Root.GetSection("Application");
			if (applicationSection.HasKey("CheckAppLocalePresence"))
				checkAppLocalePresence = applicationSection.GetKey<bool>("CheckAppLocalePresence", true);
		}

		private void UpdateConfig()
		{
			if (!IniParser.Root.HasSection("Application"))
				IniParser.Root.AddSection("Application");

			var applicationSection = IniParser.Root.GetSection("Application");
			applicationSection.SetKey("CheckAppLocalePresence", checkAppLocalePresence);
		}

		private void AppStartup(object sender, StartupEventArgs eventArgs)
		{
			string iniFilePath = !string.IsNullOrEmpty(EntryDirectory)
				? Path.Combine(EntryDirectory, Path.GetFileNameWithoutExtension(EntryPath) + "-settings.ini")
				: "settings.ini";
			IniParser = new IniParser(iniFilePath);

			LoadConfig();
			if (checkAppLocalePresence)
				CheckAppLocalePresence();

			ProcessStartupArguments(eventArgs.Args);
		}

		private void CheckAppLocalePresence()
		{
			string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			string fileExpectedToExist = Path.Combine(windowsDirectory, "AppPatch", "AlLayer.dll");
			if (File.Exists(fileExpectedToExist))
				return;

			string message =
				string.Format(
					"Looks like AppLocale is missing. Would you like to exit the program and open browser with " +
					"installation link now?\n\n(You can disable this message in {0}.)",
				Path.GetFileName(IniParser.Path));

			var result = MessageBox.Show(message, null, MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes)
			{
				const string downloadLink = "http://www.microsoft.com/en-us/download/details.aspx?id=13209";
				System.Diagnostics.Process.Start(downloadLink);
				Environment.Exit(1);
			}
		}

		private void ProcessStartupArguments(IEnumerable<string> arguments)
		{
			string programPath = null;
			string programWorkingDirectory = null;
			string localeName = null;
			IList<string> programArguments = new List<string>();

			var args = new Queue<string>(arguments);
			while (args.Count > 0)
			{
				string arg = args.Dequeue();
				if (arg == "-l" || arg == "--locale")
				{
					localeName = args.Dequeue();
				}
				else if (arg == "-p" || arg == "--program" || arg == "--path")
				{
					programPath = args.Dequeue();
				}
				else if (arg == "--cwd")
				{
					programWorkingDirectory = args.Dequeue();
				}
				else if (arg == "--")
				{
					while (args.Count > 0)
					{
						programArguments.Add(args.Dequeue());
					}
				}
			}

			if (!string.IsNullOrEmpty(programPath))
			{
				AppStarter.RunAsync(localeName, programPath, programArguments, programWorkingDirectory);
				Environment.Exit(0);
			}
		}

		private void AppExit(object sender, ExitEventArgs exitEventArgs)
		{
			UpdateConfig();
			IniParser.Save();
		}
	}
}
