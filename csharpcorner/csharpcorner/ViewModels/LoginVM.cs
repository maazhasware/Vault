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
        public Command LoginCommand
        {
            get
            {
                return new Command(Login);
            }
        }

        private async void Login()
        {
            //null or empty field validation, check weather email and password is null or empty    

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                await App.Current.MainPage.DisplayAlert("Empty Values", "Please enter Email and Password", "OK");

            }
            else
            {
                //call GetUser function which we define in Firebase helper class    
                var user = await FirebaseHelper.GetUser(Email);
                //firebase return null valuse if user data not found in database    
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
                            await App.Current.MainPage.DisplayAlert("Login Success", "", "Ok");
                            //Navigate to welcome page after successful login    
                            //pass user email to welcome page
                            App.Current.MainPage = new NavigationPage(new WelcomePage(Email));

                        }
                        else
                        {
                            await App.Current.MainPage.DisplayAlert("Login Fail", "Please enter correct Email and Password", "OK");

                        }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Login Fail", "Invalid user credentials", "OK");

                }
            }
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
