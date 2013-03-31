using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.Reactive.Linq;
using System.IO;

namespace LeeMe.Android
{
    public class EditViewModel : ReactiveObject
    {
        string _ImagePath;
        public string ImagePath {
            get { return _ImagePath; }
            set { this.RaiseAndSetIfChanged(ref _ImagePath, value); }
        }

        public ReactiveAsyncCommand LoadImage { get; protected set; }

        ObservableAsPropertyHelper<object> _LoadedImage;
        public object LoadedImage {
            get { return _LoadedImage.Value; }
        }

        public EditViewModel(Func<string, IObservable<object>> imageLoaderFunc)
        {
            LoadImage = new ReactiveAsyncCommand();

            this.WhenAny(x => x.ImagePath, x => x.Value)
                .Where(x => !String.IsNullOrWhiteSpace(x) && File.Exists(x))
                .InvokeCommand(LoadImage);

            LoadImage.RegisterAsyncObservable(x => imageLoaderFunc((string)x))
                .ToProperty(this, x => x.LoadedImage);
        }
    }
}

