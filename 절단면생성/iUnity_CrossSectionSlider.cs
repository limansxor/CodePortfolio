using UnityEngine;
using System.Collections;

//// <summary>
///  2017년 '육군과학 전투 훈련단 교보재'
///  절단면을 Panel 오브젝트 이용해서 가시과
/// </summary>
public class iUnity_CrossSectionSlider : MonoBehaviour {

    public enum CrossControlSlider {ROTZ,ROTX, OFFSET }

    public SectionSetter sectionSetter;

    public CrossControlSlider crossControlSlider;

    GameObject ParentModel;

    Section sectVars;

    bool OnInit = false;

    // Use this for initialization
    IEnumerator Start()
    {
        while (true)
        {
            if (iUnity_Manager.Instance.ParentGroupObj)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        ParentModel = GameObject.Find("ParentModel");
        sectVars = ParentModel.GetComponent<CrossSection>().sectVars;
        switch (crossControlSlider)
        {
            case CrossControlSlider.ROTZ:
                GetComponent<UISlider>().value = 0.5f;//(sectVars.rot_x + 90) / 180.0f;
                break;
            case CrossControlSlider.ROTX:
                GetComponent<UISlider>().value = 0.5f; //sectVars.rot_z / 180.0f;
                break;
            case CrossControlSlider.OFFSET:
                GetComponent<UISlider>().value = 0.5f;  //(sectVars.offset + sectVars.offsetRange) / (sectVars.offsetRange * 2);
                //     Debug.Log((sectVars.offset + sectVars.offsetRange) / (sectVars.offsetRange * 2));
                break;
            default:
                break;
        }
        sectVars = sectionSetter.section;
        yield break;
    }

    // Update is called once per frame
    void Update () {
       

    }

    public void OnValueChange()
    {

        if (OnInit)
        {
            switch (crossControlSlider)
            {
                case CrossControlSlider.ROTZ:

                    sectionSetter.section.rot_x = (GetComponent<UISlider>().value * 180.0f) - 90.0f;

                    sectionSetter.GUiChanged();
                    break;
                case CrossControlSlider.ROTX:
                    sectionSetter.section.rot_z = (GetComponent<UISlider>().value * 180.0f);

                    sectionSetter.GUiChanged();
                    break;
                case CrossControlSlider.OFFSET:
                   float value = (GetComponent<UISlider>().value * sectionSetter.section.offsetRange * 2) - sectionSetter.section.offsetRange;

                   
                    sectionSetter.section.offset = Mathf.Clamp(value, -sectionSetter.section.offsetRange, sectionSetter.section.offsetRange);
                    sectionSetter.GUiChanged();
                    break;
                default:
                    break;

            }
        }
        OnInit = true;

    }
}
