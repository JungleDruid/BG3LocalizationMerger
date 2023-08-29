using System;

namespace BG3LocalizationMerger
{
    public abstract class SingletonBase<T>
        where T : class
    {
        private static readonly Lazy<T> s_instance = new(CreateInstanceOfT);

        public static T Instance => s_instance.Value;

        private static T CreateInstanceOfT()
        {
            return (T)Activator.CreateInstance(typeof(T), true)!;
        }
    }
}
