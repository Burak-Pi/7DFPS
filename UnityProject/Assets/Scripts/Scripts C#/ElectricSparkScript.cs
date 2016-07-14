using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class electricsparkscript  : MonoBehaviour {

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
						light.intensity = opac * 2.0f;
				}

		}

		void Start () {
				opac = Random.Range(0.4f,1.0f);
				UpdateColor();
				transform.eulerAngles = new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, Random.Range(0.0f,360.0f));
				transform.localScale = new Vector3(Random.Range(0.8f,2.0f), Random.Range(0.8f,2.0f), Random.Range(0.8f,2.0f));
		}

		void Update() {
				UpdateColor();
				opac -= Time.deltaTime * 5.0f;
				transform.localScale += Vector3.one*Time.deltaTime;
				if(opac <= 0){
						Destroy(gameObject,0);
				}
		}
}
