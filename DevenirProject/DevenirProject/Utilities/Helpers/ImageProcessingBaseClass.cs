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

namespace DevenirProject.Utilities.Helpers
{
    public abstract class ImageProcessingBaseClass
    {
        public delegate void ImageProcessing(string[] result, string[] exception);
        protected event ImageProcessing ImageProcessingResult;

        abstract public void ProcessImages(Bitmap[] image);
    }
}