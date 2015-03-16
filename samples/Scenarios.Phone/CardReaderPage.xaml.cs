using Scenarios.Phone.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Scenarios.Phone
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CardReaderPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        // TODO: Get this somewhere else -- do not store in the app/binary
        private readonly byte[] MasterKey = new byte[] { 0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef };

        private CardMode mode;

        public CardReaderPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void OnCardRemoved(object sender, EventArgs e)
        {
            Debug.WriteLine("Card Removed from range");
            ConnectionStatus.Text = "Not Connected";
        }

        private void OnCardAdded(object sender, EventArgs e)
        {
            Debug.WriteLine("Card detected in range");

            ConnectionStatus.Text = "Connected";
            HandleCard();
        }

        private async void HandleCard()
        {
            var success = true;
            try
            {

                switch (mode)
                {
                    case CardMode.SetPin:
                        AppendText("New PIN", false);
                        await OnNewPin(SmartCardFactory.GetSmartCardForProvisioning(MasterKey));
                        break;
                    case CardMode.ReadData:
                        AppendText("Validate PIN", false);
                        await OnValidatePin(SmartCardFactory.GetSmartCardForValidation(CurrentStateService.Instance.Pin));
                        break;
                    case CardMode.Reset:
                        await ResetToDefault(SmartCardFactory.GetSmartCardForProvisioning(MasterKey));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                success = false;
                Debug.WriteLine(e);

                AppendText(e.ToString());
            }

            if (success)
            {
                AppendText("Complete");
            }

            QuitButton.IsEnabled = true;
        }

        private async Task OnValidatePin(ISmartCard smartCard)
        {
            try
            {
                // try to get the data
                var data = await smartCard.GetData();
                // if we get here, we got the data
                AppendText("Successfully validated pin");
                AppendText(data);
            }
            catch (SmartCardException)
            {
                AppendText("Incorrect PIN");
            }
        }

        private async Task OnNewPin(ISmartCardAdmin smartCard)
        {
            AppendText("Initializing Sector");

            await smartCard.InitializeMasterKey();

            AppendText("Sector Initialized");

            // see if there's currently data there
            var currentData = await smartCard.GetData();
            // 

            if (string.IsNullOrWhiteSpace(currentData))
            {
                // Nothing written yet
                await DoSetPin(smartCard);
            }
            else
            {
                AppendText("Error: card already has data on it!");
            }
        }

        private async Task DoSetPin(ISmartCardAdmin smartCard)
        {
            await smartCard.SetUserPin(CurrentStateService.Instance.Pin, CurrentStateService.Instance.Data);
            AppendText("PIN set");

            // read the data back
            var sc = SmartCardFactory.GetSmartCardForValidation(CurrentStateService.Instance.Pin);
            var data = await sc.GetData();

            AppendText("Verified: " + data + " was written to the card.");
        }

        private async Task ResetToDefault(ISmartCardAdmin smartCard)
        {
            AppendText("Resetting to default");

            await smartCard.ResetToDefault();

            AppendText("Reset to default");
        }

        private void AppendText(string text, bool newLine = true)
        {
            Debug.WriteLine(text);
            Result.Text += (newLine ? Environment.NewLine : string.Empty) + text;
        }


        private void OnQuitClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            mode = CurrentStateService.Instance.Mode;

            await SmartCardFactory.Initialize(); // this will also clear any prev event handlers

            SmartCardFactory.CardAdded += OnCardAdded;
            SmartCardFactory.CardRemoved += OnCardRemoved;

            Debug.WriteLine("Subscribed from smartcard factory");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SmartCardFactory.CardAdded -= OnCardAdded;
            SmartCardFactory.CardRemoved -= OnCardRemoved;

            Debug.WriteLine("Unsubscribed from smartcard factory");

            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
