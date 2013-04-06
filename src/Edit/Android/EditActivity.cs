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
using ActionbarSherlock.View;
using System.Drawing;
using Android.Util;
using System.Reactive.Concurrency;

namespace LeeMe.Edit.Android
{
    public enum SwipeDirection {
        ToRight, ToLeft, ToTop, ToBottom,
    }

    [Activity (Label = "EditActivity")]            
    public class EditActivity : SherlockActivity, IViewFor<EditViewModel>, INotifyPropertyChanged
    {
        Bitmap datLee;
        GestureDetector detector;
        
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
            ViewModel.DatImg = datLee.FromNative();

            ViewModel.LoadImage.Execute(targetFile);

            var view = new EditView(this);
            ViewModel.Changed.Subscribe(_ => view.Invalidate());

            // XXX: Debug
            //Observable.Timer(DateTimeOffset.MinValue, TimeSpan.FromSeconds(5.0f), RxApp.DeferredScheduler).Subscribe(_ => view.Invalidate());

            // XXX: Debug
            //Observable.Timer(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(4.0), RxApp.DeferredScheduler).Subscribe(_ => ViewModel.OnBottom = !ViewModel.OnBottom);
            //Observable.Timer(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(4.0), RxApp.DeferredScheduler).Subscribe(_ => ViewModel.OnRight = !ViewModel.OnRight);

            var obsListener = new ObservableGestureDetector();
            detector = new GestureDetector(obsListener);

            obsListener.SwipeGesture.Subscribe(x => {
                switch(x) {
                case SwipeDirection.ToBottom:
                    ViewModel.OnBottom = true;
                    break;
                case SwipeDirection.ToTop:
                    ViewModel.OnBottom = false;
                    break;
                case SwipeDirection.ToRight:
                    ViewModel.OnRight = true;
                    break;
                case SwipeDirection.ToLeft:
                    ViewModel.OnRight = false;
                    break;
                }
            });

            SetContentView(view);
        }

        public override bool OnCreateOptionsMenu(ActionbarSherlock.View.IMenu menu)
        {
            SupportMenuInflater.Inflate(Resource.Menu.share_action_provider, menu);
            var item = (ActionbarSherlock.View.IMenuItem)menu.FindItem(Resource.Id.menu_item_share_action_provider_action_bar);
            var actionProvider = (ActionbarSherlock.Widget.ShareActionProvider)item.ActionProvider;
            actionProvider.SetShareHistoryFileName(ShareActionProvider.DefaultShareHistoryFileName);

            var dummyIntent = new Intent(Intent.ActionSend);
            dummyIntent.SetType("image/*");
            dummyIntent.PutExtra(Intent.ExtraStream, new Uri("file:///foo").ToString());
            actionProvider.SetShareIntent(dummyIntent);

            return true;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            return detector.OnTouchEvent(e);
        }

        class EditView : View
        {
            readonly EditActivity activity;

            public EditView(EditActivity activity) : base(activity)
            {
                this.activity = activity;
            }

            protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
            {
                base.OnSizeChanged(w, h, oldw, oldh);
                var newRect = new RectangleF(0.0f, 0.0f, (float)w, (float)h);

                activity.ViewModel.ScreenRect = newRect;

                var metrics = new DisplayMetrics();
                activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
                activity.ViewModel.DensityFactor = metrics.Density;
            }

            Paint defaultPaint = new Paint();
            protected override void OnDraw(Canvas canvas)
            {
                base.OnDraw(canvas);
                var vm = activity.ViewModel;

                if (vm.LoadedImage == null || vm.ImageDestRect.Width < 1.0f) return;

                var img = vm.LoadedImage.ToNative();
                var srcRect = new Rect(0, 0, img.GetScaledWidth(canvas), img.GetScaledHeight(canvas));
                canvas.DrawBitmap(vm.LoadedImage.ToNative(), 
                    srcRect,
                    vm.ImageDestRect.ToRect(), 
                    defaultPaint);

                if (vm.HuffmanDestRect.Width > 1.0f) {
                    canvas.DrawBitmap(activity.datLee, vm.HuffmanSrcRect.ToRect(), vm.HuffmanDestRect.ToRect(), defaultPaint);
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
                var deltaX = e2.GetX() - e1.GetX();
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

    public static class AndroidRectMixins
    {
        public static Rect ToRect(this RectangleF This)
        {
            return new Rect((int)This.Left, (int)This.Top, (int)This.Right, (int)This.Bottom);
        }
        public static RectF ToRectF(this RectangleF This)
        {
            var ret = new RectF(This.Left, This.Top, This.Right, This.Bottom);
            return ret;
        }
    }
}

