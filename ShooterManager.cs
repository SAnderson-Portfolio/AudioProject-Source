using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterManager : MonoBehaviour
{
    public ShooterScript[] myChildren;
    private int maxBullets;
    private int bulletsFired;
    private int currentBulletLimit;
    private int[] activeObjectIDs;
    // Start is called before the first frame update
    void Start()
    {
        myChildren = GetComponentsInChildren<ShooterScript>();
        maxBullets = myChildren.Length;
        activeObjectIDs = new int[maxBullets];

        for (int i = 0; i < activeObjectIDs.Length; i++)
            activeObjectIDs[i] = -1;

        currentBulletLimit = 2;
        bulletsFired = 0;
        for(int i = 0; i < currentBulletLimit; i++)
        {
            if(myChildren[i])
            {
                int randInt = Random.Range(0, maxBullets);
                if (!myChildren[randInt].GetBulletShot())
                {
                    activeObjectIDs[randInt] = randInt;
                    myChildren[randInt].Fire();
                    bulletsFired++;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < maxBullets; i++)
        {
            if(activeObjectIDs[i] != -1)
            {
                if (myChildren[i].ShotFiredAndDied())
                {
                    activeObjectIDs[i] = -1;
                    bulletsFired--;
                }
            }
            
        }

        if(bulletsFired < currentBulletLimit)
        {
            int randInt = Random.Range(0, maxBullets);
            if(activeObjectIDs[randInt] == -1)
            {
                myChildren[randInt].Fire();
                activeObjectIDs[randInt] = randInt;
                bulletsFired++;
            }
            else
            {
                while(activeObjectIDs[randInt] != -1)
                {
                    randInt = Random.Range(0, maxBullets);
                }

                if (activeObjectIDs[randInt] == -1)
                {
                    myChildren[randInt].Fire();
                    activeObjectIDs[randInt] = randInt;
                    bulletsFired++;
                }
            }
        }
    }
}

