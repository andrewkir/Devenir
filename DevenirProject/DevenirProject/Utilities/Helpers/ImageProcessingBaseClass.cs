using Android.Graphics;

namespace DevenirProject.Utilities.Helpers
{
    public abstract class ImageProcessingBaseClass
    {
        public delegate void ImageProcessing(string[] result, string[] exception);
        protected event ImageProcessing ImageProcessingResult;

        abstract public void ProcessImages(Bitmap[] image);
    }
}