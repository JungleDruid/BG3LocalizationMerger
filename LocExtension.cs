using System.Windows.Data;

namespace BG3LocalizationMerger
{
    public class LocExtension : Binding
    {
        public LocExtension(string name)
            : base($"[{name}]")
        {
            Mode = BindingMode.OneWay;
            Source = TranslationSource.Instance;
        }
    }
}
