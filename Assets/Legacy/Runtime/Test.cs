using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test
{
    public void Update()
    {
        //Debug.Log("test");
    }
}

public class Test : MonoBehaviour
{
    public List<AudioClip> clips;
    public AudioSource AS;
    public AnimatorStateInfo info;

    public Animator anim;
    public bool isStart = false;

    private void Start()
    {
        //test t = new test();
        //MonoMgr.GetInstance().AddUpdateListener(t.Update);
        GetComponent<AudioSource>().clip = clips[0];

    }

    private void Update()
    {
        info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("talking") && !isStart)
        {
            AS.Play();
            isStart = true;
        }
    }
}
