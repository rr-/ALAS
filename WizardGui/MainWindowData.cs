using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WizardGui
{
	public class MainWindowData : INotifyPropertyChanged
	{
		#region fields
		private string programPath;
		private string programArguments;
		private string programWorkingDirectory;
		private EncapsulatedLocaleInfo selectedLocale;
		private IEnumerable<EncapsulatedLocaleInfo> availableLocales;
		#endregion

		#region properties
		public IEnumerable<EncapsulatedLocaleInfo> AvailableLocales
		{
			get { return availableLocales; }
			set
			{
				availableLocales = value;
				OnPropertyChanged("AvailableLocales");
			}
		}

		public EncapsulatedLocaleInfo SelectedLocale
		{
			get { return selectedLocale; }
			set
			{
				selectedLocale = value;
				OnPropertyChanged("SelectedLocale");
			}
		}

		public String ProgramPath
		{
			get { return programPath; }
			set
			{
				programPath = value;
				OnPropertyChanged("ProgramPath");
			}
		}

		public String ProgramArguments
		{
			get { return programArguments; }
			set
			{
				programArguments = value;
				OnPropertyChanged("ProgramArguments");
			}
		}

		public String ProgramWorkingDirectory
		{
			get { return programWorkingDirectory; }
			set
			{
				programWorkingDirectory = value;
				OnPropertyChanged("ProgramWorkingDirectory");
			}
		}
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName = null)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}