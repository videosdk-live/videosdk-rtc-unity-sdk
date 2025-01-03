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

typedef void (*OnMeetingJoinedDelegate)(const char* meetingId, const char* id, const char* name);
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

#pragma mark - Functions called by unity

void joinMeeting(const char* token, const char* meetingId, const char* name, bool micEnable, bool camEnable, const char* participantId) {
    NSString *tokenStr = [NSString stringWithUTF8String:token];
    NSString *meetingIdStr = [NSString stringWithUTF8String:meetingId];
    NSString *nameStr = [NSString stringWithUTF8String:name];
    
    // Only create participantIdStr if participantId is not NULL
    NSString *participantIdStr = participantId ? [NSString stringWithUTF8String:participantId] : nil;
    
    [[VideoSDKHelper shared] joinMeetingWithToken:tokenStr 
                                      meetingId:meetingIdStr 
                                participantName:nameStr 
                                   micEnabled:micEnable 
                                webCamEnabled:camEnable 
                                participantId:participantIdStr];
}

void leave() {
    [[VideoSDKHelper shared] leaveMeeting];
}

void toggleWebCam(bool status, const char* Id) {
    NSString *participantId = [NSString stringWithUTF8String:Id];
    [[VideoSDKHelper shared] toggleWebCamWithStatus:status];
}

void toggleMic(bool status, const char* Id) {
    NSString *participantId = [NSString stringWithUTF8String:Id];
    [[VideoSDKHelper shared] toggleMicWithStatus:status];
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

#pragma mark - Register Callback function

void RegisterMeetingCallbacks(
    OnMeetingJoinedDelegate onMeetingJoined,
    OnMeetingLeftDelegate onMeetingLeft,
    OnParticipantJoinedDelegate onParticipantJoined,
    OnParticipantLeftDelegate onParticipantLeft,
    OnMeetingStateChangedDelegate onMeetingStateChanged,
    OnErrorDelegate onError) {
    
    onMeetingJoinedCallback = onMeetingJoined;
    onMeetingLeftCallback = onMeetingLeft;
    onParticipantJoinedCallback = onParticipantJoined;
    onParticipantLeftCallback = onParticipantLeft;
    onMeetingStateChangedCallback = onMeetingStateChanged;
    onErrorCallback = onError;
    
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

void RegisterCallStateCallbacks(
    OnExternalCallStartedDelegate onExternalCallStarted,
    OnExternalCallRingingDelegate onExternalCallRinging,
    OnExternalCallHangupDelegate onExternalCallHangup) {
    
    onExternalCallStartedCallback = onExternalCallStarted;
    onExternalCallRingingCallback = onExternalCallRinging;
    onExternalCallHangupCallback = onExternalCallHangup;
    
    NSLog(@"Call state callbacks registered successfully");
}

#pragma mark - Callback to Unity

void OnMeetingJoined(const char* meetingId, const char* id, const char* name) {
    if (onMeetingJoinedCallback) {
        onMeetingJoinedCallback(meetingId, id, name);
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

}
