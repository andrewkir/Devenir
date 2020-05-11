using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Wang.Avi;
using DevenirProject.Utilities.API;
using DevenirProject.Utilities.FirebaseService;
using Org.Json;

namespace DevenirProject
{
    [Activity(Label = "MainViewActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainViewActivity : Android.Support.V4.App.FragmentActivity
    {
        Bitmap sourceBitmap;
        AVLoadingIndicatorView loadingAnimation;

        FirebaseImageService firebaseImageService;
        LatexApiService latexService;

        List<string> textParsigResults = new List<string>();
        List<string> latexParsigResults = new List<string>();
        int processedReady = 0;
        int processNums = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_mainview);

            string path;
            path = Intent.GetStringExtra("image");
            if (path != null && path != "") sourceBitmap = GetBitmap(path);

            latexService = new LatexApiService(this);
            firebaseImageService = new FirebaseImageService(this);
            loadingAnimation = FindViewById<AVLoadingIndicatorView>(Resource.Id.loadingAnimation);

            PhotoCropFragment photoCropFragment = new PhotoCropFragment(sourceBitmap, ProcessBitmaps);
            Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
            Android.Support.V4.App.FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.content_main, photoCropFragment, "crop_view_tag").Commit();

            firebaseImageService.AddImageResultListener(delegate (string[] text, string[] ex)
            {
                if (ex == null || ex.Length == 0)
                {
                    List<string> tmpRes = new List<string>();
                    foreach (var detectionResult in text)
                    {
                        tmpRes.Add(detectionResult);
                    }
                    textParsigResults.Add(string.Join('\n', tmpRes.ToArray()));
                    textParsigResults.AddRange(tmpRes);
                }
                else if (ex.Length > 0) Toast.MakeText(Application.Context, $"Ошибка: {ex[0]}", ToastLength.Short).Show();
                else Toast.MakeText(Application.Context, "null", ToastLength.Short).Show();
                processedReady++;
                ProcessedReady();
            });


            latexService.AddOnLatexResultListener(delegate (string[] res, string[] ex)
            {
                if (res != null)
                {
                    List<string> tmpRes = new List<string>();
                    foreach (var detectionResult in res)
                    {
                        var jsonRes = new JSONObject(detectionResult);
                        if (jsonRes.Has("latex_styled"))
                        {
                            tmpRes.Add(jsonRes.Get("latex_styled").ToString());
                        }
                        else if (jsonRes.Has("text"))
                        {
                            tmpRes.Add(jsonRes.Get("text").ToString());
                        }
                    }
                    latexParsigResults.Add(string.Join('\n', tmpRes.ToArray()));
                    latexParsigResults.AddRange(tmpRes);
                }
                else if(ex[0] != null) Toast.MakeText(Application.Context, $"Ошибка: {ex[0]}", ToastLength.Short).Show();
                processedReady++;
                ProcessedReady();
            });
        }

        void ShowLoading()
        {
            loadingAnimation.Show();
        }

        void StopLoading()
        {
            loadingAnimation.Hide();
        }

        void ProcessedReady()
        {
            if (processedReady == processNums)
            {
                Android.Support.V4.App.Fragment currentFragment = SupportFragmentManager.FindFragmentByTag("crop_view_tag");
                StopLoading();
                if (textParsigResults.Count == 0 && latexParsigResults.Count == 0)
                {
                    enableDisableViewGroup((currentFragment.View as ViewGroup), true);
                }
                else
                {
                    ParsingResultsFragment parsingResultFragment = new ParsingResultsFragment(sourceBitmap, textParsigResults.ToArray(), latexParsigResults.ToArray());
                    Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
                    Android.Support.V4.App.FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();
                    fragmentTransaction.Replace(Resource.Id.content_main, parsingResultFragment, "parsing_results_tag").Commit();
                }
            }
        }

        void ProcessBitmaps(Bitmap[] textBitmaps, Bitmap[] latexBitmaps)
        {
            if (textBitmaps.Length == 0 && latexBitmaps.Length == 0)
            {
                StopLoading();
                Toast.MakeText(this, "Вы ещё не выбрали фрагментов для анализа!", ToastLength.Short).Show();
            }
            else
            {
                Android.Support.V4.App.Fragment currentFragment = SupportFragmentManager.FindFragmentByTag("crop_view_tag");
                enableDisableViewGroup((currentFragment.View as ViewGroup), false);
                ShowLoading();

                if (textBitmaps.Length != 0)
                {
                    processNums++;
                    firebaseImageService.ProcessImages(textBitmaps);
                }
                if (latexBitmaps.Length != 0)
                {
                    processNums++;
                    latexService.ProcessImages(latexBitmaps);
                }
            }
        }

        static void enableDisableViewGroup(ViewGroup viewGroup, bool enabled)
        {
            int childCount = viewGroup.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                View view = viewGroup.GetChildAt(i);
                view.Enabled = enabled;
                if (view is ViewGroup)
                {
                    enableDisableViewGroup((ViewGroup)view, enabled);
                }
            }
        }

        private Bitmap GetBitmap(string path)
        {
            byte[] imageArray = System.IO.File.ReadAllBytes(path);
            ExifInterface exif = new ExifInterface(path);
            int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 1);
            Matrix matrix = new Matrix();
            switch (orientation)
            {
                case 3:
                    matrix.PostRotate(180);
                    break;
                case 6:
                    matrix.PostRotate(90);
                    break;
                case 8:
                    matrix.PostRotate(270);
                    break;
            }

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            return Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
        }
    }
}