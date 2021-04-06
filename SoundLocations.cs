using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLocations : MonoBehaviour
{
    private struct SoundObject
    {
        private bool isActive;//If the sound is active.
        private Vector3 position;//The position of the sound.
        private float maxRadius;//The maxium radius the sound can reveal.
        private float currentRadius;//The current radius being revealed.
        private float lifeTime;//The lifetime of the sound.
        private float revealSpeed;//Speed the reveal happens.
        private float deathSpeed;
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public float MaxRadius
        {
            get { return maxRadius; }
            set { maxRadius = value; }
        }

        public float CurrentRadius
        {
            get { return currentRadius; }
            set { currentRadius = value; }
        }

        public float LifeTime
        {
            get { return lifeTime; }
            set { lifeTime = value; }
        }

        public float RevealSpeed
        {
            get { return revealSpeed; }
            set { revealSpeed = value; }
        }

        public float DeathSpeed
        {
            get { return deathSpeed; }
            set { deathSpeed = value; }
        }
        public bool LifeUp() { return lifeTime <= 0.0f; }
        public bool RadiusReached() { return currentRadius >= maxRadius; }
        public void IncreaseRadius() { currentRadius += revealSpeed * Time.deltaTime; }
        public void ReduceRadius() { currentRadius -= revealSpeed * Time.deltaTime; }
        //If the sound is not active and the current radius is greater than 0 return false
        //else is the the sound is active then return true
        public bool InactiveRadius() { return isActive == false ? currentRadius > 0.0f ? true : false : false; }
        public void ReduceLife() { lifeTime -= deathSpeed * Time.deltaTime; }
        public bool Inactive() { return isActive == false ? currentRadius <= 0.0f ? true : false : false; }
    }

    const int SOUNDLIMIT = 36;
    private SoundObject[] sounds;
    public Material MyMaterial;
    public Material ImportantMaterial;
    public Material EnemyMaterial;
    //Enemy range and index
    private Vector2Int enemyRange = new Vector2Int(1, 5);
    private int enemyIndex;
    //Brick range and index
    private Vector2Int brickRange = new Vector2Int(6, 12);
    private int brickIndex;
    //Player range and index
    private Vector2Int playerRange = new Vector2Int(13, 19);
    private int playerIndex;
    //Radio range and index
    private Vector2Int radioRange = new Vector2Int(20, 26);
    private int radioIndex;
    //Misc range and index
    private Vector2Int miscRange = new Vector2Int(27, 35);
    private int miscIndex;
    //Breath index
    private const int breathIndex = 0;

    //Player information
    [Header("Listener Info")]
    public Transform listenerTransform;
    public string listenerName = "Player";
    //Attentuation information
    [Header("Attenuation Info")]
    [Tooltip("The maximum range before the listener cannot hear the sounds.")]
    public float maxAttenuationRange = 40.0f;
    [Header("Base Sound Radius")]
    [Tooltip("The base radius for all sounds")]
    public float baseMaxRadius = 2.5f;
    [Tooltip("Multiplier for the base radius. Applied to enemy radius.")]
    public float enemyModifier = 1.0f;
    [Tooltip("Multiplier for the base radius. Applied to pickup radius.")]
    public float pickupModifier = 1.0f;
    [Tooltip("Multiplier for the base radius. Applied to radio radius.")]
    public float radioModifier = 1.0f;
    [Tooltip("Multiplier for the base radius. Applied to misc radius.")]
    public float miscModifier = 1.0f;

    public AK.Wwise.Event inhaleEvent;
    public AK.Wwise.Event exhaleEvent;
    private bool inhalePosted;
    // Start is called before the first frame update
    void Start()
    {
        sounds = new SoundObject[SOUNDLIMIT];
        sounds[breathIndex].Position = new Vector3();
        sounds[breathIndex].Position = transform.position;
        sounds[breathIndex].IsActive = false;
        sounds[breathIndex].LifeTime = 0.1f;
        sounds[breathIndex].MaxRadius = 3.5f;
        sounds[breathIndex].CurrentRadius = 0.0f;
        sounds[breathIndex].RevealSpeed = 4.0f;
        sounds[breathIndex].DeathSpeed = 0.4f;
        for (int i = 1; i < SOUNDLIMIT; i++)
        {
            sounds[i].Position = new Vector3();
            sounds[i].Position = Vector3.zero;
            sounds[i].IsActive = false;
            sounds[i].LifeTime = Random.Range(0.5f, 1.0f);
            sounds[i].MaxRadius = baseMaxRadius;
            sounds[i].CurrentRadius = 0.0f;
            sounds[i].RevealSpeed = 5.0f;
            sounds[i].DeathSpeed = Random.Range(0.1f, 0.3f);

        }

        MyMaterial.SetVectorArray("_SoundLocations", PositionArray());
        MyMaterial.SetFloatArray("_CurrentRadius", CurrentRadiusArray());
        ImportantMaterial.SetVectorArray("_SoundLocations", PositionArray());
        ImportantMaterial.SetFloatArray("_CurrentRadius", CurrentRadiusArray());
        EnemyMaterial.SetVectorArray("_SoundLocations", PositionArray());
        EnemyMaterial.SetFloatArray("_CurrentRadius", CurrentRadiusArray());
        enemyIndex = enemyRange.x;
        brickIndex = brickRange.x;
        playerIndex = playerRange.x;
        radioIndex = radioRange.x;
        miscIndex = miscRange.x;
        inhalePosted = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckLifeTimes();
        ChangeRadius();
        Breathe();

        var posArr = PositionArray();
        var radArr = CurrentRadiusArray();
        MyMaterial.SetVectorArray("_SoundLocations", posArr);
        MyMaterial.SetFloatArray("_CurrentRadius", radArr);
        ImportantMaterial.SetVectorArray("_SoundLocations", posArr);
        ImportantMaterial.SetFloatArray("_CurrentRadius", radArr);
        EnemyMaterial.SetVectorArray("_SoundLocations", posArr);
        EnemyMaterial.SetFloatArray("_CurrentRadius", radArr);
    }

    /// <summary>
    /// Posts a Wwise event using the gameobject supplied if the sound is not occluded. 
    /// Set any RTPC values before calling this method.
    /// </summary>
    /// <param name="postingObject">The gameobject that wishes to post an event.</param>
    /// <param name="postingEvent">The event which you wish to post.</param>
    /// <param name="attenuationRTPCName">The name of the object's attenuation RTPC.</param>
    public void PostEventAndAddLocation(GameObject postingObject, AK.Wwise.Event postingEvent, string attenuationRTPCName)
    {
        //If the object posting the event is the listener then no raycasting or attenuation is needed.
        if(postingObject.name == listenerName)
        {
            postingEvent.Post(postingObject);
            PlayFootstep();
            return;
        }

        float distance = Vector3.Distance(postingObject.transform.position, listenerTransform.position);

        if (distance > maxAttenuationRange)
        {
            return;
        }

        RaycastHit hit;
        Vector3 direction = listenerTransform.position - postingObject.transform.position;
        Vector3 startPosition = postingObject.transform.position;
        bool playerHit = false;
        float occlusionModifier = 1.0f;
        bool nothingHit = false;
        float rangeModifier = 0;
        while (!playerHit && !(occlusionModifier <= 0.0f) && !nothingHit)
        {
            if (Physics.Raycast(startPosition, direction, out hit, maxAttenuationRange - rangeModifier))
            {
               
                if (hit.collider.gameObject.name == listenerName)
                {
                    playerHit = true;
                    if (attenuationRTPCName != "")
                        AkSoundEngine.SetRTPCValue(attenuationRTPCName, distance, postingObject);

                    if(postingObject.name != "Radio")
                        postingEvent.Post(postingObject);

                    //The amount to modify the sound radius by.
                    float attenuationModifier = 1.0f;
                    //The percentage of sound being attenuated
                    float attenuationPercentage = ((distance / maxAttenuationRange));

                    attenuationModifier -= attenuationPercentage;
                    if (occlusionModifier < 0)
                        occlusionModifier = 0;

                    AkSoundEngine.SetRTPCValue("Occlusion_RTPC", occlusionModifier * 100.0f, postingObject);
                    switch (postingObject.name)
                    {
                        case "Pickup":
                            AddBrickSound(postingObject.transform.position, attenuationModifier, occlusionModifier);
                            break;
                        case "Enemy":
                            PlayEnemySound(postingObject.transform.position, attenuationModifier, occlusionModifier);
                            break;
                        case "Radio":
                            RadioPlayer radioPlayer;
                            if (postingObject.TryGetComponent(out radioPlayer))
                            {
                                radioPlayer.ActivateSound(attenuationModifier, occlusionModifier);
                            }
                            break;
                        case "Gate":
                            AddMiscLocation(postingObject.transform.position, attenuationModifier, occlusionModifier, "Gate");
                            break;
                        default:
                            AddMiscLocation(postingObject.transform.position, attenuationModifier, occlusionModifier, "Unknown");
                            break;

                    }

                }
                else
                {
                    switch(hit.collider.tag)
                    {
                        case "Wood":
                            occlusionModifier -= 0.1f;
                            break;
                        case "Metal":
                            occlusionModifier -= 0.2f;
                            break;
                         case "Stone":
                            occlusionModifier -= 0.8f;
                            break;
                        default:
                            occlusionModifier -= 0.1f;
                            break;
                    }
                    rangeModifier += Vector3.Magnitude(startPosition - hit.point);
                    startPosition = hit.point;
                    direction = listenerTransform.position - startPosition;
                    startPosition += direction * 0.01f;
                }
            }
            else
            {
                nothingHit = true;
            }
        }
    }
    
    private void AddMiscLocation(Vector3 location, float attenuationModifier, float occlusionModifier, string identifier)
    {
        if(sounds[miscIndex].Inactive())
        {
            sounds[miscIndex].Position = location;
            sounds[miscIndex].IsActive = true;
            sounds[miscIndex].CurrentRadius = 0f;;

            if (identifier == "Gate")
                sounds[miscIndex].MaxRadius = 4.0f;
            else
                sounds[miscIndex].MaxRadius = (((baseMaxRadius * miscModifier) * attenuationModifier) * occlusionModifier);

        }
        

        miscIndex++;
        if(miscIndex > miscRange.y)
            miscIndex = miscRange.x;
        
    }
    public int AddRadioLocation(Vector3 location, float attenuationModifier, float occlusionModifier, int myIndex)
    {
        if(myIndex != -1)
        {
            sounds[myIndex].Position = location;
            sounds[myIndex].IsActive = true;
            sounds[myIndex].MaxRadius = (((baseMaxRadius * radioModifier) * attenuationModifier) * occlusionModifier);
            sounds[myIndex].CurrentRadius = 0;
                
            sounds[myIndex].LifeTime = 0.1f;
            return myIndex;
        }

        bool inactiveSlot = false;
        for(int i = radioRange.x; i < radioRange.y; i++)
        {
            if(sounds[i].Inactive())
            {
                radioIndex = i;
                inactiveSlot = true;
                break;
            }
        }

        if (!inactiveSlot)
            return -1;

        sounds[radioIndex].Position = location;
        sounds[radioIndex].IsActive = true;
        sounds[radioIndex].CurrentRadius = 0f;
        sounds[radioIndex].MaxRadius = (((baseMaxRadius * radioModifier) * attenuationModifier) * occlusionModifier);

        sounds[radioIndex].LifeTime = 0.1f;

        return radioIndex;
    }
    
    public void RemoveRadioLocation(int index)
    {
        if(index != -1)
        {
            sounds[index].IsActive = false;
            sounds[index].Position = Vector3.zero;
        }
       
    }

    private void AddBrickSound(Vector3 location, float attenuationModifier, float occlusionModifier)
    {
        if (sounds[brickIndex].Inactive())
        {
            sounds[brickIndex].Position = location;
            sounds[brickIndex].IsActive = true;
            sounds[brickIndex].CurrentRadius = 0f;
            sounds[brickIndex].MaxRadius = (((baseMaxRadius * pickupModifier) * attenuationModifier) * occlusionModifier);
            sounds[brickIndex].LifeTime = Random.Range(0.1f, 0.5f);
            
        }
        brickIndex++;
        if (brickIndex > brickRange.y)
            brickIndex = brickRange.x;
    }
    /// <summary>
    /// Add a footstep location at the player's location.
    /// </summary>
    private void PlayFootstep()
    {
            sounds[playerIndex].Position = listenerTransform.position;
            sounds[playerIndex].IsActive = true;
            sounds[playerIndex].CurrentRadius = 0.2f;
            sounds[playerIndex].LifeTime = 0.2f;
        playerIndex++;

            if (playerIndex > playerRange.y)
            playerIndex = playerRange.x;
    }
    /// <summary>
    /// Add an enemy sound at the proovided location, modify the radius by how much the sound is attenuated and occluded.
    /// </summary>
    /// <param name="position">The position the sound is being played at.</param>
    /// <param name="attenuationModifier">How much the sound is being attenuated.</param>
    /// <param name="occlusionModifier">How much the sound is being occluded.</param>
    private void PlayEnemySound(Vector3 position, float attenuationModifier, float occlusionModifier)
    {
        sounds[enemyIndex].Position = position;
        sounds[enemyIndex].IsActive = true;
        sounds[enemyIndex].CurrentRadius = 0.2f;
        sounds[enemyIndex].MaxRadius = (((baseMaxRadius * enemyModifier) * attenuationModifier) * occlusionModifier);
        sounds[enemyIndex].LifeTime = 0.2f;
        enemyIndex++;

        if (enemyIndex > enemyRange.y)
            enemyIndex = enemyRange.x;
    }
    /// <summary>
    /// Checks the life time on every active sound.
    /// </summary>
    private void CheckLifeTimes()
    {
        for (int i = 0; i < SOUNDLIMIT; i++)
        {
            if(sounds[i].IsActive)
            {
                if (sounds[i].RadiusReached())
                {
                    sounds[i].ReduceLife();
                    if (sounds[i].LifeUp())
                    {
                        sounds[i].IsActive = false;
                    }
                }
            }
            
        }
    }
    /// <summary>
    /// Changes the current radius of a sound either up or down depending on if it is active.
    /// </summary>
    private void ChangeRadius()
    {
        for (int i = 1; i < SOUNDLIMIT; i++)
        {
            if (sounds[i].IsActive)
            {
                if (!sounds[i].RadiusReached())
                {
                    sounds[i].IncreaseRadius();
                }
            }
            else if (sounds[i].InactiveRadius())
            {
                sounds[i].ReduceRadius();
            }
        }
    }
    /// <summary>
    /// Performs the updates on the player's breathing sound.
    /// </summary>
    private void Breathe()
    {

        if (sounds[0].IsActive)
        {
            if (!sounds[0].RadiusReached())
            {
                sounds[0].IncreaseRadius();
            }
        }
        else if (sounds[0].InactiveRadius())
        {
            if (!inhalePosted)
            {
                inhaleEvent.Post(this.gameObject);
                inhalePosted = true;
            }
            sounds[0].ReduceRadius();
        }

        if (sounds[0].Inactive())
        {
            sounds[0].Position = listenerTransform.position;
            sounds[0].IsActive = true;
            sounds[0].LifeTime = 0.1f;
            sounds[0].CurrentRadius = 0.0f;
            exhaleEvent.Post(this.gameObject);
            inhalePosted = false;
        }
    }

    private float[] CurrentRadiusArray()
    {
        float[] arr = new float[SOUNDLIMIT];
        for (int i = 0; i < SOUNDLIMIT; i++)
        {
            arr[i] = sounds[i].CurrentRadius;
        }
        return arr;
    }

    private Vector4[] PositionArray()
    {
        Vector4[] arr = new Vector4[SOUNDLIMIT];
        for (int i = 0; i < SOUNDLIMIT; i++)
        {
            arr[i] = sounds[i].Position;
        }
        return arr;
    }

    public float[] CurrentRadiusArrayWithinRange(int start, int end)
    {
        float[] arr = new float[Mathf.Abs(start - end)];
        int arrIndex = 0;
        for (int i = start; i < end; i++)
        {
            arr[arrIndex] = sounds[i].CurrentRadius;
            arrIndex++;
        }
        return arr;
    }

    public Vector3[] PositionArrayWithinRange(int start, int end)
    {
        Vector3[] arr = new Vector3[Mathf.Abs(start - end)];
        int arrIndex = 0;
        for (int i = start; i < end; i++)
        {
            arr[arrIndex] = sounds[i].Position;
            arrIndex++;
        }
        return arr;
    }
    /// <summary>
    /// Returns the range vector for a given type.
    /// </summary>
    /// <param name="name">The name of the range you wish to retrieve</param>
    /// <returns></returns>
    public Vector2Int GetRangeValues(string name)
    {
        switch(name)
        {
            case "Enemy":
                return enemyRange;
            case "Player":
                return playerRange;
            case "Brick":
                return brickRange;
            case "Radio":
                return radioRange;
            case "Misc":
                return miscRange;
            default:
                return Vector2Int.zero;
        }
    }
   
}
