namespace Hub.Shared.Interfaces
{
    public interface IUserSettingManager
    {
        void SaveSetting(string key, string value);

        string GetSetting(string key);
    }
}
