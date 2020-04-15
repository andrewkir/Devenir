using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Org.Json;
using Refit;

namespace DevenirProject.Utilities.API
{
    class LatexService
    {
        public delegate void LatexResult(string result, string ex);
        event LatexResult LatexResultEvent;

        public async void ProcessImageAsync(string image, Activity activity)
        {
            var api = RestService.For<APIService>("https://api.mathpix.com");
            JSONObject obj = new JSONObject();
            obj.Put("src", image);


            var response = await api.GetImageProcess(obj.ToString(), activity.Resources.GetString(Resource.String.mathpix_key), activity.Resources.GetString(Resource.String.mathpix_id));
            if (response.IsSuccessStatusCode)
            {
                LatexResultEvent?.Invoke(response.Content, null);
            }
            else
                LatexResultEvent?.Invoke(null, response.Error.Content.ToString());
        }

        public void AddOnLatexResultListener(LatexResult latexResult)
        {
            LatexResultEvent += latexResult;
        }
    }
}