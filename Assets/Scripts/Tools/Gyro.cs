using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gyro : MonoBehaviour
{

    private const float lowPassFilterFactor = 0.8f;//slerp����

    private Quaternion startQuaternion;

    private Quaternion originalQuaternion;

    private int frameCnt = 0;

    // Start is called before the first frame update
    void Start()
    {
        //�����豸�����ǵĿ���/�ر�״̬��ʹ�������ǹ��ܱ�������Ϊ true
        Input.gyro.enabled = true;
        //��ȡ�豸�������ٶ�����
        Vector3 deviceGravity = Input.gyro.gravity;
        //�豸����ת�ٶȣ����ؽ��Ϊx,y,z�����ת�ٶȣ���λΪ������/�룩
        Vector3 rotationVelocity = Input.gyro.rotationRate;
        //��ȡ���Ӿ�ȷ����ת
        Vector3 rotationVelocity2 = Input.gyro.rotationRateUnbiased;
        //���������ǵĸ��¼���ʱ�䣬����0.1�����һ��
        Input.gyro.updateInterval = 0.1f;
        //��ȡ�Ƴ��������ٶȺ��豸�ļ��ٶ�
        Vector3 acceleration = Input.gyro.userAcceleration;
    }

    // Update is called once per frame
    void Update()
    {
        frameCnt++;
        if (frameCnt > 5 && frameCnt <= 30)
        {
            originalQuaternion = transform.rotation;

            startQuaternion = new Quaternion(-1 * Input.gyro.attitude.x,
                -1 * Input.gyro.attitude.y,
                Input.gyro.attitude.z,
                Input.gyro.attitude.w);
            return;
        }

        Quaternion currentQuaternion = new Quaternion(-1 * Input.gyro.attitude.x, -1 * Input.gyro.attitude.y,
            Input.gyro.attitude.z, Input.gyro.attitude.w);

        transform.rotation = Quaternion.Slerp(transform.rotation,
            originalQuaternion * Quaternion.Inverse(startQuaternion) * currentQuaternion, lowPassFilterFactor);
    }
}
