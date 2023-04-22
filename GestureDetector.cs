using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;
}

public class GestureDetector : MonoBehaviour
{
    public float threshold = 0.1f;
    public OVRSkeleton skeleton;
    public List <Gesture> gestures;
    public List<OVRBone> fingerBones;
    public bool debugMode = true;

    private Gesture previousGesture;

    private void Start()
    {
        previousGesture = new Gesture();
    }
    // Update is called once per frame
    void Update()
    {
        if(debugMode && Input.GetKeyDown(KeyCode.Space)) { save(); }

        Gesture currentGesture = RecognizeGesture();
        bool hasRecognized = !currentGesture.Equals(new Gesture());
        if(hasRecognized && !previousGesture.Equals(currentGesture)) {
            print("Gesture Recognized : " +  currentGesture.name);
            previousGesture = currentGesture;
            currentGesture.onRecognized.Invoke();
        }
    }

    void save()
    {
        fingerBones = new List<OVRBone>(skeleton.Bones);
        Gesture g = new Gesture();
        g.name = "New Gesture";
        List<Vector3> data = new List<Vector3>();
        print(fingerBones.Count);
        foreach(var bone in fingerBones)
        {
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }
        g.fingerDatas = data;
        gestures.Add(g);
    }

    Gesture RecognizeGesture()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;
        fingerBones = new List<OVRBone>(skeleton.Bones);

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for(int i=0; i< fingerBones.Count; i++)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
                if(distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }
            if(!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }                
        }
        return currentGesture;
    }
}
