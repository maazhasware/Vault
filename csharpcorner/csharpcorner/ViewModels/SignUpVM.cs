using csharpcorner.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using Xamarin.Forms;
using System.Text.RegularExpressions;

namespace csharpcorner.ViewModels
{
    public class SignUpVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _firstname;
        private string _surname;
        private string _email;
        private string _password;
        private string _confirmpassword;

        public SignUpVM()
        {

        }

    
        public string Firstname
        {
            get { return _firstname; }
            set
            {
                _firstname = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Firstname"));

            }
        }

        
        public string Surname
        {
            get { return _surname; }
            set
            {
                _surname = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Surname"));

            }
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

       
        public string ConfirmPassword
        {
            get { return _confirmpassword; }
            set
            {
                _confirmpassword = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ConfirmPassword"));
            }
        }

        public Command CancelCommand
        {
            get
            {
                return new Command(Cancel);
            }
        }

        private async void Cancel()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        public Command SignUpCommand
        {
            get
            {
                return new Command(() =>
                {
                    if (Password == ConfirmPassword)
                        SignUp();
                    else
                        App.Current.MainPage.DisplayAlert("Passwords not matching", "Password and Confirm Password must match", "OK");
                });
            }
        }

        private async void SignUp()
        {
            if (string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(ConfirmPassword)||
                string.IsNullOrEmpty(Firstname) ||
                string.IsNullOrEmpty(Surname))
            {
                await App.Current.MainPage.DisplayAlert("Empty Fields", "Please enter information in all fields", "OK");

            }
            else
            {
                if (IsValidEmail(Email) == false)
                {
                    await App.Current.MainPage.DisplayAlert("Invalid Email", "Please enter a valid email address", "OK");
                }
                else
                {
                    bool PasswordValid = IsValidPassword(Password);
                    if (PasswordValid == false)
                    {
                        await App.Current.MainPage.DisplayAlert("Invalid Password", "Please enter a password in line with requirements", "OK");

                    }
                    else
                    {
                        //hash password
                        string hashedPassword = string.Empty;
                        hashedPassword = HashPassword(Password);

                        //add user to database
                        var user = await FirebaseHelper.AddUser(Firstname, Surname, Email, hashedPassword);
                        //AddUser returns true if data is inserted successfuly
                        if (!user)
                        {
                            await App.Current.MainPage.DisplayAlert("Error", "SignUp Fail", "OK");
                        }
                        else
                        {
                            await App.Current.MainPage.DisplayAlert("SignUp Success", "Welcome to your Vault " + _firstname , "Ok");
                            App.Current.MainPage = new NavigationPage( new WelcomePage(Email));
                        }
                    }
                }
            }
        }

        private string HashPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);

            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string stringHashedPassword = Convert.ToBase64String(hashBytes);

            return stringHashedPassword;


        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            //string with min length 8
            //contains at least one number
            //contains at least one lower case or upper case alphabet, 
            //has no spaces
            string emailPattern = @"(?=^[^\s]{8,}$)(?=.*\d)(?=.*[a-zA-Z])";
            return Regex.IsMatch(password, emailPattern);
        }

    }
}
