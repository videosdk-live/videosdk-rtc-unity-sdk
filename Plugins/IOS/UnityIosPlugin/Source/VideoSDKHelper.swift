//
//  VideoSDKHelper.swift
//  Unity-iPhone
//
//  Created by Uday Gajera on 05/12/24.
//

import VideoSDKRTC
import WebRTC
import Foundation

@_silgen_name("OnMeetingJoined")
func OnMeetingJoined(_ meetingId: UnsafePointer<CChar>, _ id: UnsafePointer<CChar>, _ name: UnsafePointer<CChar>)

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
func OnVideoFrameReceived(_ id: UnsafePointer<CChar>, _ data: UnsafePointer<CChar>)

@objc public class VideoSDKHelper: NSObject {
    
    @objc public static let shared = VideoSDKHelper()
    var meeting: Meeting?
    var participants: [Participant] = []
    var localParticipant: Participant?
    var webCamEnabled: Bool = false
    var micEnabled: Bool = false
    
    private let ciContext = CIContext(options: [.useSoftwareRenderer: false])
    private let compressionSettings: [CFString: Any] = [
        kCGImageDestinationLossyCompressionQuality as CFString: 0.5
    ]
    
    private var videoRenderers: [String: EmptyRenderer] = [:]
    
    @objc public func joinMeeting(token: String, meetingId: String, participantName: String, micEnabled: Bool, webCamEnabled: Bool, participantId: String) {
        VideoSDK.config(token: token)
        
        guard let customVideoTrack = try? VideoSDK.createCameraVideoTrack(encoderConfig: .h720p_w960p, facingMode: .front, multiStream: true) else {
            return
        }
        
        self.meeting = VideoSDK.initMeeting(
            meetingId: meetingId,
            participantId: participantId,
            participantName: participantName,
            micEnabled: micEnabled,
            webcamEnabled: webCamEnabled,
            customCameraVideoStream: customVideoTrack)
        
        self.webCamEnabled = webCamEnabled
        self.micEnabled = micEnabled
        
        guard let meeting = meeting else {
            // error callback
            return
        }
        meeting.addEventListener(self)
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
    
    @objc public func toggleWebCam(status: Bool) {
        guard let meeting = meeting else {
            // error callback
            return
        }
        if status {
            meeting.enableWebcam()
            self.webCamEnabled = true
        } else {
            meeting.disableWebcam()
            self.webCamEnabled = false
        }
    }
    
    @objc public func toggleMic(status: Bool) {
        guard let meeting = meeting else {
            // error callback
            return
        }
        if status {
            meeting.unmuteMic()
            self.micEnabled = true
        } else {
            meeting.muteMic()
            self.micEnabled = false
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
    
    private func participantToJson(_ participant: Participant) -> [String: Any] {
        return [
            "id": participant.id,
            "displayName": participant.displayName,
            "isLocal": participant.isLocal
        ]
    }
}

extension VideoSDKHelper: MeetingEventListener {
    public func onMeetingJoined() {
        guard let localParticipant = self.meeting?.localParticipant else { return }
        self.localParticipant = localParticipant
        participants.append(localParticipant)
        localParticipant.addEventListener(self)
        
        if let meetingIdPtr = ((meeting?.id ?? "") as NSString).utf8String,
           let idPtr = (localParticipant.id as NSString).utf8String,
           let namePtr = (localParticipant.displayName as NSString).utf8String {
            OnMeetingJoined(meetingIdPtr, idPtr, namePtr)
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
    
    public func onMeetingStateChanged(_ state: MeetingState) {
        if let statePtr = (state.rawValue as NSString).utf8String {
            OnMeetingStateChanged(statePtr)
        }
    }
    
    public func onError(_ error: Error) {
        if let errorPtr = (error.localizedDescription as NSString).utf8String {
            OnError(errorPtr)
        }
    }
}

@available(iOS 14.0, *)
extension VideoSDKHelper: ParticipantEventListener {
    
    public func onStreamEnabled(_ stream: MediaStream, forParticipant participant: Participant) {
        if stream.kind == .state(value: .video) {
            if participant.isLocal {
                self.webCamEnabled = true
            }
            HandleVideoStream(videoTrack: stream.track as? RTCVideoTrack, participant: participant)
        } else if stream.kind == .state(value: .audio) {
            if participant.isLocal {
                self.micEnabled = true
            }
            
            if let idPtr = (participant.id as NSString).utf8String,
               let dataPtr = ("audio" as NSString).utf8String {
                OnStreamEnabled(idPtr, dataPtr)
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
        let participantJSON = participantToJson(participant)
        
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
                    // Process only every 3rd frame to reduce memory pressure
                    if frame.timeStampNs % 2 != 0 {
                        return
                    }
                    
                    guard let i420Buffer = frame.buffer as? RTCI420Buffer else { return }
                    
                    autoreleasepool {
                        guard let pixelBuffer = self.createPixelBuffer(from: i420Buffer) else { return }
                        
                        autoreleasepool {
                            guard let imageData = self.compressFrame(pixelBuffer: pixelBuffer) else { return }
                            let base64String = imageData.base64EncodedString(options: [])
                            
                            DispatchQueue.main.async { [weak self] in
                                guard self != nil else { return }
                                if let idPtr = (participant.id as NSString).utf8String,
                                   let dataPtr = (base64String as NSString).utf8String {
                                    OnVideoFrameReceived(idPtr, dataPtr)
                                }
                            }
                        }
                        
                        CVPixelBufferUnlockBaseAddress(pixelBuffer, [])
                    }
                }
            }
        }
        
        videoTrack.add(renderer)
    }

    func compressFrame(pixelBuffer: CVPixelBuffer) -> Data? {
        CVPixelBufferLockBaseAddress(pixelBuffer, [])
        
        return autoreleasepool { () -> Data? in
            let ciImage = CIImage(cvPixelBuffer: pixelBuffer)
            
            // Scale down the image to reduce memory usage
            let scale = 0.75 // Reduce to 75% of original size
            let scaledExtent = ciImage.extent.applying(CGAffineTransform(scaleX: scale, y: scale))
            let scaledImage = ciImage.transformed(by: CGAffineTransform(scaleX: scale, y: scale))
            
            guard let cgImage = ciContext.createCGImage(scaledImage, from: scaledExtent,
                                                      format: .RGBA8,
                                                      colorSpace: CGColorSpaceCreateDeviceRGB()) else {
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

