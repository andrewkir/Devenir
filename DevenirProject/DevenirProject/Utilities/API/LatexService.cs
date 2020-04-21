using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Org.Json;
using Refit;
using Android.Graphics;
using DevenirProject.Utilities.Utils;

namespace DevenirProject.Utilities.API
{
    class LatexService : ImageProcessingBaseClass
    {
        event ImageProcessing LatexResultEvent;
        Activity activity;
        string image;

        public LatexService(Activity activity)
        {
            this.activity = activity;
        }

        public override void ProcessImage(Bitmap image)
        {

            using (var ms = new System.IO.MemoryStream())
            {
                image.Compress(Bitmap.CompressFormat.Jpeg, 0, ms);
                var res = Base64.EncodeToString(ms.ToArray(), Base64Flags.Default);
                this.image = "data:image/jpeg;base64," + res;
            }
            JSONObject obj = new JSONObject();
            obj.Put("src", this.image);

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
                obj.Put("src", this.image);

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
            else
            {
                LatexResultEvent?.Invoke(null,"Отсутствует подключение к сервису");
            }
        }

        public void AddOnLatexResultListener(ImageProcessing latexResult)
        {
            LatexResultEvent += latexResult;
        }
    }
}