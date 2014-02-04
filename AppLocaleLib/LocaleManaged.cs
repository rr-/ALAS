using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AppLocaleLib
{
	public class LocaleManaged
	{
		[DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int GetLocaleInfoEx(String lpLocaleName, LCTYPE lcType, StringBuilder lpLCData, int cchData);

		[DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int GetLocaleInfo(uint uiLocale, LCTYPE lcType, StringBuilder lpLCData, int cchData);

		[DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int LCIDToLocaleName(uint uiLocale, StringBuilder lpName, int cchName, int dwFlags);

		[DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern int LocaleNameToLCID(StringBuilder lpName, int dwFlags);

		public delegate bool EnumLocalesProcExDelegate(
			[MarshalAs(UnmanagedType.LPWStr)] String lpLocaleString,
			LOCALETYPE dwFlags,
			int lParam);

		[DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool EnumSystemLocalesEx(
			EnumLocalesProcExDelegate pEnumProcEx,
			LOCALETYPE dwFlags,
			int lParam,
			IntPtr lpReserved);

		public delegate bool EnumLocalesProcDelegate(
			[MarshalAs(UnmanagedType.LPTStr)] String lpLocaleString);

		[DllImport(@"kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool EnumSystemLocales(
			EnumLocalesProcDelegate pEnumProc,
			LCID dwFlags);

		public enum LCID : uint
		{
			/// <summary>
			/// All installed locales. For example, if user hasn't installed East Asian
			/// Language support, locales from that family won't be included in results.
			/// </summary>
			LCID_INSTALLED = 1,

			/// <summary>
			/// All supported locales. For example, if user hasn't installed East Asian
			/// Language support, locales from that family will still be included in results.
			/// </summary>
			LCID_SUPPORTED = 2,
		}

		public enum LOCALETYPE : uint
		{
			/// <summary>
			/// All name-based locales.
			/// </summary>
			LOCALE_ALL = 0x00000000,

			/// <summary>
			/// Shipped locales and/or replacements for them.
			/// </summary>
			LOCALE_WINDOWS = 0x00000001,

			/// <summary>
			/// Supplemental locales only.
			/// </summary>
			LOCALE_SUPPLEMENTAL = 0x00000002,

			/// <summary>
			/// Alternate sort method.
			/// </summary>
			LOCALE_ALTERNATE_SORTS = 0x00000004,

			/// <summary>
			/// Locales that are "neutral" (language only, region data is default).
			/// </summary>
			LOCALE_NEUTRALDATA = 0x00000010,

			/// <summary>
			/// Locales that contain both language and region data.
			/// </summary>
			LOCALE_SPECIFICDATA = 0x00000020,

			/// <summary>
			/// Locale that is used by user that is currently logged in.
			/// </summary>
			LOCALE_USER_DEFAULT = 0x00000400,

			/// <summary>
			/// Locale that is used by the system.
			/// </summary>
			LOCALE_SYSTEM_DEFAULT = 0x00000800,
		}

		public enum LCTYPE : uint
		{
			/// <summary>
			/// Display name (language + country usually) in English, eg "German (Germany)" (Windows 7+)
			/// </summary>
			LOCALE_SENGLISHDISPLAYNAME = 0x00000072,

			/// <summary>
			/// English name of language, eg "German" (Windows 7+)
			/// </summary>
			LOCALE_SENGLISHLANGUAGENAME = 0x00001001,

			/// <summary>
			/// English name of country, eg "Germany" (Windows 7+)
			/// </summary>
			LOCALE_SENGLISHCOUNTRYNAME = 0x00001002,

			/// <summary>
			/// Language code, eg "0401"
			/// </summary>
			LOCALE_ILANGUAGE = 0x00000001,

			/// <summary>
			/// Language name, eg "German"
			/// </summary>
			LOCALE_SLANGUAGE = 0x00000002,

			/// <summary>
			/// Country code
			/// </summary>
			LOCALE_ICOUNTRY = 0x00000005,

			/// <summary>
			/// Country name, eg "Germany"
			/// </summary>
			LOCALE_SCOUNTRY = 0x00000006,

			/// <summary>
			/// Geographical location ID
			/// </summary>
			LOCALE_IGEOID = 0x0000005B,

			/// <summary>
			/// Language name abbreviation compatible with ISO-639
			/// </summary>
			LOCALE_SISO639LANGNAME = 0x00000059,

			/// <summary>
			/// Language country abbreviation compatible with ISO-3166
			/// </summary>
			LOCALE_SISO3166CTRYNAME = 0x0000005A,

			/// <summary>
			/// Locale name (with sort info), eg de-DE_phoenb
			/// </summary>
			LOCALE_SNAME = 0x0000005c,

			/// <summary>
			/// Fallback name for within the console
			/// </summary>
			LOCALE_SCONSOLEFALLBACKNAME = 0x0000006e,
		}
	}
}
