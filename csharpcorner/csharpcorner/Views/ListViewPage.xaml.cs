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
	public partial class ListViewPage : ContentPage
	{
        string localEmail = string.Empty;
        
        public ListViewPage (string email)
		{
		    InitializeComponent();
            localEmail = email;
            BindingContext = new ListViewVM(email);
            
        }

        void OnListViewItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ImageObject selectedItem = e.SelectedItem as ImageObject;
        }

        void OnListViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            ImageObject tappedItem = e.Item as ImageObject;
            App.Current.MainPage.Navigation.PushAsync(new FullView(tappedItem, localEmail));
        }


    }
}