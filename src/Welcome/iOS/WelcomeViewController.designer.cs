// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace LeeMe.Welcome.iOS
{
	[Register ("WelcomeViewController")]
	partial class WelcomeViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton takeNewPhoto { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton chooseExistingPhoto { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView huffman { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (takeNewPhoto != null) {
				takeNewPhoto.Dispose ();
				takeNewPhoto = null;
			}

			if (chooseExistingPhoto != null) {
				chooseExistingPhoto.Dispose ();
				chooseExistingPhoto = null;
			}

			if (huffman != null) {
				huffman.Dispose ();
				huffman = null;
			}
		}
	}
}
