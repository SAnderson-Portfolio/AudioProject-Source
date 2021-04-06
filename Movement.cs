using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Movement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    public float playerSpeed = 12.0f;
    public float gravityValue = -9.81f;
    public float mouseSensitivity = 300f;
    public float maxJumpHeight = 3f;
    public Camera playerCamera;
    float xRotation = 0f;
    public AK.Wwise.Event footstepSound;
    public AK.Wwise.Event radioSwitchSound;
    private float walkCount = 0.0f;
    private bool walking = false;
    public float footstepRate = 0.3f;
    private Vector3 move;
    public SoundLocations soundLocations;
    public float pickupRange = 100f;
    private bool[] inventory;
    private const int maxItems = 5;
    private int currentItems;
    public GameObject brickPrefab;
    
    private float myRTCP = 0;

    public int maxLives = 3;
    private int currentLives;
    public static Movement instance = null;
    private bool spawnPointSet;
    private Vector3 spawnPointLocation;
    //Post events
    //AkSoundEngine.PostEvent("Event_Namee", GameObject);
    //Switch States
    //AkSoundEngine.SetState("State_Group", "State_Name");
    //Set RTCP value
    //AkSoundEngine.SetRTPCValue("RTCP_Name", myRTCP);
    // Start is called before the first frame update

    public Canvas UI;
    public Text brickText;
    public Text livesText;
    public Text objectiveText;

    private bool musicPlaying;
    private bool sceneChanged;
    private bool firstUpdate;
    private int currentLevel;

    private bool deathPosted;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(UI);
            gameObject.name = "Player";
            currentLives = maxLives;
            currentItems = -1;
            spawnPointSet = false;
            controller = gameObject.AddComponent<CharacterController>();
            controller.stepOffset = 0.7f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            playerVelocity = Vector3.zero;
            inventory = new bool[maxItems];
            AkSoundEngine.SetState("Dead_or_Alive", "Alive");
            AkSoundEngine.SetRTPCValue("Music_RTCP", myRTCP);
            musicPlaying = false;
            sceneChanged = false;
            firstUpdate = false;
            brickText.text = "Bricks: " + (currentItems + 1);
            livesText.text = "Lives: " + currentLives;

            spawnPointLocation = new Vector3(0, 2.5f, 0);
            currentLevel = 1;
            deathPosted = false;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
      
    }
    private void FixedUpdate()
    {
        Vector2Int enemyRange = soundLocations.GetRangeValues("Enemy");
        Vector3[] enemyPositions = soundLocations.PositionArrayWithinRange(enemyRange.x, enemyRange.y);
        float[] enemyRadi = soundLocations.CurrentRadiusArrayWithinRange(enemyRange.x, enemyRange.y);
        for(int i = 0; i < enemyPositions.Length; i++)
        {
            if(Vector3.Magnitude(transform.position - enemyPositions[i]) < enemyRadi[i])
            {
                controller.enabled = false;
                gameObject.transform.SetPositionAndRotation(spawnPointLocation, new Quaternion(0, 0, 0, 0));
                controller.enabled = true;
                currentLives--;
                livesText.text = "Lives: " + currentLives;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(currentLives > 0)
        {
            if (sceneChanged && firstUpdate)
            {

                GameObject spawn = GameObject.FindGameObjectWithTag("Spawn Point");
                controller.enabled = false;
                gameObject.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
                controller.enabled = true;
                sceneChanged = false;
                firstUpdate = false;
            }

            if (sceneChanged)
                firstUpdate = true;


            HandleMovement();
            if (walking && controller.isGrounded)
            {
                walkCount += Time.deltaTime * (playerSpeed / 10.0f);

                if (walkCount > footstepRate)
                {
                    soundLocations.PostEventAndAddLocation(this.gameObject, footstepSound, "");
                    walkCount = 0.0f;
                }
            }


            if (Input.GetMouseButtonDown(0))
                RayCheck();

            if (Input.GetMouseButtonDown(1))
                ThrowItem();

            if (Input.GetKeyDown(KeyCode.L))
            {
                AkSoundEngine.StopAll();
                musicPlaying = false;
                SceneManager.LoadScene("BossScene");
                sceneChanged = true;
            }
        }
        else if(currentLives <= 0)
        {
            if(!deathPosted)
            {
                deathPosted = true;
                AkSoundEngine.SetState("Dead_or_Alive", "Dead");
                objectiveText.text = "Objective: Press R to restart or press escape to quit";
            }

            if (Input.GetKeyDown(KeyCode.R))
                RestartGame();

            if (Input.GetKeyDown(KeyCode.Escape))
                QuitGame();
        }
        
    }

    private void RayCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
        {
            if (hit.collider != null)
            {
                if(currentItems < maxItems)
                {
                    if (hit.collider.gameObject.name == "Pickup")
                    {
                        currentItems++;
                        inventory[currentItems] = true;
                        Destroy(hit.collider.gameObject);
                        brickText.text = "Bricks: " + (currentItems + 1);
                    }
                }
                

                if(hit.collider.name == "Radio")
                {
                    RadioPlayer radioPlayer;
                    if(hit.collider.gameObject.TryGetComponent(out radioPlayer))
                    {
                        radioPlayer.SetIsActive(!radioPlayer.GetIsActive());
                        soundLocations.PostEventAndAddLocation(this.gameObject, radioSwitchSound, "");
                    }
                }
            }
        }
    }

    private void ThrowItem()
    {
        int index = -1;
        for(int i = 0; i < maxItems; i++)
        {
            if(inventory[i] != false)
            {
                index = i;
                break;
            }
        }

        if(currentItems != -1)
        {
            inventory[currentItems] = false;
            var newObject = Instantiate(brickPrefab, playerCamera.transform.position + playerCamera.transform.forward, Quaternion.identity);
            newObject.GetComponent<Rigidbody>().AddForce(playerCamera.transform.position + playerCamera.transform.forward * 1000f);
            currentItems--;
            brickText.text = "Bricks: " + (currentItems + 1);
        }
    }

    private int CheckInventorySpace()
    {
        for(int i = 0; i < maxItems; i++)
        {
            if (!inventory[i])
                return i;
        }

        return -1;
    }
    private void HandleMovement()
    {
        bool groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        //Rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.Rotate(Vector3.up * mouseX);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //Movement
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        move = transform.right * x + transform.forward * z;
        controller.Move(move * playerSpeed * Time.deltaTime);
        
        if(Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(maxJumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;

        controller.Move(playerVelocity * Time.deltaTime);

        if (((Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.0f) ||
            (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.0f)))
        {
            walking = true;
        }
        else
        {
            walking = false;

            walkCount = footstepRate;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(!deathPosted)
        {
            if (other.name == "FirstRoom")
            {
                AkSoundEngine.SetSwitch("Footstep_Surface", "Carpet", this.gameObject);
                objectiveText.text = "Objective One: Find the button";
            }

            if (other.name == "SecondRoom")
            {
                objectiveText.text = "Objective Two: Find the button";
                AkSoundEngine.SetSwitch("Footstep_Surface", "Wood", this.gameObject);
            }

            if (other.name == "ThirdRoom")
            {
                AkSoundEngine.SetSwitch("Footstep_Surface", "Carpet", this.gameObject);
                objectiveText.text = "Objective Three: Turn off the radios.";
            }
            if (other.name == "BossApproach")
            {
                AkSoundEngine.SetSwitch("Footstep_Surface", "Water", this.gameObject);
                objectiveText.text = "Objective Four: Turn off all the radios as they turn on.";
            }

            if (other.name == "BossRoom")
            {

            }
        }
        

        if(other.name == "End")
        {
            AkSoundEngine.StopAll();
            musicPlaying = false;
            sceneChanged = true;
            if(currentLevel == 1)
            {
                SceneManager.LoadScene("BossScene");
                currentLevel = 2;
                
            }
            //else if(currentLevel == 2)
            //{
            //    SceneManager.LoadScene("RevealScene");
            //    currentLevel = 1;
            //}
        }

        if(other.name == "RespawnTrigger")
        {
            if(!spawnPointSet)
            {
                spawnPointSet = true;
                spawnPointLocation = other.gameObject.transform.GetChild(0).transform.position;
            }
           
        }

        if(!musicPlaying)
        {
            if(other.name == "LevelOneSpawn")
            {
                AkSoundEngine.PostEvent("Level_One_Game_Music", gameObject);
                musicPlaying = true;
            }
            else if(other.name == "BossSpawn")
            {
                AkSoundEngine.PostEvent("Level_Boss_Game_Music", this.gameObject);
                musicPlaying = true;
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.name == "Bullet" || collision.collider.name == "Enemy")
        {
            Destroy(collision.collider.gameObject);
            GameObject spawn = GameObject.FindGameObjectWithTag("Spawn Point");
            controller.enabled = false;
            gameObject.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
            controller.enabled = true;
            currentLives--;
            livesText.text = "Lives: " + currentLives;
        }
    }

    public int GetNumberOfBricks()
    {
        int number = 0;
        for (int i = 0; i < maxItems; i++)
        {
            if (inventory[i])
                number++;
        }

        return number;
    }


    public int GetCurrentLives()
    {
        return currentLives;
    }

    public void RestartGame()
    {
        if (currentLevel == 1)
        {
            //SceneManager.LoadScene("RevealScene");
            currentLives = 3;
            livesText.text = "Lives: " + currentLives;
            deathPosted = false;

        }
        else if (currentLevel == 2)
        {
            //SceneManager.UnloadSceneAsync("BossScene");
           // SceneManager.LoadScene("BossScene");
            currentLives = 3;
            livesText.text = "Lives: " + currentLives;
            deathPosted = false;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
