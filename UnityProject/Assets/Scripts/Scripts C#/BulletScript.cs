using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletScript : MonoBehaviour {

		AudioClip[] sound_hit_concrete, sound_hit_metal, sound_hit_glass, sound_hit_body, sound_hit_ricochet, sound_glass_break, sound_flyby;

		GameObject bullet_obj, bullet_hole_obj, glass_bullet_hole_obj, metal_bullet_hole_obj, spark_effect, puff_effect;
		private Vector3 old_pos, velocity;
		private bool hit_something, hostile = false;
		private LineRenderer line_renderer; 
		private float life_time = 0;
		private float death_time = 0;
		private int segment = 1;

		public 	void SetVelocity(Vector3 vel){
				this.velocity = vel;
		}

		public void SetHostile(){
				GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Logarithmic;
				PlaySoundFromGroup(sound_flyby, 0.4f);
				hostile = true;
		}

		void Start () {
				line_renderer = GetComponent<LineRenderer>();
				line_renderer.SetPosition(0, transform.position);
				line_renderer.SetPosition(1, transform.position);
				old_pos = transform.position;
		}

		public 	MonoBehaviour RecursiveHasScript(GameObject obj,string script, int depth) {
				if(obj.GetComponent(script)){
						return (MonoBehaviour)obj.GetComponent(script);
				} else if(depth > 0 && obj.transform.parent){
						return RecursiveHasScript(obj.transform.parent.gameObject, script, depth-1);
				} else {
						return null;
				}
		}

		public static Quaternion RandomOrientation() {
				return Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
		}

		public void PlaySoundFromGroup(AudioClip[] group,float volume){
				var which_shot = Random.Range(0,(group.Length-1));
				GetComponent<AudioSource>().PlayOneShot(group[which_shot], volume * PlayerPrefs.GetFloat("sound_volume", 1.0f));
		}
		RaycastHit hit, new_hit;

		void Update () {
				if(!hit_something){
						life_time += Time.deltaTime;
						if(life_time > 1.5){
								hit_something = true;
						}
						transform.position += velocity * Time.deltaTime;
						velocity += Physics.gravity * Time.deltaTime;
						if(Physics.Linecast(old_pos, transform.position, out hit, 1<<0 | 1<<9 | 1<<11)){
								GameObject hit_obj = hit.collider.gameObject;
								GameObject hit_transform_obj = hit.transform.gameObject;
								ShootableLight light_script  = (ShootableLight)RecursiveHasScript(hit_obj, "ShootableLight", 1);
								AimScript aim_script = (AimScript)RecursiveHasScript(hit_obj, "AimScript", 1);
								RobotScript turret_script = (RobotScript)RecursiveHasScript(hit_obj, "RobotScript", 3);
								transform.position = hit.point;
								float ricochet_amount = Vector3.Dot(velocity.normalized, hit.normal) * -1;
								if(Random.Range(0.0f,1.0f) > ricochet_amount && Vector3.Magnitude(velocity) * (1-ricochet_amount) > 10){
										GameObject ricochet = Instantiate(bullet_obj, hit.point, transform.rotation) as GameObject;
										Vector3 ricochet_vel = velocity * 0.3f * (1-ricochet_amount);
										velocity -= ricochet_vel;
										ricochet_vel = Vector3.Reflect(ricochet_vel, hit.normal);
										ricochet.GetComponent<BulletScript>().SetVelocity(ricochet_vel);
										PlaySoundFromGroup(sound_hit_ricochet, hostile ? 1 : 0.6f);
								} else if(turret_script && velocity.magnitude > 100){
										
										if(Physics.Linecast(hit.point + velocity.normalized * 0.001f, hit.point + velocity.normalized, out new_hit, 1<<11 | 1<<12)){
												if(new_hit.collider.gameObject.layer == 12){
														turret_script.WasShotInternal(new_hit.collider.gameObject);
												}
										}					
								}
								if(hit_transform_obj.GetComponent<Rigidbody>()){
										hit_transform_obj.GetComponent<Rigidbody>().AddForceAtPosition(velocity * 0.01f, hit.point, ForceMode.Impulse);
								}
								if(light_script){
										light_script.WasShot(hit_obj, hit.point, velocity);
										if(hit.collider.material.name == "glass (Instance)"){
												PlaySoundFromGroup(sound_glass_break, 1);
										}
								}
								if(Vector3.Magnitude(velocity) > 50){
										GameObject hole, effect;
										if(turret_script){
												PlaySoundFromGroup(sound_hit_metal, hostile ? 1.0f : 0.8f);
												hole = Instantiate(metal_bullet_hole_obj, hit.point, RandomOrientation()) as GameObject;
												effect = Instantiate(spark_effect, hit.point, RandomOrientation())as GameObject;
												turret_script.WasShot(hit_obj, hit.point, velocity);
										} else if(aim_script){
												hole = Instantiate(bullet_hole_obj, hit.point, RandomOrientation())as GameObject;
												effect = Instantiate(puff_effect, hit.point, RandomOrientation())as GameObject;
												PlaySoundFromGroup(sound_hit_body, 1.0f);
												aim_script.WasShot();
										} else if(hit.collider.material.name == "metal (Instance)"){
												PlaySoundFromGroup(sound_hit_metal, hostile ? 1.0f : 0.4f);
												hole = Instantiate(metal_bullet_hole_obj, hit.point, RandomOrientation())as GameObject;
												effect = Instantiate(spark_effect, hit.point, RandomOrientation())as GameObject;
										} else if(hit.collider.material.name == "glass (Instance)"){
												PlaySoundFromGroup(sound_hit_glass, hostile ? 1.0f : 0.4f);
												hole = Instantiate(glass_bullet_hole_obj, hit.point, RandomOrientation())as GameObject;
												effect = Instantiate(spark_effect, hit.point, RandomOrientation())as GameObject;
										} else {
												PlaySoundFromGroup(sound_hit_concrete, hostile ? 1.0f : 0.4f);
												hole = Instantiate(bullet_hole_obj, hit.point, RandomOrientation())as GameObject;
												effect = Instantiate(puff_effect, hit.point, RandomOrientation())as GameObject;
										}
										effect.transform.position += hit.normal * 0.05f;
										hole.transform.position += hit.normal * 0.01f;
										if(!aim_script){
												hole.transform.parent = hit_obj.transform;
										} else {
												hole.transform.parent = GameObject.Find("Main Camera").transform;
										}
								}
								hit_something = true;
						}
						line_renderer.SetVertexCount(segment+1);
						line_renderer.SetPosition(segment, transform.position);
						++segment;
				} else {
						life_time += Time.deltaTime;
						death_time += Time.deltaTime;
						//Destroy(this.gameObject);
				}
				for(var i=0; i<segment; ++i){
						Color start_color = new Color(1,1,1,(1.0f - life_time * 5.0f)*0.05f);
						Color end_color = new Color(1,1,1,(1.0f - death_time * 5.0f)*0.05f);
						line_renderer.SetColors(start_color, end_color);
						if(death_time > 1.0f){
								Destroy(this.gameObject,0);
						}
				}
		}
}
