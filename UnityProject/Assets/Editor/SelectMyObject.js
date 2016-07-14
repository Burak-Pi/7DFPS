#pragma strict

@MenuItem ("Tools/Select My Object")
static function SelectMyObject() {
	var obj = GameObject.Find("your object here from the hierarchy window");
	EditorGUIUtility.PingObject(obj);
	Selection.activeGameObject = obj;
}
 
 
// if you want to find a specific object identified by its class, like when an object has a RaceManager.js script attached to it.
@MenuItem ("Tools/Select My Specific Object js")
static function SelectMySpecificObject() {
//	var obj = GameObject.FindObjectsOfType(AimScript);

//	for(var script_ : AimScript in obj){
//		EditorGUIUtility.PingObject(script_);
//		Selection.activeGameObject = script_.gameObject;
//
//	}

}