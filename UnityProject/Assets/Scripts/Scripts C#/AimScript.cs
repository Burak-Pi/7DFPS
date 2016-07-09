﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AimScript : MonoBehaviour {
// Prefabs 

private  GameObject magazine_obj, gun_obj, casing_with_bullet;
Texture texture_death_screen;

AudioClip[] sound_bullet_grab, sound_body_fall, sound_electrocute;

AudioSource audiosource_tape_background, audiosource_audio_content;

// Shortcuts to components

private GameObject main_camera;
private CharacterController character_controller;
private bool show_help, show_advanced_help, help_ever_shown, just_started_help;
private float help_hold_time = 0;

// Instances

private GameObject gun_instance;

// Public parameters

private float sensitivity_x = 2, sensitivity_y = 2, min_angle_y = -89, max_angle_y = 89;

//these classes haven't been converted yet so they produce errors
private GUISkinHolder holder;
private WeaponHolder weapon_holder;

//bools are by default false
bool disable_springs, disable_recoil = true;

// Private variables

public class Spring {
public float state,  target_state, vel, strength, damping; 

	public Spring ( float state,   float target_state ,   float strength ,   float damping  ){
		this.Set(state, target_state, strength, damping); 
	}
	public void  Set ( float state ,   float target_state ,   float strength ,   float _damping  ){
		this.state = state;
		this.target_state = target_state;
		this.strength = strength;
		this.damping = _damping;
		this.vel = 0.0f;		
	}
	public void  Update (){ 
		bool linear_springs = true;
		if(linear_springs){
				this.state = Mathf.MoveTowards(this.state, this.target_state, this.strength * Time.deltaTime * 0.05f);
		} else {	 
				this.vel += (this.target_state - this.state) * this.strength * Time.deltaTime;
				this.vel *= Mathf.Pow(this.damping, Time.deltaTime);
				this.state += this.vel * Time.deltaTime;
		}
	}
	
};

private bool aim_toggle= false;
private float kAimSpringStrength= 100.0f;
		private float kAimSpringDamping= 0.00001f;
		private Spring aim_spring= new Spring(0,0,kAimSpringStrength,kAimSpringDamping);

private GameObject held_flashlight = null;
private Vector3 flashlight_aim_pos;
private Quaternion flashlight_aim_rot;
		private Spring flashlight_mouth_spring= new Spring(0,0,kAimSpringStrength,kAimSpringDamping);
		private Spring flash_ground_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
		private Vector3 flash_ground_pos;

		private float rotation_x_leeway = 0;
		private float rotation_y_min_leeway = 0;
		private float rotation_y_max_leeway = 0;
		private float kRotationXLeeway = 5;
		private float kRotationYMinLeeway = 20;
		private float kRotationYMaxLeeway = 10;

		private float rotation_x = 0;
		private float rotation_y = 0;
		private float view_rotation_x = 0;
		private float view_rotation_y = 0;

		private float kRecoilSpringStrength = 800;
		private float kRecoilSpringDamping = 0.000001;
		private Spring x_recoil_spring = new Spring(0,0,kRecoilSpringStrength,kRecoilSpringDamping);
		private Spring y_recoil_spring = new Spring(0,0,kRecoilSpringStrength,kRecoilSpringDamping);
		private Spring head_recoil_spring_x = new Spring(0,0,kRecoilSpringStrength,kRecoilSpringDamping);
		private Spring head_recoil_spring_y = new Spring(0,0,kRecoilSpringStrength,kRecoilSpringDamping);

		private Vector3 mag_pos;
		private Quaternion mag_rot;

		private GameObject magazine_instance_in_hand;
		private float kGunDistance = 0.3;

		private Spring slide_pose_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
		private Spring reload_pose_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
		private Spring press_check_pose_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
		private Spring inspect_cylinder_pose_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
		private Spring add_rounds_pose_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
		private Spring eject_rounds_pose_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);

		enum GunTilt {LEFT, CENTER, RIGHT};
		private GunTilt gun_tilt = GunTilt.CENTER;

		private Spring hold_pose_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
		private Spring mag_ground_pose_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);

		private bool left_hand_occupied = false;
		private float kMaxHeadRecoil = 10;
		private float[] head_recoil_delay = new float[kMaxHeadRecoil];
		private float next_head_recoil_delay = 0;
		private Vector3 mag_ground_pos;
		private Quaternion mag_ground_rot;

		enum HandMagStage {HOLD, HOLD_TO_INSERT, EMPTY};
		private HandMagStage mag_stage = HandMagStage.EMPTY;

		private List<GameObject> collected_rounds = new List<GameObject>();

		private int target_weapon_slot = -2;
		private bool queue_drop = false;
		private List<GameObject> loose_bullets = new List<GameObject>();
		private List<Spring> loose_bullet_spring = new List<Spring>();
		private Spring show_bullet_spring = new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
		private float picked_up_bullet_delay = 0;

		private float head_fall = 0;
		private float head_fall_vel = 0;
		private float head_tilt = 0;
		private float head_tilt_vel = 0;
		private float head_tilt_x_vel = 0;
		private float head_tilt_y_vel = 0;
		private float dead_fade = 1;
		private float win_fade = 0;
		private float dead_volume_fade = 0;
		private bool dead_body_fell = false;

		private float start_tape_delay = 0;
		private float stop_tape_delay = 0;
		private List<AudioClip> tapes_heard = new List<AudioClip>();
		private List<AudioClip> tapes_remaining = new List<AudioClip>();
		private List<AudioClip> total_tapes = new List<AudioClip>();
		private bool tape_in_progress = false;
		private float unplayed_tapes = 0;

		private bool god_mode = false;
		private bool slomo_mode = false;
		private float iddqd_progress = 0;
		private float idkfa_progress = 0;
		private float slomo_progress = 0;
		private float cheat_delay = 0;
		private float level_reset_hold = 0;

		enum WeaponSlotType {GUN, MAGAZINE, FLASHLIGHT, EMPTY, EMPTYING};

		class WeaponSlot {
				GameObject obj = null;
				WeaponSlotType type = WeaponSlotType.EMPTY;
				Vector3 start_pos  = Vector3.zero;
				Quaternion start_rot = Quaternion.identity;
				Spring spring = new Spring(1,1,100,0.000001f);
		}

		private WeaponSlot[] weapon_slots = new WeaponSlot[10];

		private float health = 1;
		private bool dying, dead, won;

		bool IsAiming() {
				return (gun_instance != null && aim_spring.target_state == 1.0);
		}

		bool IsDead() {
				return dead;
		}

		void StepRecoil(float amount) {
				x_recoil_spring.vel += Random.Range(100,400) * amount;
				y_recoil_spring.vel += Random.Range(-200,200) * amount;
		}

		void WasShot(){
				head_recoil_spring_x.vel += Random.Range(-400,400);
				head_recoil_spring_y.vel += Random.Range(-400,400);
				x_recoil_spring.vel += Random.Range(-400,400);
				y_recoil_spring.vel += Random.Range(-400,400);
				rotation_x += Random.Range(-4,4);
				rotation_y += Random.Range(-4,4);
				if(!god_mode && !won){
						dying = true;
						if(Random.Range(0.0,1.0) < 0.3){
								SetDead(true);
						}
						if(dead && Random.Range(0.0,1.0) < 0.3){
								dead_fade += 0.3;
						}
				}
		}

		void FallDeath(Vector3 vel) {
				if(!god_mode && !won){
						SetDead(true);
						head_fall_vel = vel.y;
						dead_fade = Mathf.Max(dead_fade, 0.5);
						head_recoil_spring_x.vel += Random.Range(-400,400);
						head_recoil_spring_y.vel += Random.Range(-400,400);
				}
		}

		void InstaKill() {
				SetDead(true);
				dead_fade = 1;
		}

		void Shock() {
				if(!god_mode && !won){
						if(!dead){
								PlaySoundFromGroup(sound_electrocute, 1.0);
						}
						SetDead(true);
				}
				head_recoil_spring_x.vel += Random.Range(-400,400);
				head_recoil_spring_y.vel += Random.Range(-400,400);
		}

		void SetDead(bool new_dead) {
				if(new_dead == dead){
						return;
				}
				dead = new_dead;
				if(!dead){
						head_tilt_vel = 0;
						head_tilt_x_vel = 0;
						head_tilt_y_vel = 0;
						head_tilt = 0;
						head_fall = 0;
				} else {
						GetComponent<MusicScript>().HandleEvent(MusicEvent.DEAD);
						head_tilt_vel = Random.Range(-100,100);
						head_tilt_x_vel = Random.Range(-100,100);
						head_tilt_y_vel = Random.Range(-100,100);
						head_fall = 0;
						head_fall_vel = 0;
						dead_body_fell = false;
				}
		}

		void PlaySoundFromGroup(List<AudioClip> group, float volume){
				int which_shot = Random.Range(0,(group.Count-1));
				GetComponent<AudioSource>().PlayOneShot(group[which_shot], volume * PlayerPrefs.GetFloat("sound_volume", 1.0));
		}

		void AddLooseBullet(bool spring) {
				loose_bullets.Add(Instantiate(casing_with_bullet));
				var new_spring = new Spring(0.3,0.3,kAimSpringStrength,kAimSpringDamping);
				loose_bullet_spring.Add(new_spring);
				if(spring){
						new_spring.vel = 3;
						picked_up_bullet_delay = 2;
				}
		}

		void Start() { 
				disable_springs = false; 
				disable_recoil = true;
				holder = GameObject.Find("gui_skin_holder").GetComponent(GUISkinHolder);
				weapon_holder = holder.weapon.GetComponent(WeaponHolder);
				magazine_obj = weapon_holder.mag_object;
				gun_obj = weapon_holder.gun_object;
				casing_with_bullet = weapon_holder.bullet_object;

				if(Random.Range(0.0,1.0) < 0.35){
						held_flashlight = Instantiate(holder.flashlight_object);
						Destroy(held_flashlight.GetComponent.<Rigidbody>());
						held_flashlight.GetComponent(FlashlightScript).TurnOn();
						holder.has_flashlight = true;
				}

				rotation_x = transform.rotation.eulerAngles.y;
				view_rotation_x = transform.rotation.eulerAngles.y;
				gun_instance = Instantiate(gun_obj);
				var renderers = gun_instance.GetComponentsInChildren(Renderer);
				for(var renderer : Renderer in renderers){
						renderer.castShadows = false; 
				}
				main_camera = GameObject.Find("Main Camera").gameObject;
				character_controller = GetComponent(CharacterController);
				for(var i=0; i<kMaxHeadRecoil; ++i){
						head_recoil_delay[i] = -1;
				}
				for(i=0; i<10; ++i){
						weapon_slots[i] = new WeaponSlot();
				}
				var num_start_bullets = Random.Range(0,10);
				if(GetGunScript().gun_type == GunType.AUTOMATIC){
						var num_start_mags = Random.Range(0,3);
						for(i=1; i<num_start_mags+1; ++i){
								weapon_slots[i].type = WeaponSlotType.MAGAZINE;
								weapon_slots[i].obj = Instantiate(magazine_obj);
						}
				} else {
						num_start_bullets += Random.Range(0,20);
				}
				loose_bullets = new Array();
				loose_bullet_spring = new Array();
				for(i=0; i<num_start_bullets; ++i){
						AddLooseBullet(false);
				}
				audiosource_tape_background = gameObject.AddComponent(AudioSource);
				audiosource_tape_background.loop = true;
				audiosource_tape_background.clip = holder.sound_tape_background;
				audiosource_audio_content = gameObject.AddComponent(AudioSource);
				audiosource_audio_content.loop = false;

				var count = 0;
				for(var tape in holder.sound_tape_content){
						total_tapes.push(tape);
						/*++count;
		if(count >= 2){
			break;
		}*/
				}
				var temp_total_tapes = new Array(total_tapes);
				while(temp_total_tapes.length > 0){
						var rand_tape_id = Random.Range(0,temp_total_tapes.length);
						tapes_remaining.push(temp_total_tapes[rand_tape_id]);
						temp_total_tapes.RemoveAt(rand_tape_id);
				}
		}

		function GunDist() {
				return kGunDistance * (0.5 + PlayerPrefs.GetFloat("gun_distance", 1.0)*0.5);
		}

		function AimPos() : Vector3 {
				var aim_dir = AimDir();
				return main_camera.transform.position + aim_dir*GunDist();
		}

		function AimDir() : Vector3 {
				var aim_rot = Quaternion();
				aim_rot.SetEulerAngles(-rotation_y * Mathf.PI / 180.0, rotation_x * Mathf.PI / 180.0, 0.0);
				return aim_rot * Vector3(0.0,0.0,1.0);
		}

		function GetGunScript() : GunScript {
				return gun_instance.GetComponent(GunScript);
		}

		function mix( a:Vector3, b:Vector3, val:float ) : Vector3{
				return a + (b-a) * val;
		}

		function mix( a:Quaternion, b:Quaternion, val:float ) : Quaternion{
				var angle = 0;
				var axis = Vector3();
				(Quaternion.Inverse(b)*a).ToAngleAxis(angle, axis);
				if(angle > 180){
						angle -= 360;
				}
				if(angle < -180){
						angle += 360;
				}
				if(angle == 0){
						return a;
				}
				return a * Quaternion.AngleAxis(angle * -val, axis);
		}

		function ShouldPickUpNearby() : boolean {
				var nearest_mag = null;
				var nearest_mag_dist = 0;
				var colliders = Physics.OverlapSphere(main_camera.transform.position, 2.0, 1 << 8);
				for(var collider in colliders){
						if(magazine_obj && collider.gameObject.name == magazine_obj.name+"(Clone)" && collider.gameObject.GetComponent.<Rigidbody>()){
								if(mag_stage == HandMagStage.EMPTY){
										return true;
								}	
						} else if((collider.gameObject.name == casing_with_bullet.name || collider.gameObject.name == casing_with_bullet.name+"(Clone)") && collider.gameObject.GetComponent.<Rigidbody>()){
								return true;
						}
				}
				return false;
		}

		function HandleGetControl(){
				var nearest_mag = null;
				var nearest_mag_dist = 0;
				var colliders = Physics.OverlapSphere(main_camera.transform.position, 2.0, 1 << 8);
				for(var collider in colliders){
						if(magazine_obj && collider.gameObject.name == magazine_obj.name+"(Clone)" && collider.gameObject.GetComponent.<Rigidbody>()){
								var dist = Vector3.Distance(collider.transform.position, main_camera.transform.position);
								if(!nearest_mag || dist < nearest_mag_dist){	
										nearest_mag_dist = dist;
										nearest_mag = collider.gameObject;
								}					
						} else if((collider.gameObject.name == casing_with_bullet.name || collider.gameObject.name == casing_with_bullet.name+"(Clone)") && collider.gameObject.GetComponent.<Rigidbody>()){
								collected_rounds.push(collider.gameObject);			
								collider.gameObject.GetComponent.<Rigidbody>().useGravity = false;
								collider.gameObject.GetComponent.<Rigidbody>().WakeUp();
								collider.enabled = false;
						} else if(collider.gameObject.name == "cassette_tape(Clone)" && collider.gameObject.GetComponent.<Rigidbody>()){
								collected_rounds.push(collider.gameObject);			
								collider.gameObject.GetComponent.<Rigidbody>().useGravity = false;
								collider.gameObject.GetComponent.<Rigidbody>().WakeUp();
								collider.enabled = false;
						} else if(collider.gameObject.name == "flashlight_object(Clone)" && collider.gameObject.GetComponent.<Rigidbody>() && !held_flashlight){
								held_flashlight = collider.gameObject;
								Destroy(held_flashlight.GetComponent.<Rigidbody>());
								held_flashlight.GetComponent(FlashlightScript).TurnOn();
								holder.has_flashlight = true;
								flash_ground_pos = held_flashlight.transform.position;
								flash_ground_rot = held_flashlight.transform.rotation;
								flash_ground_pose_spring.state = 1;
								flash_ground_pose_spring.vel = 1;
						}
				}
				if(nearest_mag && mag_stage == HandMagStage.EMPTY){
						magazine_instance_in_hand = nearest_mag;
						Destroy(magazine_instance_in_hand.GetComponent.<Rigidbody>());
						mag_ground_pos = magazine_instance_in_hand.transform.position;
						mag_ground_rot = magazine_instance_in_hand.transform.rotation;
						mag_ground_pose_spring.state = 1;
						mag_ground_pose_spring.vel = 1;
						hold_pose_spring.state = 1;
						hold_pose_spring.vel = 0;
						hold_pose_spring.target_state = 1;
						mag_stage = HandMagStage.HOLD;
				}
		}

		function HandleInventoryControls() : boolean {	
				if(Input.GetButtonDown("Holster")){
						target_weapon_slot = -1;
				}
				if(Input.GetButtonDown("Inventory 1")){
						target_weapon_slot = 0;
				}
				if(Input.GetButtonDown("Inventory 2")){
						target_weapon_slot = 1;
				}
				if(Input.GetButtonDown("Inventory 3")){
						target_weapon_slot = 2;
				}
				if(Input.GetButtonDown("Inventory 4")){
						target_weapon_slot = 3;
				}
				if(Input.GetButtonDown("Inventory 5")){
						target_weapon_slot = 4;
				}
				if(Input.GetButtonDown("Inventory 6")){
						target_weapon_slot = 5;
				}
				if(Input.GetButtonDown("Inventory 7")){
						target_weapon_slot = 6;
				}
				if(Input.GetButtonDown("Inventory 8")){
						target_weapon_slot = 7;
				}
				if(Input.GetButtonDown("Inventory 9")){
						target_weapon_slot = 8;
				}
				if(Input.GetButtonDown("Inventory 10")){
						target_weapon_slot = 9;
				}

				var mag_ejecting = false;
				if(gun_instance && (gun_instance.GetComponent(GunScript).IsMagCurrentlyEjecting() || gun_instance.GetComponent(GunScript).ready_to_remove_mag)){
						mag_ejecting = true;
				}

				var insert_mag_with_number_key = false;

				if(target_weapon_slot != -2 && !mag_ejecting && (mag_stage == HandMagStage.EMPTY || mag_stage == HandMagStage.HOLD)){
						if(target_weapon_slot == -1 && !gun_instance){
								for(var i=0; i<10; ++i){
										if(weapon_slots[i].type == WeaponSlotType.GUN){
												target_weapon_slot = i;
												break;
										}
								}
						}
						if(mag_stage == HandMagStage.HOLD && target_weapon_slot != -1 && weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTY){
								// Put held mag in empty slot
								for(i=0; i<10; ++i){
										if(weapon_slots[target_weapon_slot].type != WeaponSlotType.EMPTY && weapon_slots[target_weapon_slot].obj == magazine_instance_in_hand){
												weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTY;
										}
								}
								weapon_slots[target_weapon_slot].type = WeaponSlotType.MAGAZINE;
								weapon_slots[target_weapon_slot].obj = magazine_instance_in_hand;
								weapon_slots[target_weapon_slot].spring.state = 0;
								weapon_slots[target_weapon_slot].spring.target_state = 1;
								weapon_slots[target_weapon_slot].start_pos = magazine_instance_in_hand.transform.position - main_camera.transform.position;
								weapon_slots[target_weapon_slot].start_rot = Quaternion.Inverse(main_camera.transform.rotation) * magazine_instance_in_hand.transform.rotation;
								magazine_instance_in_hand = null;
								mag_stage = HandMagStage.EMPTY;
								target_weapon_slot = -2;
						} else if(mag_stage == HandMagStage.HOLD && target_weapon_slot != -1 && weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTYING && weapon_slots[target_weapon_slot].obj == magazine_instance_in_hand && gun_instance && !gun_instance.GetComponent(GunScript).IsThereAMagInGun()){
								insert_mag_with_number_key = true;
								target_weapon_slot = -2;
						} else if (target_weapon_slot != -1 && mag_stage == HandMagStage.EMPTY && weapon_slots[target_weapon_slot].type == WeaponSlotType.MAGAZINE){
								// Take mag from inventory
								magazine_instance_in_hand = weapon_slots[target_weapon_slot].obj;
								mag_stage = HandMagStage.HOLD;
								hold_pose_spring.state = 1;
								hold_pose_spring.target_state = 1;
								weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTYING;
								weapon_slots[target_weapon_slot].spring.target_state = 0;
								weapon_slots[target_weapon_slot].spring.state = 1;
								target_weapon_slot = -2;
						} else if (target_weapon_slot != -1 && mag_stage == HandMagStage.EMPTY && weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTY && held_flashlight){
								// Put flashlight away
								held_flashlight.GetComponent(FlashlightScript).TurnOff();
								weapon_slots[target_weapon_slot].type = WeaponSlotType.FLASHLIGHT;
								weapon_slots[target_weapon_slot].obj = held_flashlight;
								weapon_slots[target_weapon_slot].spring.state = 0;
								weapon_slots[target_weapon_slot].spring.target_state = 1;
								weapon_slots[target_weapon_slot].start_pos = held_flashlight.transform.position - main_camera.transform.position;
								weapon_slots[target_weapon_slot].start_rot = Quaternion.Inverse(main_camera.transform.rotation) * held_flashlight.transform.rotation;
								held_flashlight = null;
								target_weapon_slot = -2;
						}  else if (target_weapon_slot != -1 && !held_flashlight && weapon_slots[target_weapon_slot].type == WeaponSlotType.FLASHLIGHT){
								// Take flashlight from inventory
								held_flashlight = weapon_slots[target_weapon_slot].obj;
								held_flashlight.GetComponent(FlashlightScript).TurnOn();
								weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTYING;
								weapon_slots[target_weapon_slot].spring.target_state = 0;
								weapon_slots[target_weapon_slot].spring.state = 1;
								target_weapon_slot = -2;
						} else if(gun_instance && target_weapon_slot == -1){
								// Put gun away
								if(target_weapon_slot == -1){
										for(i=0; i<10; ++i){
												if(weapon_slots[i].type == WeaponSlotType.EMPTY){
														target_weapon_slot = i;
														break;
												}
										}
								}
								if(target_weapon_slot != -1 && weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTY){
										for(i=0; i<10; ++i){
												if(weapon_slots[target_weapon_slot].type != WeaponSlotType.EMPTY && weapon_slots[target_weapon_slot].obj == gun_instance){
														weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTY;
												}
										}
										weapon_slots[target_weapon_slot].type = WeaponSlotType.GUN;
										weapon_slots[target_weapon_slot].obj = gun_instance;
										weapon_slots[target_weapon_slot].spring.state = 0;
										weapon_slots[target_weapon_slot].spring.target_state = 1;
										weapon_slots[target_weapon_slot].start_pos = gun_instance.transform.position - main_camera.transform.position;
										weapon_slots[target_weapon_slot].start_rot = Quaternion.Inverse(main_camera.transform.rotation) * gun_instance.transform.rotation;
										gun_instance = null;
										target_weapon_slot = -2;
								}
						} else if(target_weapon_slot >= 0 && !gun_instance){
								if(weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTY){
										target_weapon_slot = -2;
								} else {
										if(weapon_slots[target_weapon_slot].type == WeaponSlotType.GUN){
												gun_instance = weapon_slots[target_weapon_slot].obj;
												weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTYING;
												weapon_slots[target_weapon_slot].spring.target_state = 0;
												weapon_slots[target_weapon_slot].spring.state = 1;
												target_weapon_slot = -2;
										} else if(weapon_slots[target_weapon_slot].type == WeaponSlotType.MAGAZINE && mag_stage == HandMagStage.EMPTY){
												magazine_instance_in_hand = weapon_slots[target_weapon_slot].obj;
												mag_stage = HandMagStage.HOLD;
												weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTYING;
												weapon_slots[target_weapon_slot].spring.target_state = 0;
												weapon_slots[target_weapon_slot].spring.state = 1;
												target_weapon_slot = -2;
										}
								}
						}
				}
				return insert_mag_with_number_key;
		}

		function HandleGunControls(insert_mag_with_number_key : boolean) {
				var gun_script = GetGunScript();
				if(Input.GetButton("Trigger")){
						gun_script.ApplyPressureToTrigger();
				} else {
						gun_script.ReleasePressureFromTrigger();
				}
				if(Input.GetButtonDown("Slide Lock")){
						gun_script.ReleaseSlideLock();
				}
				if(Input.GetButtonUp("Slide Lock")){
						gun_script.ReleasePressureOnSlideLock();
				}
				if(Input.GetButton("Slide Lock")){
						gun_script.PressureOnSlideLock();
				}
				if(Input.GetButtonDown("Safety")){
						gun_script.ToggleSafety();			
				}	
				if(Input.GetButtonDown("Auto Mod Toggle")){
						gun_script.ToggleAutoMod();			
				}	
				if(Input.GetButtonDown("Pull Back Slide")){
						gun_script.PullBackSlide();
				}
				if(Input.GetButtonUp("Pull Back Slide")){
						gun_script.ReleaseSlide();
				}	
				if(Input.GetButtonDown("Swing Out Cylinder")){
						gun_script.SwingOutCylinder();
				}	
				if(Input.GetButtonDown("Close Cylinder")){
						gun_script.CloseCylinder();
				}	
				if(Input.GetButton("Extractor Rod")){
						gun_script.ExtractorRod();
				}
				if(Input.GetButton("Hammer")){
						gun_script.PressureOnHammer();
				}
				if(Input.GetButtonUp("Hammer")){
						gun_script.ReleaseHammer();
				}		
				if(Input.GetAxis("Mouse ScrollWheel")){
						gun_script.RotateCylinder(Input.GetAxis("Mouse ScrollWheel"));
				}		
				if(Input.GetButtonDown("Insert")){
						if(loose_bullets.length > 0){
								if(GetGunScript().AddRoundToCylinder()){
										GameObject.Destroy(loose_bullets.pop());
										loose_bullet_spring.pop();
								}
						}
				}
				if(slide_pose_spring.target_state < 0.1 && reload_pose_spring.target_state < 0.1){
						gun_tilt = GunTilt.CENTER;
				} else if(slide_pose_spring.target_state > reload_pose_spring.target_state){
						gun_tilt = GunTilt.LEFT;
				} else {
						gun_tilt = GunTilt.RIGHT;
				}

				slide_pose_spring.target_state = 0;
				reload_pose_spring.target_state = 0;
				press_check_pose_spring.target_state = 0;

				if(gun_script.IsSafetyOn()){
						reload_pose_spring.target_state = 0.2;
						slide_pose_spring.target_state = 0;
						gun_tilt = GunTilt.RIGHT;
				}

				if(gun_script.IsSlideLocked()){
						if(gun_tilt != GunTilt.LEFT){
								reload_pose_spring.target_state = 0.7;
						} else {
								slide_pose_spring.target_state = 0.7;
						}
				}
				if(gun_script.IsSlidePulledBack()){
						if(gun_tilt != GunTilt.RIGHT){
								slide_pose_spring.target_state = 1;
						} else {
								reload_pose_spring.target_state = 1;
						}
				}
				if(gun_script.IsPressCheck()){
						slide_pose_spring.target_state = 0;
						reload_pose_spring.target_state = 0;
						press_check_pose_spring.target_state = 0.6;
				}

				add_rounds_pose_spring.target_state = 0;
				eject_rounds_pose_spring.target_state = 0;
				inspect_cylinder_pose_spring.target_state = 0;
				if(gun_script.IsEjectingRounds()){
						eject_rounds_pose_spring.target_state = 1;
						//} else if(gun_script.IsAddingRounds()){
						//	add_rounds_pose_spring.target_state = 1;
				} else if(gun_script.IsCylinderOpen()){
						inspect_cylinder_pose_spring.target_state = 1;
				}

				x_recoil_spring.vel += gun_script.recoil_transfer_x;
				y_recoil_spring.vel += gun_script.recoil_transfer_y;
				rotation_x += gun_script.rotation_transfer_x;
				rotation_y += gun_script.rotation_transfer_y;
				gun_script.recoil_transfer_x = 0;
				gun_script.recoil_transfer_y = 0;
				gun_script.rotation_transfer_x = 0;
				gun_script.rotation_transfer_y = 0;
				if(gun_script.add_head_recoil){
						head_recoil_delay[next_head_recoil_delay] = 0.1;
						next_head_recoil_delay = (next_head_recoil_delay + 1)%kMaxHeadRecoil;
						gun_script.add_head_recoil = false;
				}

				if(gun_script.ready_to_remove_mag && !magazine_instance_in_hand){
						magazine_instance_in_hand = gun_script.RemoveMag();
						mag_stage = HandMagStage.HOLD;
						hold_pose_spring.state = 0;
						hold_pose_spring.vel = 0;
						hold_pose_spring.target_state = 1;
				}
				if((Input.GetButtonDown("Insert")/* && aim_spring.state > 0.5*/) || insert_mag_with_number_key){
						if(mag_stage == HandMagStage.HOLD && !gun_script.IsThereAMagInGun() || insert_mag_with_number_key){
								hold_pose_spring.target_state = 0;
								mag_stage = HandMagStage.HOLD_TO_INSERT;
						}
				}
				if(mag_stage == HandMagStage.HOLD_TO_INSERT){
						if(hold_pose_spring.state < 0.01){
								gun_script.InsertMag(magazine_instance_in_hand);
								magazine_instance_in_hand = null;
								mag_stage = HandMagStage.EMPTY;
						}
				}
		}

		function HandleControls() {
				if(Input.GetButton("Get")){
						HandleGetControl();
				}

				for(var i = 0; i < kMaxHeadRecoil; ++i){
						if(head_recoil_delay[i] != -1.0){
								head_recoil_delay[i] -= Time.deltaTime;
								if(head_recoil_delay[i] <= 0.0){
										head_recoil_spring_x.vel += Random.Range(-30.0,30.0);
										head_recoil_spring_y.vel += Random.Range(-30.0,30.0);
										head_recoil_delay[i] = -1;
								}
						}
				}

				var insert_mag_with_number_key = HandleInventoryControls();

				if(Input.GetButtonDown("Eject/Drop") || queue_drop){
						if(mag_stage == HandMagStage.HOLD){
								mag_stage = HandMagStage.EMPTY;
								magazine_instance_in_hand.AddComponent(Rigidbody);
								magazine_instance_in_hand.GetComponent.<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
								magazine_instance_in_hand.GetComponent.<Rigidbody>().velocity = character_controller.velocity;
								magazine_instance_in_hand = null;
								queue_drop = false;
						}
				}

				if(Input.GetButtonDown("Eject/Drop")){
						if(mag_stage == HandMagStage.EMPTY && gun_instance){
								if(gun_instance.GetComponent(GunScript).IsMagCurrentlyEjecting()){
										queue_drop = true;
								} else {
										gun_instance.GetComponent(GunScript).MagEject();
								}
						} else if(mag_stage == HandMagStage.HOLD_TO_INSERT){
								mag_stage = HandMagStage.HOLD;
								hold_pose_spring.target_state = 1;
						}
				}

				if(gun_instance){
						HandleGunControls(insert_mag_with_number_key);
				} else if(mag_stage == HandMagStage.HOLD){
						if(Input.GetButtonDown("Insert")){
								if(loose_bullets.length > 0){
										if(magazine_instance_in_hand.GetComponent(mag_script).AddRound()){
												GameObject.Destroy(loose_bullets.pop());
												loose_bullet_spring.pop();
										}
								}
						}
						if(Input.GetButtonDown("Pull Back Slide")){
								if(magazine_instance_in_hand.GetComponent(mag_script).RemoveRoundAnimated()){
										AddLooseBullet(true);
										PlaySoundFromGroup(sound_bullet_grab, 0.2);
								}
						}
				}

				if(Input.GetButtonDown("Aim Toggle")){
						aim_toggle = !aim_toggle;
				}
				if(Input.GetButtonDown("Slow Motion Toggle") && slomo_mode){
						if(Time.timeScale == 1.0){
								Time.timeScale = 0.1;
						} else {
								Time.timeScale = 1;
						}
				}
		}

		function StartTapePlay() {
				GetComponent.<AudioSource>().PlayOneShot(holder.sound_tape_start, 1.0 * PlayerPrefs.GetFloat("voice_volume", 1.0));
				audiosource_tape_background.Play();
				if(tape_in_progress && start_tape_delay == 0.0){ 
						audiosource_audio_content.Play();
				}
				if(!tape_in_progress && tapes_remaining.length > 0){
						audiosource_audio_content.clip = tapes_remaining[0];
						tapes_remaining.RemoveAt(0);
						//audiosource_audio_content.pitch = 10;
						//audiosource_audio_content.clip = holder.sound_scream[Random.Range(0,holder.sound_scream.length)];
						start_tape_delay = Random.Range(0.5,3.0);
						stop_tape_delay = 0;
						tape_in_progress = true;
				}
				audiosource_tape_background.pitch = 0.1;
				audiosource_audio_content.pitch = 0.1;
		}

		function StopTapePlay() {
				GetComponent.<AudioSource>().PlayOneShot(holder.sound_tape_end, 1.0 * PlayerPrefs.GetFloat("voice_volume", 1.0));
				if(tape_in_progress){
						audiosource_tape_background.Pause();
						audiosource_audio_content.Pause();
				} else {
						audiosource_tape_background.Stop();
						audiosource_audio_content.Stop();
				}
		}

		function StartWin() {
				GetComponent(MusicScript).HandleEvent(MusicEvent.WON);
				won = true;
		}

		function ApplyPose(name : String, amount : float){
				var pose = gun_instance.transform.FindChild(name);
				if(amount == 0.0 || !pose){
						return;
				}
				gun_instance.transform.position = mix(gun_instance.transform.position,
						pose.position,
						amount);
				gun_instance.transform.rotation = mix(
						gun_instance.transform.rotation,
						pose.rotation,
						amount);
		}

		function UpdateCheats() {
				if(iddqd_progress == 0 && Input.GetKeyDown('i')){
						++iddqd_progress; cheat_delay = 1;
				} else if(iddqd_progress == 1 && Input.GetKeyDown('d')){
						++iddqd_progress; cheat_delay = 1;
				} else if(iddqd_progress == 2 && Input.GetKeyDown('d')){
						++iddqd_progress; cheat_delay = 1;
				} else if(iddqd_progress == 3 && Input.GetKeyDown('q')){
						++iddqd_progress; cheat_delay = 1;
				} else if(iddqd_progress == 4 && Input.GetKeyDown('d')){
						iddqd_progress = 0;
						god_mode = !god_mode; 
						PlaySoundFromGroup(holder.sound_scream, 1.0);
				}
				if(idkfa_progress == 0 && Input.GetKeyDown('i')){
						++idkfa_progress; cheat_delay = 1;
				} else if(idkfa_progress == 1 && Input.GetKeyDown('d')){
						++idkfa_progress; cheat_delay = 1;
				} else if(idkfa_progress == 2 && Input.GetKeyDown('k')){
						++idkfa_progress; cheat_delay = 1;
				} else if(idkfa_progress == 3 && Input.GetKeyDown('f')){
						++idkfa_progress; cheat_delay = 1;
				} else if(idkfa_progress == 4 && Input.GetKeyDown('a')){
						idkfa_progress = 0;
						if(loose_bullets.length < 30){
								PlaySoundFromGroup(sound_bullet_grab, 0.2);
						}
						while(loose_bullets.length < 30){
								AddLooseBullet(true);
						}
						PlaySoundFromGroup(holder.sound_scream, 1.0);
				}
				if(slomo_progress == 0 && Input.GetKeyDown('s')){
						++slomo_progress; cheat_delay = 1;
				} else if(slomo_progress == 1 && Input.GetKeyDown('l')){
						++slomo_progress; cheat_delay = 1;
				} else if(slomo_progress == 2 && Input.GetKeyDown('o')){
						++slomo_progress; cheat_delay = 1;
				} else if(slomo_progress == 3 && Input.GetKeyDown('m')){
						++slomo_progress; cheat_delay = 1;
				} else if(slomo_progress == 4 && Input.GetKeyDown('o')){
						slomo_progress = 0;
						slomo_mode = true;
						if(Time.timeScale == 1.0){
								Time.timeScale = 0.1;
						} else {
								Time.timeScale = 1;
						}
						PlaySoundFromGroup(holder.sound_scream, 1.0);
				}
				if(cheat_delay > 0.0){
						cheat_delay -= Time.deltaTime;
						if(cheat_delay <= 0.0){
								cheat_delay = 0;
								iddqd_progress = 0;
								idkfa_progress = 0;
								slomo_progress = 0;
						}
				}
		}

		function UpdateTape() {
				if(!tape_in_progress && unplayed_tapes > 0){
						--unplayed_tapes;
						StartTapePlay();
				}
				if(Input.GetButtonDown("Tape Player") && tape_in_progress){
						if(!audiosource_tape_background.isPlaying){
								StartTapePlay();
						} else {
								StopTapePlay();
						}
				}
				if(tape_in_progress && audiosource_tape_background.isPlaying){ 
						GetComponent(MusicScript).SetMystical((tapes_heard.length+1.0)/total_tapes.length);
						audiosource_tape_background.volume = PlayerPrefs.GetFloat("voice_volume", 1.0);
						audiosource_tape_background.pitch = Mathf.Min(1.0,audiosource_audio_content.pitch + Time.deltaTime * 3.0);
						audiosource_audio_content.volume = PlayerPrefs.GetFloat("voice_volume", 1.0);
						audiosource_audio_content.pitch = Mathf.Min(1.0,audiosource_audio_content.pitch + Time.deltaTime * 3.0);
						//audiosource_audio_content.pitch = 10;
						//audiosource_audio_content.volume = 0.1;
						if(start_tape_delay > 0.0){
								if(!audiosource_audio_content.isPlaying){
										start_tape_delay = Mathf.Max(0.0, start_tape_delay - Time.deltaTime);
										if(start_tape_delay == 0.0){
												audiosource_audio_content.Play();
										}
								}
						} else if(stop_tape_delay > 0.0){
								stop_tape_delay = Mathf.Max(0.0, stop_tape_delay - Time.deltaTime);
								if(stop_tape_delay == 0.0){
										tape_in_progress = false;
										tapes_heard.push(audiosource_audio_content.clip);
										StopTapePlay();
										if(tapes_heard.length == total_tapes.length){
												StartWin();
										}
								}
						} else if(!audiosource_audio_content.isPlaying){
								stop_tape_delay = Random.Range(0.5,3.0);
						}
				}
		}

		function UpdateHealth() {
				if(dying){
						health -= Time.deltaTime;
				}
				if(health <= 0.0){
						health = 0;
						SetDead(true);
						dying = false;
				}
		}

		function UpdateHelpToggle() {
				if(Input.GetButton("Help Toggle")){
						help_hold_time += Time.deltaTime;
						if(show_help && help_hold_time >= 1.0){
								show_advanced_help = true;
						}
				}
				if(Input.GetButtonDown("Help Toggle")){
						if(!show_help){
								show_help = true;
								help_ever_shown = true;
								just_started_help = true;
						}
						help_hold_time = 0;
				}
				if(Input.GetButtonUp("Help Toggle")){
						if(show_help && help_hold_time < 1.0 && !just_started_help){
								show_help = false;
								show_advanced_help = false;
						}
						just_started_help = false;
				}
		}

		function UpdateLevelResetButton() {
				if(Input.GetButtonDown("Level Reset")){
						level_reset_hold = 0.01;
				}
				if(level_reset_hold != 0.0 && Input.GetButton("Level Reset")){
						level_reset_hold += Time.deltaTime; 
						dead_volume_fade = Mathf.Min(1.0-level_reset_hold * 0.5, dead_volume_fade);
						dead_fade = level_reset_hold * 0.5;
						if(level_reset_hold >= 2.0){
								Application.LoadLevel(Application.loadedLevel);
								level_reset_hold = 0;
						}
				} else {
						level_reset_hold = 0;
				}
		}

		function UpdateLevelEndEffects() {
				if(won){
						win_fade = Mathf.Min(1.0, win_fade + Time.deltaTime * 0.1);
						dead_volume_fade = Mathf.Max(0.0, dead_volume_fade - Time.deltaTime * 0.1);
				} else if(dead){
						dead_fade = Mathf.Min(1.0, dead_fade + Time.deltaTime * 0.3);
						dead_volume_fade = Mathf.Max(0.0, dead_volume_fade - Time.deltaTime * 0.23);
						head_fall_vel -= 9.8 * Time.deltaTime;
						head_fall += head_fall_vel * Time.deltaTime;
						head_tilt += head_tilt_vel * Time.deltaTime;
						view_rotation_x += head_tilt_x_vel * Time.deltaTime;
						view_rotation_y += head_tilt_y_vel * Time.deltaTime;
						var min_fall = character_controller.height * character_controller.transform.localScale.y * -1;
						if(head_fall < min_fall && head_fall_vel < 0.0){			
								if(Mathf.Abs(head_fall_vel) > 0.5){
										head_recoil_spring_x.vel += Random.Range(-10,10) * Mathf.Abs(head_fall_vel);
										head_recoil_spring_y.vel += Random.Range(-10,10) * Mathf.Abs(head_fall_vel);
										head_tilt_vel = 0;
										head_tilt_x_vel = 0;
										head_tilt_y_vel = 0;
										if(!dead_body_fell){
												PlaySoundFromGroup(sound_body_fall, 1.0);
												dead_body_fell = true;
										}
								}
								head_fall_vel *= -0.3;
						}
						head_fall = Mathf.Max(min_fall,head_fall);
				} else {
						dead_fade = Mathf.Max(0.0, dead_fade - Time.deltaTime * 1.5);
						dead_volume_fade = Mathf.Min(1.0, dead_volume_fade + Time.deltaTime * 1.5);
				}
		}

		function UpdateLevelChange() {
				if((dead && dead_volume_fade <= 0.0)){ 
						Application.LoadLevel(Application.loadedLevel);
				}
				if(won && dead_volume_fade <= 0.0){ 
						Application.LoadLevel("winscene");
				}
		}

		function UpdateFallOffMapDeath() {
				if(transform.position.y < -1){
						InstaKill();
				}
		}

		function UpdateAimSpring() {
				var offset_aim_target = false;
				if((Input.GetButton("Hold To Aim") || aim_toggle) && !dead && gun_instance){
						aim_spring.target_state = 1;
						var hit : RaycastHit;
						if(Physics.Linecast(main_camera.transform.position, AimPos() + AimDir() * 0.2, hit, 1 << 0)){
								aim_spring.target_state = Mathf.Clamp(
										1.0 - (Vector3.Distance(hit.point, main_camera.transform.position)/(GunDist() + 0.2)),
										0.0,
										1.0);
								offset_aim_target = true;
						}
				} else {
						aim_spring.target_state = 0;
				}
				aim_spring.Update();
				if(offset_aim_target){
						aim_spring.target_state = 1;
				}
		}

		function UpdateCameraRotationControls() {
				rotation_y_min_leeway = Mathf.Lerp(0.0,kRotationYMinLeeway,aim_spring.state);
				rotation_y_max_leeway = Mathf.Lerp(0.0,kRotationYMaxLeeway,aim_spring.state);
				rotation_x_leeway = Mathf.Lerp(0.0,kRotationXLeeway,aim_spring.state);

				if(PlayerPrefs.GetInt("lock_gun_to_center", 0)==1){
						rotation_y_min_leeway = 0;
						rotation_y_max_leeway = 0;
						rotation_x_leeway = 0;
				}

				sensitivity_x = PlayerPrefs.GetFloat("mouse_sensitivity", 1.0) * 10;
				sensitivity_y = PlayerPrefs.GetFloat("mouse_sensitivity", 1.0) * 10;
				if(PlayerPrefs.GetInt("mouse_invert", 0) == 1){
						sensitivity_y = -Mathf.Abs(sensitivity_y);
				} else {
						sensitivity_y = Mathf.Abs(sensitivity_y);
				}

				var in_menu = GameObject.Find("gui_skin_holder").GetComponent(optionsmenuscript).IsMenuShown();
				if(!dead && !in_menu){
						rotation_x += Input.GetAxis("Mouse X") * sensitivity_x;
						rotation_y += Input.GetAxis("Mouse Y") * sensitivity_y;
						rotation_y = Mathf.Clamp (rotation_y, min_angle_y, max_angle_y);

						if((Input.GetButton("Hold To Aim") || aim_toggle) && gun_instance){
								view_rotation_y = Mathf.Clamp(view_rotation_y, rotation_y - rotation_y_min_leeway, rotation_y + rotation_y_max_leeway);
								view_rotation_x = Mathf.Clamp(view_rotation_x, rotation_x - rotation_x_leeway, rotation_x + rotation_x_leeway);
						} else {
								view_rotation_x += Input.GetAxis("Mouse X") * sensitivity_x;
								view_rotation_y += Input.GetAxis("Mouse Y") * sensitivity_y;
								view_rotation_y = Mathf.Clamp (view_rotation_y, min_angle_y, max_angle_y);

								rotation_y = Mathf.Clamp(rotation_y, view_rotation_y - rotation_y_max_leeway, view_rotation_y + rotation_y_min_leeway);
								rotation_x = Mathf.Clamp(rotation_x, view_rotation_x - rotation_x_leeway, view_rotation_x + rotation_x_leeway);
						}
				}
		}

		function UpdateCameraAndPlayerTransformation() {
				main_camera.transform.localEulerAngles = Vector3(-view_rotation_y, view_rotation_x, head_tilt);
				if(!disable_recoil){
						main_camera.transform.localEulerAngles += Vector3(head_recoil_spring_y.state, head_recoil_spring_x.state, 0); 
				}
				character_controller.transform.localEulerAngles.y = view_rotation_x;
				main_camera.transform.position = transform.position;
				main_camera.transform.position.y += character_controller.height * character_controller.transform.localScale.y - 0.1;
				main_camera.transform.position.y += head_fall;
		}

		function UpdateGunTransformation() {
				var aim_dir = AimDir();
				var aim_pos = AimPos();	

				var unaimed_dir = (transform.forward + Vector3(0,-1,0)).normalized;
				var unaimed_pos = main_camera.transform.position + unaimed_dir*GunDist();

				if(disable_springs){ 
						gun_instance.transform.position = mix(unaimed_pos, aim_pos, aim_spring.target_state);
						gun_instance.transform.forward = mix(unaimed_dir, aim_dir, aim_spring.target_state);
				} else { 
						gun_instance.transform.position = mix(unaimed_pos, aim_pos, aim_spring.state);
						gun_instance.transform.forward = mix(unaimed_dir, aim_dir, aim_spring.state);
				}

				if(disable_springs) {
						ApplyPose("pose_slide_pull", slide_pose_spring.target_state);
						ApplyPose("pose_reload", reload_pose_spring.target_state);
						ApplyPose("pose_press_check", press_check_pose_spring.target_state);
						ApplyPose("pose_inspect_cylinder", inspect_cylinder_pose_spring.target_state);
						ApplyPose("pose_add_rounds", add_rounds_pose_spring.target_state);
						ApplyPose("pose_eject_rounds", eject_rounds_pose_spring.target_state); 
				} else {
						ApplyPose("pose_slide_pull", slide_pose_spring.state);
						ApplyPose("pose_reload", reload_pose_spring.state);
						ApplyPose("pose_press_check", press_check_pose_spring.state);
						ApplyPose("pose_inspect_cylinder", inspect_cylinder_pose_spring.state);
						ApplyPose("pose_add_rounds", add_rounds_pose_spring.state);
						ApplyPose("pose_eject_rounds", eject_rounds_pose_spring.state); 
				}

				if(!disable_recoil){		
						gun_instance.transform.RotateAround(
								gun_instance.transform.FindChild("point_recoil_rotate").position,
								gun_instance.transform.rotation * Vector3(1,0,0),
								x_recoil_spring.state);

						gun_instance.transform.RotateAround(
								gun_instance.transform.FindChild("point_recoil_rotate").position,
								Vector3(0,1,0),
								y_recoil_spring.state); 
				}
		}

		function UpdateFlashlightTransformation() {
				var flashlight_hold_pos = main_camera.transform.position + main_camera.transform.rotation*Vector3(-0.15,-0.01,0.15);
				var flashlight_hold_rot = main_camera.transform.rotation;

				var flashlight_pos = flashlight_hold_pos;
				var flashlight_rot = flashlight_hold_rot;

				held_flashlight.transform.position = flashlight_pos;
				held_flashlight.transform.rotation = flashlight_rot;

				held_flashlight.transform.RotateAround(
						held_flashlight.transform.FindChild("point_recoil_rotate").position,
						held_flashlight.transform.rotation * Vector3(1,0,0),
						x_recoil_spring.state * 0.3);

				held_flashlight.transform.RotateAround(
						held_flashlight.transform.FindChild("point_recoil_rotate").position,
						Vector3(0,1,0),
						y_recoil_spring.state * 0.3);

				flashlight_pos = held_flashlight.transform.position;
				flashlight_rot = held_flashlight.transform.rotation;

				if(gun_instance){
						flashlight_aim_pos = gun_instance.transform.position + gun_instance.transform.rotation*Vector3(0.07,-0.03,0.0);
						flashlight_aim_rot = gun_instance.transform.rotation;

						flashlight_aim_pos -= main_camera.transform.position;
						flashlight_aim_pos = Quaternion.Inverse(main_camera.transform.rotation) * flashlight_aim_pos;
						flashlight_aim_rot = Quaternion.Inverse(main_camera.transform.rotation) * flashlight_aim_rot;
				}

				if(disable_springs){
						flashlight_pos = mix(flashlight_pos, main_camera.transform.rotation * flashlight_aim_pos + main_camera.transform.position, aim_spring.target_state);
						flashlight_rot = mix(flashlight_rot, main_camera.transform.rotation * flashlight_aim_rot, aim_spring.target_state);
				} else {
						flashlight_pos = mix(flashlight_pos, main_camera.transform.rotation * flashlight_aim_pos + main_camera.transform.position, aim_spring.state);
						flashlight_rot = mix(flashlight_rot, main_camera.transform.rotation * flashlight_aim_rot, aim_spring.state);
				} 

				var flashlight_mouth_pos = main_camera.transform.position + main_camera.transform.rotation*Vector3(0.0,-0.08,0.05);
				var flashlight_mouth_rot = main_camera.transform.rotation;

				flashlight_mouth_spring.target_state = 0;
				if(magazine_instance_in_hand){
						flashlight_mouth_spring.target_state = 1;
				}
				flashlight_mouth_spring.target_state = Mathf.Max(flashlight_mouth_spring.target_state,
						(inspect_cylinder_pose_spring.state + eject_rounds_pose_spring.state + (press_check_pose_spring.state/0.6) + (reload_pose_spring.state/0.7) + slide_pose_spring.state) * aim_spring.state);

				flashlight_mouth_spring.Update();

				if(disable_springs){
						flashlight_pos = mix(flashlight_pos, flashlight_mouth_pos, flashlight_mouth_spring.target_state);
						flashlight_rot = mix(flashlight_rot, flashlight_mouth_rot, flashlight_mouth_spring.target_state);

						flashlight_pos = mix(flashlight_pos, flash_ground_pos, flash_ground_pose_spring.target_state);
						flashlight_rot = mix(flashlight_rot, flash_ground_rot, flash_ground_pose_spring.target_state);
				} else {
						flashlight_pos = mix(flashlight_pos, flashlight_mouth_pos, flashlight_mouth_spring.state);
						flashlight_rot = mix(flashlight_rot, flashlight_mouth_rot, flashlight_mouth_spring.state);

						flashlight_pos = mix(flashlight_pos, flash_ground_pos, flash_ground_pose_spring.state);
						flashlight_rot = mix(flashlight_rot, flash_ground_rot, flash_ground_pose_spring.state);
				}

				held_flashlight.transform.position = flashlight_pos;
				held_flashlight.transform.rotation = flashlight_rot;
		}

		function UpdateMagazineTransformation() {
				if(gun_instance){
						mag_pos = gun_instance.transform.position;
						mag_rot = gun_instance.transform.rotation;
						mag_pos += (gun_instance.transform.FindChild("point_mag_to_insert").position - 
								gun_instance.transform.FindChild("point_mag_inserted").position);
				}
				if(mag_stage == HandMagStage.HOLD || mag_stage == HandMagStage.HOLD_TO_INSERT){
						var mag_script = magazine_instance_in_hand.GetComponent(mag_script);
						var hold_pos = main_camera.transform.position + main_camera.transform.rotation*mag_script.hold_offset;
						var hold_rot = main_camera.transform.rotation * Quaternion.AngleAxis(mag_script.hold_rotation.x, Vector3(0,1,0)) * Quaternion.AngleAxis(mag_script.hold_rotation.y, Vector3(1,0,0));
						if(disable_springs){ 
								hold_pos = mix(hold_pos, mag_ground_pos, mag_ground_pose_spring.target_state);
								hold_rot = mix(hold_rot, mag_ground_rot, mag_ground_pose_spring.target_state);
						} else {
								hold_pos = mix(hold_pos, mag_ground_pos, mag_ground_pose_spring.state);
								hold_rot = mix(hold_rot, mag_ground_rot, mag_ground_pose_spring.state);
						}
						if(hold_pose_spring.state != 1.0){ 
								var amount = hold_pose_spring.state;
								if(disable_springs){ 
										amount = hold_pose_spring.target_state;
								}
								magazine_instance_in_hand.transform.position = mix(mag_pos, hold_pos, amount);
								magazine_instance_in_hand.transform.rotation = mix(mag_rot, hold_rot, amount);
						} else {
								magazine_instance_in_hand.transform.position = hold_pos;
								magazine_instance_in_hand.transform.rotation = hold_rot;
						}
				} else {
						magazine_instance_in_hand.transform.position = mag_pos;
						magazine_instance_in_hand.transform.rotation = mag_rot;
				} 
		}

		function UpdateInventoryTransformation() {
				var i = 0;
				for(i=0; i<10; ++i){
						var slot = weapon_slots[i];
						if(slot.type == WeaponSlotType.EMPTY){
								continue;
						}
						slot.obj.transform.localScale = Vector3(1.0,1.0,1.0); 
				}
				for(i=0; i<10; ++i){
						slot = weapon_slots[i];
						if(slot.type == WeaponSlotType.EMPTY){
								continue;
						}
						var start_pos = main_camera.transform.position + slot.start_pos;
						var start_rot = main_camera.transform.rotation * slot.start_rot;
						if(slot.type == WeaponSlotType.EMPTYING){
								start_pos = slot.obj.transform.position;
								start_rot = slot.obj.transform.rotation;
								if(Mathf.Abs(slot.spring.vel) <= 0.01 && slot.spring.state <= 0.01){
										slot.type = WeaponSlotType.EMPTY;
										slot.spring.state = 0;
								}
						} 
						var scale = 0;
						if(disable_springs){  
								slot.obj.transform.position = mix(
										start_pos, 
										main_camera.transform.position + main_camera.GetComponent.<Camera>().ScreenPointToRay(Vector3(main_camera.GetComponent.<Camera>().pixelWidth * (0.05 + i*0.15), main_camera.GetComponent.<Camera>().pixelHeight * 0.17,0)).direction * 0.3, 
										slot.spring.target_state);
								scale = 0.3 * slot.spring.target_state + (1.0 - slot.spring.target_state);
								slot.obj.transform.localScale.x *= scale;
								slot.obj.transform.localScale.y *= scale;
								slot.obj.transform.localScale.z *= scale; 
								slot.obj.transform.rotation = mix(
										start_rot, 
										main_camera.transform.rotation * Quaternion.AngleAxis(90, Vector3(0,1,0)), 
										slot.spring.target_state);
						} else {  
								slot.obj.transform.position = mix(
										start_pos, 
										main_camera.transform.position + main_camera.GetComponent.<Camera>().ScreenPointToRay(Vector3(main_camera.GetComponent.<Camera>().pixelWidth * (0.05 + i*0.15), main_camera.GetComponent.<Camera>().pixelHeight * 0.17,0)).direction * 0.3, 
										slot.spring.state);
								scale = 0.3 * slot.spring.state + (1.0 - slot.spring.state);
								slot.obj.transform.localScale.x *= scale;
								slot.obj.transform.localScale.y *= scale;
								slot.obj.transform.localScale.z *= scale; 
								slot.obj.transform.rotation = mix(
										start_rot, 
										main_camera.transform.rotation * Quaternion.AngleAxis(90, Vector3(0,1,0)), 
										slot.spring.state);
						}
						var renderers = slot.obj.GetComponentsInChildren(Renderer);
						for(var renderer : Renderer in renderers){
								renderer.castShadows = false; 
						}
						slot.spring.Update();
				}
		}

		function UpdateLooseBulletDisplay() {
				var revolver_open = (gun_instance && gun_instance.GetComponent(GunScript).IsCylinderOpen());
				if((mag_stage == HandMagStage.HOLD && !gun_instance) || picked_up_bullet_delay > 0.0 || revolver_open){
						show_bullet_spring.target_state = 1;
						picked_up_bullet_delay = Mathf.Max(0.0, picked_up_bullet_delay - Time.deltaTime);
				} else {	
						show_bullet_spring.target_state = 0;
				}
				show_bullet_spring.Update();

				for(var i=0; i<loose_bullets.length; ++i){
						var spring : Spring = loose_bullet_spring[i];
						spring.Update();
						var bullet : GameObject = loose_bullets[i];
						bullet.transform.position = main_camera.transform.position + main_camera.GetComponent.<Camera>().ScreenPointToRay(Vector3(0.0, main_camera.GetComponent.<Camera>().pixelHeight,0)).direction * 0.3;
						bullet.transform.position += main_camera.transform.rotation * Vector3(0.02,-0.01,0);
						bullet.transform.position += main_camera.transform.rotation * Vector3(0.006 * i,0.0,0);
						bullet.transform.position += main_camera.transform.rotation * Vector3(-0.03,0.03,0) * (1.0 - show_bullet_spring.state);
						bullet.transform.localScale.x = spring.state;
						bullet.transform.localScale.y = spring.state;
						bullet.transform.localScale.z = spring.state;
						bullet.transform.rotation = main_camera.transform.rotation * Quaternion.AngleAxis(90, Vector3(-1,0,0));
						var renderers = bullet.GetComponentsInChildren(Renderer);
						for(var renderer : Renderer in renderers){
								renderer.castShadows = false; 
						}
				}
		}

		function UpdateSprings() {	
				slide_pose_spring.Update();
				reload_pose_spring.Update();
				press_check_pose_spring.Update();
				inspect_cylinder_pose_spring.Update();
				add_rounds_pose_spring.Update();
				eject_rounds_pose_spring.Update();
				x_recoil_spring.Update();
				y_recoil_spring.Update();
				head_recoil_spring_x.Update();
				head_recoil_spring_y.Update();
				if(mag_stage == HandMagStage.HOLD || mag_stage == HandMagStage.HOLD_TO_INSERT){
						hold_pose_spring.Update();
						mag_ground_pose_spring.Update();
				}
				flash_ground_pose_spring.Update();
		}

		function UpdatePickupMagnet() {
				var attract_pos = transform.position - Vector3(0,character_controller.height * 0.2,0);
				for(var i=0; i<collected_rounds.length; ++i){
						var round = collected_rounds[i] as GameObject;
						if(!round){
								continue;
						}
						round.GetComponent.<Rigidbody>().velocity += (attract_pos - round.transform.position) * Time.deltaTime * 20;
						round.GetComponent.<Rigidbody>().velocity *= Mathf.Pow(0.1, Time.deltaTime);;
						//round.rigidbody.position += round.rigidbody.velocity * Time.deltaTime;
						if(Vector3.Distance(round.transform.position, attract_pos) < 0.5){
								if(round.gameObject.name == "cassette_tape(Clone)"){
										++unplayed_tapes;
								} else {
										AddLooseBullet(true);
										collected_rounds.splice(i,1);
										PlaySoundFromGroup(sound_bullet_grab, 0.2);
								}
								GameObject.Destroy(round);
						}
				}
				collected_rounds.remove(null);
		}

		function Update() {
				UpdateTape();
				UpdateCheats();
				UpdateFallOffMapDeath();
				UpdateHealth();
				UpdateHelpToggle();	
				UpdateLevelResetButton();
				UpdateLevelChange();
				UpdateLevelEndEffects();
				AudioListener.volume = dead_volume_fade * PlayerPrefs.GetFloat("master_volume", 1.0);
				UpdateAimSpring();
				UpdateCameraRotationControls();
				UpdateCameraAndPlayerTransformation();	
				if(gun_instance){
						UpdateGunTransformation();
				}
				if(held_flashlight){
						UpdateFlashlightTransformation();
				}				
				if(magazine_instance_in_hand){
						UpdateMagazineTransformation();
				}
				UpdateInventoryTransformation();
				UpdateLooseBulletDisplay();
				var in_menu = GameObject.Find("gui_skin_holder").GetComponent(optionsmenuscript).IsMenuShown();
				if(!dead && !in_menu){
						HandleControls();
				}
				UpdateSprings();	
				UpdatePickupMagnet();
		}

		function FixedUpdate() {
		}

		class DisplayLine {
				var str : String;
				var bold : boolean;
				function DisplayLine(_str : String, _bold : boolean){
						bold = _bold;
						str = _str;
				}
		};

		function ShouldHolsterGun() : boolean {
				if(!loose_bullets){
						return;
				}
				if(loose_bullets.length > 0){
				} else return false;
				if(magazine_instance_in_hand){
				} else return false;
				if(magazine_instance_in_hand.GetComponent(mag_script).NumRounds() == 0){
				} else return false;
				return true;
		}

		function CanLoadBulletsInMag() : boolean {
				return !gun_instance && mag_stage == HandMagStage.HOLD && loose_bullets.length > 0 && !magazine_instance_in_hand.GetComponent(mag_script).IsFull();
		}

		function CanRemoveBulletFromMag() : boolean {
				return !gun_instance && mag_stage == HandMagStage.HOLD && magazine_instance_in_hand.GetComponent(mag_script).NumRounds() > 0;
		}

		function ShouldDrawWeapon() : boolean {
				return !gun_instance && !CanLoadBulletsInMag();
		}

		function GetMostLoadedMag() : int {
				var max_rounds = 0;
				var max_rounds_slot = -1;
				for(var i=0; i<10; ++i){
						if(weapon_slots[i].type == WeaponSlotType.MAGAZINE){
								var rounds = weapon_slots[i].obj.GetComponent(mag_script).NumRounds();
								if(rounds > max_rounds){
										max_rounds_slot = i+1;
										max_rounds = rounds;
								}
						}
				}
				return max_rounds_slot;
		}

		function ShouldPutMagInInventory() : boolean {
				var rounds = magazine_instance_in_hand.GetComponent(mag_script).NumRounds();
				var most_loaded = GetMostLoadedMag();
				if(most_loaded == -1){
						return false;
				}
				if(weapon_slots[most_loaded-1].obj.GetComponent(mag_script).NumRounds() > rounds){
						return true;
				}
				return false;
		}

		function GetEmptySlot() : int {
				var empty_slot = -1;
				for(var i=0; i<10; ++i){
						if(weapon_slots[i].type == WeaponSlotType.EMPTY){
								empty_slot = i+1;
								break;
						}
				}
				return empty_slot;
		}

		function GetFlashlightSlot() : int {
				var flashlight_slot = -1;
				for(var i=0; i<10; ++i){
						if(weapon_slots[i].type == WeaponSlotType.FLASHLIGHT){
								flashlight_slot = i+1;
								break;
						}
				}
				return flashlight_slot;
		}

		function OnGUI() {
				var display_text = new Array();
				var gun_script : GunScript = null;
				if(gun_instance){
						gun_script = gun_instance.GetComponent(GunScript);
				}
				display_text.push(new DisplayLine(tapes_heard.length + " tapes absorbed out of "+total_tapes.length, true));
				if(!show_help){
						display_text.push(new DisplayLine("View help: Press [ ? ]", !help_ever_shown));
				} else {
						display_text.push(new DisplayLine("Hide help: Press [ ? ]", false));
						display_text.push(new DisplayLine("", false));
						if(tape_in_progress){
								display_text.push(new DisplayLine("Pause/Resume tape player: [ x ]", false));
						}

						display_text.push(new DisplayLine("Look: [ move mouse ]", false));
						display_text.push(new DisplayLine("Move: [ WASD ]", false));
						display_text.push(new DisplayLine("Jump: [ space ]", false));
						display_text.push(new DisplayLine("Pick up nearby: hold [ g ]", ShouldPickUpNearby()));
						if(held_flashlight){
								var empty_slot = GetEmptySlot();
								if(empty_slot != -1){
										var str = "Put flashlight in inventory: tap [ ";
										str += empty_slot;
										str += " ]";
										display_text.push(new DisplayLine(str, false));
								}
						} else {
								var flashlight_slot = GetFlashlightSlot();
								if(flashlight_slot != -1){
										str = "Equip flashlight: tap [ ";
										str += flashlight_slot;
										str += " ]";
										display_text.push(new DisplayLine(str, true));
								}
						}
						if(gun_instance){
								display_text.push(new DisplayLine("Fire weapon: tap [ left mouse button ]", false));
								var should_aim = (aim_spring.state < 0.5);			
								display_text.push(new DisplayLine("Aim weapon: hold [ right mouse button ]", should_aim));
								display_text.push(new DisplayLine("Aim weapon: tap [ q ]", should_aim));
								display_text.push(new DisplayLine("Holster weapon: tap [ ~ ]", ShouldHolsterGun()));
						} else {
								display_text.push(new DisplayLine("Draw weapon: tap [ ~ ]", ShouldDrawWeapon()));
						}
						if(gun_instance){
								if(gun_script.HasSlide()){
										display_text.push(new DisplayLine("Pull back slide: hold [ r ]", gun_script.ShouldPullSlide()?true:false));
										display_text.push(new DisplayLine("Release slide lock: tap [ t ]", gun_script.ShouldReleaseSlideLock()?true:false));
								}
								if(gun_script.HasSafety()){
										display_text.push(new DisplayLine("Toggle safety: tap [ v ]", gun_script.IsSafetyOn()?true:false));
								}
								if(gun_script.HasAutoMod()){
										display_text.push(new DisplayLine("Toggle full-automatic: tap [ v ]", gun_script.ShouldToggleAutoMod()?true:false));
								}
								if(gun_script.HasHammer()){
										display_text.push(new DisplayLine("Pull back hammer: hold [ f ]", gun_script.ShouldPullBackHammer()?true:false));
								}
								if(gun_script.gun_type == GunType.REVOLVER){
										if(!gun_script.IsCylinderOpen()){
												display_text.push(new DisplayLine("Open cylinder: tap [ e ]", (gun_script.ShouldOpenCylinder() && loose_bullets.length!=0)?true:false));
										} else {
												display_text.push(new DisplayLine("Close cylinder: tap [ r ]", (gun_script.ShouldCloseCylinder() || loose_bullets.length==0)?true:false));
												display_text.push(new DisplayLine("Extract casings: hold [ v ]", gun_script.ShouldExtractCasings()?true:false));
												display_text.push(new DisplayLine("Insert bullet: tap [ z ]", (gun_script.ShouldInsertBullet() && loose_bullets.length!=0)?true:false));
										}
										display_text.push(new DisplayLine("Spin cylinder: [ mousewheel ]", false));
								}
								if(mag_stage == HandMagStage.HOLD && !gun_script.IsThereAMagInGun()){
										var should_insert_mag = (magazine_instance_in_hand.GetComponent(mag_script).NumRounds() >= 1);
										display_text.push(new DisplayLine("Insert magazine: tap [ z ]", should_insert_mag));
								} else if(mag_stage == HandMagStage.EMPTY && gun_script.IsThereAMagInGun()){
										display_text.push(new DisplayLine("Eject magazine: tap [ e ]", gun_script.ShouldEjectMag()?true:false));
								} else if(mag_stage == HandMagStage.EMPTY && !gun_script.IsThereAMagInGun()){
										var max_rounds_slot = GetMostLoadedMag();
										if(max_rounds_slot != -1){
												display_text.push(new DisplayLine("Equip magazine: tap [ "+max_rounds_slot+" ]", true));
										}
								}
						} else {
								if(CanLoadBulletsInMag()){
										display_text.push(new DisplayLine("Insert bullet in magazine: tap [ z ]", true));
								}
								if(CanRemoveBulletFromMag()){
										display_text.push(new DisplayLine("Remove bullet from magazine: tap [ r ]", false));
								}
						}
						if(mag_stage == HandMagStage.HOLD){
								empty_slot = GetEmptySlot();
								if(empty_slot != -1){
										str = "Put magazine in inventory: tap [ ";
										str += empty_slot;
										str += " ]";
										display_text.push(new DisplayLine(str, ShouldPutMagInInventory()));
								}
								display_text.push(new DisplayLine("Drop magazine: tap [ e ]", false));
						}

						display_text.push(new DisplayLine("", false));
						if(show_advanced_help){
								display_text.push(new DisplayLine("Advanced help:", false));
								display_text.push(new DisplayLine("Toggle crouch: [ c ]", false));
								if(aim_spring.state < 0.5){
										display_text.push(new DisplayLine("Run: tap repeatedly [ w ]", false));
								}
								if(gun_instance){
										if(!gun_script.IsSafetyOn() && gun_script.IsHammerCocked()){
												display_text.push(new DisplayLine("Decock: Hold [f], hold [LMB], release [f]", ShouldPickUpNearby()));
										}
										if(!gun_script.IsSlideLocked() && !gun_script.IsSafetyOn()){
												display_text.push(new DisplayLine("Inspect chamber: hold [ t ] and then [ r ]", false));
										}
										if(mag_stage == HandMagStage.EMPTY && !gun_script.IsThereAMagInGun()){
												max_rounds_slot = GetMostLoadedMag();
												if(max_rounds_slot != -1){
														display_text.push(new DisplayLine("Quick load magazine: double tap [ "+max_rounds_slot+" ]", false));
												}
										}
								}
								display_text.push(new DisplayLine("Reset game: hold [ l ]", false));
						} else {
								display_text.push(new DisplayLine("Advanced help: Hold [ ? ]", false));
						}
				}
				var style : GUIStyle = holder.gui_skin.label;
				var width = Screen.width * 0.5;
				var offset = 0;
				for(var line : DisplayLine in display_text){
						if(line.bold){
								style.fontStyle = FontStyle.Bold;
						} else {
								style.fontStyle = FontStyle.Normal;
						}
						style.fontSize = 18;
						style.normal.textColor = Color(0,0,0);
						GUI.Label(Rect(width+0.5,offset+0.5,width+0.5,offset+20+0.5),line.str, style);
						if(line.bold){
								style.normal.textColor = Color(1,1,1);
						} else {
								style.normal.textColor = Color(0.7,0.7,0.7);
						}
						GUI.Label(Rect(width,offset,width,offset+30),line.str, style);
						offset += 20;
				}
				if(dead_fade > 0.0){
						if(!texture_death_screen){
								Debug.LogError("Assign a Texture in the inspector.");
								return;
						}
						GUI.color = Color(0,0,0,dead_fade);
						GUI.DrawTexture(Rect(0,0,Screen.width,Screen.height), texture_death_screen, ScaleMode.StretchToFill, true);
				}
				if(win_fade > 0.0){
						GUI.color = Color(1,1,1,win_fade);
						GUI.DrawTexture(Rect(0,0,Screen.width,Screen.height), texture_death_screen, ScaleMode.StretchToFill, true);
				}
		}
private Quaternion flash_ground_rot;

private FIXME_VAR_TYPE rotation_x_leeway= 0.0f;
private FIXME_VAR_TYPE rotation_y_min_leeway= 0.0f;
private FIXME_VAR_TYPE rotation_y_max_leeway= 0.0f;
private FIXME_VAR_TYPE kRotationXLeeway= 5.0f;
private FIXME_VAR_TYPE kRotationYMinLeeway= 20.0f;
private FIXME_VAR_TYPE kRotationYMaxLeeway= 10.0f;

private FIXME_VAR_TYPE rotation_x= 0.0f;
private FIXME_VAR_TYPE rotation_y= 0.0f;
private FIXME_VAR_TYPE view_rotation_x= 0.0f;
private FIXME_VAR_TYPE view_rotation_y= 0.0f;

private FIXME_VAR_TYPE kRecoilSpringStrength= 800.0f;
private FIXME_VAR_TYPE kRecoilSpringDamping= 0.000001f;
private FIXME_VAR_TYPE x_recoil_spring= new Spring(0,0,kRecoilSpringStrength,kRecoilSpringDamping);
private FIXME_VAR_TYPE y_recoil_spring= new Spring(0,0,kRecoilSpringStrength,kRecoilSpringDamping);
private FIXME_VAR_TYPE head_recoil_spring_x= new Spring(0,0,kRecoilSpringStrength,kRecoilSpringDamping);
private FIXME_VAR_TYPE head_recoil_spring_y= new Spring(0,0,kRecoilSpringStrength,kRecoilSpringDamping);

private Vector3 mag_pos;
private Quaternion mag_rot;

private GameObject magazine_instance_in_hand;
private FIXME_VAR_TYPE kGunDistance= 0.3f;

private FIXME_VAR_TYPE slide_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
private FIXME_VAR_TYPE reload_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
private FIXME_VAR_TYPE press_check_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
private FIXME_VAR_TYPE inspect_cylinder_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
private FIXME_VAR_TYPE add_rounds_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
private FIXME_VAR_TYPE eject_rounds_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);

enum GunTilt {LEFT, CENTER, RIGHT};
private GunTilt gun_tilt = GunTilt.CENTER;

private FIXME_VAR_TYPE hold_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
private FIXME_VAR_TYPE mag_ground_pose_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);

private FIXME_VAR_TYPE left_hand_occupied= false;
private FIXME_VAR_TYPE kMaxHeadRecoil= 10;
private float[] head_recoil_delay = new float[kMaxHeadRecoil];
private FIXME_VAR_TYPE next_head_recoil_delay= 0;
private Vector3 mag_ground_pos;
private Quaternion mag_ground_rot;

enum HandMagStage {HOLD, HOLD_TO_INSERT, EMPTY};
private FIXME_VAR_TYPE mag_stage= HandMagStage.EMPTY;

private FIXME_VAR_TYPE collected_rounds= new Array();

private FIXME_VAR_TYPE target_weapon_slot= -2;
private FIXME_VAR_TYPE queue_drop= false;
private Array loose_bullets;
private Array loose_bullet_spring;
private FIXME_VAR_TYPE show_bullet_spring= new Spring(0,0,kAimSpringStrength, kAimSpringDamping);
private FIXME_VAR_TYPE picked_up_bullet_delay= 0.0f;

private FIXME_VAR_TYPE head_fall= 0.0f;
private FIXME_VAR_TYPE head_fall_vel= 0.0f;
private FIXME_VAR_TYPE head_tilt= 0.0f;
private FIXME_VAR_TYPE head_tilt_vel= 0.0f;
private FIXME_VAR_TYPE head_tilt_x_vel= 0.0f;
private FIXME_VAR_TYPE head_tilt_y_vel= 0.0f;
private FIXME_VAR_TYPE dead_fade= 1.0f;
private FIXME_VAR_TYPE win_fade= 0.0f;
private FIXME_VAR_TYPE dead_volume_fade= 0.0f;
private FIXME_VAR_TYPE dead_body_fell= false;

private FIXME_VAR_TYPE start_tape_delay= 0.0f;
private FIXME_VAR_TYPE stop_tape_delay= 0.0f;
private FIXME_VAR_TYPE tapes_heard= new Array();
private FIXME_VAR_TYPE tapes_remaining= new Array();
private FIXME_VAR_TYPE total_tapes= new Array();
private FIXME_VAR_TYPE tape_in_progress= false;
private FIXME_VAR_TYPE unplayed_tapes= 0;

private FIXME_VAR_TYPE god_mode= false;
private FIXME_VAR_TYPE slomo_mode= false;
private FIXME_VAR_TYPE iddqd_progress= 0;
private FIXME_VAR_TYPE idkfa_progress= 0;
private FIXME_VAR_TYPE slomo_progress= 0;
private FIXME_VAR_TYPE cheat_delay= 0.0f;
private FIXME_VAR_TYPE level_reset_hold= 0.0f;

enum WeaponSlotType {GUN, MAGAZINE, FLASHLIGHT, EMPTY, EMPTYING};

class WeaponSlot {
GameObject obj = null;
WeaponSlotType type = WeaponSlotType.EMPTY;
Vector3 start_pos = Vector3(0,0,0);
Quaternion start_rot = Quaternion.identity;
FIXME_VAR_TYPE spring= new Spring(1,1,100,0.000001f);
};

private WeaponSlot[] weapon_slots = new WeaponSlot[10];

private FIXME_VAR_TYPE health= 1.0f;
private FIXME_VAR_TYPE dying= false;
private FIXME_VAR_TYPE dead= false;
private FIXME_VAR_TYPE won= false;

bool IsAiming (){
return (gun_instance != null && aim_spring.target_state == 1.0f);
}

bool IsDead (){
return dead;
}

void  StepRecoil ( float amount  ){
x_recoil_spring.vel += Random.Range(100,400) * amount;
y_recoil_spring.vel += Random.Range(-200,200) * amount;
}

void  WasShot (){
head_recoil_spring_x.vel += Random.Range(-400,400);
head_recoil_spring_y.vel += Random.Range(-400,400);
x_recoil_spring.vel += Random.Range(-400,400);
y_recoil_spring.vel += Random.Range(-400,400);
rotation_x += Random.Range(-4,4);
rotation_y += Random.Range(-4,4);
if(!god_mode && !won){
dying = true;
if(Random.Range(0.0f,1.0f) < 0.3f){
		SetDead(true);
}
if(dead && Random.Range(0.0f,1.0f) < 0.3f){
		dead_fade += 0.3f;
}
}
}

void  FallDeath ( Vector3 vel  ){
if(!god_mode && !won){
SetDead(true);
head_fall_vel = vel.y;
dead_fade = Mathf.Max(dead_fade, 0.5f);
head_recoil_spring_x.vel += Random.Range(-400,400);
head_recoil_spring_y.vel += Random.Range(-400,400);
}
}

void  InstaKill (){
SetDead(true);
dead_fade = 1.0f;
}

void  Shock (){
if(!god_mode && !won){
if(!dead){
		PlaySoundFromGroup(sound_electrocute, 1.0f);
}
SetDead(true);
}
head_recoil_spring_x.vel += Random.Range(-400,400);
head_recoil_spring_y.vel += Random.Range(-400,400);
}

void  SetDead ( bool new_dead  ){
if(new_dead == dead){
return;
}
dead = new_dead;
if(!dead){
head_tilt_vel = 0.0f;
head_tilt_x_vel = 0.0f;
head_tilt_y_vel = 0.0f;
head_tilt = 0.0f;
head_fall = 0.0f;
} else {
GetComponent<MusicScript>().HandleEvent(MusicEvent.DEAD);
head_tilt_vel = Random.Range(-100,100);
head_tilt_x_vel = Random.Range(-100,100);
head_tilt_y_vel = Random.Range(-100,100);
head_fall = 0.0f;
head_fall_vel = 0.0f;
dead_body_fell = false;
}
}

void  PlaySoundFromGroup ( Array group ,   float volume  ){
FIXME_VAR_TYPE which_shot= Random.Range(0,group.length);
GetComponent.<AudioSource>().PlayOneShot(group[which_shot], volume * PlayerPrefs.GetFloat("sound_volume", 1.0f));
}

void  AddLooseBullet ( bool spring  ){
loose_bullets.push(Instantiate(casing_with_bullet));
FIXME_VAR_TYPE new_spring= new Spring(0.3f,0.3f,kAimSpringStrength,kAimSpringDamping);
loose_bullet_spring.push(new_spring);
if(spring){
new_spring.vel = 3.0f;
picked_up_bullet_delay = 2.0f;
}
}

void  Start (){ 
disable_springs = false; 
disable_recoil = true;
holder = GameObject.Find("gui_skin_holder").GetComponent<GUISkinHolder>();
weapon_holder = holder.weapon.GetComponent<WeaponHolder>();
magazine_obj = weapon_holder.mag_object;
gun_obj = weapon_holder.gun_object;
casing_with_bullet = weapon_holder.bullet_object;

if(Random.Range(0.0f,1.0f) < 0.35f){
held_flashlight = Instantiate(holder.flashlight_object);
Destroy(held_flashlight.GetComponent.<Rigidbody>());
held_flashlight.GetComponent<FlashlightScript>().TurnOn();
holder.has_flashlight = true;
}

rotation_x = transform.rotation.eulerAngles.y;
view_rotation_x = transform.rotation.eulerAngles.y;
gun_instance = Instantiate(gun_obj);
FIXME_VAR_TYPE renderers= gun_instance.GetComponentsInChildren<Renderer>();
foreach(Renderer renderer in renderers){
renderer.castShadows = false; 
}
main_camera = GameObject.Find("Main Camera").gameObject;
character_controller = GetComponent<CharacterController>();
for(FIXME_VAR_TYPE i=0; i<kMaxHeadRecoil; ++i){
head_recoil_delay[i] = -1.0f;
}
for(i=0; i<10; ++i){
weapon_slots[i] = new WeaponSlot();
}
FIXME_VAR_TYPE num_start_bullets= Random.Range(0,10);
if(GetGunScript().gun_type == GunType.AUTOMATIC){
FIXME_VAR_TYPE num_start_mags= Random.Range(0,3);
for(i=1; i<num_start_mags+1; ++i){
		weapon_slots[i].type = WeaponSlotType.MAGAZINE;
		weapon_slots[i].obj = Instantiate(magazine_obj);
}
} else {
num_start_bullets += Random.Range(0,20);
}
loose_bullets = new Array();
loose_bullet_spring = new Array();
for(i=0; i<num_start_bullets; ++i){
AddLooseBullet(false);
}
audiosource_tape_background = gameObject.AddComponent<AudioSource>();
audiosource_tape_background.loop = true;
audiosource_tape_background.clip = holder.sound_tape_background;
audiosource_audio_content = gameObject.AddComponent<AudioSource>();
audiosource_audio_content.loop = false;

FIXME_VAR_TYPE count= 0;
foreach(var tape in holder.sound_tape_content){
total_tapes.push(tape);
/*++count;
if(count >= 2){
break;
}*/
}
FIXME_VAR_TYPE temp_total_tapes= new Array(total_tapes);
while(temp_total_tapes.length > 0){
FIXME_VAR_TYPE rand_tape_id= Random.Range(0,temp_total_tapes.length);
tapes_remaining.push(temp_total_tapes[rand_tape_id]);
temp_total_tapes.RemoveAt(rand_tape_id);
}
}

void  GunDist (){
return kGunDistance * (0.5f + PlayerPrefs.GetFloat("gun_distance", 1.0f)*0.5f);
}

Vector3 AimPos (){
FIXME_VAR_TYPE aim_dir= AimDir();
return main_camera.transform.position + aim_dir*GunDist();
}

Vector3 AimDir (){
FIXME_VAR_TYPE aim_rot= Quaternion();
aim_rot.SetEulerAngles(-rotation_y * Mathf.PI / 180.0f, rotation_x * Mathf.PI / 180.0f, 0.0f);
return aim_rot * Vector3(0.0f,0.0f,1.0f);
}

GunScript GetGunScript (){
return gun_instance.GetComponent<GunScript>();
}

Vector3 mix (  Vector3 a ,   Vector3 b ,   float val   ){
return a + (b-a) * val;
}

Quaternion mix (  Quaternion a ,   Quaternion b ,   float val   ){
FIXME_VAR_TYPE angle= 0.0f;
FIXME_VAR_TYPE axis= Vector3();
(Quaternion.Inverse(b)*a).ToAngleAxis(angle, axis);
if(angle > 180){
angle -= 360;
}
if(angle < -180){
angle += 360;
}
if(angle == 0){
return a;
}
return a * Quaternion.AngleAxis(angle * -val, axis);
}

bool ShouldPickUpNearby (){
FIXME_VAR_TYPE nearest_mag= null;
FIXME_VAR_TYPE nearest_mag_dist= 0.0f;
FIXME_VAR_TYPE colliders= Physics.OverlapSphere(main_camera.transform.position, 2.0f, 1 << 8);
foreach(var collider in colliders){
if(magazine_obj && collider.gameObject.name == magazine_obj.name+"(Clone)" && collider.gameObject.GetComponent.<Rigidbody>()){
		if(mag_stage == HandMagStage.EMPTY){
				return true;
		}	
} else if((collider.gameObject.name == casing_with_bullet.name || collider.gameObject.name == casing_with_bullet.name+"(Clone)") && collider.gameObject.GetComponent.<Rigidbody>()){
		return true;
}
}
return false;
}

void  HandleGetControl (){
FIXME_VAR_TYPE nearest_mag= null;
FIXME_VAR_TYPE nearest_mag_dist= 0.0f;
FIXME_VAR_TYPE colliders= Physics.OverlapSphere(main_camera.transform.position, 2.0f, 1 << 8);
foreach(var collider in colliders){
if(magazine_obj && collider.gameObject.name == magazine_obj.name+"(Clone)" && collider.gameObject.GetComponent.<Rigidbody>()){
		FIXME_VAR_TYPE dist= Vector3.Distance(collider.transform.position, main_camera.transform.position);
		if(!nearest_mag || dist < nearest_mag_dist){	
				nearest_mag_dist = dist;
				nearest_mag = collider.gameObject;
		}					
} else if((collider.gameObject.name == casing_with_bullet.name || collider.gameObject.name == casing_with_bullet.name+"(Clone)") && collider.gameObject.GetComponent.<Rigidbody>()){
		collected_rounds.push(collider.gameObject);			
		collider.gameObject.GetComponent.<Rigidbody>().useGravity = false;
		collider.gameObject.GetComponent.<Rigidbody>().WakeUp();
		collider.enabled = false;
} else if(collider.gameObject.name == "cassette_tape(Clone)" && collider.gameObject.GetComponent.<Rigidbody>()){
		collected_rounds.push(collider.gameObject);			
		collider.gameObject.GetComponent.<Rigidbody>().useGravity = false;
		collider.gameObject.GetComponent.<Rigidbody>().WakeUp();
		collider.enabled = false;
} else if(collider.gameObject.name == "flashlight_object(Clone)" && collider.gameObject.GetComponent.<Rigidbody>() && !held_flashlight){
		held_flashlight = collider.gameObject;
		Destroy(held_flashlight.GetComponent.<Rigidbody>());
		held_flashlight.GetComponent<FlashlightScript>().TurnOn();
		holder.has_flashlight = true;
		flash_ground_pos = held_flashlight.transform.position;
		flash_ground_rot = held_flashlight.transform.rotation;
		flash_ground_pose_spring.state = 1.0f;
		flash_ground_pose_spring.vel = 1.0f;
}
}
if(nearest_mag && mag_stage == HandMagStage.EMPTY){
magazine_instance_in_hand = nearest_mag;
Destroy(magazine_instance_in_hand.GetComponent.<Rigidbody>());
mag_ground_pos = magazine_instance_in_hand.transform.position;
mag_ground_rot = magazine_instance_in_hand.transform.rotation;
mag_ground_pose_spring.state = 1.0f;
mag_ground_pose_spring.vel = 1.0f;
hold_pose_spring.state = 1.0f;
hold_pose_spring.vel = 0.0f;
hold_pose_spring.target_state = 1.0f;
mag_stage = HandMagStage.HOLD;
}
}

bool HandleInventoryControls (){	
if(Input.GetButtonDown("Holster")){
target_weapon_slot = -1;
}
if(Input.GetButtonDown("Inventory 1")){
target_weapon_slot = 0;
}
if(Input.GetButtonDown("Inventory 2")){
target_weapon_slot = 1;
}
if(Input.GetButtonDown("Inventory 3")){
target_weapon_slot = 2;
}
if(Input.GetButtonDown("Inventory 4")){
target_weapon_slot = 3;
}
if(Input.GetButtonDown("Inventory 5")){
target_weapon_slot = 4;
}
if(Input.GetButtonDown("Inventory 6")){
target_weapon_slot = 5;
}
if(Input.GetButtonDown("Inventory 7")){
target_weapon_slot = 6;
}
if(Input.GetButtonDown("Inventory 8")){
target_weapon_slot = 7;
}
if(Input.GetButtonDown("Inventory 9")){
target_weapon_slot = 8;
}
if(Input.GetButtonDown("Inventory 10")){
target_weapon_slot = 9;
}

FIXME_VAR_TYPE mag_ejecting= false;
if(gun_instance && (gun_instance.GetComponent<GunScript>().IsMagCurrentlyEjecting() || gun_instance.GetComponent<GunScript>().ready_to_remove_mag)){
mag_ejecting = true;
}

FIXME_VAR_TYPE insert_mag_with_number_key= false;

if(target_weapon_slot != -2 && !mag_ejecting && (mag_stage == HandMagStage.EMPTY || mag_stage == HandMagStage.HOLD)){
if(target_weapon_slot == -1 && !gun_instance){
		for(FIXME_VAR_TYPE i=0; i<10; ++i){
				if(weapon_slots[i].type == WeaponSlotType.GUN){
						target_weapon_slot = i;
						break;
				}
		}
}
if(mag_stage == HandMagStage.HOLD && target_weapon_slot != -1 && weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTY){
		// Put held mag in empty slot
		for(i=0; i<10; ++i){
				if(weapon_slots[target_weapon_slot].type != WeaponSlotType.EMPTY && weapon_slots[target_weapon_slot].obj == magazine_instance_in_hand){
						weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTY;
				}
		}
		weapon_slots[target_weapon_slot].type = WeaponSlotType.MAGAZINE;
		weapon_slots[target_weapon_slot].obj = magazine_instance_in_hand;
		weapon_slots[target_weapon_slot].spring.state = 0.0f;
		weapon_slots[target_weapon_slot].spring.target_state = 1.0f;
		weapon_slots[target_weapon_slot].start_pos = magazine_instance_in_hand.transform.position - main_camera.transform.position;
		weapon_slots[target_weapon_slot].start_rot = Quaternion.Inverse(main_camera.transform.rotation) * magazine_instance_in_hand.transform.rotation;
		magazine_instance_in_hand = null;
		mag_stage = HandMagStage.EMPTY;
		target_weapon_slot = -2;
} else if(mag_stage == HandMagStage.HOLD && target_weapon_slot != -1 && weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTYING && weapon_slots[target_weapon_slot].obj == magazine_instance_in_hand && gun_instance && !gun_instance.GetComponent<GunScript>().IsThereAMagInGun()){
		insert_mag_with_number_key = true;
		target_weapon_slot = -2;
} else if (target_weapon_slot != -1 && mag_stage == HandMagStage.EMPTY && weapon_slots[target_weapon_slot].type == WeaponSlotType.MAGAZINE){
		// Take mag from inventory
		magazine_instance_in_hand = weapon_slots[target_weapon_slot].obj;
		mag_stage = HandMagStage.HOLD;
		hold_pose_spring.state = 1.0f;
		hold_pose_spring.target_state = 1.0f;
		weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTYING;
		weapon_slots[target_weapon_slot].spring.target_state = 0.0f;
		weapon_slots[target_weapon_slot].spring.state = 1.0f;
		target_weapon_slot = -2;
} else if (target_weapon_slot != -1 && mag_stage == HandMagStage.EMPTY && weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTY && held_flashlight){
		// Put flashlight away
		held_flashlight.GetComponent<FlashlightScript>().TurnOff();
		weapon_slots[target_weapon_slot].type = WeaponSlotType.FLASHLIGHT;
		weapon_slots[target_weapon_slot].obj = held_flashlight;
		weapon_slots[target_weapon_slot].spring.state = 0.0f;
		weapon_slots[target_weapon_slot].spring.target_state = 1.0f;
		weapon_slots[target_weapon_slot].start_pos = held_flashlight.transform.position - main_camera.transform.position;
		weapon_slots[target_weapon_slot].start_rot = Quaternion.Inverse(main_camera.transform.rotation) * held_flashlight.transform.rotation;
		held_flashlight = null;
		target_weapon_slot = -2;
}  else if (target_weapon_slot != -1 && !held_flashlight && weapon_slots[target_weapon_slot].type == WeaponSlotType.FLASHLIGHT){
		// Take flashlight from inventory
		held_flashlight = weapon_slots[target_weapon_slot].obj;
		held_flashlight.GetComponent<FlashlightScript>().TurnOn();
		weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTYING;
		weapon_slots[target_weapon_slot].spring.target_state = 0.0f;
		weapon_slots[target_weapon_slot].spring.state = 1.0f;
		target_weapon_slot = -2;
} else if(gun_instance && target_weapon_slot == -1){
		// Put gun away
		if(target_weapon_slot == -1){
				for(i=0; i<10; ++i){
						if(weapon_slots[i].type == WeaponSlotType.EMPTY){
								target_weapon_slot = i;
								break;
						}
				}
		}
		if(target_weapon_slot != -1 && weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTY){
				for(i=0; i<10; ++i){
						if(weapon_slots[target_weapon_slot].type != WeaponSlotType.EMPTY && weapon_slots[target_weapon_slot].obj == gun_instance){
								weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTY;
						}
				}
				weapon_slots[target_weapon_slot].type = WeaponSlotType.GUN;
				weapon_slots[target_weapon_slot].obj = gun_instance;
				weapon_slots[target_weapon_slot].spring.state = 0.0f;
				weapon_slots[target_weapon_slot].spring.target_state = 1.0f;
				weapon_slots[target_weapon_slot].start_pos = gun_instance.transform.position - main_camera.transform.position;
				weapon_slots[target_weapon_slot].start_rot = Quaternion.Inverse(main_camera.transform.rotation) * gun_instance.transform.rotation;
				gun_instance = null;
				target_weapon_slot = -2;
		}
} else if(target_weapon_slot >= 0 && !gun_instance){
		if(weapon_slots[target_weapon_slot].type == WeaponSlotType.EMPTY){
				target_weapon_slot = -2;
		} else {
				if(weapon_slots[target_weapon_slot].type == WeaponSlotType.GUN){
						gun_instance = weapon_slots[target_weapon_slot].obj;
						weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTYING;
						weapon_slots[target_weapon_slot].spring.target_state = 0.0f;
						weapon_slots[target_weapon_slot].spring.state = 1.0f;
						target_weapon_slot = -2;
				} else if(weapon_slots[target_weapon_slot].type == WeaponSlotType.MAGAZINE && mag_stage == HandMagStage.EMPTY){
						magazine_instance_in_hand = weapon_slots[target_weapon_slot].obj;
						mag_stage = HandMagStage.HOLD;
						weapon_slots[target_weapon_slot].type = WeaponSlotType.EMPTYING;
						weapon_slots[target_weapon_slot].spring.target_state = 0.0f;
						weapon_slots[target_weapon_slot].spring.state = 1.0f;
						target_weapon_slot = -2;
				}
		}
}
}
return insert_mag_with_number_key;
}

void  HandleGunControls ( bool insert_mag_with_number_key  ){
FIXME_VAR_TYPE gun_script= GetGunScript();
if(Input.GetButton("Trigger")){
gun_script.ApplyPressureToTrigger();
} else {
gun_script.ReleasePressureFromTrigger();
}
if(Input.GetButtonDown("Slide Lock")){
gun_script.ReleaseSlideLock();
}
if(Input.GetButtonUp("Slide Lock")){
gun_script.ReleasePressureOnSlideLock();
}
if(Input.GetButton("Slide Lock")){
gun_script.PressureOnSlideLock();
}
if(Input.GetButtonDown("Safety")){
gun_script.ToggleSafety();			
}	
if(Input.GetButtonDown("Auto Mod Toggle")){
gun_script.ToggleAutoMod();			
}	
if(Input.GetButtonDown("Pull Back Slide")){
gun_script.PullBackSlide();
}
if(Input.GetButtonUp("Pull Back Slide")){
gun_script.ReleaseSlide();
}	
if(Input.GetButtonDown("Swing Out Cylinder")){
gun_script.SwingOutCylinder();
}	
if(Input.GetButtonDown("Close Cylinder")){
gun_script.CloseCylinder();
}	
if(Input.GetButton("Extractor Rod")){
gun_script.ExtractorRod();
}
if(Input.GetButton("Hammer")){
gun_script.PressureOnHammer();
}
if(Input.GetButtonUp("Hammer")){
gun_script.ReleaseHammer();
}		
if(Input.GetAxis("Mouse ScrollWheel")){
gun_script.RotateCylinder(Input.GetAxis("Mouse ScrollWheel"));
}		
if(Input.GetButtonDown("Insert")){
if(loose_bullets.length > 0){
		if(GetGunScript().AddRoundToCylinder()){
				GameObject.Destroy(loose_bullets.pop());
				loose_bullet_spring.pop();
		}
}
}
if(slide_pose_spring.target_state < 0.1f && reload_pose_spring.target_state < 0.1f){
gun_tilt = GunTilt.CENTER;
} else if(slide_pose_spring.target_state > reload_pose_spring.target_state){
gun_tilt = GunTilt.LEFT;
} else {
gun_tilt = GunTilt.RIGHT;
}

slide_pose_spring.target_state = 0.0f;
reload_pose_spring.target_state = 0.0f;
press_check_pose_spring.target_state = 0.0f;

if(gun_script.IsSafetyOn()){
reload_pose_spring.target_state = 0.2f;
slide_pose_spring.target_state = 0.0f;
gun_tilt = GunTilt.RIGHT;
}

if(gun_script.IsSlideLocked()){
if(gun_tilt != GunTilt.LEFT){
		reload_pose_spring.target_state = 0.7f;
} else {
		slide_pose_spring.target_state = 0.7f;
}
}
if(gun_script.IsSlidePulledBack()){
if(gun_tilt != GunTilt.RIGHT){
		slide_pose_spring.target_state = 1.0f;
} else {
		reload_pose_spring.target_state = 1.0f;
}
}
if(gun_script.IsPressCheck()){
slide_pose_spring.target_state = 0.0f;
reload_pose_spring.target_state = 0.0f;
press_check_pose_spring.target_state = 0.6f;
}

add_rounds_pose_spring.target_state = 0.0f;
eject_rounds_pose_spring.target_state = 0.0f;
inspect_cylinder_pose_spring.target_state = 0.0f;
if(gun_script.IsEjectingRounds()){
eject_rounds_pose_spring.target_state = 1.0f;
//} else if(gun_script.IsAddingRounds()){
//	add_rounds_pose_spring.target_state = 1.0f;
} else if(gun_script.IsCylinderOpen()){
inspect_cylinder_pose_spring.target_state = 1.0f;
}

x_recoil_spring.vel += gun_script.recoil_transfer_x;
y_recoil_spring.vel += gun_script.recoil_transfer_y;
rotation_x += gun_script.rotation_transfer_x;
rotation_y += gun_script.rotation_transfer_y;
gun_script.recoil_transfer_x = 0.0f;
gun_script.recoil_transfer_y = 0.0f;
gun_script.rotation_transfer_x = 0.0f;
gun_script.rotation_transfer_y = 0.0f;
if(gun_script.add_head_recoil){
head_recoil_delay[next_head_recoil_delay] = 0.1f;
next_head_recoil_delay = (next_head_recoil_delay + 1)%kMaxHeadRecoil;
gun_script.add_head_recoil = false;
}

if(gun_script.ready_to_remove_mag && !magazine_instance_in_hand){
magazine_instance_in_hand = gun_script.RemoveMag();
mag_stage = HandMagStage.HOLD;
hold_pose_spring.state = 0.0f;
hold_pose_spring.vel = 0.0f;
hold_pose_spring.target_state = 1.0f;
}
if((Input.GetButtonDown("Insert")/* && aim_spring.state > 0.5f*/) || insert_mag_with_number_key){
if(mag_stage == HandMagStage.HOLD && !gun_script.IsThereAMagInGun() || insert_mag_with_number_key){
		hold_pose_spring.target_state = 0.0f;
		mag_stage = HandMagStage.HOLD_TO_INSERT;
}
}
if(mag_stage == HandMagStage.HOLD_TO_INSERT){
if(hold_pose_spring.state < 0.01f){
		gun_script.InsertMag(magazine_instance_in_hand);
		magazine_instance_in_hand = null;
		mag_stage = HandMagStage.EMPTY;
}
}
}

void  HandleControls (){
if(Input.GetButton("Get")){
HandleGetControl();
}

for(FIXME_VAR_TYPE i= 0; i < kMaxHeadRecoil; ++i){
if(head_recoil_delay[i] != -1.0f){
		head_recoil_delay[i] -= Time.deltaTime;
		if(head_recoil_delay[i] <= 0.0f){
				head_recoil_spring_x.vel += Random.Range(-30.0f,30.0f);
				head_recoil_spring_y.vel += Random.Range(-30.0f,30.0f);
				head_recoil_delay[i] = -1.0f;
		}
}
}

FIXME_VAR_TYPE insert_mag_with_number_key= HandleInventoryControls();

if(Input.GetButtonDown("Eject/Drop") || queue_drop){
if(mag_stage == HandMagStage.HOLD){
		mag_stage = HandMagStage.EMPTY;
		magazine_instance_in_hand.AddComponent<Rigidbody>();
		magazine_instance_in_hand.GetComponent.<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		magazine_instance_in_hand.GetComponent.<Rigidbody>().velocity = character_controller.velocity;
		magazine_instance_in_hand = null;
		queue_drop = false;
}
}

if(Input.GetButtonDown("Eject/Drop")){
if(mag_stage == HandMagStage.EMPTY && gun_instance){
		if(gun_instance.GetComponent<GunScript>().IsMagCurrentlyEjecting()){
				queue_drop = true;
		} else {
				gun_instance.GetComponent<GunScript>().MagEject();
		}
} else if(mag_stage == HandMagStage.HOLD_TO_INSERT){
		mag_stage = HandMagStage.HOLD;
		hold_pose_spring.target_state = 1.0f;
}
}

if(gun_instance){
HandleGunControls(insert_mag_with_number_key);
} else if(mag_stage == HandMagStage.HOLD){
if(Input.GetButtonDown("Insert")){
		if(loose_bullets.length > 0){
				if(magazine_instance_in_hand.GetComponent<mag_script>().AddRound()){
						GameObject.Destroy(loose_bullets.pop());
						loose_bullet_spring.pop();
				}
		}
}
if(Input.GetButtonDown("Pull Back Slide")){
		if(magazine_instance_in_hand.GetComponent<mag_script>().RemoveRoundAnimated()){
				AddLooseBullet(true);
				PlaySoundFromGroup(sound_bullet_grab, 0.2f);
		}
}
}

if(Input.GetButtonDown("Aim Toggle")){
aim_toggle = !aim_toggle;
}
if(Input.GetButtonDown("Slow Motion Toggle") && slomo_mode){
if(Time.timeScale == 1.0f){
		Time.timeScale = 0.1f;
} else {
		Time.timeScale = 1.0f;
}
}
}

void  StartTapePlay (){
GetComponent.<AudioSource>().PlayOneShot(holder.sound_tape_start, 1.0f * PlayerPrefs.GetFloat("voice_volume", 1.0f));
audiosource_tape_background.Play();
if(tape_in_progress && start_tape_delay == 0.0f){ 
audiosource_audio_content.Play();
}
if(!tape_in_progress && tapes_remaining.length > 0){
audiosource_audio_content.clip = tapes_remaining[0];
tapes_remaining.RemoveAt(0);
//audiosource_audio_content.pitch = 10.0f;
//audiosource_audio_content.clip = holder.sound_scream[Random.Range(0,holder.sound_scream.length)];
start_tape_delay = Random.Range(0.5f,3.0f);
stop_tape_delay = 0.0f;
tape_in_progress = true;
}
audiosource_tape_background.pitch = 0.1f;
audiosource_audio_content.pitch = 0.1f;
}

void  StopTapePlay (){
GetComponent.<AudioSource>().PlayOneShot(holder.sound_tape_end, 1.0f * PlayerPrefs.GetFloat("voice_volume", 1.0f));
if(tape_in_progress){
audiosource_tape_background.Pause();
audiosource_audio_content.Pause();
} else {
audiosource_tape_background.Stop();
audiosource_audio_content.Stop();
}
}

void  StartWin (){
GetComponent<MusicScript>().HandleEvent(MusicEvent.WON);
won = true;
}

void  ApplyPose ( string name ,   float amount  ){
FIXME_VAR_TYPE pose= gun_instance.transform.FindChild(name);
if(amount == 0.0f || !pose){
return;
}
gun_instance.transform.position = mix(gun_instance.transform.position,
pose.position,
amount);
gun_instance.transform.rotation = mix(
gun_instance.transform.rotation,
pose.rotation,
amount);
}

void  UpdateCheats (){
if(iddqd_progress == 0 && Input.GetKeyDown('i')){
++iddqd_progress; cheat_delay = 1.0f;
} else if(iddqd_progress == 1 && Input.GetKeyDown('d')){
++iddqd_progress; cheat_delay = 1.0f;
} else if(iddqd_progress == 2 && Input.GetKeyDown('d')){
++iddqd_progress; cheat_delay = 1.0f;
} else if(iddqd_progress == 3 && Input.GetKeyDown('q')){
++iddqd_progress; cheat_delay = 1.0f;
} else if(iddqd_progress == 4 && Input.GetKeyDown('d')){
iddqd_progress = 0;
god_mode = !god_mode; 
PlaySoundFromGroup(holder.sound_scream, 1.0f);
}
if(idkfa_progress == 0 && Input.GetKeyDown('i')){
++idkfa_progress; cheat_delay = 1.0f;
} else if(idkfa_progress == 1 && Input.GetKeyDown('d')){
++idkfa_progress; cheat_delay = 1.0f;
} else if(idkfa_progress == 2 && Input.GetKeyDown('k')){
++idkfa_progress; cheat_delay = 1.0f;
} else if(idkfa_progress == 3 && Input.GetKeyDown('f')){
++idkfa_progress; cheat_delay = 1.0f;
} else if(idkfa_progress == 4 && Input.GetKeyDown('a')){
idkfa_progress = 0;
if(loose_bullets.length < 30){
		PlaySoundFromGroup(sound_bullet_grab, 0.2f);
}
while(loose_bullets.length < 30){
		AddLooseBullet(true);
}
PlaySoundFromGroup(holder.sound_scream, 1.0f);
}
if(slomo_progress == 0 && Input.GetKeyDown('s')){
++slomo_progress; cheat_delay = 1.0f;
} else if(slomo_progress == 1 && Input.GetKeyDown('l')){
++slomo_progress; cheat_delay = 1.0f;
} else if(slomo_progress == 2 && Input.GetKeyDown('o')){
++slomo_progress; cheat_delay = 1.0f;
} else if(slomo_progress == 3 && Input.GetKeyDown('m')){
++slomo_progress; cheat_delay = 1.0f;
} else if(slomo_progress == 4 && Input.GetKeyDown('o')){
slomo_progress = 0;
slomo_mode = true;
if(Time.timeScale == 1.0f){
		Time.timeScale = 0.1f;
} else {
		Time.timeScale = 1.0f;
}
PlaySoundFromGroup(holder.sound_scream, 1.0f);
}
if(cheat_delay > 0.0f){
cheat_delay -= Time.deltaTime;
if(cheat_delay <= 0.0f){
		cheat_delay = 0.0f;
		iddqd_progress = 0;
		idkfa_progress = 0;
		slomo_progress = 0;
}
}
}

void  UpdateTape (){
if(!tape_in_progress && unplayed_tapes > 0){
--unplayed_tapes;
StartTapePlay();
}
if(Input.GetButtonDown("Tape Player") && tape_in_progress){
if(!audiosource_tape_background.isPlaying){
		StartTapePlay();
} else {
		StopTapePlay();
}
}
if(tape_in_progress && audiosource_tape_background.isPlaying){ 
GetComponent<MusicScript>().SetMystical((tapes_heard.length+1.0f)/total_tapes.length);
audiosource_tape_background.volume = PlayerPrefs.GetFloat("voice_volume", 1.0f);
audiosource_tape_background.pitch = Mathf.Min(1.0f,audiosource_audio_content.pitch + Time.deltaTime * 3.0f);
audiosource_audio_content.volume = PlayerPrefs.GetFloat("voice_volume", 1.0f);
audiosource_audio_content.pitch = Mathf.Min(1.0f,audiosource_audio_content.pitch + Time.deltaTime * 3.0f);
//audiosource_audio_content.pitch = 10.0f;
//audiosource_audio_content.volume = 0.1f;
if(start_tape_delay > 0.0f){
		if(!audiosource_audio_content.isPlaying){
				start_tape_delay = Mathf.Max(0.0f, start_tape_delay - Time.deltaTime);
				if(start_tape_delay == 0.0f){
						audiosource_audio_content.Play();
				}
		}
} else if(stop_tape_delay > 0.0f){
		stop_tape_delay = Mathf.Max(0.0f, stop_tape_delay - Time.deltaTime);
		if(stop_tape_delay == 0.0f){
				tape_in_progress = false;
				tapes_heard.push(audiosource_audio_content.clip);
				StopTapePlay();
				if(tapes_heard.length == total_tapes.length){
						StartWin();
				}
		}
} else if(!audiosource_audio_content.isPlaying){
		stop_tape_delay = Random.Range(0.5f,3.0f);
}
}
}

void  UpdateHealth (){
if(dying){
health -= Time.deltaTime;
}
if(health <= 0.0f){
health = 0.0f;
SetDead(true);
dying = false;
}
}

void  UpdateHelpToggle (){
if(Input.GetButton("Help Toggle")){
help_hold_time += Time.deltaTime;
if(show_help && help_hold_time >= 1.0f){
		show_advanced_help = true;
}
}
if(Input.GetButtonDown("Help Toggle")){
if(!show_help){
		show_help = true;
		help_ever_shown = true;
		just_started_help = true;
}
help_hold_time = 0.0f;
}
if(Input.GetButtonUp("Help Toggle")){
if(show_help && help_hold_time < 1.0f && !just_started_help){
		show_help = false;
		show_advanced_help = false;
}
just_started_help = false;
}
}

void  UpdateLevelResetButton (){
if(Input.GetButtonDown("Level Reset")){
level_reset_hold = 0.01f;
}
if(level_reset_hold != 0.0f && Input.GetButton("Level Reset")){
level_reset_hold += Time.deltaTime; 
dead_volume_fade = Mathf.Min(1.0f-level_reset_hold * 0.5f, dead_volume_fade);
dead_fade = level_reset_hold * 0.5f;
if(level_reset_hold >= 2.0f){
		Application.LoadLevel(Application.loadedLevel);
		level_reset_hold = 0.0f;
}
} else {
level_reset_hold = 0.0f;
}
}

void  UpdateLevelEndEffects (){
if(won){
win_fade = Mathf.Min(1.0f, win_fade + Time.deltaTime * 0.1f);
dead_volume_fade = Mathf.Max(0.0f, dead_volume_fade - Time.deltaTime * 0.1f);
} else if(dead){
dead_fade = Mathf.Min(1.0f, dead_fade + Time.deltaTime * 0.3f);
dead_volume_fade = Mathf.Max(0.0f, dead_volume_fade - Time.deltaTime * 0.23f);
head_fall_vel -= 9.8f * Time.deltaTime;
head_fall += head_fall_vel * Time.deltaTime;
head_tilt += head_tilt_vel * Time.deltaTime;
view_rotation_x += head_tilt_x_vel * Time.deltaTime;
view_rotation_y += head_tilt_y_vel * Time.deltaTime;
FIXME_VAR_TYPE min_fall= character_controller.height * character_controller.transform.localScale.y * -1.0f;
if(head_fall < min_fall && head_fall_vel < 0.0f){			
		if(Mathf.Abs(head_fall_vel) > 0.5f){
				head_recoil_spring_x.vel += Random.Range(-10,10) * Mathf.Abs(head_fall_vel);
				head_recoil_spring_y.vel += Random.Range(-10,10) * Mathf.Abs(head_fall_vel);
				head_tilt_vel = 0.0f;
				head_tilt_x_vel = 0.0f;
				head_tilt_y_vel = 0.0f;
				if(!dead_body_fell){
						PlaySoundFromGroup(sound_body_fall, 1.0f);
						dead_body_fell = true;
				}
		}
		head_fall_vel *= -0.3f;
}
head_fall = Mathf.Max(min_fall,head_fall);
} else {
dead_fade = Mathf.Max(0.0f, dead_fade - Time.deltaTime * 1.5f);
dead_volume_fade = Mathf.Min(1.0f, dead_volume_fade + Time.deltaTime * 1.5f);
}
}

void  UpdateLevelChange (){
if((dead && dead_volume_fade <= 0.0f)){ 
Application.LoadLevel(Application.loadedLevel);
}
if(won && dead_volume_fade <= 0.0f){ 
Application.LoadLevel("winscene");
}
}

void  UpdateFallOffMapDeath (){
if(transform.position.y < -1){
InstaKill();
}
}

void  UpdateAimSpring (){
FIXME_VAR_TYPE offset_aim_target= false;
if((Input.GetButton("Hold To Aim") || aim_toggle) && !dead && gun_instance){
aim_spring.target_state = 1.0f;
RaycastHit hit;
if(Physics.Linecast(main_camera.transform.position, AimPos() + AimDir() * 0.2f, hit, 1 << 0)){
		aim_spring.target_state = Mathf.Clamp(
				1.0f - (Vector3.Distance(hit.point, main_camera.transform.position)/(GunDist() + 0.2f)),
				0.0f,
				1.0f);
		offset_aim_target = true;
}
} else {
aim_spring.target_state = 0.0f;
}
aim_spring.Update();
if(offset_aim_target){
aim_spring.target_state = 1.0f;
}
}

void  UpdateCameraRotationControls (){
rotation_y_min_leeway = Mathf.Lerp(0.0f,kRotationYMinLeeway,aim_spring.state);
rotation_y_max_leeway = Mathf.Lerp(0.0f,kRotationYMaxLeeway,aim_spring.state);
rotation_x_leeway = Mathf.Lerp(0.0f,kRotationXLeeway,aim_spring.state);

if(PlayerPrefs.GetInt("lock_gun_to_center", 0)==1){
rotation_y_min_leeway = 0;
rotation_y_max_leeway = 0;
rotation_x_leeway = 0;
}

sensitivity_x = PlayerPrefs.GetFloat("mouse_sensitivity", 1.0f) * 10.0f;
sensitivity_y = PlayerPrefs.GetFloat("mouse_sensitivity", 1.0f) * 10.0f;
if(PlayerPrefs.GetInt("mouse_invert", 0) == 1){
sensitivity_y = -Mathf.Abs(sensitivity_y);
} else {
sensitivity_y = Mathf.Abs(sensitivity_y);
}

FIXME_VAR_TYPE in_menu= GameObject.Find("gui_skin_holder").GetComponent<optionsmenuscript>().IsMenuShown();
if(!dead && !in_menu){
rotation_x += Input.GetAxis("Mouse X") * sensitivity_x;
rotation_y += Input.GetAxis("Mouse Y") * sensitivity_y;
rotation_y = Mathf.Clamp (rotation_y, min_angle_y, max_angle_y);

if((Input.GetButton("Hold To Aim") || aim_toggle) && gun_instance){
		view_rotation_y = Mathf.Clamp(view_rotation_y, rotation_y - rotation_y_min_leeway, rotation_y + rotation_y_max_leeway);
		view_rotation_x = Mathf.Clamp(view_rotation_x, rotation_x - rotation_x_leeway, rotation_x + rotation_x_leeway);
} else {
		view_rotation_x += Input.GetAxis("Mouse X") * sensitivity_x;
		view_rotation_y += Input.GetAxis("Mouse Y") * sensitivity_y;
		view_rotation_y = Mathf.Clamp (view_rotation_y, min_angle_y, max_angle_y);

		rotation_y = Mathf.Clamp(rotation_y, view_rotation_y - rotation_y_max_leeway, view_rotation_y + rotation_y_min_leeway);
		rotation_x = Mathf.Clamp(rotation_x, view_rotation_x - rotation_x_leeway, view_rotation_x + rotation_x_leeway);
}
}
}

void  UpdateCameraAndPlayerTransformation (){
main_camera.transform.localEulerAngles = Vector3(-view_rotation_y, view_rotation_x, head_tilt);
if(!disable_recoil){
main_camera.transform.localEulerAngles += Vector3(head_recoil_spring_y.state, head_recoil_spring_x.state, 0); 
}
character_controller.transform.localEulerAngles.y = view_rotation_x;
main_camera.transform.position = transform.position;
main_camera.transform.position.y += character_controller.height * character_controller.transform.localScale.y - 0.1f;
main_camera.transform.position.y += head_fall;
}

void  UpdateGunTransformation (){
FIXME_VAR_TYPE aim_dir= AimDir();
FIXME_VAR_TYPE aim_pos= AimPos();	

FIXME_VAR_TYPE unaimed_dir= (transform.forward + Vector3(0,-1,0)).normalized;
FIXME_VAR_TYPE unaimed_pos= main_camera.transform.position + unaimed_dir*GunDist();

if(disable_springs){ 
gun_instance.transform.position = mix(unaimed_pos, aim_pos, aim_spring.target_state);
gun_instance.transform.forward = mix(unaimed_dir, aim_dir, aim_spring.target_state);
} else { 
gun_instance.transform.position = mix(unaimed_pos, aim_pos, aim_spring.state);
gun_instance.transform.forward = mix(unaimed_dir, aim_dir, aim_spring.state);
}

if(disable_springs) {
ApplyPose("pose_slide_pull", slide_pose_spring.target_state);
ApplyPose("pose_reload", reload_pose_spring.target_state);
ApplyPose("pose_press_check", press_check_pose_spring.target_state);
ApplyPose("pose_inspect_cylinder", inspect_cylinder_pose_spring.target_state);
ApplyPose("pose_add_rounds", add_rounds_pose_spring.target_state);
ApplyPose("pose_eject_rounds", eject_rounds_pose_spring.target_state); 
} else {
ApplyPose("pose_slide_pull", slide_pose_spring.state);
ApplyPose("pose_reload", reload_pose_spring.state);
ApplyPose("pose_press_check", press_check_pose_spring.state);
ApplyPose("pose_inspect_cylinder", inspect_cylinder_pose_spring.state);
ApplyPose("pose_add_rounds", add_rounds_pose_spring.state);
ApplyPose("pose_eject_rounds", eject_rounds_pose_spring.state); 
}

if(!disable_recoil){		
gun_instance.transform.RotateAround(
		gun_instance.transform.FindChild("point_recoil_rotate").position,
		gun_instance.transform.rotation * Vector3(1,0,0),
		x_recoil_spring.state);

gun_instance.transform.RotateAround(
		gun_instance.transform.FindChild("point_recoil_rotate").position,
		Vector3(0,1,0),
		y_recoil_spring.state); 
}
}

void  UpdateFlashlightTransformation (){
FIXME_VAR_TYPE flashlight_hold_pos= main_camera.transform.position + main_camera.transform.rotation*Vector3(-0.15f,-0.01f,0.15f);
FIXME_VAR_TYPE flashlight_hold_rot= main_camera.transform.rotation;

FIXME_VAR_TYPE flashlight_pos= flashlight_hold_pos;
FIXME_VAR_TYPE flashlight_rot= flashlight_hold_rot;

held_flashlight.transform.position = flashlight_pos;
held_flashlight.transform.rotation = flashlight_rot;

held_flashlight.transform.RotateAround(
held_flashlight.transform.FindChild("point_recoil_rotate").position,
held_flashlight.transform.rotation * Vector3(1,0,0),
x_recoil_spring.state * 0.3f);

held_flashlight.transform.RotateAround(
held_flashlight.transform.FindChild("point_recoil_rotate").position,
Vector3(0,1,0),
y_recoil_spring.state * 0.3f);

flashlight_pos = held_flashlight.transform.position;
flashlight_rot = held_flashlight.transform.rotation;

if(gun_instance){
flashlight_aim_pos = gun_instance.transform.position + gun_instance.transform.rotation*Vector3(0.07f,-0.03f,0.0f);
flashlight_aim_rot = gun_instance.transform.rotation;

flashlight_aim_pos -= main_camera.transform.position;
flashlight_aim_pos = Quaternion.Inverse(main_camera.transform.rotation) * flashlight_aim_pos;
flashlight_aim_rot = Quaternion.Inverse(main_camera.transform.rotation) * flashlight_aim_rot;
}

if(disable_springs){
flashlight_pos = mix(flashlight_pos, main_camera.transform.rotation * flashlight_aim_pos + main_camera.transform.position, aim_spring.target_state);
flashlight_rot = mix(flashlight_rot, main_camera.transform.rotation * flashlight_aim_rot, aim_spring.target_state);
} else {
flashlight_pos = mix(flashlight_pos, main_camera.transform.rotation * flashlight_aim_pos + main_camera.transform.position, aim_spring.state);
flashlight_rot = mix(flashlight_rot, main_camera.transform.rotation * flashlight_aim_rot, aim_spring.state);
} 

FIXME_VAR_TYPE flashlight_mouth_pos= main_camera.transform.position + main_camera.transform.rotation*Vector3(0.0f,-0.08f,0.05f);
FIXME_VAR_TYPE flashlight_mouth_rot= main_camera.transform.rotation;

flashlight_mouth_spring.target_state = 0.0f;
if(magazine_instance_in_hand){
flashlight_mouth_spring.target_state = 1.0f;
}
flashlight_mouth_spring.target_state = Mathf.Max(flashlight_mouth_spring.target_state,
(inspect_cylinder_pose_spring.state + eject_rounds_pose_spring.state + (press_check_pose_spring.state/0.6f) + (reload_pose_spring.state/0.7f) + slide_pose_spring.state) * aim_spring.state);

flashlight_mouth_spring.Update();

if(disable_springs){
flashlight_pos = mix(flashlight_pos, flashlight_mouth_pos, flashlight_mouth_spring.target_state);
flashlight_rot = mix(flashlight_rot, flashlight_mouth_rot, flashlight_mouth_spring.target_state);

flashlight_pos = mix(flashlight_pos, flash_ground_pos, flash_ground_pose_spring.target_state);
flashlight_rot = mix(flashlight_rot, flash_ground_rot, flash_ground_pose_spring.target_state);
} else {
flashlight_pos = mix(flashlight_pos, flashlight_mouth_pos, flashlight_mouth_spring.state);
flashlight_rot = mix(flashlight_rot, flashlight_mouth_rot, flashlight_mouth_spring.state);

flashlight_pos = mix(flashlight_pos, flash_ground_pos, flash_ground_pose_spring.state);
flashlight_rot = mix(flashlight_rot, flash_ground_rot, flash_ground_pose_spring.state);
}

held_flashlight.transform.position = flashlight_pos;
held_flashlight.transform.rotation = flashlight_rot;
}

void  UpdateMagazineTransformation (){
if(gun_instance){
mag_pos = gun_instance.transform.position;
mag_rot = gun_instance.transform.rotation;
mag_pos += (gun_instance.transform.FindChild("point_mag_to_insert").position - 
		gun_instance.transform.FindChild("point_mag_inserted").position);
}
if(mag_stage == HandMagStage.HOLD || mag_stage == HandMagStage.HOLD_TO_INSERT){
FIXME_VAR_TYPE mag_script= magazine_instance_in_hand.GetComponent<mag_script>();
FIXME_VAR_TYPE hold_pos= main_camera.transform.position + main_camera.transform.rotation*mag_script.hold_offset;
FIXME_VAR_TYPE hold_rot= main_camera.transform.rotation * Quaternion.AngleAxis(mag_script.hold_rotation.x, Vector3(0,1,0)) * Quaternion.AngleAxis(mag_script.hold_rotation.y, Vector3(1,0,0));
if(disable_springs){ 
		hold_pos = mix(hold_pos, mag_ground_pos, mag_ground_pose_spring.target_state);
		hold_rot = mix(hold_rot, mag_ground_rot, mag_ground_pose_spring.target_state);
} else {
		hold_pos = mix(hold_pos, mag_ground_pos, mag_ground_pose_spring.state);
		hold_rot = mix(hold_rot, mag_ground_rot, mag_ground_pose_spring.state);
}
if(hold_pose_spring.state != 1.0f){ 
		FIXME_VAR_TYPE amount= hold_pose_spring.state;
		if(disable_springs){ 
				amount = hold_pose_spring.target_state;
		}
		magazine_instance_in_hand.transform.position = mix(mag_pos, hold_pos, amount);
		magazine_instance_in_hand.transform.rotation = mix(mag_rot, hold_rot, amount);
} else {
		magazine_instance_in_hand.transform.position = hold_pos;
		magazine_instance_in_hand.transform.rotation = hold_rot;
}
} else {
magazine_instance_in_hand.transform.position = mag_pos;
magazine_instance_in_hand.transform.rotation = mag_rot;
} 
}

void  UpdateInventoryTransformation (){
FIXME_VAR_TYPE i= 0;
for(i=0; i<10; ++i){
FIXME_VAR_TYPE slot= weapon_slots[i];
if(slot.type == WeaponSlotType.EMPTY){
		continue;
}
slot.obj.transform.localScale = Vector3(1.0f,1.0f,1.0f); 
}
for(i=0; i<10; ++i){
slot = weapon_slots[i];
if(slot.type == WeaponSlotType.EMPTY){
		continue;
}
FIXME_VAR_TYPE start_pos= main_camera.transform.position + slot.start_pos;
FIXME_VAR_TYPE start_rot= main_camera.transform.rotation * slot.start_rot;
if(slot.type == WeaponSlotType.EMPTYING){
		start_pos = slot.obj.transform.position;
		start_rot = slot.obj.transform.rotation;
		if(Mathf.Abs(slot.spring.vel) <= 0.01f && slot.spring.state <= 0.01f){
				slot.type = WeaponSlotType.EMPTY;
				slot.spring.state = 0.0f;
		}
} 
FIXME_VAR_TYPE scale= 0.0f;
if(disable_springs){  
		slot.obj.transform.position = mix(
				start_pos, 
				main_camera.transform.position + main_camera.GetComponent.<Camera>().ScreenPointToRay(Vector3(main_camera.GetComponent.<Camera>().pixelWidth * (0.05f + i*0.15f), main_camera.GetComponent.<Camera>().pixelHeight * 0.17f,0)).direction * 0.3f, 
				slot.spring.target_state);
		scale = 0.3f * slot.spring.target_state + (1.0f - slot.spring.target_state);
		slot.obj.transform.localScale.x *= scale;
		slot.obj.transform.localScale.y *= scale;
		slot.obj.transform.localScale.z *= scale; 
		slot.obj.transform.rotation = mix(
				start_rot, 
				main_camera.transform.rotation * Quaternion.AngleAxis(90, Vector3(0,1,0)), 
				slot.spring.target_state);
} else {  
		slot.obj.transform.position = mix(
				start_pos, 
				main_camera.transform.position + main_camera.GetComponent.<Camera>().ScreenPointToRay(Vector3(main_camera.GetComponent.<Camera>().pixelWidth * (0.05f + i*0.15f), main_camera.GetComponent.<Camera>().pixelHeight * 0.17f,0)).direction * 0.3f, 
				slot.spring.state);
		scale = 0.3f * slot.spring.state + (1.0f - slot.spring.state);
		slot.obj.transform.localScale.x *= scale;
		slot.obj.transform.localScale.y *= scale;
		slot.obj.transform.localScale.z *= scale; 
		slot.obj.transform.rotation = mix(
				start_rot, 
				main_camera.transform.rotation * Quaternion.AngleAxis(90, Vector3(0,1,0)), 
				slot.spring.state);
}
FIXME_VAR_TYPE renderers= slot.obj.GetComponentsInChildren<Renderer>();
foreach(Renderer renderer in renderers){
		renderer.castShadows = false; 
}
slot.spring.Update();
}
}

void  UpdateLooseBulletDisplay (){
FIXME_VAR_TYPE revolver_open= (gun_instance && gun_instance.GetComponent<GunScript>().IsCylinderOpen());
if((mag_stage == HandMagStage.HOLD && !gun_instance) || picked_up_bullet_delay > 0.0f || revolver_open){
show_bullet_spring.target_state = 1.0f;
picked_up_bullet_delay = Mathf.Max(0.0f, picked_up_bullet_delay - Time.deltaTime);
} else {	
show_bullet_spring.target_state = 0.0f;
}
show_bullet_spring.Update();

for(FIXME_VAR_TYPE i=0; i<loose_bullets.length; ++i){
Spring spring = loose_bullet_spring[i];
spring.Update();
GameObject bullet = loose_bullets[i];
bullet.transform.position = main_camera.transform.position + main_camera.GetComponent.<Camera>().ScreenPointToRay(Vector3(0.0f, main_camera.GetComponent.<Camera>().pixelHeight,0)).direction * 0.3f;
bullet.transform.position += main_camera.transform.rotation * Vector3(0.02f,-0.01f,0);
bullet.transform.position += main_camera.transform.rotation * Vector3(0.006f * i,0.0f,0);
bullet.transform.position += main_camera.transform.rotation * Vector3(-0.03f,0.03f,0) * (1.0f - show_bullet_spring.state);
bullet.transform.localScale.x = spring.state;
bullet.transform.localScale.y = spring.state;
bullet.transform.localScale.z = spring.state;
bullet.transform.rotation = main_camera.transform.rotation * Quaternion.AngleAxis(90, Vector3(-1,0,0));
FIXME_VAR_TYPE renderers= bullet.GetComponentsInChildren<Renderer>();
foreach(Renderer renderer in renderers){
		renderer.castShadows = false; 
}
}
}

void  UpdateSprings (){	
slide_pose_spring.Update();
reload_pose_spring.Update();
press_check_pose_spring.Update();
inspect_cylinder_pose_spring.Update();
add_rounds_pose_spring.Update();
eject_rounds_pose_spring.Update();
x_recoil_spring.Update();
y_recoil_spring.Update();
head_recoil_spring_x.Update();
head_recoil_spring_y.Update();
if(mag_stage == HandMagStage.HOLD || mag_stage == HandMagStage.HOLD_TO_INSERT){
hold_pose_spring.Update();
mag_ground_pose_spring.Update();
}
flash_ground_pose_spring.Update();
}

void  UpdatePickupMagnet (){
FIXME_VAR_TYPE attract_pos= transform.position - Vector3(0,character_controller.height * 0.2f,0);
for(FIXME_VAR_TYPE i=0; i<collected_rounds.length; ++i){
FIXME_VAR_TYPE round= collected_rounds[i] as GameObject;
if(!round){
		continue;
}
round.GetComponent.<Rigidbody>().velocity += (attract_pos - round.transform.position) * Time.deltaTime * 20.0f;
round.GetComponent.<Rigidbody>().velocity *= Mathf.Pow(0.1f, Time.deltaTime);;
//round.rigidbody.position += round.rigidbody.velocity * Time.deltaTime;
if(Vector3.Distance(round.transform.position, attract_pos) < 0.5f){
		if(round.gameObject.name == "cassette_tape(Clone)"){
				++unplayed_tapes;
		} else {
				AddLooseBullet(true);
				collected_rounds.splice(i,1);
				PlaySoundFromGroup(sound_bullet_grab, 0.2f);
		}
		GameObject.Destroy(round);
}
}
collected_rounds.remove(null);
}

void  Update (){
UpdateTape();
UpdateCheats();
UpdateFallOffMapDeath();
UpdateHealth();
UpdateHelpToggle();	
UpdateLevelResetButton();
UpdateLevelChange();
UpdateLevelEndEffects();
AudioListener.volume = dead_volume_fade * PlayerPrefs.GetFloat("master_volume", 1.0f);
UpdateAimSpring();
UpdateCameraRotationControls();
UpdateCameraAndPlayerTransformation();	
if(gun_instance){
UpdateGunTransformation();
}
if(held_flashlight){
UpdateFlashlightTransformation();
}				
if(magazine_instance_in_hand){
UpdateMagazineTransformation();
}
UpdateInventoryTransformation();
UpdateLooseBulletDisplay();
FIXME_VAR_TYPE in_menu= GameObject.Find("gui_skin_holder").GetComponent<optionsmenuscript>().IsMenuShown();
if(!dead && !in_menu){
HandleControls();
}
UpdateSprings();	
UpdatePickupMagnet();
}

void  FixedUpdate (){
}

class DisplayLine {
string str;
bool  bold;
void  DisplayLine ( string _str ,   bool _bold  ){
bold = _bold;
str = _str;
}
};

bool ShouldHolsterGun (){
if(!loose_bullets){
return;
}
if(loose_bullets.length > 0){
} else return false;
if(magazine_instance_in_hand){
} else return false;
if(magazine_instance_in_hand.GetComponent<mag_script>().NumRounds() == 0){
} else return false;
return true;
}

bool CanLoadBulletsInMag (){
return !gun_instance && mag_stage == HandMagStage.HOLD && loose_bullets.length > 0 && !magazine_instance_in_hand.GetComponent<mag_script>().IsFull();
}

bool CanRemoveBulletFromMag (){
return !gun_instance && mag_stage == HandMagStage.HOLD && magazine_instance_in_hand.GetComponent<mag_script>().NumRounds() > 0;
}

bool ShouldDrawWeapon (){
return !gun_instance && !CanLoadBulletsInMag();
}

int GetMostLoadedMag (){
FIXME_VAR_TYPE max_rounds= 0;
FIXME_VAR_TYPE max_rounds_slot= -1;
for(FIXME_VAR_TYPE i=0; i<10; ++i){
if(weapon_slots[i].type == WeaponSlotType.MAGAZINE){
		FIXME_VAR_TYPE rounds= weapon_slots[i].obj.GetComponent<mag_script>().NumRounds();
		if(rounds > max_rounds){
				max_rounds_slot = i+1;
				max_rounds = rounds;
		}
}
}
return max_rounds_slot;
}

bool ShouldPutMagInInventory (){
FIXME_VAR_TYPE rounds= magazine_instance_in_hand.GetComponent<mag_script>().NumRounds();
FIXME_VAR_TYPE most_loaded= GetMostLoadedMag();
if(most_loaded == -1){
return false;
}
if(weapon_slots[most_loaded-1].obj.GetComponent<mag_script>().NumRounds() > rounds){
return true;
}
return false;
}

int GetEmptySlot (){
FIXME_VAR_TYPE empty_slot= -1;
for(FIXME_VAR_TYPE i=0; i<10; ++i){
if(weapon_slots[i].type == WeaponSlotType.EMPTY){
		empty_slot = i+1;
		break;
}
}
return empty_slot;
}

int GetFlashlightSlot (){
FIXME_VAR_TYPE flashlight_slot= -1;
for(FIXME_VAR_TYPE i=0; i<10; ++i){
if(weapon_slots[i].type == WeaponSlotType.FLASHLIGHT){
		flashlight_slot = i+1;
		break;
}
}
return flashlight_slot;
}

void  OnGUI (){
FIXME_VAR_TYPE display_text= new Array();
GunScript gun_script = null;
if(gun_instance){
gun_script = gun_instance.GetComponent<GunScript>();
}
display_text.push(new DisplayLine(tapes_heard.length + " tapes absorbed out of "+total_tapes.length, true));
if(!show_help){
display_text.push(new DisplayLine("View help: Press [ ? ]", !help_ever_shown));
} else {
display_text.push(new DisplayLine("Hide help: Press [ ? ]", false));
display_text.push(new DisplayLine("", false));
if(tape_in_progress){
		display_text.push(new DisplayLine("Pause/Resume tape player: [ x ]", false));
}

display_text.push(new DisplayLine("Look: [ move mouse ]", false));
display_text.push(new DisplayLine("Move: [ WASD ]", false));
display_text.push(new DisplayLine("Jump: [ space ]", false));
display_text.push(new DisplayLine("Pick up nearby: hold [ g ]", ShouldPickUpNearby()));
if(held_flashlight){
		FIXME_VAR_TYPE empty_slot= GetEmptySlot();
		if(empty_slot != -1){
				FIXME_VAR_TYPE str= "Put flashlight in inventory: tap [ ";
				str += empty_slot;
				str += " ]";
				display_text.push(new DisplayLine(str, false));
		}
} else {
		FIXME_VAR_TYPE flashlight_slot= GetFlashlightSlot();
		if(flashlight_slot != -1){
				str = "Equip flashlight: tap [ ";
				str += flashlight_slot;
				str += " ]";
				display_text.push(new DisplayLine(str, true));
		}
}
if(gun_instance){
		display_text.push(new DisplayLine("Fire weapon: tap [ left mouse button ]", false));
		FIXME_VAR_TYPE should_aim= (aim_spring.state < 0.5f);			
		display_text.push(new DisplayLine("Aim weapon: hold [ right mouse button ]", should_aim));
		display_text.push(new DisplayLine("Aim weapon: tap [ q ]", should_aim));
		display_text.push(new DisplayLine("Holster weapon: tap [ ~ ]", ShouldHolsterGun()));
} else {
		display_text.push(new DisplayLine("Draw weapon: tap [ ~ ]", ShouldDrawWeapon()));
}
if(gun_instance){
		if(gun_script.HasSlide()){
				display_text.push(new DisplayLine("Pull back slide: hold [ r ]", gun_script.ShouldPullSlide()?true:false));
				display_text.push(new DisplayLine("Release slide lock: tap [ t ]", gun_script.ShouldReleaseSlideLock()?true:false));
		}
		if(gun_script.HasSafety()){
				display_text.push(new DisplayLine("Toggle safety: tap [ v ]", gun_script.IsSafetyOn()?true:false));
		}
		if(gun_script.HasAutoMod()){
				display_text.push(new DisplayLine("Toggle full-automatic: tap [ v ]", gun_script.ShouldToggleAutoMod()?true:false));
		}
		if(gun_script.HasHammer()){
				display_text.push(new DisplayLine("Pull back hammer: hold [ f ]", gun_script.ShouldPullBackHammer()?true:false));
		}
		if(gun_script.gun_type == GunType.REVOLVER){
				if(!gun_script.IsCylinderOpen()){
						display_text.push(new DisplayLine("Open cylinder: tap [ e ]", (gun_script.ShouldOpenCylinder() && loose_bullets.length!=0)?true:false));
				} else {
						display_text.push(new DisplayLine("Close cylinder: tap [ r ]", (gun_script.ShouldCloseCylinder() || loose_bullets.length==0)?true:false));
						display_text.push(new DisplayLine("Extract casings: hold [ v ]", gun_script.ShouldExtractCasings()?true:false));
						display_text.push(new DisplayLine("Insert bullet: tap [ z ]", (gun_script.ShouldInsertBullet() && loose_bullets.length!=0)?true:false));
				}
				display_text.push(new DisplayLine("Spin cylinder: [ mousewheel ]", false));
		}
		if(mag_stage == HandMagStage.HOLD && !gun_script.IsThereAMagInGun()){
				FIXME_VAR_TYPE should_insert_mag= (magazine_instance_in_hand.GetComponent<mag_script>().NumRounds() >= 1);
				display_text.push(new DisplayLine("Insert magazine: tap [ z ]", should_insert_mag));
		} else if(mag_stage == HandMagStage.EMPTY && gun_script.IsThereAMagInGun()){
				display_text.push(new DisplayLine("Eject magazine: tap [ e ]", gun_script.ShouldEjectMag()?true:false));
		} else if(mag_stage == HandMagStage.EMPTY && !gun_script.IsThereAMagInGun()){
				FIXME_VAR_TYPE max_rounds_slot= GetMostLoadedMag();
				if(max_rounds_slot != -1){
						display_text.push(new DisplayLine("Equip magazine: tap [ "+max_rounds_slot+" ]", true));
				}
		}
} else {
		if(CanLoadBulletsInMag()){
				display_text.push(new DisplayLine("Insert bullet in magazine: tap [ z ]", true));
		}
		if(CanRemoveBulletFromMag()){
				display_text.push(new DisplayLine("Remove bullet from magazine: tap [ r ]", false));
		}
}
if(mag_stage == HandMagStage.HOLD){
		empty_slot = GetEmptySlot();
		if(empty_slot != -1){
				str = "Put magazine in inventory: tap [ ";
				str += empty_slot;
				str += " ]";
				display_text.push(new DisplayLine(str, ShouldPutMagInInventory()));
		}
		display_text.push(new DisplayLine("Drop magazine: tap [ e ]", false));
}

display_text.push(new DisplayLine("", false));
if(show_advanced_help){
		display_text.push(new DisplayLine("Advanced help:", false));
		display_text.push(new DisplayLine("Toggle crouch: [ c ]", false));
		if(aim_spring.state < 0.5f){
				display_text.push(new DisplayLine("Run: tap repeatedly [ w ]", false));
		}
		if(gun_instance){
				if(!gun_script.IsSafetyOn() && gun_script.IsHammerCocked()){
						display_text.push(new DisplayLine("Decock: Hold [f], hold [LMB], release [f]", ShouldPickUpNearby()));
				}
				if(!gun_script.IsSlideLocked() && !gun_script.IsSafetyOn()){
						display_text.push(new DisplayLine("Inspect chamber: hold [ t ] and then [ r ]", false));
				}
				if(mag_stage == HandMagStage.EMPTY && !gun_script.IsThereAMagInGun()){
						max_rounds_slot = GetMostLoadedMag();
						if(max_rounds_slot != -1){
								display_text.push(new DisplayLine("Quick load magazine: double tap [ "+max_rounds_slot+" ]", false));
						}
				}
		}
		display_text.push(new DisplayLine("Reset game: hold [ l ]", false));
} else {
		display_text.push(new DisplayLine("Advanced help: Hold [ ? ]", false));
}
}
GUIStyle style = holder.gui_skin.label;
FIXME_VAR_TYPE width= Screen.width * 0.5f;
FIXME_VAR_TYPE offset= 0;
foreach(DisplayLine line in display_text){
if(line.bold){
		style.fontStyle = FontStyle.Bold;
} else {
		style.fontStyle = FontStyle.Normal;
}
style.fontSize = 18;
style.normal.textColor = Color(0,0,0);
GUI.Label( new Rect(width+0.5f,offset+0.5f,width+0.5f,offset+20+0.5f),line.str, style);
if(line.bold){
		style.normal.textColor = Color(1,1,1);
} else {
		style.normal.textColor = Color(0.7f,0.7f,0.7f);
}
GUI.Label( new Rect(width,offset,width,offset+30),line.str, style);
offset += 20;
}
if(dead_fade > 0.0f){
if(!texture_death_screen){
		Debug.LogError("Assign a Texture in the inspector.");
		return;
}
GUI.color = Color(0,0,0,dead_fade);
GUI.DrawTexture( new Rect(0,0,Screen.width,Screen.height), texture_death_screen, ScaleMode.StretchToFill, true);
}
if(win_fade > 0.0f){
GUI.color = Color(1,1,1,win_fade);
GUI.DrawTexture( new Rect(0,0,Screen.width,Screen.height), texture_death_screen, ScaleMode.StretchToFill, true);
}
}
}

}
