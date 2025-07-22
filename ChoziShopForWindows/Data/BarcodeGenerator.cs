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
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using ChoziShopForWindows.MerchantsApi;
using ChoziShopForWindows.ViewModels;

namespace ChoziShopForWindows.Data
{
    class BarcodeGenerator
    {
        public static readonly string AUTHENTICATION_URL = $"{HttpService.BASE_URL}/windows_accounts";
        public static readonly string MERCHANT_AUTHORIZATION_URI = $"windows_sessions/unknown/validate_session?device_token=";
        private string _deviceToken;
        private string _loginToken;
        private string _sessionAuthToken;

        private long activeSessionId;
        private bool _isMerchantSessionActive;
        private bool isAuthorizationCompleted;

        private MainWindowViewModel _mainWindowViewModel;

        private BaseApi _baseApi;

        private readonly Timer _pollingTimer;
        private readonly Timer _authorizationTimer;

        public EventHandler<MerchantResponse> MerchantResponseHandler;
        public EventHandler<WindowsSessionResponse> WindowsSessionResponseHandler;

        public BarcodeGenerator(MainWindowViewModel mainWindowViewModel)
        {
            _pollingTimer = new Timer(2000);
            _pollingTimer.Elapsed += PollForAuthentication;
            _mainWindowViewModel = mainWindowViewModel;
            _mainWindowViewModel.InternetStatusHandler += OnInternetConnectionChanged;
        }



        public BarcodeGenerator(BaseApi baseApi)
        {
            _baseApi = baseApi;
            _authorizationTimer = new Timer(2000);
            _authorizationTimer.Elapsed += PollForAuthorization;
        }

        public void SetMainWindowViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _mainWindowViewModel.InternetStatusHandler += OnInternetConnectionChanged;
        }

        public void SetIsMerchantSessionActive(bool isActive)
        {
            _isMerchantSessionActive = isActive;
        }

        public void SetActiveSessionId(long sessionId)
        {
            activeSessionId = sessionId;
        }

        public void SetSessionAuthToken(string sessionAuthToken)
        {
            _sessionAuthToken = sessionAuthToken;
        }

        public void StopAuthorizationTimer()
        {
            if (_authorizationTimer != null && _authorizationTimer.Enabled)
            {
                Debug.WriteLine("Stopping authorization timer");
                _authorizationTimer.Stop();
                _authorizationTimer.Elapsed -= PollForAuthorization;
            }
        }

        public BitmapSource GenerateLoginBarcode()
        {
            // geneerate random token in qrcode for authentication purposes
            _loginToken = new WindowsAccountTokenGenerator().generateLoginToken();

            // string value for our qr code
            var qrcodePayload = string.Empty;
            if (_isMerchantSessionActive)
            {
                qrcodePayload = $"validate-{_sessionAuthToken}";
            }
            else
            {
                qrcodePayload =
                     $"login-{_loginToken}";
            }

            // generate qr code
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 120,
                    Width = 120,
                    Margin = 1
                }
            };

            var bitmap = writer.Write(qrcodePayload);

            // let us load our logo image
            var logo = LoadBitmapFromFile();
            _authorizationTimer.Start();

            if (logo == null) return BitmapToImageSource(bitmap);

            // calculate logo size (20% of the total qrcode image size)
            var logoWidth = (int)(bitmap.Width * 0.2);
            var logoHeight = (int)(bitmap.Height * 0.2);

            // create drawing visual
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                // draw qr code
                drawingContext.DrawImage(BitmapToImageSource(bitmap), new Rect(0, 0, bitmap.Width, bitmap.Height));
                // Draw centered logo
                var logoRect = new Rect((bitmap.Width - logoWidth) / 2, (bitmap.Height - logoHeight) / 2, logoWidth, logoHeight);
                drawingContext.DrawImage(logo, logoRect);
            }

            // render visual to bitmap
            var finalBitmap = new RenderTargetBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Pbgra32);
            finalBitmap.Render(drawingVisual);
            return finalBitmap;
        }

        public BitmapSource GenerateBarcodeImage()
        {
            _deviceToken = new WindowsAccountTokenGenerator().generateWindowsAccountToken();

            // string value for our qr code
            var qrcodePayload = $"create-{_deviceToken}";

            // generate qr code
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 220,
                    Width = 220,
                    Margin = 1
                }
            };

            var bitmap = writer.Write(qrcodePayload);

            // let us load our logo image
            var logo = LoadBitmapFromFile();

            _pollingTimer.Start();

            if (logo == null) return BitmapToImageSource(bitmap);

            // calculate logo size (20% of the total qrcode image size)
            var logoWidth = (int)(bitmap.Width * 0.2);
            var logoHeight = (int)(bitmap.Height * 0.2);

            // create drawing visual
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                // draw qr code
                drawingContext.DrawImage(BitmapToImageSource(bitmap), new Rect(0, 0, bitmap.Width, bitmap.Height));
                // Draw centered logo
                var logoRect = new Rect((bitmap.Width - logoWidth) / 2, (bitmap.Height - logoHeight) / 2, logoWidth, logoHeight);
                drawingContext.DrawImage(logo, logoRect);
            }

            // render visual to bitmap
            var finalBitmap = new RenderTargetBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Pbgra32);
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
            }
            catch
            {
                return null;
            }
        }

        private async void PollForAuthorization(object sender, ElapsedEventArgs e)
        {
            if (!_isMerchantSessionActive)
            {
                var isAuthorized = await IsAuthorized();
                Debug.WriteLine("Polling for authorization: Authorization status: " + isAuthorized);
                Application application = Application.Current;
                if (application != null)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (isAuthorized)
                            {
                                Status = "Authorized";
                                _authorizationTimer.Stop();
                                isAuthorizationCompleted = true;
                                //QrImage.Source = null;
                            }
                        });
                    }
                    catch 
                    {

                    }
                }
            }
            else
            {
                var response = await ConnectToSession();
                Debug.WriteLine("Polling for session authorization: Session status: " + response?.Status);
                if (response != null)
                {
                    Debug.WriteLine("Authorization successfully completed!");
                    _authorizationTimer.Stop();
                    isAuthorizationCompleted = true;
                    WindowsSessionResponseHandler?.Invoke(this, response);
                }
            }
        }

        private async void PollForAuthentication(object sender, ElapsedEventArgs e)
        {
            var isAuthenticated = await CheckAuthentication(_deviceToken);
            Debug.WriteLine("Polling for authentication: Auth status: " + isAuthenticated);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isAuthenticated)
                {
                    Status = "Authentication";
                    _pollingTimer.Stop();
                    //QrImage.Source = null;
                }
            });
        }

        private async Task<bool> CheckAuthentication(string deviceToken)
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"{HttpService.BASE_URL}/windows_accounts/0/?windows_device_token={deviceToken}");
            Debug.WriteLine(response.Content.ReadAsStringAsync().Result, " Status: " + response.StatusCode);
            if (response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                var json = await response.Content.ReadAsStringAsync();
                var merchant = JsonConvert.DeserializeObject<MerchantResponse>(json);
                if (merchant != null)
                    MerchantResponseHandler?.Invoke(this, merchant);

                return true;
            }
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> IsAuthorized()
        {
            var client = new HttpClient();
            var response = await _baseApi.ValidateWindowsSession(_loginToken);
            if (response != null)
            {
                Debug.WriteLine("WindowsSession found with status: " + response.Status);

                WindowsSessionResponseHandler?.Invoke(this, response);
                return true;
            }

            return false;
        }

        private async Task<WindowsSessionResponse> ConnectToSession()
        {
            return await _baseApi.RestartMerchantSession(_sessionAuthToken);
        }

        public async void ActivateMerchantSession(WindowsSessionResponse sessionResponse)
        {
            var client = new HttpClient();
            var response = await _baseApi.ActivateWindowsSession(sessionResponse);
            if (response != null)
            {
                Debug.WriteLine("WindowsSession successfully activated with status: " + response.Status);
                WindowsSessionResponseHandler?.Invoke(this, response);
            }

            else
            {
                Debug.WriteLine("WindowsSession not found");

            }
        }

        private void OnInternetConnectionChanged(object? sender, bool isConnected)
        {
            if (_authorizationTimer != null)
            {
                bool isAuthorizationTimerRunning = _authorizationTimer.Enabled;
                if (!isConnected)
                {
                    // stop polling if authorization timer is running and no internet detected
                    if (isAuthorizationTimerRunning)
                    {
                        Debug.WriteLine("No internet connection detected. Stop unneccessary polling");
                        _authorizationTimer.Stop();
                    }
                } // only start 
                else if (!isAuthorizationTimerRunning && !isAuthorizationCompleted)
                {
                    Debug.WriteLine("Start polling for authorization");
                    _authorizationTimer.Start();
                }
            }
        }
    }
}
