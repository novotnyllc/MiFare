using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MiFare;
using MiFare.Devices;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MiFareReader.Tablet
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SmartCardReader reader;

        public MainPage()
        {
            this.InitializeComponent();
            GetDevices();
        }

        /// <summary>
        /// Enumerates NFC reader and registers event handlers for card added/removed
        /// </summary>
        /// <returns>None</returns>
        private void GetDevices()
        {
            try
            {
                var readerList = CardReader.GetReaderNames();
                
                if(readerList.Count == 0)
                {
                    PopupMessage("No Readers Found");
                    return;
                }

                reader = CardReader.Create(readerList.First());
                reader.CardAdded += CardAdded;
                reader.CardRemoved += CardRemoved;
            }
            catch (Exception e)
            {
                PopupMessage("Exception: " + e.Message);
            }
        }

        private void CardRemoved(object sender, EventArgs e)
        {
           // card?.Dispose();

            ChangeTextBlockFontColor(TextBlock_Header, Windows.UI.Colors.Red);
        }

        private void CardAdded(object sender, EventArgs e)
        {

            try
            {
                ChangeTextBlockFontColor(TextBlock_Header, Windows.UI.Colors.Green);
                //await HandleCard(args);
            }
            catch (Exception ex)
            {
                PopupMessage("CardAdded Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Changes font color of main application banner
        /// </summary>
        /// <returns>None</returns>
        private void ChangeTextBlockFontColor(TextBlock textBlock, Windows.UI.Color color)
        {
            var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBlock.Foreground = new SolidColorBrush(color);
            });
        }

        /// <summary>
        /// Display message via dialogue box
        /// </summary>
        /// <returns>None</returns>
        public async void PopupMessage(string message)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var dlg = new MessageDialog(message);
                await dlg.ShowAsync();
            });
        }
    }
}
