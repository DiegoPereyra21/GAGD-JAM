using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private Transform cinemachineCameraTransform;
    [SerializeField] private float blendDuration = 0.5f;
    [SerializeField] private Transform startingAnchor;

    private Coroutine blendRoutine;
    private Transform activeAnchor;

    private void Awake()
    {
        if (startingAnchor != null)
        {
            cinemachineBrain.enabled = false;
            transform.position = startingAnchor.position;
            transform.rotation = startingAnchor.rotation;
        }
    }

    public void TransitionTo(Transform anchor, Action onComplete = null)
    {
        cinemachineBrain.enabled = false;
        activeAnchor = null;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartBlend(anchor.position, anchor.rotation, () =>
        {
            activeAnchor = anchor;
            onComplete?.Invoke();
        });
    }

    public void TransitionToPlayer(Action onComplete = null)
    {
        activeAnchor = null;

        Vector3 targetPos = cinemachineCameraTransform.position;
        Quaternion targetRot = cinemachineCameraTransform.rotation;

        StartBlend(targetPos, targetRot, () =>
        {
            cinemachineBrain.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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