using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using AppLocaleLib;
using ini_dotnet;
using IWshRuntimeLibrary;

namespace WizardGui
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private MainWindowData mainWindowData;
		private readonly IniParser iniParser;

		public MainWindowData MainWindowData
		{
			get { return mainWindowData; }
			set
			{
				mainWindowData = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("MainWindowData"));
			}
		}

		private string EntryDirectory
		{
			get
			{
				return Path.GetDirectoryName(EntryPath);
			}
		}

		private string EntryPath
		{
			get
			{
				return System.Reflection.Assembly.GetEntryAssembly().Location;
			}
		}

		public MainWindow()
		{
			InitializeComponent();
			MainWindowData = new MainWindowData();

			ILocaleInfoService localeInfoService = new LocaleInfoService();
			MainWindowData.AvailableLocales = localeInfoService.GetAvailableLocaleNames()
				.Select(localeInfo => new EncapsulatedLocaleInfo(localeInfoService.GetLocaleInfo(localeInfo)))
				.OrderBy(localeInfo => localeInfo.ToString())
				.ToList(); //necessary for getting "SelectedLocale" binding to work

			string iniFilePath = !string.IsNullOrEmpty(EntryDirectory)
				? Path.Combine(EntryDirectory, "settings.ini")
				: "settings.ini";
			iniParser = new IniParser(iniFilePath);

			if (iniParser.Root.HasSection("Gui"))
			{
				var iniSection = iniParser.Root["Gui"];
				MainWindowData.ProgramPath = iniSection.TryGetString("ProgramPath");
				MainWindowData.ProgramArguments = iniSection.TryGetString("ProgramArguments");
				MainWindowData.ProgramWorkingDirectory = iniSection.TryGetString("ProgramWorkingDirectory");
				MainWindowData.SelectedLocale = MainWindowData.AvailableLocales
					.FirstOrDefault(locale => locale.ToString() == iniSection.TryGetString("SelectedLocale"));
			}

			if (MainWindowData.SelectedLocale == null)
				MainWindowData.SelectedLocale = MainWindowData.AvailableLocales
					.FirstOrDefault(locale => locale.ToString().IndexOf("Japan", StringComparison.InvariantCultureIgnoreCase) > 0);
		}

		private void LaunchButtonClick(object sender, RoutedEventArgs eventArgs)
		{
			try
			{
				Validate();

				AppStarter.RunAsync(
					MainWindowData.SelectedLocale.LocaleInfo.LocaleName,
					MainWindowData.ProgramPath,
					ParseArguments(MainWindowData.ProgramArguments),
					MainWindowData.ProgramWorkingDirectory);
			}
			catch (Exception exception)
			{
				MessageBox.Show("Error: " + exception.Message);
			}
		}

		private void MakeShortcutButtonClick(object sender, RoutedEventArgs eventArgs)
		{
			try
			{
				var saveDialog = new Microsoft.Win32.SaveFileDialog
				{
					DefaultExt = ".lnk",
					Filter = "Shortcuts (.lnk)|*.lnk",
					FileName = String.Format("{0}-{1}",
						Path.GetFileNameWithoutExtension(mainWindowData.ProgramPath),
						mainWindowData.SelectedLocale.LocaleInfo.LanguageEnglishName.ToLower())
				};

				bool? result = saveDialog.ShowDialog();
				if (result == true)
				{
					IWshShortcut shortcut = (new WshShell()).CreateShortcut(saveDialog.FileName);

					shortcut.TargetPath = EntryPath;
					shortcut.WorkingDirectory = EntryDirectory;
					shortcut.IconLocation = MainWindowData.ProgramPath + ",0";

					shortcut.Description = String.Format("Run \"{0}\" in {1} locale",
						Path.GetFileName(mainWindowData.ProgramPath),
						mainWindowData.SelectedLocale.LocaleInfo.LanguageEnglishName);

					shortcut.Arguments = String.Format("--locale {0} --cwd \"{1}\" --path \"{2}\" -- {3}",
						mainWindowData.SelectedLocale.LocaleInfo.LocaleName,
						mainWindowData.ProgramWorkingDirectory,
						mainWindowData.ProgramPath,
						string.Join(" ", ParseArguments(mainWindowData.ProgramArguments).Select(arg => "\"" + arg + "\"")));

					shortcut.Save();
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show("Error: " + exception.Message);
			}
		}

		private void Validate()
		{
			if (String.IsNullOrEmpty(MainWindowData.ProgramPath))
				throw new ArgumentException("Program path is empty");

			if (!System.IO.File.Exists(MainWindowData.ProgramPath))
				throw new FileNotFoundException("Program not found");
		}

		private void WindowDragEnter(object sender, DragEventArgs eventArgs)
		{
			if (eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
				eventArgs.Effects = DragDropEffects.Move;
		}

		private void WindowDrop(object sender, DragEventArgs eventArgs)
		{
			var files = (string[])eventArgs.Data.GetData(DataFormats.FileDrop);
			foreach (string file in files) Console.WriteLine(file);
			MainWindowData.ProgramPath = files[0];
		}

		private void WindowClosed(object sender, EventArgs e)
		{
			if (!iniParser.Root.HasSection("Gui"))
				iniParser.Root.AddSection("Gui");
			var iniSection = iniParser.Root["Gui"];
			iniSection.SetString("ProgramPath", MainWindowData.ProgramPath);
			iniSection.SetString("ProgramArguments", MainWindowData.ProgramArguments);
			iniSection.SetString("ProgramWorkingDirectory", MainWindowData.ProgramWorkingDirectory);
			iniSection.SetString("SelectedLocale", MainWindowData.SelectedLocale != null ? MainWindowData.SelectedLocale.ToString() : "");
			iniParser.Save();
		}

		private void TextBoxPreviewDragEnterOrOver(object sender, DragEventArgs eventArgs)
		{
			//allow drop events on textboxes as well
			if (eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
			{
				eventArgs.Effects = DragDropEffects.Move;
				eventArgs.Handled = true; //without this, WPF denies handling drop event
			}
		}

		static private IEnumerable<string> ParseArguments(string commandLine)
		{
			const string fullRegex = "^(((?<quote>([\"']|))(?<arg>.+?)\\k<quote>)(\\s+|$))+";
			var matches = Regex.Match(commandLine, fullRegex);
			return matches.Groups["arg"].Captures.Cast<Capture>().Select(group => group.Value);
		}
	}
}
