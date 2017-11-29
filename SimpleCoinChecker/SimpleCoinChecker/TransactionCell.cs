using Hodlr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Hodlr
{
    public class TransactionCell : ViewCell
    {
        private Label FiatAmountLabel;
        private Label BtcAmountLabel;
        private Label DateLabel;
        private Label CurrentAmountLabel;

        public TransactionCell()
        {
            FiatAmountLabel = new Label
            {
                FontSize = 19,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            BtcAmountLabel = new Label
            {
                FontSize = 15,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.EndAndExpand
            };
            DateLabel = new Label
            {
                FontSize = 15,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start
            };
            CurrentAmountLabel = new Label
            {
                FontSize = 19,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.EndAndExpand
            };

            View = new StackLayout
            {
                Padding = new Thickness(15, 10),
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        HorizontalOptions = LayoutOptions.Start,
                        Children =
                        {
                            FiatAmountLabel,
                            BtcAmountLabel
                        }
                    },
                    new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        Children =
                        {
                            DateLabel,
                            CurrentAmountLabel
                        }
                    }
                }
            };
        }

        protected override void OnBindingContextChanged()
        {
            var c = (WrappedCell<Transaction>)BindingContext;
            if (c == null || c.Item == null) return;

            FiatAmountLabel.Text = AppUtils.GetMoneyString(c.Item.FiatValue, c.Item.FiatCurrency);

            CurrentAmountLabel.Text = AppUtils.GetMoneyString(
                AppUtils.GetFiatValOfCrypto(c.Item.FiatCurrency, c.Item.CryptoCurrency, c.Item.CryptoAmount),
                c.Item.FiatCurrency);

            BtcAmountLabel.Text = string.Format("{0} {1:0.0000000} {2}", 
                (c.Item.AcquireCrypto)? "Bought" : "Sold",
                c.Item.CryptoAmount,
                c.Item.CryptoCurrency);
            DateLabel.Text = string.Format("Made on {0:dd/MM/yy}", c.Item.CreatedAt);

            base.OnBindingContextChanged();
        }

    }
}
