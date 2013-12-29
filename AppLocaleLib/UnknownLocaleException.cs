using System;

namespace AppLocaleLib
{
	public class UnknownLocaleException : ArgumentException
	{
		public UnknownLocaleException(uint localeId)
			: base(String.Format("Unknown locale ID: {0}", localeId))
		{
		}

		public UnknownLocaleException(string localeName)
			: base(String.Format("Unknown locale name: {0}", localeName))
		{
		}
	}
}