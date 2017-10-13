﻿using Acr.UserDialogs;
using Hodlr.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace Hodlr.Pages
{
    public class MainPage : ContentPage
    {
        private Label userValueLabel;
        private Label fiatValueLabel;
        private Label profitLabel;
        private Picker fiatPicker;
        private ScrollView scroller;
        private bool loaded = false;
        private ListView listView;
        private double width = 0;
        private double height = 0;
        private ObservableCollection<WrappedCell<Transaction>> WrappedItems = new ObservableCollection<WrappedCell<Transaction>>();

        public MainPage()
        {
            Title = "To the Moon!";

            ToolbarItem refresh = new ToolbarItem
            {
                Icon = "ic_refresh_white_24dp",
                Priority = (int)ToolbarItemOrder.Primary
            };
            refresh.Clicked += Refresh_Clicked;
            ToolbarItems.Add(refresh);

            userValueLabel = new Label
            {
                FontSize = 30,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Margin = new Thickness(10),
                Text = "Loading"
            };
            profitLabel = new Label
            {
                FontSize = 27,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Loading"
            };
            fiatValueLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Loading"
            };

            fiatPicker = new Picker
            {
                Title = "Choose Fiat Currency",
                HorizontalOptions = LayoutOptions.Center
            };
            fiatPicker.SelectedIndexChanged += FiatPicker_SelectedIndexChanged;

            scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };

            Button addButton = new Button { Text = "Add Transaction" };
            addButton.Clicked += (a,b)=>{ Navigation.PushAsync(new AddTransactionPage()); };

            listView = new ListView();
            listView.ItemsSource = WrappedItems;
            listView.HasUnevenRows = true;
            listView.VerticalOptions = LayoutOptions.FillAndExpand;
            listView.ItemTemplate = new DataTemplate(typeof(TransactionCell));
            listView.ItemSelected += ListView_ItemSelected;
            scroller.Content = listView;

            StackLayout profitSection = new StackLayout
            {
                Spacing = 15,
                Padding = new Thickness(30, 10),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = {
                    userValueLabel,
                    profitLabel,
                    fiatValueLabel,
                    fiatPicker
                }
            };

            StackLayout transactionsSection = new StackLayout
            {
                Spacing = 15,
                Padding = new Thickness(30, 10),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new Label
                    {
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = "Transactions:",
                        HorizontalOptions = LayoutOptions.Center
                    },
                    scroller,
                    addButton
                }
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    profitSection,
                    transactionsSection
                }
            };

            GetVals();
        }

        private void Refresh_Clicked(object sender, EventArgs e)
        {
            GetVals();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;

                ((StackLayout)Content).Orientation = (width > height)? StackOrientation.Horizontal : StackOrientation.Vertical;
            }
        }

        private async void GetVals()
        {
            loaded = false;
            while(!loaded)
            {
                UserDialogs.Instance.ShowLoading(title: "Getting Data");
                bool success = await AppUtils.RefreshVals();
                UserDialogs.Instance.HideLoading();

                if (!success || App.FiatConvert == null || App.FiatValues == null)
                {
                    bool existingData = App.FiatConvert != null && App.FiatValues != null;

                    if (existingData)
                    {
                        await DisplayAlert("Error", "Error fetching latest price data. Using cached data.", "Got it");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Error fetching latest price data.", "Retry");
                        continue;
                    }
                }

                loaded = true;
            }

            if(App.FiatValues != null)
            {
                SetupPicker();
                LoadTransactions();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(loaded)
            {
                LoadTransactions();
            }
        }

        private void LoadTransactions()
        {
            var transactions = App.db.GetTransactions().ToList();
            if (WrappedItems == null)
            {
                WrappedItems = new ObservableCollection<WrappedCell<Transaction>>
                (transactions.Select(item => new WrappedCell<Transaction>()
                {
                    Item = item,
                    IsSelected = false
                }).ToList());
            }
            else
            {
                var copy = new ObservableCollection<WrappedCell<Transaction>>(WrappedItems);
                var toRemove = copy.Where(wc => !transactions.Any(t => t.Id == wc.Item.Id)).ToList();
                var toAdd = transactions.Where(t => !copy.Any(wc => wc.Item.Id == t.Id));

                foreach (var rem in toRemove) WrappedItems.Remove(rem);
                foreach (var add in toAdd) WrappedItems.Add(new WrappedCell<Transaction>()
                {
                    Item = add,
                    IsSelected = false
                });
            }
            
            UpdateLabels();
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            ((ListView)sender).SelectedItem = null;

            Transaction chosen = ((WrappedCell<Transaction>)e.SelectedItem).Item;

            var answer = await DisplayAlert("Delete Transaction", "Do you want to delete this transaction?", "Yes", "No");
            if (answer)
            {
                App.db.DeleteTransaction(chosen.Id);
                LoadTransactions();
            }
        }

        private void SetupPicker()
        {
            string current = null;
            if(fiatPicker.Items?.Count > 0)
            {
                current = fiatPicker.Items[fiatPicker.SelectedIndex];
            }

            if(App.FiatValues != null)
            {
                fiatPicker.ItemsSource = App.FiatValues.Keys.OrderBy(k => k).ToList();
            }

            current = (current != null)? current : App.FiatPref;
            fiatPicker.SelectedIndex = fiatPicker.Items.IndexOf(current);
        }

        private void FiatPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fiatPicker.Items == null ||
                fiatPicker.SelectedIndex == -1 ||
                fiatPicker.SelectedIndex > fiatPicker.Items.Count) return;
            App.FiatPref = fiatPicker.Items[fiatPicker.SelectedIndex];
            AppUtils.SaveCache();
            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (App.FiatValues == null) return;

            FiatValue fiat = App.FiatValues[App.FiatPref];
            List<Transaction> transactions = App.db.GetTransactions().ToList();
            double totalBtc = 0;
            double totalFiat = 0;

            foreach (var tr in transactions)
            {
                double thisFiat = AppUtils.ConvertFiat(tr.FiatCurrency, App.FiatPref, tr.FiatValue);

                if (tr.AcquireBtc)
                {
                    totalBtc += tr.BtcAmount;
                    totalFiat -= thisFiat;
                }
                else
                {
                    totalBtc -= tr.BtcAmount;
                    totalFiat += thisFiat;
                }
            }
            
            double btcFiatVal = totalBtc * fiat.Delayed;
            double profit = totalFiat + btcFiatVal;

            fiatValueLabel.Text = string.Format("{0:0.00000000} BTC at {1}{2:0.00} per coin", totalBtc, fiat.Symbol, fiat.Delayed);
            userValueLabel.Text = string.Format("{0}{1:0.00}", fiat.Symbol, btcFiatVal);

            string profLoss = (profit >= 0) ? "Profit" : "Loss";
            profitLabel.TextColor = (profit >= 0) ? Color.ForestGreen : Color.IndianRed;
            profitLabel.Text = string.Format("{0}: {1}{2:0.00}", profLoss, fiat.Symbol, Math.Abs(profit));
        }
    }
}