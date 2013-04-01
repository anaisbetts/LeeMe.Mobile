using System;
using System.Threading.Tasks;
using System.IO;
using Android.Graphics;
using System.Threading;

namespace LeeMe.Android
{
    public class BitmapLoader : IBitmapLoader
    {
        static IBitmapLoader _Current = new BitmapLoader();
        public static IBitmapLoader Current {
            get { return _Current; }
            set { _Current = value; }
        }

        protected BitmapLoader() { }

        public Task<IBitmap> Load(Stream sourceStream)
        {
            return Task.Run(() => BitmapFactory.DecodeStream(sourceStream).FromNative());
        }

        public IBitmap Create(double width, double height)
        {
            return Bitmap.CreateBitmap((int)width, (int)height, Bitmap.Config.Argb8888).FromNative();
        }
    }
            
    sealed class AndroidBitmap : IBitmap
    {
        internal Bitmap inner;
        public AndroidBitmap(Bitmap inner)
        {
            this.inner = inner;
        }
                    
        public double Width {
            get { return inner.Width; }
        }

        public double Height {
            get { return inner.Height; }
        }

        public int[] GetPixels(double x, double y, double width, double height)
        {
            var ret = new int[(int)(width * height)];
            inner.GetPixels(ret, 0, (int)width, (int)x, (int)y, (int)width, (int)height);
            return ret;
        }

        public Task Save(CompressedBitmapFormat format, double quality, Stream target)
        {
            var fmt = format == CompressedBitmapFormat.Jpeg ? Bitmap.CompressFormat.Jpeg : Bitmap.CompressFormat.Png;
            return Task.Run(() => inner.Compress(fmt, (int)quality * 100, target));
        }

        public void Dispose()
        {
            var disp = Interlocked.Exchange(ref inner, null);
            if (disp != null) disp.Dispose();
        }
    }

    public static class BitmapMixins
    {
        public static Bitmap ToNative(this IBitmap This)
        {
            return ((AndroidBitmap)This).inner;
        }

        public static IBitmap FromNative(this Bitmap This, bool copy = false)
        {
            if (copy) return new AndroidBitmap(This.Copy(This.GetConfig(), true));
            return new AndroidBitmap(This);
        }
    }
}

