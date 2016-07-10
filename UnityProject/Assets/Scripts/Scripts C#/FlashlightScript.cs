using UnityEngine;
using System.Collections;

public class FlashlightScript : MonoBehaviour {

		AnimationCurve battery_curve;
		AudioClip sound_turn_on, sound_turn_off;
		private float kSoundVolume = 0.3f, max_battery_life = 60*60*5.5f, battery_life_remaining = max_battery_life;
		private bool switch_on;

		private float initial_pointlight_intensity, initial_spotlight_intensity;

		void Start () {
				initial_pointlight_intensity = transform.FindChild("Pointlight").gameObject.GetComponent<Light>().intensity;
				initial_spotlight_intensity = transform.FindChild("Spotlight").gameObject.GetComponent<Light>().intensity;
				battery_life_remaining = Random.Range(max_battery_life*0.2f, max_battery_life);
		}

		public void TurnOn(){
				if(!switch_on){
						switch_on = true;
						GetComponent<AudioSource>().PlayOneShot(sound_turn_on, kSoundVolume * PlayerPrefs.GetFloat("sound_volume", 1));
				}
		}

		public void TurnOff(){
				if(switch_on){
						switch_on = false;
						GetComponent<AudioSource>().PlayOneShot(sound_turn_off, kSoundVolume * PlayerPrefs.GetFloat("sound_volume", 1));
				}
		}

		void Update () {
				if(switch_on){
						battery_life_remaining -= Time.deltaTime;
						if(battery_life_remaining <= 0){
								battery_life_remaining = 0;
						}
						var battery_curve_eval = battery_curve.Evaluate(1.0-battery_life_remaining/max_battery_life);
						transform.FindChild("Pointlight").gameObject.GetComponent<Light>().intensity = initial_pointlight_intensity * battery_curve_eval * 8.0;
						transform.FindChild("Spotlight").gameObject.GetComponent<Light>().intensity = initial_spotlight_intensity * battery_curve_eval * 3.0;
						transform.FindChild("Pointlight").gameObject.GetComponent<Light>().enabled = true;
						transform.FindChild("Spotlight").gameObject.GetComponent<Light>().enabled = true;
				} else {
						transform.FindChild("Pointlight").gameObject.GetComponent<Light>().enabled = false;
						transform.FindChild("Spotlight").gameObject.GetComponent<Light>().enabled = false;
				}
				if(GetComponent<Rigidbody>()){
						transform.FindChild("Pointlight").GetComponent<Light>().enabled = true;
						transform.FindChild("Pointlight").GetComponent<Light>().intensity = 1 + Mathf.Sin(Time.time * 2.0f);
						transform.FindChild("Pointlight").GetComponent<Light>().range = 1;
				} else {
						transform.FindChild("Pointlight").GetComponent<Light>().range = 10;
				}
		}
}
