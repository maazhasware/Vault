using csharpcorner.Views;
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
    public class UploadImageVM:INotifyPropertyChanged
    {
        private string _email;
        private MediaFile _file;
        private ImageSource _imagePreview;
        private bool _activityIndicator = false;
        private bool _btnPickImage = true;
        private bool _btnUploadImage = false;
        public event PropertyChangedEventHandler PropertyChanged;

        public UploadImageVM(string email)
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

        public bool BtnPickImage
        {
            get { return _btnPickImage;  }
            set
            {
                _btnPickImage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BtnPickImage"));
            }
        }

        public bool BtnUploadImage
        {
            get { return _btnUploadImage; }
            set
            {
                _btnUploadImage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BtnUploadImage"));
            }
        }

        public ImageSource ImagePreview
        {
            get { return _imagePreview; }
            set
            {
                _imagePreview = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ImagePreview"));
            }
        }

        private async void FileEncrypt(string inputFile)
        {
            try
            {
                string galleryPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
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

        public Command PickImageCommand
        {
            get
            {
                return new Command(PickImage);
            }
        }

        private async void PickImage()
        {
            await CrossMedia.Current.Initialize();
            try
            {
                _file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (_file == null)
                    return;

                ImagePreview = ImageSource.FromStream(() =>
                {
                    var imageStream = _file.GetStream();
                    return imageStream;
                });

                //set Upload Image button enabled
                BtnUploadImage = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public Command UploadImageCommand
        {
            get
            {
                return new Command(UploadImage);
            }
        }

        private async void UploadImage()
        {
            if (_imagePreview == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You haven't picked an image yet", "Ok");
            }
            else
            {
                try
                {
                    //start actiity indicator
                    ActivityIndicator = true;

                    //disable buttons
                    BtnUploadImage = false;
                    BtnPickImage = false;

                    string galleryPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
                    string outputPath = Path.Combine(galleryPath + "/Vault", Path.GetFileName(_file.Path) + ".aes");
                    FileEncrypt(_file.Path);
                    var user = await FirebaseHelper.GetUser(_email);

                    FileStream filestream = System.IO.File.OpenRead(outputPath);

                    await FirebaseHelper.UploadFile(filestream, Path.GetFileName(_file.Path), user.UserID);
                    var downloadurl = await FirebaseHelper.GetFile(Path.GetFileName(_file.Path), user.UserID);
                    await FirebaseHelper.UploadURL(Path.GetFileName(_file.Path), downloadurl.ToString(), user.UserID);

                    //delete encrypted file we create on device
                    System.IO.File.Delete(outputPath);

                    //stop activity indicator
                    ActivityIndicator = false;

                    await App.Current.MainPage.DisplayAlert("Success", "Image has been uploaded", "OK");
                    ImagePreview = "";

                    //re-enable PickImage button
                    BtnPickImage = true;
                }
                catch
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Error in uploading image, please try again", "Ok");
                    ImagePreview = "";
                    ActivityIndicator = false;
                    BtnPickImage = true;
                    BtnUploadImage = false;
                }
                

               
            }
        }


    }
}
