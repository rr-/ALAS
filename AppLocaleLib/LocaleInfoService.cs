using System;
using System.Collections.Generic;

namespace AppLocaleLib
{
	public class LocaleInfoService : ILocaleInfoService
	{
		public string GetSystemLocaleName()
		{
			return LocaleIdToLocaleName(LocaleManaged.LOCALE_SYSTEM_DEFAULT);
		}

		public string GetUserLocaleName()
		{
			return LocaleIdToLocaleName(LocaleManaged.LOCALE_USER_DEFAULT);
		}

		public IEnumerable<string> GetAvailableLocaleNames()
		{
			IList<string> availableLocaleNames = new List<string>();
			var enumCallback = new LocaleManaged.EnumLocalesProcExDelegate(
				(localeName, flags, param) =>
				{
					availableLocaleNames.Add(localeName);
					return true;
				});

			if (!LocaleManaged.EnumSystemLocalesEx(enumCallback, LocaleManaged.LOCALETYPE.LOCALE_SPECIFICDATA, 0, (IntPtr)0))
				throw new InvalidOperationException();

			return availableLocaleNames;
		}

		public bool HasLocale(string localeName)
		{
			var lpcData = new System.Text.StringBuilder(256);
			return LocaleManaged.GetLocaleInfoEx(localeName, LocaleManaged.LCTYPE.LOCALE_ICOUNTRY, lpcData, lpcData.Capacity) > 0;
		}

		public LocaleInfo GetLocaleInfo(uint localeId)
		{
			string localeName = LocaleIdToLocaleName(localeId);
			return GetLocaleInfo(localeName);
		}

		public LocaleInfo GetLocaleInfo(string localeName)
		{
			return new LocaleInfo
			{
				LocaleName = localeName,
				DisplayLocalizedName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SNATIVEDISPLAYNAME),
				DisplayEnglishName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SENGLISHDISPLAYNAME),
				DisplayNativeName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SNATIVEDISPLAYNAME),
				LanguageLocalizedName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SLOCALIZEDLANGUAGENAME),
				LanguageEnglishName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SENGLISHLANGUAGENAME),
				LanguageNativeName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SNATIVELANGUAGENAME),
				CountrySymbol = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_ICOUNTRY),
				CountryLocalizedName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SLOCALIZEDCOUNTRYNAME),
				CountryEnglishName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SENGLISHCOUNTRYNAME),
				CountryNativeName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SNATIVECOUNTRYNAME),
			};
		}

		private string LocaleIdToLocaleName(uint localeId)
		{
			var lpcData = new System.Text.StringBuilder(256);
			var ret = LocaleManaged.LCIDToLocaleName(localeId, lpcData, lpcData.Capacity, 0);
			if (ret <= 0)
				return null;

			return lpcData.ToString().Substring(0, ret - 1);
		}

		private string GetLocaleInfo(string localeName, LocaleManaged.LCTYPE infoTypeId)
		{
			if (!HasLocale(localeName))
				throw new UnknownLocaleException(localeName);

			var lpcData = new System.Text.StringBuilder(256);
			var ret = LocaleManaged.GetLocaleInfoEx(localeName, infoTypeId, lpcData, lpcData.Capacity);
			if (ret <= 0)
				throw new InvalidOperationException();

			return lpcData.ToString().Substring(0, ret - 1);
		}
	}

	public class LocaleInfo
	{
		public string LocaleName { get; set; }
		public string LanguageLocalizedName { get; set; }
		public string LanguageEnglishName { get; set; }
		public string LanguageNativeName { get; set; }
		public string CountrySymbol { get; set; }
		public string CountryLocalizedName { get; set; }
		public string CountryEnglishName { get; set; }
		public string CountryNativeName { get; set; }
		public string DisplayLocalizedName { get; set; }
		public string DisplayNativeName { get; set; }
		public string DisplayEnglishName { get; set; }
	}
}