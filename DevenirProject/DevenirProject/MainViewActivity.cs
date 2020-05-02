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
using DevenirProject.WebService;
using Org.Json;

namespace DevenirProject
{
    [Activity(Label = "MainViewActivity")]
    public class MainViewActivity : Android.Support.V4.App.FragmentActivity
    {
        Bitmap sourceBitmap;
        AVLoadingIndicatorView loadingAnimation;

        FirebaseImageService firebaseImageService = new FirebaseImageService();
        LatexService latexService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_mainview);

            string path;
            path = Intent.GetStringExtra("image");
            if(path != null && path!="") sourceBitmap = GetBitmap(path);

            latexService = new LatexService(this);
            loadingAnimation = FindViewById<AVLoadingIndicatorView>(Resource.Id.loadingAnimation);

            PhotoCropFragment photoCropFragment = new PhotoCropFragment(sourceBitmap, ProcessBitmaps);
            Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
            Android.Support.V4.App.FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.content_main, photoCropFragment, "crop_view_tag").Commit();

            firebaseImageService.AddImageResultListener(delegate (string[] text, string[] ex)
            {
                if (ex == null)
                {
                    foreach (var detectionResult in text)
                    {
                        Toast.MakeText(Application.Context, detectionResult, ToastLength.Short).Show();
                    }
                }
                else if (text != null) Toast.MakeText(Application.Context, ex[0], ToastLength.Short).Show();
                else Toast.MakeText(Application.Context, "null", ToastLength.Short).Show();
            });


            latexService.AddOnLatexResultListener(delegate (string[] res, string[] ex)
            {
                if (res != null)
                {
                    foreach (var detectionResult in res)
                    {
                        var jsonRes = new JSONObject(detectionResult);
                        if (jsonRes.Has("latex_styled")) Toast.MakeText(Application.Context, jsonRes.Get("latex_styled").ToString(), ToastLength.Short).Show();
                        if (jsonRes.Has("text")) Toast.MakeText(Application.Context, jsonRes.Get("text").ToString(), ToastLength.Short).Show(); ;
                    }
                }
                else Toast.MakeText(Application.Context, ex[0], ToastLength.Long).Show();
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

        void ProcessBitmaps(Bitmap[] textBitmaps, Bitmap[] latexBitmaps)
        {
            Android.Support.V4.App.Fragment currentFragment = SupportFragmentManager.FindFragmentByTag("crop_view_tag");

            //enableDisableViewGroup((currentFragment.View as ViewGroup), false);
            //ShowLoading();

            if (textBitmaps.Length != 0) firebaseImageService.ProcessImages(textBitmaps);
            if (latexBitmaps.Length != 0) latexService.ProcessImages(latexBitmaps);
            //////
            ParsingResultFragment parsingResultFragment = new ParsingResultFragment(sourceBitmap, new string[] { "я общий", "я первый"}, new string[] { "я общий латех", "я первый латех","а я второй" });
            Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
            Android.Support.V4.App.FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.content_main, parsingResultFragment, "parsing_results_tag").Commit();
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