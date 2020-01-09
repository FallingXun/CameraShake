using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestDemo : MonoBehaviour
{
    public Button init;
    public Button shake;
    public Button cancel;
    public bool autoRotation = false;
    public Camera cam;
    public Transform target;
    public float speed = 1f;
    private void Awake()
    {
        if (init != null)
        {
            init.onClick.AddListener(CameraShakeManager.Instance.AddCamera);
        }
        if (shake != null)
        {
            shake.onClick.AddListener(() => { CameraShakeManager.Instance.Shake("Main Camera"); });
        }
        if (shake != null)
        {
            cancel.onClick.AddListener(() => { CameraShakeManager.Instance.Cancel("Main Camera"); });
        }
    }

    private void Update()
    {
        if (autoRotation)
        {
            if (cam != null)
            {
                cam.transform.RotateAround(target.transform.position,target.transform.up, Time.deltaTime * speed);
            }
        }
    }
}
