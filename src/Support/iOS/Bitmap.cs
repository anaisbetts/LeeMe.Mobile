using System;
using System.Threading.Tasks;
using System.IO;
using MonoTouch.UIKit;
using System.Threading;
using MonoTouch.Foundation;

namespace LeeMe.Support
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
            return Task.Run(() => {
                var data = NSData.FromStream(sourceStream);
                return (IBitmap) new CocoaBitmap(UIImage.LoadFromData(data));
            });
        }
        
        public IBitmap Create(float width, float height)
        {
            throw new NotImplementedException();
        }
    }
    
    sealed class CocoaBitmap : IBitmap
    {
        internal UIImage inner;
        public CocoaBitmap(UIImage inner)
        {
            this.inner = inner;
        }
        
        public float Width {
            get { return inner.Size.Width; }
        }
        
        public float Height {
            get { return inner.Size.Height; }
        }
        
        public int[] GetPixels(float x, float y, float width, float height)
        {
            throw new NotImplementedException();
        }
        
        public Task Save(CompressedBitmapFormat format, float quality, Stream target)
        {
            return Task.Run(() => {
                var data = format == CompressedBitmapFormat.Jpeg ? inner.AsJPEG((float)quality) : inner.AsPNG();
                data.AsStream().CopyTo(target);
            });
        }
        
        public void Dispose()
        {
            var disp = Interlocked.Exchange(ref inner, null);
            if (disp != null) disp.Dispose();
        }
    }
    
    public static class BitmapMixins
    {
        public static UIImage ToNative(this IBitmap This)
        {
            return ((CocoaBitmap)This).inner;
        }
        
        public static IBitmap FromNative(this UIImage This, bool copy = false)
        {
            if (copy) return new CocoaBitmap((UIImage)This.Copy());

            return new CocoaBitmap(This);
        }
    }
}


