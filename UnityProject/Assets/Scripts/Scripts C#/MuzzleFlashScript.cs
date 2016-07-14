using UnityEngine;
using System.Collections;

public class muzzleflashscript : MonoBehaviour {

		float opac = 0;
		Component[] renderers, lights;

		void UpdateColor() {
				renderers = transform.GetComponentsInChildren<MeshRenderer>();
				Color color = new Vector4(opac,opac,opac,opac);
				foreach(MeshRenderer renderer in renderers){
						renderer.material.SetColor("_TintColor", color);
				}
				lights = transform.GetComponentsInChildren<Light>();
				foreach(Light light in lights){
						light.intensity = opac;
				}
		}

		void Start () {
				opac = Random.Range(0.0f,1.0f);
				UpdateColor();
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Random.Range(0.0f,360.0f));
				transform.localScale = new Vector3(Random.Range(0.8f,2.0f),Random.Range(0.8f,2.0f),Random.Range(0.8f,2.0f));
		}

		void Update() {
				UpdateColor();
				opac -= Time.deltaTime * 50;
				if(opac <= 0){
						Destroy(gameObject,0);
				}
		}
}
