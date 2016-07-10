using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShellCasingScript : MonoBehaviour {

				AudioClip[] sound_shell_bounce;
				public bool collided = false;
				Vector3 old_pos;
		float life_time= 0.0f, glint_delay= 0, glint_progress= 0;
				private Light glint_light;

		void  PlaySoundFromGroup (AudioClip[] group, float volume){
				int which_shot = Random.Range(0,group.Length-1);
						GetComponent<AudioSource>().PlayOneShot(group[which_shot], volume * PlayerPrefs.GetFloat("sound_volume", 1.0f));
				}

				void  Start (){
						old_pos = transform.position;
						if(transform.FindChild("light_pos")){
								glint_light = transform.FindChild("light_pos").GetComponent<Light>();
								glint_light.enabled = false;
						}
				}

				void  CollisionSound (){
						if(!collided){
								collided = true;
								PlaySoundFromGroup(sound_shell_bounce, 0.3f);
						}
				}

				void  FixedUpdate (){
						if(GetComponent<Rigidbody>() && !GetComponent<Rigidbody>().IsSleeping() && GetComponent<Collider>() && GetComponent<Collider>().enabled){
								life_time += Time.deltaTime;
								RaycastHit hit;
								if(Physics.Linecast(old_pos, transform.position, out hit, 1)){
										transform.position = hit.point;
										transform.GetComponent<Rigidbody>().velocity *= -0.3f;
								}
								if(life_time > 2.0f){
										GetComponent<Rigidbody>().Sleep();
								}
						}
						if(GetComponent<Rigidbody>() && GetComponent<Rigidbody>().IsSleeping() && glint_light){
								if(glint_delay == 0.0f){
										glint_delay = Random.Range(1.0f,5.0f);
								}
								glint_delay = Mathf.Max(0.0f, glint_delay - Time.deltaTime);
								if(glint_delay == 0.0f){
										glint_progress = 1.0f;
								}
								if(glint_progress > 0.0f){
										glint_light.enabled = true;
										glint_light.intensity = Mathf.Sin(glint_progress * Mathf.PI);
										glint_progress = Mathf.Max(0.0f, glint_progress - Time.deltaTime * 2.0f);
								} else {
										glint_light.enabled = false;
								}
						}
						old_pos = transform.position;
				}

				void  OnCollisionEnter ( Collision collision  ){
						CollisionSound();
				}
		
}
