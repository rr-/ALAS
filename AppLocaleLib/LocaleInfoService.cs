using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AppLocaleLib
{
	public class LocaleInfoService : ILocaleInfoService
	{
		private readonly bool useOldApi;

		public string GetSystemLocaleName()
		{
			return LocaleIdToLocaleName((uint) LocaleManaged.LOCALETYPE.LOCALE_SYSTEM_DEFAULT);
		}

		public string GetUserLocaleName()
		{
			return LocaleIdToLocaleName((uint) LocaleManaged.LOCALETYPE.LOCALE_USER_DEFAULT);
		}

		public LocaleInfoService()
		{
			try
			{
				var lpcData = new StringBuilder(256);
				LocaleManaged.GetLocaleInfoEx(GetSystemLocaleName(), LocaleManaged.LCTYPE.LOCALE_SENGLISHLANGUAGENAME, lpcData, lpcData.Capacity);
			}
			catch (EntryPointNotFoundException)
			{
				useOldApi = true;
			}
		}

		public IEnumerable<string> GetAvailableLocaleNames()
		{
			IList<string> availableLocaleNames = new List<string>();

			bool success = false;
			if (!useOldApi)
			{
				var enumCallback = new LocaleManaged.EnumLocalesProcExDelegate(
					(localeName, flags, param) =>
					{
						availableLocaleNames.Add(localeName);
						return true;
					});

				success = LocaleManaged.EnumSystemLocalesEx(enumCallback, LocaleManaged.LOCALETYPE.LOCALE_SPECIFICDATA, 0, (IntPtr)0);
			}
			else
			{
				var enumCallback = new LocaleManaged.EnumLocalesProcDelegate(
					otherLocaleIdString =>
					{
						var otherLocaleId = ConvertOldLocaleId(otherLocaleIdString).Value;
						availableLocaleNames.Add(LocaleIdToLocaleName(otherLocaleId));
						return true;
					});

				success = LocaleManaged.EnumSystemLocales(enumCallback, LocaleManaged.LCID.LCID_SUPPORTED);
			}

			if (!success)
				throw new InvalidOperationException("Getting system locales failed.");

			return availableLocaleNames;
		}

		public bool HasLocale(string localeName)
		{
			var lpcData = new StringBuilder(256);
			return !useOldApi
				? LocaleManaged.GetLocaleInfoEx(localeName, LocaleManaged.LCTYPE.LOCALE_ICOUNTRY, lpcData, lpcData.Capacity) > 0
				: LocaleManaged.GetLocaleInfo(LocaleNameToLocaleId(localeName).Value, LocaleManaged.LCTYPE.LOCALE_ICOUNTRY, lpcData, lpcData.Capacity) > 0;
		}

		public string LocaleIdToLocaleName(uint localeId)
		{
			var lpcData = new StringBuilder(256);
			var ret = !useOldApi
				? LocaleManaged.LCIDToLocaleName(localeId, lpcData, lpcData.Capacity, 0)
				: LocaleManaged.GetLocaleInfo(localeId, LocaleManaged.LCTYPE.LOCALE_ILANGUAGE, lpcData, lpcData.Capacity);
			if (ret <= 0)
				return null;

			return lpcData.ToString().Substring(0, ret - 1);
		}

		public uint? LocaleNameToLocaleId(string localeName)
		{
			if (!useOldApi)
			{
				var lpcData = new StringBuilder(256);
				lpcData.Append(localeName);
				int localeId = LocaleManaged.LocaleNameToLCID(lpcData, 0);
				if (localeId == 0)
					return null;
				return (uint)localeId;
			}
			else
			{
				uint? foundLocaleId = null;

				var enumCallback = new LocaleManaged.EnumLocalesProcDelegate(
					otherLocaleIdString =>
					{
						var otherLocaleId = ConvertOldLocaleId(otherLocaleIdString).Value;
						if (LocaleIdToLocaleName(otherLocaleId) == localeName)
						{
							foundLocaleId = otherLocaleId;
							return false;
						}
						return true;
					});

				if (!LocaleManaged.EnumSystemLocales(enumCallback, LocaleManaged.LCID.LCID_SUPPORTED))
					throw new InvalidOperationException(String.Format("Getting locale ID for \"{0}\" failed.", localeName));

				return foundLocaleId;
			}
		}

		public LocaleInfo GetLocaleInfo(string localeName)
		{
			try
			{
				return new LocaleInfo
				{
					LocaleName = localeName,
					DisplayName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SENGLISHDISPLAYNAME),
					LanguageName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SENGLISHLANGUAGENAME),
					CountryName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SENGLISHCOUNTRYNAME),
				};
			}
			catch (InvalidOperationException)
			{
				/* Error in retrieval can mean that LOCALE_SENGLISH* constants are not supported (= user is on
				 * system earlier than Windows 7). useOldMode is deliberately not used in this case, because it
				 * only tells whether to use Ex() family, not what constants are supported.*/
				return new LocaleInfo
				{
					LocaleName = localeName,
					DisplayName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SLANGUAGE),
					LanguageName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SLANGUAGE),
					CountryName = GetLocaleInfo(localeName, LocaleManaged.LCTYPE.LOCALE_SCOUNTRY),
				};
			}
		}

		private string GetLocaleInfo(string localeName, LocaleManaged.LCTYPE infoTypeId)
		{
			var lpcData = new StringBuilder(256);
			var ret = !useOldApi
				? LocaleManaged.GetLocaleInfoEx(localeName, infoTypeId, lpcData, lpcData.Capacity)
				: LocaleManaged.GetLocaleInfo(LocaleNameToLocaleId(localeName).Value, infoTypeId, lpcData, lpcData.Capacity);
			if (ret <= 0)
				throw new InvalidOperationException(String.Format("Getting locale information for \"{0}\" failed.", localeName));

			var lpcDataString = lpcData.ToString();

			if (ret - 1 < lpcDataString.Length)
				return lpcDataString.Substring(0, ret - 1);
			return lpcDataString;
		}

		private uint? ConvertOldLocaleId(string localeIdString)
		{
			uint localeId;

			bool success = uint.TryParse(
				localeIdString,
				NumberStyles.HexNumber,
				CultureInfo.InvariantCulture,
				out localeId);

			return success ? (uint?)localeId : null;
		}
	}
}
