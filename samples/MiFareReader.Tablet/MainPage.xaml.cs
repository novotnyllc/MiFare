using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
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
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;
using SmartCardReader = MiFare.Devices.SmartCardReader;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MiFareReader.Tablet
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SmartCardReader reader;
        private MiFareCard card;
        

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
                reader = CardReader.Find();
                if (reader == null)
                {
                    PopupMessage("No Readers Found");
                    return;
                }

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
            Debug.WriteLine("Card Removed");
            card?.Dispose();
            card = null;

            ChangeTextBlockFontColor(TextBlock_Header, Windows.UI.Colors.Red);
        }

        private async void CardAdded(object sender, CardEventArgs args)
        {
            Debug.WriteLine("Card Added");
            try
            {
                ChangeTextBlockFontColor(TextBlock_Header, Windows.UI.Colors.Green);
                await HandleCard(args);
            }
            catch (Exception ex)
            {
                PopupMessage("CardAdded Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Sample code to hande a couple of different cards based on the identification process
        /// </summary>
        /// <returns>None</returns>
        private async Task HandleCard(CardEventArgs args)
        {
            try
            {
                card?.Dispose();
                card = args.SmartCard.CreateMiFareCard();

                var localCard = card;

                var cardIdentification = await localCard.GetCardInfo();


                DisplayText("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() + "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass
                     && (cardIdentification.PcscCardName == CardName.MifareStandard1K || cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    // Handle MIFARE Standard/Classic
                    DisplayText("MIFARE Standard/Classic card detected");

                    var uid = await localCard.GetUid();
                    DisplayText("UID:  " + BitConverter.ToString(uid));

                    // 16 sectors, print out each one
                    for (var sector = 0; sector < 16 && card != null; sector++)
                    {
                        try
                        {
                            var data = await localCard.GetData(sector, 0, 48);

                            string hexString = "";
                            for (int i = 0; i < data.Length; i++)
                            {
                                hexString += data[i].ToString("X2") + " ";
                            }

                            DisplayText(string.Format("Sector '{0}':{1}", sector, hexString));

                        }
                        catch (Exception)
                        {
                            DisplayText("Failed to load sector: " + sector);
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                PopupMessage("HandleCard Exception: " + e.Message);
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
        /// Change text of UI textbox
        /// </summary>
        /// <returns>None</returns>
        private void DisplayText(string message)
        {
            Debug.WriteLine(message);
            var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtLog.Text += message + Environment.NewLine;
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
