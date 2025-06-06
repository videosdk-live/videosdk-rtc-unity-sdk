//
//  VideoSDKHelper.swift
//  Unity-iPhone
//
//  Created by Uday Gajera on 05/12/24.
//

import VideoSDKRTC
import WebRTC
import Foundation
import CallKit

@_silgen_name("OnMeetingJoined")
func OnMeetingJoined(_ meetingId: UnsafePointer<CChar>, _ id: UnsafePointer<CChar>, _ name: UnsafePointer<CChar>, _ enabledLogs: Bool, _ logEndPoint: UnsafePointer<CChar>, _ jwtKey: UnsafePointer<CChar>, _ peerId: UnsafePointer<CChar>, _ sessionId: UnsafePointer<CChar>)

@_silgen_name("OnMeetingLeft")
func OnMeetingLeft(_ id: UnsafePointer<CChar>, _ name: UnsafePointer<CChar>)

@_silgen_name("OnParticipantJoined")
func OnParticipantJoined(_ id: UnsafePointer<CChar>, _ name: UnsafePointer<CChar>)

@_silgen_name("OnParticipantLeft")
func OnParticipantLeft(_ id: UnsafePointer<CChar>, _ name: UnsafePointer<CChar>)

@_silgen_name("OnMeetingStateChanged")
func OnMeetingStateChanged(_ state: UnsafePointer<CChar>)

@_silgen_name("OnError")
func OnError(_ error: UnsafePointer<CChar>)

@_silgen_name("OnStreamEnabled")
func OnStreamEnabled(_ id: UnsafePointer<CChar>, _ data: UnsafePointer<CChar>)

@_silgen_name("OnStreamDisabled")
func OnStreamDisabled(_ id: UnsafePointer<CChar>, _ data: UnsafePointer<CChar>)

@_silgen_name("OnVideoFrameReceived")
func OnVideoFrameReceived(_ id: UnsafePointer<CChar>, _ data: UnsafePointer<UInt8>, _ length: Int32)

@_silgen_name("OnExternalCallStarted")
func OnExternalCallStarted()

@_silgen_name("OnExternalCallRinging")
func OnExternalCallRinging()

@_silgen_name("OnExternalCallHangup")
func OnExternalCallHangup()

@_silgen_name("OnAudioDeviceChanged")
func OnAudioDeviceChanged(_ availableDevice: UnsafePointer<CChar>, _ selectedDevice: UnsafePointer<CChar>)

@_silgen_name("OnSpeakerChanged")
func OnSpeakerChanged(_ id : UnsafePointer<CChar>)

@_silgen_name("OnPausedAllStreams")
func OnPausedAllStreams(_ kind: UnsafePointer<CChar>)

@_silgen_name("OnResumedAllStreams")
func OnResumedAllStreams(_ kind: UnsafePointer<CChar>)

@_silgen_name("OnMicRequested")
func OnMicRequested(_ id: UnsafePointer<CChar>)

@_silgen_name("OnWebcamRequested")
func OnWebcamRequested(_ id: UnsafePointer<CChar>)



@available(iOS 14.0, *)
@objc public class VideoSDKHelper: NSObject {
    
    @objc public static let shared = VideoSDKHelper()
    var meeting: Meeting?
    var participants: [Participant] = []
    var localParticipant: Participant?
    var webCamEnabled: Bool = false
    var micEnabled: Bool = false
    private var isCallConnected = false
    private var isSpeakerMute = false
    
    private let ciContext = CIContext(options: [.useSoftwareRenderer: false])
    private let compressionSettings: [CFString: Any] = [
        kCGImageDestinationLossyCompressionQuality as CFString: 0.5
    ]
    
    private var videoRenderers: [String: EmptyRenderer] = [:]
    
    private let callObserver = CXCallObserver()
    private var selectedAudioDevice: String?
    private var encoderConfig: CustomVideoTrackConfig = .h240p_w320p
    private var multiStream: Bool = true
    private var meetingState: MeetingState = .DISCONNECTED
    
    private var facingMode: CameraFacingMode = .front
    private var currentVideoDevice: VideoDeviceInfo?
    private var currentAudioDevice: AudioDeviceInfo?
    private var portName = AVAudioSession.sharedInstance().currentRoute.inputs.first?.portName ?? ""
    private let audioSession: AVAudioSession
    
    override init() {
        self.audioSession = AVAudioSession.sharedInstance()
        super.init()
        VideoSDK.getAudioPermission()
        VideoSDK.getVideoPermission()
        try? self.audioSession.setCategory(.playAndRecord, options: [.allowBluetooth, .allowBluetoothA2DP])
        setupCallMonitoring()
        setupAudioRouteMonitoring()
        
        self.portName = self.audioSession.currentRoute.inputs.first?.portName ?? "Speaker"
        self.currentAudioDevice = AudioDeviceInfo(label: portName, kind: "audio", deviceId: portName)
        self.currentVideoDevice = facingMode.deviceInfo
    }
    
    private func setupCallMonitoring() {
        callObserver.setDelegate(self, queue: nil)
    }
    
        private func setupAudioRouteMonitoring() {
            NotificationCenter.default.addObserver(
                self,
                selector: #selector(handleAudioRouteChange),
                name: AVAudioSession.routeChangeNotification,
                object: nil
            )
        }
    
    @objc public func joinMeeting(token: String,
                                  meetingId: String,
                                  participantName: String,
                                  micEnabled: Bool,
                                  webCamEnabled: Bool,
                                  participantId: String? = nil,
                                  sdkName: String? = nil,
                                  sdkVersion: String? = nil,
                                  encoderConfig: String? = nil) {
        print("encoder config", encoderConfig)
        var (encoderEnum, multi, facing) = (self.encoderConfig, self.multiStream, self.facingMode.facing)
        if let encoderConfig = encoderConfig {
            (encoderEnum, multi, facing) = extractEncoderConfig(from: encoderConfig)
             self.encoderConfig = encoderEnum
             self.multiStream = multi
        }
        VideoSDK.config(token: token)
        
        let customVideoTrack = webCamEnabled ? createCustomVideoTrack(facing: facing, encoder: encoderEnum, multiStream: multi) : nil
        self.meeting = VideoSDK.initMeeting(
            meetingId: meetingId,
            meetingId: meetingId,
            participantId: participantId ?? "",
            participantName: participantName,
            micEnabled: micEnabled,
            webcamEnabled: webCamEnabled,
            customCameraVideoStream: customVideoTrack)
        
        let portName = self.audioSession.currentRoute.inputs.first?.portName ?? "Speaker"
        self.webCamEnabled = webCamEnabled
        self.micEnabled = micEnabled
        self.currentVideoDevice = webCamEnabled ? facingMode.deviceInfo : nil
        self.currentAudioDevice = micEnabled ? AudioDeviceInfo(label: portName, kind: "audio", deviceId: portName) : nil
        
        guard let meeting = meeting else {
            // error callback
            return
        }
        meeting.addEventListener(self)
        meeting.setAttributes(sdkName ?? "", sdkVersion ?? "")
        meeting.join()
    }
    
    @objc public func leaveMeeting() {
        guard let meeting = meeting else {
            return
        }
        videoRenderers.removeAll()
        
        meeting.leave()
        self.meeting = nil
    }
    
    @objc public func toggleWebCam(status: Bool, encoderConfig: String? = nil) {
        print("got called")
        guard let meeting = meeting,
              status != webCamEnabled,
              meetingState == .CONNECTED
        else { return }
        if status {
            meeting.disableWebcam()
            var (encoderEnum, multi, facing) = (self.encoderConfig, self.multiStream, self.facingMode.facing)
            if let encoderConfig = encoderConfig {
                 (encoderEnum, multi, facing) = extractEncoderConfig(from: encoderConfig)
                 self.encoderConfig = encoderEnum
                 self.multiStream = multi
            }
            print(encoderEnum, multi, facing.rawValue)
            let customTrack  = createCustomVideoTrack(
                facing: facing,
                encoder: encoderEnum,
                multiStream: multi
            )
//            let customTrack = try? VideoSDK.createCameraVideoTrack(encoderConfig: .h90p_w160p, facingMode: .back, multiStream: true)
            meeting.enableWebcam(customVideoStream: customTrack)
            webCamEnabled = true
            self.currentVideoDevice = facingMode.deviceInfo
        } else {
            meeting.disableWebcam()
            webCamEnabled = false
            self.currentVideoDevice = nil
        }
    }
    
    @objc public func toggleMic(status: Bool) {
        guard let meeting = meeting,
        status != micEnabled,
        meetingState == .CONNECTED  else {
            // error callback
            return
        }
        if status {
            meeting.unmuteMic()
            self.micEnabled = true
            self.currentAudioDevice = AudioDeviceInfo(label: portName, kind: "audio", deviceId: portName)
        } else {
            meeting.muteMic()
            self.micEnabled = false
            self.currentAudioDevice = nil
        }
    }
    
    @objc public func getLocalParticipant() -> String {
        guard let localParticipant = localParticipant else {
            return ""
        }
        return participantToJson(localParticipant).toJSONString()
    }
    
    @objc public func getParticipantList() -> String {
        var participantsData: [String: Any] = [:]
        participants.forEach { participant in
            participantsData[participant.id] = participantToJson(participant)
        }
        return participantsData.toJSONString()
    }
    
    @objc public func pauseAllStreams(kind: String) {
        guard let meeting = meeting else {
            return
        }
        meeting.pauseAllStreams(kind)
    }
    
    @objc public func resumeAllStreams(kind: String) {
        guard let meeting = meeting else {
            return
        }
        meeting.resumeAllStreams(kind)
    }
    
    @objc public func pauseStream(participantId: String, kind: String) {
        guard let participant = participants.first(where: { $0.id == participantId }), meetingState == .CONNECTED else {
            print("Participant not found: \(participantId) or Meeting is not connected")
            return
        }
        
        switch kind.lowercased() {
        case "video":
            if let videostream = participant.streams.first(where: { $1.kind == .state(value: .video) })?.value {
                videostream.pause()
            }
        case "audio":
            if let audiostream = participant.streams.first(where: { $1.kind == .state(value: .audio) })?.value {
                audiostream.pause()
            }
        case "share":
            if let sharestream = participant.streams.first(where: { $1.kind == .share })?.value {
                sharestream.pause()
            }
        default:
            print("Invalid stream kind: \(kind)")
        }
    }
    
    @objc public func resumeStream(participantId: String, kind: String) {
        guard let participant = participants.first(where: { $0.id == participantId }), meetingState == .CONNECTED else {
            print("Participant not found: \(participantId) or Meeting is not connected")
            return
        }
        
        switch kind.lowercased() {
        case "video":
            if let videostream = participant.streams.first(where: { $1.kind == .state(value: .video) })?.value {
                videostream.resume()
            }
        case "audio":
            if let audiostream = participant.streams.first(where: { $1.kind == .state(value: .audio) })?.value {
                audiostream.resume()
            }
        case "share":
            if let sharestream = participant.streams.first(where: { $1.kind == .share })?.value {
                sharestream.resume()
            }
        default:
            print("Invalid stream kind: \(kind)")
        }
    }
    
    @objc public func getAudioDevices() -> String {
        let audioDevices = getMics()
        let devicesArray = audioDevices.map { name, type in
            return AudioDeviceInfo(label: name, kind: "audio", deviceId: name)
        }
        return devicesArray.toJsonString(prettyPrint: true)
    }
    
    @objc public func getSelectedAudioDevice() -> String {
        print(currentAudioDevice?.toJsonString(prettyPrint: true))
        return currentAudioDevice?.toJsonString(prettyPrint: true) ?? ""
    }
    
    @objc public func changeAudioDevice(_ device: String) {
        guard let meeting = meeting else {
//            let device = self.changeMic(selectedDevice: device)
            self.selectedAudioDevice = device
            self.currentAudioDevice = AudioDeviceInfo(label: device, kind: "audio", deviceId: device)
            if let devicePtr = (currentAudioDevice.toJsonString(prettyPrint: true) as NSString).utf8String,
               let listPtr = (getAudioDevices() as NSString).utf8String {
                OnAudioDeviceChanged(listPtr, devicePtr)
            }
            return
        }
        meeting.changeMic(selectedDevice: device)
        self.selectedAudioDevice = device
    }
    
    @objc public func changeVideoDevice(_ device: String) {
        guard currentVideoDevice?.deviceId != device else {
            print("returning")
                return
        }
        guard meeting != nil else {
            print("meeting is nil")
            facingMode.toggle()
            self.currentVideoDevice = facingMode.deviceInfo
            return
        }
            meeting?.switchWebcam()
            facingMode.toggle()
            currentVideoDevice = facingMode.deviceInfo
    }
    
    @objc public func setSpeakerMute(status: Bool) {
        self.isSpeakerMute = status
        guard let meeting = meeting else { return }
        
        for participant in participants {
            if !participant.isLocal {
                if status {
                    self.pauseStream(participantId: participant.id, kind: "audio")
                } else {
                    self.resumeStream(participantId: participant.id, kind: "audio")
                }
            }
        }
    }
    
    @objc public func getVideoDevices() -> String {
        let cameraNames = VideoSDK.getCameras()
        let devicesArray = cameraNames.map { name -> VideoDeviceInfo in
            let mode = name.lowercased().contains("back") ? "back" : "front"
            return VideoDeviceInfo(label: name,
                                   kind: "video",
                                   deviceId: name,
                                   facingMode: mode)
        }
        return devicesArray.toJsonString(prettyPrint: true)
    }
    
    @objc public func getSelectedVideoDevice() -> String {
        return currentVideoDevice?.toJsonString(prettyPrint: true) ?? ""
    }
    
    private func participantToJson(_ participant: Participant) -> [String: Any] {
        return [
            "id": participant.id,
            "displayName": participant.displayName,
            "isLocal": participant.isLocal
        ]
    }
    
    @objc public func toggleRemoteMic(participantId: String, micStatus: Bool) {
        
        let participant = participants.first { $0.id == participantId }
        print("Found participant: \(participant?.id ?? "nil")")
        print("micStatus status", micStatus)

        if micStatus {
            print("Enabling mic for participant \(participantId)")
            participant?.enableMic()
        } else {
            print("Disabling mic for participant \(participantId)")
            participant?.disableMic()
        }

        print("toggleRemoteMic method called End")
    }

    
    @objc public func toggleRemoteWebcam(participantId: String,webcamStatus: Bool){
        
        let participant = participants.first { $0.id == participantId }
        print("Found participant: \(participant?.id ?? "nil")")
        print("webcam status", webcamStatus)
        if webcamStatus {
            print("Enabling webcam for participant \(participantId)")
            participant?.enableWebcam()
        } else {
            print("Disabling webcam for participant \(participantId)")
            participant?.disableWebcam()
        }
    }

    @objc public func removeRemoteParticipant(participantId: String){
        print("removeRemoteParticipant method called start")
        let participant = participants.first { $0.id == participantId }
        print("Found participant: \(participant?.id ?? "nil")")

        participant?.remove()
        print("removeRemoteParticipant method called End")
    }

    
    @objc public func dummy() {
        
    }
    
    
}

@available(iOS 14.0, *)
extension VideoSDKHelper: MeetingEventListener {
    public func onMeetingJoined() {
        if let selectedAudioDevice = selectedAudioDevice {
            meeting?.changeMic(selectedDevice: selectedAudioDevice)
        }
        
        let attributes = meeting?.getAttributes() ?? [:]
        
        guard let localParticipant = self.meeting?.localParticipant else { return }
        self.localParticipant = localParticipant
        participants.append(localParticipant)
        localParticipant.addEventListener(self)
        
        if let meetingIdPtr = ((meeting?.id ?? "") as NSString).utf8String,
           let idPtr = (localParticipant.id as NSString).utf8String,
           let namePtr = (localParticipant.displayName as NSString).utf8String,
           let logEndPointPtr = ((attributes["logEndPoint"] as? String ?? "") as NSString).utf8String,
           let jwtKeyPtr = ((attributes["jwtKey"] as? String ?? "") as NSString).utf8String,
           let peerIdPtr = ((attributes["peerId"] as? String ?? "") as NSString).utf8String,
           let sessionIdPtr = ((attributes["sessionId"] as? String ?? "") as NSString).utf8String {
            
            let enabledLogs = attributes["enabledLogs"] as? Bool ?? false
            
            OnMeetingJoined(meetingIdPtr,
                            idPtr,
                            namePtr,
                            enabledLogs,
                            logEndPointPtr,
                            jwtKeyPtr,
                            peerIdPtr,
                            sessionIdPtr)
        }
    }
    
    public func onMeetingLeft() {
        guard let localParticipant = self.localParticipant else { return }
        
        participants.removeAll()
        self.localParticipant?.removeEventListener(self)
        
        if let idPtr = (localParticipant.id as NSString).utf8String,
           let namePtr = (localParticipant.displayName as NSString).utf8String {
            OnMeetingLeft(idPtr, namePtr)
        }
        self.meeting = nil
    }
    
    public func onParticipantJoined(_ participant: Participant) {
        participants.append(participant)
        participant.addEventListener(self)
        
        if let idPtr = (participant.id as NSString).utf8String,
           let namePtr = (participant.displayName as NSString).utf8String {
            OnParticipantJoined(idPtr, namePtr)
        }
    }
    
    public func onParticipantLeft(_ participant: Participant) {
        participants.removeAll { $0 === participant }
        participant.removeEventListener(self)
        
        if let idPtr = (participant.id as NSString).utf8String,
           let namePtr = (participant.displayName as NSString).utf8String {
            OnParticipantLeft(idPtr, namePtr)
        }
        
        if let renderer = videoRenderers[participant.id] {
            videoRenderers.removeValue(forKey: participant.id)
        }
    }
    
    public func onMeetingStateChanged(meetingState: MeetingState) {
        self.meetingState = meetingState
        if let statePtr = (meetingState.rawValue as NSString).utf8String {
            OnMeetingStateChanged(statePtr)
        }
    }
    
    public func onError(_ error: Error) {
        if let errorPtr = (error.localizedDescription as NSString).utf8String {
            OnError(errorPtr)
        }
    }
    
    public func onSpeakerChanged(participantId: String?) {
        if let participantId = participantId {
            if let idpPtr = (participantId as NSString).utf8String {
                OnSpeakerChanged(idpPtr)
            }
        }
    }
    
    public func onPausedAllStreams(kind: String) {
        if let kindPtr = (kind as NSString).utf8String {
            OnPausedAllStreams(kindPtr)
        }
    }
    
    public func onResumedAllStreams(kind: String) {
        if let kindPtr = (kind as NSString).utf8String {
            OnResumedAllStreams(kindPtr)
        }
    }
    
    public func onMicChanged(selectedDevice: String) {
        self.currentAudioDevice = AudioDeviceInfo(label: selectedDevice, kind: "audio", deviceId: selectedDevice)
        if let devicePtr = (currentAudioDevice.toJsonString(prettyPrint: true) as NSString).utf8String,
           let listPtr = (getAudioDevices() as NSString).utf8String {
            OnAudioDeviceChanged(listPtr, devicePtr)
        }
    }
    
       @objc private func handleAudioRouteChange(notification: Notification) {
           guard let userInfo = notification.userInfo,
                 let reasonValue = userInfo[AVAudioSessionRouteChangeReasonKey] as? UInt,
                 let reason = AVAudioSession.RouteChangeReason(rawValue: reasonValue) else {
               return
           }
    
           switch reason {
           case .newDeviceAvailable, .oldDeviceUnavailable, .override:
               let currentRoute = AVAudioSession.sharedInstance().currentRoute
               let currentDevice = currentRoute.inputs.first?.portName ?? ""
    //           let deviceList = VideoSDK.getAudioDevices()
               let currentDeviceInfo = AudioDeviceInfo(label: currentDevice, kind: "audio", deviceId: currentDevice)
               self.currentAudioDevice = currentDeviceInfo
               let deviceList = self.getAudioDevices()
    
//               let deviceListString = deviceList.description
               if let devicePtr = (currentDeviceInfo.toJsonString(prettyPrint: true) as NSString).utf8String,
                  let listPtr = (deviceList as NSString).utf8String {
                   OnAudioDeviceChanged(listPtr, devicePtr)
               }
           default:
               break
           }
       }
    
    
    @objc public func onMicRequested(participantId: String?, accept: @escaping () -> Void, reject: @escaping () -> Void) {
        let cString = participantId?.cString(using: .utf8) ?? "".cString(using: .utf8)!
        cString.withUnsafeBufferPointer { buffer in
            if let participantId = buffer.baseAddress {
                OnMicRequested(participantId)
            }
        }
    }

  @objc public func onWebcamRequested(participantId: String?, accept: @escaping () -> Void, reject: @escaping () -> Void) {
        let cString = participantId?.cString(using: .utf8) ?? "".cString(using: .utf8)!
        cString.withUnsafeBufferPointer { buffer in
            if let participantId = buffer.baseAddress {
                OnWebcamRequested(participantId)
            }
        }
    }
    
}

@available(iOS 14.0, *)
extension VideoSDKHelper: ParticipantEventListener {
    
    public func onStreamEnabled(_ stream: MediaStream, forParticipant participant: Participant) {
        var kind: String = ""
        if stream.kind == .state(value: .video) {
            kind = "video"
            if participant.isLocal {
                self.webCamEnabled = true
            }
            if let idPtr = (participant.id as NSString).utf8String,
               let dataPtr = (kind as NSString).utf8String {
                OnStreamEnabled(idPtr, dataPtr)
            }
            HandleVideoStream(videoTrack: stream.track as? RTCVideoTrack, participant: participant)
        } else if stream.kind == .state(value: .audio) {
            kind = "audio"
            if participant.isLocal {
                self.micEnabled = true
            }
            if let idPtr = (participant.id as NSString).utf8String,
               let dataPtr = (kind as NSString).utf8String {
                OnStreamEnabled(idPtr, dataPtr)
            }
            
            if self.isSpeakerMute {
                self.pauseStream(participantId: participant.id, kind: kind)
            }
        }
    }
    
    public func onStreamDisabled(_ stream: MediaStream, forParticipant participant: Participant) {
        if participant.isLocal {
            if stream.kind == .state(value: .video) {
                self.webCamEnabled = false
            } else if stream.kind == .state(value: .audio) {
                self.micEnabled = false
            }
        }
        
        var kind: String = ""
        
        if stream.kind == .state(value: .video) {
            kind = "video"
        } else if stream.kind == .state(value: .audio) {
            kind = "audio"
        }
        
        if let idPtr = (participant.id as NSString).utf8String,
           let dataPtr = (kind as NSString).utf8String {
            OnStreamDisabled(idPtr, dataPtr)
        }
        
        if stream.kind == .state(value: .video) {
            if let renderer = videoRenderers[participant.id] {
                if let videoTrack = stream.track as? RTCVideoTrack {
                    videoTrack.remove(renderer)
                }
                videoRenderers.removeValue(forKey: participant.id)
            }
        }
    }
    
    func HandleVideoStream(videoTrack: RTCVideoTrack? = nil, participant: Participant) {
        guard let videoTrack = videoTrack else { return }
        
        // Remove existing renderer if any
        if let existingRenderer = videoRenderers[participant.id] {
            videoTrack.remove(existingRenderer)
            videoRenderers.removeValue(forKey: participant.id)
        }
        
        let renderer = EmptyRenderer()
        videoRenderers[participant.id] = renderer
        weak var weakParticipant = participant
        
        renderer.frameHandler = { [weak self, weak renderer] frame in
            guard let self = self,
                  let participant = weakParticipant,
                  renderer != nil else { return }
            DispatchQueue.global(qos: .userInitiated).async {
                autoreleasepool {
                    if frame.timeStampNs % 2 != 0 {
                        return
                    }
                    
                    var pixelBuffer: CVPixelBuffer?
                    
                    if let i420Buffer = frame.buffer as? RTCI420Buffer {
                        pixelBuffer = self.createPixelBuffer(from: i420Buffer)
                    } else if let cvPixelBuffer = frame.buffer as? RTCCVPixelBuffer {
                        pixelBuffer = cvPixelBuffer.pixelBuffer
                    }
                    
                    guard let finalPixelBuffer = pixelBuffer else {
                        return
                    }
                    
                    autoreleasepool {
                        guard let imageData = self.compressFrame(pixelBuffer: finalPixelBuffer, isLocal: participant.isLocal) else {
                            return
                        }
                        
                        DispatchQueue.main.async { [weak self] in
                            guard self != nil else { return }
                            if let idPtr = (participant.id as NSString).utf8String {
                                imageData.withUnsafeBytes { (bytes: UnsafeRawBufferPointer) in
                                    let bytePtr = bytes.baseAddress!.assumingMemoryBound(to: UInt8.self)
                                    OnVideoFrameReceived(idPtr, bytePtr, Int32(imageData.count))
                                }
                            }
                        }
                    }
                }
            }
        }
        
        videoTrack.add(renderer)
    }
    
    
    func compressFrame(pixelBuffer: CVPixelBuffer, isLocal: Bool) -> Data? {
        CVPixelBufferLockBaseAddress(pixelBuffer, [])
        
        return autoreleasepool { () -> Data? in
            let ciImage = CIImage(cvPixelBuffer: pixelBuffer)
            let scale = 0.75
            
            let scaledExtent = ciImage.extent.applying(CGAffineTransform(scaleX: scale, y: scale))
            
            guard let cgImage = ciContext.createCGImage(ciImage, from: scaledExtent,
                                                        format: .RGBA8,
                                                        colorSpace: CGColorSpaceCreateDeviceRGB()) else {
                print("Failed to create CGImage")
                return nil
            }
            
            let data = NSMutableData()
            guard let destination = CGImageDestinationCreateWithData(
                data as CFMutableData,
                UTType.jpeg.identifier as CFString,
                1,
                nil
            ) else { return nil }
            
            // Increase compression for better memory usage
            let compressionSettings: [CFString: Any] = [
                kCGImageDestinationLossyCompressionQuality as CFString: 0.3 // Increased compression
            ]
            
            CGImageDestinationAddImage(destination, cgImage, compressionSettings as CFDictionary)
            
            guard CGImageDestinationFinalize(destination) else { return nil }
            return data as Data
        }
    }
    
    func createPixelBuffer(from i420Buffer: RTCI420Buffer) -> CVPixelBuffer? {
        return autoreleasepool { () -> CVPixelBuffer? in
            let width = Int(i420Buffer.width)
            let height = Int(i420Buffer.height)
            
            var pixelBuffer: CVPixelBuffer?
            let attrs = [
                kCVPixelBufferMetalCompatibilityKey: true,
                kCVPixelBufferIOSurfacePropertiesKey: [:],
                kCVPixelBufferCGImageCompatibilityKey: true,
                kCVPixelBufferCGBitmapContextCompatibilityKey: true
            ] as CFDictionary
            
            let status = CVPixelBufferCreate(kCFAllocatorDefault,
                                             width,
                                             height,
                                             kCVPixelFormatType_420YpCbCr8BiPlanarFullRange,
                                             attrs,
                                             &pixelBuffer)
            
            guard status == kCVReturnSuccess, let pixelBuffer = pixelBuffer else { return nil }
            
            CVPixelBufferLockBaseAddress(pixelBuffer, [])
            
            // Process Y plane
            if let dstY = CVPixelBufferGetBaseAddressOfPlane(pixelBuffer, 0) {
                let srcY = i420Buffer.dataY
                let dstStrideY = CVPixelBufferGetBytesPerRowOfPlane(pixelBuffer, 0)
                
                for row in 0..<height {
                    memcpy(dstY.advanced(by: row * dstStrideY),
                           srcY.advanced(by: row * Int(i420Buffer.strideY)),
                           width)
                }
            }
            
            // Process UV planes
            if let dstUV = CVPixelBufferGetBaseAddressOfPlane(pixelBuffer, 1) {
                let chromaWidth = (width + 1) / 2
                let chromaHeight = (height + 1) / 2
                let dstStrideUV = CVPixelBufferGetBytesPerRowOfPlane(pixelBuffer, 1)
                
                for row in 0..<chromaHeight {
                    let srcU = i420Buffer.dataU.advanced(by: row * Int(i420Buffer.strideU))
                    let srcV = i420Buffer.dataV.advanced(by: row * Int(i420Buffer.strideV))
                    let dst = dstUV.advanced(by: row * dstStrideUV)
                        .assumingMemoryBound(to: UInt8.self)
                    
                    for col in 0..<chromaWidth {
                        dst[col * 2] = srcU[col]
                        dst[col * 2 + 1] = srcV[col]
                    }
                }
            }
            
            return pixelBuffer
        }
    }
    
//    func createCustomVideoTrack() -> CustomRTCMediaStream? {
//        var VideoencoderConfig: CustomVideoTrackConfig
//        switch encoderConfig {
//        case "h480p_w640p":
//            VideoencoderConfig = .h480p_w640p
//        case "h720p_w960p":
//            VideoencoderConfig = .h720p_w1280p
//        case "h480p_w720p":
//            VideoencoderConfig = .h480p_w640p
//        default:
//            VideoencoderConfig = .h90p_w160p
//        }
//        guard let customVideoTrack = try? VideoSDK.createCameraVideoTrack(encoderConfig: VideoencoderConfig, facingMode: .front, multiStream: true) else {
//            return nil
//        }
//        return customVideoTrack
//    }
    
    func createCustomVideoTrack(
        facing: AVCaptureDevice.Position = .front,
        encoder: CustomVideoTrackConfig,
        multiStream: Bool
    ) -> CustomRTCMediaStream? {
        do {
            return try VideoSDK.createCameraVideoTrack(
                encoderConfig: encoder,
                facingMode:    facing,
                multiStream:   multiStream
            )
        } catch {
            print("Failed to create custom video track:", error)
            return nil
        }
    }
}

@available(iOS 14.0, *)
extension VideoSDKHelper: CXCallObserverDelegate {
    
    public func callObserver(_ callObserver: CXCallObserver, callChanged call: CXCall) {
        if !call.isOutgoing {
            if call.hasEnded {
                isCallConnected = false
                OnExternalCallHangup()
            } else if call.hasConnected && !isCallConnected {
                isCallConnected = true
                OnExternalCallStarted()
            } else if !call.hasConnected && !call.hasEnded {
                OnExternalCallRinging()
            }
        }
    }
}

extension VideoSDKHelper {
    
    func getMics() -> [(deviceName: String, deviceType: String)] {
        let audioSession = AVAudioSession.sharedInstance()
        
        var devices: [(String, String)] = [("Speaker", "Speaker")]
        for input in audioSession.availableInputs! {
            print(input.portName, input.portType)
            let deviceName: String = input.portName
            let deviceType: String =  (deviceName == "iPhone Microphone") ? "Receiver" : input.portType.rawValue
            let device = (deviceName,deviceType)
            devices.append(device)
        }
        
        let wiredHeadsetAvailable = audioSession.availableInputs?.contains { $0.portType == AVAudioSession.Port.headsetMic } ?? false

        if (wiredHeadsetAvailable) {
            devices.remove(at: 1)
            
        }
        
        devices = devices.map({ device in
            var updatedDevice = device
            if device.1.lowercased() == "microphonewired" {
                    updatedDevice.0 = "Headphones"
                    updatedDevice.1 = "Headphones"
                }
                return updatedDevice
        })
        
        return devices
    }
    
    func changeMic(selectedDevice: String) -> String {
        let deviceSelected: String = selectedDevice
        for input in audioSession.availableInputs! {
            
            if(selectedDevice == input.portName) {
                
                switch input.portType {
                    
                case AVAudioSession.Port.bluetoothLE, AVAudioSession.Port.bluetoothHFP, AVAudioSession.Port.bluetoothA2DP:
                        DispatchQueue.main.async {
                            do {
                                try self.audioSession.overrideOutputAudioPort(AVAudioSession.PortOverride.none)
                                try self.audioSession.setPreferredInput(input)
                            } catch let error as NSError {
                                print("error changing to bluetooth device \(error.localizedDescription)")
                            }
                        }

                        return deviceSelected
                    
                case AVAudioSession.Port.builtInReceiver:
                        DispatchQueue.main.async {
                            do {
                                try self.audioSession.overrideOutputAudioPort(AVAudioSession.PortOverride.none)
                                try self.audioSession.setPreferredInput(input)
                            } catch let error as NSError {
                                print("error changing to built-in device \(error.localizedDescription)")
                            }
                        }
                        return deviceSelected

                case AVAudioSession.Port.headphones, AVAudioSession.Port.headsetMic:
                        DispatchQueue.main.async {
                            do {
                                try self.audioSession.overrideOutputAudioPort(AVAudioSession.PortOverride.none)
                                try self.audioSession.setPreferredInput(input)
                            } catch let error as NSError {
                                print("error changing to headphones \(error.localizedDescription)")
                            }
                        }
                        return deviceSelected
                 
                case AVAudioSession.Port.builtInMic:
                        DispatchQueue.main.async {
                            do {
                                try self.audioSession.overrideOutputAudioPort(AVAudioSession.PortOverride.none)
                                try self.audioSession.setPreferredInput(input)
                            } catch let error as NSError {
                                print("error changing to headphones \(error.localizedDescription)")
                            }
                        }
                        return deviceSelected
                    
                default:
                        DispatchQueue.main.async {
                            do {
                                try self.audioSession.overrideOutputAudioPort(AVAudioSession.PortOverride.speaker)
                            } catch let error as NSError {
                                print("error changing to speaker \(error.localizedDescription)")
                            }
                        }
                        return deviceSelected
                }

            }

            if(selectedDevice.lowercased().contains("headphones")) {
                DispatchQueue.main.async {
                    do {
                        try self.audioSession.overrideOutputAudioPort(AVAudioSession.PortOverride.none)
                        try self.audioSession.setPreferredInput(input)
                    } catch let error as NSError {
                        print("error changing to speaker \(error.localizedDescription)")
                    }
                }
                return deviceSelected
            }
            
        }
        
        if(selectedDevice.lowercased().contains("speaker")) {
            DispatchQueue.main.async {
                do {
                    try self.audioSession.overrideOutputAudioPort(AVAudioSession.PortOverride.speaker)
                } catch let error as NSError {
                    print("error changing to speaker \(error.localizedDescription)")
                }
            }
            return deviceSelected
        }
        
        return "Device was not changed"
    }
    
    private func extractEncoderConfig(from json: String)
    -> (encoder: CustomVideoTrackConfig, multiStream: Bool, facing: AVCaptureDevice.Position)
    {
        var videoEnum: CustomVideoTrackConfig = .h240p_w320p
        var multiStream = true
        var facing = AVCaptureDevice.Position.front
        
        guard
            let data = json.data(using: .utf8),
            let obj  = try? JSONSerialization.jsonObject(with: data),
            let dict = obj as? [String:Any]
        else {
            return (videoEnum, multiStream, facing)
        }
        
        let rawEncoder    = dict["encoder"]       as? String ?? ""
        multiStream       = dict["isMultiStream"] as? Bool   ?? true
        let facingMode = dict["deviceId"] as? String ?? ""
        switch facingMode {
            case "Front Camera":
                facing = .front
            case "Back Camera":
                facing = .back
            default:
                facing = .front
        }
        
        switch rawEncoder {
        case "h240p_w320p":
            videoEnum = .h240p_w320p
        case "h480p_w640p":
            videoEnum = .h480p_w640p
        case "h720p_w1280p", "h720p_w960p":
            videoEnum = .h720p_w1280p
        case "h1080p_w1440p":
            videoEnum = .h1080p_w1440p
        default:
            videoEnum = .h240p_w320p
        }
        
        return (videoEnum, multiStream, facing)
    }
}

private class EmptyRenderer: NSObject, RTCVideoRenderer {
    var frameHandler: ((RTCVideoFrame) -> Void)?
    func setSize(_ size: CGSize) {
        
    }
    
    func renderFrame(_ frame: RTCVideoFrame?) {
        guard let frame = frame else {
            return
        }
        frameHandler?(frame)
    }
}

extension Dictionary {
    func toJSONString() -> String {
        let data = try? JSONSerialization.data(withJSONObject: self, options: [])
        return data?.toJSONString() ?? ""
    }
}

// Helpers

public struct AudioDeviceInfo: Codable {
    var label: String
    var kind: String
    var deviceId: String
}

public struct VideoDeviceInfo: Codable {
    var label: String
    var kind: String
    var deviceId: String
    var facingMode: String
}

enum CameraFacingMode: String {
    case front
    case back
    
    var facing: AVCaptureDevice.Position {
        switch self {
        case .front: return .front
        case .back: return .back
        }
    }

    mutating func toggle() {
        self = (self == .front) ? .back : .front
    }

    var displayName: String {
        switch self {
        case .front: return "Front Camera"
        case .back:  return "Back Camera"
        }
    }

    var deviceInfo: VideoDeviceInfo {
        VideoDeviceInfo(
            label: displayName,
            kind:  "video",
            deviceId:   displayName,
            facingMode: rawValue
        )
    }
}

extension Encodable {
    func toJsonString(prettyPrint: Bool = false) -> String {
        let encoder = JSONEncoder()
        if prettyPrint { encoder.outputFormatting = .prettyPrinted }
        guard let data = try? encoder.encode(self) else { return "" }
        return String(data: data, encoding: .utf8) ?? ""
    }
}
