using UnityEngine;
using System.Collections;

public class GUISkinHolder : MonoBehaviour {

		GUISkin gui_skin;
		AudioClip[] sound_scream, sound_tape_content, sound_tape_start;
		AudioClip sound_tape_end, sound_tape_background, win_sting;
		GameObject tape_object, weapon, flashlight_object;
		GameObject[] weapons;
		bool has_flashlight;

		void Awake () {
			weapon = weapons[Random.Range(0,weapons.Length)];
		}

}
