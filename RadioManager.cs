using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioManager : MonoBehaviour
{
    public RadioPlayer[] myChildren;
    public Transform myDoor;
    public AK.Wwise.Event doorEvent;
    private int numRadios;
    private int currentActive;
    private int turnedOff;
    private bool[] wasActive;
    private bool doorOpen;
    private float currentRTPCValue;
    // Start is called before the first frame update
    void Start()
    {
        myChildren = GetComponentsInChildren<RadioPlayer>();
        numRadios = myChildren.Length;

        wasActive = new bool[numRadios];
        currentActive = 0;
        turnedOff = 0;
        doorOpen = false;
        wasActive[currentActive] = true;
        for(int i = 1; i < numRadios; i++)
        {
             myChildren[i].SetIsActive(false);
        }
        myChildren[currentActive].ChangeTrack(0);
        currentRTPCValue = 0;
        AkSoundEngine.SetRTPCValue("Music_RTPC", currentRTPCValue);
    }

    // Update is called once per frame
    void Update()
    {
        if(!doorOpen)
        {
            if (myChildren[currentActive].GetIsActive() == false)
            {
                myChildren[currentActive].SetMasterShutOff(true);
                turnedOff++;
                if (currentActive < numRadios - 1)
                {
                    currentActive++;
                    wasActive[currentActive] = true;
                    myChildren[currentActive].SetIsActive(true);
                    myChildren[currentActive].ChangeTrack(Random.Range(1, 4));
                    if (currentRTPCValue < 100)
                        currentRTPCValue += 20.0f;
                    AkSoundEngine.SetRTPCValue("Music_RTPC", currentRTPCValue);
                }
            }

            
        }
        else if(doorOpen)
        {
            if (myDoor.position.y < 9.2f)
            {
                myDoor.position += Vector3.up * 12.0f * Time.deltaTime;
            }
        }

        if (turnedOff >= numRadios - 1)
        {
            doorOpen = true;
        }

    }
}
