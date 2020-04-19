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
        Activity activity;
        string image;

        public LatexService(Activity activity)
        {
            this.activity = activity;
        }

        public void ProcessImageAsync(string image)
        {
            this.image = image;

            JSONObject obj = new JSONObject();
            obj.Put("src", image);

            var api = new ApiImplementation();
            api.AddOnRequestResultListener(ResultListener);
            api.LatexRecogniseRequestAsync(obj, activity);
        }

        private void ResultListener(bool result, ApiResponse<string> response)
        {
            if (result && response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    LatexResultEvent?.Invoke(response.Content, null);
                }
                else
                    LatexResultEvent?.Invoke(null, response.Error.Content.ToString());
            }
            else
            {
                JSONObject obj = new JSONObject();
                obj.Put("src", image);

                var api = new ApiImplementation();
                api.AddOnRequestResultListener(FinalResultListener);
                api.LatexRecogniseRequestAsync(obj, activity);
            }
        }

        private void FinalResultListener(bool result, ApiResponse<string> response)
        {
            if (result && response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    LatexResultEvent?.Invoke(response.Content, null);
                }
                else
                    LatexResultEvent?.Invoke(null, response.Error.Content.ToString());
            }
        }

        public void AddOnLatexResultListener(LatexResult latexResult)
        {
            LatexResultEvent += latexResult;
        }
    }
}