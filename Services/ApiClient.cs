using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PhoenixCacheDesktop.Services
{
    public class ApiClient
    {
        private string _baseUrl;
        private bool connected = false;

        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = 10, // Controla el número de conexiones simultáneas
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });
        public ApiClient(string baseUrl = "http://localhost:8080")
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
            _httpClient.DefaultRequestVersion = new Version(2, 0);
            _baseUrl = baseUrl;
        }

        public async Task<bool> UpdateBaseUrl(string host, string port)
        {
            _baseUrl = $"http://{host}:{port}";

            var (status, _) = await GetValueAsync("testConnect");
            if (status)
            {
                connected = true;
            }
            return connected;
        }

        public async Task<(bool status, string result)> GetValueAsync(string key)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/get?key={key}");
                if (response.IsSuccessStatusCode)
                {
                    return (true, await response.Content.ReadAsStringAsync());
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (true, null);
                }
                return (false, $"Error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<bool> SetValueAsync(string key, string value, int ttl)
        {
            if (!connected)
                return false;

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/set?key={key}&ttl={ttl}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RemoveKeyAsync(string key)
        {
            if (!connected)
                return false;

            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/remove?key={key}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<CacheDto>> ListKeysAsync()
        {
            if (!connected)
                return null;

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/list?allValue");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<CacheDto>>(json);
                }
                return new List<CacheDto>();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> FlushCacheAsync()
        {
            if (!connected)
                return false;

            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/flush", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
