using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;

namespace MiFareReader.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SmartCardReader reader;
        private MiFareCard card;

        public MainWindow()
        {
            InitializeComponent();
            GetDevices();
        }


        /// <summary>
        /// Enumerates NFC reader and registers event handlers for card added/removed
        /// </summary>
        /// <returns>None</returns>
        private async void GetDevices()
        {
            try
            {
                reader = await CardReader.FindAsync();
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

            ChangeTextBlockFontColor(TextBlock_Header, Colors.Red);
        }

        private async void CardAdded(object sender, CardEventArgs args)
        {
            Debug.WriteLine("Card Added");
            try
            {
                ChangeTextBlockFontColor(TextBlock_Header, Colors.Green);
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
        private void ChangeTextBlockFontColor(TextBlock textBlock, Color color)
        {
            var ignored = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,(Action) (() =>
            {
                textBlock.Foreground = new SolidColorBrush(color);
            }));
        }

        /// <summary>
        /// Change text of UI textbox
        /// </summary>
        /// <returns>None</returns>
        private void DisplayText(string message)
        {
            Debug.WriteLine(message);

            var ignored = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                txtLog.Text += message + Environment.NewLine;
            }));
        }

        /// <summary>
        /// Display message via dialogue box
        /// </summary>
        /// <returns>None</returns>
        public void PopupMessage(string message)
        {
            var ignored = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                MessageBox.Show(message);
            }));
        }
    }
}
