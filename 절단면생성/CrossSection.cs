using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//// <summary>
///  2017년 '육군과학 전투 훈련단 교보재'
///  절단면 형성 모듈 
/// </summary>
public class CrossSection : MonoBehaviour {
	private Material [][] allMaterials;
	private Material [][] allMatInstances;
	private Renderer[] renderers;
    [HideInInspector]
	public SectionSetter setter;
	private Vector3 boundsCentre;
	private Vector4 sectionplane = new Vector4(0f,0f,1f,1f);
	public Section sectVars;


	void Awake () {	
	

        //enabled = !enabled;
        //setter.newSettings(gameObject, sectVars);
        //Cut(enabled);
    }


public 	IEnumerator Start () {
        while (true)
        {
            if (iUnity_Manager.Instance.Resource_Loding_Complete ==true)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }

        renderers = GetComponentsInChildren<Renderer>();
        if(renderers ==null)
        {
            Debug.Log("잡히는게 없군요");
        }
        sectionplane = new Vector4(Mathf.Sin(Mathf.Deg2Rad * sectVars.rot_x), Mathf.Cos(Mathf.Deg2Rad * sectVars.rot_x) * Mathf.Sin(Mathf.Deg2Rad * sectVars.rot_z), Mathf.Cos(Mathf.Deg2Rad * sectVars.rot_x) * Mathf.Cos(Mathf.Deg2Rad * sectVars.rot_z), 1);
        setter = FindObjectOfType(typeof(SectionSetter)) as SectionSetter;
        calculateBounds();
        makeSectionMaterials();
        enabled = false;

        yield break;
	}


	void makeSectionMaterials () {
		int rl = renderers.Length;
		allMaterials = new Material[rl][];
		allMatInstances = new Material[rl][];
		for(int i = 0; i < rl; i++) {
			allMaterials[i]= renderers[i].materials;
			int n = renderers[i].materials.Length;
			allMatInstances[i] = new Material [n];
			for(int j = 0; j < n; j++) {
				allMatInstances[i][j] = new Material(allMaterials[i][j]);
				string shaderName = allMatInstances[i][j].shader.name;
				Shader replacementShader = Shader.Find("CrossSection/" + shaderName);
				if(replacementShader==null){
					if(shaderName.Contains("Transparent/VertexLit")) {
						replacementShader = Shader.Find("CrossSection/Transparent/Specular");
					}else if(shaderName.Contains("Transparent")) {
						replacementShader = Shader.Find("CrossSection/Transparent/Diffuse");
					}else{
						replacementShader = Shader.Find("CrossSection/Diffuse");
					}
				}
				allMatInstances[i][j].shader = replacementShader;
				allMatInstances[i][j].SetVector("_SectionPoint", new Vector4(boundsCentre.x, boundsCentre.y, boundsCentre.z, 1));
				allMatInstances[i][j].SetVector("_SectionPlane", sectionplane);
				allMatInstances[i][j].SetFloat("_ClipOffset", sectVars.offset);
				
			}
		}
	}

     

	void OnMouseDown() {
		enabled = !enabled;
		setter.newSettings(gameObject,sectVars);
		Cut(enabled);
	}


	public void Cut (bool val) {
		enabled = val;
		for(int i = 0; i < renderers.Length; i++) {
			if(val){
				renderers[i].materials = allMatInstances[i];
			}else{
				renderers[i].materials = allMaterials[i];
			}
		}
	}


	public void applySettings (Section val) {

		sectionplane = new Vector4(Mathf.Sin(Mathf.Deg2Rad*val.rot_x),Mathf.Cos(Mathf.Deg2Rad*val.rot_x)*Mathf.Sin(Mathf.Deg2Rad*val.rot_z),Mathf.Cos(Mathf.Deg2Rad*val.rot_x)*Mathf.Cos(Mathf.Deg2Rad*val.rot_z),1);
		int rl = allMatInstances.Length;
		for(int i = 0; i < rl; i++) {
			int n = allMatInstances[i].Length;
			for(int j = 0; j < n; j++) {
				allMatInstances[i][j].SetVector("_SectionPlane", sectionplane);
				allMatInstances[i][j].SetFloat("_ClipOffset", val.offset);
			}
		}
	}


	void calculateBounds() {
		if (renderers.Length > 0) {
			
			Bounds bound = renderers[0].bounds;
			for(int i = 1; i < renderers.Length; i++) {
				bound.Encapsulate(renderers[i].bounds);
               

            }
			boundsCentre = bound.center;
			sectVars.offsetRange = bound.extents.magnitude;
		}
	}
}
