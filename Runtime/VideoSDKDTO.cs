using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace live.videosdk
{

    internal class VideoSDKDTO: IVideoSDKDTO
    {
        private string _jwt;
        private string _dtoUri;
        private bool _enabledLogs;
        private IApiCaller _apiCaller;
        private Attributes _attribute;
        private string _modelName;
        private string _osVersion;
        public VideoSDKDTO(IApiCaller apiCaller)
        {
            _apiCaller = apiCaller;
            _modelName = SystemInfo.deviceModel;
            _osVersion = SystemInfo.operatingSystem;
        }

        public void Initialize(string sessionId,string jwt,string roomId,string peerId,bool enabledLogs,string dtoUri,string packageVersion)
        {
            _jwt = jwt;
            _enabledLogs = enabledLogs;
            _dtoUri = dtoUri;
            _attribute = new Attributes(roomId, peerId, sessionId, "unity-sdk",_modelName,_osVersion, packageVersion);
        }

        public void SendDTO(string logtype,string logtext)
        {
            if(_attribute==null)
            {
                return;
            }
            if (string.IsNullOrEmpty(_dtoUri) || string.IsNullOrEmpty(_jwt) || string.IsNullOrEmpty(_attribute.roomId) || string.IsNullOrEmpty(_attribute.sessionId)
                ||string.IsNullOrEmpty(_attribute.peerId) || !_enabledLogs)
            {
                return;
            }
            var dtoInfo = new VideoSDKDTOConfig(logtype, logtext, _attribute);
            Task.Run(async () =>
            {
                try
                {
                    var jsonString = JsonConvert.SerializeObject(dtoInfo);
                    string result = await _apiCaller.CallApi(_dtoUri, _jwt, jsonString);
                }
                catch (Exception)
                {
                    // Handle exceptions from the API call
                    //Debug.LogError("API call failed: " + ex.InnerException.Message);
                }
            });

        }

    }


}
