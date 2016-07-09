using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelCreatorScript : MonoBehaviour {

		GameObject[] level_tiles;
		List<Light> shadowed_lights = new List<Light>();
		List<int> tiles = new List<int>();

		float shadowed_amount, shadow_threshold, fade_threshold;
		Transform main_camera;

		void SpawnTile(int where,float challenge , bool player){
				GameObject level_obj = level_tiles[Random.Range(0,level_tiles.Length)];
				GameObject level = new GameObject(level_obj.name + " (Clone)");
				foreach (Transform child in level_obj.transform){
						if(child.gameObject.name != "enemies" && child.gameObject.name != "player_spawn" && child.gameObject.name != "items"){
								GameObject child_obj = Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition, child.localRotation) as GameObject;
								child_obj.transform.parent = level.transform;
						}
				}
				var enemies = level_obj.transform.FindChild("enemies");
				if(enemies){
						for(var child : Transform in enemies){
								if(Random.Range(0.0,1.0) <= challenge){
										child_obj = Instantiate(child.gameObject, Vector3(0,0,where*20) + child.localPosition + enemies.localPosition, child.localRotation);
										child_obj.transform.parent = level.transform;
								}
						}
				}
				var items = level_obj.transform.FindChild("items");
				if(items){
						for(var child : Transform in items){
								if(Random.Range(0.0,1.0) <= (player?challenge+0.3:challenge)){
										child_obj = Instantiate(child.gameObject, Vector3(0,0,where*20) + child.localPosition + items.localPosition, items.localRotation);
										child_obj.transform.parent = level.transform;
								}
						}
				}
				if(player){
						var players = level_obj.transform.FindChild("player_spawn");
						if(players){
								var num = 0;
								for(var child : Transform in players){
										++num;
								}
								var save = Random.Range(0,num);
								var j=0;
								for(var child : Transform in players){
										if(j == save){
												child_obj = Instantiate(child.gameObject, Vector3(0,0,where*20) + child.localPosition + players.localPosition, child.localRotation);
												child_obj.transform.parent = level.transform;
												child_obj.name = "Player";
										}
										++j;
								}
						}
				}
				level.transform.parent = this.gameObject.transform;

				var lights = GetComponentsInChildren(Light);
				for(var light : Light in lights){
						if(light.enabled && light.shadows == LightShadows.Hard){
								shadowed_lights.push(light);
						}
				}
				tiles.Add(where);
		}

		void Awake(){
				main_camera = GameObject.Find("Main Camera").transform;
		}

		void Start () {
				shadowed_lights = new Array();
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
				int tile_x = main_camera.position.z / 20 + 0.5f;
				for(int i=-2; i <= 2; ++i){
						CreateTileIfNeeded(tile_x+i);
				}
				for(Light light in shadowed_lights){
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
