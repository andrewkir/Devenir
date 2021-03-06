﻿
using Android.App;
using Android.Content;
using Android.Preferences;

namespace DevenirProject.Utilities.Helpers
{
    static class SharedPrefsManager
    {
        static public void SaveTokens(string access_token, string refresh_token)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            if(access_token != null) editor.PutString("access_token", access_token);
            if (refresh_token != null) editor.PutString("refresh_token", refresh_token);
            editor.Apply();
        }
        static public (string access_token, string refresh_token) GetTokens()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            string access_token = prefs.GetString("access_token", "");
            string refresh_token = prefs.GetString("refresh_token", "");

            return (access_token, refresh_token);
        }
    }
}