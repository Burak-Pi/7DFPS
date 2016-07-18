using UnityEngine;
using System.Collections;

public class splashscript : MonoBehaviour {

		public  Texture splash_credits, 
		splash_back, splash_text, splash_play;
		public enum SplashState{FADE_IN, WAIT, FADE_OUT};
		public SplashState state = SplashState.FADE_IN;
		public GameObject GameScene;
		private float fade_in = 0;
		private float fade_out = 0;
		private float fade_out_delay = 0;

		public AudioClip music_a, music_b, play_sound, stop_sound;

		private AudioSource audiosource_music_a, audiosource_music_b, audiosource_effect;

		void Awake () {
				Application.targetFrameRate = 60;
				StartCoroutine(LerpFadeIn());
		}

		void Start () {
				Time.timeScale=1;
				audiosource_music_a = gameObject.AddComponent<AudioSource>();
				audiosource_music_a.loop = true;
				audiosource_music_a.clip = music_a;
				audiosource_music_a.volume = PlayerPrefs.GetFloat("music_volume");
				audiosource_music_b = gameObject.AddComponent<AudioSource>();
				audiosource_music_b.loop = true;
				audiosource_music_b.clip = music_b;
				audiosource_music_b.volume = PlayerPrefs.GetFloat("music_volume");
				audiosource_music_b.Play();
				audiosource_music_a.Play();
				audiosource_effect = gameObject.AddComponent<AudioSource>();
				audiosource_effect.PlayOneShot(play_sound, PlayerPrefs.GetFloat("sound_volume", 1));
				audiosource_effect.volume = PlayerPrefs.GetFloat("sound_volume");
		}
		float perc =0, dur = 5, startThyme=0;
		IEnumerator LerpFadeIn(){
				yield return new WaitForSeconds(1);
				print("waited");
				startThyme=Time.time;
				while (fade_in<dur) {
						fade_in = Mathf.Lerp(0, dur, perc);
						perc = (Time.time-startThyme)/dur;
						print("Fade in lerping " + fade_in + " Start " + startThyme + " curr " + Time.time + "perc " + perc);
						yield return null;
				}
				print("Fin");
		}
		void Update () {
				
				/*fade_in += Mathf.MoveTowards(fade_in, 5, Time.deltaTime);*/
				//fade_in += Time.deltaTime;
				if(state == SplashState.FADE_OUT){
						fade_out = Mathf.Min(1, fade_in + Time.deltaTime * 2);
						fade_out_delay += Time.deltaTime;
				}
				AudioListener.volume = PlayerPrefs.GetFloat("master_volume", 1);
		}
		Color color;float scale;
		void OnGUI(){
				color = new Color(fade_in,fade_in,fade_in,1);
				color.r *= 1 - fade_out;
				color.g *= 1 - fade_out;
				color.b *= 1 - fade_out;
				GUI.color = color;
				//GUI.DrawTexture(Rect(0,0,Screen.width, Screen.height),splash_back, ScaleMode.ScaleAndCrop, false);
				scale = 1 + (Mathf.Sin(Time.time*0.5f)+1)*0.1f;
				GUI.DrawTexture(new Rect(Screen.width*(1-scale)*0.5f,Screen.height*(1-scale)*0.5f,Screen.width*scale,Screen.height*scale), splash_back, ScaleMode.ScaleAndCrop, true);
				GUI.DrawTexture(new Rect(Screen.width*0.125f, Screen.height * 0.3f - splash_text.height*0.5f, Screen.width*0.75f,splash_text.height), splash_text, ScaleMode.ScaleToFit, true);
				GUI.color *= new Vector4(1,1,1,(fade_in-1.6f)*4);
				GUI.DrawTexture(new Rect(Screen.width*0.5f - splash_play.width, 
						Screen.height * 0.7f - splash_play.height*0.5f, 
						splash_play.width*2.0f,
						splash_play.height),
						splash_play, ScaleMode.ScaleToFit, true);
				GUI.color = new Color(color.r, color.g, color.b, 0.35f * (fade_in-3.6f)*4);
				GUI.DrawTexture(new Rect(Screen.width*0.125f, Screen.height * 0.9f - splash_credits.height*0.5f, Screen.width*0.75f,splash_credits.height), splash_credits, ScaleMode.ScaleToFit, true);

				if(state != SplashState.FADE_OUT){
						if (Event.current.type == EventType.KeyDown ||
								Event.current.type == EventType.MouseDown)
						{
								state = SplashState.FADE_OUT;
								audiosource_effect.PlayOneShot(stop_sound, PlayerPrefs.GetFloat("sound_volume", 1));
								audiosource_music_a.Stop();
								audiosource_music_b.Stop();
						}
				}
				if(fade_out_delay >= 0.2){
						UnityEngine.SceneManagement.SceneManager.LoadScene("scene");
				}	
		}
}
