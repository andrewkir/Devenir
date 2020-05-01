﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using DevenirProject.Views;
using Java.Interop;

namespace DevenirProject
{
    class PhotoCropFragment : Android.Support.V4.App.Fragment
    {
        event Action<Bitmap[], Bitmap[]> processBitmapsEvent;

        string path;

        MultiPointCropView cropView;
        Bitmap srcBitmap;

        List<Bitmap> textModeBitmaps = new List<Bitmap>();
        List<Bitmap> LaTeXBitmaps = new List<Bitmap>();

        FloatingActionButton floatingAddButton;
        FloatingActionButton floatingDoneButton;
        Switch switchMode;

        bool init = true;


        public PhotoCropFragment(string path, Action<Bitmap[], Bitmap[]> processBitmap)
        {
            this.path = path;
            processBitmapsEvent += processBitmap;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_photoCrop, container, false);
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (path != "" && path != null)
            {
                srcBitmap = GetBitmap(path);
            }

            cropView = view.FindViewById<MultiPointCropView>(Resource.Id.cropview_layout);
            cropView.ViewTreeObserver.GlobalLayout += ViewTreeObserver_GlobalLayout;

            floatingAddButton = view.FindViewById<FloatingActionButton>(Resource.Id.floatingAddButton);
            floatingAddButton.Click += delegate
            {
                //LaTeX mode
                if (switchMode.Checked)
                {
                    if (LaTeXBitmaps.Count < 3) LaTeXBitmaps.Add(cropView.CropView());
                    else
                    {
                        var snackbar = Snackbar.Make(view, "Вы не можете добавить больше 3 фрагментов фотографии для анализа", Snackbar.LengthShort);
                        snackbar.SetAction("Ок", (view) => { });
                        snackbar.Show();
                        return;
                    }
                }
                //Text mode
                else
                {
                    if (textModeBitmaps.Count < 3) textModeBitmaps.Add(cropView.CropView());
                    else
                    {
                        var snackbar = Snackbar.Make(view, "Вы не можете добавить больше 3 фрагментов фотографии для анализа", Snackbar.LengthShort);
                        snackbar.SetAction("Ок", (view) => { });
                        snackbar.Show();
                        return;
                    }
                }

                cropView.SetPointsDefault();
                Toast.MakeText(view.Context, "Успешно добавлено", ToastLength.Short).Show();
            };
            floatingAddButton.LongClick += delegate
            {
                AlertDialog.Builder alertDialog = new AlertDialog.Builder(view.Context);
                alertDialog.SetTitle("Подтвердите действие");
                alertDialog.SetMessage("Вы уверены, что хотите сбросить состояние до первоначального?");
                alertDialog.SetPositiveButton("Да", (sender, args) =>
                {
                    cropView.SetPointsDefault();
                });
                alertDialog.SetNeutralButton("Отмена", (sender, args) => { });
                Dialog dialog = alertDialog.Create();
                dialog.Show();
            };

            floatingDoneButton = view.FindViewById<FloatingActionButton>(Resource.Id.floatingDoneButton);
            floatingDoneButton.Click += delegate
            {
                processBitmapsEvent?.Invoke(textModeBitmaps.ToArray(), LaTeXBitmaps.ToArray());
            };


            cropView.SetCropColor(Color.Gray);
            switchMode = view.FindViewById<Switch>(Resource.Id.switchMode);
            switchMode.CheckedChange += delegate
            {
                if (!switchMode.Checked)
                {
                    cropView.SetCropColor(Color.Gray);
                }
                else
                {
                    cropView.SetCropColor(Resources.GetColor(Resource.Color.PointViewColor));
                }
            };
        }

        private void ViewTreeObserver_GlobalLayout(object sender, EventArgs e)
        {
            if (init)
            {
                cropView.SetBitmap(srcBitmap);
                cropView.SetPointsDefault();
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