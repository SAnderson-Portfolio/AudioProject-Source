using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioPlayer : MonoBehaviour
{
    //float[][] pitches = { 440.0f * Mathf.Pow(2.0f, 0.0f / 12.0f), 440.0f * Mathf.Pow(2.0f, 2.0f / 12.0f), 440.0f * Mathf.Pow(2.0f, 4.0f / 12.0f),
    //            440.0f * Mathf.Pow(2.0f, 7.0f / 12.0f),
    //            440.0f * Mathf.Pow(2.0f, 9.0f / 12.0f) };

    float[,] pitches =  { { 550.00f * Mathf.Pow(2.0f, 0.0f / 12.0f), 550.00f *  Mathf.Pow(2.0f, 2.0f / 12.0f), 550.00f *  Mathf.Pow(2.0f, 4.0f / 12.0f), 550.00f *  Mathf.Pow(2.0f, 7.0f / 12.0f), 550.00f *  Mathf.Pow(2.0f, 9.0f / 12.0f) },
    { 1760.000f *  Mathf.Pow(2.0f, 0.0f / 12.0f),   1760.000f *  Mathf.Pow(2.0f, 2.0f / 12.0f), 1760.000f *  Mathf.Pow(2.0f, 4.0f / 12.0f), 1760.000f *  Mathf.Pow(2.0f, 7.0f / 12.0f), 1760.000f *  Mathf.Pow(2.0f, 9.0f / 12.0f) },
    { 1174.659f *  Mathf.Pow(2.0f, 0.0f / 12.0f),   1174.659f *  Mathf.Pow(2.0f, 1.0f / 12.0f), 1174.659f *  Mathf.Pow(2.0f, 4.0f / 12.0f), 1174.659f *  Mathf.Pow(2.0f, 7.0f / 12.0f), 1174.659f *  Mathf.Pow(2.0f, 9.0f / 12.0f) },
    { 2166.00f *  Mathf.Pow(2.0f, 0.0f / 12.0f),   2166.00f *  Mathf.Pow(2.0f, -2.0f / 12.0f),    2166.00f *  Mathf.Pow(2.0f, 4.0f / 12.0f), 2166.00f *  Mathf.Pow(2.0f, 7.0f / 12.0f), 2166.00f *  Mathf.Pow(2.0f, 9.0f / 12.0f) },
    { 440.00f *  Mathf.Pow(2.0f, 0.0f / 12.0f), 440.00f *  Mathf.Pow(2.0f, 7.0f / 12.0f),   440.00f *  Mathf.Pow(2.0f, -4.0f / 12.0f),  440.00f *  Mathf.Pow(2.0f, 9.0f / 12.0f),   440.00f *  Mathf.Pow(2.0f, 11.85f / 12.0f) } };

    public AK.Wwise.Event synthSound;
    public AK.Wwise.Event switchSound;
    public SoundLocations soundLocations;
    private float counter;
    [Header("RTPC Names")]
    public string amplitudeName = "Sine_Amplitude_RTPC";
    public string durationName = "Sine_Durationo_RTPC";
    public string frequencyName = "Sine_Frequency_RTPC";
    public string delayGainName = "Delay_Gain_RTPC";
    public string delayTimeName = "Delay_Time_RTPC";
    public string maxDelayName = "Max_Delay_RTPC";
    public string attenuationName = "Attenuation_RTPC";
    [Header("RTPC Values")]
    [Range(0, 100)]
    public float amplitudeValue;
    [Range(0, 100)]
    public float durationValue;
    [Range(0.0f, 5.0f)]
    public float maxDelayTime;
    [Range(0.0f, 5.0f)]
    public float delayTime;

    private bool isOn;
    private bool masterShutOff;
    private bool musicPlaying;
    private int myID;
    public float triggerTimer = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = "Radio";
        counter = 0.0f;
        isOn = true;
        masterShutOff = false;
        musicPlaying = false;
        myID = -1;
        AkSoundEngine.SetRTPCValue(durationName, durationValue, this.gameObject);
        AkSoundEngine.SetRTPCValue(maxDelayName, maxDelayTime, this.gameObject);
        if(soundLocations == null)
        {
            soundLocations = GameObject.FindGameObjectWithTag("WwiseGlobal").GetComponent<SoundLocations>();
        }
    }

    private void OnDestroy()
    {
        soundLocations.RemoveRadioLocation(myID);
    }
    // Update is called once per frame
    void Update()
    {
        if(synthSound.Name == "Synth_Trigger")
        {
            if(!masterShutOff)
            {
                if (isOn)
                {
                    counter += (triggerTimer * Time.deltaTime);
                    if (counter >= 1.0f)
                    {
                        float randNum = Random.Range(0.0f, 100.0f);
                        if (!(randNum > 85))
                        {
                            if (randNum >= 0 && randNum <= 10)//10%
                            {
                                AkSoundEngine.SetRTPCValue(frequencyName, pitches[0, Random.Range(0, 5)], this.gameObject);
                            }
                            else if (randNum >= 11 && randNum <= 40)//29%
                            {
                                AkSoundEngine.SetRTPCValue(frequencyName, pitches[1, Random.Range(0, 5)], this.gameObject);
                            }
                            else if (randNum >= 41 && randNum <= 50)//9%
                            {
                                AkSoundEngine.SetRTPCValue(frequencyName, pitches[3, Random.Range(0, 5)], this.gameObject);
                            }
                            else if (randNum >= 51 && randNum <= 70)//19%
                            {
                                AkSoundEngine.SetRTPCValue(frequencyName, pitches[4, Random.Range(0, 5)], this.gameObject);
                            }
                            else if (randNum >= 71 && randNum <= 85)//14%
                            {
                                AkSoundEngine.SetRTPCValue(frequencyName, pitches[2, Random.Range(0, 5)], this.gameObject);
                            }

                            AkSoundEngine.SetRTPCValue(amplitudeName, Random.Range(50.0f, 85.0f), this.gameObject);
                            AkSoundEngine.SetRTPCValue(delayGainName, Random.Range(40.0f, 60.0f), this.gameObject);
                            AkSoundEngine.SetRTPCValue(delayTimeName, Random.Range(0.10f, maxDelayTime), this.gameObject);
                            soundLocations.PostEventAndAddLocation(this.gameObject, synthSound, attenuationName);
                        }

                        counter -= 1.0f;
                    }
                }
            }
            
        }
        else if(synthSound.Name == "Radio_Event")
        {
            if (!masterShutOff)
            {
                if(isOn & !musicPlaying)
                {
                    soundLocations.PostEventAndAddLocation(this.gameObject, synthSound, attenuationName);
                }
            }
        }
       
       
    }

    public bool GetIsActive() { return isOn; }
    public void SetIsActive(bool newState)
    {
        if(!masterShutOff)
        {
            isOn = newState;
            musicPlaying = false;
            if (!isOn)
            {
                soundLocations.RemoveRadioLocation(myID);
                AkSoundEngine.StopAll(this.gameObject);
                myID = -1;
            }
        }
        
    }

    public void ActivateSound(float radiusModifier, float occlusionModifier)
    {
        myID = soundLocations.AddRadioLocation(this.gameObject.transform.position, radiusModifier, occlusionModifier, myID);

        if(synthSound.Name == "Radio_Event" && !musicPlaying)
        {
            synthSound.Post(this.gameObject, (uint)AkCallbackType.AK_MusicSyncBeat, CallbackFunction);
            musicPlaying = true;
        }
        else if(synthSound.Name == "Synth_Trigger")
        {
            synthSound.Post(this.gameObject);
        }
    }

    public void ChangeTrack(int trackNum)
    {
        switch(trackNum)
        {
            case 1:
                AkSoundEngine.SetState("Radio_Track", "One");
                break;
            case 2:
                AkSoundEngine.SetState("Radio_Track", "Two");
                break;
            case 3:
                AkSoundEngine.SetState("Radio_Track", "Three");
                break;
            case 4:
                AkSoundEngine.SetState("Radio_Track", "Four");
                break;
            default:
                AkSoundEngine.SetState("Radio_Track", "One");
                break;
        }
    }

    public void SetMasterShutOff(bool newState)
    {
        masterShutOff = newState;
    }

    void CallbackFunction(object in_cookie, AkCallbackType in_type, object in_info)
    {
        soundLocations.PostEventAndAddLocation(this.gameObject, synthSound, attenuationName);
    }
}
