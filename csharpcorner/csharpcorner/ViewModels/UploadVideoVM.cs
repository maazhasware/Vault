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
        private string _source;
        private string _email;
        private MediaFile _file;
        private bool _activityIndicator = false;
        private bool _btnPickVideo = true;
        private bool _btnUploadVideo = false;
        public event PropertyChangedEventHandler PropertyChanged;

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

        public string Source
        {
            get { return _source; }
            set
            {
                _source = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Source"));
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
                AES.KeySize = 128;//set to 128 bit for videos to make faster
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
                    await App.Current.MainPage.DisplayAlert("Encryption Error", " Please try again", "Ok");
                }
                finally
                {
                    cs.Close();
                    fsCrypt.Close();
                }
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Encryption Failed", "Please try again", "Ok");
            }

            string galleryPath1 = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMovies).AbsolutePath;
            string outputPath1 = Path.Combine(galleryPath1 + "/Vault", Path.GetFileName(_file.Path) + ".aes");

            var user1 = await FirebaseHelper.GetUser(_email);

            FileStream filestream = File.OpenRead(outputPath1);

            await FirebaseHelper.UploadVideo(filestream, Path.GetFileName(_file.Path), user1.UserID);
            var downloadurl = await FirebaseHelper.GetVideo(Path.GetFileName(_file.Path), user1.UserID);
            await FirebaseHelper.UploadVideoURL(Path.GetFileName(_file.Path), downloadurl.ToString(), user1.UserID);

            //delete encrypted file we create on device
            if (File.Exists(outputPath1))
            {
                File.Delete(outputPath1);
            }

            //stop activity indicator
            ActivityIndicator = false;

            await App.Current.MainPage.DisplayAlert("Upload Success", "Video has been uploaded", "OK");
            Source = string.Empty;

            //re-enable PickImage button
            BtnPickVideo = true;

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
                Source = _file.AlbumPath;

                //set Upload Video button enabled
                BtnUploadVideo = true;

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
            try
            {
                ActivityIndicator = true;

                BtnUploadVideo = false;
                BtnPickVideo = false;

                //string galleryPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
                //string outputPath = Path.Combine(galleryPath + "/Vault", Path.GetFileName(_file.Path) + ".aes");
                FileEncrypt(_file.Path);

                //var user = await FirebaseHelper.GetUser(_email);

                //FileStream filestream = System.IO.File.OpenRead(outputPath);

                //await FirebaseHelper.UploadFile(filestream, Path.GetFileName(_file.Path), user.UserID);
                //var downloadurl = await FirebaseHelper.GetFile(Path.GetFileName(_file.Path), user.UserID);
                //await FirebaseHelper.UploadURL(Path.GetFileName(_file.Path), downloadurl.ToString(), user.UserID);

                ////delete encrypted file we create on device
                //System.IO.File.Delete(outputPath);

                ////stop activity indicator
                //ActivityIndicator = false;

                //await App.Current.MainPage.DisplayAlert("Upload Success", "Video has been uploaded", "OK");

                ////re-enable PickImage button
                //BtnPickVideo = true;
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Upload Failed", "Please try again", "Ok");
                Source = string.Empty;
                ActivityIndicator = false;
                BtnPickVideo = true;
                BtnUploadVideo= false;
            }
        }


    }
}
