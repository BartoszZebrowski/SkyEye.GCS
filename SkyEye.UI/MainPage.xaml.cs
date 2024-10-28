using SkyEye.Connector;

namespace SkyEye.UI
{
    public partial class MainPage : ContentPage
    {
        VideoReciver _videoReciver;
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            _videoReciver = new VideoReciver("192.168.1.42", 9002);
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {





        }
    }
}
