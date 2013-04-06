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
        IBitmap Create(float width, float height);
    }

    public interface IBitmap : IDisposable
    {
        float Width { get; }
        float Height { get; }
        int[] GetPixels(float x, float y, float width, float height);
        Task Save(CompressedBitmapFormat format, float quality, Stream target);
    }
}

