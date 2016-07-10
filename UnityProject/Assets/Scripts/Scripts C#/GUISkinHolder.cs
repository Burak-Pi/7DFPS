using UnityEngine;
using System.Collections;

public class GUISkinHolder : MonoBehaviour {

		public GUISkin gui_skin;
		public 	AudioClip[] sound_scream, sound_tape_content;
		public AudioClip sound_tape_end, sound_tape_background, sound_tape_start, win_sting;
		public GameObject tape_object, weapon, flashlight_object;
		public GameObject[] weapons;
		public bool has_flashlight;

		void Awake () {
			weapon = weapons[Random.Range(0,weapons.Length)];
		}

}
