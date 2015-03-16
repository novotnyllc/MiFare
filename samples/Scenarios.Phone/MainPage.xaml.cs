using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Scenarios.Auth;
using Scenarios.Tickets;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Scenarios.Phone
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }


        private void OnSetClicked(object sender, RoutedEventArgs e)
        {
            CurrentStateService.Instance.Pin = PinBox.Text;
            CurrentStateService.Instance.Data = DataBox.Text;
            CurrentStateService.Instance.Mode = CardMode.SetPin;

            App.RootFrame.Navigate(typeof(CardReaderPage));
        }

        private void OnValidateClicked(object sender, RoutedEventArgs e)
        {
            CurrentStateService.Instance.Pin = PinBox.Text;
            CurrentStateService.Instance.Data = string.Empty;
            CurrentStateService.Instance.Mode = CardMode.ReadData;

            App.RootFrame.Navigate(typeof(CardReaderPage));
        }

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            CurrentStateService.Instance.Pin = string.Empty;
            CurrentStateService.Instance.Data = string.Empty;
            CurrentStateService.Instance.Mode = CardMode.Reset;

            App.RootFrame.Navigate(typeof(CardReaderPage));
        }

        private void OnWriteTicketClicked(object sender, RoutedEventArgs e)
        {
            TicketState.Instance.Mode = Mode.Cashier;
            TicketState.Instance.TicketData = TicketData.Date.ToString("O");

            App.RootFrame.Navigate(typeof(TicketCardReader));
        }

        private void OnSkiLiftClicked(object sender, RoutedEventArgs e)
        {
            TicketState.Instance.Mode = Mode.SkiLift;
            TicketState.Instance.TicketData = string.Empty;

            App.RootFrame.Navigate(typeof(TicketCardReader));
        }

        private void OnResetSkiClicked(object sender, RoutedEventArgs e)
        {
            TicketState.Instance.Mode = Mode.Reset;
            TicketState.Instance.TicketData = string.Empty;

            App.RootFrame.Navigate(typeof(TicketCardReader));
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
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
