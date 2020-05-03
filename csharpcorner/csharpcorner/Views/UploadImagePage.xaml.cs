using csharpcorner.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace csharpcorner.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UploadImagePage : ContentPage
	{
        public UploadImagePage (string email)
		{
			InitializeComponent ();
            BindingContext = new UploadImageVM(email);
		}

    }
}