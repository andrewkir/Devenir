using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DevenirProject.Utilities.Utils;
using Firebase;
using Firebase.ML.Vision;
using Firebase.ML.Vision.Common;
using Firebase.ML.Vision.Document;
using Firebase.ML.Vision.Text;

namespace DevenirProject.WebService
{
    public class FirebaseImageService : ImageProcessingBaseClass
    {
        event ImageProcessing ImageDetectionEvent;

        public override void ProcessImage(Bitmap bitmap)
        {
            try
            {
                var app = FirebaseApp.InitializeApp(Application.Context);
                FirebaseVisionTextRecognizer detector = FirebaseVision.GetInstance(app).OnDeviceTextRecognizer;


                FirebaseVisionCloudDocumentRecognizerOptions options = new FirebaseVisionCloudDocumentRecognizerOptions.Builder()
                    .SetLanguageHints(new List<String> { "en", "ru" })
                    .Build();
                FirebaseVisionDocumentTextRecognizer det = FirebaseVision.GetInstance(app).GetCloudDocumentTextRecognizer(options);

                FirebaseVisionImage image = FirebaseVisionImage.FromBitmap(bitmap);
                var result = det
                    .ProcessImage(image)
                    .AddOnCompleteListener(new ImageDetectionListener(ImageDetectionEvent));
            }
            catch (Exception)
            {
                ImageDetectionEvent?.Invoke(null, "Произошла непредвиденная ошибка");
            }
        }

        public void AddImageResultListener(ImageProcessing imageDetectionResult)
        {
            ImageDetectionEvent += imageDetectionResult;
        }

        class ImageDetectionListener : Java.Lang.Object, IOnCompleteListener
        {
            ImageProcessing eventRes;
            public ImageDetectionListener(ImageProcessing eventRes)
            {
                this.eventRes = eventRes;
            }
            public void OnComplete(Android.Gms.Tasks.Task task)
            {
                if (!task.IsSuccessful)
                {
                    eventRes?.Invoke(null, "Ошибка во время обработки изображения");
                }
                if (task.Result != null && ((FirebaseVisionDocumentText)task.Result).Text != null) eventRes?.Invoke(((FirebaseVisionDocumentText)task.Result).Text, null);
                else eventRes?.Invoke(null, null);
            }
        }
    }
}