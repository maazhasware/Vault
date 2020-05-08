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
        private string videoUrl = "https://sec.ch9.ms/ch9/e68c/690eebb1-797a-40ef-a841-c63dded4e68c/Cognitive-Services-Emotion_high.mp4";

        public UploadVideoPage(string email)
        {
            InitializeComponent();
            BindingContext = new UploadVideoVM(email);
        }

        //private void PlayStopButton(object sender, EventArgs e)
        //{
        //    if (PlayStopButtonText.Text == "Play")
        //    {
        //        CrossMediaManager.Current.Play(videoUrl);

        //        PlayStopButtonText.Text = "Stop";
        //    }
        //    else if (PlayStopButtonText.Text == "Stop")
        //    {
        //        CrossMediaManager.Current.Play(videoUrl);

        //        PlayStopButtonText.Text = "Play";
        //    }
        //}
    }
}