using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuidingPanel : BasePanel
{
    [SerializeField]
    private TextMeshProUGUI guidingText;
    [SerializeField]
    private TextMeshProUGUI desName;
    [SerializeField]
    private TextMeshProUGUI disMiles;

    protected override void Awake()
    {
        base.Awake();
        EventCenter.GetInstance().AddEventListener(EventName.UpdateGuidingInfo, UpdateGuidingInfo);//添加事件
    }

    protected override void Start()
    {
        base.Start();
        guidingText = UIManager.GetInstance().GetPanel<GuidingPanel>("GuidingPanel").GetControl<TextMeshProUGUI>("GuidingText");
        desName = UIManager.GetInstance().GetPanel<GuidingPanel>("GuidingPanel").GetControl<TextMeshProUGUI>("DesName");
        disMiles = UIManager.GetInstance().GetPanel<GuidingPanel>("GuidingPanel").GetControl<TextMeshProUGUI>("DisMiles");
    }

    private void UpdateGuidingInfo(object sender, EventArgs e)
    {
        var data = e as UpdateGuidingInfoArgs;
        if (data != null)
        {
            guidingText.text = data.guidingText;
            desName.text = data.desName;
            disMiles.text = data.disMiles;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventCenter.GetInstance().RemoveEventListener(EventName.UpdateGuidingInfo, UpdateGuidingInfo);//移除事件
    }
}
