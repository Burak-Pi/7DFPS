// MyScriptEditor.cs
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

//[CustomEditor(typeof(GUISkinHolder_))]
public class FindScritps : EditorWindow {

		// if you want to find a specific object identified by its class, like when an object has a RaceManager.js script attached to it.
		[MenuItem ("Tools/Find Scripts")]
		static void SelectMySpecificObject() {
				/*
				foreach (GameObject item in Selection.gameObjects) {
						DestroyImmediate(item.GetComponent<GUISkinHolder>());
					//	item.AddComponent(typeof());

				}*/
		}

		static ReplaceScripts window;
		[MenuItem ("Tools/Open Script Swap Window %#l")]
		static void Init () {
				// Get existing open window or if none, make a new one:
				window = (ReplaceScripts)EditorWindow.GetWindow (typeof (ReplaceScripts));
				window.Show();
		}

}

public class ReplaceScripts : EditorWindow {
		int activeScripts=0, convertionNumberTotal = 0;
		void HexplodinateAllTehScripts(){


				if (Selection.gameObjects.Length==0) {
						ShowNotification(new GUIContent("Select some GOs brah!"));
				}
				else if (objList[0]==null) {
						ShowNotification(new GUIContent("Select some C# scripts brah!"));
				}
				activeScripts=0;

				for (int i = 0; i < objList.Count; i++) {
						if (objList[i]!=null) {
								activeScripts++;
						} else {
								break;
						}
				}
				Debug.Log("Outside " + activeScripts);
				convertionNumberTotal=0;
				foreach (GameObject item in Selection.gameObjects) {
						Debug.Log("intside");

						MonoBehaviour[] comp = item.GetComponents<MonoBehaviour>();
						//									int scriptLocation =-1;

						//Search through all scripts attached to this GO
						for (int i = 0; i < comp.Length; i++) {

								if (comp[i]==null) {
										continue;
								}

								//Check the type of script this be against the C# scripts we have

								for (int j = 0; j < activeScripts; j++) {
										MonoScript objConvertedToMonoScript = (MonoScript)objList[j];
										Debug.Log("Obj is  "+ objConvertedToMonoScript.GetClass() + " him is " + comp[i].GetType());
										//Debug.Log(comp[i].GetType() + " " + objConvertedToMonoScript.GetClass() + " Checl " + (comp[i].GetType()==objConvertedToMonoScript.GetClass()) + " string " +
										//												comp[i].GetType().ToString() + " other " + objConvertedToMonoScript.GetClass().ToString() + " string manual check " + (comp[i].GetType().ToString()==objConvertedToMonoScript.GetClass().ToString()));
										//										if (comp[i] && objConvertedToMonoScript.GetClass()!=null) {

										if (comp[i].GetType().ToString()==objConvertedToMonoScript.GetClass().ToString()) {
												//REMOVE OLD SCRIPT AND REPLACE IT WITH THE NEW ONE
												UnityEditorInternal.ComponentUtility.CopyComponent(comp[i]);
												//														Component tempCOMP = item.AddComponent(newScriptMono.GetClass());
												Component tempCOMP = item.AddComponent(objConvertedToMonoScript.GetClass());
												//EditorUtility.CopySerialized(comp[i], tempCOMP);

												UnityEditorInternal.ComponentUtility.PasteComponentValues(tempCOMP);

												DestroyImmediate(comp[i]);
												convertionNumberTotal++;
												//														item.AddComponent(newScriptMono.GetClass());
										}
										//										}
								}

						}
						//								DestroyImmediate(item.GetComponent();
						//	item.AddComponent(typeof());

				}
				Debug.Log("Total files converted be: " + convertionNumberTotal);
		}


		void InitialiseCSharpScripts(){
				//newSerialInfo = new System.Runtime.Serialization.SerializationInfo(MonoBehaviour, System.Runtime.Serialization.IFormatterConverter);
				//				streamingContext	= new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All);
				//fileInfo = info.GetFiles();
				//allCSharpScripts = info.GetType();

		}


		//		DirectoryInfo info = new DirectoryInfo("Assets/Scripts/Scripts C#/");
		//		FileInfo[] fileInfo;

		bool groupEnabled;
		float myFloat = 1.23f;
		UnityEngine.Object oldScript, newScript;
		UnityEngine.Object[] allCSharpScripts = new Object[24];
		UnityEngine.Object allCSharpScriptsSolo;
		MonoScript oldScriptMono, newScriptMono;
		//		System.Runtime.Serialization.SerializationInfo newSerialInfo;
		//		System.Runtime.Serialization.StreamingContext streamingContext;
		int numberOfScripts =24;
		bool parseFail, toggleCSharp =true;
		string tempString ="0";
		List<UnityEngine.Object> objList = new List<Object>();
		float yPos;
		void OnGUI () {
				GUILayout.Label ("Auto-Swap Scripts", EditorStyles.boldLabel);
				EditorGUILayout.Space();

				//				GUILayout.Label ("New C# Scripts", EditorStyles.label);

				//tempString = GUILayout.TextField("Number of scripts", tempString);
				//numberOfScripts = int.Parse(tempString);
				//				parseFail = int.TryParse(tempString, out numberOfScripts);

				//for (int i = 0; i < 2; i++) {
				//								oldScript = EditorGUILayout.ObjectField(oldScript, typeof(Object), true);
				toggleCSharp = EditorGUILayout.Foldout(toggleCSharp, tempString);
				if(toggleCSharp){
						if (tempString!="New C# Scripts Open") {
								tempString="New C# Scripts Open";
						}
						for (int i = 0; i < numberOfScripts; i++) {
								if (objList.Count<numberOfScripts) {
										objList.Add(EditorGUILayout.ObjectField(oldScript, typeof(Object), true));
								}
								EditorGUILayout.BeginHorizontal();
								objList[i] = EditorGUILayout.ObjectField(objList[i], typeof(Object), true);
								EditorGUILayout.EndHorizontal();


						}
						/*
						if(Selection.activeTransform) {
								Selection.activeTransform.position = 
										EditorGUILayout.Vector3Field("Position", Selection.activeTransform.position);
						tempString = Selection.activeTransform.name;
							


						}

						else {
							tempString = "New C# Scripts Closed";
							toggleCSharp = true;	
						}*/



				}
				else if (tempString!="New C# Scripts") {
						tempString="New C# Scripts";
				}
				//	}









				EditorGUILayout.Space();
				EditorGUILayout.Space();




				#region Object field
				//				GUILayout.Label ("All C# Scripts", EditorStyles.boldLabel);

				GUILayout.Label ("Currently selected gameObject count: " + Selection.gameObjects.Length, EditorStyles.boldLabel);

				//						
				//				GUILayout.BeginArea(new Rect(((position.width/2)-position.width/8), 50
				//						,position.width, position.height));
				//				GUILayout.Label ("Auto-Swap Scripts", EditorStyles.boldLabel);

				if (GUILayout.Button("Giant Button of D00M!", GUILayout.Height(position.height/8),GUILayout.Width(position.width/1.01f))){
						HexplodinateAllTehScripts();
						/*if (fileInfo==null) {
								InitialiseCSharpScripts();
						}
						foreach (FileInfo item in fileInfo) {
								//item.GetObjectData(newSerialInfo,streamingContext);
							//	newScriptMono = (MonoScript)item;

								Debug.Log("Type is " + newSerialInfo.GetType());
						}*/

				}
				//				GUILayout.EndArea();
				EditorGUILayout.BeginHorizontal();
				//	allCSharpScripts = EditorGUILayout.ObjectField(allCSharpScripts, typeof(Object[]), true) as MonoScript;
				EditorGUILayout.EndHorizontal();

				#region Replace Code
				/*	GUILayout.Label ("Old Script", EditorStyles.boldLabel);

				EditorGUILayout.BeginHorizontal();
				oldScript = EditorGUILayout.ObjectField(oldScript, typeof(Object), true) as MonoScript;
				EditorGUILayout.EndHorizontal();
				GUILayout.Label ("New Script", EditorStyles.boldLabel);
				EditorGUILayout.BeginHorizontal();
				newScript = EditorGUILayout.ObjectField(newScript, typeof(Object), true);
				EditorGUILayout.EndHorizontal();

				if (GUILayout.Button("Replace!")){
					if (oldScript == null)
							ShowNotification(new GUIContent("No object selected for searching"));
					else if (!newScript) {
							ShowNotification(new GUIContent("No new object selected for searching"));
					}
					else {
								Debug.Log("Outside");
								oldScriptMono = (MonoScript)oldScript;
								newScriptMono = (MonoScript)newScript;
							foreach (GameObject item in Selection.gameObjects) {
										Debug.Log("intside");

										MonoBehaviour[] comp = item.GetComponents<MonoBehaviour>();
//									int scriptLocation =-1;
									for (int i = 0; i < comp.Length; i++) {
												
												if (comp[i].GetType()==oldScriptMono.GetClass()) {
														Debug.Log("Same " + comp[i].GetType() + " checking for " + oldScript + " type opf " + oldScript.GetType());



														//REMOVE OLD SCRIPT AND REPLACE IT WITH THE NEW ONE
														UnityEditorInternal.ComponentUtility.CopyComponent(comp[i]);
//														Component tempCOMP = item.AddComponent(newScriptMono.GetClass());
														Component tempCOMP = item.AddComponent(newScriptMono.GetClass());
														//EditorUtility.CopySerialized(comp[i], tempCOMP);

														UnityEditorInternal.ComponentUtility.PasteComponentValues(tempCOMP);

//														DestroyImmediate(comp[i]);
//														item.AddComponent(newScriptMono.GetClass());



											}
												else 	Debug.Log("nope " + comp[i].GetType() + " checking for " + oldScriptMono.GetClass() + " type opf " + oldScript.GetType());
									}
	//								DestroyImmediate(item.GetComponent();
									//	item.AddComponent(typeof());

							}
					}
				}*/
				#endregion

				#endregion

				/*
				groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
				myBool = EditorGUILayout.Toggle ("Toggle", myBool);
				myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
				EditorGUILayout.EndToggleGroup ();*/
		}
}






//Component[] obj = FindObjectsOfType<GUISkinHolder_>();

//Selection.objects = obj;

//				Selection.activeGameObject = 
/*	foreach (GUISkinHolder_ item in obj) {
						EditorGUIUtility.PingObject(item);
//						Selection.activeGameObject = item.gameObject;
				}*/

//UnityEngine.Object[] gos = UnityEngine.GameObject.FindObjectsOfType(typeof(GUISkinHolder_));
//				Selection.activeGameObject = gos[1];
//Selection.objects= gos;
//				Selection.gameObjects = gos;
//	GameObject[] go = (GameObject) GameObject.FindObjectsOfType(typeof( GUISkinHolder_));

//GameObject bla = item;
//bla.SetActive(false);
//Debug.Log("Shello? " + (GameObject)item);