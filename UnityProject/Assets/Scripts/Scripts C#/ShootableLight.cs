using UnityEngine;
using System.Collections;

public class ShootableLight : MonoBehaviour {

		public GameObject destroy_effect;

		public Color light_color = new Color(1,1,1);
		public bool destroyed;
		public LightType light_type = LightType.NORMAL;
		private float blink_delay = 0, light_amount = 1;
		Color combined_color;
		public enum LightType {
			AIRPLANE_BLINK, NORMAL, FLICKER
		}
	
	void Update () {
				if(!destroyed){
						switch(light_type){
						case LightType.AIRPLANE_BLINK:
								if(blink_delay <= 0.0){
										blink_delay = 1;
										if(light_amount == 1){
												light_amount = 0;
										} else {
												light_amount = 1;
										}
								}
								blink_delay -= Time.deltaTime;
								break;
						}
				}

				combined_color = new Color(light_color.r * light_amount,light_color.g * light_amount,light_color.b * light_amount);
				foreach(Light light in gameObject.GetComponentsInChildren<Light>()){
						light.color = Color.white;
				}
				foreach(MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>()){
						renderer.material.SetColor("_Illum", combined_color);
						if(renderer.gameObject.name == "shade"){
								renderer.material.SetColor("_Illum", combined_color * 0.5f);
						}
				}
	}


		public void WasShot(GameObject obj, Vector3 pos, Vector3 vel) {
				if(!destroyed){
						destroyed = true;
						light_amount = 0;
						Instantiate(destroy_effect, transform.FindChild("bulb").position, Quaternion.identity);
				}
				if(obj && obj.GetComponent<Collider>() && obj.GetComponent<Collider>().material.name == "glass (Instance)"){
						GameObject.Destroy(obj);
				}
		}

}
