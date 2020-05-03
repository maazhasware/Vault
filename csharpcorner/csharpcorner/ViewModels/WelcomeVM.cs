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

        //view all files command
        public Command ViewAllFilesCommand
        {
            get
            {
                return new Command(ViewAllFiles);
            }
        }

        private async void ViewAllFiles()
        {
            User user = await FirebaseHelper.GetUser(_email);
            await App.Current.MainPage.Navigation.PushAsync(new ListViewPage(_email));
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
