using Microsoft.Band;
using Microsoft.Band.Sensors;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Text;
using Windows.Web.Http;
using Windows.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace HeartSenson.WindowsPhone
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public IBandInfo[] pairedBands;
        public IBandClient bandClient;
        public IBandHeartRateReading heartreading;
        private BandInformation currentBand;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);
            
            ConnectWithBand();
        }

        async void App_Suspending(Object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            await bandClient.SensorManager.HeartRate.StopReadingsAsync();
        }

        public async void ConnectWithBand()
        {
            pairedBands = await BandClientManager.Instance.GetBandsAsync();

            bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);

            if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }

            currentBand = new BandInformation();
            await currentBand.RetrieveInfo(pairedBands[0], bandClient);

            bandClient.SensorManager.HeartRate.ReadingChanged += HeartRateReceived;

            await bandClient.SensorManager.HeartRate.StartReadingsAsync();
        }

        async void HeartRateReceived(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            var currentHR = "";
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    currentHR = e.SensorReading.HeartRate.ToString();
                    heartT.Text = e.SensorReading.HeartRate.ToString();
                });

            PostTelemetryAsync(new BandEvent { SourceBand = currentBand, HR = currentHR, EventTime = DateTime.Now });
        }

        private async void PostTelemetryAsync(BandEvent deviceTelemetry)
        {
            
            try {

                var sas = "SharedAccessSignature sr=YourSAS";
                
                // Namespace info.
                var serviceNamespace = "ServiceBusNamespace";
                var hubName = "EventHubName";
                var publisher = "MSBand";
                var url = string.Format("{0}/publishers/{1}/messages", hubName, deviceTelemetry.SourceBand.Name);

                // Create client.
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", sas);
                
                var content = new HttpStringContent(JsonConvert.SerializeObject(deviceTelemetry));
                httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
              

                var result = await httpClient.PostAsync(new Uri(string.Format("https://{0}.servicebus.windows.net/{1}/publishers/{2}/messages", serviceNamespace, hubName, publisher)), content);

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        resultLog.Text = string.Format("Data sent at {0}", DateTime.Now);

                    });
            }
            catch(Exception e)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
               () =>
               {
                   resultLog.Text = e.ToString();

               });
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
