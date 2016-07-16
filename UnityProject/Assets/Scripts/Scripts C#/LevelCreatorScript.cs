using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelCreatorScript : MonoBehaviour {

		public GameObject[] level_tiles;
		List<Light> shadowed_lights = new List<Light>();
		List<int> tiles = new List<int>();
		GameObject child_obj;
		float shadowed_amount, shadow_threshold, fade_threshold;
		Transform main_camera;
		Component[] lights;
		void SpawnTile(int where,float challenge , bool player){
				GameObject level_obj = level_tiles[Random.Range(0,level_tiles.Length)];
				GameObject level = new GameObject(level_obj.name + " (Clone)");
				foreach (Transform child in level_obj.transform){
						if(child.gameObject.name != "enemies" && child.gameObject.name != "player_spawn" && child.gameObject.name != "items"){
								child_obj = Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition, child.localRotation) as GameObject;
								child_obj.transform.parent = level.transform;
						}
				}
				Transform enemies = level_obj.transform.FindChild("enemies");
				if(enemies){
						foreach(Transform child in enemies){
								if(Random.Range(0.0f,1.0f) <= challenge){
										child_obj = Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition + enemies.localPosition, child.localRotation) as GameObject;
										child_obj.transform.parent = level.transform;
								}
						}
				}
				Transform items = level_obj.transform.FindChild("items");
				if(items){
						foreach(Transform child in items){
								if(Random.Range(0.0f,1.0f) <= (player?challenge+0.3f:challenge)){
										child_obj = Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition + items.localPosition, items.localRotation) as GameObject;
										child_obj.transform.parent = level.transform;
								}
						}
				}
				if(player){
						Transform players = level_obj.transform.FindChild("player_spawn");
						if(players){
								var num = 0;
								foreach(Transform child in players){
										++num;
								}
								var save = Random.Range(0,num);
								var j=0;
								foreach(Transform child in players){
										if(j == save){
												child_obj = Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition + players.localPosition, child.localRotation) as GameObject;
												child_obj.transform.parent = level.transform;
												child_obj.name = "Player";
										}
										++j;
								}
						}
				}
				level.transform.parent = this.gameObject.transform;

				lights = GetComponentsInChildren<Light>();
				foreach(Light light in lights){
						if(light.enabled && light.shadows == LightShadows.Hard){
								shadowed_lights.Add(light);
						}
				}
				tiles.Add(where);
		}

		void Awake(){
				main_camera = GameObject.Find("Main Camera").transform;
		}

		void Start () {
				tiles.Clear();
				SpawnTile(0,0.0f,true);
				for(int i=-3; i <= 3; ++i){
						CreateTileIfNeeded(i);
				}
		}

		void CreateTileIfNeeded(int which){
				bool found = false;
				foreach(int tile in tiles){
						if(tile == which){
							found = true;
						}
				}
				if(!found){
						//Debug.Log("Spawning tile: "+which);
						SpawnTile(which, Mathf.Min(0.6f,0.1f * Mathf.Abs(which)), false);
				}
		}


		void Update () {
				float tile_x = main_camera.position.z / 20 + 0.5f;
				for(int i=-2; i <= 2; ++i){
						CreateTileIfNeeded((int)(tile_x+i));
				}
				foreach(Light light in shadowed_lights){
						if(!light){
								Debug.Log("LIGHT IS MISSING");
						}
						if(light){
								shadowed_amount = Vector3.Distance(main_camera.position, light.gameObject.transform.position);
								shadow_threshold = Mathf.Min(30,light.range*2.0f);
								fade_threshold = shadow_threshold * 0.75f;
								if(shadowed_amount < shadow_threshold){
										light.shadows = LightShadows.Hard;
										light.shadowStrength = Mathf.Min(1.0f, 1.0f-(fade_threshold - shadowed_amount) / (fade_threshold - shadow_threshold));
								} else {
										light.shadows = LightShadows.None;
								}
						}
				}
		}
}
