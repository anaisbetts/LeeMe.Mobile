using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using ReactiveUI;
using ReactiveUI.Android;
using Android.Graphics;
using System.Reactive.Linq;
using System.IO;

namespace LeeMe.Android
{
    [Activity (Label = "EditActivity")]            
    public class EditActivity : Activity, IViewFor<EditViewModel>, INotifyPropertyChanged
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RxApp.DeferredScheduler = new AndroidUIScheduler(this);
            
            ViewModel = new EditViewModel(x => 
                Observable.Start(() => BitmapFactory.DecodeFile(x), RxApp.TaskpoolScheduler));

            var targetFile = (bundle ?? new Bundle()).GetString("imagePath");
            if (targetFile == null || !File.Exists(targetFile)) {
                var intent = new Intent(this, typeof(WelcomeActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }

            SetContentView(Resource.Layout.Edit);
        }
        
        #region Boring copy-paste code I want to die
        EditViewModel _ViewModel;
        public EditViewModel ViewModel {
            get { return _ViewModel; }
            set {
                if (_ViewModel == value) return;
                _ViewModel = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ViewModel"));
            }
        }
        
        object IViewFor.ViewModel {
            get { return ViewModel; }
            set { ViewModel = (EditViewModel)value; }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}

