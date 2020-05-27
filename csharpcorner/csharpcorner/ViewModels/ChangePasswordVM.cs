using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace csharpcorner.ViewModels
{
    public class ChangePasswordVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _email;
        private string _password;
        private string _passwordConfirmation;
        public ChangePasswordVM(string email)
        {
            this._email = email;
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

        public string PasswordConfirmation
        {
            get { return _passwordConfirmation; }
            set
            {
                _passwordConfirmation = value;
                PropertyChanged(this, new PropertyChangedEventArgs("PasswordConfirmation"));
            }
        }

        public Command SubmitCommand
        {
            get
            {
                return new Command(Submit);
            }
        }

        private async void Submit()
        {
            var user = await FirebaseHelper.GetUser(_email);
            if (!(string.IsNullOrEmpty(_password) | string.IsNullOrEmpty(_passwordConfirmation)))
            {
                if (_password == _passwordConfirmation)
                {
                    bool PasswordValid = IsValidPassword(_password);

                    if (PasswordValid == true)
                    {
                        string hashedPassword = string.Empty;
                        hashedPassword = HashPassword(_password);

                        try
                        {
                            await FirebaseHelper.UpdateUser(user.UserID, user.FirstName, user.Surname, user.Email, hashedPassword, user.Key, user.Salt);
                            await App.Current.MainPage.DisplayAlert("Success", "Password has been changed", "Ok");
                            await App.Current.MainPage.Navigation.PopAsync();
                        }
                        catch (Exception e)
                        {
                            await App.Current.MainPage.DisplayAlert("Error", "Password change failed, please try again", "Ok");
                        }
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Invalid Password", "Please enter a password in line with requirements", "OK");
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Passwords must match", "Ok");
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Empty Values", "Fields should not be empty", "Ok");
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

        private bool IsValidPassword(string password)
        {
            //string with min length 8
            //contains at least one number
            //contains at least one lower case or upper case alphabet, 
            //has no spaces
            string emailPattern = @"(?=^[^\s]{8,}$)(?=.*\d)(?=.*[a-zA-Z])";
            return Regex.IsMatch(password, emailPattern);
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



    }
}
