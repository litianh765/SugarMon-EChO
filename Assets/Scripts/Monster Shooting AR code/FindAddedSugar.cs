﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Text;
using System.Threading.Tasks;
using System;
using TMPro;


public class FindAddedSugar : MonoBehaviour
{
    //ref of singleton 
    UIManager um;
    private static FindAddedSugar instance;
    public static FindAddedSugar Instance { get { return instance; } }
    public static List<string> repository = new List<string>();
    private static List<string> db = new List<string>();                 //The list of products
    public List<List<string>> dbList = new List<List<string>>();         //The list of lists of product information
    public AudioSource Audio;
    public RuntimeAnimatorController animController;
    private SimpleDemo simpleDemo;
    private bool wasSkipped = false;


    [SerializeField]
    string superCode;                                                     //The barcode including all sugars for testing

    private int currentNumMonster = 0;                                    //Counter for displaying sugar cards found from the scanned product

    public TextAsset sugarRepository;                                     //Sugar repository database

    public string toggleOption;
    public Button toggleButton;

    public Sprite Sound;
    public Sprite Vibrate;
    public Sprite Mute;

    public bool vibrate;
    public bool sound;
    private bool soundInitialized = false;
    public AudioSource goodSound;
    public AudioSource badSound;
    public AudioSource unknownSound;
    public AudioSource onSound;
    public AudioSource sweepSound;
    private bool firstBadSound;

    public GameObject scanFrame;
    public GameObject greenCartGo;
    public GameObject greenCartBtn;
    public GameObject dropdownMenu;

    private int numCount;                                                 //the number of found sugar
    public GameObject sugarDex, redDot, canvas, familyBackground, mainCam;
    public GameObject totalCount, foundCount;



    //Must follow the title in Database.txt
    [Header("Column names")]
    [Tooltip("In put must be exactly the same with the titles in Database.txt")]
    public string numberInAppColumn = "Number in the App";
    public string sugarNameColumn = "Added Sugar List Name";
    public string numberInRepositoryColumn = "Number in Added Sugar Repository";
    public string monsterFamilyColumn = "MonstersFamily";
    public string descriptionColumn = "Description";


    [HideInInspector]
    public int familyIndex, deckNumIndex, nameIndex, repoNumIndex, familyNum, descriptionIndex;

    public List<string> sugarInWall;                                      //The list of types of found sugar
    public List<string> fms = new List<string>();                         //The list of families
    public List<string> scannedAddedSugars = new List<string>();          //The list of types of sugar in the scanned product
    public List<string> allScanned = new List<string>();

    [HideInInspector]
    public Dictionary<string, int> familyDictionary;                      //The list of pair of family names and the number of sugars in the family
    
    private GameObject monster;

    private int ts;                                                       //tutorial stage to display different tutorial masks

    // Use this for initialization
    void Awake()
    {
        if (instance != null) Destroy(this);
        else instance = this;

        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        Sound = Resources.Load<Sprite>("Images/Sound") as Sprite;
        Vibrate = Resources.Load<Sprite>("Images/Vibrate") as Sprite;
        Mute = Resources.Load<Sprite>("Images/Mute") as Sprite;

        if (PlayerPrefs.HasKey("ToggleOption"))
        {
            toggleOption = PlayerPrefs.GetString("ToggleOption");
            switch (toggleOption)
            {
                case "Sound":
                    SetSound();
                    break;
                case "Vibrate":
                    SetVibrate();
                    break;
                case "Mute":
                    SetMute();
                    break;
            }
        } else
        {
            SetSound();
        }
        firstBadSound = true;
        soundInitialized = true;
    }

    void Start()
    {

        greenCartGo.gameObject.SetActive(false);

        //Get singleton ref
        um = UIManager.Instance;
        
        //Load the number of the previous found sugars
        numCount = PlayerPrefs.GetInt("count");

        //Load the types of the previous found sugars
        for (int i = 1; i <= PlayerPrefs.GetInt("count"); i++)
        {
            sugarInWall.Add(PlayerPrefs.GetString("num_" + i));
        }

        //Read Database         
        string dbContent = Encoding.UTF8.GetString(sugarRepository.bytes);
        db = dbContent.Split(new char[] { '\n' }).ToList();
        
        //Save data in a list of lists
        for (int i = 0; i < db.Count; i++)
        {
            dbList.Add(db[i].Split(new char[] { '\t' }).ToList());
            dbList[i] = dbList[i].ConvertAll(item => item.Trim());
        }

        familyIndex = dbList[0].IndexOf(monsterFamilyColumn);
        deckNumIndex = dbList[0].IndexOf(numberInAppColumn);
        nameIndex = dbList[0].IndexOf(sugarNameColumn);
        repoNumIndex = dbList[0].IndexOf(numberInRepositoryColumn);  //use only when you need the sugar index in sugar repository
        descriptionIndex = dbList[0].IndexOf(descriptionColumn);
        //Find out how many different families and how many type of sugar they contain 
        //Save in familyDictionary [family name, count]
        //Save all types of added sugar in the repository variable
        familyDictionary = new Dictionary<string, int>();
        repository = new List<string>();
        foreach (List<string> item in dbList)
        {
            repository.Add(item[nameIndex].ToLower());
            if (!familyDictionary.ContainsKey(item[familyIndex]))
            {
                familyDictionary.Add(item[familyIndex], 1);
            }
            else
            {
                int count = 0;
                familyDictionary.TryGetValue(item[familyIndex], out count);
                familyDictionary.Remove(item[familyIndex]);
                familyDictionary.Add(item[familyIndex], count + 1);
            }
        }

        fms = familyDictionary.Keys.ToList();

        //Remove title
        fms.RemoveAt(0);
        repository.RemoveAt(0);
        familyNum = fms.Count;

        //Initiate sugar dex
        for (int i = 1; i <= PlayerPrefs.GetInt("count"); i++)
        {
            sugarDex.GetComponent<SugarDisk>().allCollectedSugars.Add(PlayerPrefs.GetString("num_" + i));
            allScanned.Add(PlayerPrefs.GetString("num_" + i));
        }

        //Remove duplicates
        allScanned.Distinct().ToList();

        familyBackground.gameObject.SetActive(true);
        GameObject.Find("FamilyContent").GetComponent<PopulateFamilyPanels>().PopulateFamilies();

        //Update Family Background
        sugarDex.GetComponent<SugarDisk>().UpdateSugarDex(dbList, sugarDex.GetComponent<SugarDisk>().allCollectedSugars);
       

        GameObject.Find("SugarDisk").GetComponent<SugarDisk>().allCollectedSugars = sugarDex.GetComponent<SugarDisk>().allCollectedSugars.Distinct().ToList();
        foundCount.GetComponent<Text>().text = "Found: " + sugarDex.GetComponent<SugarDisk>().allCollectedSugars.Count;
        totalCount.GetComponent<Text>().text = "Total: " + repository.Count;
        sugarDex.GetComponent<SugarDisk>().CloseSugarDisk();
        
    }


    // Update is called once per frame
    void Update() {
#if UNITY_ANDROID
        if (Input.GetKey(KeyCode.Escape) && sugarDex.GetComponent<SugarDisk>().sugarDexOpen == true)
        {
            sugarDex.GetComponent<SugarDisk>().CloseSugarDisk();
        }
#endif
    }

    public void ToggleOption() 
    {
        switch (toggleOption)
        {
            case "Sound":
                SetVibrate();
                break;
            case "Vibrate":
                SetMute();
                break;
            case "Mute":
                SetSound();
                break;
        }
    }

    public void SetSound()
    {
        toggleOption = "Sound";
        PlayerPrefs.SetString("ToggleOption", toggleOption);
        sound = true;
        vibrate = true;
        if (soundInitialized)
        {
            onSound.Play();
        }
        toggleButton.GetComponent<Image>().sprite = Sound;
    }

    public void SetVibrate()
    {
        toggleOption = "Vibrate";
        PlayerPrefs.SetString("ToggleOption", toggleOption);
        sound = false;
        vibrate = true;
#if UNITY_ANDROID || UNITY_IOS
        if (soundInitialized)
        {
            Handheld.Vibrate();
        }
#endif
        toggleButton.GetComponent<Image>().sprite = Vibrate;
    }

    public void SetMute()
    {
        toggleOption = "Mute";
        PlayerPrefs.SetString("ToggleOption", toggleOption);
        sound = false;
        vibrate = false;
        toggleButton.GetComponent<Image>().sprite = Mute;
    }

   
    public void AllTypeOfSugars(string ingredientFromDB, string bcv)
    {
        sugarDex.GetComponent<Button>().enabled = false;
        greenCartBtn.GetComponent<Button>().enabled = false;
        mainCam.GetComponent<SimpleDemo>().enabled = false;
        if(bcv == superCode)
        {
            //Add super code exception
            ingredientFromDB = superCode + ", super code";
        }

        //Barcode not in database
        if (ingredientFromDB == "Not Found")
        {
            scanFrame.SetActive(false);
            CreateSugarMonster("Not Found");
        }
        else
        {
            currentNumMonster = 0;
            scannedAddedSugars.Clear();
            
            ingredientFromDB = ingredientFromDB.Replace('.', ',').Replace(';', ',');
            List<string> dbIngredientList = ingredientFromDB.Split(',').ToList();
            dbIngredientList = dbIngredientList.ConvertAll(item => item.Trim());
            dbIngredientList.RemoveAt(0); // remove the upc/bcv number as we already have it
            string name = dbIngredientList[0]; // get the name of the product
            name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name); // make the first letter of every word uppcase
            dbIngredientList.RemoveAt(0); // remove the name from the sugar list
            if (bcv == superCode)
            {
                if (allScanned.Count == repository.Count)
                {
                    GameObject.Find("SugarDisk").GetComponent<SugarDisk>().ResetData();
                }
                else
                {
                    dbIngredientList = repository;
                }       
            }
            
            foreach (string r in repository)
            {
                if (dbIngredientList.Contains(r.ToLower()))
                {
                    dbIngredientList.IndexOf(r.ToLower());
                    scannedAddedSugars.Add(char.ToUpper(r[0]) + r.Substring(1));
                    if (!allScanned.Contains(r.ToLower()))
                    {
                        allScanned.Add(r.ToLower());


                        //This is the newly add indicator showing func.
                        foreach (List<string> sl in dbList)
                        {
                            if (sl[nameIndex].ToLower() == r.ToLower())
                            {
                                Info info = new Info(sl[familyIndex]);
                                um.IndicateController(info,"Notification", dropdownMenu.GetComponent<TMP_Dropdown>().options);
                            }
                        }

                        numCount++;
                        //playerprefAs.set array
                        PlayerPrefs.SetString("num_" + numCount, r.ToLower());
                        //playerprefs.set array.length()
                        PlayerPrefs.SetInt("count", numCount);
                    }
                }
            }

            if (scannedAddedSugars.Count == 0)
            {
                //add green cart code here
                //GreenCartController.Instance.PCAdd(bcv);
                //GreenCartController.Instance.PC.PCSave();
                um.simpleDemo.RequestAsync(bcv, name, String.Join(", ", scannedAddedSugars.ToArray()));
                scannedAddedSugars.Add("No Added Sugar");
                CreateSugarMonster(scannedAddedSugars[currentNumMonster]);
                scanFrame.SetActive(false);              
            }
            //Include added sugar
            else
            {
                string sugars = String.Join(", ", scannedAddedSugars.ToArray());
                sugars = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(sugars); // make the first letter of every word uppcase
                um.simpleDemo.RequestAsync(bcv, name, sugars);
                scanFrame.SetActive(false);
                CreateSugarMonster(scannedAddedSugars[currentNumMonster]);
            }
            
        }
    }
    
    /// <summary>
    /// Animation of moving sugar card to sugar dex
    /// </summary>
    /// <param name="s">the string of the kind of card</param>
    private IEnumerator AnimatorSugarCardToDex(string s)
    {
        //Animation - Card To Dex
        var anim = Instantiate(GameObject.Find(scannedAddedSugars[currentNumMonster]), GameObject.Find("Canvas").transform) as GameObject;
        anim.name = "Animation";
        anim.GetComponent<Image>().sprite = monster.GetComponent<Image>().sprite;
        anim.AddComponent<Animator>();
        anim.GetComponent<Animator>().runtimeAnimatorController = animController;

        //Found or new card
        if (s == "Sugar")
        {
            canvas.transform.Find("Animation/Sugar Name").GetComponent<Text>().text = scannedAddedSugars[currentNumMonster];

            

            float animWaitCounter = 0f;
            while (animWaitCounter < 2f && wasSkipped == false) // wait for 2 seconds
            {
                if (sound && firstBadSound)
                {
                    badSound.Play();
                    firstBadSound = false;
                }
                if (Input.GetButtonDown("Fire1") || Input.touchCount > 0) break; // if mouse pressed or screen tapped, end timer
                yield return null;
                animWaitCounter += Time.deltaTime;
            }
            if (animWaitCounter < 2f) // if the previous animation was skipped
            {
                if (sound)
                {
                    sweepSound.Play();
                }
                wasSkipped = true;
                yield return new WaitForSeconds(.2f); // delay between rapid SugarCardToDex animations
            }
            firstBadSound = true;
            if (currentNumMonster + 1 == scannedAddedSugars.Count) // if last card
            {
                Destroy(GameObject.Find(scannedAddedSugars[currentNumMonster])); // destroy stationary card
                greenCartBtn.GetComponent<Button>().enabled = true; // active FoodDex button
                wasSkipped = false; // reset wasSkipped for the next scan
            }
            else // if there are more cards
            {
                GameObject.Find(scannedAddedSugars[currentNumMonster]).GetComponentInChildren<Text>().text = scannedAddedSugars[currentNumMonster + 1]; // set atationary card to the next card
            }

            ChangeNextCardText();
            anim.GetComponent<Animator>().Play("SugarCardToDex");
            yield return new WaitForSeconds(1f);   //wait a second for displaying animation
            Destroy(anim);
        }
        //Green no added sugar card
        else if (s == "NoAddedSugar")
        {
            if (sound)
            {
                goodSound.Play();
            }
            yield return new WaitForSeconds(2f);
            if (currentNumMonster + 1 == scannedAddedSugars.Count)
            {
                Destroy(GameObject.Find(scannedAddedSugars[currentNumMonster]));
            }
            anim.GetComponent<Animator>().Play("GreenCard");
            yield return new WaitForSeconds(1f);
            Destroy(anim);
            greenCartBtn.GetComponent<Button>().enabled = true; //Active FoodDex button
            ChangeNextCardText();
        }
        //orange not found card
        else if (s == "NotFound")
        {
            if (sound)
            {
                unknownSound.Play();
            }
            yield return new WaitForSeconds(2f);
            if (currentNumMonster + 1 == scannedAddedSugars.Count)
            {
                Destroy(GameObject.Find(scannedAddedSugars[currentNumMonster]));
            }
            anim.GetComponent<Animator>().Play("NotFoundCard");
            yield return new WaitForSeconds(1f);
            scanFrame.SetActive(true);
            Destroy(anim);
            greenCartBtn.GetComponent<Button>().enabled = true; //Active FoodDex button
            ChangeNextCardText();
        }
        
    }

    /// <summary>
    /// Change the information displaying on the sugar card
    /// </summary>
    private void ChangeNextCardText()
    {
        currentNumMonster++;
        if (currentNumMonster == scannedAddedSugars.Count)
        {
            if (redDot.gameObject.activeSelf)
            {
                //Third stage of tutorial
                if (ts == 2 && !scannedAddedSugars.Contains("No Added Sugar"))
                {
                    GameObject.Find("TutorialButton").GetComponent<TutorialDisplay>().DisplayTutorial(ts, null);
                }
            }
            GameObject.Destroy(GameObject.Find(scannedAddedSugars[currentNumMonster - 1]));
            scanFrame.SetActive(true);
            
            if (GameObject.Find("Magic Tree") == null && !greenCartGo.activeSelf)
            {
                sugarDex.GetComponent<Button>().enabled = true;
                mainCam.GetComponent<SimpleDemo>().enabled = true;
                mainCam.GetComponent<SimpleDemo>().Invoke("ClickStart", 3f); //wait for 3 seconds for next scan
            }
        }
        else
        {

            if (!sugarInWall.Contains(scannedAddedSugars[currentNumMonster].ToLower()))
            {

#if UNITY_ANDROID || UNITY_IOS
                if (vibrate) {
                    Handheld.Vibrate();
                }
#endif
                monster.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/NewAddedSugar");
                redDot.gameObject.SetActive(true);
            }
            else monster.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/CollectedAddedSugar");

            if (!sugarInWall.Contains(scannedAddedSugars[currentNumMonster].ToLower())) sugarInWall.Add(scannedAddedSugars[currentNumMonster].ToLower());



            monster.name = scannedAddedSugars[currentNumMonster];
            canvas.transform.Find(scannedAddedSugars[currentNumMonster] + "/Sugar Name").GetComponent<Text>().text = scannedAddedSugars[currentNumMonster];
            monster.transform.Find("SugarDesign").GetComponent<Image>().sprite = GetMonsterDesign(monster.name.ToLower());

            //Audio.Play();
            DisplayMonsters();

        }
    }

    /// <summary>
    /// Play animation
    /// </summary>
    public void DisplayMonsters()
    {
        ts = mainCam.GetComponent<SimpleDemo>().tutorialStage;
        if (scannedAddedSugars.Contains("Not Found"))
        {

            StartCoroutine("AnimatorSugarCardToDex", "NotFound");
            GameObject.Destroy(GameObject.Find("Not Found"));
            mainCam.GetComponent<SimpleDemo>().Invoke("ClickStart", 3f); //Scan after 3 seconds

            sugarDex.GetComponent<Button>().enabled = true;
        }
        else
        {
            if (scannedAddedSugars[currentNumMonster] != "No Added Sugar")
            {
                StartCoroutine("AnimatorSugarCardToDex", "Sugar");
            }
            else StartCoroutine("AnimatorSugarCardToDex", "NoAddedSugar");
        }
    }


    /// <summary>
    /// Instantiate monster
    /// </summary>
    /// <param name="sugarName">the name of the type of sugar</param>
    public void CreateSugarMonster(string sugarName)
    {
        
        ts = mainCam.GetComponent<SimpleDemo>().tutorialStage;
        
        monster = Instantiate(Resources.Load("Prefabs/Monster"), GameObject.Find("Canvas").transform) as GameObject;

        monster.name = sugarName;

        if (sugarName == "No Added Sugar")
        {
            monster.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/NoAddedSugar");
            monster.transform.Find("SugarDesign").gameObject.SetActive(false);
            
        }
        else if (sugarName == "Not Found")
        {
            monster.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Not Found Card");
            monster.transform.Find("SugarDesign").gameObject.SetActive(false);
            scannedAddedSugars.Add("Not Found");
        }
        else
        {
            //Find new added sugar
            if (!sugarInWall.Contains(sugarName.ToLower()))
            {
#if UNITY_ANDROID || UNITY_IOS
                if (vibrate) {
                    Handheld.Vibrate();
                }
#endif
                monster.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/NewAddedSugar");
                redDot.gameObject.SetActive(true);
            }
            else monster.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/CollectedAddedSugar");
            GameObject.Find("Canvas").transform.Find(sugarName + "/Sugar Name").GetComponent<Text>().text = sugarName;

            monster.transform.Find("SugarDesign").GetComponent<Image>().sprite = GetMonsterDesign(sugarName);
            
            if (!sugarInWall.Contains(sugarName.ToLower())) sugarInWall.Add(sugarName.ToLower());

        }


        if (ts == 1)
        {
            GameObject.Find("TutorialButton").GetComponent<TutorialDisplay>().DisplayTutorial(ts, sugarName);
        }
        else
        {
            DisplayMonsters();
        }
    }
    /// <summary>
    /// Checks if a monster has been found
    /// </summary>
    /// <param name="sugarName">Name of the monster to look for</param>
    /// <returns>true if the monster has been found before</returns>
    public bool MonsterFound(string sugarName) {
        return sugarInWall.Contains(sugarName.ToLower());
    }
    /// <summary>
    /// Returns the Sprite of a monster given the name (not including black shading as it's just the sprite not the image)
    /// </summary>
    /// <param name="sugarName">Name of the monster to find</param>
    /// <returns>Sprite of the correct monster</returns>
    public Sprite GetMonsterDesign(string sugarName)
    {
        return Resources.Load<Sprite>("Images/Monsters/" + sugarName);
    }
}
