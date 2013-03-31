using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using ActionbarSherlock.App;
using ReactiveUI;
using ReactiveUI.Xaml;
using System.ComponentModel;
using Xamarin.Media;
using ReactiveUI.Android;
using System.Reactive.Linq;

namespace LeeMe.Android
{
    [Activity (Label = "Lee-Me.Android", MainLauncher = true)]
    public class WelcomeActivity : SherlockActivity, IViewFor<WelcomeViewModel>, INotifyPropertyChanged
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RxApp.DeferredScheduler = new AndroidUIScheduler(this);

            ViewModel = new WelcomeViewModel();

            SetContentView(Resource.Layout.Welcome);

            FindViewById<Button>(Resource.Id.takeNewPhoto).Click += (o,e) => {
                if (ViewModel.TakeNewPhoto.CanExecute(null)) {
                    ViewModel.TakeNewPhoto.Execute(null);
                }
            };

            FindViewById<Button>(Resource.Id.chooseExistingPhoto).Click += (o,e) => {
                if (ViewModel.ChooseExistingPhoto.CanExecute(null)) {
                    ViewModel.ChooseExistingPhoto.Execute(null);
                }
            };

            var defaultCamera = new StoreCameraMediaOptions() { DefaultCamera = CameraDevice.Rear, Directory = "LeeMe", Name = "turnt.jpg", };
            var picker = new MediaPicker(this);

            SupportActionBar.Title = "Lee Me";
            SupportActionBar.SetIcon(Resource.Drawable.Icon);

            Observable.Merge(
                    ViewModel.TakeNewPhoto.RegisterAsyncTask(_ => picker.TakePhotoAsync(defaultCamera)),
                    ViewModel.ChooseExistingPhoto.RegisterAsyncTask(_ => picker.PickPhotoAsync()))
                .Subscribe(x => {
                    Console.WriteLine(x.Path);
                });
        }

        #region Boring copy-paste code I want to die
        WelcomeViewModel _ViewModel;
        public WelcomeViewModel ViewModel {
            get { return _ViewModel; }
            set {
                if (_ViewModel == value) return;
                _ViewModel = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ViewModel"));
            }
        }
        
        object IViewFor.ViewModel {
            get { return ViewModel; }
            set { ViewModel = (WelcomeViewModel)value; }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}


