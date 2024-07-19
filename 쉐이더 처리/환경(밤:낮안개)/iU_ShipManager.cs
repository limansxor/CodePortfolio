using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using UnityEngine;
using UnityEngine.SceneManagement;

//// <summary>
///  2018년 '디지털 선박 목업 시뮬레이션' 
///  전체기능 제어 코드 
/// </summary>
using System.Runtime.InteropServices;

public class iU_ShipManager : MonoBehaviour
{

    private static iU_ShipManager instance = null;

    public static iU_ShipManager Instance  // public static 으로 선언 ( 중요 ).
    {
        get  // set 이 아닌 get 이다 !
        {
            if (instance == null)
            {
                instance = new iU_ShipManager();
            }

            return instance;
        }
    }
    void Awake()
    {
        instance = this;

        PreStart();
    }

    #region 클래스 및 변수 정의
    public class BtnImgInfo
    {
        public GameObject obj;
        public string NormalName;
        public string hoverName;
        public string PressedName;
        public bool Pressed;

    }

    //enum
    public enum UI_MODE { SHIP_UI, ROOM_UI };
    public enum _OBSERVES { ORBIT, TILT, FLY, HUMAN, HUMAN_FLY, OBSERVES_MAX };
    public enum _OPTION_WEATHER_BTN { NONE, SUN, RAIN, SNOW, FOG, SHIPUP };
    public enum _OPTION_CLASSIFY { NONE, NATURE, SYS, HELP, QUALITY }
    #endregion

    #region Inspector
    public UI_MODE UI_Mode; // SHIP_UI, ROOM_UI 인지

    public GameObject OBJCam = null; // 씬전환에 필요한 그리고중요한 카메라 스크립트 정도  UI_Mode 가 Room 일경우 OBJ 카메라가 null 일 수 있다 .
    public GameObject HumanCam = null;

    public GameObject EnvironmentObj;

    public GameObject snowParticle = null;
    public GameObject rainParticle = null;

    public NewMoveObject MoveObject = null;

    public GameObject PlayerCamPointObj;

    public GameObject HideUI;
    public GameObject btn_option;
    public GameObject QuitManuUI;
    public GameObject Indoors;

    public VolumeFogManager fogManager = null;



    public bool OnPhysicsEffect;
    public bool ShipUP;

    public string ScreenShotPath = "";
    public bool VRCaptureEnable = false;
    public string[] ReservationSceneNames; // 임시 씬 이름 저장소

    public GameObject Btn_ShipUP;
    public GameObject Btn_PhysicsEffect;
    public GameObject Btn_NightVision;

    public DeferredNightVisionEffect[] Effects;

    public GameObject Btn_Screenshot;
    public UILabel Label_ScreenPath;
    #endregion

    #region 버튼 컨트롤 UI 변수 

    [HideInInspector]
    public bool OptionMenuToogle;
    [HideInInspector]
    public List<BtnImgInfo> ToggleInfo; // 토글 버튼을 위한 정보 등록 
    private List<bool> PreToggleState; // 내부 변수 이지만 ...


    [HideInInspector]
    public bool UIView = false;
    #endregion

    #region 네비게이션 변수
    [HideInInspector]
    public _OBSERVES CurrentSel = _OBSERVES.ORBIT;
    [HideInInspector]
    public int IndoorNum = 0; // 0 이먼 인도어 모드가 아닌 것으로 
    #endregion

    #region 자연 환경 변수 물리, 배 업 
    [HideInInspector]
    public NeoSky neoSky = null;
    GameObject NeoOceanObj = null;
    [HideInInspector]
    public _OPTION_WEATHER_BTN OptionNaturDown;

    public _OPTION_CLASSIFY Option_classify =_OPTION_CLASSIFY.NONE;

    private Rigidbody rigidBody = null;
    private NeoBuoyancy neoBuoyancy = null;

   public bool OneFogRun = false;



    public AlphaSortedGlobalFog SkyFog = null;
    public AlphaSortedGlobalFog SkyHumanFog = null;

    
    [HideInInspector]
    public bool SliSnowChange = false;
    [HideInInspector]
    public bool SliRainChange = false;

    Material CloudMaterial;
    Material OceanMaterial;

    float FogOpacity = 0.0f;
    int RainStep = 0;
    int SnowStep = 0;

    NeoProjectedGrid WaveGrid = null;

    #endregion

    #region 내부 변수
    float keyDelay = 0.0f;
    private int ScreenShotCount = 0;

    bool enableAddValue = false;

    [HideInInspector]
    public GameObject[] PlayerCamPoint;


    private Vector3 PreCamObjPosition;
    private Quaternion PreCamObjRotation;

    private bool UpdateOne = false;

   

    #endregion

    #region 잠수를 위한 변수
    private bool PreDive = false;
    #endregion
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    Vector3 EquatorColor;
    Vector3 GroundColor;

    void PreStart()
    {
        ToggleInfo = new List<BtnImgInfo>();

        PreToggleState = new List<bool>();

        if (UI_Mode == UI_MODE.SHIP_UI)
        {
            #region SHIP_UI
          //  OBJCam = iUnity_Manager.Instance.MainCam;
            // 인도어 모드 Transform 설정
            if (PlayerCamPointObj != null)
            {
                PlayerCamPoint = new GameObject[PlayerCamPointObj.transform.childCount];

                for (int i = 0; i < PlayerCamPointObj.transform.childCount; i++)
                {
                    PlayerCamPoint[i] = PlayerCamPointObj.transform.GetChild(i).gameObject;
                }
            }

            if (EnvironmentObj != null)
            {
                // 환경 변수 초기화
                NeoOceanObj = EnvironmentObj.transform.Find("NeoOcean").gameObject;
                neoSky = EnvironmentObj.GetComponentInChildren<NeoSky>();
                neoSky.Cycle.TimeOfDay = 15;
                WaveGrid = NeoOceanObj.transform.Find("NeoOceanGrid").gameObject.GetComponent<NeoProjectedGrid>();
                CloudMaterial = neoSky.CloudInstance.GetComponent<MeshRenderer>().material;
                OceanMaterial = NeoOceanObj.transform.Find("NeoOceanGrid").GetComponent<NeoProjectedGrid>().high;



               // SkyFog = OBJCam.transform.Find("SkyCamera").GetComponent<AlphaSortedGlobalFog>();
               // SkyHumanFog = HumanCam.transform.Find("SkyCameraHuman").GetComponent<AlphaSortedGlobalFog>();


                Texture CloudTexture = (Texture)Resources.Load("SkyTextures/SkyCloud01", typeof(Texture));

                CloudMaterial.SetTexture("_NoiseTexture", CloudTexture);

                Color OceanColor = new Color(1.0f / 255.0f, 20.0f / 255.0f, 39.0f / 255.0f);
                OceanMaterial.SetColor("_BaseColor", OceanColor);

                rigidBody = MoveObject.gameObject.GetComponent<Rigidbody>();
                neoBuoyancy = MoveObject.GetComponent<NeoBuoyancy>();

                OnPhysicsEffect = false;
                ShipUP = false;
                fnUseRigidbody(false);

                SliSnowChange = false;
                SliRainChange = false;

                OptionNaturDown = _OPTION_WEATHER_BTN.SUN;

                CurrentSel = _OBSERVES.ORBIT;

                setWaveControl(0);

                ToggleStateSet(Btn_ShipUP);
                ToggleStateSet(Btn_PhysicsEffect);
                ToggleStateSet(Btn_NightVision);

                Btn_ShipUP.GetComponent<UIButton>().onClick.Add(new EventDelegate(this, "ShipUPClick"));
                Btn_PhysicsEffect.GetComponent<UIButton>().onClick.Add(new EventDelegate(this, "PhysicsEffectClick"));
                Btn_NightVision.GetComponent<UIButton>().onClick.Add(new EventDelegate(this, "NightVisionEffectClick"));
               
            }
            #endregion
        }
        else if (UI_Mode == UI_MODE.ROOM_UI)
        {
            CurrentSel = _OBSERVES.HUMAN;
        }

        // 공통 

        ScreenShotPath = UnityEngine.Application.dataPath;

        // VRCapture.VRCapture.Instance.RegisterCompleteDelegate(HandleCaptureFinish);
        if (GameObject.Find("OnPlay"))
        {
            GameObject.Find("OnPlay").GetComponent<OneBtnAniPlay>().VRCaptureEnable = VRCaptureEnable;
        }
        else
        {
            VRCaptureEnable = false;
        }

        OptionMenuToogle = false;

        enableGameObject(HideUI, false); //HideUI 초기화

        Btn_Screenshot.GetComponent<UIButton>().onClick.Add(new EventDelegate(this, "ScreenShotClick"));

        ToggleStateSet(QuitManuUI);

    }
    // Use this for initialization
    void Start()
    {
       
        EquatorColor = new Vector3(RenderSettings.ambientEquatorColor.r, RenderSettings.ambientEquatorColor.g, RenderSettings.ambientEquatorColor.b);
        GroundColor = new Vector3(RenderSettings.ambientGroundColor.r, RenderSettings.ambientGroundColor.g, RenderSettings.ambientGroundColor.b);

    }

    public void fnUseRigidbody(bool enable)
    {
        rigidBody.useGravity = enable;
        neoBuoyancy.enabled = enable;

        if (!enable)
        {
            Vector3 PrePos = rigidBody.transform.localPosition; ////물리 안정화를 위해서 11 수치 주다
            PrePos.y = -11.0f;
            rigidBody.transform.localPosition = PrePos;
        }
        else
        {

            MoveObject.ResetEngineForce();
        }


    }

    public void IndoorDecrease()
    {
        if (IndoorNum > 1) IndoorNum--;
        else { IndoorNum = PlayerCamPoint.Length - 1; }
        setCameraOption((int)_OBSERVES.HUMAN);
    }
    public void IndoorIncrease()
    {
        if (IndoorNum < PlayerCamPoint.Length - 1) IndoorNum++;
        else { IndoorNum = 1; }
        setCameraOption((int)_OBSERVES.HUMAN);
    }

    // Update is called once per frame
    void Update()
    {
        if (keyDelay > 0)
        {
            keyDelay -= Time.deltaTime;
        }
        if (UI_Mode == UI_MODE.SHIP_UI)
        {
            if (Input.GetKeyDown(KeyCode.F9) && keyDelay <= 0)
            {
                SceneLoad(ReservationSceneNames[0]);
                keyDelay = 1.0f;
            }

            if (Input.GetKeyDown(KeyCode.F10) && keyDelay <= 0)
            {
                SceneLoad(ReservationSceneNames[1]);
                keyDelay = 1.0f;
            }

            if (Input.GetKeyDown(KeyCode.F11) && keyDelay <= 0)
            {
                //if (CurrentSel < _OBSERVES.HUMAN_FLY) CurrentSel++;
                //else { CurrentSel = _OBSERVES.ORBIT; }
                setCameraOption(0);
                keyDelay = 0.2f;
            }

            if (Input.GetKeyDown(KeyCode.F12) && keyDelay <= 0)
            {
                Debug.Log("모드변환");
                IndoorIncrease();
                //if (CamModeSp >= Limit) CamModeSp = 0;
                //setCameraOption(CamModeSp);
                keyDelay = 0.2f;
            }
        }


        if (Input.GetKeyDown(KeyCode.Escape) && keyDelay <= 0)
        {
            //  Application.Quit();
            //   System.Diagnostics.Process.GetCurrentProcess().Kill();
            QuitManuUI.GetComponent<OuitMenuView>().OnClick();
            keyDelay = 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.Return) && keyDelay <= 0)
        {
        
            UIOnOff(!UIView);
            keyDelay = 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.P) && keyDelay <= 0)
        {
            ScreenShot();
            keyDelay = 0.1f;
        }

      
        for (int i = 0; i < ToggleInfo.Count; i++)
        {
            if (PreToggleState[i] != ToggleInfo[i].Pressed)
            {
                if (ToggleInfo[i].Pressed)
                {
                    ToggleInfo[i].obj.GetComponent<UIButton>().normalSprite = ToggleInfo[i].PressedName;
                    ToggleInfo[i].obj.GetComponent<UIButton>().hoverSprite = ToggleInfo[i].PressedName;
                    ToggleInfo[i].obj.GetComponent<UIButton>().disabledSprite = ToggleInfo[i].PressedName;
                    ToggleInfo[i].obj.GetComponent<UIButton>().state = UIButtonColor.State.Pressed;
                    //    ToggleInfo[i].obj.GetComponent<UIButton>().isEnabled = false;

                }
                else
                {
                 
              //      ToggleInfo[i].obj.GetComponent<UIButton>().isEnabled = true;
                    ToggleInfo[i].obj.GetComponent<UIButton>().normalSprite = ToggleInfo[i].NormalName;
                    ToggleInfo[i].obj.GetComponent<UIButton>().hoverSprite = ToggleInfo[i].hoverName;

                }

                PreToggleState[i] = ToggleInfo[i].Pressed;
            }
        }

       




        if (CurrentSel == _OBSERVES.FLY && Input.GetMouseButton(1))
        {
            if (!Input.GetAxis("Mouse X").Equals(0) || !Input.GetAxis("Mouse Y").Equals(0))
            {
                //   MainControlCamera.transform.localPosition += new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));
                OBJCam.transform.Translate(new Vector3(Input.GetAxis("Mouse X") * 0.5f, Input.GetAxis("Mouse Y") * 0.5f, 0));
                OBJCam.transform.localRotation = Quaternion.Euler(80, PreCamObjRotation.eulerAngles.y, 0);
            }
        }
        if (UI_Mode == UI_MODE.SHIP_UI)
        {
            if (OBJCam != null &&PreDive != OBJCam.GetComponent<NeoUnderWater>().enabled)
            {
                if(OBJCam.GetComponent<NeoUnderWater>().enabled) // 잠수 했을 경우
                {
                    OBJCam.transform.Find("SkyCamera").GetComponent<Camera>().enabled = false;
                }
                else // 수면 위 
                {
                    OBJCam.transform.Find("SkyCamera").GetComponent<Camera>().enabled = true;
                }
                PreDive = OBJCam.GetComponent<NeoUnderWater>().enabled;
            }
        }

    }

    #region 기능 함수들 
    void UIOnOff(bool view)
    {
        UIView = view;
        


        enableGameObject(HideUI, view);

        if (view)
        {
            enableGameObject(QuitManuUI, true);
            enableGameObject(QuitManuUI.transform.GetChild(0).gameObject, false);
            QuitManuUI.GetComponent<OuitMenuView>().View = false;

            if (Option_classify == _OPTION_CLASSIFY.NONE)
            {
                for (int i = 0; i < btn_option.transform.childCount; i++)
                {
                    enableGameObject(btn_option.transform.GetChild(i).gameObject, false);
                }
            }
            else
            {
                for (int i = 0; i < btn_option.transform.childCount; i++)
                {
                    enableGameObject(btn_option.transform.GetChild(i).gameObject, true);
                    enableGameObject(btn_option.transform.GetChild(i).transform.GetChild(0).gameObject, false);
                }

                enableGameObject(btn_option.transform.GetChild((int)Option_classify - 1).gameObject, true);
            }
        }

        if (UI_Mode == UI_MODE.SHIP_UI)
        {
            if (Indoors != null)
                enableGameObject(Indoors, false);// 함선 근접 모드 선택 창 
        }
        else if (UI_Mode == UI_MODE.ROOM_UI)
        {

        }
    }
    void ScreenShotClick()
    {
        string ScreenShotPath = openDialog();

        Label_ScreenPath.GetComponent<UILabel>().text = ScreenShotPath;
    }
    void ScreenShot()
    {
        ScreenShotCount++;
        string sDirPath = ScreenShotPath + "\\Screenshot" + ScreenShotCount.ToString() + ".png";
        Debug.Log("스크린샷 경로:" + sDirPath);
        ScreenCapture.CaptureScreenshot(sDirPath);
    }
    public void enableGameObject(GameObject Obj, bool enable)
    {
        if (Obj != null)
        {
            Transform[] ts = Obj.transform.GetComponentsInChildren<Transform>(); // 자식 요소 까지 모두 찾아 낸다~
            foreach (Transform t in ts)
            {
                UISprite Sprite = t.gameObject.GetComponent<UISprite>();
                Collider coll = t.gameObject.GetComponent<Collider>();
                UILabel Label = t.gameObject.GetComponent<UILabel>();

                if (Sprite != null)
                {
                    Sprite.enabled = enable;
                }
                if (coll != null)
                {
                    coll.enabled = enable;
                }
                if (Label != null)
                {
                    Label.enabled = enable;
                }
            }
        }

    }

    public void ToggleStateSet(GameObject obj)
    {
        BtnImgInfo btnImgInfo = new BtnImgInfo();
        btnImgInfo.NormalName = obj.GetComponent<UIButton>().normalSprite;
        btnImgInfo.hoverName = obj.GetComponent<UIButton>().hoverSprite;
        btnImgInfo.PressedName = obj.GetComponent<UIButton>().pressedSprite;
        btnImgInfo.Pressed = false;
        btnImgInfo.obj = obj;

        ToggleInfo.Add(btnImgInfo);

        PreToggleState.Add(false);
    }

    public int getObjNum(GameObject obj)
    {
        for (int j = 0; j < ToggleInfo.Count; j++)
        {
            if (obj == ToggleInfo[j].obj)
            {
                return j;
            }
        }

        return -1;
    }

    public void SceneLoad(string sceneName)
    {
        NeoOceanObj.SetActive(false);
        if (UI_Mode == UI_MODE.SHIP_UI)
        {
            Destroy(OBJCam);
        }
        Destroy(HumanCam);

        //PauseMenu pauseMenu = (PauseMenu)FindObjectOfType(typeof(PauseMenu));

        SceneManager.LoadScene(sceneName);
    }

    public void LoadURL(string url)
    {
        UnityEngine.Application.OpenURL(url);
    }

    public BtnImgInfo ToggleBtn(GameObject obj)
    {
        int p = getObjNum(obj);
        return ToggleInfo[p];
    }
    #endregion

    #region 네비게이션 컨트롤
    public void setCameraOption(int sel) // 변경시에만 들어 오도록
    {
        #region 룸 모드에서는 사용 하지 않는다.   
        #endregion

        // 카메라 뷰잉 방식 ( 스크립트 초기화 )
        
        // 카메라 찾기 
        Camera MainCam = GameObject.Find("OBJCamera").GetComponent<Camera>(); // OBJCam.GetComponent<Camera>();//.GetComponentInChildren<Camera>();//GameObject.Find (OBJCamera).GetComponent<Camera> ();
        Camera PlayerCam = GameObject.Find("HumanEyeCam").GetComponent<Camera>(); ///.transform.GetComponentInChildren<Camera>();

        MainCam.enabled = true; // 메인 카메라만 작동 
        PlayerCam.enabled = false;

        HumanCam.GetComponent<Player>().enabled = false;
        HumanCam.GetComponent<PlayerJoyStick>().enabled = false;

        CurrentSel = (_OBSERVES)sel;
        if (sel == 0)
        {

            OBJCam.GetComponent<OrbitCamera>().enabled = true;
            OBJCam.GetComponent<OrbitCamera>().InitOrbit();
            OBJCam.GetComponent<TiltCamera>().enabled = false;
            OBJCam.GetComponentInChildren<ZoomCamera>().enabled = true;
            OBJCam.GetComponentInChildren<PanCamera>().enabled = true;
            SkyFogControl(false);

            //	targetObj.transform.position = InitTargetPos; // 이동 되었던 타겟 오브 젝트를 제자리로 
            for (int i = 0; i < OBJCam.transform.childCount; i++)
            {
                if (OBJCam.transform.name.IndexOf("SkyCamera") != -1)
                {
                    neoSky.targetcamera = OBJCam.transform.GetChild(i).GetComponent<Camera>();
                    break;
                }

            }

            IndoorNum = 0;


        }
        else if (sel == 1)
        {
            OBJCam.GetComponent<OrbitCamera>().enabled = false;
            OBJCam.GetComponent<TiltCamera>().enabled = true;
            OBJCam.GetComponent<TiltCamera>().InitTilt();
            OBJCam.GetComponentInChildren<ZoomCamera>().enabled = true;
            OBJCam.GetComponentInChildren<PanCamera>().enabled = true;
            SkyFogControl(false);
            for (int i = 0; i < OBJCam.transform.childCount; i++)
            {
                if (OBJCam.transform.name.IndexOf("SkyCamera") != -1)
                {
                    neoSky.targetcamera = OBJCam.transform.GetChild(i).GetComponent<Camera>();
                    break;
                }

            }
            IndoorNum = 0;
        }
        else if (sel == 2)
        {
            OBJCam.GetComponent<OrbitCamera>().enabled = false;
            OBJCam.GetComponent<TiltCamera>().enabled = false;
            OBJCam.GetComponentInChildren<ZoomCamera>().enabled = false;
            OBJCam.GetComponentInChildren<PanCamera>().enabled = false;
            SkyFogControl(false);

            PreCamObjRotation = OBJCam.transform.localRotation;
            OBJCam.transform.localRotation = Quaternion.Euler(80, PreCamObjRotation.eulerAngles.y, 0);

            IndoorNum = 0;
        }
        else
        {
            //if(IndoorNum ==0)
            //{
            //    IndoorNum = 1;
            //} 
        //    UIOnOff(false);
            SkyFogControl(true);
            MainCam.enabled = false;
            OBJCam = GameObject.Find("OBJCamera");
            OBJCam.GetComponent<OrbitCamera>().enabled = false;
            OBJCam.GetComponent<TiltCamera>().enabled = false;

            OBJCam.GetComponentInChildren<ZoomCamera>().enabled = false;
            OBJCam.GetComponentInChildren<PanCamera>().enabled = false;

            HumanCam.GetComponent<Player>().enabled = true;
            HumanCam.GetComponent<PlayerJoyStick>().enabled = true;
            HumanCam.GetComponent<CharacterController>().enabled = false;


            for (int i = 0; i < OBJCam.transform.childCount; i++)
            {
                if (HumanCam.transform.name.IndexOf("SkyCamera") != -1)
                {
                    neoSky.targetcamera = OBJCam.transform.GetChild(i).GetComponent<Camera>();
                    break;
                }

            }

            PlayerCam.enabled = true; // 배근접 걷는 모드 

            GameObject initPos; 
            if (IndoorNum == 0)
            {
                 initPos = PlayerCamPoint[0];
            }
            else
            {
                 initPos = PlayerCamPoint[IndoorNum - 1]; //GameObject.Find (FindObj);
            }

            UnityEngine.Debug.Log(initPos.name);

      
            // Vive 제어 

            if (GameObject.Find("[CameraRig]"))
            {
                if (HumanCam.transform.GetChild(0).GetChild(0).transform)
                {
                    HumanCam.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).localPosition = new Vector3(0, 1, 0);
                }
               
            }
            else
            {
                HumanCam.transform.GetChild(0).localPosition = new Vector3(0, 1, 0);
            }
            if (IndoorNum == 0)
            {
                Vector3 Temp = initPos.transform.position;
                Temp.y += 11;
                HumanCam.transform.position = Temp;
            }
            else
            {
                HumanCam.transform.position = initPos.transform.position;
            }
            HumanCam.transform.rotation = initPos.transform.rotation;



        }



    }

    #endregion

    #region 날씨 컨트롤
    public void setWeatherControl(_OPTION_WEATHER_BTN Weather)
    {

        switch (Weather)
        {
            case _OPTION_WEATHER_BTN.SUN:

                neoSky.Clouds.Sharpness = 0.6f;
                snowParticle.gameObject.SetActive(false);
                rainParticle.gameObject.SetActive(false);
                SkyFog.enabled = false;
                SkyHumanFog.enabled = false;
                neoSky.Clouds.Tone = 1.5f;
                OceanMaterial.SetColor("_BaseColor", new Color(1.0f / 255.0f, 20.0f / 255.0f, 39.0f / 255.0f));
                neoSky.SunInstance.SetActive(true);
                if (OneFogRun)
                {
                    fogManager.SetEnableFog(false);
                    fogManager.SetFogValue(0.0F);
                }
                break;
            case _OPTION_WEATHER_BTN.RAIN:
                SkyFog.enabled = true;
                SkyHumanFog.enabled = true;
                neoSky.Clouds.Sharpness = 1.0f;
                neoSky.Clouds.Tone = 0.1f;

                snowParticle.gameObject.SetActive(false);

                rainParticle.gameObject.SetActive(true);

                neoSky.SunInstance.SetActive(false);

                OceanMaterial.SetColor("_BaseColor", new Color(22.0f / 255.0f, 38.0f / 255.0f, 42.0f / 255.0f));

                break;
            case _OPTION_WEATHER_BTN.SNOW:
                SkyFog.enabled = true;
                SkyHumanFog.enabled = true;
                neoSky.Clouds.Sharpness = 1.0f;
                neoSky.Clouds.Tone = 0.1f;

                rainParticle.gameObject.SetActive(false);
                snowParticle.gameObject.SetActive(true);

                neoSky.SunInstance.SetActive(false);

                OceanMaterial.SetColor("_BaseColor", new Color(22.0f / 255.0f, 38.0f / 255.0f, 42.0f / 255.0f));

                break;
            case _OPTION_WEATHER_BTN.FOG:
                OneFogRun = true;
            //    SkyFog.enabled = true;
             //   SkyHumanFog.enabled = true;
                neoSky.Clouds.Sharpness = 1.0f;
                neoSky.Clouds.Tone = 0.1f;
                fogManager.SetEnableFog(true);
                fogManager.SetFogValue(FogOpacity);

                neoSky.SunInstance.SetActive(false);

                break;
            case _OPTION_WEATHER_BTN.SHIPUP:
                break;
        }
    }

    public void setFogValueControl(float Opacity)
    {
        FogOpacity = Opacity;


        if (FogOpacity == 0 && RainStep == 0 && SnowStep == 0)
        {
            setWeatherControl(_OPTION_WEATHER_BTN.SUN);
        }
        else
        {
            setWeatherControl(_OPTION_WEATHER_BTN.FOG);
            fogManager.SetFogValue(Opacity);


        }


    }

    public void setRainValueControl(int Step)
    {
        RainStep = Step;
        SnowStep = 0;
        if (FogOpacity == 0 && RainStep == 0 && SnowStep == 0)
        {
            setWeatherControl(_OPTION_WEATHER_BTN.SUN);
        }
        else if (RainStep == 0)
        {

            rainParticle.gameObject.SetActive(false);
        }
        else
        {
            setWeatherControl(_OPTION_WEATHER_BTN.RAIN);
            switch (Step)
            {
                case 1:
                    rainParticle.GetComponent<RainParticle>().SetRainState(RAINSTATE.RAIN_1, 0);
                    break;
                case 2:
                    rainParticle.GetComponent<RainParticle>().SetRainState(RAINSTATE.RAIN_2, 0);
                    break;
                case 3:
                    rainParticle.GetComponent<RainParticle>().SetRainState(RAINSTATE.RAIN_3, 0);
                    break;
            }

        }
    }

    public void setSnowValueControl(int Step)
    {
        SnowStep = Step;
        RainStep = 0;
        if (FogOpacity == 0 && RainStep == 0 && SnowStep == 0)
        {
            setWeatherControl(_OPTION_WEATHER_BTN.SUN);
        }
        else if (SnowStep == 0)
        {
            snowParticle.gameObject.SetActive(false);
        }
        else
        {
            setWeatherControl(_OPTION_WEATHER_BTN.SNOW);

            switch (Step)
            {
                case 1:
                    snowParticle.GetComponent<SnowParticle>().SetSnowState(SNOWSTATE.SNOW_1, 0);
                    break;
                case 2:
                    snowParticle.GetComponent<SnowParticle>().SetSnowState(SNOWSTATE.SNOW_2, 0);
                    break;
                case 3:
                    snowParticle.GetComponent<SnowParticle>().SetSnowState(SNOWSTATE.SNOW_3, 0);
                    break;
            }


        }
    }

    public void SkyFogControl(bool Human)
    {
        if (Human)
        {
            if(SkyFog)
            SkyFog.transform.GetComponent<Camera>().enabled = false;
            if(SkyHumanFog) 
            SkyHumanFog.transform.GetComponent<Camera>().enabled = true;
        }
        else
        {
            if (SkyFog)
                SkyFog.transform.GetComponent<Camera>().enabled = true;
            if (SkyHumanFog)
                SkyHumanFog.transform.GetComponent<Camera>().enabled = false;
        }
    }

    public void ShipUPClick()
    {
        bool PhysicsCheck = OnPhysicsEffect;

        ShipUP = !ShipUP;
        int Lp;

        if (ShipUP)
        {
            fnUseRigidbody(false); // 물리 끄고

            Lp = getObjNum(Btn_PhysicsEffect);
            ToggleInfo[Lp].Pressed = false;

            fnShipUp(ShipUP); // 올리고

            Lp = getObjNum(Btn_ShipUP);
            ToggleInfo[Lp].Pressed = true;
        }
        else if (ShipUP == false && OnPhysicsEffect == true)
        {

            fnUseRigidbody(true);

            Lp = getObjNum(Btn_PhysicsEffect);
            ToggleInfo[Lp].Pressed = true;



            fnShipUp(false);

            Lp = getObjNum(Btn_ShipUP);
            ToggleInfo[Lp].Pressed = false;
        }
        else if(ShipUP == false && OnPhysicsEffect == false) 
        {
            Lp = getObjNum(Btn_PhysicsEffect);
            ToggleInfo[Lp].Pressed = false;
            fnShipUp(ShipUP);
        }
    }

    public void PhysicsEffectClick()
    {
        int Lp;

        if (!ShipUP)
        {
            OnPhysicsEffect = !OnPhysicsEffect;

            fnUseRigidbody(Instance.OnPhysicsEffect);

            if (OnPhysicsEffect)
            {
                Lp = getObjNum(Btn_PhysicsEffect);
                ToggleInfo[Lp].Pressed = true;


            }
            else
            {
                Lp = getObjNum(Btn_PhysicsEffect);
                ToggleInfo[Lp].Pressed = false;

            }
        }
        else
        {
            Lp = getObjNum(Btn_PhysicsEffect);
            ToggleInfo[Lp].Pressed = false;
        }
    }

    public void NightVisionEffectClick()
    {
        int lp;
        lp = getObjNum(Btn_NightVision);
        ToggleInfo[lp].Pressed = !ToggleInfo[lp].Pressed;

        for (int i = 0; i < Effects.Length; i++)
        {
            Effects[i].enabled = ToggleInfo[lp].Pressed;
        }


    }
    #endregion

    #region 스크린샷

    public string openDialog()
    {
        UnityEngine.Debug.Log("파일 다이얼로그 부르다 ");

        FolderBrowserDialog myDialogue = new FolderBrowserDialog();

   
        //OpenFileDialog myDialogue = new OpenFileDialog();
        //    myDialogue.Filter = "All Files (*.*)|*.*";
        //   myDialogue.FilterIndex = 1;

        //   myDialogue.Multiselect = true;
        if (myDialogue.ShowDialog() == DialogResult.OK)
        {

            //  ScreenShotPath = myDialogue.FileName;
            ScreenShotPath = myDialogue.SelectedPath;

            //   IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, "NVHSim");
            //    ShowWindow(hwnd, 3);

        }

        IntPtr hwnd = FindWindowByCaption(IntPtr.Zero, GetProjectName());
        ShowWindow(hwnd, 3);
        myDialogue.Reset();

        return ScreenShotPath;
    }

    public string GetProjectName()
    {
        string[] s = UnityEngine.Application.dataPath.Split('/');
        string projectName = s[s.Length - 2];
        Debug.Log("project = " + projectName);
        return projectName;
    }

    public void fnShipUp(bool enable)
    {
        Transform strShip = GameObject.Find("STRShip").transform;
        if (enable)
        {

            strShip.position =
                new Vector3(strShip.position.x, strShip.position.y + 100, strShip.position.z);
            enableAddValue = true;
        }
        else
        {
            if (enableAddValue)
            {
                strShip.position =
            new Vector3(strShip.position.x, strShip.position.y - 100, strShip.position.z);

                enableAddValue = false;
            }
        }
    }



    #endregion

    #region 파도 조절


    public void setWaveControl(int SelectWave)
    {
        switch (SelectWave)
        {
            case 0:
                {
                    WaveGrid.fftParam.windSpeed = 3.0f;
                    WaveGrid.bkParam.breakHeight = 0.05f;
                }
                break;

            case 1:
                {
                    WaveGrid.fftParam.windSpeed = 8.5f;
                    WaveGrid.bkParam.breakHeight = 0.3f;
                }
                break;

            case 2:
                {
                    WaveGrid.fftParam.windSpeed = 13.5f;
                    WaveGrid.bkParam.breakHeight = 0.88f;
                }
                break;

            case 3:
                {
                    WaveGrid.fftParam.windSpeed = 19.0f;
                    WaveGrid.bkParam.breakHeight = 1.88f;
                }
                break;

            case 4:
                {
                    WaveGrid.fftParam.windSpeed = 24.5f;
                    WaveGrid.bkParam.breakHeight = 3.25f;
                }
                break;

            default:
                break;
        }
    }
    #endregion

    #region 시간 조절

    public void setDayAndNight(float danTime)
    {
        neoSky.Cycle.TimeOfDay = 12 * danTime + 12;

    Vector3 ec =  Vector3.Lerp(EquatorColor, new Vector3(0, 0, 0), danTime);
    Vector3 gc =  Vector3.Lerp(GroundColor , new Vector3(0, 0, 0), danTime);
    Vector3 dl = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(0, 0, 0), danTime);
        RenderSettings.ambientEquatorColor = new Color(ec.x, ec.y, ec.z);
        RenderSettings.ambientGroundColor = new Color(gc.x, gc.y, gc.z);

       // Directional_Light = GameObject.Find("Directional Light").GetComponent<Light>();
        //if (Directional_Light) 
        //Directional_Light.color = new Color(dl.x, dl.y, dl.z);
        /*
        if (Time >= 24)
            neoSky.Cycle.TimeOfDay = Time - 24;
        else
            neoSky.Cycle.TimeOfDay = Time;
        */
    }
    #endregion
}
