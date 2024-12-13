using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace live.videosdk
{
    public sealed class ApiCaller : IApiCaller
    {
        private static readonly HttpClient _httpclient = new HttpClient();
        public async Task<string> CallApi(string url, string token)
        {
            _httpclient.Timeout = TimeSpan.FromSeconds(10); // Set timeout to 10 seconds
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", token);
            request.Content = new StringContent("", Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpclient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                Debug.Log("Time-OUT");
                throw new HttpRequestException("Request timed out. Please check your internet connection.", ex);
            }
            catch (HttpRequestException ex)
            {
                Debug.Log("Failed");
                throw new HttpRequestException($"Create Meet API call failed: {ex.Message}", ex);
            }

        }
    }
}
