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
	public partial class SignUpPage : ContentPage
	{
        SignUpVM signUpVM;
        public SignUpPage ()
		{
			InitializeComponent ();
            signUpVM = new SignUpVM();
            //set binding    
            BindingContext = signUpVM;
        }
	}
}