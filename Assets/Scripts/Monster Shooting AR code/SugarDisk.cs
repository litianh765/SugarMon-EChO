﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class SugarDisk : MonoBehaviour {

    //singleton ref
    UIManager um;

    [HideInInspector]
    public int foundMonsterNumber;

    public NumbersOfEachSugar sugarCardData;
    public GameObject summonSystem;
    public GameObject sugarDiskImage;
    public GameObject mainCam;
    private Vector3 diskPosition;
    public GameObject canvas;
    private GameObject[] allTypesOfSugars;
    public GameObject addButtonOnSugarCard;


    private List<string> sugarFromMain, newSugars;
    public List<string> allCollectedSugars;
    private int numCount;
    public List<string> scannedAddedSugars;

    //change to local
    //private Transform sci;
    // Use this for initialization

    void Start () {

        Transform sugarDiskImage = GameObject.Find("Canvas").transform.Find("FamilyBackground");
        diskPosition = sugarDiskImage.transform.localPosition;
        foundMonsterNumber = 0;
        newSugars = new List<string>();

        um = UIManager.Instance;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OpenSugarDisk()
    {
        UpdateDexData();
        //if Open from Summon System
        if (summonSystem.activeInHierarchy)
        {
            addButtonOnSugarCard.gameObject.SetActive(true);
        }
        else
        {
            //UpdateCounterOfEachSugar();
        }
        
    }
    public void CloseSugarDisk()
    {
        mainCam.GetComponent<SimpleDemo>().enabled = true;
        if (mainCam.GetComponent<SimpleDemo>().tutorialStage != 0) mainCam.GetComponent<SimpleDemo>().StartScan();
        GameObject.Find("SugarDisk").transform.Find("RedDot").gameObject.SetActive(false);
        sugarDiskImage.gameObject.SetActive(false);
        addButtonOnSugarCard.gameObject.SetActive(false);

        um.DisAllUp("Notification");
    }
    public void UpdateDexData()
    {
        mainCam.GetComponent<SimpleDemo>().enabled = false;

        newSugars.Clear();
        sugarDiskImage.transform.localPosition = diskPosition;
        canvas.transform.Find("FamilyBackground").gameObject.SetActive(true);
        GameObject.Find("FamilyBackground").transform.Find("TopBar/Found Count").GetComponent<Text>().text = "Found: " + canvas.GetComponent<FindAddedSugar>().allScanned.Count;

        scannedAddedSugars = canvas.GetComponent<FindAddedSugar>().scannedAddedSugars;
        sugarFromMain = canvas.GetComponent<FindAddedSugar>().allScanned;
        foreach (string ni in sugarFromMain)
        {
            if (!allCollectedSugars.Contains(ni) && ni.ToLower() != "no added sugar")
            {
                newSugars.Add(ni);
                allCollectedSugars.Add(ni.ToLower());

            }
        }
        UpdateSugarDex(canvas.GetComponent<FindAddedSugar>().dbList, newSugars);

        #region Old Update Family Backgound Func
        //foreach (List<string> s in canvas.GetComponent<FindAddedSugar>().dbList)
        //{
        //    foreach (string ss in newSugars)
        //    {
        //        if (s[canvas.GetComponent<FindAddedSugar>().nameIndex].ToLower() == ss.ToLower())
        //        {
        //            var sc = GameObject.Find(s[canvas.GetComponent<FindAddedSugar>().deckNumIndex]);
        //            var sci = sc.transform.Find("Image");

        //            if (sc != null)
        //            {
        //                sc.name = ss;
        //                List<string> sugarWords = ss.Split(' ').ToList();
        //                if (sugarWords[0].ToCharArray().Count() > 12)
        //                {
        //                    string text = char.ToUpper(ss[0]) + ss.Substring(1);
        //                    text = text.Insert(12, "- ");
        //                    sc.transform.Find("Name").GetComponent<Text>().text = text;

        //                }
        //                else sc.transform.Find("Name").GetComponent<Text>().text = char.ToUpper(ss[0]) + ss.Substring(1);



        //                sci.GetComponentInChildren<Text>().text = "";

        //                //placing and resizing the monster image in sugardex

        //                sci.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        //                sci.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        //                sci.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        //                sci.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);
        //                sci.GetComponent<RectTransform>().sizeDelta = new Vector2(122, 150);
        //                sci.GetComponent<RectTransform>().localScale = new Vector2(1.5f, 1.5f);

        //                sci.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Monsters/" + s[canvas.GetComponent<FindAddedSugar>().familyIndex]);
        //                sci.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Monsters/" + sc.name);
        //                //sci.GetComponent<Image>().sprite = Resources.Load<Sprite>(monsterImagePath);
        //                //sci.gameObject.AddComponent<Button>().onClick.AddListener(() => summonSystem.GetComponent<SummonSystem>().PopupSugarInfoCardInSugarDex(sc.name, s[canvas.GetComponent<FindAddedSugar>().familyIndex]));
        //                //sc.transform.Find("Image").GetComponent<Button>().enabled = true;
        //            }
        //        }
        //    }

        //}
        #endregion

    }
    public void UpdateCounterOfEachSugar()
    {
        foreach (string sugar in scannedAddedSugars.ConvertAll(item => item.ToLower()))
        {
            GameObject.Find(sugar).transform.Find("Counter").GetComponent<Text>().text = "X" + sugarCardData.GetNumberOfSugar(sugar).ToString();
        }
    }
    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        sugarCardData.sugars.Clear();
    }

    public void UpdateSugarDex(List<List<string>> dbList, List<string> newSugars)
    {
        foreach (List<string> s in dbList)
        {
            foreach (string ss in newSugars)
            {
                if (s[canvas.GetComponent<FindAddedSugar>().nameIndex].ToLower() == ss.ToLower())
                {
                    var sc = GameObject.Find(s[canvas.GetComponent<FindAddedSugar>().deckNumIndex]);
                    var sci = sc.transform.Find("Image");

                    if (sc != null)
                    {
                        sc.name = ss;
                        List<string> sugarWords = ss.Split(' ').ToList();
                        if (sugarWords[0].ToCharArray().Count() > 12)
                        {
                            string text = char.ToUpper(ss[0]) + ss.Substring(1);
                            text = text.Insert(12, "- ");
                            sc.transform.Find("Name").GetComponent<Text>().text = text;

                        }
                        else sc.transform.Find("Name").GetComponent<Text>().text = char.ToUpper(ss[0]) + ss.Substring(1);



                        sci.GetComponentInChildren<Text>().text = "";

                        //placing and resizing the monster image in sugardex

                        sci.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                        sci.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                        sci.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                        sci.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);
                        sci.GetComponent<RectTransform>().sizeDelta = new Vector2(122, 150);
                        sci.GetComponent<RectTransform>().localScale = new Vector2(1.5f, 1.5f);

                        sci.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Monsters/" + s[canvas.GetComponent<FindAddedSugar>().familyIndex]);
                        sci.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Monsters/" + sc.name);
                    }
                }
            }

        }
    }
}
