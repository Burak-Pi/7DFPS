using UnityEngine;
using System.Collections;

public class References : MonoBehaviour {

	public static References thisReferences;
		public GameObject destroy_effect;
	void Awake () {
		thisReferences = this;
	}
	
}
