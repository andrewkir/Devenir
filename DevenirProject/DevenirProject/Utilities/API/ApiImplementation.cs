using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json.Linq;
using Org.Apache.Http;
using Org.Json;
using Refit;
using static Android.Provider.Settings;

namespace DevenirProject.Utilities.API
{
    public class ApiImplementation
    {
        const string PATH = "https://devenir.andrewkir.ru";

        public delegate void RequestResult(bool result, ApiResponse<string> response, string error);
        event RequestResult RequestResultEvent;

        string deviceId;

        public async void LatexRecogniseRequestAsync(JObject body, Activity activity)
        {
            try
            {
                deviceId = Secure.GetString(activity.ContentResolver, Secure.AndroidId);

                ApiResponse<string> response = null;
                var api = RestService.For<APIService>(PATH);
                string access_token = SharedPrefsManager.GetTokens().access_token;
                if (access_token != null && access_token != "")
                {
                    response = await api.GetImageProcess(body.ToString(), "Bearer " + access_token, deviceId);
                    if (response == null)
                    {
                        RequestResultEvent?.Invoke(false, null, activity.GetString(Resource.String.noInternetException));
                        return;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                    {
                        Toast.MakeText(Application.Context, "Обновление токенов", ToastLength.Short).Show();
                        await RefreshTokenAsync(activity);
                    }
                    else
                    {
                        RequestResultEvent?.Invoke(true, response, null); 
                        return;
                    }
                }
                else
                {
                    await RefreshTokenAsync(activity);
                }
            }
            catch (HttpRequestException)
            {
                RequestResultEvent?.Invoke(false, null, activity.GetString(Resource.String.latexNoConnectionException));
                return;
            }
            catch (Exception ex)
            {
                RequestResultEvent?.Invoke(false, null, activity.GetString(Resource.String.unexpectedException));
                return;
            }
        }

        private async System.Threading.Tasks.Task RefreshTokenAsync(Activity activity)
        {
            try
            {
                var api = RestService.For<APIService>(PATH);
                string refresh_token = SharedPrefsManager.GetTokens().refresh_token;
                bool success = false;
                if (refresh_token != null && refresh_token != "")
                {
                    var response = await api.RefreshToken("Bearer " + refresh_token, deviceId);
                    if (response == null)
                    {
                        RequestResultEvent?.Invoke(false, null, activity.GetString(Resource.String.noInternetException));
                        return;
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        JSONObject resp = new JSONObject(response.Content);
                        try
                        {
                            string access_token = resp.GetString("access_token");
                            string refresh_token_new = resp.GetString("refresh_token");
                            SharedPrefsManager.SaveTokens(access_token, null);
                            SharedPrefsManager.SaveTokens(refresh_token_new, null);
                            success = true;
                            Toast.MakeText(Application.Context, "Успешное обновление access_token и refresh_token", ToastLength.Short).Show();
                        }
                        catch (JSONException)
                        {
                            RequestResultEvent?.Invoke(false, null, activity.GetString(Resource.String.latexServerException));
                            return;
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                    {
                        Toast.MakeText(Application.Context, "Переаттестация! Истёк refresh token", ToastLength.Short).Show();
                        var attest = new ApiAttestation();
                        attest.AddOnResultListener(delegate (bool res, string exception)
                        {
                            if (res) RequestResultEvent?.Invoke(res, null, null);
                            else RequestResultEvent?.Invoke(res, null, exception);
                        });
                        await attest.AttestateAsync(activity);
                    }
                }
                else
                {
                    var attest = new ApiAttestation();
                    attest.AddOnResultListener(delegate (bool res, string exception)
                    {
                        if (res) RequestResultEvent?.Invoke(res, null, null);
                        else RequestResultEvent?.Invoke(res, null, exception);
                    });
                    await attest.AttestateAsync(activity);
                }
            }
            catch (HttpRequestException)
            {
                RequestResultEvent?.Invoke(false, null, activity.GetString(Resource.String.latexNoConnectionException));
            }
            catch (Exception ex)
            {
                RequestResultEvent?.Invoke(false, null, activity.GetString(Resource.String.latexNoConnectionException));
            }
        }

        public void AddOnRequestResultListener(RequestResult requestResult)
        {
            RequestResultEvent += requestResult;
        }
    }
}