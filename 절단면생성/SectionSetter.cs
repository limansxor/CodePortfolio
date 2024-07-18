using UnityEngine;
using System.Collections;

//// <summary>
///  2017년 '육군과학 전투 훈련단 교보재'
///  절단면 형성 모듈 제어코드 
/// </summary>
public class SectionSetter : MonoBehaviour {
	private GameObject toBeSectioned;
	public Shader[] crossSectionShaders;
	public GUISkin skin1;
	private MonoBehaviour orbitScript;
    [HideInInspector]
	public Section section;

	// Use this for initialization
	void Start () {
		try{
			orbitScript = Camera.main.GetComponent<MonoBehaviour>();
		}
		catch{
			return;
		}
	}
	
	public void newSettings (GameObject go, Section sect) {

		if(go == toBeSectioned) {
			toBeSectioned = null;
			}else{
			if(toBeSectioned){
				CrossSection cs = toBeSectioned.GetComponent<CrossSection>();
				cs.Cut(false);
			}
			toBeSectioned = go;
			section = sect;
		}
	}
	
	// Update is called once per frame
	void OnGUI () {
		
		GUI.skin = skin1;
	}
	public void GUiChanged()
    {
        CrossSection cs = toBeSectioned.GetComponent<CrossSection>();
        cs.applySettings(section);
    }
}
