﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
/// <summary>
/// * This Class Controls overall behavior of GreenDex
/// * This is a singleton
/// * Attached to GreenCartBack
/// </summary>
public class GreenCartController : MonoBehaviour
{
    public bool rollable { get; set; }
    private static GreenCartController instance;
    public static GreenCartController Instance { get { return instance; } }
    public GameObject DetailPage;
    public GameObject NetIndicator;
    [SerializeField]
    ProductCollection pc = new ProductCollection();
    public ProductCollection PC { get { return pc; } }
    [SerializeField]
    GameObject dashHolder;
    [SerializeField]
    List<GameObject> Containers;
    public List<GameObject> CONTAINERS { get { return Containers; } }
    [SerializeField]
    List<Sprite> cateImg;//0:food,1:drink,2:snack,3:uncate,4:sauce,5:not cate but a check mark
    public List<Sprite> CateImg { get { return cateImg; } }
    [SerializeField]
    float containerHeight;
    int position;
    int incre;

    [SerializeField]
    string key;
    [SerializeField]
    string gkey;
    //where the request file/properity is here
    public IRequester requester;
    public IRequester grequester;
    [SerializeField]
    TextAsset text;
    [SerializeField]
    char delimiter;
    bool down;
    //variable be used to fast scrolling. function not implemented yet, so variable not in use
    //float downTimer;
    //Vector3 downPos = new Vector3();
    //Vector3 upPos = new Vector3();
    //bool fastRool;
    Vector3 lastPos;

    Vector3 lastTouchPos;

    public GameObject testtextbox;
#if UNITY_EDITOR
    int[] ints = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
#endif
    float totalDisRollingDis;


    private List<Category> currentCates = new List<Category>();
    public List<Category> CurrentCates { get { return currentCates; } set { currentCates = value; } }


    private List<ProductInfo> curSelectedPI = new List<ProductInfo>();
    public List<ProductInfo> CurSelectedPI { get { return curSelectedPI; } }

    //this val is used to adjust the rooling
    //use this val to avoid rooling overflow
    float microAdjustVal;
    private void Awake()
    {

        if (instance != null)
        {
            Destroy(this);
        }
        else instance = this;
        position = 0;
        down = false;
        totalDisRollingDis = 0;
        rollable = true;
        //there is an chance incre value and containerHeight is alway the same
        //so there should be only one value.
        incre = 150;
        containerHeight = 150f;
        try
        {
            pc.products = pc.Load();
            ///*pc.products = */pc.BinaryLoader();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            Debug.Log(ex.StackTrace);
        }
        NetIndicator.SetActive(false);
        //creat the requester. no need for await, use this for the build. it is faster 
        //when in editor use this. but use Async method in build will be faster
        //!not support Async load TextAsset in Editor
#if UNITY_EDITOR
        StartCoroutine("InitRequester");
#else
        StartAsync(); 
#endif
        microAdjustVal = 0.5f;
    }
    /// <summary>
    /// *Load UPC&NDB Lookup table into memory and sign to USDARequester
    /// *requester is the used to send request to usda
    /// *grequester is use to send request to Google using google map api.
    /// *!Warnning: Seems like Unity do not support TextAsset streaming using Task. Do not use this in unity editor
    /// </summary>
    /// <returns></returns>
    private async Task StartAsync()
    {
        await Task.Run(() =>
        {
            List<string[]> strList = new List<string[]>();
            var textAssetArr = text.text.Split('\n');
            foreach (var line in textAssetArr)
            {
                var contentArr = line.Split(delimiter);
                strList.Add(contentArr);
            }
            requester = new USDARequester(strList, 1,key);
            grequester = new GoogleRequester(gkey);
        });
        //await SendRequest("123");
    }
    /// <summary>
    /// * This function is same as StartAsync()
    /// * But used only in Editor
    /// </summary>
    System.Collections.IEnumerator InitRequester()
    {

        List<string[]> strList = new List<string[]>();
        var textAssetArr = text.text.Split('\n');
        foreach (var line in textAssetArr)
        {
            var contentArr = line.Split(delimiter);
            strList.Add(contentArr);
        }
        requester = new USDARequester(strList, 1,key);
        grequester = new GoogleRequester(gkey);
        yield return null;
    }


    public void Update()
    {
        //when roolable, rolling
        //disable ro
        if (rollable)
        {
            RollingAction();
        }

    }
    /// <summary>
    /// *Simulate the Scrolling in Mobile
    /// *currentTouch record users Touch position at current frame
    /// *lastTouchPos is the recorded last frame Touch postition
    /// *! Unity Editor do not support Touch, use mouse input in editor
    /// </summary>
    private void RollingAction()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            down = true;
        }
        if (Input.GetButtonUp("Fire1"))
        {
            down = false;
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPos = touch.position;
            }
            else
            {
                var currentTouch = touch.position;
                NewRolling(currentTouch, lastTouchPos);
                lastTouchPos = touch.position;
            }
        }

#if UNITY_EDITOR
        #region mouseaction
        //action to drag test
        var currentPos = Input.mousePosition;
        //if (!fastRool)
        //{
        if (down)
        {
            NewRolling(currentPos, lastPos);
        }
        //}
        lastPos = Input.mousePosition;

        #endregion
#endif
    }

    /// <summary>
    /// * Check offSet of last/current Touch/mouse click position
    /// * Adjust the GreenDex Container's position accordingly
    /// </summary>
    /// <param name="currentPos">current user's Touch/mouse position</param>
    /// <param name="lastPos">Touch/mouse position of last frame</param>
    private void NewRolling(Vector3 currentPos, Vector3 lastPos)
    {
        var offSet = lastPos.y - currentPos.y;
        try
        {
            //when the rolling distance is more than the totaly data user have
            //then set offSet value to 0 to prevent furthe rolling
            //pc.GetCount(currentCates) is the total number of products in current selected category
            //Containers.Count is the container number in editor(number is 10 when writing this) 
            //containerHeight is height of each container
            if ((pc.GetCount(currentCates) - Containers.Count-microAdjustVal) * containerHeight < -totalDisRollingDis && offSet < 0)
            {
#if UNITY_EDITOR
                Debug.Log("there is no more data");
#endif
                offSet = 0;
            }
            else if (totalDisRollingDis > /*containerHeight*/0f && offSet > 0)
            {
#if UNITY_EDITOR
                Debug.Log("this is the top of data");
#endif
                offSet = 0;
            }
        }
        catch { }
        var info = new NotifyInfo();
        info.Offset = offSet;
        info.RollingDis = totalDisRollingDis;
        #region will repalce this in container update
        //rolling is the new method which have refatored
        foreach (GameObject go in Containers)
        {
            var rectTrans = go.GetComponent<RectTransform>();
            //var offSet = lastPos.y - currentPos.y;
            var curPos = new Vector3(rectTrans.localPosition.x, rectTrans.localPosition.y, rectTrans.localPosition.z);
            curPos.y -= offSet;
            if (curPos.y > containerHeight)
            {
                curPos.y -= containerHeight * Containers.Count;
                //Debug.Log("Move to bottom");
                //var text = go.transform.Find("ProductName").GetComponent<Text>();
                try
                {
                    int i = pc.GetCount(currentCates) + (int)(info.RollingDis / containerHeight) - Containers.Count - 1;

                    //go.GetComponent<GreenDexContainer>().PIUpdate(pc.GetProduct(i));
                    go.GetComponent<GreenDexContainer>().PIUpdate(pc.GetProduct(i, currentCates));
                }
                catch (System.Exception ex)
                {
                    Debug.Log(totalDisRollingDis);
                    Debug.Log(ex.StackTrace);
                }
            }
            if (curPos.y < (-Containers.Count+microAdjustVal) * containerHeight)
            {
                curPos.y += Containers.Count * containerHeight;
                try
                {
                    int i = pc.GetCount(currentCates) - (int)(-info.RollingDis / containerHeight) /*- Containers.Count*/ - 1;
                    //go.GetComponent<GreenDexContainer>().PIUpdate(pc.GetProduct(i);
                    go.GetComponent<GreenDexContainer>().PIUpdate(pc.GetProduct(i, currentCates));
                }
                catch (System.Exception ex)
                {
                    Debug.Log(totalDisRollingDis);
                    Debug.Log(ex.StackTrace);
                }
            }
            rectTrans.localPosition = curPos;
        }
        //Rolling(offSet, info);
        #endregion
        totalDisRollingDis += offSet;
    }

    //private void Rolling(float offSet, NotifyInfo info)
    //{
    //    foreach (GameObject go in Containers)
    //    {
    //        var rectTrans = go.GetComponent<RectTransform>();
    //        //var offSet = lastPos.y - currentPos.y;
    //        var curPos = new Vector3(rectTrans.localPosition.x, rectTrans.localPosition.y, rectTrans.localPosition.z);
    //        curPos.y -= offSet;
    //        if (curPos.y > 0)
    //        {
    //            curPos.y -= containerHeight * Containers.Count;
    //            //Debug.Log("Move to bottom");
    //            //var text = go.transform.Find("ProductName").GetComponent<Text>();
    //            try
    //            {
    //                int i = pc.GetCount(currentCate) + (int)(info.RollingDis / containerHeight) - Containers.Count - 1;

    //                //go.GetComponent<GreenDexContainer>().PIUpdate(pc.GetProduct(i));
    //                go.GetComponent<GreenDexContainer>().PIUpdate(pc.GetProduct(i, currentCate));
    //            }
    //            catch (System.Exception ex)
    //            {
    //                Debug.Log(totalDisRollingDis);
    //                Debug.Log(ex.StackTrace);
    //            }
    //        }
    //        if (curPos.y < -Containers.Count * containerHeight)
    //        {
    //            curPos.y += Containers.Count * containerHeight;
    //            try
    //            {
    //                int i = pc.GetCount(currentCate) - (int)(-info.RollingDis / containerHeight) /*- Containers.Count*/ - 1;
    //                //go.GetComponent<GreenDexContainer>().PIUpdate(pc.GetProduct(i);
    //                go.GetComponent<GreenDexContainer>().PIUpdate(pc.GetProduct(i, currentCate));
    //            }
    //            catch (System.Exception ex)
    //            {
    //                Debug.Log(totalDisRollingDis);
    //                Debug.Log(ex.StackTrace);
    //            }
    //        }
    //        rectTrans.localPosition = curPos;
    //    }
    //}

    /// <summary>
    /// * Add Scanned product to PC(Product Collection)
    /// </summary>
    /// <param name="name">prodcut name</param>
    /// <param name="pos">location where the product is scanned</param>
    public void PCAdd(string name,string pos)
    {
        pc.AddProduct(name,pos);
    }
    /// <summary>
    /// * Reset the Container's Position every time user Open GreenDex
    /// * Make it easier to control the container's UI position
    /// </summary>
    private void OnEnable()
    {
        ResetContainer(currentCates);
    }
    /// <summary>
    /// update container content
    /// * reset container pos
    /// * update content
    /// * When products in cates are less than container than disable the extra container
    /// </summary>
    /// <param name="cates">current user selected Categorys</param>
    public void ResetContainer(List<Category> cates)
    {
        totalDisRollingDis = 0;
        //var pos = -200;
        var pos = 0;
        for (int i = 0; i < Containers.Count; i++)
        {
            //as some container may have be disabled when there is not enough product in category
            //so enable all container first.
            if (!Containers[i].activeSelf)
            {
                Containers[i].SetActive(!Containers[i].activeSelf);
            }
            var offset = dashHolder.GetComponent<RectTransform>().rect.width / 2;
            Containers[i].GetComponent<RectTransform>().localPosition = new Vector3(offset, pos, 0);
            pos -= incre;
            if (cates.Count != 0)
            {
                //dupe
                foreach (Category cate in cates)
                {
                    if (i > pc.CurDic.Count - 1)
                    {
                        Containers[i].SetActive(false);
                    }
                    else Containers[i].GetComponent<GreenDexContainer>().PIUpdate(pc.CurDic[pc.CurDic.Count - i - 1]);
                }
            }
            else
            {
                if (i > pc.products.Count - 1)
                {

                    Containers[i].SetActive(false);
                }
                else Containers[i].GetComponent<GreenDexContainer>().PIUpdate(pc.products[pc.products.Count - 1 - i]);
            }

        }
    }
    /// <summary>
    /// * IsSelected is recorded in pi(Prodcut Infomation)
    /// * Set the value to false make sure it can be selected in later action
    /// * Reset curSelelctedPI to en empty List.
    /// </summary>
    public void ClearCurSelectedPI()
    {
        foreach (ProductInfo pi in curSelectedPI)
        {
            pi.IsSelected = false;
        };
        curSelectedPI = new List<ProductInfo>();
    }

    /// <summary>
    /// * Send Request To USDA and Google to get Information
    /// * Using normal method will cause halt
    /// * Set the NetIndicator to be Active to show the request is activeing, and set it to false when it ends
    /// * The NetIndicator will cause bug when the first request is not end but another request is send. Need further test
    /// * Call the PCAdd(string,string) to Add product to collection
    /// * Call PC.PCSave() to save the products to user's locat file
    /// </summary>
    /// <param name="bcv"></param>
    /// <returns></returns>
    public async Task RequesetAsync(string bcv)
    {
        NetIndicator.SetActive(true);
        //start the locationservice here and give it some time to get the latitude and longitude info
        Input.location.Start();
        string name = await requester.SendRequest(bcv);
        if (name == bcv)
        {
            await Task.Run(() => {
                float i = 0;
                while (i < 1)
                {
                    i += Time.deltaTime;
                }
            });
            name += $"UPC: {bcv}";
        }
        //wait 1 second to give the location service more time to get latlng info
#if !UNITY_EDITOR
        await Task.Run(() => {
            float i = 0;
            while (i < 2)
            {
                i += Time.deltaTime;
            }
        });
#endif
        //stop the locationservice to save battery life. 
        //hopefully, the time to get internet request will give the device enought to get the location info
        Input.location.Stop();

        var pos = Input.location.lastData;
        //change the info to an format google api support
        var info = $@"latlng={pos.latitude.ToString()},{pos.longitude.ToString()}";
        var realpos = await grequester.SendRequest(info);
        PCAdd(name, realpos);
        PC.PCSave();
        NetIndicator.SetActive(false);
    }
}
