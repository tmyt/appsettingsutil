using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ApplicationSettingsUtility
{
    /// <summary>
    /// Roaming/Local Settings wrapper for Windows Runtime Applications.
    /// </summary>
    /// <example>
    /// public class AppConfig : AppConfigBase&lt;AppConfig&gt;
    /// {
    ///     public string YourRoamingValue { get { return GetValue&lt;string&gt;(); } set { SetValue(value); } }
    ///     [LocalValue]
    ///     public string YourLocalValue { get { return GetValue&lt;string&gt;(); } set { SetValue(value); } }
    ///     public string WithDefaultValue { get { return GetValue("default value"); } set { SetValue(value); } }
    /// }
    /// </example>
    /// <typeparam name="TImpl">Delived class type</typeparam>
    public class AppConfigBase<TImpl> : INotifyPropertyChanged where TImpl : class, new()
    {
        private readonly IPropertySet _localValues;
        private readonly IPropertySet _roamingValues;

        private readonly Dictionary<string, bool> _attributeCache;

        #region AppConfig Logic
        private IPropertySet GetValues(bool isLocal)
        {
            return isLocal ? _localValues : _roamingValues;
        }

        private T GetLocalValue<T>(string name, T defaultValue, bool fallbackToRoamingSettings)
        {
            try
            {
                var values = _localValues;
                if (values.ContainsKey(name)) return (T)values[name];
                if (fallbackToRoamingSettings)
                {
                    values = _roamingValues;
                    if (values.ContainsKey(name)) return (T)values[name];
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private T GetRoamingValue<T>(string name, T defaultValue)
        {
            try
            {
                var values = _roamingValues;
                if (values.ContainsKey(name)) return (T)values[name];
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private void SetValue<T>(string name, T value, bool isLocal)
        {
            try
            {
                var values = GetValues(isLocal);
                if (values.ContainsKey(name))
                {
                    values[name] = value;
                }
                else
                {
                    values.Add(name, value);
                }
                OnPropertyChanged(name);
            }
            catch
            {
                // ignore
            }
        }

        private bool IsLocalValue(string name)
        {
            if (_attributeCache.ContainsKey(name))
            {
                // Use cached value
                return _attributeCache[name];
            }
            var prop = GetType().GetRuntimeProperty(name);
            var attr = prop.GetCustomAttribute(typeof(LocalValueAttribute)) != null;
            _attributeCache.Add(name, attr);
            return attr;
        }

        private T GetValueProxy<T>(string name, T defaultValue)
        {
            var isLocal = IsLocalValue(name);
            return RoamingEnabled && !isLocal ? GetRoamingValue(name, defaultValue) : GetLocalValue(name, defaultValue, !RoamingEnabled);
        }

        private void SetValueProxy<T>(string name, T value)
        {
            var isLocal = IsLocalValue(name);
            SetValue(name, value, isLocal);
        }

        #endregion

        #region Protected APIs
        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            return GetValueProxy(name, default(T));
        }

        protected T GetValue<T>(T defaultValue, [CallerMemberName] string name = null)
        {
            return GetValueProxy(name, defaultValue);
        }

        protected void SetValue<T>(T value, [CallerMemberName] string name = null)
        {
            SetValueProxy(name, value);
        }
        #endregion

        protected AppConfigBase()
        {
            var config = ApplicationData.Current;
            _localValues = config.LocalSettings.Values;
            _roamingValues = config.RoamingSettings.Values;
            _attributeCache = new Dictionary<string, bool>();
        }

        private static TImpl _instance;
        public static TImpl Instance => _instance ?? (_instance = new TImpl());

        public bool RoamingEnabled { get { return GetLocalValue("RoamingEnabled", true, false); } set { SetValue("RoamingEnabled", value, true); } }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class LocalValueAttribute : Attribute { }

}
