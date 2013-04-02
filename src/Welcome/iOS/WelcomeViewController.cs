
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.ComponentModel;
using ReactiveUI;
using ReactiveUI.Xaml;
using Xamarin.Media;

namespace LeeMe.Welcome.iOS
{
    public partial class WelcomeViewController : UIViewController, IViewFor<WelcomeViewModel>, INotifyPropertyChanged
    {
        static bool UserInterfaceIdiomIsPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        public WelcomeViewController()
            : base (UserInterfaceIdiomIsPhone ? "WelcomeViewController_iPhone" : "WelcomeViewController_iPad", null)
        {
            ViewModel = new WelcomeViewModel(new MediaPicker());
        }
        
        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
            
            // Release any cached data, images, etc that aren't in use.
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var img = UIImage.FromBundle("drawable/lee.png");
            huffman.Image = img;

            this.BindCommand(ViewModel, x => x.TakeNewPhoto, x => x.takeNewPhoto);
            this.BindCommand(ViewModel, x => x.ChooseExistingPhoto, x => x.chooseExistingPhoto);
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

