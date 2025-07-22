using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.models;
using ChoziShopForWindows.Serialized;
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

        public BaseApi() { }

        public async Task<WindowsSessionResponse> ValidateWindowsSession(string deviceToken)
        {

            Debug.WriteLine("Validating windows session with token: " + deviceToken);
            return await GetAsync<WindowsSessionResponse>(ApiRoutes.WindowsSession()
                .AddQuery("device_token", deviceToken).Build());
        }

        public async Task<WindowsSessionResponse> ActivateWindowsSession(WindowsSessionResponse windowsSessionResponse)
        {
            
            Debug.WriteLine("Activating windows session with id: " + windowsSessionResponse.Id);
            return await PatchAsync<WindowsSessionResponse>(
                ApiRoutes.ActivateWindowsSessionUrl(windowsSessionResponse.Id)
                .Build(), windowsSessionResponse);
        }

        public async Task<List<StoreResponse>> GetWindowsAccountStores(long merchantId)
        {
            Debug.WriteLine("Fetching store details for merchant no: " + merchantId);
            return await GetListAsync<StoreResponse>(
                ApiRoutes.GetWindowsAccountStoresUrl()
                .AddQuery("merchant_id", merchantId).Build());
        }

        public async Task<List<CategorySectionResponse>> GetCategorySections(long storeId)
        {
            Debug.WriteLine("Fetching category sections for store no: " + storeId);
            return await GetListAsync<CategorySectionResponse>(
                ApiRoutes.GetCategorySectionsUrl()
                .AddQuery("store_id", storeId).Build());
        }

        public async Task<WindowsSessionResponse> RestartMerchantSession(string sessionAuthToken)
        {                      
         
            WindowsSessionResponse data = new WindowsSessionResponse
            {
                AuthToken = sessionAuthToken,
                DeviceToken = sessionAuthToken,
                Status = "active",
                Type = "scan"
            };

            return await PatchAsync<WindowsSessionResponse>(
                ApiRoutes.RestartSessionUrl()
                .AddQuery("session_auth_token", sessionAuthToken).Build(), data);
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
            return await PostAsync<SerializedOrder>(
                ApiRoutes.CreateOrderUrl(serializedOrder.StoreId)
                .AddQuery("account_type", accountType).Build(), serializedOrder);
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

        public async Task<SerializedKeeper> CreateKeeperAccount(long merchantId, long storeId, Keeper keeper)
        {
            SerializedKeeper serializedKeeper = new SerializedKeeper
            {
                PhoneNumber = keeper.PhoneNumber,
                StoreId = storeId
            };

            return await PostAsync<SerializedKeeper>(ApiRoutes.CreateKeeperAccount()
                .AddQuery("merchant_id", merchantId)
                .AddQuery("store_id", storeId).Build(), serializedKeeper);
        }

        public async Task<SerializedKeeper> VerifyKeeperVerificationCode(string verificationCode)
        {
            Debug.WriteLine("Verifying keeper with code: " + verificationCode);
            return await GetAsync<SerializedKeeper>(ApiRoutes.VerifyKeeper()
                .AddQuery("invitation_code", verificationCode).Build());
        }

        public async Task<CategoriesResponse> GetDefaultCategoriesAsync( string accountType)
        {
            Debug.WriteLine($"Fetching serialized categories at {ApiRoutes.FetchCategorySections().Build()}");
            return await GetAsync<CategoriesResponse>(
                ApiRoutes.FetchCategorySections()
                .AddQuery("account_type", accountType)
                .Build());
        }

        public async Task<CategoryProductResponse> CreateCategoryProductAsyncCall(string accountType, long onlineStoreId, 
            SerializedCategoryProduct serializedCategoryProduct)
        {
            return await PostAsync<CategoryProductResponse, SerializedCategoryProduct>(
                ApiRoutes.CreateCategoryProduct(onlineStoreId, serializedCategoryProduct.CategorySectionId)
                .AddQuery("account_type", accountType).Build(), serializedCategoryProduct
                );
        }

        public async Task<SerializedCategorySectionResponse> CreateCategorySectionAsyncCall(long storeId, string accountType, CategorySectionCall categorySectionCall)
        {
            Debug.WriteLine("Creating category section with " + JsonConvert.SerializeObject(categorySectionCall));
            return await PostAsync<SerializedCategorySectionResponse, CategorySectionCall>(
                ApiRoutes.CreateCategorySection(storeId)
                .AddQuery("account_type", accountType).Build(), categorySectionCall);
        }

        public async Task<SerializedResponse> DeleteCategorySectionAsyncCall(long storeId, long categorySectionId, string accountType)
        {
            Debug.WriteLine("Deleting category section with id: " + categorySectionId);
            return await DeleteAsync<SerializedResponse>(
                ApiRoutes.DeleteCategorySection(storeId, categorySectionId)
                .AddQuery("account_type", accountType).Build());
        }

        public async Task<DefaultCategoryProductResponse> GetDefaultCategorySectionProductsAsync(long onlineCategoryId, string accountType)
        {
            try
            {
                Debug.WriteLine($"Fetching default category section products for category id {onlineCategoryId} with account type {accountType}");
                return await GetAsync<DefaultCategoryProductResponse>(
                    ApiRoutes.GetDefaultCategorySectionProducts(onlineCategoryId)
                    .AddQuery("account_type", accountType).Build());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting default category section products for category id {onlineCategoryId}", ex);
            }
        }
    }
}
