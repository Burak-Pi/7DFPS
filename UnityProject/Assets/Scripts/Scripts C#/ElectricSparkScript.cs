using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElectricSparkScript : MonoBehaviour {

		float opac = 0;
		Component[] renderers, lights;

		void UpdateColor() {

				renderers = transform.GetComponentsInChildren(MeshRenderer);
				Color color = Vector4(opac,opac,opac,opac);
				foreach(MeshRenderer renderer in renderers){
						renderer.material.SetColor("_TintColor", color);
				}

				lights = transform.GetComponentsInChildren(Light);
				foreach(Light light in lights){
						light.intensity = opac * 2.0f;
				}

		}

		void Start () {
				opac = Random.Range(0.4f,1.0f);
				UpdateColor();
				transform.localRotation.eulerAngles.z = Random.Range(0.0f,360.0f);
				transform.localScale.x = Random.Range(0.8f,2.0f);
				transform.localScale.y = Random.Range(0.8f,2.0f);
				transform.localScale.z = Random.Range(0.8f,2.0f);
		}

		void Update() {
				UpdateColor();
				opac -= Time.deltaTime * 5.0f;
				transform.localScale += Vector3(1,1,1)*Time.deltaTime;
				if(opac <= 0){
						Destroy(gameObject,0);
				}
		}
}
