using Android.Media;
using csharpcorner.Models;
using csharpcorner.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace csharpcorner.ViewModels
{
    public class FullViewVM:INotifyPropertyChanged
    {
        private string _outputPathForDecryptedFile = string.Empty;
        private bool _btnDownloadClicked = false;
        private string _galleryPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
        private bool _btnDelete = true;
        private bool _btnDownload = true;
        private bool _activityIndicator;
        private ImageSource _imageView = string.Empty;
        private string _email;
        private ImageObject _imageObject;
        public event PropertyChangedEventHandler PropertyChanged;

        public FullViewVM(ImageObject imageObject, string email)
        {
            this._email = email;
            this._imageObject = imageObject;
            //Subscribe to event from FullViewPage OnAppearing override method to load page then image => appears as better performance
            MessagingCenter.Subscribe<FullView>(this, "LoadPage", (sender) =>
            {
                Download();
            });
            //Subscribe to event from FullViewPage OnDisappearing override method to check download button state
            MessagingCenter.Subscribe<FullView>(this, "BackButtonPressed", (sender)=>
            {
                //check to see if download button clicked, delete image if not clicked
                CheckDownloadButtonState();
            });
           
        }

        public bool BtnDownload
        {
            get { return _btnDownload; }
            set
            {
                _btnDownload = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BtnDownload"));
            }
        }

        public bool BtnDelete
        {
            get { return _btnDelete; }
            set
            {
                _btnDelete = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BtnDelete"));
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

        public ImageSource ImageView
        {
            get { return _imageView; }
            set
            {
                _imageView = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ImageView"));
            }
        }

        private async void FileDecrypt(string inputFile, string outputFile)
        {
            try
            {
                var user = await FirebaseHelper.GetUser(_email);
                FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
                fsCrypt.Read(user.Salt, 0, user.Salt.Length);
                RijndaelManaged AES = new RijndaelManaged();
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.PKCS7;
                var key = new Rfc2898DeriveBytes(user.Key, user.Salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CFB;
                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
                FileStream fsOut = new FileStream(outputFile, FileMode.Create);
                int read;
                byte[] buffer = new byte[1048576];
                try
                {
                    while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fsOut.Write(buffer, 0, read);
                    }
                }
                catch (CryptographicException ex_CryptographicException)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Decryption error, please try again ", "Ok");
                    await App.Current.MainPage.Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Decryption error, please try again" , "Ok");
                    await App.Current.MainPage.Navigation.PopAsync();
                }
                try
                {
                    cs.Close();
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Decryption error, please try again ", "Ok");
                    await App.Current.MainPage.Navigation.PopAsync();
                }
                finally
                {
                    fsOut.Close();
                    fsCrypt.Close();
                }

                //to show pics in users gallery
                MediaScannerConnection.ScanFile(Android.App.Application.Context, new string[] { outputFile }, new string[] { "image / jpg", "image/ png", "image /jpeg" }, null);

                //display image in image preview
                ImageView = ImageSource.FromFile(outputFile);

                //delete encrypted file we downloaded before
                if (File.Exists(inputFile))
                {
                    File.Delete(inputFile);
                }

                ActivityIndicator = false;
                BtnDownload = true;
                BtnDelete = true;
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Decryption Error", "Please try again", "Ok");
                await App.Current.MainPage.Navigation.PopAsync();
            }
        }

        private async void Download()
        {
            try
            {
                ActivityIndicator = true;
                BtnDownload = false;
                BtnDelete = false;

                //creating output path for encrypted file without file extension
                string filename = _imageObject.FileName;
                int index = filename.LastIndexOf(".");
                if (index > 0)
                {
                    filename = filename.Substring(0, index);
                }
                string outputPathForEncryptedFile = Path.Combine(_galleryPath + "/Vault/", filename);

                //download encyrpted file
                using (var client = new WebClient())
                {
                    client.DownloadFile(_imageObject.Url, outputPathForEncryptedFile);
                }

                //output path for decrypted file
                string outputPathForDecryptedFile = Path.Combine(_galleryPath + "/Vault/", _imageObject.FileName);
                _outputPathForDecryptedFile = outputPathForDecryptedFile;

                //if file already exists, no need to fetch from storage and decrypt => saves time
                if (File.Exists(_outputPathForDecryptedFile))
                {
                    ImageView = ImageSource.FromFile(_outputPathForDecryptedFile);
                    ActivityIndicator = false;
                    BtnDownload = true;
                    BtnDelete = true;
                    return;
                }

                //decrypt and download file
                FileDecrypt(outputPathForEncryptedFile, outputPathForDecryptedFile);
            }
            catch
            {
                await App.Current.MainPage.DisplayAlert("Preview Error", "Please try again", "Ok");
                await App.Current.MainPage.Navigation.PopAsync();
            }
            
           
        }

        //delete image if download button not clicked
        private async void CheckDownloadButtonState()
        {
            if (_btnDownloadClicked == false)
            {
                File.Delete(_outputPathForDecryptedFile);
            }
        }

        public Command BtnDownloadCommand
        {
            get
            {
                return new Command(BtnDownloadClicked);
            }
        }

        private async void BtnDownloadClicked()
        {
            _btnDownloadClicked = true;
            await App.Current.MainPage.DisplayAlert("Success", "Image saved to gallery", "OK");
        }

        public Command BtnDeleteCommand
        {
            get
            {
                return new Command(BtnDeleteClicked);
            }
        }

        private async void BtnDeleteClicked()
        {
            try
            {
                BtnDelete = false;
                BtnDownload = false;
                ActivityIndicator = true;
                await FirebaseHelper.DeleteImage(_imageObject.FileName, _imageObject.Userid);
                await FirebaseHelper.DeleteImageObject(_imageObject.FileName, _imageObject.Userid);
                ActivityIndicator= false;
                //Message centre sends a message to ListViewPage telling it to refresh the list view so that the deleted file isn't shown in list view anymore
                MessagingCenter.Send<FullViewVM>(this, "RefreshPage");
                await App.Current.MainPage.DisplayAlert("Success", "File was deleted", "OK");
                await App.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error deleting file", "Please try again", "OK");
            }
        }

    }


}
