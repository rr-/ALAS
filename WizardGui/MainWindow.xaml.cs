using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using AppLocaleLib;
using ini_dotnet;

namespace WizardGui
{
	public partial class MainWindow : INotifyPropertyChanged
	{
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

		public MainWindow()
		{
			InitializeComponent();
			MainWindowData = new MainWindowData();

			ILocaleInfoService localeInfoService = new LocaleInfoService();
			MainWindowData.AvailableLocales = localeInfoService.GetAvailableLocaleNames()
				.Select(localeInfo => new EncapsulatedLocaleInfo(localeInfoService.GetLocaleInfo(localeInfo)))
				.OrderBy(localeInfo => localeInfo.ToString())
				.ToList(); //necessary for getting "SelectedLocale" binding to work

			string entryDirectoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			string iniFilePath = !string.IsNullOrEmpty(entryDirectoryName)
				? Path.Combine(entryDirectoryName, "settings.ini")
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
		}

		private void LaunchButtonClick(object sender, RoutedEventArgs eventArgs)
		{
			try
			{
				Validate();
				throw new NotImplementedException();
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
				Validate();
				throw new NotImplementedException();
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

			if (MainWindowData.SelectedLocale == null)
				throw new ArgumentException("No locale selected");
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

		public event PropertyChangedEventHandler PropertyChanged;

		private void TextBoxPreviewDragEnterOrOver(object sender, DragEventArgs eventArgs)
		{
			//allow drop events on textboxes as well
			if (eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
			{
				eventArgs.Effects = DragDropEffects.Move;
				eventArgs.Handled = true; //without this, WPF denies handling drop event
			}
		}
	}
}
