using System;
using System.Collections.Generic;
using System.Linq;
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

        public delegate void ReturnJwt(string res);
        public event ReturnJwt ReturnJwtEvent;

        string timestamp;
        string device_id;

        public async System.Threading.Tasks.Task AttestateAsync(Activity activity)
        {
            
            
            var api = RestService.For<APIService>("https://devenir.andrewkir.ru");
            JSONObject obj = new JSONObject();

            timestamp = DateTime.Now.Ticks.ToString();
            device_id = Secure.GetString(activity.ContentResolver, Secure.AndroidId);

            obj.Put("timestamp", timestamp);
            obj.Put("device_id", device_id);


            var response = await api.GetNonce(obj.ToString());

            if (response.IsSuccessStatusCode)
            {
                string nonce = response.Content;

                if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Application.Context) == ConnectionResult.Success)
                {
                    // The SafetyNet Attestation API is available.
                    SafetyNetClient client = SafetyNetClass.GetClient(activity);
                    var task = client.Attest(Encoding.ASCII.GetBytes(nonce), activity.Resources.GetString(Resource.String.api_safetyNetKey))
                                     .AddOnSuccessListener(activity, new OnSuccessListener(activity, ReturnJwtEvent, timestamp))
                                     .AddOnFailureListener(activity, new OnFailureListener(activity));
                }
                else
                {
                    // Prompt user to update Google Play services.
                    Toast.MakeText(Application.Context, "You need to update your Google Play services!", ToastLength.Short).Show();
                }
                return;
            } else
            {
                Toast.MakeText(activity, response.Error.Content, ToastLength.Long).Show();
                return;
            }
        }

        private static async System.Threading.Tasks.Task CheckResultsAsync(string res, string timestamp, Activity activity)
        {
            var api = RestService.For<APIService>("https://devenir.andrewkir.ru");
            JSONObject obj = new JSONObject();
            obj.Put("safetynet", res);
            obj.Put("timestamp", timestamp);
            obj.Put("device_id", Secure.GetString(activity.ContentResolver, Secure.AndroidId));

            var response = await api.CreateAccessToken(obj.ToString());
            if (response.IsSuccessStatusCode)
            {
                Toast.MakeText(Application.Context, "Successful response " + response.Content.ToString(), ToastLength.Long).Show();
            }
            else
                Toast.MakeText(Application.Context, $"Error response {response.Content.ToString()}", ToastLength.Long).Show();
        }


        public class OnFailureListener : Java.Lang.Object, IOnFailureListener
        {
            Activity activity;
            public OnFailureListener(Activity activity) { this.activity = activity; }

            public void OnFailure(Java.Lang.Exception e)
            {
                Toast.MakeText(activity, $"Во время проверки приложения возникла ошибка", ToastLength.Long).Show();
            }
        }

        public class OnSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            Activity activity;
            ReturnJwt returnJwtevent;
            string timestamp;

            public OnSuccessListener(Activity activity, ReturnJwt returnJwt, string timestamp)
            {
                this.activity = activity;
                this.returnJwtevent = returnJwt;
                this.timestamp = timestamp;
            }

            public void OnSuccess(Java.Lang.Object result)
            {
                var res = (result as SafetyNetApiAttestationResponse).JwsResult.ToString();

                returnJwtevent?.Invoke($"{res} {timestamp} {Secure.GetString(activity.ContentResolver, Secure.AndroidId)}");
                CheckResultsAsync(res, timestamp, activity);
            }
        }
    }
}