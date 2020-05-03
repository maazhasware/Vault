using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xamarin.Forms;

namespace csharpcorner.ViewModels
{
    public class UploadVideoVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _email;
        private MediaFile _file;
        private bool _activityIndicator = false;
        private bool _btnPickVideo = true;
        private bool _btnUploadVideo = false;
        public UploadVideoVM(string email)
        {
            this._email = email;
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

        public bool BtnPickVideo
        {
            get { return _btnPickVideo; }
            set
            {
                _btnPickVideo = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BtnPickVideo"));
            }
        }

        public bool BtnUploadVideo
        {
            get { return _btnUploadVideo; }
            set
            {
                _btnUploadVideo = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BtnUploadVideo"));
            }
        }



        private async void FileEncrypt(string inputFile)
        {
            try
            {
                string galleryPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMovies).AbsolutePath;
                string outputPath = Path.Combine(galleryPath + "/Vault", Path.GetFileName(_file.Path) + ".aes");
                var user = await FirebaseHelper.GetUser(_email);
                FileStream fsCrypt = new FileStream(outputPath, FileMode.Create);
                //Set Rijndael symmetric encryption algorithm
                RijndaelManaged AES = new RijndaelManaged();
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.PKCS7;
                var key = new Rfc2898DeriveBytes(user.Key, user.Salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CFB;
                // write salt to the begining of the output file
                fsCrypt.Write(user.Salt, 0, user.Salt.Length);
                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
                FileStream fsIn = new FileStream(inputFile, FileMode.Open);

                //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
                //1048576 is 1MB in binary
                byte[] buffer = new byte[1048576];
                int read;
                try
                {
                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        cs.Write(buffer, 0, read);
                    }
                    fsIn.Close();
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Error: " + ex.Message, "Ok");
                }
                finally
                {
                    cs.Close();
                    fsCrypt.Close();
                }
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Error", "Error uploading image, please try again", "Ok");
            }
        }




        public Command BtnPickVideoClicked
        {
            get { return new Command(PickVideoClicked); }
        }

        private async void PickVideoClicked()
        {
            await CrossMedia.Current.Initialize();
            try
            {
                _file = await Plugin.Media.CrossMedia.Current.PickVideoAsync();
                if (_file == null)
                    return;
                //set video preview here

                //set Upload Video button enabled


            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public Command BtnUploadVideoClicked
        {
            get
            {
                return new Command(UploadVideoClicked);    
            }
        }

        private async void UploadVideoClicked()
        {
            
        }


    }
}
