using UnityEngine;
using System.Collections;

public class PlasterPuffScript : MonoBehaviour {

		float opac = 0;
		Component[] renderers, lights;

		void UpdateColor() {
				renderers = transform.GetComponentsInChildren(MeshRenderer);
				Color color = new Vector4(0,0,0,opac*0.2f);
				foreach(MeshRenderer renderer in renderers){
						renderer.material.SetColor("_TintColor", color);
				}
		}

		void Start () {
				opac = Random.Range(0.0f,1.0f);
				UpdateColor();
				transform.localRotation.eulerAngles.z = Random.Range(0.0f,360.0f);
				transform.localScale.x = Random.Range(0.8f,2.0f);
				transform.localScale.y = Random.Range(0.8f,2.0f);
				transform.localScale.z = Random.Range(0.8f,2.0f);
		}

		void Update() {
				UpdateColor();
				opac -= Time.deltaTime * 10;
				transform.localScale += Vector3(1,1,1)*Time.deltaTime*30;
				if(opac <= 0){
						Destroy(gameObject,0);
				}
		}
}
