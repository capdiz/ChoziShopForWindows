using ChoziShop.Data.Models;
using ChoziShopForWindows.Data;
using ChoziShopForWindows.Serialized;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.MerchantsApi
{
    public abstract class HttpService
    {
        public const string BASE_URL = "https://merchants.chozishop.com";
        public const string BASE_PAYMENTS_URL = "https://payments.chozishop.com/";
        private const string BASE_PAYMENTS_AUTH_URL = "https://payments.chozishop.com/authorizations";
        private const string REQUEST_TO_COLLECT_URL = $"{BASE_PAYMENTS_URL}airtel_pays/collections/request-to-collect";
        private const string REQUEST_TO_DISBURSE_URL = $"{BASE_PAYMENTS_URL}/airtel_pays/disbursements/request-to-disburse";
        private readonly HttpClient _httpClient;

        protected HttpService(string authToken = null)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BASE_URL);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            }
        }



        protected async Task<List<T>> GetAllAsync<T>(string endpoint)
        {
            try
            {
                Debug.WriteLine($"Connecting to {endpoint}");
                var response = await _httpClient.GetAsync($"{endpoint}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Debug.WriteLine($"Endpoint {endpoint} not found.");
                    return default;
                }
                Debug.WriteLine($"Response content: {response.Content.ReadAsStringAsync()}");
                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
             
                    return JsonConvert.DeserializeObject<List<T>>(jsonContent) ?? throw new InvalidOperationException("Deserialization returned null.");
               
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Something went wrong: {ex.Message}");
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error fetching all data: " + ex.Message);
            }
        }

        protected async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                Debug.WriteLine($"Connecting to {endpoint}");
                var response = await _httpClient.GetAsync(endpoint);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Debug.WriteLine($"Endpoint {endpoint} not found.");
                    return default;
                }
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response from endpoint: {content}");

                return JsonConvert.DeserializeObject<T>(content) ?? throw new InvalidOperationException("Deserialization returned null.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching data from {endpoint}: {ex.Message}");
                {
                    // Handle the exception (e.g., log it, rethrow it, etc.)
                    Debug.WriteLine($"Error fetching data from {endpoint}: {ex.Message}");
                    return default;
                }
            }

        }

        protected async Task<T> GetTransactionStatusAsync<T>(AirtelPayCollection airtelPayCollection)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_PAYMENTS_URL}airtel_pays/collections/{airtelPayCollection.AirtelPayCollectionRequestId}/transaction-enquiry");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content) ?? throw new InvalidOperationException("Deserialization returned null.");
            }
            catch (Exception ex)
            {

                throw new Exception("Error getting transaction status: " + ex.Message);

            }
        }

        protected async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            Debug.WriteLine($"Connecting to {BASE_URL}/{endpoint}");
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    DateParseHandling = DateParseHandling.None
                };
                var response = await _httpClient.GetAsync($"{BASE_URL}/{endpoint}");
                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response content: {jsonContent}");
                return JsonConvert.DeserializeObject<List<T>>(jsonContent, settings: settings);
            }
            catch (JsonException ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error fetching list: " + ex.Message);
            }
        }

        protected async Task<T> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Post response: " + responseData);
                return JsonConvert.DeserializeObject<T>(responseData) ?? throw new InvalidOperationException("Deserialization returned null.");
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error posting data: " + ex.Message);
            }
        }

        protected async Task<TResponse> PostAsync<TResponse, TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Post response: " + responseData);
                return JsonConvert.DeserializeObject<TResponse>(responseData) ?? throw new InvalidOperationException("Deserialization returned null.");
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error posting data: " + ex.Message);
            }
        }

        protected async Task<T> DeleteAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Debug.WriteLine($"Endpoint {endpoint} not found.");
                    return default;
                }
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Delete response: " + responseData);
                return JsonConvert.DeserializeObject<T>(responseData) ?? throw new InvalidOperationException("Deserialization returned null.");
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error deleting data: " + ex.Message);
            }
        }

        protected async Task<T> GeneratePaymentAuthentication<T>(T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BASE_PAYMENTS_AUTH_URL, content);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseData) ?? throw new InvalidOperationException("Deserialization returned null.");
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error posting data: " + ex.Message);
            }
        }

        protected async Task<T> PaymentCollectionRequestAsync<T>(T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(REQUEST_TO_COLLECT_URL, content);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response content: {responseData}");
                return JsonConvert.DeserializeObject<T>(responseData) ?? throw new InvalidOperationException("Deserialization returned null.");
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error posting data: " + ex.Message);
            }
        }

        protected async Task<T> PaymentDisbursementRequest<T>(T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(REQUEST_TO_DISBURSE_URL, content);
                Debug.WriteLine(response);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(responseData) ?? throw new InvalidOperationException("Deserialization returned null.");
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error posting data: " + ex.Message);
            }
        }

        protected async Task<T> PatchAsync<T>(string endPoint, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync(endPoint, content);
                if (Convert.ToInt32(response.StatusCode) == 202)
                {
                    response.EnsureSuccessStatusCode();
                    var responseData = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine("Receiving response: " + responseData);
                    return JsonConvert.DeserializeObject<T>(responseData) ?? throw new InvalidOperationException("Deserialization returned null.");
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    return default;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error patching data: " + ex.Message);
            }
        }

        
    }
}
