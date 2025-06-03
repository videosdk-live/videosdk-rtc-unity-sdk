//
//  VideoSDKUnityPlugin.mm
//  Unity-iPhone
//
//  Created by Uday Gajera on 05/12/24.
//

#import <Foundation/Foundation.h>
#import <CallKit/CallKit.h>
#include "UnityFramework/UnityFramework-Swift.h"

extern "C" {

typedef void (*OnMeetingJoinedDelegate)(const char* meetingId, const char* id, const char* name, bool enabledLogs, const char* logEndPoint, const char* jwtKey, const char* peerId, const char* sessionId);
typedef void (*OnMeetingLeftDelegate)(const char* id, const char* name);
typedef void (*OnParticipantJoinedDelegate)(const char* id, const char* name);
typedef void (*OnParticipantLeftDelegate)(const char* id, const char* name);
typedef void (*OnMeetingStateChangedDelegate)(const char* state);
typedef void (*OnErrorDelegate)(const char* error);
typedef void (*OnStreamEnabledDelegate)(const char* id, const char* data);
typedef void (*OnStreamDisabledDelegate)(const char* id, const char* data);
typedef void (*OnVideoFrameReceivedDelegate)(const char* id, const unsigned char* data, int length);
typedef void (*OnExternalCallStartedDelegate)();
typedef void (*OnExternalCallRingingDelegate)();
typedef void (*OnExternalCallHangupDelegate)();
typedef void (*OnSpeakerChangedDelegate)(const char* id);
typedef void (*OnPausedAllStreamsDelegate)(const char* kind);
typedef void (*OnResumedAllStreamsDelegate)(const char* kind);
typedef void (*OnAudioDeviceChangedDelegate)(const char* availableDevice, const char* selectedDevice);
typedef void (*OnMicRequestedDelegate)(const char* participantId);
typedef void (*OnWebcamRequestedDelegate)(const char* participantId);

static OnMeetingJoinedDelegate onMeetingJoinedCallback = NULL;
static OnMeetingLeftDelegate onMeetingLeftCallback = NULL;
static OnParticipantJoinedDelegate onParticipantJoinedCallback = NULL;
static OnParticipantLeftDelegate onParticipantLeftCallback = NULL;
static OnMeetingStateChangedDelegate onMeetingStateChangedCallback = NULL;
static OnErrorDelegate onErrorCallback = NULL;
static OnStreamEnabledDelegate onStreamEnabledCallback = NULL;
static OnStreamDisabledDelegate onStreamDisabledCallback = NULL;
static OnVideoFrameReceivedDelegate onVideoFrameReceivedCallback = NULL;
static OnExternalCallStartedDelegate onExternalCallStartedCallback = NULL;
static OnExternalCallRingingDelegate onExternalCallRingingCallback = NULL;
static OnExternalCallHangupDelegate onExternalCallHangupCallback = NULL;
static OnSpeakerChangedDelegate onSpeakerChangedCallback = NULL;
static OnPausedAllStreamsDelegate onPausedAllStreamsCallback = NULL;
static OnResumedAllStreamsDelegate onResumedAllStreamsCallback = NULL;
static OnAudioDeviceChangedDelegate onAudioDeviceChangedCallback = NULL;
static OnMicRequestedDelegate onMicRequestedCallback = NULL;
static OnWebcamRequestedDelegate onWebcamRequestedCallback = NULL;


#pragma mark - Functions called by unity

void joinMeeting(const char* token, const char* meetingId, const char* name, bool micEnable, bool camEnable, const char* participantId, const char* sdkVersion, const char* sdkName, const char* encoderConfig) {
    NSString *tokenStr = [NSString stringWithUTF8String:token];
    NSString *meetingIdStr = [NSString stringWithUTF8String:meetingId];
    NSString *nameStr = [NSString stringWithUTF8String:name];
    
    // Only create strings if parameters are not NULL
    NSString *participantIdStr = participantId ? [NSString stringWithUTF8String:participantId] : nil;
    NSString *sdkVersionStr = sdkVersion ? [NSString stringWithUTF8String:sdkVersion] : nil;
    NSString *sdkNameStr = sdkName ? [NSString stringWithUTF8String:sdkName] : nil;
    NSString *encoderConfigStr = encoderConfig ? [NSString stringWithUTF8String:encoderConfig] : nil;
    
    [[VideoSDKHelper shared] joinMeetingWithToken:tokenStr
                                            meetingId:meetingIdStr
                                            participantName:nameStr
                                            micEnabled:micEnable
                                            webCamEnabled:camEnable
                                            participantId:participantIdStr
                                            sdkName:sdkNameStr
                                            sdkVersion:sdkVersionStr
                                            encoderConfig:encoderConfigStr];
}

void leave() {
    [[VideoSDKHelper shared] leaveMeeting];
}

void toggleWebCam(bool status, const char* Id, const char* encoderConfig) {
    NSString *participantId = [NSString stringWithUTF8String:Id];
    NSString *encoderConfigStr = encoderConfig ? [NSString stringWithUTF8String:encoderConfig] : nil;
    [[VideoSDKHelper shared] toggleWebCamWithStatus:status encoderConfig:encoderConfigStr];
}

void toggleMic(bool status, const char* Id) {
    NSString *participantId = [NSString stringWithUTF8String:Id];
    [[VideoSDKHelper shared] toggleMicWithStatus:status];
}

void pauseAllStreams(const char* kind) {
    NSString *kindStr = [NSString stringWithUTF8String:kind];
    [[VideoSDKHelper shared] pauseAllStreamsWithKind:kindStr];
}

void resumeAllStreams(const char* kind) {
    NSString *kindStr = [NSString stringWithUTF8String:kind];
    [[VideoSDKHelper shared] resumeAllStreamsWithKind:kindStr];
}

void pauseStream(const char* participantId, const char* kind) {
    NSString *participantIdStr = [NSString stringWithUTF8String:participantId];
    NSString *kindStr = [NSString stringWithUTF8String:kind];
    [[VideoSDKHelper shared] pauseStreamWithParticipantId:participantIdStr kind:kindStr];
}

void resumeStream(const char* participantId, const char* kind) {
    NSString *participantIdStr = [NSString stringWithUTF8String:participantId];
    NSString *kindStr = [NSString stringWithUTF8String:kind];
    [[VideoSDKHelper shared] resumeStreamWithParticipantId:participantIdStr kind:kindStr];
}

const char* getAudioDevices() {
    @autoreleasepool {
        NSString *json = [[VideoSDKHelper shared] getAudioDevices];
        return strdup([json UTF8String]);
    }
}

const char* getVideoDevices() {
    @autoreleasepool {
        NSString *json = [[VideoSDKHelper shared] getVideoDevices];
        return strdup([json UTF8String]);
    }
}

const char* getSelectedAudioDevice() {
    @autoreleasepool {
        NSLog(@"called");
        NSString *json = [[VideoSDKHelper shared] getSelectedAudioDevice];
        return strdup([json UTF8String]);
    }
}

const char* getSelectedVideoDevice() {
    @autoreleasepool {
        NSString *json = [[VideoSDKHelper shared] getSelectedVideoDevice];
        return strdup([json UTF8String]);
    }
}

void changeAudioDevice(const char* device) {
    NSString *deviceStr = [NSString stringWithUTF8String:device];
    [[VideoSDKHelper shared] changeAudioDevice:deviceStr];
}

void changeVideoDevice(const char* device) {
    NSString *deviceStr = [NSString stringWithUTF8String:device];
    [[VideoSDKHelper shared] changeVideoDevice:deviceStr];
}

void setSpeakerMute(bool status) {
    [[VideoSDKHelper shared] setSpeakerMuteWithStatus:status];
}

void toggleRemoteMic(const char* participantId, bool micStatus) {
    NSString *participantIdStr = [NSString stringWithUTF8String:participantId];
    [[VideoSDKHelper shared] toggleRemoteMicWithParticipantId:participantIdStr micStatus:micStatus];
}

void toggleRemoteWebcam(const char* participantId, bool webcamStatus) {
    NSString *participantIdStr = [NSString stringWithUTF8String:participantId];
    [[VideoSDKHelper shared] toggleRemoteWebcamWithParticipantId:participantIdStr webcamStatus:webcamStatus];
}

void removeRemoteParticipant(const char* participantId) {
    NSString *participantIdStr = [NSString stringWithUTF8String:participantId];
    [[VideoSDKHelper shared] removeRemoteParticipantWithParticipantId:participantIdStr];
}

void setVideoEncoderConfig(const char* config) {
    NSString *configStr = [NSString stringWithUTF8String:config];
//    [[VideoSDKHelper shared] setVideoEncoderConfigWithConfig:configStr];
}

void onMicResponse(const char* Id, bool micStatus) {
    NSString *participantId = [NSString stringWithUTF8String:Id];
    [[VideoSDKHelper shared] toggleMicWithStatus:micStatus];
}

void onWebCamResponse(const char* Id, bool webCamStatus,const char* encoderConfig) {
    NSString *participantId = [NSString stringWithUTF8String:Id];
    NSString *encoderConfigStr = encoderConfig ? [NSString stringWithUTF8String:encoderConfig] : nil;
    [[VideoSDKHelper shared] toggleWebCamWithStatus:webCamStatus encoderConfig:encoderConfigStr];
}


#pragma mark - Register Callback function

void RegisterMeetingCallbacks(
    OnMeetingJoinedDelegate onMeetingJoined,
    OnMeetingLeftDelegate onMeetingLeft,
    OnParticipantJoinedDelegate onParticipantJoined,
    OnParticipantLeftDelegate onParticipantLeft,
    OnMeetingStateChangedDelegate onMeetingStateChanged,
    OnErrorDelegate onError,
    OnSpeakerChangedDelegate onSpeakerChanged,
    OnExternalCallStartedDelegate onExternalCallStarted,
    OnExternalCallRingingDelegate onExternalCallRinging,
    OnExternalCallHangupDelegate onExternalCallHangup,
    OnPausedAllStreamsDelegate onPausedAllStreams,
    OnResumedAllStreamsDelegate onResumedAllStreams,
    OnAudioDeviceChangedDelegate onAudioDeviceChanged,
    OnMicRequestedDelegate  onMicRequested,
    OnWebcamRequestedDelegate onWebcamRequested) {
    
    onMeetingJoinedCallback = onMeetingJoined;
    onMeetingLeftCallback = onMeetingLeft;
    onParticipantJoinedCallback = onParticipantJoined;
    onParticipantLeftCallback = onParticipantLeft;
    onMeetingStateChangedCallback = onMeetingStateChanged;
    onErrorCallback = onError;
    onSpeakerChangedCallback = onSpeakerChanged;
    onExternalCallStartedCallback = onExternalCallStarted;
    onExternalCallRingingCallback = onExternalCallRinging;
    onExternalCallHangupCallback = onExternalCallHangup;
    onPausedAllStreamsCallback = onPausedAllStreams;
    onResumedAllStreamsCallback = onResumedAllStreams;
    onAudioDeviceChangedCallback = onAudioDeviceChanged;
    onMicRequestedCallback = onMicRequested;
    onWebcamRequestedCallback = onWebcamRequested;


    [[VideoSDKHelper shared] dummy];
    NSLog(@"Meeting callbacks registered successfully");
}

void RegisterUserCallbacks(
    OnStreamEnabledDelegate onStreamEnabled,
    OnStreamDisabledDelegate onStreamDisabled,
    OnVideoFrameReceivedDelegate onVideoFrameReceived) {
    
    onStreamEnabledCallback = onStreamEnabled;
    onStreamDisabledCallback = onStreamDisabled;
    onVideoFrameReceivedCallback = onVideoFrameReceived;
    
    NSLog(@"Stream callbacks registered successfully");
}

//void RegisterCallStateCallbacks(
//    OnExternalCallStartedDelegate onExternalCallStarted,
//    OnExternalCallRingingDelegate onExternalCallRinging,
//    OnExternalCallHangupDelegate onExternalCallHangup) {
//
//    onExternalCallStartedCallback = onExternalCallStarted;
//    onExternalCallRingingCallback = onExternalCallRinging;
//    onExternalCallHangupCallback = onExternalCallHangup;
//
//    NSLog(@"Call state callbacks registered successfully");
//}

#pragma mark - Callback to Unity

void OnMeetingJoined(const char* meetingId, const char* id, const char* name, bool enabledLogs, const char* logEndPoint, const char* jwtKey, const char* peerId, const char* sessionId) {
    if (onMeetingJoinedCallback) {
        onMeetingJoinedCallback(meetingId, id, name, enabledLogs, logEndPoint, jwtKey, peerId, sessionId);
    }
}

void OnMeetingLeft(const char* id, const char* name) {
    if (onMeetingLeftCallback) {
        onMeetingLeftCallback(id, name);
    }
}

void OnParticipantJoined(const char* id, const char* name) {
    if (onParticipantJoinedCallback) {
        onParticipantJoinedCallback(id, name);
    }
}

void OnParticipantLeft(const char* id, const char* name) {
    if (onParticipantLeftCallback) {
        onParticipantLeftCallback(id, name);
    }
}

void OnMeetingStateChanged(const char* state) {
    if (onMeetingStateChangedCallback) {
        onMeetingStateChangedCallback(state);
    }
}

void OnError(const char* error) {
    if (onErrorCallback) {
        onErrorCallback(error);
    }
}

void OnStreamEnabled(const char* id, const char* data) {
    if (onStreamEnabledCallback) {
        onStreamEnabledCallback(id, data);
    }
}

void OnStreamDisabled(const char* id, const char* data) {
    if (onStreamDisabledCallback) {
        onStreamDisabledCallback(id, data);
    }
}

void OnVideoFrameReceived(const char* id, const unsigned char* data, int length) {
    if (onVideoFrameReceivedCallback) {
        onVideoFrameReceivedCallback(id, data, length);
    }
}

void OnExternalCallStarted() {
    if (onExternalCallStartedCallback) {
        onExternalCallStartedCallback();
    }
}

void OnExternalCallRinging() {
    if (onExternalCallRingingCallback) {
        onExternalCallRingingCallback();
    }
}

void OnExternalCallHangup() {
    if (onExternalCallHangupCallback) {
        onExternalCallHangupCallback();
    }
}

void OnAudioDeviceChanged(const char* availableDevice, const char* selectedDevice) {
    if (onAudioDeviceChangedCallback) {
        onAudioDeviceChangedCallback(availableDevice, selectedDevice);
    }
}

void OnSpeakerChanged(const char* id) {
    if (onSpeakerChangedCallback) {
        onSpeakerChangedCallback(id);
    }
}

void OnPausedAllStreams(const char* kind) {
    if (onPausedAllStreamsCallback) {
        onPausedAllStreamsCallback(kind);
    }
}

void OnResumedAllStreams(const char* kind) {
    if (onResumedAllStreamsCallback) {
        onResumedAllStreamsCallback(kind);
    }
}
void OnMicRequested(const char* participantId) {
    if (onMicRequestedCallback) {
        onMicRequestedCallback(participantId);
    }
}

void OnWebcamRequested(const char* participantId) {
    if (onWebcamRequestedCallback) {
        onWebcamRequestedCallback(participantId);
    }
}

}



