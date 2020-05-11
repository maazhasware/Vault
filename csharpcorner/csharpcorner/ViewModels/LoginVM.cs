using csharpcorner.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using Xamarin.Forms;

namespace csharpcorner.ViewModels
{
    public class LoginVM : INotifyPropertyChanged
    {
        private bool _activityIndicator;
        private string _email;
        private string _password;
        public event PropertyChangedEventHandler PropertyChanged;

        public LoginVM()
        {

        }
        
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Email"));
            }
        }
        
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Password"));
            }
        }

        public bool ActivityIndicator
        {
            get { return _activityIndicator; }
            set
            {
                _activityIndicator = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ActivityIndicator"));
            }
        }

        public Command LoginCommand
        {
            get
            {
                return new Command(Login);
            }
        }

        private async void Login()
        {
            ActivityIndicator = true;

            //check if email or password fields are null or empty
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ActivityIndicator = false;
                await App.Current.MainPage.DisplayAlert("Empty Fields", "Email and Password field cannot be empty", "OK");
            }
            else
            {
                //call GetUser function from FirebaseHelper class    
                var user = await FirebaseHelper.GetUser(Email);
                if (user != null)
                { 
                    //get stored hashed password
                    byte[] hashBytes = Convert.FromBase64String(user.Password);

                    //hash entered password
                    byte[] salt = new byte[16];
                    Array.Copy(hashBytes, 0, salt, 0, 16);
                    var pbkdf2 = new Rfc2898DeriveBytes(Password, salt, 10000);
                    byte[] hash = pbkdf2.GetBytes(20);

                    //compare entered password to stored password
                    bool passwordMatches = true;
                    for (int i = 0; i < 20; i++)
                    {
                        if (hashBytes[i + 16] != hash[i])
                        {
                            passwordMatches = false;
                        }
                    }

                    if (Email == user.Email && passwordMatches == true)
                    {
                        ActivityIndicator = false;
                        await App.Current.MainPage.DisplayAlert("Login Successful", "Welcome to your Vault " + user.FirstName , "Ok");
                        //set welcome page as new navigation page instead of navigating to it so user can't press back button to come back to login page    
                        App.Current.MainPage = new NavigationPage(new WelcomePage(Email));
                    }
                    else
                    {
                        ActivityIndicator = false;
                        await App.Current.MainPage.DisplayAlert("Login Failed", "Please enter correct password", "OK");
                    }
                }
                else
                {
                    ActivityIndicator = false;
                    await App.Current.MainPage.DisplayAlert("Login Failed", "Please enter correct email address", "OK");

                }
            }
        }

        public Command ForgotPasswordCommand
        {
            get
            {
                return new Command(ForgotPassword);
            }
        }

        private async void ForgotPassword()
        {
            await App.Current.MainPage.Navigation.PushAsync(new ForgotPasswordPage());
        }

        public Command RegisterCommand
        {
            get
            {
                return new Command(Register);
            }
        }

        private async void Register()
        {
            await App.Current.MainPage.Navigation.PushAsync(new SignUpPage());
        }

    }
}
