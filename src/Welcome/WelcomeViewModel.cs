using System;
using ReactiveUI;
using ReactiveUI.Xaml;
using Xamarin.Media;

namespace LeeMe.Android
{
    public class WelcomeViewModel : ReactiveObject
    {
        public ReactiveAsyncCommand TakeNewPhoto { get; protected set; }
        public ReactiveAsyncCommand ChooseExistingPhoto { get; protected set; }

        public WelcomeViewModel()
        {
            TakeNewPhoto = new ReactiveAsyncCommand();
            ChooseExistingPhoto = new ReactiveAsyncCommand();
        }
    }
}

