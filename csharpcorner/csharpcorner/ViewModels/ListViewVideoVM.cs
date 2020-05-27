using csharpcorner.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace csharpcorner.ViewModels
{
    public class ListViewVideoVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _email;
        private IList<VideoObject> _videosList;
        private bool _isRefreshing = false;
        public ListViewVideoVM(string email)
        {
            this._email = email;
            SetupListViewItems();
            //Subscribe to event from FullViewVideoVM delete button, used to refresh list view so deleted item not still displayed
            MessagingCenter.Subscribe<FullViewVideoVM>(this, "RefreshPage", (sender) =>
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

        public IList<VideoObject> VideosList
        {
            get { return _videosList; }
            set
            {
                _videosList = value;
                PropertyChanged(this, new PropertyChangedEventArgs("VideosList"));
            }
        }

        private async void SetupListViewItems()
        {
            User user = await FirebaseHelper.GetUser(_email);
            VideosList = await FirebaseHelper.GetUsersVideoObjects(user.UserID);
        }



    }
}
