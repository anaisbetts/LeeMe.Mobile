using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using System.IO;

namespace LeeMe.Android
{
    public class EditViewModel : ReactiveObject, IDisposable
    {
        string _ImagePath;
        public string ImagePath {
            get { return _ImagePath; }
            set { this.RaiseAndSetIfChanged(ref _ImagePath, value); }
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

        double _ZoomLevel;
        public double ZoomLevel {
            get { return _ZoomLevel; }
            set { this.RaiseAndSetIfChanged(ref _ZoomLevel, value); }
        }

        double _TranslateX;
        public double TranslateX {
            get { return _TranslateX; }
            set { this.RaiseAndSetIfChanged(ref _TranslateX, value); }
        }

        double _TranslateY;
        public double TranslateY {
            get { return _TranslateY; }
            set { this.RaiseAndSetIfChanged(ref _TranslateY, value); }
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
        }

        public void Dispose()
        {
            LoadedImage.Dispose();
        }
    }
}

