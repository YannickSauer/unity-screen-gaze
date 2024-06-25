using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;

public class PupilLabsGaze : MonoBehaviour
{
    public SubscriptionsController subscriptionsController;
    private RequestController requestCtrl;
    public Vector2 gazePos; // relative gaze position returned by PupilLabs
    public Vector3 screenPos; // screen Position in px, z value is irrelevant
    public Ray gazeRay; // gaze ray in Unity coordinates, can be used for other simulations
    public bool showGazePoint = true;
    GameObject gazePoint; // Add a GameObject "GazePoint" to the scene that is placed at the current gaze location (intersection of gaze ray with the environment)
    Vector3 initalScale;

    void Awake()
    {
        requestCtrl = subscriptionsController.requestCtrl;
    }

    private void Start()
    {        
        gazePoint = GameObject.Find("GazePoint");
        initalScale = gazePoint.transform.localScale;
        if (!showGazePoint)
        {
            gazePoint.SetActive(false);
        }
    }

    void OnEnable()
    {
        requestCtrl.OnConnected += StartGazeSubscription;

        if (requestCtrl.IsConnected)
        {
            StartGazeSubscription();
        }
    }

    void OnDisable()
    {
        requestCtrl.OnConnected -= StartGazeSubscription;

        if (requestCtrl.IsConnected)
        {
            StopGazeSubscription();
        }
    }
    void Update()
    {
        if (requestCtrl == null) { return; }

        if (requestCtrl.IsConnected)
        {
            //Debug.Log("requestCtrl connected");
        }


      
        gazeRay = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;


        if (Physics.Raycast(gazeRay, out hit, 1000))
        {

            if (showGazePoint)
            {
                gazePoint.SetActive(true);
                gazePoint.transform.position = hit.point;
                gazePoint.transform.localScale = initalScale * hit.distance;
            }
        }
    }

    void StartGazeSubscription()
    {
        Debug.Log("StartGazeSubscription");

        subscriptionsController.SubscribeTo("surfaces.screen", CustomReceiveData);
    }

    void StopGazeSubscription()
    {
        Debug.Log("StopGazeSubscription");

        //requestCtrl.StopPlugin("Blink_Detection");

        subscriptionsController.UnsubscribeFrom("surfaces.screen", CustomReceiveData);
    }

    void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
    {
        LogCameraTrace(dictionary);
    }


    private void LogCameraTrace(Dictionary<string, object> dictionary)
    {
        if (dictionary.TryGetValue("gaze_on_surfaces", out object gaze_on_surf_obj))
        {
            //Debug.Log(gaze_on_surf_obj.GetType());
            object[] gaze_on_surf_list = (object[])gaze_on_surf_obj;
            Debug.Log(gaze_on_surf_list[0].GetType());

            Dictionary<object, object> gaze_on_surf = (Dictionary<object, object>)gaze_on_surf_list[0];
            if (gaze_on_surf.TryGetValue("norm_pos", out object norm_pos))
            { 
                if (norm_pos is object[] gazeArray && gazeArray.Length == 2)
                {
                    float x = Convert.ToSingle(gazeArray[0]);
                    float y = Convert.ToSingle(gazeArray[1]);
    
                    if (!HasNaNElements(new Vector3(x, y, 1)))
                    {
                        gazePos = new Vector2(x, y);
                        screenPos = new Vector3(Screen.currentResolution.width*x, Screen.currentResolution.height*y, 15.5f);
                    }
                    Debug.Log("screenPos: " + screenPos);
                }
                else
                {
                    Debug.LogError("Invalid 'norm_pos' type or length. Expected object array with two elements.");
                }
            }
            else
            {
                Debug.LogError("Invalid 'gaze_on_surfaces'");
            }
        }
        else
        {
            Debug.LogError("Key 'gaze_on_surfaces' not found in the dictionary.");
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
