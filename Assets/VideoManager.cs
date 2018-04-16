using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
        videoPlayer.url = videoUrl;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Play();
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp) {
        playingVideo = false;
        Destroy(camera.GetComponent<UnityEngine.Video.VideoPlayer>());
    }
}
