using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChoziShopForWindows.MerchantsApi
{
    public abstract class HttpService
    {
        public const string BASE_URL = "https://merchants.chozishop.com";
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

        protected async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync($"{BASE_URL}/{endpoint}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content) ?? throw new InvalidOperationException("Deserialization returned null.");
        }

        protected async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            Debug.WriteLine($"Connecting to {BASE_URL}/{endpoint}");
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_URL}/{endpoint}");
                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response content: {jsonContent}");
                return JsonConvert.DeserializeObject<List<T>>(jsonContent);
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                throw new Exception("Error fetching list: " + ex.Message);
            }
        }

        protected async Task<T> PostAsync<T>(string endpoint, T data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseData) ?? throw new InvalidOperationException("Deserialization returned null.");
        }
    }
}
