using System;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Xaml;
using Xamarin.Media;

namespace LeeMe.Welcome
{
    public class WelcomeViewModel : ReactiveObject
    {
        public ReactiveAsyncCommand TakeNewPhoto { get; protected set; }
        public ReactiveAsyncCommand ChooseExistingPhoto { get; protected set; }
        public ReactiveCommand StartEdit { get; protected set; }

        public WelcomeViewModel(MediaPicker picker)
        {
            TakeNewPhoto = new ReactiveAsyncCommand();
            ChooseExistingPhoto = new ReactiveAsyncCommand();
            StartEdit = new ReactiveCommand();

            var defaultCamera = new StoreCameraMediaOptions() { DefaultCamera = CameraDevice.Rear, Directory = "LeeMe", Name = "turnt.jpg", };

            Observable.Merge(
                    TakeNewPhoto.RegisterAsyncTask(_ => picker.TakePhotoAsync(defaultCamera)),
                    ChooseExistingPhoto.RegisterAsyncTask(_ => picker.PickPhotoAsync()))
                .Select(x => x.Path)
                .InvokeCommand(StartEdit);

            // TODO: Write a proper UserError handler for this
            TakeNewPhoto.ThrownExceptions.Subscribe(ex => Console.WriteLine(ex));
            ChooseExistingPhoto.ThrownExceptions.Subscribe(ex => Console.WriteLine(ex));
        }
    }
}

