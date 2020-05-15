using System;
using System.Collections.Generic;

using Android.App;
using Android.Gms.Tasks;
using Android.Graphics;
using DevenirProject.Utilities.Helpers;
using Firebase;
using Firebase.ML.Vision;
using Firebase.ML.Vision.Common;
using Firebase.ML.Vision.Document;
using Firebase.ML.Vision.Text;

namespace DevenirProject.Utilities.FirebaseService
{
    public class FirebaseImageService : ImageProcessingBaseClass
    {
        event ImageProcessing ImageDetectionEvent;
        int imagesProcessedCount;
        int imagesCount;
        List<string> processedResult;
        List<string> errors;

        Activity activity;

        public FirebaseImageService(Activity activity)
        {
            this.activity = activity;
        }

        public override void ProcessImages(Bitmap[] images)
        {
            imagesProcessedCount = 0;
            processedResult = new List<string>();
            errors = new List<string>();

            imagesCount = images.Length;
            foreach (var img in images)
            {
                ProcessImage(img, ImageResult);
            }
        }

        public void ProcessImage(Bitmap bitmap, Action<string, string> resultAction)
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
                    .AddOnCompleteListener(new ImageDetectionListener(resultAction, activity));
            }
            catch (Exception)
            {
                resultAction?.Invoke(null, activity.GetString(Resource.String.unexpectedException));
            }
        }

        public void AddImageResultListener(ImageProcessing imageDetectionResult)
        {
            ImageDetectionEvent += imageDetectionResult;
        }


        private void ImageResult(string res, string ex)
        {
            imagesProcessedCount++;
            if (res != null) processedResult.Add(res);
            if (ex != null) errors.Add(ex);
            if (imagesProcessedCount == imagesCount)
            {
                ImageDetectionEvent?.Invoke(processedResult.ToArray(), errors == null ? null : errors.ToArray());
            }
        }

        class ImageDetectionListener : Java.Lang.Object, IOnCompleteListener
        {
            Action<string, string> imageResult;
            Activity activity;
            public ImageDetectionListener(Action<string, string> imageResult, Activity activity)
            {
                this.imageResult = imageResult;
                this.activity = activity;
            }
            public void OnComplete(Android.Gms.Tasks.Task task)
            {
                if (!task.IsSuccessful)
                {
                    imageResult?.Invoke(null, activity.GetString(Resource.String.firebaseNoConnectionException));
                }
                else
                {
                    if (task.Result != null && ((FirebaseVisionDocumentText)task.Result).Text != null) imageResult?.Invoke(((FirebaseVisionDocumentText)task.Result).Text, null);
                    else imageResult?.Invoke(null, null);
                }
            }
        }
    }
}