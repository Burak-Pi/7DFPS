using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotScript : MonoBehaviour {


		AudioClip[] sound_gunshot,_damage_camera, sound_damage_gun,  sound_damage_battery, sound_damage_camera
		, sound_damage_ammo, sound_damage_motor, sound_bump;
		AudioClip sound_alert, sound_unalert, sound_engine_loop, sound_damaged_engine_loop;

		private AudioSource audiosource_taser, audiosource_motor,  audiosource_effect, audiosource_foley;
		private GameObject object_audiosource_motor;
		private float sound_line_of_sight = 0;

		GameObject electric_spark_obj, muzzle_flash, bullet_obj;

		enum RobotType {SHOCK_DRONE, STATIONARY_TURRET, MOBILE_TURRET, GUN_DRONE};
		RobotType robot_type;

		private float gun_delay = 0;
		private bool alive = true;
		private AimScript.Spring rotation_x = new AimScript.Spring(0,0,100,0.0001f)
				, rotation_y = new AimScript.Spring(0,0,100,0.0001f);
		private Quaternion initial_turret_orientation;
		private Vector3 initial_turret_position;
		private Transform gun_pivot;
		enum AIState {IDLE, ALERT, ALERT_COOLDOWN, AIMING, FIRING, DEACTIVATING, DEAD};
		private AIState ai_state = AIState.IDLE;
		private bool battery_alive = true, motor_alive = true, camera_alive = true, trigger_alive = true
				, barrel_alive = true, ammo_alive = true, trigger_down, stuck, distance_sleep;
		private int bullets = 15;
		private float kAlertDelay = 0.6f, kAlertCooldownDelay = 2, alert_delay = 0
				, alert_cooldown_delay = 0, kMaxRange = 20, rotor_speed = 0, top_rotor_rotation = 0
				, bottom_rotor_rotation = 0, stuck_delay = 0, kSleepDistance = 20;
		private Vector3 initial_pos, tilt_correction;

		Vector3 target_pos;
		enum CameraPivotState {DOWN, WAIT_UP, UP, WAIT_DOWN};
		CameraPivotState camera_pivot_state = CameraPivotState.WAIT_DOWN;
		float camera_pivot_delay = 0, camera_pivot_angle = 0;

		void PlaySoundFromGroup(AudioClip[] group,float volume){
				if (group == null) {
						return;
				}
				if(group.Length == 0){
						return;
				}
				int which_shot = Random.Range(0,group.Length-1);
				audiosource_effect.PlayOneShot(group[which_shot], volume * PlayerPrefs.GetFloat("sound_volume", 1));
		}

		GameObject GetTurretLightObject() {
				return transform.FindChild("gun pivot").FindChild("camera").FindChild("light").gameObject;
		}

		GameObject GetDroneLightObject() {
				return transform.FindChild("camera_pivot").FindChild("camera").FindChild("light").gameObject;
		}

		GameObject GetDroneLensFlareObject() {
				return transform.FindChild("camera_pivot").FindChild("camera").FindChild("lens flare").gameObject;
		}


		Quaternion RandomOrientation() {
				return Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
		}

		void Damage(GameObject obj){
				bool damage_done = false;
				if(obj.name == "battery" && battery_alive){
						battery_alive = false;
						motor_alive = false;
						camera_alive = false;
						trigger_alive = false;
						if(robot_type == RobotType.SHOCK_DRONE){
								barrel_alive = false;
						}
						PlaySoundFromGroup(sound_damage_battery,1);
						rotation_x.target_state = 40;
						damage_done = true;
				} else if((obj.name == "pivot motor" || obj.name == "motor") && motor_alive){
						motor_alive = false;
						PlaySoundFromGroup(sound_damage_motor,1);
						damage_done = true;
				} else if(obj.name == "power cable" && (camera_alive || trigger_alive)){
						camera_alive = false;
						damage_done = true;
						PlaySoundFromGroup(sound_damage_battery,1);
						trigger_alive = false;
				} else if(obj.name == "ammo box" && ammo_alive){
						ammo_alive = false;
						PlaySoundFromGroup(sound_damage_ammo,1);
						damage_done = true;
				} else if((obj.name == "gun" || obj.name == "shock prod") && barrel_alive){
						barrel_alive = false;
						PlaySoundFromGroup(sound_damage_gun,1);
						damage_done = true;
				} else if(obj.name == "camera" && camera_alive){
						camera_alive = false;
						PlaySoundFromGroup(sound_damage_camera,1);
						damage_done = true;
				} else if(obj.name == "camera armor" && camera_alive){
						camera_alive = false;
						PlaySoundFromGroup(sound_damage_camera,1);
						damage_done = true;
				}
				if(damage_done){
						Instantiate(electric_spark_obj, obj.transform.position, RandomOrientation());
				}
		}

		public void WasShotInternal(GameObject obj) {
				Damage(obj);
		}

		public void WasShot(GameObject obj,Vector3 pos,Vector3 vel) {
				if(transform.parent && transform.parent.gameObject.name == "gun pivot"){
						//Multiplying quaternion with a Vector3 returns a vector3 type
						Vector3 x_axis = transform.FindChild("point_pivot").rotation * Vector3.right;
						Vector3 y_axis = transform.FindChild("point_pivot").rotation * Vector3.up;
						Vector3 z_axis = transform.FindChild("point_pivot").rotation * Vector3.forward;

						Vector3 y_plane_vel = new Vector3(Vector3.Dot(vel, x_axis), 0, Vector3.Dot(vel, z_axis));
						Vector3 rel_pos = pos - transform.FindChild("point_pivot").position;
						Vector3 y_plane_pos = new Vector3(Vector3.Dot(rel_pos, z_axis), 0, -Vector3.Dot(rel_pos, x_axis));
						rotation_y.vel += Vector3.Dot(y_plane_vel, y_plane_pos) * 10;

						Vector3 x_plane_vel = new Vector3(Vector3.Dot(vel, y_axis), 0, Vector3.Dot(vel, z_axis));
						rel_pos = pos - transform.FindChild("point_pivot").position;
						Vector3 x_plane_pos = new Vector3(-Vector3.Dot(rel_pos, z_axis), 0, Vector3.Dot(rel_pos, y_axis));
						rotation_x.vel += Vector3.Dot(x_plane_vel, x_plane_pos) * 10;
				}
				if(robot_type == RobotType.SHOCK_DRONE){
						if(Random.Range(0.0f,1f) < 0.5f){
								Damage(transform.FindChild("battery").gameObject);
						}
				} else {
						if(Random.Range(0.0f,1f) < 0.25f){
								Damage(transform.FindChild("battery").gameObject);
						}
				}
				Damage(obj);
		}

		void Start () {

				audiosource_effect = gameObject.AddComponent<AudioSource>();
				audiosource_effect.rolloffMode = AudioRolloffMode.Linear;
				audiosource_effect.maxDistance = 30;

				object_audiosource_motor = new GameObject("motor audiosource object");
				object_audiosource_motor.transform.parent = transform;
				object_audiosource_motor.transform.localPosition = Vector3.zero;

				audiosource_motor = object_audiosource_motor.AddComponent<AudioSource>();
				object_audiosource_motor.AddComponent<AudioLowPassFilter>();
				audiosource_motor.loop = true;
				audiosource_motor.volume = 0.4f * PlayerPrefs.GetFloat("sound_volume", 1);
				audiosource_motor.clip = sound_engine_loop;

				switch(robot_type){
				case RobotType.STATIONARY_TURRET:
						gun_pivot = transform.FindChild("gun pivot");
						initial_turret_orientation = gun_pivot.transform.localRotation;
						initial_turret_position = gun_pivot.transform.localPosition;
						audiosource_motor.rolloffMode = AudioRolloffMode.Linear;
						audiosource_motor.maxDistance = 4;
						break;
				case RobotType.SHOCK_DRONE:
						audiosource_motor.maxDistance = 8;
						audiosource_foley = gameObject.AddComponent<AudioSource>();
						audiosource_taser = gameObject.AddComponent<AudioSource>();
						audiosource_taser.rolloffMode = AudioRolloffMode.Linear;
						audiosource_taser.loop = true;
						audiosource_taser.clip = sound_gunshot[0];
						break;
				}

				initial_pos = transform.position;	
				target_pos = initial_pos;
		}
		RaycastHit hit;
		void UpdateStationaryTurret() {
				if(Vector3.Distance(GameObject.Find("Player").transform.position, transform.position) > kSleepDistance){
						GetTurretLightObject().GetComponent<Light>().shadows = LightShadows.None;		
						if(audiosource_motor.isPlaying){
								audiosource_motor.Stop();
						}
						return;
				} else {
						if(!audiosource_motor.isPlaying){
								audiosource_motor.volume = PlayerPrefs.GetFloat("sound_volume", 1);
								audiosource_motor.Play();
						}
						audiosource_motor.volume = 0.4f * PlayerPrefs.GetFloat("sound_volume", 1);
						if(GetTurretLightObject().GetComponent<Light>().intensity > 0){
								GetTurretLightObject().GetComponent<Light>().shadows = LightShadows.Hard;
						} else {
								GetTurretLightObject().GetComponent<Light>().shadows = LightShadows.None;
						}
				}
				if(motor_alive){
						switch(ai_state){
						case AIState.IDLE:
								rotation_y.target_state += Time.deltaTime * 100;
								break;				
						case AIState.AIMING:
						case AIState.ALERT:
						case AIState.ALERT_COOLDOWN:
						case AIState.FIRING:
								var rel_pos = target_pos - transform.FindChild("point_pivot").position;
								var x_axis = transform.FindChild("point_pivot").rotation * Vector3.right;
								var y_axis = transform.FindChild("point_pivot").rotation * Vector3.up;
								var z_axis = transform.FindChild("point_pivot").rotation * Vector3.forward;
								var y_plane_pos = new Vector3(Vector3.Dot(rel_pos, z_axis), 0, -Vector3.Dot(rel_pos, x_axis)).normalized;
								var target_y = Mathf.Atan2(y_plane_pos.x, y_plane_pos.z)/Mathf.PI*180-90;
								while(target_y > rotation_y.state + 180){
										rotation_y.state += 360;
								}
								while(target_y < rotation_y.state - 180){
										rotation_y.state -= 360;
								}
								rotation_y.target_state = target_y;
								var y_height = Vector3.Dot(y_axis, rel_pos.normalized);
								var target_x = -Mathf.Asin(y_height)/Mathf.PI*180;
								rotation_x.target_state = target_x;
								rotation_x.target_state = Mathf.Min(40,Mathf.Max(-40,target_x));
								break;
						}
				}
				if(battery_alive){
						switch(ai_state){
						case AIState.FIRING:
								trigger_down = true;
								break;
						default:
								trigger_down = false;
								break;
						}
				}
				if(barrel_alive){
						if(trigger_down){
								if(gun_delay <= 0){
										gun_delay += 0.1f;
										var point_muzzle_flash = gun_pivot.FindChild("gun").FindChild("point_muzzleflash");
										Instantiate(muzzle_flash, point_muzzle_flash.position, point_muzzle_flash.rotation);
										PlaySoundFromGroup(sound_gunshot, 1);

										GameObject bullet = Instantiate(bullet_obj, point_muzzle_flash.position, point_muzzle_flash.rotation) as GameObject;
										bullet.GetComponent<BulletScript>().SetVelocity(point_muzzle_flash.forward * 300);
										bullet.GetComponent<BulletScript>().SetHostile();
										rotation_x.vel += Random.Range(-50,50);
										rotation_y.vel += Random.Range(-50,50);
										--bullets;
								}
						}
						if(ammo_alive && bullets > 0){
								gun_delay = Mathf.Max(0, gun_delay - Time.deltaTime);
						}
				}
				float danger = 0;
				GameObject player = GameObject.Find("Player");
				float dist = Vector3.Distance(player.transform.position, transform.position);
				if(battery_alive){
						danger += Mathf.Max(0, 1- dist/kMaxRange);
				}
				if(camera_alive){
						if(danger > 0){
								danger = Mathf.Min(0.2f, danger);
						}
						if(ai_state == AIState.AIMING || ai_state == AIState.FIRING){
								danger = 1;
						}
						if(ai_state == AIState.ALERT || ai_state == AIState.ALERT_COOLDOWN){
								danger += 0.5f;
						}

						Transform camera = transform.FindChild("gun pivot").FindChild("camera");
						Vector3 rel_pos = player.transform.position - camera.position;
						var sees_target = false;
						if(dist < kMaxRange && Vector3.Dot(camera.rotation*Vector3.down, rel_pos.normalized) > 0.7f){
								if(!Physics.Linecast(camera.position, player.transform.position, out hit, 1<<0)){
										sees_target = true;
								}
						}
						if(sees_target){
								switch(ai_state){
								case AIState.IDLE:
										ai_state = AIState.ALERT;
										audiosource_effect.PlayOneShot(sound_alert, 0.3f * PlayerPrefs.GetFloat("sound_volume", 1));
										alert_delay = kAlertDelay;
										break;
								case AIState.AIMING:
										if(Vector3.Dot(camera.rotation*Vector3.down, rel_pos.normalized) > 0.9f){
												ai_state = AIState.FIRING;
										}
										target_pos = player.transform.position;
										break;					
								case AIState.FIRING:
										target_pos = player.transform.position;
										break;
								case AIState.ALERT:
										alert_delay -= Time.deltaTime;
										if(alert_delay <= 0){
												ai_state = AIState.AIMING;
										}
										target_pos = player.transform.position;
										break;
								case AIState.ALERT_COOLDOWN:
										ai_state = AIState.ALERT;
										alert_delay = kAlertDelay;
										break;
								}
						} else {
								switch(ai_state){
								case AIState.AIMING:
								case AIState.FIRING:
								case AIState.ALERT:
										ai_state = AIState.ALERT_COOLDOWN;
										alert_cooldown_delay = kAlertCooldownDelay;
										break;
								case AIState.ALERT_COOLDOWN:
										alert_cooldown_delay -= Time.deltaTime;
										if(alert_cooldown_delay <= 0){
												ai_state = AIState.IDLE;
												audiosource_effect.PlayOneShot(sound_unalert, 0.3f * PlayerPrefs.GetFloat("sound_volume", 1));
										}
										break;
								}
						}
						switch(ai_state){
						case AIState.IDLE:
								GetTurretLightObject().GetComponent<Light>().color = new Color(0,0,1);
								break;
						case AIState.AIMING:
								GetTurretLightObject().GetComponent<Light>().color = new Color(1,0,0);
								break;
						case AIState.ALERT:
						case AIState.ALERT_COOLDOWN:
								GetTurretLightObject().GetComponent<Light>().color = new Color(1,1,0);
								break;
						}
				}
				player.GetComponent<MusicScript>().AddDangerLevel(danger);
				if(!camera_alive){
						GetTurretLightObject().GetComponent<Light>().intensity *= Mathf.Pow(0.01f, Time.deltaTime);
				}
				var target_pitch = (Mathf.Abs(rotation_y.vel) + Mathf.Abs(rotation_x.vel)) * 0.01f;
				target_pitch = Mathf.Clamp(target_pitch, 0.2f, 2);
				audiosource_motor.pitch = Mathf.Lerp(audiosource_motor.pitch, target_pitch, Mathf.Pow(0.0001f, Time.deltaTime));

				rotation_x.Update();
				rotation_y.Update();
				gun_pivot.localRotation = initial_turret_orientation;
				gun_pivot.localPosition = initial_turret_position;
				gun_pivot.RotateAround(
						transform.FindChild("point_pivot").position, 
						transform.FindChild("point_pivot").rotation * Vector3.right,
						rotation_x.state);
				gun_pivot.RotateAround(
						transform.FindChild("point_pivot").position, 
						transform.FindChild("point_pivot").rotation * Vector3.up,
						rotation_y.state);
		}

		void UpdateDrone() {
				if(Vector3.Distance(GameObject.Find("Player").transform.position, transform.position) > kSleepDistance){
						GetDroneLightObject().GetComponent<Light>().shadows = LightShadows.None;
						if(motor_alive){
								distance_sleep = true;
								GetComponent<Rigidbody>().Sleep();
						}
						if(audiosource_motor.isPlaying){
								audiosource_motor.Stop();
						}
						return;
				} else {
						if(GetDroneLightObject().GetComponent<Light>().intensity > 0){
								GetDroneLightObject().GetComponent<Light>().shadows = LightShadows.Hard;
						} else {
								GetDroneLightObject().GetComponent<Light>().shadows = LightShadows.None;
						}
						if(motor_alive && distance_sleep){
								GetComponent<Rigidbody>().WakeUp();
								distance_sleep = false;
						}
						if(!audiosource_motor.isPlaying){
								audiosource_motor.volume = PlayerPrefs.GetFloat("sound_volume", 1);
								audiosource_motor.Play();
						}
				}
				var rel_pos = target_pos - transform.position;
				if(motor_alive){		
						var kFlyDeadZone = 0.2f;
						var kFlySpeed = 10;
						Vector3 target_vel = (target_pos - transform.position) / kFlyDeadZone;
						if(target_vel.magnitude > 1){
								target_vel = target_vel.normalized;
						}
						target_vel *= kFlySpeed;
						var target_accel = (target_vel - GetComponent<Rigidbody>().velocity);
						if(ai_state == AIState.IDLE){
								target_accel *= 0.1f;
						}
						target_accel.y += 9.81f;

						rotor_speed = target_accel.magnitude;
						rotor_speed = Mathf.Clamp(rotor_speed, 0, 14);

						var up = transform.rotation * Vector3.up;
						Quaternion correction = new Quaternion();
						correction.SetFromToRotation(up, target_accel.normalized);
						Vector3 correction_vec;
						float correction_angle;
						correction.ToAngleAxis(out correction_angle, out correction_vec);
						tilt_correction = correction_vec * correction_angle;
						tilt_correction -= GetComponent<Rigidbody>().angularVelocity;


						var x_axis = transform.rotation * Vector3.right;
						var y_axis = transform.rotation * Vector3.up;
						var z_axis = transform.rotation * Vector3.forward;
						if(ai_state != AIState.IDLE){
								Vector3 y_plane_pos = new Vector3(Vector3.Dot(rel_pos, z_axis), 0, -Vector3.Dot(rel_pos, x_axis)).normalized;
								var target_y = Mathf.Atan2(y_plane_pos.x, y_plane_pos.z)/Mathf.PI*180-90;
								while(target_y > 180){
										target_y -= 360;
								}
								while(target_y < -180){
										target_y += 360;
								}
								tilt_correction += y_axis * target_y;	
								tilt_correction *= 5;
						} else {
								tilt_correction += y_axis;	
						}

						if(ai_state == AIState.IDLE){
								tilt_correction *= 0.1f;
						}

						if(GetComponent<Rigidbody>().velocity.magnitude < 0.2){ 
								stuck_delay += Time.deltaTime;
								if(stuck_delay > 1){
										target_pos = transform.position + new Vector3(Random.Range(-1,1), Random.Range(-1,1), Random.Range(-1,1));
										stuck_delay = 0;
								}
						} else {
								stuck_delay = 0;
						}

				} else {
						rotor_speed = Mathf.Max(0, rotor_speed - Time.deltaTime * 5);
						GetComponent<Rigidbody>().angularDrag = 0.05f;
				}
				if(barrel_alive && ai_state == AIState.FIRING){
						if(!audiosource_taser.isPlaying){
								audiosource_taser.volume = PlayerPrefs.GetFloat("sound_volume", 1);
								audiosource_taser.Play();
						} else {
								audiosource_taser.volume = PlayerPrefs.GetFloat("sound_volume", 1);
						}
						if(gun_delay <= 0){
								gun_delay = 0.1f;	
								Instantiate(muzzle_flash, transform.FindChild("point_spark").position, RandomOrientation());
								if(Vector3.Distance(transform.FindChild("point_spark").position, GameObject.Find("Player").transform.position) < 1){;
										GameObject.Find("Player").GetComponent<AimScript>().Shock();
								}
						}
				} else {
						audiosource_taser.Stop();
				}
				gun_delay = Mathf.Max(0, gun_delay - Time.deltaTime);

				top_rotor_rotation += rotor_speed * Time.deltaTime * 1000;
				bottom_rotor_rotation -= rotor_speed * Time.deltaTime * 1000;
				if(rotor_speed * Time.timeScale > 7){
						transform.FindChild("bottom rotor").gameObject.GetComponent<Renderer>().enabled = false;
						transform.FindChild("top rotor").gameObject.GetComponent<Renderer>().enabled = false;
				} else {
						transform.FindChild("bottom rotor").gameObject.GetComponent<Renderer>().enabled = true;
						transform.FindChild("top rotor").gameObject.GetComponent<Renderer>().enabled = true;
				}

				Vector3 tempEul = transform.FindChild("top rotor").localEulerAngles;
				transform.FindChild("top rotor").localEulerAngles = new Vector3(tempEul.x, top_rotor_rotation, tempEul.z);
				tempEul = transform.FindChild("bottom rotor").localEulerAngles;
				transform.FindChild("bottom rotor").localEulerAngles = new Vector3(tempEul.x, bottom_rotor_rotation, tempEul.z);

				//rigidbody.velocity += transform.rotation * Vector3(0,1,0) * rotor_speed * Time.deltaTime;
				if(camera_alive){
						if(ai_state == AIState.IDLE){
								switch(camera_pivot_state) {
								case CameraPivotState.DOWN:
										camera_pivot_angle += Time.deltaTime * 25;
										if(camera_pivot_angle > 50){
												camera_pivot_angle = 50;
												camera_pivot_state = CameraPivotState.WAIT_UP;
												camera_pivot_delay = 0.2f;
										}
										break;
								case CameraPivotState.UP:
										camera_pivot_angle -= Time.deltaTime * 25;
										if(camera_pivot_angle < 0){
												camera_pivot_angle = 0;
												camera_pivot_state = CameraPivotState.WAIT_DOWN;
												camera_pivot_delay = 0.2f;
										}
										break;
								case CameraPivotState.WAIT_DOWN:
										camera_pivot_delay -= Time.deltaTime;
										if(camera_pivot_delay < 0){
												camera_pivot_state = CameraPivotState.DOWN;
										}
										break;
								case CameraPivotState.WAIT_UP:
										camera_pivot_delay -= Time.deltaTime;
										if(camera_pivot_delay < 0){
												camera_pivot_state = CameraPivotState.UP;
										}
										break;
								}
						} else {
								camera_pivot_angle -= Time.deltaTime * 25;
								if(camera_pivot_angle < 0){
										camera_pivot_angle = 0;
								}
						}
						var cam_pivot = transform.FindChild("camera_pivot");
						cam_pivot.localEulerAngles = new Vector3(camera_pivot_angle, cam_pivot.localEulerAngles.y, cam_pivot.localEulerAngles.z);
						var player = GameObject.Find("Player");
						var dist = Vector3.Distance(player.transform.position, transform.position);
						var danger = Mathf.Max(0, 1- dist/kMaxRange);
						if(danger > 0){
								danger = Mathf.Min(0.2f, danger);
						}
						if(ai_state == AIState.AIMING || ai_state == AIState.FIRING){
								danger = 1;
						}
						if(ai_state == AIState.ALERT || ai_state == AIState.ALERT_COOLDOWN){
								danger += 0.5f;
						}
						player.GetComponent<MusicScript>().AddDangerLevel(danger);

						var camera = transform.FindChild("camera_pivot").FindChild("camera");
						rel_pos = player.transform.position - camera.position;
						var sees_target = false;
						if(dist < kMaxRange && Vector3.Dot(camera.rotation*Vector3.down, rel_pos.normalized) > 0.7f){
								if(!Physics.Linecast(camera.position, player.transform.position, out hit, 1<<0)){
										sees_target = true;
								}
						}
						if(sees_target){
								var new_target = player.transform.position + player.GetComponent<CharacterMotor>().GetVelocity() * 
										Mathf.Clamp(Vector3.Distance(player.transform.position, transform.position) * 0.1f, 0.5f, 1);
								switch(ai_state){
								case AIState.IDLE:
										ai_state = AIState.ALERT;
										alert_delay = kAlertDelay;
										audiosource_effect.PlayOneShot(sound_alert, 0.3f * PlayerPrefs.GetFloat("sound_volume", 1));
										break;
								case AIState.AIMING:
										target_pos = new_target;
										if(Vector3.Distance(transform.position, target_pos) < 4){
												ai_state = AIState.FIRING;
										}
										target_pos.y += 1;
										break;					
								case AIState.FIRING:
										target_pos = new_target;
										if(Vector3.Distance(transform.position, target_pos) > 4){
												ai_state = AIState.AIMING;
										}
										break;
								case AIState.ALERT:
										alert_delay -= Time.deltaTime;
										target_pos = new_target;
										target_pos.y += 1;
										if(alert_delay <= 0){
												ai_state = AIState.AIMING;
										}
										break;
								case AIState.ALERT_COOLDOWN:
										ai_state = AIState.ALERT;
										alert_delay = kAlertDelay;
										break;
								}
						} else {
								switch(ai_state){
								case AIState.AIMING:
								case AIState.FIRING:
								case AIState.ALERT:
										ai_state = AIState.ALERT_COOLDOWN;
										alert_cooldown_delay = kAlertCooldownDelay;
										break;
								case AIState.ALERT_COOLDOWN:
										alert_cooldown_delay -= Time.deltaTime;
										if(alert_cooldown_delay <= 0){
												ai_state = AIState.IDLE;
												audiosource_effect.PlayOneShot(sound_unalert, 0.3f * PlayerPrefs.GetFloat("sound_volume", 1));
										}
										break;
								}
						}
						switch(ai_state){
						case AIState.IDLE:
								GetDroneLightObject().GetComponent<Light>().color = new Color(0,0,1);
								break;
						case AIState.AIMING:
								GetDroneLightObject().GetComponent<Light>().color = new Color(1,0,0);
								break;
						case AIState.ALERT:
						case AIState.ALERT_COOLDOWN:
								GetDroneLightObject().GetComponent<Light>().color = new Color(1,1,0);
								break;
						}
				}
				if(!camera_alive){
						GetDroneLightObject().GetComponent<Light>().intensity *= Mathf.Pow(0.01f, Time.deltaTime);
				}
				(GetDroneLensFlareObject().GetComponent<LensFlare>() as LensFlare).color = GetDroneLightObject().GetComponent<Light>().color;
				(GetDroneLensFlareObject().GetComponent<LensFlare>() as LensFlare).brightness = GetDroneLightObject().GetComponent<Light>().intensity;
				var target_pitch = rotor_speed * 0.2f;
				target_pitch = Mathf.Clamp(target_pitch, 0.2f, 3);
				audiosource_motor.pitch = Mathf.Lerp(audiosource_motor.pitch, target_pitch, Mathf.Pow(0.0001f, Time.deltaTime));
				audiosource_motor.volume = rotor_speed * 0.1f * PlayerPrefs.GetFloat("sound_volume", 1);

				audiosource_motor.volume -= Vector3.Distance(GameObject.Find("Main Camera").transform.position, transform.position) * 0.0125f * PlayerPrefs.GetFloat("sound_volume", 1);

				var line_of_sight = true;
				if(Physics.Linecast(transform.position, GameObject.Find("Main Camera").transform.position, out hit, 1<<0)){
						line_of_sight = false;
				}
				if(line_of_sight){
						sound_line_of_sight += Time.deltaTime * 3;
				} else {
						sound_line_of_sight -= Time.deltaTime * 3;
				}
				sound_line_of_sight = Mathf.Clamp(sound_line_of_sight,0,1);

				audiosource_motor.volume *= 0.5f + sound_line_of_sight * 0.5f;
				object_audiosource_motor.GetComponent<AudioLowPassFilter>().cutoffFrequency = 
						Mathf.Lerp(5000, 44000, sound_line_of_sight);
		}


		void Update () {
				switch(robot_type){
				case RobotType.STATIONARY_TURRET:
						UpdateStationaryTurret();
						break;
				case RobotType.SHOCK_DRONE:
						UpdateDrone();
						break;
				}
		}

		void OnCollisionEnter(Collision collision) {
				if(robot_type == RobotType.SHOCK_DRONE){
						if(collision.relativeVelocity.magnitude > 10){
								if(Random.Range(0f,1f)<0.5f && motor_alive){
										Damage(transform.FindChild("motor").gameObject);
								} else if(Random.Range(0f,1f)<0.5f && camera_alive){
										Damage(transform.FindChild("camera_pivot").FindChild("camera").gameObject);
								} else if(Random.Range(0f,1f)<0.5f && battery_alive){
										Damage(transform.FindChild("battery").gameObject);
								} else {
										motor_alive = true;
										Damage(transform.FindChild("motor").gameObject);
								} 
						} else {
								var which_shot = Random.Range(0,sound_bump.Length);
								audiosource_foley.PlayOneShot(sound_bump[which_shot], collision.relativeVelocity.magnitude * 0.15f * PlayerPrefs.GetFloat("sound_volume", 1));
						}
				}
		}

		void FixedUpdate() {
				if(robot_type == RobotType.SHOCK_DRONE && !distance_sleep){
						GetComponent<Rigidbody>().AddForce(transform.rotation * Vector3.up * rotor_speed, ForceMode.Force);
						if(motor_alive){
								GetComponent<Rigidbody>().AddTorque(tilt_correction, ForceMode.Force);
						}
				}
		}

}
