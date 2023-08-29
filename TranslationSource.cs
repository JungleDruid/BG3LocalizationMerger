using BG3LocalizationMerger.Resources.Strings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace BG3LocalizationMerger
{
    public class TranslationSource : SingletonBase<TranslationSource>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly ResourceManager _resManager = Strings.ResourceManager;

        public string this[string key] => _resManager.GetString(key, Culture) ?? $"{{{key}}}";

        public CultureInfo Culture
        {
            get { return Strings.Culture; }
            set
            {
                if (Strings.Culture != value)
                {
                    Strings.Culture = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        public static IEnumerable<CultureInfo> GetAllCultures()
        {
            var rm = Strings.ResourceManager;

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            foreach (CultureInfo culture in cultures)
            {
                if (culture.Equals(CultureInfo.InvariantCulture))
                    continue;
                ResourceSet? rs = rm.GetResourceSet(culture, true, false);
                if (rs != null)
                {
                    yield return culture;
                }
            }
        }
    }
}
