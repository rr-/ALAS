using System.Collections.Generic;

namespace AppLocaleLib
{
	public interface ILocaleInfoService
	{
		string GetSystemLocaleName();
		string GetUserLocaleName();
		IEnumerable<string> GetAvailableLocaleNames();
		bool HasLocale(string localeName);
		LocaleInfo GetLocaleInfo(string localeName);
		string LocaleIdToLocaleName(uint localeId);
		uint? LocaleNameToLocaleId(string localeName);
	}
}
