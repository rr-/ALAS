using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AppLocaleLib
{
	public class AppStarter
	{
		public static void RunAsync(
			string localeName,
			string programPath,
			IEnumerable<string> arguments,
			string workingDirectory)
		{
			var localeInfoService = new LocaleInfoService();

			if (string.IsNullOrWhiteSpace(workingDirectory))
				workingDirectory = Path.GetDirectoryName(programPath);

			if (string.IsNullOrWhiteSpace(localeName))
				throw new ArgumentException("No locale selected");

			if (!localeInfoService.HasLocale(localeName))
				throw new ArgumentException(String.Format("Locale \"{0}\" isn't available", localeName));

			if (!File.Exists(programPath))
				throw new FileNotFoundException(String.Format("\"{0}\" not found", programPath));

			uint localeId = localeInfoService.LocaleNameToLocaleId(localeName).Value;
			Environment.SetEnvironmentVariable("__COMPAT_LAYER", "#ApplicationLocale");
			Environment.SetEnvironmentVariable("AppLocaleID", String.Format("{0:x4}", localeId));
			Environment.SetEnvironmentVariable("LANG", localeName);
			var start = new ProcessStartInfo
			{
				Arguments = string.Join(" ", arguments.Select(arg => "\"" + arg + "\"")),
				FileName = programPath,
				WorkingDirectory = workingDirectory ?? string.Empty
			};

			Process.Start(start);
		}
	}
}
