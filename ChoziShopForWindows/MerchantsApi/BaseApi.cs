using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.MerchantsApi
{
    public class BaseApi : HttpService
    {
        private const string WSS_SESSION_URL = "wss://merchants.chozishop.com/cable?session_auth_token=";
        private const string SESSION_URL = "https://merchants.chozishop.com/restart_merchant_session/";
        private const string STORES_URL = $"{BASE_URL}/stores";
        

        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
        // prevents concurrent socket operations
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private string authToken;


        public BaseApi(string authToken) : base(authToken)
        {
            this.authToken = authToken;
            _webSocket = new ClientWebSocket();


        }

        public async Task<WindowsSessionResponse> ValidateWindowsSession(string deviceToken, string sessionUrl)
        {
            Debug.WriteLine("Validating windows session with token: " + deviceToken);
            return await GetAsync<WindowsSessionResponse>($"{sessionUrl}{deviceToken}");
        }

        public async Task<WindowsSessionResponse> ActivateWindowsSession(WindowsSessionResponse windowsSessionResponse)
        {
            string activationUrl = $"windows_sessions/{windowsSessionResponse.Id}/activate_device";
            Debug.WriteLine("Activating windows session with id: " + windowsSessionResponse.Id);
            return await PatchAsync<WindowsSessionResponse>($"{activationUrl}", windowsSessionResponse);
        }

        public async Task<List<StoreResponse>> GetWindowsAccountStores(long merchantId)
        {
            Debug.WriteLine("Fetching store details for merchant no: " + merchantId);
            return await GetListAsync<StoreResponse>($"{Store.WindowsAccountStoreUrl}?merchant_id={merchantId}");
        }

        public async Task<List<CategorySectionResponse>> GetCategorySections(long storeId)
        {
            Debug.WriteLine("Fetching category sections for store no: " + storeId);
            return await GetListAsync<CategorySectionResponse>($"{Store.CategorySectionUrl}?store_id={storeId}");
        }

        public async Task<WindowsSessionResponse> RestartMerchantSession(string sessionAuthToken)
        {                      
            var restartUrl = $"{SESSION_URL}{sessionAuthToken}";
            Debug.WriteLine("Restarting merchant session with token: " + sessionAuthToken + " at url: "+restartUrl);
            WindowsSessionResponse data = new WindowsSessionResponse
            {
                AuthToken = sessionAuthToken,
                DeviceToken = sessionAuthToken,
                Status = "active",
                Type = "scan"
            };
            return await PatchAsync<WindowsSessionResponse>($"{restartUrl}", data);
        }

        public async Task<SerializedOrder> ProcessMobileMoneyOrder(Order order, string accountType)
        {
            string jsonOrderProducts = JsonConvert.SerializeObject(order.OrderProductItems, Formatting.Indented);
            var serializedOrder = new SerializedOrder()
            {
                StoreId = order.StoreId,
                PreferredPaymentMode = order.PreferredPaymentMode,
                OrderStatus = order.OrderStatus,
                TotalAmountCents = order.TotalAmountCents,
                Currency = order.TotalAmountCurrency,                
                OrderCategoryProducts = jsonOrderProducts
            };
            Debug.WriteLine("Json order products: "+jsonOrderProducts);
            var createOrderUrl = $"{STORES_URL}/{serializedOrder.StoreId}/orders?account_type={accountType}";
            return await PostAsync<SerializedOrder>(createOrderUrl, serializedOrder);
        }

        public async Task<AirtelPaymentCollectionRequest> InitiatePaymentRequest(string phoneNumber, decimal amount)
        {
            Debug.WriteLine("Initiating AirtelPay request");
            var collectionRequest = new AirtelPaymentCollectionRequest
            {
                Msisdn = phoneNumber,
                Amount = 300           
            };
            return await PaymentCollectionRequestAsync(collectionRequest);
        }

        public async Task<PaymentAuthRequest> CreatePaymentAuth(string email, string accessId)
        {
            var paymentAuth = new PaymentAuthRequest();
            paymentAuth.Email = email;
            paymentAuth.AccessId = accessId;
            return await GeneratePaymentAuthentication<PaymentAuthRequest>(paymentAuth);
        }

        //public async Task<CollectionRequest> GetTransactionStatus(AirtelPayCollection airtelPayCollection)
        //{
        //    return await GetTransactionStatusAsync<CollectionRequest>(airtelPayCollection);
        //}

        //public async Task CheckForActiveSession(string sessionAuthToken)
        //{
        //    await _semaphoreSlim.WaitAsync();

        //    try
        //    {
        //        Debug.WriteLine("Cleaning up any previous connections...");
        //        if (_webSocket?.State == WebSocketState.Open)
        //        {
        //            await DisconnectAsync(silent: true);
        //        }

        //        // Initialize a new connection
        //        _cts = new CancellationTokenSource();
        //        _webSocket = new ClientWebSocket();

        //        // Configure headers
        //        _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {authToken}");

        //        _webSocket.Options.AddSubProtocol("actioncable-v-json");

        //        // Add required headers for Action cable            
        //        _webSocket.Options.SetRequestHeader("Origin", "https://merchants.chozishop.com");


        //        Debug.WriteLine("Connecting to session with token: " + sessionAuthToken);
        //        await _webSocket.ConnectAsync(new Uri($"{SESSION_URL}{sessionAuthToken}"), _cts.Token);

        //        // start the Receive loop
        //        _ = Task.Run(ReceiveLoop, _cts.Token);

        //        var identifier = new
        //        {
        //            channel = "WindowsSessionChannel",
        //            session_auth_token = sessionAuthToken
        //        };
        //        var identifierJson = JsonConvert.SerializeObject(identifier);

        //        var sunscription = new
        //        {
        //            command = "subscribe",
        //            identifier = identifierJson
        //        };

        //        await SendInternalWebSocketMessage(JsonConvert.SerializeObject(sunscription), _cts.Token);
        //    }
        //    catch(Exception ex)
        //    {
        //        Debug.WriteLine("Error while connecting to websocket: " + ex.Message);
        //    }
        //    finally
        //    {
        //        Debug.WriteLine("Wjaiting for semaphore to be released...");

        //        _semaphoreSlim.Release();
        //    }
        //}


        //private void HandleIncomingMessage(string message)
        //{
        //    Debug.WriteLine($"Received message: {message}");
        //    var scanEvent = JsonConvert.DeserializeObject<WindowsSessionResponse>(message);
        //    if (scanEvent != null && scanEvent?.Type == "scan")
        //    {
        //        Debug.WriteLine($"Scan event received: {scanEvent}");

        //        // Handle the scan event here
        //    }
        //    else
        //    {
        //        Debug.WriteLine($"I must have received something i don't know abot");
        //    }

        //}

        //private async Task SendInternalWebSocketMessage(string message, CancellationToken cancellationToken)
        //{
        //    // no semaphore lock here. State check is happening directly
        //    if(_webSocket?.State != WebSocketState.Open)
        //        throw new InvalidOperationException("WebSocket is not connected.");

        //    Debug.WriteLine("Sending message: " + message);
        //    byte[] buffer = Encoding.UTF8.GetBytes(message);
        //    await _webSocket.SendAsync(
        //        new ArraySegment<byte>(buffer),
        //        WebSocketMessageType.Text,
        //        true, 
        //        cancellationToken);

        //}

        //private async Task SendWebSocketMessage(string message)
        //{
        //    await _semaphoreSlim.WaitAsync();
        //    try
        //    {
        //        Debug.WriteLine("Sendin message: " + message);
        //        if (_webSocket?.State != WebSocketState.Open)
        //            throw new InvalidOperationException("WebSocket is not connected.");

        //        var buffer = Encoding.UTF8.GetBytes(message);
        //        await _webSocket.SendAsync(
        //            new ArraySegment<byte>(buffer),
        //            WebSocketMessageType.Text,
        //            true, _cts.Token);
        //    }
        //    catch(Exception ex)
        //    {
        //        Debug.WriteLine("Something went wrong: "+ex.Message);
        //    }
        //    finally { _semaphoreSlim.Release(); }
        //}

        //private async Task ReceiveLoop()
        //{
        //    var buffer = new byte[1024];
        //    try
        //    {
        //        while (_webSocket?.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
        //        {
        //            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
        //                _cts.Token);
        //            Debug.WriteLine($"Received message with type: {result.MessageType}");
        //            if (result.MessageType == WebSocketMessageType.Close)
        //            {
        //                Debug.WriteLine("WebSocket closed by server.");
        //                await DisconnectAsync();
        //                break;
        //            }
        //            else
        //            {
        //                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        //                Debug.WriteLine($"Received message: {message}");
        //                HandleIncomingMessage(message);
        //            }
        //        }
        //    }
        //    catch (OperationCanceledException ex)
        //    {
        //        // Normal shutdown
        //    }
        //    catch (WebSocketException ex)
        //    {
        //        Debug.WriteLine($"WebSocket error: {ex.Message}");
        //        await DisconnectAsync();

        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Unexpected error: {ex.Message}");
        //        await DisconnectAsync();
        //    }
        //}

        //public async Task DisconnectAsync(bool silent = false)
        //{
        //    await _semaphoreSlim.WaitAsync();

        //    try
        //    {
        //        if (_webSocket != null)
        //        {
        //            if (_webSocket?.State == WebSocketState.Open)
        //            {
        //                await _webSocket.CloseAsync(
        //                    WebSocketCloseStatus.NormalClosure,
        //                    "Closing",
        //                    CancellationToken.None);
        //                Debug.WriteLine("WebSocket closed.");
        //            }

        //            // Only reset if not silent
        //            if (!silent)
        //            {
        //                Debug.WriteLine("Resetting WebSocket and CancellationTokenSource.");

        //                _cts.Cancel();
        //                _webSocket.Dispose();
        //                _webSocket = null;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error while disconnecting: {ex.Message}");
        //    }

        //    finally
        //    {
        //        _semaphoreSlim.Release();
        //    }
        //}
    }
}
