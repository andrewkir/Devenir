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
using Plugin.Media;
using Plugin.Media.Abstractions;

namespace DevenirProject.ImageUtils
{
    public class ImageManager
    {
        public delegate void ImageResult(Bitmap bitmap, string path, Exception ex);
        event ImageResult ImageResultEvent;

        public async void TakePhoto()
        {
            try
            {
                await CrossMedia.Current.Initialize();
                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                    CompressionQuality = 60,
                    //Name = "myimage.jpg",
                    Directory = "Devenir",
                    AllowCropping = true
                });

                if (file == null)
                {
                    return;
                }

                // Convert file to byte array and set the resulting bitmap to imageview
                byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
                Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
                ImageResultEvent?.Invoke(bitmap, file.Path, null);
            }
            catch (Exception ex)
            {
                ImageResultEvent?.Invoke(null, null, ex);
            }
        }

        public async void PickPhoto()
        {
            try
            {
                await CrossMedia.Current.Initialize();

                var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize = PhotoSize.Large
                });

                if (file == null)
                {
                    throw new ArgumentException("Ошибка, пустой файл");
                }
                // Convert file to byte array and set the resulting bitmap to imageview
                byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
                Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
                ImageResultEvent?.Invoke(bitmap, file.Path, null);
            }
            catch (Exception ex)
            {
                ImageResultEvent?.Invoke(null, null, ex);
            }
        }

        public void AddOnImageResultListener(ImageResult imageResult)
        {
            ImageResultEvent += imageResult;
        }
    }
}