using Android.Webkit;
using csharpcorner.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Java.Net;
using Java.IO;
using csharpcorner.Models;

namespace csharpcorner.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WelcomePage : ContentPage
    {
        public WelcomePage(string email)
        {
            InitializeComponent();
            BindingContext = new WelcomeVM(email);

        }

    }
}