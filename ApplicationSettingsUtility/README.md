ApplicationSettingsUtility
===

Summary
--

Wrapper library for ApplicationData.Current.RoamingSettings & LocalSettings.

Usage
--

```cs
public class AppConfig : AppConfigBase<AppConfig>
{
    [LocalValue]
    public string StringValue { get { return GetValue<string>(); } set { SetValue(value); } }

    public string RoamingStringValue { get { return GetValue<string>(); } set { SetValue(value); } }
}
```

