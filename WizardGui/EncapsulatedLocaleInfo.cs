using AppLocaleLib;

namespace WizardGui
{
	public class EncapsulatedLocaleInfo
	{
		private readonly LocaleInfo localeInfo;

		public EncapsulatedLocaleInfo(LocaleInfo value)
		{
			localeInfo = value;
		}

		public override string ToString()
		{
			return string.Format("{0}", localeInfo.DisplayEnglishName);
		}
	}
}