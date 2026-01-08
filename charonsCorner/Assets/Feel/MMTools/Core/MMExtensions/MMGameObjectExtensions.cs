using UnityEngine;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Game object extensions
	/// </summary>
	public static class MMGameObjectExtensions
	{
		static List<Component> m_ComponentCache = new List<Component>();

		/// <summary>
		/// Grabs a component without allocating memory uselessly
		/// </summary>
		/// <param name="this"></param>
		/// <param name="componentType"></param>
		/// <returns></returns>
		public static Component MMGetComponentNoAlloc(this GameObject @this, System.Type componentType)
		{
			@this.GetComponents(componentType, m_ComponentCache);
			Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
			m_ComponentCache.Clear();
			return component;
		}

		/// <summary>
		/// Grabs a component without allocating memory uselessly
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="this"></param>
		/// <returns></returns>
		public static T MMGetComponentNoAlloc<T>(this GameObject @this) where T : Component
		{
			@this.GetComponents(typeof(T), m_ComponentCache);
			Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
			m_ComponentCache.Clear();
			return component as T;
		}

		/// <summary>
		/// Grabs a component on the object, or on its children objects, or on a parent, or adds it to the object if none were found
		/// </summary>
		/// <param name="this"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T MMGetComponentAroundOrAdd<T>(this GameObject @this) where T : Component
		{
			T component = @this.GetComponentInChildren<T>(true);
			if (component == null)
			{
				component = @this.GetComponentInParent<T>();    
			}
			if (component == null)
			{
				component = @this.AddComponent<T>();    
			}
			return component;
		}
		
		/// <summary>
		/// Gets the specified component on the object, or adds it and returns it if there isn't already one
		/// </summary>
		/// <param name="gameObject"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T MMGetOrAddComponent<T>(this GameObject @this) where T : Component
		{
			T component = @this.GetComponent<T>();
			if (component == null)
			{
				component = @this.AddComponent<T>();
			}
			return component;
		}
		
		/// <summary>
		/// Gets the specified component on the object, or adds it and returns it if there isn't already one
		/// </summary>
		/// <param name="gameObject"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static (T newComponent, bool createdNew) MMFindOrCreateObjectOfType<T>(this GameObject @this, string newObjectName, Transform parent, bool forceNewCreation = false) where T : Component
		{
			T searchedObject = (T)Object.FindAnyObjectByType(typeof(T));
			if ((searchedObject == null) || forceNewCreation)
			{
				GameObject newGo = new GameObject(newObjectName);
				newGo.transform.SetParent(parent);
				return (newGo.AddComponent<T>(), true);
			}
			else
			{
				return (searchedObject, false);	
			}
		}
		
		/// <summary>
		/// Instantiates a game object in a disabled state without triggering its Awake/OnEnable/Start methods.
		/// </summary>
		/// <param name="originalObject"></param>
		/// <param name="parent"></param>
		/// <param name="worldPositionStays"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T MMInstantiateDisabled<T>(T originalObject, Transform parent = null, bool worldPositionStays = false) where T : Object
		{
			if (!MMGetActiveState(originalObject))
			{
				return Object.Instantiate(originalObject, parent, worldPositionStays);
			}
			
			(GameObject coreObject, Transform coreObjectTransform) = MMCreateDisabledObject(parent);
			T instance = Object.Instantiate(originalObject, coreObjectTransform, worldPositionStays);
			MMSetActiveState(instance, false);
			MMSetParent(instance, parent, worldPositionStays);
			Object.Destroy(coreObject);
			return instance;
		}
		
		/// <summary>
		/// Instantiates a game object in a disabled state without triggering its Awake/OnEnable/Start methods.
		/// </summary>
		/// <param name="originalObject"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <param name="parent"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T MMInstantiateDisabled<T>(T originalObject, Vector3 position, Quaternion rotation, Transform parent = null) where T : Object
		{
			if (!MMGetActiveState(originalObject))
			{
				return Object.Instantiate(originalObject, position, rotation, parent);
			}
		
			(GameObject newObject, Transform newObjectTransform) = MMCreateDisabledObject(parent);
			T instance = Object.Instantiate(originalObject, position, rotation, newObjectTransform);
			MMSetActiveState(instance, false);
			MMSetParent(instance, parent, false);
			Object.Destroy(newObject);
			return instance;
		}
		
		private static (GameObject coreObject, Transform newObjectTransform) MMCreateDisabledObject(Transform parent = null)
		{
			GameObject newObject = new GameObject(string.Empty);
			newObject.SetActive(false);
			Transform coreObjectTransform = newObject.transform;
			coreObjectTransform.SetParent(parent);

			return (newObject, coreObjectTransform);
		}

		private static bool MMGetActiveState<T>(T targetObject) where T : Object
		{
			switch (targetObject)
			{
				case GameObject gameObject:
				{
					return gameObject.activeSelf;
				}
				case Component component:
				{
					return component.gameObject.activeSelf;
				}
				default:
				{
					return false;
				}
			}
		}

		private static void MMSetActiveState<T>(T targetObject, bool newState) where T : Object
		{
			switch (targetObject)
			{
				case GameObject gameObject:
				{
					gameObject.SetActive(newState);
					break;
				}
				case Component component:
				{
					component.gameObject.SetActive(newState);
					break;
				}
			}
		}

		private static void MMSetParent<T>(T targetObject, Transform parent, bool worldPositionStays) where T : Object
		{
			switch (targetObject)
			{
				case GameObject gameObject:
				{
					gameObject.transform.SetParent(parent, worldPositionStays);
					break;
				}
				case Component component:
				{
					component.transform.SetParent(parent, worldPositionStays);
					break;
				}
			}
		}
	}
}