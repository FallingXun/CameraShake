using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // 震屏相机
    public Camera cam;

    #region 震屏参数（配置）
    // 震动次数
    public int numberOfShakes = 2;
    // 位移向量
    public Vector3 shakeAmount = Vector3.one;
    // 旋转向量
    public Vector3 rotationAmount = Vector3.one;
    // 距离
    public float distance = 0.1f;
    // 速度
    public float speed = 50f;
    // 衰减
    public float decay = 0.2f;
    // 延迟
    public float delay = 0.1f;
    // 拉近镜头持续时间
    public float during = 1f;
    #endregion

    // 震屏中
    private bool shaking = false;
    // 拉近镜头中
    private bool pulling = false;
    // 镜头的FieldOfView默认值
    private float defaultFieldOfView = 0;
    // 检查震屏和拉近的最小值
    private const bool checkForMininumValue = true;
    // 震屏距离最小值
    private const float minShakeValue = 0.001f;
    // 旋转强度最小值
    private const float minRotationValue = 0.001f;

    private void Awake()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
    }

    #region 震屏
    public void DoShake()
    {
        if (cam == null)
        {
            return;
        }
        Vector3 seed = Random.insideUnitSphere;
        if (shaking)
        {
            return;
        }
        StartCoroutine(DoShake_Internal(this.cam, seed, numberOfShakes, this.shakeAmount, this.rotationAmount, this.distance, this.speed, this.decay, this.delay));
    }

    private IEnumerator DoShake_Internal(Camera cam, Vector3 seed, int numberOfShakes, Vector3 shakeAmount, Vector3 rotationAmount, float distance, float speed, float decay, float delay)
    {
        shaking = true;
        // 震屏延迟
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        Transform cachedTransform = cam.transform;

        // seed可作为随机数影响震动
        var mod1 = seed.x > 0.5 ? 1 : 1;
        var mod2 = seed.y > 0.5 ? 1 : 1;
        var mod3 = seed.z > 0.5 ? 1 : 1;

        // 开始时间
        float startTime = Time.time;

        // 相机位移变量
        Vector3 camOffset = Vector3.zero;
        // 相机旋转变量
        Quaternion camRot = Quaternion.identity;
        // 位移起始位置
        Vector3 start = Vector3.zero;
        // 旋转起始角度
        Quaternion startRot = Quaternion.identity;
        // 有TimeScale等时间缩放时可调整
        float scale = 1;

        int currentShakes = numberOfShakes;
        float rotationStrength = 1f;
        float shakeDistance = distance;

        while (currentShakes > 0)
        {
            // 检查最小值限制，提前结束震屏
            if (checkForMininumValue)
            {
                if (rotationAmount.sqrMagnitude != 0 && rotationStrength <= minRotationValue)
                {
                    break;
                }
                if (shakeAmount.sqrMagnitude != 0 && distance != 0 && shakeDistance <= minShakeValue)
                {
                    break;
                }
            }

            // 时间变量
            float timer = (Time.time - startTime) * speed;
            // 位移
            var shakeOffset = start + new Vector3(
                mod1 * Mathf.Sin(timer) * (shakeAmount.x * shakeDistance * scale),
                mod2 * Mathf.Sin(timer) * (shakeAmount.y * shakeDistance * scale),
                mod3 * Mathf.Sin(timer) * (shakeAmount.z * shakeDistance * scale)
                );
            var shakeRotation = startRot * Quaternion.Euler(
                mod1 * Mathf.Cos(timer) * (rotationAmount.x * rotationStrength * scale),
                mod2 * Mathf.Cos(timer) * (rotationAmount.y * rotationStrength * scale),
                mod3 * Mathf.Cos(timer) * (rotationAmount.z * rotationStrength * scale)
                );

            camOffset = GetGeometricAvg(shakeOffset);
            camRot = GetRotationAvg(shakeRotation);

            // 构建变换矩阵，修改worldToCameraMatrix后，在调用ResetWorldToCameraMatrix方法前都不会恢复
            Matrix4x4 m = Matrix4x4.TRS(camOffset, camRot, new Vector3(1, 1, -1));
            cam.worldToCameraMatrix = m * cachedTransform.worldToLocalMatrix;

            // 震屏周期
            if (timer > 2 * Mathf.PI)
            {
                startTime = Time.time;
                shakeDistance *= (1 - Mathf.Clamp01(decay));
                rotationStrength *= (1 - Mathf.Clamp01(decay));
                --currentShakes;
            }
            yield return null;
        }
        ResetShake(this.cam);
    }

    private void ResetShake(Camera cam)
    {
        if (cam != null)
        {
            cam.ResetWorldToCameraMatrix();
        }
        shaking = false;
    }

    // 获取位移
    private Vector3 GetGeometricAvg(Vector3 shakeOffset)
    {
        return -1 * shakeOffset;
    }

    // 获取旋转角度
    private Quaternion GetRotationAvg(Quaternion shakeRotation)
    {
        Quaternion avg = new Quaternion(0, 0, 0, 0);
        // -90~90
        if (Quaternion.Dot(shakeRotation, avg) > 0)
        {
            avg.x += shakeRotation.x;
            avg.y += shakeRotation.y;
            avg.z += shakeRotation.z;
            avg.w += shakeRotation.w;
        }
        else
        {
            avg.x -= shakeRotation.x;
            avg.y -= shakeRotation.y;
            avg.z -= shakeRotation.z;
            avg.w -= shakeRotation.w;
        }

        var mag = avg.x * avg.x + avg.y * avg.y + avg.z * avg.z + avg.w * avg.w;

        var sqr = Mathf.Sqrt(mag);
        if (sqr > 0.0001f)
        {
            avg.x /= sqr;
            avg.y /= sqr;
            avg.z /= sqr;
            avg.w /= sqr;
        }
        else
        {
            avg = shakeRotation;
        }
        return avg;
    }
    #endregion

    #region 镜头拉伸
    public void DoPull()
    {
        if (cam == null)
        {
            return;
        }
        if (pulling)
        {
            return;
        }
        if (during <= 0)
        {
            return;
        }
        if (defaultFieldOfView <= 0)
        {
            defaultFieldOfView = cam.fieldOfView;
        }

        StartCoroutine(DoPull_Internal(this.cam, this.distance, this.decay, this.delay, this.during));
    }

    private IEnumerator DoPull_Internal(Camera cam, float distance, float decay, float delay, float during)
    {
        pulling = true;
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        float targetFieldOfView = defaultFieldOfView;
        // 拉近镜头，减小FieldOfView
        if (distance > 0)
        {
            targetFieldOfView -= defaultFieldOfView * Mathf.Clamp01(distance);
        }
        else
        {
            targetFieldOfView += defaultFieldOfView * Mathf.Clamp01(distance);
        }
        if (targetFieldOfView <= 0)
        {
            targetFieldOfView = defaultFieldOfView;
        }
        while (cam.fieldOfView > targetFieldOfView)
        {
            cam.fieldOfView -= defaultFieldOfView * delay;
            yield return null;
        }
        yield return new WaitForSeconds(during);
        while (cam.fieldOfView < defaultFieldOfView)
        {
            cam.fieldOfView += defaultFieldOfView * delay;
            yield return null;
        }
        ResetPull(this.cam);
    }

    private void ResetPull(Camera cam)
    {
        if (cam != null)
        {
            cam.fieldOfView = defaultFieldOfView;
        }
        pulling = false;
    }
    #endregion

    #region 取消震屏
    public void DoCancel()
    {
        this.StopAllCoroutines();
        ResetShake(this.cam);
        ResetPull(this.cam);
    }
    #endregion
}
