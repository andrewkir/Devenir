using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using DevenirProject.Views;
using Java.IO;

namespace DevenirProject
{
    [Activity(Label = "Activity1")]
    public class PhotoCropActivity : Activity
    {
        MultiPointCropView cropView;
        Bitmap bitmap;

        Button setDotsDefault;
        Button cropViewButton;

        bool init = true;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.AppTheme);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_photocrop);
            string path = Intent.GetStringExtra("image");

            if (path != "" && path != null)
            {
                bitmap = GetBitmap(path);
            }

            cropView = FindViewById<MultiPointCropView>(Resource.Id.cropview_layout);
            cropView.ViewTreeObserver.GlobalLayout += ViewTreeObserver_GlobalLayout;

            setDotsDefault = FindViewById<Button>(Resource.Id.resetPointsButton);
            setDotsDefault.Click += delegate { cropView.SetDotsDefault(); };

            cropViewButton = FindViewById<Button>(Resource.Id.cropViewButton);
            cropViewButton.Click += delegate {
                Bitmap res = cropView.CropView();
                if(res != null)
                {
                    string path = System.IO.Path.Combine(
                        Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, "Devenir");

                    var filename = System.IO.Path.Combine(path, $"DEVENIR_{DateTime.Now:yyyy_MM_dd_hh_mm_ss.ff}.jpg");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (System.IO.File.Exists(filename))
                    {
                        System.IO.File.Delete(filename);
                    }

                    try
                    {
                        FileStream outStream = new FileStream(filename, FileMode.Create);
                        res.Compress(Bitmap.CompressFormat.Jpeg, 90, outStream);
                        outStream.Close();

                        var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                        mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(filename)));
                        SendBroadcast(mediaScanIntent);
                        Toast.MakeText(this, "Crop saved", ToastLength.Short).Show();
                    }
                    catch (Exception e)
                    {
                        Log.Debug("ERROR", e.Message);
                        Toast.MakeText(this, "Error: " + e.Message, ToastLength.Short).Show();
                    }
                }
            
            };
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