using csharpcorner.Models;
using csharpcorner.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace csharpcorner.ViewModels
{
    public class WelcomeVM
    {
        private string _email;


        public WelcomeVM(string email)
        {
            this._email = email;
        }

        //upload image command
        public Command UploadImageCommand
        {
            get
            {
                return new Command(UploadImage);
            }
        }

        private async void UploadImage()
        {
            await App.Current.MainPage.Navigation.PushAsync(new UploadImagePage(_email));
        }

        //upload video command
        public Command UploadVideoCommand
        {
            get
            {
                return new Command(UploadVideo);
            }
        }

        private async void UploadVideo()
        {
            await App.Current.MainPage.Navigation.PushAsync(new UploadVideoPage(_email));
        }

        //view all images command
        public Command ViewAllImagesCommand
        {
            get
            {
                return new Command(ViewAllImages);
            }
        }

        private async void ViewAllImages()
        {
            await App.Current.MainPage.Navigation.PushAsync(new ListViewPage(_email));
        }

        //view all videos command
        public Command ViewAlVideosCommand
        {
            get
            {
                return new Command(ViewAllVideos);
            }
        }

        private async void ViewAllVideos()
        {
            await App.Current.MainPage.Navigation.PushAsync(new ListViewPage(_email)); //TODO: create listview for videos page
        }

        //settings command
        public Command SettingsCommand
        {
            get
            {
                return new Command(Settings);
            }
        }

        private async void Settings()
        {
            await App.Current.MainPage.Navigation.PushAsync(new SettingsPage(_email));
        }


        public Command LogoutCommand
        {
            get
            {
                return new Command(Logout);

            }
        }

        private async void Logout()
        {
            App.Current.MainPage = new LoginPage();
        }


    }
}
