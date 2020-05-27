using csharpcorner.Models;
using csharpcorner.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace csharpcorner.ViewModels
{
    public class ListViewVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _email;
        private IList<ImageObject> _imagesList;
        private bool _isRefreshing = false;
        public ListViewVM(string email)
        {
            this._email = email;
            SetupListViewItems();
            //Subscribe to event from FullViewVM delete button, used to refresh list view so deleted item not still displayed
            MessagingCenter.Subscribe<FullViewVM>(this, "RefreshPage", (sender) =>
            {
                SetupListViewItems();
            });
        }

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsRefreshing"));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(() =>
                {
                    IsRefreshing = true;

                    SetupListViewItems();

                    IsRefreshing = false;
                });
            }
        }

        public IList<ImageObject> ImagesList
        {
            get { return _imagesList; }
            set
            {
                _imagesList = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ImagesList"));
            }
        }

        private async void SetupListViewItems()
        {
            User user = await FirebaseHelper.GetUser(_email);
            ImagesList  = await FirebaseHelper.GetUsersImageObjects(user.UserID);
        }

    }
}
