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
        public async Task<string> CallApi(string url, string token,string jsonString)
        {
            _httpclient.Timeout = TimeSpan.FromSeconds(10); // Set timeout to 10 seconds
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", token);
            request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpclient.SendAsync(request);
                var result= await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API call failed: {result}");
                }
                //response.EnsureSuccessStatusCode();
                return result;
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                throw new HttpRequestException("Request timed out. Please check your internet connection.", ex);
            }
            
        }

        public async Task<string> CallApi(string url, string token)
        {
            _httpclient.Timeout = TimeSpan.FromSeconds(10); // Set timeout to 10 seconds
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", token);
            try
            {
                var response = await _httpclient.SendAsync(request);
                var result= await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API call failed: {result}");
                }
                //response.EnsureSuccessStatusCode();
                return result;
            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                throw new HttpRequestException("Request timed out. Please check your internet connection.", ex);
            }

        }

    }
}
