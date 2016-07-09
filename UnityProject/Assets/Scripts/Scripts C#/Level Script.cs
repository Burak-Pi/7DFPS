using UnityEngine;
using System.Collections;

public class LevelScript : MonoBehaviour {

		void Start () {
				GameObject enemies = transform.FindChild("enemies");

				if(enemies!=null){
						foreach(Transform child in enemies){
								if(Random.Range(0.0f,1.0f) < 0.9f){
									Destroy(child.gameObject, 0);
								}
						}
				}
				GameObject players = transform.FindChild("player_spawn");
				if(players){
						int num = 0;
						foreach(Transform child in players){
								++num;
						}
						int save = Random.Range(0,num);
						int j=0;
						foreach(Transform child in players){
								if(j != save){
										Destroy(child.gameObject,0);
								}
								++j;
						}
				}
				GameObject items = transform.FindChild("items");
				if(items!=null){
						foreach(Transform child in items){
								if(Random.Range(0.0f,1.0f) < 0.9f){
										Destroy(child.gameObject,0);
								}
						}
				}
		}

}
