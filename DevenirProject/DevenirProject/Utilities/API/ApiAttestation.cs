using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Gms.Common;
using Android.Gms.SafetyNet;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Json;
using Refit;
using static Android.Provider.Settings;

namespace DevenirProject.Utilities.API
{
    class ApiAttestation
    {

        string timestamp;
        string device_id;

        public delegate void AttestationResult(bool res, string exception);
        event AttestationResult AttestationResultEvent;

        public async System.Threading.Tasks.Task AttestateAsync(Activity activity)
        {
            try
            {
                var api = RestService.For<APIService>("https://devenir.andrewkir.ru");
                JSONObject obj = new JSONObject();

                timestamp = DateTime.Now.Ticks.ToString();
                device_id = Secure.GetString(activity.ContentResolver, Secure.AndroidId);

                obj.Put("timestamp", timestamp);
                obj.Put("device_id", device_id);


                var response = await api.GetNonce(obj.ToString());
                if (response == null)
                {
                    AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.noInternetException));
                    return;
                }


                if (response.IsSuccessStatusCode)
                {
                    string nonce = response.Content;

                    if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Application.Context) == ConnectionResult.Success)
                    {
                        // The SafetyNet Attestation API is available.
                        SafetyNetClient client = SafetyNetClass.GetClient(activity);
                        var task = client.Attest(Encoding.ASCII.GetBytes(nonce), activity.Resources.GetString(Resource.String.api_safetyNetKey))
                                         .AddOnSuccessListener(activity, new OnSuccessListener(activity, timestamp, this))
                                         .AddOnFailureListener(activity, new OnFailureListener(activity, this));
                    }
                    else
                    {
                        // Prompt user to update Google Play services
                        AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.attestationGooglePlayException));
                    }
                    return;
                }
                else
                {
                    AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.serverException));
                    return;
                }
            }
            catch (HttpRequestException)
            {
                AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.serverException));
            }
            catch (Exception ex)
            {
                AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.unexpectedException));
            }
        }

        public void AddOnResultListener(AttestationResult result)
        {
            if (result != null) AttestationResultEvent += result;
        }

        private async System.Threading.Tasks.Task CheckResultsAsync(string res, string timestamp, Activity activity)
        {
            try
            {
                var api = RestService.For<APIService>("https://devenir.andrewkir.ru");
                JSONObject obj = new JSONObject();
                obj.Put("safetynet", res);
                obj.Put("timestamp", timestamp);
                obj.Put("device_id", Secure.GetString(activity.ContentResolver, Secure.AndroidId));

                var response = await api.CreateAccessToken(obj.ToString());
                if (response == null)
                {
                    AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.noInternetException));
                    return;
                }
                if (response.IsSuccessStatusCode)
                {
                    JSONObject resp = new JSONObject(response.Content);
                    try
                    {
                        string access_token = resp.GetString("access_token");
                        string refrest_token = resp.GetString("refresh_token");
                        SharedPrefsManager.SaveTokens(access_token, refrest_token);
                        AttestationResultEvent?.Invoke(true, null);
                        Toast.MakeText(Application.Context, "Attestated", ToastLength.Short).Show();
                    }
                    catch (JSONException)
                    {
                        AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.serverException));
                    }
                }
                else
                {
                    AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.serverException));
                }
            }
            catch (HttpRequestException)
            {
                AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.serviceNoConnectionException));
            }
            catch (Exception ex)
            {
                AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.unexpectedException));
            }
        }


        public class OnFailureListener : Java.Lang.Object, IOnFailureListener
        {
            Activity activity;
            ApiAttestation sender;
            public OnFailureListener(Activity activity, ApiAttestation sender)
            {
                this.activity = activity;
                this.sender = sender;
            }

            public void OnFailure(Java.Lang.Exception e)
            {
                sender.AttestationResultEvent?.Invoke(false, activity.GetString(Resource.String.unexpectedException));
            }
        }

        public class OnSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            Activity activity;
            string timestamp;
            ApiAttestation sender;

            public OnSuccessListener(Activity activity, string timestamp, ApiAttestation sender)
            {
                this.activity = activity;
                this.timestamp = timestamp;
                this.sender = sender;
            }

            public void OnSuccess(Java.Lang.Object result)
            {
                var res = (result as SafetyNetApiAttestationResponse).JwsResult.ToString();
                System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(() => sender.CheckResultsAsync(res, timestamp, activity).RunSynchronously());
                task.RunSynchronously();
            }
        }
    }
}