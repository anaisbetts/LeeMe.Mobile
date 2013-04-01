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
using System.Reactive.Subjects;
using LeeMe.Welcome.Android;
using LeeMe;
using LeeMe.Support;

namespace LeeMe.Edit.Android
{
    public enum SwipeDirection {
        ToRight, ToLeft, ToTop, ToBottom,
    }

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

            var view = new EditView(this);
            ViewModel.Changed.Subscribe(_ => view.Invalidate());

            // XXX: Debug
            Observable.Timer(DateTimeOffset.MinValue, TimeSpan.FromSeconds(5.0f), RxApp.DeferredScheduler).Subscribe(_ => view.Invalidate());

            // XXX: Debug
            //Observable.Timer(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(4.0), RxApp.DeferredScheduler).Subscribe(_ => ViewModel.OnBottom = !ViewModel.OnBottom);
            //Observable.Timer(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(4.0), RxApp.DeferredScheduler).Subscribe(_ => ViewModel.OnRight = !ViewModel.OnRight);

            SetContentView(view);
        }

        class EditView : View
        {
            readonly EditActivity activity;
            readonly GestureDetector detector;

            public EditView(EditActivity activity) : base(activity)
            {
                this.activity = activity;

                var obsListener = new ObservableGestureDetector();
                detector = new GestureDetector(obsListener);

                obsListener.SwipeGesture.Subscribe(x => {
                    switch(x) {
                    case SwipeDirection.ToBottom:
                        activity.ViewModel.OnBottom = true;
                        break;
                    case SwipeDirection.ToTop:
                        activity.ViewModel.OnBottom = false;
                        break;
                    case SwipeDirection.ToRight:
                        activity.ViewModel.OnRight = true;
                        break;
                    case SwipeDirection.ToLeft:
                        activity.ViewModel.OnRight = false;
                        break;
                    }
                });
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                return detector.OnTouchEvent(e);
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

                // XXX: Debug
                canvas.DrawRect(new Rect(0, (int)startTop, imageCanvasHeight, imageCanvasHeight + (int)startTop),
                                new Paint() { Color = Color.AliceBlue, StrokeWidth = 2 });

                if (activity.ViewModel.LoadedImage != null) {
                    var img = activity.ViewModel.LoadedImage.ToNative();
                    var imgHeight = img.GetScaledHeight(canvas);
                    var imgWidth = img.GetScaledWidth(canvas);

                    float longSide = Math.Max(imgHeight, imgWidth);
                    float imgScaleFactor = (float)canvas.Width / longSide;
                    float scaledImgHeight = imgHeight * imgScaleFactor;
                    float scaledImgWidth = imgWidth * imgScaleFactor;

                    if (imgHeight > imgWidth) {
                        var imgLeft = (canvas.Width - scaledImgWidth) / 2.0f;
                        canvas.DrawBitmap(img,
                            new Rect(0, 0, imgWidth, imgHeight),
                            new Rect((int)imgLeft, (int)startTop, (int)(imgLeft + scaledImgWidth), (int)startTop + imageCanvasHeight),
                            defaultPaint);
                    } else {
                        var imgTop = (imageCanvasHeight - scaledImgHeight) / 2.0f;

                        canvas.DrawBitmap(img,
                            new Rect(0, 0, imgWidth, imgHeight),
                            new Rect(0, (int)imgTop + (int)startTop, canvas.Width, (int)imgTop + (int)scaledImgHeight + (int)startTop),
                            defaultPaint);
                    }
                }

                if (activity.ViewModel.OnBottom && !activity.ViewModel.OnRight) {
                    canvas.DrawBitmap(activity.datLee, 
                        new Rect(0, 0, width, height),
                        new Rect(0, (int)imageCanvasHeight - scaledHeight + (int)startTop, canvas.Width / 2, imageCanvasHeight + (int)startTop),
                        defaultPaint);
                } else if (activity.ViewModel.OnBottom && activity.ViewModel.OnRight) {
                    canvas.DrawBitmap(activity.datLee, 
                        new Rect(width, 0, 0, height),
                        new Rect(canvas.Width / 2, (int)imageCanvasHeight - scaledHeight + (int)startTop, canvas.Width, imageCanvasHeight + (int)startTop),
                        defaultPaint);
                } else if (!activity.ViewModel.OnBottom && !activity.ViewModel.OnRight) {
                    canvas.DrawBitmap(activity.datLee, 
                        new Rect(0, height, width, 0),
                        new Rect(0, (int)startTop, canvas.Width / 2, scaledHeight + (int)startTop),
                        defaultPaint);
                } else if (!activity.ViewModel.OnBottom && activity.ViewModel.OnRight) {
                    canvas.DrawBitmap(activity.datLee, 
                        new Rect(width, height, 0, 0),
                        new Rect(canvas.Width / 2, (int)startTop, canvas.Width, scaledHeight + (int)startTop),
                        defaultPaint);
                }
            }
        }

        class ObservableGestureDetector : GestureDetector.SimpleOnGestureListener
        {
            const int SWIPE_MIN_DISTANCE = 120;
            const int SWIPE_MAX_OFF_PATH = 100;
            const int SWIPE_THRESHOLD_VELOCITY = 200;

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                var deltaX = e2.GetX() - e1.GetY();
                var deltaY = e2.GetY() - e1.GetY();
                var absX = Math.Abs(deltaX);
                var absY = Math.Abs(deltaY);

                // Diagonal or too short
                if (Math.Abs(absY - absX) <= SWIPE_MAX_OFF_PATH) {
                    return false;
                }

                if (absX > absY && absX > SWIPE_MIN_DISTANCE && Math.Abs(velocityX) > SWIPE_THRESHOLD_VELOCITY) {
                    _SwipeGesture.OnNext(deltaX > 0 ? SwipeDirection.ToRight : SwipeDirection.ToLeft);
                } 

                if (absX < absY && absY > SWIPE_MIN_DISTANCE && Math.Abs(velocityY) > SWIPE_THRESHOLD_VELOCITY) {
                    _SwipeGesture.OnNext(deltaY > 0 ? SwipeDirection.ToBottom : SwipeDirection.ToTop);
                }

                return false;
            }

            readonly Subject<SwipeDirection> _SwipeGesture = new Subject<SwipeDirection>();
            public IObservable<SwipeDirection> SwipeGesture { get { return _SwipeGesture; } }
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

