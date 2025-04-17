using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace moneyConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string _BNPLink = "https://static.nbp.pl/dane/kursy/xml/LastA.xml";
        List<ListaA> aktualneKursy = new List<ListaA>();
        private ListaA selectedEntry;
        private ListaA selectedOutput;
        public MainPage()
        {
            this.InitializeComponent();

        }

        private void GoToAbout_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(aboutProgram));
        }

        private void entryValueTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = (TextBox)sender;
            if (textbox.Text != "")
            {
                if (!Regex.IsMatch(textbox.Text, @"^\d*[.,]?\d*$"))
                {
                    int txtPos = textbox.SelectionStart - 1;
                    textbox.Text = textbox.Text.Remove(txtPos, 1);
                    textbox.SelectionStart = txtPos;

                }

                char ch = textbox.Text.Last();
                if (ch == ',')
                {
                    textbox.Text = textbox.Text.Remove(textbox.Text.Length - 1);
                    textbox.Text += ".";
                }
                if (textbox.Text == ".")
                {
                    textbox.Text = "0.";
                }
                textbox.Select(textbox.Text.Length, 0);
            }

            convert();

        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                var listonosz = new HttpClient();
                var dane = await listonosz.GetStringAsync(new Uri(_BNPLink));

                var daneXml = XDocument.Parse(dane);
                var listaPozycji = from item in daneXml.Descendants("pozycja")
                                   select new ListaA()
                                   {
                                       przelicznik = item.Element("przelicznik").Value,
                                       kodWaluty = item.Element("kod_waluty").Value,
                                       kursSredni = item.Element("kurs_sredni").Value.Replace(',', '.')
                                   };
                aktualneKursy = listaPozycji.ToList();

                aktualneKursy.Insert(0,
                    new ListaA()
                    {
                        przelicznik = "1",
                        kodWaluty = "PLN",
                        kursSredni = "1.0000"
                    }
                );


                foreach (var item in aktualneKursy)
                {
                    entryValueComboBox.Items.Add(string.Concat(item.kodWaluty + " | " + item.kursSredni));
                    outputValueComboBox.Items.Add(string.Concat(item.kodWaluty + " | " + item.kursSredni));
                }

            } catch (Exception exeption)
            {
                Console.WriteLine(exeption.Message);
                ContentDialog dialog = new ContentDialog
                {
                    Content = "Can't access BNP data, check your internet connection",
                    CloseButtonText = "OK"
                };
                dialog.CloseButtonClick += (sender2, args) =>
                {
                    ExitButtonDialog();
                };
                await dialog.ShowAsync();
            }
        }

        private void entryValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string temp = entryValueComboBox.SelectedItem.ToString();
            temp = temp.Split("|")[0];
            temp = temp.Trim();
            for (int i = 0; i < aktualneKursy.Count(); i++)
            {
                ListaA currentItem = aktualneKursy[i];
                if(temp == currentItem.kodWaluty)
                {
                    selectedEntry = currentItem;
                    break;
                }
            }
            convert();
        }

        private void outputValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string temp = outputValueComboBox.SelectedItem.ToString();
            temp = temp.Split("|")[0];
            temp = temp.Trim();
            for (int i = 0; i < aktualneKursy.Count(); i++)
            {
                ListaA currentItem = aktualneKursy[i];
                if (temp == currentItem.kodWaluty)
                {
                    selectedOutput = currentItem;
                    break;
                }
            }
            convert();
        }

        private void convert()
        {
            if (entryValueTxtBox.Text != null && entryValueTxtBox.Text != "")
            {
                if (selectedEntry != null && selectedEntry.kodWaluty != "PLN" && selectedOutput != null)
                {
                    double exchange = double.Parse(entryValueTxtBox.Text) * double.Parse(selectedEntry.kursSredni) / double.Parse(selectedEntry.przelicznik);
                    exchange = exchange / (double.Parse(selectedOutput.kursSredni) * double.Parse(selectedOutput.przelicznik));
                    outputValueTxtBox.Text = Math.Round(exchange, 4).ToString();
                }
                else if (selectedEntry != null && selectedOutput != null )
                {
                    double exchange = double.Parse(entryValueTxtBox.Text) / (double.Parse(selectedOutput.kursSredni) / double.Parse(selectedOutput.przelicznik));
                    outputValueTxtBox.Text = Math.Round(exchange, 4).ToString();
                }
            }
            else if (entryValueTxtBox.Text == "")
            {
                outputValueTxtBox.Text = "0";
            }
        }

        private void ExitButtonDialog()
        {
            CoreApplication.Exit();
        }

    }
}
