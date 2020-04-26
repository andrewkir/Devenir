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
using DevenirProject.Views;

namespace DevenirProject
{
    [Activity(Label = "Activity1")]
    public class PhotoCropActivity : Activity
    {
        MultiPointCropView cropView;
        Bitmap bitmap;

        Button setDotsDefault;

        bool init = true;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.AppTheme);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_photocrop);
            string path = Intent.GetStringExtra("image");

            if (path != "" && path != null)
            {
                byte[] imageArray = System.IO.File.ReadAllBytes(path);
                bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            }
            cropView = FindViewById<MultiPointCropView>(Resource.Id.cropview_layout);
            cropView.ViewTreeObserver.GlobalLayout += ViewTreeObserver_GlobalLayout;

            setDotsDefault = FindViewById<Button>(Resource.Id.resetPointsButton);
            setDotsDefault.Click += delegate { cropView.SetDotsDefault(); };
        }

        private void ViewTreeObserver_GlobalLayout(object sender, EventArgs e)
        {
            if (init)
            {
                cropView.SetBitmap(bitmap);
                cropView.SetDotsDefault();
                init = false;
            }
        }
    }
}