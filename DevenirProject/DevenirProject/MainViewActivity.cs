using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Wang.Avi;

namespace DevenirProject
{
    [Activity(Label = "MainViewActivity")]
    public class MainViewActivity : Android.Support.V4.App.FragmentActivity
    {
        string path;
        AVLoadingIndicatorView loadingAnimation;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_mainview);
            
            path = Intent.GetStringExtra("image");

            loadingAnimation = FindViewById<AVLoadingIndicatorView>(Resource.Id.loadingAnimation);

            PhotoCropFragment photoCropFragment = new PhotoCropFragment(path, ProcessBitmaps);
            Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
            Android.Support.V4.App.FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.content_main, photoCropFragment,"first_fragment_tag").Commit();
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
            ShowLoading();
            Toast.MakeText(this, $"Объектов в textBitmap: {textBitmaps.Length}, latexBitmap: {latexBitmaps.Length}", ToastLength.Short).Show();
            ParsingResultFragment parsingResultFragment = new ParsingResultFragment();
            Android.Support.V4.App.FragmentManager fragmentManager = SupportFragmentManager;
            Android.Support.V4.App.FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();
            fragmentTransaction.Replace(Resource.Id.content_main, parsingResultFragment, "first_fragment_tag").Commit();
        }
    }
}