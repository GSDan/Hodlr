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
        private Entry btcAmountEntry;
        private Picker fiatPicker;
        private DatePicker datePicker;

        private Dictionary<bool, string> switchLabelVals = new Dictionary<bool, string>();

        public AddTransactionPage()
        {
            Title = "Add a new transaction";

            fiatPicker = new Picker
            {
                Title = "Choose currency"
            };
            fiatPicker.ItemsSource = App.FiatValues.Keys.OrderBy(k => k).ToList();
            fiatPicker.SelectedItem = App.FiatPref;
            fiatPicker.SelectedIndexChanged += FiatPicker_SelectedIndexChanged;

            switchLabelVals.Add(true, "Purchasing Bitcoin");
            switchLabelVals.Add(false, "Selling Bitcoin");

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

            btcAmountEntry = new Entry
            {
                Placeholder = "Bitcoin amount",
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
                        new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            Children =
                            {
                                typeLabel,
                                typeSwitch
                            }
                        },
                        new Label { Text = "Choose currency:", FontSize = 15 },
                        fiatPicker,
                        new Label { Text = "Amount of fiat exchanged:", FontSize = 15 },
                        fiatAmountEntry,
                        new Label { Text = "Amount of BTC exchanged:", FontSize = 15 },
                        btcAmountEntry,
                        new Label { Text = "Transaction date:", FontSize = 15 },
                        datePicker,
                        submitButton
                    }
                }
            };
        }

        private void TypeSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            typeLabel.Text = switchLabelVals[typeSwitch.IsToggled];
        }

        private void FiatPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateBtcLabel();
        }

        private void FiatAmountEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateBtcLabel();
        }

        private void UpdateBtcLabel()
        {
            if(string.IsNullOrWhiteSpace(fiatAmountEntry.Text))
            {
                btcAmountEntry.Text = "0";
            }
            else
            {
                btcAmountEntry.Text = AppUtils.GetBtcValOfFiat(fiatPicker.SelectedItem.ToString(),
                    Double.Parse(fiatAmountEntry.Text)).ToString();
            }
        }

        private void SubmitButton_Clicked(object sender, EventArgs e)
        {
            App.db.AddOrUpdateTransaction(new Transaction
            {
                BtcAmount = Double.Parse(btcAmountEntry.Text),
                FiatValue = Double.Parse(fiatAmountEntry.Text),
                FiatCurrency = fiatPicker.SelectedItem.ToString(),
                CreatedAt = datePicker.Date,
                AcquireBtc = typeSwitch.IsToggled
            });

            Navigation.PopAsync();
        }
    }
}