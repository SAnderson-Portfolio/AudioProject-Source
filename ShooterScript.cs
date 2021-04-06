using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterScript : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject bulletPrefab;
    private BulletScript myBullet;
    public AK.Wwise.Event shootEvent;
    public SoundLocations soundLocations;
    public bool bulletFired;
    public bool shotDead;
    // Start is called before the first frame update
    void Start()
    {
        bulletFired = false;
        shotDead = false;
        if (soundLocations == null)
        {
            soundLocations = GameObject.FindGameObjectWithTag("WwiseGlobal").GetComponent<SoundLocations>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(bulletFired)
        {
            if(myBullet == null)
            {
                bulletFired = false;
                shotDead = true;
            }
        }
    }

    public bool ShotFiredAndDied()
    {
        return shotDead;
    }

    public bool GetBulletShot()
    {
        return bulletFired;
    }

    public void Fire()
    {
        bulletFired = true;
        shotDead = false;
        myBullet = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity).GetComponent<BulletScript>();
        Vector3 velocity = -gameObject.transform.right;
        velocity.Normalize();
        myBullet.SetVelocity(velocity);

        soundLocations.PostEventAndAddLocation(this.gameObject, shootEvent, "Attenuation_RTPC");
    }
}
