using System;
using System.Collections.Generic;
using System.Windows;
using AppLocaleLib;

namespace WizardGui
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		private void AppStartup(object sender, StartupEventArgs eventArgs)
		{
			string programPath = null;
			string programWorkingDirectory = null;
			string localeName = null;
			IList<string> programArguments = new List<string>();

			var args = new Queue<string>(eventArgs.Args);
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
	}
}
