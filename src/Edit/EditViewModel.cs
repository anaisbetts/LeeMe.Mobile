using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using System.IO;
using LeeMe.Support;
using System.Drawing;

namespace LeeMe.Edit
{
    public class EditViewModel : ReactiveObject, IDisposable
    {
        string _ImagePath;
        public string ImagePath {
            get { return _ImagePath; }
            set { this.RaiseAndSetIfChanged(ref _ImagePath, value); }
        }

        IBitmap _DatImg;
        public IBitmap DatImg {
            get { return _DatImg; }
            set { this.RaiseAndSetIfChanged(ref _DatImg, value); }
        }

        public ReactiveAsyncCommand LoadImage { get; protected set; }

        ObservableAsPropertyHelper<IBitmap> _LoadedImage;
        public IBitmap LoadedImage {
            get { return _LoadedImage.Value; }
        }

        bool _OnBottom = true;
        public bool OnBottom {
            get { return _OnBottom; }
            set { this.RaiseAndSetIfChanged(ref _OnBottom, value); }
        }

        bool _OnRight = false;
        public bool OnRight {
            get { return _OnRight; }
            set { this.RaiseAndSetIfChanged(ref _OnRight, value); }
        }

        float _ZoomLevel;
        public float ZoomLevel {
            get { return _ZoomLevel; }
            set { this.RaiseAndSetIfChanged(ref _ZoomLevel, value); }
        }

        float _TranslateX;
        public float TranslateX {
            get { return _TranslateX; }
            set { this.RaiseAndSetIfChanged(ref _TranslateX, value); }
        }

        float _TranslateY;
        public float TranslateY {
            get { return _TranslateY; }
            set { this.RaiseAndSetIfChanged(ref _TranslateY, value); }
        }

        RectangleF _ScreenRect;
        public RectangleF ScreenRect {
            get { return _ScreenRect; }
            set { this.RaiseAndSetIfChanged(ref _ScreenRect, value); }
        }

        float _DensityFactor;
        public float DensityFactor {
            get { return _DensityFactor; }
            set { this.RaiseAndSetIfChanged(ref _DensityFactor, value); }
        }

        ObservableAsPropertyHelper<RectangleF> _HuffmanSrcRect;
        public RectangleF HuffmanSrcRect {
            get { return _HuffmanSrcRect.Value; }
        }

        ObservableAsPropertyHelper<RectangleF> _HuffmanDestRect;
        public RectangleF HuffmanDestRect {
            get { return _HuffmanDestRect.Value; }
        }

        ObservableAsPropertyHelper<RectangleF> _ImageDestRect;
        public RectangleF ImageDestRect {
            get { return _ImageDestRect.Value; }
        }

        public EditViewModel()
        {
            LoadImage = new ReactiveAsyncCommand();

            this.WhenAny(x => x.ImagePath, x => x.Value)
                .Where(x => !String.IsNullOrWhiteSpace(x) && File.Exists(x))
                .InvokeCommand(LoadImage);

            LoadImage.RegisterAsyncTask(x => BitmapLoader.Current.Load(File.OpenRead((string)x)))
                .Do(_ => { if(LoadedImage != null) LoadedImage.Dispose(); })
                .ToProperty(this, x => x.LoadedImage);

            //var huffRects = this.Changed
             //   .Where(x => !x.PropertyName.Contains("Rect"))
              //  .Select(_ => calculateHuffmanRects(ScreenRect, DatImg, DensityFactor, OnBottom, OnRight));
            var huffRects = this.WhenAny(x => x.ScreenRect, x => x.DatImg, x => x.DensityFactor, x => x.OnBottom, x => x.OnRight, 
                (screen, img, density, bottom, right) => calculateHuffmanRects(screen.Value, img.Value, density.Value, bottom.Value, right.Value));

            huffRects.Select(x => x.Item1).ToProperty(this, x => x.HuffmanSrcRect);
            huffRects.Select(x => x.Item2).ToProperty(this, x => x.HuffmanDestRect);

            this.WhenAny(x => x.ScreenRect, x => x.LoadedImage, x => x.DensityFactor, 
                (frame, img, density) => calculateImageRect(frame.Value, img.Value, density.Value))
                .ToProperty(this, x => x.ImageDestRect);
            /*
            this.Changed
                .Where(x => !x.PropertyName.Contains("Rect"))
                .Select(_ => calculateImageRect(ScreenRect, LoadedImage, DensityFactor))
                .ToProperty(this, x => x.ImageDestRect);
*/
        }

        Tuple<RectangleF, RectangleF> calculateHuffmanRects(RectangleF frameRect, IBitmap datImg, float density, bool onBottom, bool onRight)
        {
            if (datImg == null || frameRect.Width < 1.0f || frameRect.Height < 1.0f) {
                return Tuple.Create(
                    new RectangleF(0.0f, 0.0f, 0.0f, 0.0f), 
                    new RectangleF(0.0f, 0.0f, 0.0f, 0.0f));
            }

            // NB: This code is all written for portrait, landscape is fucked
            // The general idea here is we want to define a square centered on
            // the screen whose size is the canvas short axis (i.e. the width
            // in portrait). Then, we're gonna scale everything to fit in that
            // box

            float imageCanvasHeight = frameRect.Width;   // Square, remember?
            float startTop = (frameRect.Height - imageCanvasHeight) / 2.0f;

            float width = datImg.Width / density;
            float height = datImg.Height / density;
            float scaleFactor = (frameRect.Width / 2.0f) / width;
            float scaledHeight = height * scaleFactor;

            if (onBottom && !onRight) {
                return Tuple.Create(
                    new RectangleF(0.0f, 0.0f, datImg.Width, datImg.Height), 
                    new RectangleF(0.0f, imageCanvasHeight - scaledHeight + startTop, frameRect.Width / 2.0f, scaledHeight));
            } else if (onBottom && onRight) {
                return Tuple.Create(
                    new RectangleF(datImg.Width, 0.0f, -datImg.Width, datImg.Height),
                    new RectangleF(frameRect.Width / 2.0f, imageCanvasHeight - scaledHeight + startTop, frameRect.Width / 2.0f, scaledHeight));
            } else if (!onBottom && !onRight) {
                return Tuple.Create(
                    new RectangleF(0.0f, datImg.Height, datImg.Width, -datImg.Height), 
                    new RectangleF(0.0f, startTop, frameRect.Width / 2.0f, scaledHeight));
            } else { 
                return Tuple.Create(
                    new RectangleF(datImg.Width, datImg.Height, -datImg.Width, -datImg.Height), 
                    new RectangleF(frameRect.Width / 2.0f, startTop, frameRect.Width / 2.0f, scaledHeight));
            }
        }

        RectangleF calculateImageRect(RectangleF screenRect, IBitmap loadedImage, float density)
        {
            if (loadedImage == null || screenRect.Width < 1.0f) {
                return new RectangleF(0.0f, 0.0f, 0.0f, 0.0f);
            }

            // NB: This code is all written for portrait, landscape is fucked
            // The general idea here is we want to define a square centered on
            // the screen whose size is the canvas short axis (i.e. the width
            // in portrait). Then, we're gonna scale everything to fit in that
            // box

            float imageCanvasHeight = screenRect.Width;   // Square, remember?
            float startTop = (screenRect.Height - imageCanvasHeight) / 2.0f;

            var imgHeight = loadedImage.Height / density;
            var imgWidth = loadedImage.Width / density;

            float longSide = Math.Max(imgHeight, imgWidth);
            float imgScaleFactor = screenRect.Width / longSide;
            float scaledImgHeight = imgHeight * imgScaleFactor;
            float scaledImgWidth = imgWidth * imgScaleFactor;

            if (imgHeight > imgWidth) {
                var imgLeft = (screenRect.Width - scaledImgWidth) / 2.0f;
                return new RectangleF(imgLeft, startTop, scaledImgWidth, imageCanvasHeight);
            } else {
                var imgTop = (imageCanvasHeight - scaledImgHeight) / 2.0f;
                return new RectangleF(0.0f, imgTop + startTop, screenRect.Width, scaledImgHeight);
            }
        }

        public void Dispose()
        {
            LoadedImage.Dispose();
        }
    }
}

