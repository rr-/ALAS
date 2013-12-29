using AppLocaleLib;

namespace WizardGui
{
	public class EncapsulatedLocaleInfo
	{
		public LocaleInfo LocaleInfo { get; private set; }

		public EncapsulatedLocaleInfo(LocaleInfo value)
		{
			LocaleInfo = value;
		}

		public override string ToString()
		{
			return string.Format("{0}", LocaleInfo.DisplayEnglishName);
		}
	}
}
