using UnityEngine;

namespace Aspen.Tools.FileSync
{
	public class FileSyncAPI : MonoBehaviour
	{
		public static void SyncAll()
		{
			SyncModels();
			SyncAnimations();
		}
		
		public static void SyncModels()
		{
			print("Syncing models");
		}
		
		public static void SyncAnimations()
		{
			print("Syncing animations");
		}
	}
}