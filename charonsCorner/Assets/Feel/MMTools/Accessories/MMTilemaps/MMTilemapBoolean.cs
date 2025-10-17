using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
	[AddComponentMenu("More Mountains/Tools/Tilemaps/MM Tilemap Boolean")]
	public class MMTilemapBoolean : MonoBehaviour
	{
		#if MM_PHYSICS2D
		
		public Tilemap TilemapToClean;

		[MMInspectorButton("BooleanClean")]
		public bool BooleanCleanButton;

		protected Tilemap _tilemap;

		/// <summary>
		/// This method will copy the reference tilemap into the one on this gameobject
		/// </summary>
		public virtual void BooleanClean()
		{
			if (TilemapToClean == null)
			{
				return;
			}

			_tilemap = this.gameObject.GetComponent<Tilemap>();

			// we grab all filled positions from the ref tilemap
			foreach (Vector3Int pos in _tilemap.cellBounds.allPositionsWithin)
			{
				Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
				if (_tilemap.HasTile(localPlace))
				{
					if (TilemapToClean.HasTile(localPlace))
					{
						TilemapToClean.SetTile(localPlace, null);
					}
				}                
			}
			// we clear our tilemap and resize it
			_tilemap.RefreshAllTiles();            
		}

		#endif
	}
}