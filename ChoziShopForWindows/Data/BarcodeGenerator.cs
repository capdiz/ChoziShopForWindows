using HandyControl.Tools.Converter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using Image = System.Windows.Controls.Image;
using System.Windows;
using System.Windows.Media;

namespace ChoziShopForWindows.Data
{
    class BarcodeGenerator
    {
        public static readonly string BASE_URL = "https://merchants.chozishop.com";
        public static readonly string AUTHENTICATION_URL = $"{BASE_URL}/windows_accounts";
        private string _deviceToken;
        private readonly Timer _pollingTimer;

        public BarcodeGenerator() { 
            _pollingTimer = new Timer(2000);
            _pollingTimer.Elapsed += PollForAuthentication;
        }

        public BitmapSource GenerateBarcodeImage()
        {
            _deviceToken= new WindowsAccountTokenGenerator().generateWindowsAccountToken();

            // string value for our qr code
            var qrcodePayload = $"{_deviceToken}";

            // generate qr code
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 220,
                    Width = 220,
                   Margin =1
                }
            };

            var bitmap = writer.Write(qrcodePayload);        
            
            // let us load our logo image
            var logo = LoadBitmapFromFile();            
            _pollingTimer.Start();
            if(logo==null) return BitmapToImageSource(bitmap);

            // calculate logo size (20% of the total qrcode image size)
            var logoWidth = (int)(bitmap.Width * 0.2);
            var logoHeight = (int)(bitmap.Height * 0.2);

            // create drawing visual
            var drawingVisual = new DrawingVisual();
            using(var drawingContext = drawingVisual.RenderOpen())
            {
                // draw qr code
                drawingContext.DrawImage(BitmapToImageSource(bitmap), new Rect(0, 0, bitmap.Width, bitmap.Height));
               // Draw centered logo
               var logoRect =new Rect((bitmap.Width - logoWidth) / 2, (bitmap.Height - logoHeight) / 2, logoWidth, logoHeight);
                drawingContext.DrawImage(logo, logoRect);
            }

            // render visual to bitmap
            var finalBitmap =new RenderTargetBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Pbgra32);
            finalBitmap.Render(drawingVisual);
            return finalBitmap;
        }

        public string Status { get; set; }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        

        private BitmapSource LoadBitmapFromFile()
        {
            try
            {
               string path = "pack://application:,,,/Resources/Images/chozi_icon_black.png";
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }catch
            {
                return null;
            }
        }

        private async void PollForAuthentication(object sender, ElapsedEventArgs e)
        {
           var isAuthenticated = await CheckAuthentication(_deviceToken);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isAuthenticated)
                {
                    Status = "Authentciation";
                    _pollingTimer.Stop();
                    //QrImage.Source = null;
                }
            });
        }

        private async Task<bool> CheckAuthentication(string deviceToken)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"{BASE_URL}/windows_accounts?deviceToken={deviceToken}");
            Debug.WriteLine( response.Content.ReadAsStringAsync().Result, " Status: "+response.StatusCode );
            return response.IsSuccessStatusCode;
        }

    }
}
