using System;
using System.Collections;
using UnityEngine;
//blend muuuuy lindo, sin necesidad de cinemachine
public class CameraTransition : MonoBehaviour
{
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform basketViewAnchor;
    [SerializeField] private float blendDuration = 0.5f;

    private Coroutine blendRoutine;

    public void TransitionToBasket(Action onComplete = null)
    {
        cameraFollow.enabled = false;
        StartBlend(basketViewAnchor.position, basketViewAnchor.rotation, onComplete);
    }

    public void TransitionToPlayer(Action onComplete = null)
    {
        Vector3 targetPos = cameraFollow.GetDesiredPosition();
        Quaternion targetRot = cameraFollow.FixedRotation;

        StartBlend(targetPos, targetRot, () =>
        {
            cameraFollow.enabled = true;
            onComplete?.Invoke();
        });
    }

    private void StartBlend(Vector3 targetPos, Quaternion targetRot, Action onComplete)
    {
        if (blendRoutine != null) StopCoroutine(blendRoutine);
        blendRoutine = StartCoroutine(BlendRoutine(targetPos, targetRot, onComplete));
    }

    private IEnumerator BlendRoutine(Vector3 targetPos, Quaternion targetRot, Action onComplete)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float t = 0f;

        while (t < blendDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / blendDuration);
            transform.position = Vector3.Lerp(startPos, targetPos, p);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, p);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        onComplete?.Invoke();
    }
}