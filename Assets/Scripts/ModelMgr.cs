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

    private bool isStart = false;//�Ƿ�ʼ����

    private void Awake()
    {
        EventCenter.GetInstance().AddEventListener(EventName.ChangeModeTo2DGuiding, changeModeTo2DGuiding);//����¼�

        MonoMgr.GetInstance().AddUpdateListener(modelUpdate);//���֡�����¼�
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        AS = GetComponent<AudioSource>();
    }

    private void modelUpdate()
    {
        info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("talking") && !isStart)//�����ʼ���Ž�������
        {
            Introduce();
            isStart = true;
        }
    }

    private void changeModeTo2DGuiding(object sender, EventArgs e)//���ת����2D��������ģ�ͼ����ϵĸýű�һ������
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// ���ܾ���
    /// </summary>
    private void Introduce()
    {
        AS.clip = audioClip[InfoPanel.desIndex - 1];//��ȡ��ǰҪ���ŵ���Ƶ
        AS.Play();//������Ƶ
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener(EventName.ChangeModeTo2DGuiding, changeModeTo2DGuiding);//�Ƴ��¼�

        MonoMgr.GetInstance().RemoveUpdateListener(modelUpdate);//�Ƴ�֡�����¼�
    }
}
