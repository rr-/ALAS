using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using AppLocaleLib;
using Ookii.Dialogs.Wpf;

namespace WizardGui
{
	public partial class MainWindow : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private MainWindowData mainWindowData;

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

		public MainWindow()
		{
			InitializeComponent();
			MainWindowData = new MainWindowData();

			ILocaleInfoService localeInfoService = new LocaleInfoService();
			MainWindowData.AvailableLocales = localeInfoService.GetAvailableLocaleNames()
				.Select(localeInfo => new EncapsulatedLocaleInfo(localeInfoService.GetLocaleInfo(localeInfo)))
				.OrderBy(localeInfo => localeInfo.ToString())
				.ToList(); //necessary for getting "SelectedLocale" binding to work

			LoadConfig();

			if (MainWindowData.SelectedLocale == null)
				MainWindowData.SelectedLocale = MainWindowData.AvailableLocales
					.FirstOrDefault(locale => locale.ToString().IndexOf("Japan", StringComparison.InvariantCultureIgnoreCase) >= 0);
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
				var dialog = new VistaSaveFileDialog
				{
					DefaultExt = ".lnk",
					Filter = "Shortcuts (.lnk)|*.lnk",
					FileName = String.Format("{0}-{1}",
						Path.GetFileNameWithoutExtension(MainWindowData.ProgramPath),
						MainWindowData.SelectedLocale.LocaleInfo.LanguageName.ToLower())
				};

				bool? result = dialog.ShowDialog();
				if (result == true)
				{
					string shortcutPath = dialog.FileName;

					// Create empty .lnk file
					System.IO.File.WriteAllBytes(shortcutPath, new byte[0]);

					// Create a Shell32.Folder that points to destination folder
					Type shellAppType = Type.GetTypeFromProgID("Shell.Application");

					var folder = (Shell32.Folder) shellAppType.InvokeMember(
						"NameSpace",
						System.Reflection.BindingFlags.InvokeMethod,
						null,
						Activator.CreateInstance(shellAppType),
						new object[]
						{
							Path.GetDirectoryName(shortcutPath)
						});

					Shell32.FolderItem folderItem = folder.Items().Item(Path.GetFileName(shortcutPath));

					// Finally, create a Shell32.ShellLinkObject
					var shortcut = (Shell32.ShellLinkObject) folderItem.GetLink;

					shortcut.Path = App.EntryPath;
					shortcut.WorkingDirectory = App.EntryDirectory;
					shortcut.SetIconLocation(MainWindowData.ProgramPath, 0);

					shortcut.Description = String.Format("Run \"{0}\" in {1} locale",
						Path.GetFileName(MainWindowData.ProgramPath),
						MainWindowData.SelectedLocale.LocaleInfo.LanguageName);

					shortcut.Arguments = String.Format("--locale {0} --cwd \"{1}\" --path \"{2}\" -- {3}",
						MainWindowData.SelectedLocale.LocaleInfo.LocaleName,
						MainWindowData.ProgramWorkingDirectory,
						MainWindowData.ProgramPath,
						string.Join(" ", ParseArguments(MainWindowData.ProgramArguments).Select(arg => "\"" + arg + "\"")));

					shortcut.Save(shortcutPath);
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
			UpdateConfig();
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

		private void ChooseTargetProgramClick(object sender, RoutedEventArgs eventArgs)
		{
			var dialog = new VistaOpenFileDialog
			{
				DefaultExt = ".exe",
				Filter = "Executable files (.exe)|*.exe"
			};

			bool? result = dialog.ShowDialog();
			if (result == true)
			{
				MainWindowData.ProgramPath = dialog.FileName;
			}
		}

		private void ChooseWorkingDirectoryClick(object sender, RoutedEventArgs eventArgs)
		{
			var dialog = new VistaFolderBrowserDialog();

			bool? result = dialog.ShowDialog(this);
			if (result == true)
			{
				MainWindowData.ProgramWorkingDirectory = dialog.SelectedPath;
			}
		}

		private void LoadConfig()
		{
			if (!((App) Application.Current).IniParser.Root.HasSection("Gui"))
				return;

			var guiSection = ((App)Application.Current).IniParser.Root.GetSection("Gui");
			MainWindowData.ProgramPath = guiSection.GetKey("ProgramPath");
			MainWindowData.ProgramArguments = guiSection.GetKey("ProgramArguments");
			MainWindowData.ProgramWorkingDirectory = guiSection.GetKey("ProgramWorkingDirectory");
			MainWindowData.SelectedLocale = MainWindowData.AvailableLocales
				.FirstOrDefault(locale => locale.ToString() == guiSection.GetKey("SelectedLocale"));
		}

		private void UpdateConfig()
		{
			if (!((App)Application.Current).IniParser.Root.HasSection("Gui"))
				((App)Application.Current).IniParser.Root.AddSection("Gui");

			var guiSection = ((App)Application.Current).IniParser.Root.GetSection("Gui");
			guiSection.SetKey("ProgramPath", MainWindowData.ProgramPath);
			guiSection.SetKey("ProgramArguments", MainWindowData.ProgramArguments);
			guiSection.SetKey("ProgramWorkingDirectory", MainWindowData.ProgramWorkingDirectory);
			guiSection.SetKey("SelectedLocale", MainWindowData.SelectedLocale != null ? MainWindowData.SelectedLocale.ToString() : "");
		}

		static private IEnumerable<string> ParseArguments(string commandLine)
		{
			const string fullRegex = "^(((?<quote>([\"']|))(?<arg>.+?)\\k<quote>)(\\s+|$))+";
			var matches = Regex.Match(commandLine, fullRegex);
			return matches.Groups["arg"].Captures.Cast<Capture>().Select(group => group.Value);
		}
	}
}
