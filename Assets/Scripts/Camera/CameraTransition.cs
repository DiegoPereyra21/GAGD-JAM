using System;
using System.Collections;
using UnityEngine;
//blend muuuuy lindo, sin necesidad de cinemachine
public class CameraTransition : MonoBehaviour
{
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private float blendDuration = 0.5f;

    private Coroutine blendRoutine;
    private Transform activeAnchor;


    //para que comience en la cama, como teniamosplaneada la historia
    [SerializeField] private Transform startingAnchor;

    private void Awake()
    {
        if (startingAnchor != null)
        {
            cameraFollow.enabled = false;
            transform.position = startingAnchor.position;
            transform.rotation = startingAnchor.rotation;
        }
    }
    //para q no este hardcodeado al basketViewAnchor nada mas
    public void TransitionTo(Transform anchor, Action onComplete = null)
    {
        cameraFollow.enabled = false;
        activeAnchor = null;

        StartBlend(anchor.position, anchor.rotation, () =>
        {
            activeAnchor = anchor;
            onComplete?.Invoke();
        });
    }

    public void TransitionToPlayer(Action onComplete = null)
    {
        activeAnchor = null;

        Vector3 targetPos = cameraFollow.GetDesiredPosition();
        Quaternion targetRot = cameraFollow.FixedRotation;

        StartBlend(targetPos, targetRot, () =>
        {
            cameraFollow.enabled = true;
            onComplete?.Invoke();
        });
    }

    private void LateUpdate()
    {
        if (activeAnchor != null && blendRoutine == null)
        {
            transform.position = activeAnchor.position;
            transform.rotation = activeAnchor.rotation;
        }
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
        blendRoutine = null;
        onComplete?.Invoke();
    }
}