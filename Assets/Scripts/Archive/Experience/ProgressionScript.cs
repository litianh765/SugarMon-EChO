﻿/* 
 * This file was created by Mark Botaish on May 21st, 2019 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionScript : MonoBehaviour {

    [Tooltip("The multiplier to multiply the next xp by after leveling up")]
    public float _xpMulitplier = 1.6f;

    //Slider info
    private Text _levelNumber;
    private Slider _fillImage;

    //Backend xp info
    private float _currentXP;
    private int _xpToNextLevel;
    private int _xpPreviousLevel;
    private float _xpFillTime = 2.0f;
    private int _currentLevel;

    // Use this for initialization
    void Start () {

        //TODO: Get the current xp (from saved file?)
        _currentXP = 0;
        _currentLevel = 0;
        _xpToNextLevel = 100;
        _xpPreviousLevel = 0;

        _levelNumber = this.gameObject.transform.GetChild(1).GetComponent<Text>();
        _fillImage = this.gameObject.transform.GetChild(0).GetComponent<Slider>();

        _levelNumber.text = "Level " + _currentLevel + " ( " + (_currentXP - _xpPreviousLevel) + " / " + (_xpToNextLevel - _xpPreviousLevel) + " )";
        _fillImage.value = _currentXP / _xpToNextLevel;

    }
	/*
     * This fucntion is to test the xp bar from the inspector
    */
    [ContextMenu("ADD EXP")]
    public void TestXP()
    {
        AddXP(12345);
    }
    
    /*
     * This function is the public function that can be called anywhere to add a certain ammount of xp to the bar
     */
    public void AddXP(int xp)
    {
        StartCoroutine(AddXPAnimation(xp));
    }

    /*
     * This function does the fill animation for the xp bar
     */
    IEnumerator AddXPAnimation(int xp)
    {
        float nextXp = _currentXP + xp;
        float timer = 0;
        float startingXP = _currentXP;
        while (_currentXP < nextXp)
        {
            _currentXP = Mathf.Lerp(startingXP, nextXp, timer);
            timer += Time.deltaTime / 2.0f;
            //print(timer);   
            UpdateFillBar();

            if (_currentXP >= _xpToNextLevel)
            {
                _currentLevel++;
                _xpPreviousLevel = _xpToNextLevel;
                _xpToNextLevel = (int)(_xpToNextLevel * _xpMulitplier);
                UpdateFillBar();
            }
            yield return null;
        }
        if (nextXp < _currentXP)
        {
            print("over " + _currentXP);
            //_currentXP = nextXp;
            //UpdateFillBar();            
        }
    }

    /*
     * This funciton is used to update the fill amount of the progress bar and the text that goes over it 
     * <Gets called in the AddXPAnimation functions>
     */
    void UpdateFillBar()
    {
        _fillImage.value = (_currentXP - _xpPreviousLevel) / (_xpToNextLevel - _xpPreviousLevel);
        _levelNumber.text = "Level " + _currentLevel + "( " + (_currentXP - _xpPreviousLevel) + " / " + (_xpToNextLevel - _xpPreviousLevel) + " )";
    }


	// Update is called once per frame
	void Update () {
	}
}