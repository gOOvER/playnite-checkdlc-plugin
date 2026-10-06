using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using Playnite.SDK.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using CheckDlc.Services;

namespace CheckDlc.Models
{
    public class Dlc : ObservableObject
    {
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;
        
        [DontSerialize]
        public string Id => DlcId + "##" + Name;

        public string DlcId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }

        private bool _isOwned;
        public bool IsOwned
        {
            get => IsManualOwned || _isOwned;
            set => _isOwned = value;
        }

        [DontSerialize]
        public bool IsHidden => PluginDatabase?.PluginSettings?.Settings?.IgnoredList?.Contains(Id) ?? false;

        [DontSerialize]
        public bool IsManualOwned => PluginDatabase?.PluginSettings?.Settings?.ManuallyOwneds?.Contains(Id) ?? false;

        private string _price;
        public string Price
        {
            get => _price;
            set
            {
                _price = value;
                _priceNumeric = null;
            }
        }

        private string _priceBase;
        public string PriceBase
        {
            get => _priceBase;
            set
            {
                _priceBase = value;
                _priceBaseNumeric = null;
            }
        }

        private static readonly Regex SplitWhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        private static double ParsePrice(string rawPrice)
        {
            if (rawPrice.IsNullOrEmpty())
            {
                return 0;
            }

            string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string temp = rawPrice.Replace(",--", string.Empty).Replace(".--", string.Empty).Replace(sep + "--", string.Empty);
            string[] parts = SplitWhitespaceRegex.Split(temp).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (parts.Length == 0)
            {
                return 0;
            }

            string first = parts[0].Replace(".", sep).Replace(",", sep);
            first = Regex.Replace(first, @"[^\d" + Regex.Escape(sep) + "-]", string.Empty);
            if (double.TryParse(first, NumberStyles.Any, CultureInfo.CurrentCulture, out double dPrice) && dPrice > 0)
            {
                return dPrice;
            }

            string last = parts[parts.Length - 1].Replace(".", sep).Replace(",", sep);
            last = Regex.Replace(last, @"[^\d" + Regex.Escape(sep) + "-]", string.Empty);
            _ = double.TryParse(last, NumberStyles.Any, CultureInfo.CurrentCulture, out dPrice);
            return dPrice;
        }

        private double? _priceNumeric;
        [DontSerialize]
        public double PriceNumeric => _priceNumeric ?? (_priceNumeric = ParsePrice(Price)).Value;

        private double? _priceBaseNumeric;
        [DontSerialize]
        public double PriceBaseNumeric => _priceBaseNumeric ?? (_priceBaseNumeric = ParsePrice(PriceBase)).Value;

        [DontSerialize]
        public bool IsFree => !Price.IsNullOrEmpty() && PriceNumeric == 0;

        [DontSerialize]
        public bool IsDiscount => !Price.IsEqual(PriceBase);

        [DontSerialize]
        public BitmapImage ImageBitmap => ImageSourceManagerPlugin.GetImage(Image, false, new BitmapLoadProperties(200, 200));

        [DontSerialize]
        public string ImagePath => ImageSourceManagerPlugin.GetImagePath(Image);
    }
}
