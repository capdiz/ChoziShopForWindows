using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.Enums;
using ChoziShopForWindows.MerchantsApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace ChoziShopForWindows.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly HttpClient _httpClient;
        private IAuthTokenProvider _authTokenProvider;
        public TransactionService(HttpClient httpClient, IAuthTokenProvider authTokenProvider)
        {
            _httpClient = httpClient;
           //_authTokenProvider = authTokenProvider;
        }

       // public object BASE_PAYMENTS_URL { get; private set; }

        public void SetAuthTokenProvider(IAuthTokenProvider authTokenProvider)
        {
            _authTokenProvider = authTokenProvider;
        }



        public async Task<TransactionStatus> CheckTransactionStatusAsync(AirtelPayCollection airtelPayCollection, IDataObjects dataObjects)
        {
            var token = _authTokenProvider.GetPaymentAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                using var request =
                    new HttpRequestMessage(HttpMethod.Get,
                    $"{_httpClient.BaseAddress}airtel_pays/collections/{airtelPayCollection.AirtelPayCollectionRequestId}/transaction-enquiry");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                try
                {
                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var transactionStatusResponse = JsonConvert.DeserializeObject<TransactionStatusResponse>(content);
                    if (transactionStatusResponse != null && transactionStatusResponse.AirtelCollectionResponse != null)
                    {
                        if (transactionStatusResponse.AirtelCollectionResponse.Status != null && transactionStatusResponse.AirtelCollectionResponse.AirtelTransactionId != null)
                        {
                            airtelPayCollection.Status = transactionStatusResponse.AirtelCollectionResponse.Status;
                            airtelPayCollection.AirtelTransactionId = transactionStatusResponse.AirtelCollectionResponse.AirtelTransactionId;
                            if (transactionStatusResponse.AirtelCollectionResponse.Status == "TS")
                            {
                                Debug.WriteLine($"Transaction Status: {transactionStatusResponse.AirtelCollectionResponse.Status} AirtelId: {airtelPayCollection.AirtelTransactionId}");
                                airtelPayCollection.PaymentConfirmedAt = transactionStatusResponse.AirtelCollectionResponse.UpdatedAt;
                            }
                            bool updateCompleted = await dataObjects.UpdateAirtelPayCollection(airtelPayCollection);
                            if (updateCompleted)
                            {
                                return MapTransactionStatus(transactionStatusResponse.AirtelCollectionResponse.Status);
                            }
                        }

                    }
                }
                catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return TransactionStatus.FAILED;
                }

            }
            return TransactionStatus.TIP;
        }

        private TransactionStatus MapTransactionStatus(string transactionStatus)
        {
            switch (transactionStatus)
            {
                case "TIP":
                    return TransactionStatus.TIP;
                case "TF":
                    return TransactionStatus.TIP;
                case "TS":
                    return TransactionStatus.TS;
                case "INVALID_PIN":
                    return TransactionStatus.INVALID_PIN;
                case "BALANCE_LOW":
                    return TransactionStatus.INSUFFICIENT_FUNDS;
                case "TRANSACTION_LIMIT":
                    return TransactionStatus.TRANSACTION_LIMIT_HIT;
                case "TTO":
                    return TransactionStatus.TTO;
                default:
                    return TransactionStatus.TIP;
            }
        }
    }
}
