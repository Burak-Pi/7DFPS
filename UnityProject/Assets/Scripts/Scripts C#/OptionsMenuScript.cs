﻿using UnityEngine;
using System.Collections;

public class OptionsMenuScript : MonoBehaviour {

		GUISkin skin;
		Rect windowRect;
		float menu_width = 300, menu_height = 500;
		bool show_menu;

		void OnApplicationPause() {  
			Screen.lockCursor = false;
		}

		void OnApplicationFocus() {
			if(!show_menu){
				Screen.lockCursor = true;
			}
		}

		void ShowMenu(){
			show_menu = true;
		}

		void HideMenu(){
			show_menu = false;
		}

		//On Gui is constantly updating, so cache local variables within the functions it calls
		void OnGUI () {
			if(!show_menu){
					return;
			}
			windowRect = GUI.Window (
					0, 
					Rect(Screen.width*0.5f - menu_width*0.5f, Screen.height*0.5f - menu_height*0.5f, menu_width, menu_height), 
						WindowVoid(), 
					"",
					skin.window);
		}

		private Vector2 draw_cursor, draw_cursor_line;
		private float line_height = 24, line_offset = 24;

		void DrawCursor_Init() {
				draw_cursor = Vector2(25,25);
				draw_cursor_line = Vector2(0,0);	
		}

		void DrawCursor_NextLine() {
				draw_cursor_line = Vector2(0,0);	
				draw_cursor.y += line_offset;
		}

		void DrawCursor_Offset(float val) {
				draw_cursor_line.x += val;	
		}

		Vector2 DrawCursor_Get() {
				return draw_cursor + draw_cursor_line;
		}
		Rect rect;
		Rect DrawCursor_RectSpace(float width){
				rect = new Rect(draw_cursor.x + draw_cursor_line.x,
						draw_cursor.y + draw_cursor_line.y,
						width,
						line_height);
				DrawCursor_Offset(width);
				return rect;
		}

		void DrawLabel(string _text) {
				DrawCursor_Offset(17);
				GUI.Label (
						DrawCursor_RectSpace(400),//GUI.skin.label.CalcSize(new GUIContent(text)).x), 
						_text, 
						skin.label);
		}

		bool DrawCheckbox(bool val,string text) {
				val = GUI.Toggle (
						DrawCursor_RectSpace(400), 
						val,
						text, 
						skin.toggle);
				return val;			
		}

		float DrawSlider(float val){
				DrawCursor_Offset(18);
				val = GUI.HorizontalSlider (
						DrawCursor_RectSpace(400 - draw_cursor_line.x), 
						val,
						0.0f, 
						1.0f);
				return val;			
		}

		bool DrawButton(string _text){
				var val = GUI.Button (
						DrawCursor_RectSpace(200), 
						_text,
						skin.button);
				return val;			
		}

		//private var brightness = 0.3;
		private float master_volume = 1, sound_volume = 1, music_volume = 1, voice_volume = 1, mouse_sensitivity = 0.2f
				, vert_scroll = 0, gun_distance = 1, vert_scroll_pixels = 0;
		private bool lock_gun_to_center, mouse_invert, show_advanced_sound, toggle_crouch = true;
		private Vector2 scroll_view_vector = Vector2.zero;

		void RestoreDefaults() {
				master_volume = 1;
				sound_volume = 1;
				music_volume = 1;
				voice_volume = 1;
				mouse_sensitivity = 0.2f;
				lock_gun_to_center = false;
				mouse_invert = false;
				toggle_crouch = true;
		}

		void Start() {
				Screen.lockCursor = true;
				RestoreDefaults();
				master_volume = PlayerPrefs.GetFloat("master_volume", master_volume);
				sound_volume = PlayerPrefs.GetFloat("sound_volume", sound_volume);
				music_volume = PlayerPrefs.GetFloat("music_volume", music_volume);
				voice_volume = PlayerPrefs.GetFloat("voice_volume", voice_volume);
				mouse_sensitivity = PlayerPrefs.GetFloat("mouse_sensitivity", mouse_sensitivity);
				lock_gun_to_center = PlayerPrefs.GetInt("lock_gun_to_center", lock_gun_to_center?1:0)==1;
				mouse_invert = PlayerPrefs.GetInt("mouse_invert", mouse_invert?1:0)==1;
				toggle_crouch = PlayerPrefs.GetInt("toggle_crouch", toggle_crouch?1:0)==1;      
				gun_distance = PlayerPrefs.GetFloat("gun_distance", 1.0);       
		}


		void SavePrefs() {
				PlayerPrefs.SetFloat("master_volume", master_volume);
				PlayerPrefs.SetFloat("sound_volume", sound_volume);
				PlayerPrefs.SetFloat("music_volume", music_volume);
				PlayerPrefs.SetFloat("voice_volume", voice_volume);
				PlayerPrefs.SetFloat("mouse_sensitivity", mouse_sensitivity);
				PlayerPrefs.SetInt("lock_gun_to_center", lock_gun_to_center?1:0);
				PlayerPrefs.SetInt("mouse_invert", mouse_invert?1:0);
				PlayerPrefs.SetInt("toggle_crouch", toggle_crouch?1:0);    
				PlayerPrefs.SetFloat("gun_distance", gun_distance);    
		}

		bool IsMenuShown(){
			return show_menu;
		}

		void Update() {
				if(vert_scroll != -1.0f){
						vert_scroll += Input.GetAxis("Mouse ScrollWheel");
				}
				if(Input.GetKeyDown ("escape") && !show_menu){
						Screen.lockCursor = false;
						ShowMenu();
				} else if(Input.GetKeyDown ("escape") && show_menu){
						Screen.lockCursor = true;
						HideMenu();
				}
				if(Input.GetMouseButtonDown(0) && !show_menu){
						Screen.lockCursor = true;
				}
				if(show_menu){
						Time.timeScale = 0;
				} else {
						if(Time.timeScale == 0){
							Time.timeScale = 1;
						}
				}
		}

		void WindowVoid (int windowID) {

				scroll_view_vector = GUI.BeginScrollView (
						Rect (0, 0, windowRect.width, windowRect.height), 
						scroll_view_vector, 
						Rect (0, vert_scroll_pixels, windowRect.width, windowRect.height));

				DrawCursor_Init();
				mouse_invert = DrawCheckbox(mouse_invert, "Invert mouse");
				DrawCursor_NextLine();
				DrawLabel("Mouse sensitivity:");
				DrawCursor_NextLine();
				mouse_sensitivity = DrawSlider(mouse_sensitivity);
				DrawCursor_NextLine();
				DrawLabel("Distance from eye to gun:");
				DrawCursor_NextLine();
				gun_distance = DrawSlider(gun_distance);
				DrawCursor_NextLine();
				toggle_crouch = DrawCheckbox(toggle_crouch, "Toggle crouch");
				DrawCursor_NextLine();
				lock_gun_to_center = DrawCheckbox(lock_gun_to_center, "Lock gun to screen center");
				DrawCursor_NextLine();
				/*DrawLabel("Brightness:");
	DrawCursor_NextLine();
	brightness = DrawSlider(brightness);
	DrawCursor_NextLine();*/
				DrawLabel("Master volume:");
				DrawCursor_NextLine();
				master_volume = DrawSlider(master_volume);
				DrawCursor_NextLine();
				show_advanced_sound = DrawCheckbox(show_advanced_sound, "Advanced sound options");
				DrawCursor_NextLine();
				if(show_advanced_sound){
						int indent = 44;
						DrawLabel("....Sounds:");
						DrawCursor_NextLine();
						DrawCursor_Offset(indent);
						sound_volume = DrawSlider(sound_volume);
						DrawCursor_NextLine();
						DrawLabel("....Voice:");
						DrawCursor_NextLine();
						DrawCursor_Offset(indent);
						voice_volume = DrawSlider(voice_volume);
						DrawCursor_NextLine();
						DrawLabel("....Music:");
						DrawCursor_NextLine();
						DrawCursor_Offset(indent);
						music_volume = DrawSlider(music_volume);
						DrawCursor_NextLine();
				}
				if(DrawButton("Resume")){
						Screen.lockCursor = true;
						show_menu = false;
				}
				draw_cursor.y += line_offset * 0.3f;
				DrawCursor_NextLine();
				if(DrawButton("Restore defaults")){
						RestoreDefaults();
				}
				DrawCursor_NextLine();	
				draw_cursor.y += line_offset * 0.3f;
				if(DrawButton("Exit game")){
						Application.Quit();
				}

				var content_height = draw_cursor.y;

				GUI.EndScrollView();

				if(content_height > windowRect.height){
						var leeway = (content_height / windowRect.height - windowRect.height / content_height);
						if(vert_scroll == -1.0f){
								vert_scroll = leeway;
						}
						vert_scroll = GUI.VerticalScrollbar (
								Rect (menu_width-20, 20, menu_width, menu_height-25), 
								vert_scroll, 
								windowRect.height / content_height,
								content_height / windowRect.height, 
								0);
						vert_scroll_pixels = windowRect.height * (leeway - vert_scroll);
				} else {
						vert_scroll = -1;
						vert_scroll_pixels = 0;
				}
				SavePrefs();
		}




}
