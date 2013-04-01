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
using ActionbarSherlock.App;

namespace LeeMe.Android
{
    [Activity (Label = "EditActivity")]            
    public class EditActivity : SherlockActivity, IViewFor<EditViewModel>, INotifyPropertyChanged
    {
        Bitmap datLee;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RxApp.DeferredScheduler = new AndroidUIScheduler(this);
            
            ViewModel = new EditViewModel();

            var targetFile = Intent.GetStringExtra("imagePath");
            if (targetFile == null || !File.Exists(targetFile)) {
                var intent = new Intent(this, typeof(WelcomeActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }

            SupportActionBar.Title = "Get Turnt";
            SupportActionBar.SetIcon(Resource.Drawable.Icon);

            datLee = BitmapFactory.DecodeResource(Resources, Resource.Drawable.lee);

            ViewModel.LoadImage.Execute(targetFile);

            SetContentView(new EditView(this));
        }

        class EditView : View
        {
            readonly EditActivity activity;

            public EditView(EditActivity activity) : base(activity)
            {
                this.activity = activity;
            }

            Paint defaultPaint = new Paint();
            protected override void OnDraw(Canvas canvas)
            {
                base.OnDraw(canvas);

                // NB: This code is all written for portrait, landscape is fucked
                // The general idea here is we want to define a square centered on
                // the screen whose size is the canvas short axis (i.e. the width
                // in portrait). Then, we're gonna scale everything to fit in that
                // box

                var imageCanvasHeight = canvas.Width;   // Square, remember?
                var startTop = (canvas.Height - imageCanvasHeight) / 2.0f;

                var width = activity.datLee.GetScaledWidth(canvas);
                var height = activity.datLee.GetScaledHeight(canvas);
                var scaleFactor = (canvas.Width / 2.0) / (double)width;
                int scaledHeight = (int)(height * scaleFactor);

                canvas.DrawRect(new Rect(0, (int)startTop, imageCanvasHeight, imageCanvasHeight + (int)startTop),
                                new Paint() { Color = Color.AliceBlue, StrokeWidth = 2 });

                canvas.DrawBitmap(activity.datLee, 
                    new Rect(0, 0, width, height),
                    new Rect(0, (int)imageCanvasHeight - scaledHeight + (int)startTop, canvas.Width / 2, imageCanvasHeight + (int)startTop),
                    defaultPaint);
            }
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

