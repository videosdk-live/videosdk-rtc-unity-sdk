﻿using System.Threading.Tasks;
namespace live.videosdk
{
    public interface IApiCaller
    {
        Task<string> CallApi(string url, string token);
    }
}