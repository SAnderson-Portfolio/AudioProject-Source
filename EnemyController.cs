using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public SoundLocations soundLocations;
    public AK.Wwise.Event footstepSound = new AK.Wwise.Event();
    private CharacterController controller;
    private float walkCount = 0.0f;
    private bool walking = true;
    public float footstepRate = 0.3f;
    public float enemySpeed = 2f;
    public Transform[] transforms;
    private int currentPoint;
    public string attenuationRTPCName;
    // Start is called before the first frame update
    void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        controller.stepOffset = 0.7f;
        currentPoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = transforms[currentPoint].position - transform.position;
        direction.Normalize();
        controller.Move(direction * enemySpeed * Time.deltaTime);
        if(Vector3.Magnitude(transforms[currentPoint].position - transform.position) < 1.0f)
        {
            currentPoint++;
            if (currentPoint == transforms.Length)
                currentPoint = 0;
        }
        if (walking)
        {
            walkCount += Time.deltaTime * (enemySpeed / 10f);

            if (walkCount > footstepRate)
            {
                soundLocations.PostEventAndAddLocation(this.gameObject, footstepSound, attenuationRTPCName);
                //soundLocations.PlayEnemySound(transform.position);
                //footstepSound.Post(gameObject);
                walkCount = 0.0f;
            }
        }

        
    }
}
