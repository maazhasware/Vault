using csharpcorner.ViewModels;
using MediaManager;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace csharpcorner.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UploadVideoPage : ContentPage
	{
        public UploadVideoPage(string email)
        {
            InitializeComponent();
            BindingContext = new UploadVideoVM(email);
        }

    }
}