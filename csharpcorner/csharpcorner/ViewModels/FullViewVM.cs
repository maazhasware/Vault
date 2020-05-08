using Android.Media;
using csharpcorner.Models;
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
        private bool _btnDelete = true;
        private bool _btnDownload = true;
        private bool _activityIndicator;
        private string _outputPath = string.Empty;
        private ImageSource _imageView = string.Empty;
        private string _email;
        private ImageObject _imageObject;
        public event PropertyChangedEventHandler PropertyChanged;

        public FullViewVM(ImageObject imageObject, string email)
        {
            this._email = email;
            this._imageObject = imageObject;
            Download();
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
                await App.Current.MainPage.DisplayAlert("Error", "CryptographicException error: " + ex_CryptographicException.Message, "Ok");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Error: " + ex.Message, "Ok");
            }
            try
            {
                cs.Close();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Error by closing CryptoStream: " + ex.Message, "Ok");
            }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }

            //added bit
            string galleryPath1 = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
            string outputPath1 = Path.Combine(galleryPath1 + "/Vault/", _imageObject.FileName);
            _outputPath = outputPath1;

            //to show pics in gallery
            MediaScannerConnection.ScanFile(Android.App.Application.Context, new string[] { _outputPath }, new string[] { "image / jpg", "image/ png", "image /jpeg" }, null);

            //display image in image preview
            ImageView = ImageSource.FromFile(_outputPath);

            //delete extra file we downloaded
            File.Delete(inputFile);
        }

        private async void Download()
        {
            //output path for encrypted file
            string galleryPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
            string filename = _imageObject.FileName;
            //remove file extension
            int index = filename.LastIndexOf(".");
            if (index > 0)
            {
                filename = filename.Substring(0, index);
            }
            string outputPath = Path.Combine(galleryPath + "/Vault/", filename);
            //download encyrpted file
            using (var client = new WebClient())
            {
                client.DownloadFile(_imageObject.Url, outputPath);
            }

            //output path for decrypted file
            string galleryPath1 = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
            string outputPath1 = Path.Combine(galleryPath1 + "/Vault/", _imageObject.FileName);

            //decrypt and download file
            FileDecrypt(outputPath, outputPath1);
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
                await App.Current.MainPage.DisplayAlert("Success", "Deleted", "OK");
                await App.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Error deleting the file", "OK");
            }
        }

    }


}
