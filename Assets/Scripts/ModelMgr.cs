using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelMgr : MonoBehaviour
{
    private Animator anim;
    private AudioSource AS;
    public List<AudioClip> audioClip;

    private AnimatorStateInfo info;

    private bool isStart = false;//是否开始讲述

    private void Awake()
    {
        EventCenter.GetInstance().AddEventListener(EventName.ChangeModeTo2DGuiding, changeModeTo2DGuiding);//添加事件

        MonoMgr.GetInstance().AddUpdateListener(modelUpdate);//添加帧更新事件
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        AS = GetComponent<AudioSource>();
    }

    private void modelUpdate()
    {
        info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("talking") && !isStart)//如果开始播放讲述动画
        {
            Introduce();
            isStart = true;
        }
    }

    private void changeModeTo2DGuiding(object sender, EventArgs e)//如果转换回2D导航，则将模型及其上的该脚本一并销毁
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 介绍景点
    /// </summary>
    private void Introduce()
    {
        AS.clip = audioClip[InfoPanel.desIndex - 1];//获取当前要播放的音频
        AS.Play();//播放音频
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener(EventName.ChangeModeTo2DGuiding, changeModeTo2DGuiding);//移除事件

        MonoMgr.GetInstance().RemoveUpdateListener(modelUpdate);//移除帧更新事件
    }
}
