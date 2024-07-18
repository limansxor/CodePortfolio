using UnityEngine;
using System.Collections;
using VolumetricFogAndMist;


//// <summary>
///  2018년 '디지털 선박 목업 시뮬레이션' 
///  안개 제어 코드 
/// </summary>
public class VolumeFogManager : MonoBehaviour
{
    private VolumetricFog OceanFog = null;

    public VolumetricFog BaseOceanFog = null;
    public VolumetricFog WalkOceanFog = null;

    public Color DayFogColor;
    public Color NightFogColor;

    public NeoSky neoSky = null;

    public GameObject FogPlane = null;

    private WeatherControlManager WeatherManager = null;

    private Color ORG_AmbientEquatorColor;
    private Color ORG_ambientGroundColor;
    private bool preDay = false;
    void Awake()
    {
        OceanFog = BaseOceanFog;
    }

    // Use this for initialization
    void Start()
    {
        WeatherManager = gameObject.GetComponent<WeatherControlManager>();
        ORG_AmbientEquatorColor = RenderSettings.ambientEquatorColor;
        ORG_ambientGroundColor = RenderSettings.ambientGroundColor; 

    }

    // Update is called once per frame
    void Update()
    {
        bool Day = IsDay();
    
        if (BaseOceanFog.enabled)// (WeatherManager.bFog)
        {
            if (Day)
            {
                OceanFog.color = DayFogColor;
            }
            else
            {
                OceanFog.color = NightFogColor;
            }
        }
    }

    public void SetFogTarget(bool Walk)
    {
        if (Walk)
            OceanFog = WalkOceanFog;
        else
            OceanFog = BaseOceanFog;
    }

    public void SetEnableFog(bool Enable)
    {
        // OceanFog.enabled = Enable;
        // SkyFog.enabled = enabled;

        BaseOceanFog.enabled = Enable;
        WalkOceanFog.enabled = Enable;

        if (Enable)
            ResetFogValue();

        FogPlane.SetActive(Enable);
    }

    public void ResetFogValue()
    {
        OceanFog.noiseStrength = 0.0f;
        OceanFog.noiseScale = 0.0f;
        OceanFog.distance = 0.0f;
        OceanFog.distanceFallOff = 0.0f;
        OceanFog.stepping = 1.0f;
        OceanFog.steppingNear = 0.0f;

        OceanFog.specularThreshold = 0.0f;
        OceanFog.specularIntensity = 0.0f;
        OceanFog.lightIntensity = 0.0f;

        OceanFog.skyHaze = 0.0f;
        OceanFog.skySpeed = 0.0f;
        OceanFog.skyNoiseStrength = 0.0f;
        OceanFog.skyAlpha = 0.0f;
    }

    public void SetFogValue(float percent)
    {
        //Debug.Log("aasdsagdsaga  " + percent.ToString());

        SetOceanFogValue(percent);
    }


    private float StartDensity = 0.01f;
    private float EndDensity = 0.3f;

    private float StartHeight = 40.0f;
    private float EndHeight = 60.0f;

    public void SetOceanFogValue(float percent)
    {
        float minusDensity = EndDensity - StartDensity;
        float RateDensity = minusDensity * percent;

        // OceanFog.density = StartDensity + RateDensity;
        BaseOceanFog.density = StartDensity + RateDensity;
        WalkOceanFog.density = StartDensity + RateDensity;

        float minusHeight = EndHeight - StartHeight;
        float RateHeight = minusHeight * percent;

        //  OceanFog.height = StartHeight + RateHeight;
        BaseOceanFog.height = StartHeight + RateHeight;
        WalkOceanFog.height = StartHeight + RateHeight;
    }

    public bool IsDay()
    {
        if ((neoSky.Cycle.TimeOfDay < 18.0f) && (neoSky.Cycle.TimeOfDay > 4.0f))
        {

           
            return true;
        }

      
        return false;
    }
}
