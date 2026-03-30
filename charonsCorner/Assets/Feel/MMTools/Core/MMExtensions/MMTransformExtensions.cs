using UnityEngine;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Transform extensions
	/// </summary>
	public static class TransformExtensions
	{
		/// <summary>
		/// Destroys a transform's children
		/// </summary>
		/// <param name="transform"></param>
		public static void MMDestroyAllChildren(this Transform transform)
		{
			for (int t = transform.childCount - 1; t >= 0; t--)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(transform.GetChild(t).gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(transform.GetChild(t).gameObject);
				}
			}
		}

		/// <summary>
		/// Finds children by name, breadth first
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="transformName"></param>
		/// <returns></returns>
		public static Transform MMFindDeepChildBreadthFirst(this Transform parent, string transformName)
		{
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(parent);
			while (queue.Count > 0)
			{
				Transform child = queue.Dequeue();
				if (child.name == transformName)
				{
					return child;
				}
				foreach (Transform t in child)
				{
					queue.Enqueue(t);
				}
			}
			return null;
		}

		/// <summary>
		/// Finds children by name, depth first
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="transformName"></param>
		/// <returns></returns>
		public static Transform MMFindDeepChildDepthFirst(this Transform parent, string transformName)
		{
			foreach (Transform child in parent)
			{
				if (child.name == transformName)
				{
					return child;
				}

				Transform result = child.MMFindDeepChildDepthFirst(transformName);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		/// <summary>
		/// Changes the layer of a transform and all its children to the new one
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="layerName"></param>
		public static void ChangeLayersRecursively(this Transform transform, string layerName)
		{
			transform.gameObject.layer = LayerMask.NameToLayer(layerName);
			foreach (Transform child in transform)
			{
				child.ChangeLayersRecursively(layerName);
			}
		}

		/// <summary>
		/// Changes the layer of a transform and all its children to the new one
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="layerIndex"></param>
		public static void ChangeLayersRecursively(this Transform transform, int layerIndex)
		{
			transform.gameObject.layer = layerIndex;
			foreach (Transform child in transform)
			{
				child.ChangeLayersRecursively(layerIndex);
			}
		}
		
		
		/// <summary>
		/// Enumerates all parents of a transform
		/// </summary>
		/// <param name="targetTransform"></param>
		/// <param name="includeSelf"></param>
		/// <returns></returns>
		public static IEnumerable<Transform> MMEnumerateAllParents(this Transform targetTransform, bool includeSelf = false)
		{
			if (!includeSelf)
			{
				targetTransform = targetTransform?.parent;
			}
			while (targetTransform)
			{
				yield return targetTransform;
				targetTransform = targetTransform.parent;
			}
		}
	}
}