using UnityEngine;
using System.Collections;

public class TaserSpark : MonoBehaviour {

		float opac= 0;
		Component[] renderers, lights;
		Color color;
				void  UpdateColor (){
						renderers= transform.GetComponentsInChildren<MeshRenderer>();
						color= new Vector4(opac,opac,opac,opac);
						foreach(MeshRenderer renderer in renderers){
								renderer.material.SetColor("_TintColor", color);
						}
						lights= transform.GetComponentsInChildren<Light>();
						foreach(Light light in lights){
								light.intensity = opac * 2.0f;
						}
				}

				void  Start (){
						opac = Random.Range(0.4f,1.0f);
						UpdateColor();
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Random.Range(0.0f,360.0f));
				transform.localScale = new Vector3(Random.Range(0.8f,2.0f),Random.Range(0.8f,2.0f),Random.Range(0.8f,2.0f));
				}

				void  Update (){
						UpdateColor();
						opac -= Time.deltaTime * 50.0f;
						if(opac <= 0.0f){
								Destroy(gameObject);
						}
				}
		
}
