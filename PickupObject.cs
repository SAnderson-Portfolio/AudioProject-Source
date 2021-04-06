using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public SoundLocations soundLocations;
    public Transform playerTransform;
    private bool isPickedUp;
    private bool isActive;
    public bool thrown;
    public float attenuationRange;
    public AK.Wwise.Event brickSound;
    
    public string attenuationRTPCName;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = "Pickup";
        isPickedUp = false;
        isActive = true;
        soundLocations = GameObject.FindGameObjectWithTag("WwiseGlobal").GetComponent<SoundLocations>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        //AkSoundEngine.SetRTPCValue("Brick_RTPC", 10);
        //AkSoundEngine.SetRTPCValue("Coeff_RTPC", 50);
        attenuationRange = 30.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(isPickedUp && isActive)
        {
            gameObject.SetActive(false);
            isActive = false;
        }

        if(!isPickedUp && !isActive)
        {
            gameObject.SetActive(true);
            isActive = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(thrown)
        {
            soundLocations.PostEventAndAddLocation(this.gameObject, brickSound, attenuationRTPCName);
            //soundLocations.AddBrickSound(transform.position);
            //AkSoundEngine.PostEvent("Drop_Brick", gameObject);
        }
            
    }
}
