//
//  VideoSDKUnityPlugin.h
//  Unity-iPhone
//
//  Created by Uday Gajera on 06/12/24.
//

#ifdef __cplusplus
extern "C" {
#endif

void OnMeetingJoined(const char* meetingId, const char* id, const char* name, bool enabledLogs, const char* logEndPoint, const char* jwtKey, const char* peerId, const char* sessionId);
void OnMeetingLeft(const char* id, const char* name);
void OnParticipantJoined(const char* id, const char* name);
void OnParticipantLeft(const char* id, const char* name);
void OnMeetingStateChanged(const char* state);
void OnError(const char* error);

void OnStreamEnabled(const char* id, const char* data);
void OnStreamDisabled(const char* id, const char* data);
void OnStreamPaused(const char* id, const char* data);
void OnStreamResumed(const char* id, const char* data);
void OnVideoFrameReceived(const char* id, const unsigned char* data, int length);

void OnExternalCallStarted();
void OnExternalCallRinging();
void OnExternalCallHangup();

void OnAudioDeviceChanged(const char* selectedDevice, const char* deviceList);
void OnSpeakerChanged(const char* id);

#ifdef __cplusplus
}
#endif
