﻿using System.Collections.Generic;

using Android.App;
using Android.Util;
using Refit;
using Android.Graphics;
using DevenirProject.Utilities.Helpers;
using Newtonsoft.Json.Linq;

namespace DevenirProject.Utilities.API
{
    class LatexApiService : ImageProcessingBaseClass
    {
        event ImageProcessing LatexResultEvent;
        Activity activity;
        List<string> images;

        public LatexApiService(Activity activity)
        {
            this.activity = activity;
        }

        public override void ProcessImages(Bitmap[] images)
        {
            this.images = new List<string>();
            foreach (var image in images)
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    image.Compress(Bitmap.CompressFormat.Jpeg, 0, ms);
                    var res = Base64.EncodeToString(ms.ToArray(), Base64Flags.Default);
                    this.images.Add("data:image/jpeg;base64," + res);
                }
            }

            JObject obj = new JObject();
            obj.Add("images", new JArray(this.images));

            var api = new ApiImplementation();
            api.AddOnRequestResultListener(ResultListener);
            api.LatexRecogniseRequestAsync(obj, activity);
        }

        private void ResultListener(bool result, ApiResponse<string> response, string exception)
        {
            if (result && response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    LatexResultEvent?.Invoke(ParseResponse(response.Content), null);
                }
                else
                    LatexResultEvent?.Invoke(null, new string[]{ activity.GetString(Resource.String.latexServerException) });
            }
            else
            {
                JObject obj = new JObject();
                obj.Add("images", new JArray(this.images));

                var api = new ApiImplementation();
                api.AddOnRequestResultListener(FinalResultListener);
                api.LatexRecogniseRequestAsync(obj, activity);
            }
        }

        private void FinalResultListener(bool result, ApiResponse<string> response, string exception)
        {
            if (result && response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    LatexResultEvent?.Invoke(ParseResponse(response.Content), null);
                }
                else
                    LatexResultEvent?.Invoke(null, new string[] { activity.GetString(Resource.String.latexServerException) });
            }
            else
            {
                LatexResultEvent?.Invoke(null, new string[] { exception });
            }
        }

        public void AddOnLatexResultListener(ImageProcessing latexResult)
        {
            LatexResultEvent += latexResult;
        }

        private string[] ParseResponse(string response)
        {
            List<string> result = new List<string>();
            JObject responseJson = JObject.Parse(response);
            if (responseJson.ContainsKey("output"))
            {
                JArray imagesResponse = JArray.Parse(responseJson.GetValue("output").ToString());
                foreach (JToken item in imagesResponse)
                {
                    result.Add(item.ToString());
                }
                return result.ToArray();
            }
            else
            {
                LatexResultEvent?.Invoke(null, new string[] { $"Проблемы на стороне сервера :(, Response:{response}" });
                return null;
            }
        }
    }
}