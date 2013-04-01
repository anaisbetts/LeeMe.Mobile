using System;
using System.IO;
using System.Threading.Tasks;

namespace LeeMe.Support
{
    public enum CompressedBitmapFormat {
        Png, Jpeg,
    }

    public interface IBitmapLoader
    {
        Task<IBitmap> Load(Stream sourceStream);
        IBitmap Create(double width, double height);
    }

    public interface IBitmap : IDisposable
    {
        double Width { get; }
        double Height { get; }
        int[] GetPixels(double x, double y, double width, double height);
        Task Save(CompressedBitmapFormat format, double quality, Stream target);
    }
}

