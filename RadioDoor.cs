using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioDoor : MonoBehaviour
{
    public RadioPlayer[] radioList;
    private bool doorOpen = false;
    private bool apexReached = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!doorOpen)
        {
            int radiosOff = 0;
            for (int i = 0; i < radioList.Length; i++)
            {
                if (!radioList[i].GetIsActive())
                {
                    radiosOff++;
                }
                else
                    break;
            }

            if (radiosOff == radioList.Length)
                doorOpen = true;
        }
        
        if(doorOpen && !apexReached)
        {
            transform.position += Vector3.up * (1.0f * Time.deltaTime);
            if (transform.position.y >= 6.8f)
                apexReached = true;
        }
    }

}
