using Hodlr.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace Hodlr.Pages
{
    public class AddTransactionPage : ContentPage
    {
        private Label typeLabel;
        private Switch typeSwitch;
        private Entry fiatAmountEntry;
        private Entry cryptoAmountEntry;
        private Picker fiatPicker;
        private Picker cryptoPicker;
        private DatePicker datePicker;
        private Label cryptoAmountPrompt;

        private Dictionary<bool, string> switchLabelVals = new Dictionary<bool, string>();

        public AddTransactionPage()
        {
            Title = "Add a new transaction";

            fiatPicker = new Picker
            {
                Title = "Choose currency"
            };
            fiatPicker.ItemsSource = AppUtils.GetCurrencies();
            fiatPicker.SelectedItem = AppUtils.FiatPref;
            fiatPicker.SelectedIndexChanged += FiatPicker_SelectedIndexChanged;

            cryptoPicker = new Picker
            {
                Title = "Choose crypto currency"
            };
            cryptoPicker.ItemsSource = AppUtils.CryptoCurrencies;
            cryptoPicker.SelectedItem = AppUtils.CryptoCurrencies[0];
            cryptoPicker.SelectedIndexChanged += CryptoPicker_SelectedIndexChanged;

            switchLabelVals.Add(true, "Buying " + cryptoPicker.SelectedItem.ToString());
            switchLabelVals.Add(false, "Selling " + cryptoPicker.SelectedItem.ToString());

            typeLabel = new Label
            {
                Text = switchLabelVals[true],
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 15
            };
            typeSwitch = new Switch
            {
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
                IsToggled = true
            };
            typeSwitch.Toggled += TypeSwitch_Toggled;

            fiatAmountEntry = new Entry
            {
                Placeholder = "Fiat amount",
                Keyboard = Keyboard.Numeric
            };
            fiatAmountEntry.TextChanged += FiatAmountEntry_TextChanged;

            cryptoAmountPrompt = new Label {
                Text = string.Format("Amount of {0} exchanged:", cryptoPicker.SelectedItem.ToString()),
                FontSize = 15 };

            cryptoAmountEntry = new Entry
            {
                Placeholder = string.Format("{0} amount", cryptoPicker.SelectedItem.ToString()),
                Keyboard = Keyboard.Numeric
            };

            datePicker = new DatePicker
            {
                Format = "dd MMMM yyyy",
                MaximumDate = DateTime.Now,
                MinimumDate = DateTime.Parse("01 Jan 2009 00:00:00 GMT"),
                Date = DateTime.Now
            };

            Button submitButton = new Button
            {
                Text = "Add Transaction"
            };
            submitButton.Clicked += SubmitButton_Clicked;

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Spacing = 10,
                    Padding = new Thickness(30),
                    Children =
                    {
                        new Label { Text = "Choose currency:", FontSize = 15 },
                        fiatPicker,
                        new Label { Text = "Choose crypto:", FontSize = 15 },
                        cryptoPicker,
                        new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Children =
                            {
                                typeLabel,
                                typeSwitch
                            }
                        },
                        new Label { Text = "Amount of fiat exchanged:", FontSize = 15 },
                        fiatAmountEntry,
                        cryptoAmountPrompt,
                        cryptoAmountEntry,
                        new Label { Text = "Transaction date:", FontSize = 15 },
                        datePicker,
                        submitButton
                    }
                }
            };
        }

        private void CryptoPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            cryptoAmountEntry.Placeholder = string.Format("{0} amount", cryptoPicker.SelectedItem.ToString());
            switchLabelVals[true] = "Buying " + cryptoPicker.SelectedItem.ToString();
            switchLabelVals[false] = "Selling " + cryptoPicker.SelectedItem.ToString();
            typeLabel.Text = switchLabelVals[typeSwitch.IsToggled];
            cryptoAmountPrompt.Text = string.Format("Amount of {0} exchanged:", cryptoPicker.SelectedItem.ToString());
        }

        private void TypeSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            typeLabel.Text = switchLabelVals[typeSwitch.IsToggled];
        }

        private void FiatPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCryptoLabel();
        }

        private void FiatAmountEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCryptoLabel();
        }

        private void UpdateCryptoLabel()
        {
            if(string.IsNullOrWhiteSpace(fiatAmountEntry.Text))
            {
                cryptoAmountEntry.Text = "0";
            }
            else
            {
                if (!Double.TryParse(fiatAmountEntry.Text, out double fiatAmount)) fiatAmount = 0;

                cryptoAmountEntry.Text = AppUtils.GetCryptoValOfFiat(fiatPicker.SelectedItem.ToString(),
                    cryptoPicker.SelectedItem.ToString(),
                    fiatAmount).ToString();
            }
        }

        private void SubmitButton_Clicked(object sender, EventArgs e)
        {
            if (!Double.TryParse(fiatAmountEntry.Text, out double fiatAmount)) fiatAmount = 0;
            if (!Double.TryParse(cryptoAmountEntry.Text, out double btcAmount)) btcAmount = 0;

            App.DB.AddTransaction(new Transaction
            {
                CryptoAmount = btcAmount,
                FiatValue = fiatAmount,
                FiatCurrency = fiatPicker.SelectedItem.ToString(),
                CreatedAt = datePicker.Date,
                AcquireCrypto = typeSwitch.IsToggled,
                CryptoCurrency = cryptoPicker.SelectedItem.ToString(),
                DataVersion = Transaction.CurrentDataVersion
            });

            Navigation.PopAsync();
        }
    }
}