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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace moneyConverter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class aboutProgram : Page
    {
        public aboutProgram()
        {
            this.InitializeComponent();
            Data.Text = String.Concat("Dane z: ", MainPage.BNPDate);
            SourceValue.Text = String.Concat("Waluta wejsciowa: ", MainPage._selectedEntry?.nazwa.ToString());
            OutputValue.Text = String.Concat("Waluta wyjściowa: ", MainPage._selectedOutput?.nazwa.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }


    }
}
