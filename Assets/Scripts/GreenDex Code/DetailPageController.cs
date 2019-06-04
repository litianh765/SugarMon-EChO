﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
/// <summary>
/// * This Class Controls overall behavior of CartDetailCanvas
/// * This is a singleton
/// * Attached to GreenCartBack
/// </summary>
public class DetailPageController : MonoBehaviour {
    private static DetailPageController instance;
    public static DetailPageController Instance { get { return instance; } }
    ProductInfo pi;
    public ProductInfo PI { get { return pi; } set { pi = value; } }

    [SerializeField]
    public GameObject CategoryLabel;
    [SerializeField]
    public GameObject ProductIcon;
    [SerializeField]
    public GameObject LocationLabel;
    [SerializeField]
    public GameObject SugarsLabel;
    [SerializeField]
    public GameObject ProductSugars;
    [SerializeField]
    public GameObject ProductName;
    [SerializeField]
    public GameObject ProductLocation;
    [SerializeField]
    public GameObject ProductDate;

    public Color32 HeaderColor;
    public Color32 BodyColor = Color.white;

    private Color32 GreenHeader = new Color32(68,111,76,255);
    private Color32 RedHeader = new Color32(111, 38, 46, 255);

    public void Awake() {
        if (instance != null) {
            Destroy(this);
        }
        else instance = this;
    }

    public void UpdateDisplay() {
        if (pi != null) {
            InitText();
            InitColorsAndImages();
        }
        else {
            Debug.Log("No Product Given");
        }
    }
    private void InitText() {
        if (pi.Type == Category.containsaddedsugar) {
            SugarsLabel.GetComponent<TextMeshProUGUI>().text = "Added Sugars:";
            ProductSugars.GetComponent<TextMeshProUGUI>().text = FormattedSugars();
        }
        else {
            ProductSugars.GetComponent<TextMeshProUGUI>().text = "";
            SugarsLabel.GetComponent<TextMeshProUGUI>().text = "";
        }
        ProductName.GetComponent<TextMeshProUGUI>().text = $"{pi.GetDetailPageName()}";
        ProductLocation.GetComponent<TextMeshProUGUI>().text = $"{pi.GetDetailPageLocation()}";
        ProductDate.GetComponent<TextMeshProUGUI>().text = $"{pi.displayFullDateTime()}";
    }
    private void InitColorsAndImages() {
        ProductIcon.GetComponent<Image>().sprite = pi.GetSprite();
        if (pi.Type == Category.containsaddedsugar) {
            UIManager.Instance.background.GetComponentInChildren<Image>().sprite = UIManager.Instance.Backgrounds[2];
            CategoryLabel.GetComponent<Image>().sprite = UIManager.Instance.Buttons[5];
            HeaderColor = RedHeader;
        }
        else {
            UIManager.Instance.background.GetComponentInChildren<Image>().sprite = UIManager.Instance.Backgrounds[1];
            CategoryLabel.GetComponent<Image>().sprite = UIManager.Instance.Buttons[4];
            HeaderColor = GreenHeader;
        }
        LocationLabel.GetComponent<TextMeshProUGUI>().color = HeaderColor;
        SugarsLabel.GetComponent<TextMeshProUGUI>().color = HeaderColor;
        ProductName.GetComponent<TextMeshProUGUI>().color = HeaderColor;
        ProductSugars.GetComponent<TextMeshProUGUI>().color = BodyColor;

        ProductLocation.GetComponent<TextMeshProUGUI>().color = BodyColor;
        ProductDate.GetComponent<TextMeshProUGUI>().color = BodyColor;
    }
    public void PIUpdate(ProductInfo pi) {
        this.pi = pi;
        UpdateDisplay();
    }
    private string FormattedSugars() {
        string formatted = "";
        List<string> sugars = SimpleDemo.GetSugarsFromBCV(pi.GetUPC());
        if(sugars.Count <= 10)
            return String.Join("\n", sugars.ToArray());
        else
            return String.Join(", ", sugars.ToArray());
    }
}
