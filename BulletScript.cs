using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Vector3 velocity;
    public float bulletSpeed;
    public AK.Wwise.Event bulletEvent;
    public SoundLocations soundLocations;
    // Start is called before the first frame update
    void Start()
    {
        if (soundLocations == null)
        {
            soundLocations = GameObject.FindGameObjectWithTag("WwiseGlobal").GetComponent<SoundLocations>();
        }
        name = "Bullet";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        gameObject.GetComponent<Transform>().position += velocity * bulletSpeed * Time.deltaTime;
          
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!collision.collider.CompareTag("Bullet"))
        {
            soundLocations.PostEventAndAddLocation(this.gameObject, bulletEvent, "Attenuatuion_RTPC");
            Destroy(gameObject);
        }
    }
}
