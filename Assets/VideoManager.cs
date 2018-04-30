using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour {
    GameObject camera;
    
    public bool playingVideo;

    void Start() {
        Object.DontDestroyOnLoad(this);
        playingVideo = false;
    }

    public void PlayVideo(string videoUrl) {
        playingVideo = true;
        camera = GameObject.Find("Main Camera");
        VideoPlayer videoPlayer = camera.AddComponent<VideoPlayer>();
        AudioSource audioSource = camera.AddComponent<AudioSource>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.url = videoUrl;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Play();
        audioSource.Play();
    }

    void EndReached(VideoPlayer vp) {
        playingVideo = false;
        Destroy(camera.GetComponent<VideoPlayer>());
    }
}
