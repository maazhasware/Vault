using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using csharpcorner.Views;

namespace csharpcorner.ViewModels
{
    public class SettingsVM
    {
        private string _email;
        public SettingsVM(string email)
        {
            this._email = email;
        }

        public Command ChangePasswordCommand
        {
            get
            {
                return new Command(ChangePassword);
            }
        }

        private async void ChangePassword()
        {
            await App.Current.MainPage.Navigation.PushAsync(new ChangePasswordPage(_email));
        }

    }
}
