using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    private Dictionary<string, CameraShake> dic = new Dictionary<string, CameraShake>();

    private static CameraShakeManager instance;
    public static  CameraShakeManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CameraShakeManager");
                instance = go.AddComponent<CameraShakeManager>();
            }
            return instance;
        }
    }

    public void AddCamera()
    {
        if(dic.ContainsKey("Main Camera") == false)
        {
            GameObject go = GameObject.Find("Main Camera");
            if (go != null)
            {
                CameraShake shake = go.AddComponent<CameraShake>();
                dic[go.name] = shake;
            }
        }
    }

    public void Shake(string camName)
    {
        if (dic.ContainsKey(camName))
        {
            CameraShake cameraShake = dic[camName];
            cameraShake.DoShake();
            cameraShake.DoPull();
        }
    }

    public void Cancel(string camName)
    {
        if (dic.ContainsKey(camName))
        {
            CameraShake cameraShake = dic[camName];
            cameraShake.DoCancel();
        }
    }
}
