using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;

public class PupilLabsHeadpose : MonoBehaviour
{
    public SubscriptionsController subscriptionsController;
    private RequestController requestCtrl;
    public Vector3 origin = new Vector3(0, 0, 0);
    public Vector3 trackerPos;
    public float scaling = 0.1f;
    void Awake()
    {
        requestCtrl = subscriptionsController.requestCtrl;
    }

    void OnEnable()
    {
        requestCtrl.OnConnected += StartHeadposeSubscription;

        if (requestCtrl.IsConnected)
        {
            StartHeadposeSubscription();
        }
    }

    void OnDisable()
    {
        requestCtrl.OnConnected -= StartHeadposeSubscription;

        if (requestCtrl.IsConnected)
        {
            StopHeadposeSubscription();
        }
    }
    void Update()
    {
        if (requestCtrl == null) { return; }

        if (requestCtrl.IsConnected)
        {
            Debug.Log("requestCtrl connected");
        }
        this.transform.position = origin + trackerPos;
    }

    void StartHeadposeSubscription()
    {
        Debug.Log("StartHeadposeSubscription");

        subscriptionsController.SubscribeTo("head_pose", CustomReceiveData);

    }

    void StopHeadposeSubscription()
    {
        Debug.Log("StopHeadpostSubscription");
        subscriptionsController.UnsubscribeFrom("head_pose", CustomReceiveData);
    }

    void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
    {
        LogCameraTrace(dictionary);

    }


    private void LogCameraTrace(Dictionary<string, object> dictionary)
    {
        if (dictionary.TryGetValue("camera_trace", out object cameraTraceObject))
        {
            if (cameraTraceObject is object[] cameraTraceArray && cameraTraceArray.Length == 3)
            {
                float x = Convert.ToSingle(cameraTraceArray[0]);
                float y = Convert.ToSingle(cameraTraceArray[1]);
                float z = Convert.ToSingle(cameraTraceArray[2]);

                if (!HasNaNElements(new Vector3(x, y, z)))
                {
                    trackerPos = scaling * new Vector3(x, y, -z);
                }
                Debug.Log("Camera Trace Vector: " + trackerPos);
            }
            else
            {
                Debug.LogError("Invalid 'camera_trace' type or length. Expected object array with three elements.");
            }
        }
        else
        {
            Debug.LogError("Key 'camera_trace' not found in the dictionary.");
        }
    }
    private bool HasNaNElements(Vector3 vector)
    {
        return float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z);
    }
    private void LogDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        foreach (var kvp in dictionary)
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
        }
    }
}
