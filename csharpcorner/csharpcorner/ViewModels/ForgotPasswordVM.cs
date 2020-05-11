using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Xamarin.Forms;

namespace csharpcorner.ViewModels
{
    public class ForgotPasswordVM:INotifyPropertyChanged
    {
        private string _emailConfirmation;
        private string _email;
        public event PropertyChangedEventHandler PropertyChanged;

        public ForgotPasswordVM()
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

        public string EmailConfirmation
        {
            get { return _emailConfirmation; }
            set
            {
                _emailConfirmation = value;
                PropertyChanged(this, new PropertyChangedEventArgs("EmailConfirmation"));
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
            if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_emailConfirmation))
            {
                await App.Current.MainPage.DisplayAlert("Empty values error", "Both Email and Email Confirmation must not be empty", "Ok");
            }
            else
            {
                if (_email != _emailConfirmation)
                {
                    await App.Current.MainPage.DisplayAlert("Matching values error", "Both Email and Email Confirmation must match", "Ok");
                }
                else
                {
                    var user = await FirebaseHelper.GetUser(_email);
                    if (user == null)
                    {
                        await App.Current.MainPage.DisplayAlert("Email address not found", "Please ensure you have provided the correct email address used with your account", "Ok");
                    }
                    else
                    {
                        //reset users password to random password
                        try
                        {
                            string password = "Cherry123";
                            string hashedPassword = string.Empty;
                            hashedPassword = HashPassword(password);
                            await FirebaseHelper.UpdateUser(user.UserID, user.FirstName, user.Surname, user.Email, hashedPassword, user.Key, user.Salt);
                        }
                        catch
                        {
                            await App.Current.MainPage.DisplayAlert("Password reset error", "Failed to reset password, please try again", "Ok");
                        }

                        //send user an email telling them new password and advise to reset it straight away after logging in
                        try
                        {
                            MailMessage mail = new MailMessage();
                            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                            mail.From = new MailAddress("sambuhart@gmail.com");
                            mail.To.Add(user.Email);
                            mail.Subject = "Your Temporary Vault Password";
                            mail.Body = "Dear " + user.FirstName + ", \n Your new temporary password is Cherry123. Please login with your email and this temporary password and change your password immediately after logging in. \n  Kind Regards \n Vault Management Team";

                            SmtpServer.Port = 587;
                            SmtpServer.Host = "smtp.gmail.com";
                            SmtpServer.EnableSsl = true;
                            SmtpServer.UseDefaultCredentials = false;
                            SmtpServer.Credentials = new System.Net.NetworkCredential("sambuhart@gmail.com", "");//insert password when want to use
                            SmtpServer.Send(mail);

                            await App.Current.MainPage.DisplayAlert("Password reset success", "Please check your email inbox or junk folder for the email sent out to you", "Ok");
                        }
                        catch
                        {
                            await App.Current.MainPage.DisplayAlert("Email sending error", "Failed to send email, please try again", "Ok");
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

    }
}
