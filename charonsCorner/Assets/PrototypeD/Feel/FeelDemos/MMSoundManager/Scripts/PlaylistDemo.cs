using MoreMountains.Tools;
using UnityEngine;
#if MM_UGUI2
using TMPro;
#endif
#if MM_UI
namespace MoreMountains.Feel
{
	/// <summary>
	/// A small script used to power the FeelMMSoundManagerPlaylistManager demo scene
	/// </summary>
	[AddComponentMenu("")]
	public class PlaylistDemo : MonoBehaviour
	{
		/// the playlist manager to read data on
		public MMSMPlaylistManager PlaylistManager;
		/// a progress bar meant to display the progress of the song currently playing 
		public MMProgressBar ProgressBar;
		#if MM_UGUI2
		/// the name of the song currently playing
		public TMP_Text SongName;
		/// a text displaying the current progress of the song in minutes/seconds 
		public TMP_Text SongDuration;
		#endif

		/// <summary>
		/// On Update, updates the progress bar and song duration counter
		/// </summary>
		protected virtual void Update()
		{
			if (PlaylistManager.CurrentClipDuration == 0f)
			{
				ProgressBar.SetBar(0f, 0f, 1f);
			}
			else
			{
				ProgressBar.SetBar(PlaylistManager.CurrentTime, 0f, PlaylistManager.CurrentClipDuration);
				#if MM_UGUI2
				SongDuration.text = MMTime.FloatToTimeString(PlaylistManager.CurrentTime, false, true, true, false)
				                    + " / "
				                    + MMTime.FloatToTimeString(PlaylistManager.CurrentClipDuration, false, true, true, false);
				#endif
			}
		}

		/// <summary>
		/// Updates the song name display
		/// </summary>
		protected virtual void UpdateSongName()
		{
			int displayIndex = PlaylistManager.CurrentSongIndex + 1;
			#if MM_UGUI2
			SongName.text = displayIndex + ". " + PlaylistManager.CurrentSongName;
			#endif
		}
		
		/// <summary>
		/// When a new song starts to play, we update its name
		/// </summary>
		/// <param name="channel"></param>
		protected virtual void OnPlayEvent(int channel)
		{
			UpdateSongName();
		}
		
		/// <summary>
		/// Starts listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			MMPlaylistNewSongStartedEvent.Register(OnPlayEvent);
		}
		
		/// <summary>
		/// Stops listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			MMPlaylistNewSongStartedEvent.Unregister(OnPlayEvent);
		}
	}
}
#endif
