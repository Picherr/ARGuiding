using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class test1 : MonoBehaviour
{
    [SerializeField]
    private GameObject btnPlay;
    [SerializeField]
    private GameObject btnPause;
    [SerializeField]
    private GameObject btnAgain;
    [SerializeField]
    private Slider videoSlider;
    [SerializeField]
    private VideoPlayer videoPlayer;
    [SerializeField]
    private GameObject btnOpen;

    private bool isStart = false;

    public void Start()
    {
        
        btnPlay.GetComponent<Button>().onClick.AddListener(OnPlay);
        btnPause.GetComponent<Button>().onClick.AddListener(OnPause);
        btnAgain.GetComponent<Button>().onClick.AddListener(OnAgain);
        btnOpen.GetComponent<Button>().onClick.AddListener(OnOpen);
        //videoSlider.onValueChanged.AddListener(OnSliderValueChanged);
        videoSlider.GetComponent<Slider>().onValueChanged.AddListener(SliderEvent);
        btnPause.SetActive(false);
        btnAgain.SetActive(false);
    }

    private void Update()
    {
        if (videoPlayer.frame==(long)(videoPlayer.frameCount-1))//此时未播放但是已经开始播放，即结束
        {
            Debug.Log("isPlaying");
            btnAgain.SetActive(true);
        }
        videoSlider.value = (float)videoPlayer.time / (float)videoPlayer.clip.length;
    }

    private void OnPlay()
    {
        videoPlayer.Play();
        btnPlay.SetActive(false);
    }

    private void OnPause()
    {
        videoPlayer.Pause();
        btnPlay.SetActive(true);
    }

    private void OnAgain()
    {
        videoPlayer.time = 0;
        btnAgain.SetActive(false);
    }

    private void OnOpen()
    {
        btnPause.SetActive(true);
    }

    private void OnSliderValueChanged(float value)
    {
        videoPlayer.time = (long)(videoSlider.value * videoPlayer.clip.length);
    }

    public void SliderEvent(float value)
    {
        videoPlayer.frame = long.Parse((value * videoPlayer.frameCount).ToString("0."));
    }
}
