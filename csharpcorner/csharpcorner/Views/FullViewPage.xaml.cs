using Android.Content;
using Android.Provider;
using csharpcorner.Models;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using csharpcorner.ViewModels;
using System.Security.Cryptography;
using System.Net;
using Android.Media;

namespace csharpcorner.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FullView : ContentPage
    {
        public FullView(ImageObject imageObject, string email)
        {
            InitializeComponent();
            BindingContext = new FullViewVM(imageObject, email);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //Messaging centre sends message
            MessagingCenter.Send<FullView>(this, "LoadPage");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //Messaging centre sends message to check download button state
            MessagingCenter.Send<FullView>(this, "BackButtonPressed");
        }

    }
}