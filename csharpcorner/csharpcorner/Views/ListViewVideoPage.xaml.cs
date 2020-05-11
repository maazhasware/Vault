using csharpcorner.Models;
using csharpcorner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace csharpcorner.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ListViewVideoPage : ContentPage
	{
        string localEmail = string.Empty;
		public ListViewVideoPage (string email)
		{
			InitializeComponent ();
            localEmail = email;
            BindingContext = new ListViewVideoVM(email);
		}

        void OnListViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            VideoObject tappedItem = e.Item as VideoObject;
            App.Current.MainPage.Navigation.PushAsync(new FullViewVideoPage(tappedItem, localEmail));
        }

    }
}