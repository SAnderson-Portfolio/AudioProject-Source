using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateScript : MonoBehaviour
{
    public GameObject myButton;
    public Transform myDoor;

    private float doorSpeed = 10f;

    public AK.Wwise.Event buttonSound;
    public AK.Wwise.Event doorSound;
    public SoundLocations soundLocations;
    private bool isPressed = false;
    public string attenuationRTPCName = "Attenuation_RTPC";
    // Update is called once per frame
    void Update()
    {
        if(isPressed)
        {
            if(myDoor.position.y < 4.5f)
            {
                myDoor.position += Vector3.up * doorSpeed * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!isPressed && other.gameObject.name == "Player")
        {
            isPressed = true;
            soundLocations.PostEventAndAddLocation(this.gameObject, buttonSound, attenuationRTPCName);
            soundLocations.PostEventAndAddLocation(myDoor.gameObject, doorSound, attenuationRTPCName);
        }
    }
}
