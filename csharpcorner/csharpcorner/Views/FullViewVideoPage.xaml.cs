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
	public partial class FullViewVideoPage : ContentPage
	{
		public FullViewVideoPage (VideoObject videoObject, string email)
		{
			InitializeComponent ();
            BindingContext = new FullViewVideoVM(videoObject, email);
		}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //Messaging centre sends message to FullViewVideoPage so that video loads after page load
            MessagingCenter.Send<FullViewVideoPage>(this, "LoadPage");
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //Messaging centre sends message to check download button state
            MessagingCenter.Send<FullViewVideoPage>(this, "BackButtonPressed");
        }

    }
}