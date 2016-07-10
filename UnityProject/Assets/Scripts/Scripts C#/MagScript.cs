using UnityEngine;
using System.Collections;

public class MagScript : MonoBehaviour {

		private int num_rounds = 8;
		int kMaxRounds = 8;
		private Vector3[] round_pos;
		public  Vector3 old_pos, hold_offset, hold_rotation;
		private Quaternion[] round_rot;
		bool collided, disable_interp = true;
		AudioClip[] sound_add_round, sound_mag_bounce;
		float life_time = 0, mag_load_progress = 0;
		enum MagLoadStage {NONE, PUSHING_DOWN, ADDING_ROUND, REMOVING_ROUND, PUSHING_UP};
		MagLoadStage mag_load_stage = MagLoadStage.NONE;

		public bool RemoveRound() {
				if(num_rounds == 0){
						return false;
				}
				GameObject round_obj = transform.FindChild("round_"+num_rounds);
				round_obj.GetComponent<Renderer>().enabled = false;
				--num_rounds;
				return true;
		}

		public bool RemoveRoundAnimated() {
				if(num_rounds == 0 || mag_load_stage != MagLoadStage.NONE){
						return false;
				}
				mag_load_stage = MagLoadStage.REMOVING_ROUND;
				mag_load_progress = 0;
				return true;
		}

		public bool IsFull(){
			return num_rounds == kMaxRounds;
		}

		public bool AddRound() {
				if(num_rounds >= kMaxRounds || mag_load_stage != MagLoadStage.NONE){
						return false;
				}
				mag_load_stage = MagLoadStage.PUSHING_DOWN;
				mag_load_progress = 0;
				PlaySoundFromGroup(sound_add_round, 0.3f);
				++num_rounds;
				GameObject round_obj = transform.FindChild("round_"+num_rounds);
				round_obj.GetComponent<Renderer>().enabled = true;
				return true;
		}

		public int NumRounds(){
				return num_rounds;
		}

		void Start () {
				old_pos = transform.position;
				num_rounds = Random.Range(0,kMaxRounds);
				round_pos = new Vector3[kMaxRounds];
				round_rot = new Quaternion[kMaxRounds];
				for(int i=0; i<kMaxRounds; ++i){
						GameObject round = transform.FindChild("round_"+(i+1));
						round_pos[i] = round.localPosition;
						round_rot[i] = round.localRotation;
						if(i < num_rounds){
								round.GetComponent<Renderer>().enabled = true;
						} else {
								round.GetComponent<Renderer>().enabled = false;
						}
				}
		}

		void PlaySoundFromGroup(AudioClip[] group, float volume){
				if(group.Length == 0)return;
				AudioClip which_shot = Random.Range(0,group.Length);
				GetComponent<AudioSource>().PlayOneShot(group[which_shot], volume * PlayerPrefs.GetFloat("sound_volume", 1));
		}

		void CollisionSound() {
				if(!collided){
						collided = true;
						PlaySoundFromGroup(sound_mag_bounce, 0.3f);
				}
		}
		RaycastHit hit;
		void FixedUpdate () {
				if(GetComponent<Rigidbody>() && !GetComponent<Rigidbody>().IsSleeping() && GetComponent<Collider>() && GetComponent<Collider>().enabled){
						life_time += Time.deltaTime;
						if(Physics.Linecast(old_pos, transform.position, hit, 1)){
								transform.position = hit.point;
								transform.GetComponent<Rigidbody>().velocity *= -0.3f;
						}
						if(life_time > 2.0f){
								GetComponent<Rigidbody>().Sleep();
						}
				} else if(!GetComponent<Rigidbody>()){
						life_time = 0;
						collided = false;
				}
				old_pos = transform.position;
		}
		Transform obj;
		void Update() {
				switch(mag_load_stage){
				case MagLoadStage.PUSHING_DOWN:
						mag_load_progress += Time.deltaTime * 20;
						if(mag_load_progress >= 1){
								mag_load_stage = MagLoadStage.ADDING_ROUND;
								mag_load_progress = 0;
						}
						break;
				case MagLoadStage.ADDING_ROUND:
						mag_load_progress += Time.deltaTime * 20;
						if(mag_load_progress >= 1){
								mag_load_stage = MagLoadStage.NONE;
								mag_load_progress = 0;
								for(int i=0; i<num_rounds; ++i){
										obj = transform.FindChild("round_"+(i+1));
										obj.localPosition = round_pos[i];
										obj.localRotation = round_rot[i];
								}
						}
						break;
				case MagLoadStage.PUSHING_UP:
						mag_load_progress += Time.deltaTime * 20;
						if(mag_load_progress >= 1){
								mag_load_stage = MagLoadStage.NONE;
								mag_load_progress = 0;
								RemoveRound();
								for(int i=0; i<num_rounds; ++i){
										obj = transform.FindChild("round_"+(i+1));
										obj.localPosition = round_pos[i];
										obj.localRotation = round_rot[i];
								}
						}
						break;
				case MagLoadStage.REMOVING_ROUND:
						mag_load_progress += Time.deltaTime * 20;
						if(mag_load_progress >= 1){
								mag_load_stage = MagLoadStage.PUSHING_UP;
								mag_load_progress = 0;
						}
						break;
				}
				var mag_load_progress_display = mag_load_progress;
				if(disable_interp){
						mag_load_progress_display = Mathf.Floor(mag_load_progress + 0.5f);
				}
				switch(mag_load_stage){
				case MagLoadStage.PUSHING_DOWN:
						obj = transform.FindChild("round_1");
						obj.localPosition = Vector3.Lerp(transform.FindChild("point_start_load").localPosition, 
								transform.FindChild("point_load").localPosition, 
								mag_load_progress_display);
						obj.localRotation = Quaternion.Slerp(transform.FindChild("point_start_load").localRotation, 
								transform.FindChild("point_load").localRotation, 
								mag_load_progress_display);
						for(int i=1; i<num_rounds; ++i){
								obj = transform.FindChild("round_"+(i+1));
								obj.localPosition = Vector3.Lerp(round_pos[i-1], round_pos[i], mag_load_progress_display);
								obj.localRotation = Quaternion.Slerp(round_rot[i-1], round_rot[i], mag_load_progress_display);
						}
						break;
				case MagLoadStage.ADDING_ROUND:
						obj = transform.FindChild("round_1");
						obj.localPosition = Vector3.Lerp(transform.FindChild("point_load").localPosition, 
								round_pos[0], 
								mag_load_progress_display);
						obj.localRotation = Quaternion.Slerp(transform.FindChild("point_load").localRotation, 
								round_rot[0], 
								mag_load_progress_display);
						for(int i=1; i<num_rounds; ++i){
								obj = transform.FindChild("round_"+(i+1));
								obj.localPosition = round_pos[i];
						}
						break;
				case MagLoadStage.PUSHING_UP:
						obj = transform.FindChild("round_1");
						obj.localPosition = Vector3.Lerp(transform.FindChild("point_start_load").localPosition, 
								transform.FindChild("point_load").localPosition, 
								1.0f-mag_load_progress_display);
						obj.localRotation = Quaternion.Slerp(transform.FindChild("point_start_load").localRotation, 
								transform.FindChild("point_load").localRotation, 
								1.0f-mag_load_progress_display);
						for(int i=1; i<num_rounds; ++i){
								obj = transform.FindChild("round_"+(i+1));
								obj.localPosition = Vector3.Lerp(round_pos[i-1], round_pos[i], mag_load_progress_display);
								obj.localRotation = Quaternion.Slerp(round_rot[i-1], round_rot[i], mag_load_progress_display);
						}
						break;
				case MagLoadStage.REMOVING_ROUND:
						obj = transform.FindChild("round_1");
						obj.localPosition = Vector3.Lerp(transform.FindChild("point_load").localPosition, 
								round_pos[0], 
								1.0f-mag_load_progress_display);
						obj.localRotation = Quaternion.Slerp(transform.FindChild("point_load").localRotation, 
								round_rot[0], 
								1.0f-mag_load_progress_display);
						for(int i=1; i<num_rounds; ++i){
								obj = transform.FindChild("round_"+(i+1));
								obj.localPosition = round_pos[i];
								obj.localRotation = round_rot[i];
						}
						break;
				}
		}

		void OnCollisionEnter (Collision collision) {
				CollisionSound();
		}
}
