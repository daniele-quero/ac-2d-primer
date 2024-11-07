﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ConstantIDManager.cs"
 * 
 *	This script is used to store a record of all ConstantID components in the Hierarchy, as well as provide functions to retrieve them based on ID number.
 * 
 */
 
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace AC
{

	/** This script is used to store a record of all ConstantID components in the Hierarchy, as well as provide functions to retrieve them based on ID number. */
	public struct ConstantIDManager
	{

		#region Variables

		private HashSet<ConstantID> constantIDs;
		private HashSet<ConstantID> menuConstantIDs;

		#endregion


		#region PublicFunctions

		/**
		 * <summary>Gets a component in the Hierarchy that also has a ConstantID component on the same GameObject.</summary>
		 * <param name = "constantIDValue">The Constant ID number generated by the ConstantID component</param>
		 * <param name = "prioritisePersistentOrMainScene">If True, then components in the main scene, or those that survive scene-changes, will be prioritised in the search</param>
		 * <returns>The component with a matching Constant ID number</returns>
		 */
		public T GetComponent <T> (int constantIDValue, bool prioritisePersistentOrMainScene = true) where T : Component
		{
			if (!KickStarter.sceneChanger.SubScenesAreOpen ())
			{
				prioritisePersistentOrMainScene = false;
			}
			
			if (prioritisePersistentOrMainScene)
			{
				foreach (ConstantID constantID in ConstantIDs)
				{
					if (constantID.constantID != constantIDValue)
					{
						continue;
					}

					if (!constantID.gameObject.IsPersistent () && constantID.gameObject.scene != SceneChanger.CurrentScene)
					{
						continue;
					}

					T component = constantID.GetComponent <T>();
					if (component != null)
					{
						return component;
					}
				}
			}

			foreach (ConstantID constantID in ConstantIDs)
			{
				if (constantID == null || constantID.constantID != constantIDValue)
				{
					continue;
				}

				T component = constantID.GetComponent <T>();
				if (component != null)
				{
					return component;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets a component in the Hierarchy that also has a ConstantID component on the same GameObject.</summary>
		 * <param name = "constantIDValue">The Constant ID number generated by the ConstantID component</param>
		 * <param name = "scene">The scene to search within for the component</param>
		 * <param name = "sceneOnlyPrioritises">If True, then the supplied scene is searched first, but all other scenes are then searched if no result is yet found</param>
		 * <returns>The component with a matching Constant ID number</returns>
		 */
		public T GetComponent <T> (int constantIDValue, Scene scene, bool sceneOnlyPrioritises = false) where T : Component
		{
			if (!KickStarter.sceneChanger.SubScenesAreOpen ())
			{
				return GetComponent <T> (constantIDValue);
			}

			if (sceneOnlyPrioritises)
			{
				foreach (ConstantID constantID in ConstantIDs)
				{
					if (constantID.constantID != constantIDValue)
					{
						continue;
					}

					if (constantID.gameObject.scene != scene)
					{
						continue;
					}

					T component = constantID.GetComponent <T>();
					if (component != null)
					{
						return component;
					}
				}
			}

			foreach (ConstantID constantID in ConstantIDs)
			{
				if (constantID.constantID != constantIDValue)
				{
					continue;
				}

				if (!sceneOnlyPrioritises && constantID.gameObject.scene != scene)
				{
					continue;
				}

				T component = constantID.GetComponent <T>();
				if (component != null)
				{
					return component;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets a ConstantID component ID number, in a particular scene</summary>
		 * <param name = "constantIDValue">The ID number to search for</param>
		 * <param name = "scene">The scene to search</param>
		 * <param name = "sceneOnlyPrioritises">If True, then the supplied scene is searched first, but all other scenes are then searched if no result is yet found</param>
		 * <returns>The ConstantID component associated with the ID number</returns>
		 */
		public ConstantID GetConstantID (int constantIDValue, Scene scene, bool sceneOnlyPrioritises = false)
		{
			if (!KickStarter.sceneChanger.SubScenesAreOpen ())
			{
				foreach (ConstantID constantID in ConstantIDs)
				{
					if (constantID.constantID != constantIDValue)
					{
						continue;
					}

					return constantID;
				}
				return null;
			}

			if (sceneOnlyPrioritises)
			{
				foreach (ConstantID constantID in ConstantIDs)
				{
					if (constantID.constantID != constantIDValue)
					{
						continue;
					}

					if (constantID.gameObject.scene != scene)
					{
						continue;
					}

					return constantID;
				}
			}

			foreach (ConstantID constantID in ConstantIDs)
			{
				if (constantID.constantID != constantIDValue)
				{
					continue;
				}

				if (!sceneOnlyPrioritises && constantID.gameObject.scene != scene)
				{
					continue;
				}

				return constantID;
			}
			return null;
		}


		/**
		 * <summary>Gets all components in the Hierarchy that also have a ConstantID component on the same GameObject.</summary>
		 * <param name = "constantIDValue">The Constant ID number generated by the ConstantID component</param>
		 * <returns>The components with a matching Constant ID number</returns>
		 */
		public HashSet<T> GetComponents <T> (int constantIDValue) where T : Component
		{
			HashSet<T> components = new HashSet<T>();

			foreach (ConstantID constantID in ConstantIDs)
			{
				if (constantID.constantID != constantIDValue)
				{
					continue;
				}

				T[] _components = constantID.gameObject.GetComponents <T>();
				foreach (T component in _components)
				{
					components.Add (component);
				}
			}

			return components;
		}


		/**
		 * <summary>Gets all components of a particular type within the scene, with a given ConstantID number.</summary>
		 * <param name = "constantIDValue">The ID number to search for</param>
		 * <param name = "scene">The scene to search</param>
		 * <returns>All components of the give type in the scene, provided they have an associated ConstantID</returns>
		 */
		public HashSet<T> GetComponents <T> (int constantIDValue, Scene scene) where T : Component
		{
			if (!KickStarter.sceneChanger.SubScenesAreOpen ())
			{
				return GetComponents <T> (constantIDValue);
			}

			HashSet<T> components = new HashSet<T>();

			foreach (ConstantID constantID in ConstantIDs)
			{
				if (constantID.constantID != constantIDValue)
				{
					continue;
				}

				if (constantID.gameObject.scene != scene)
				{
					continue;
				}

				T[] _components = constantID.gameObject.GetComponents <T>();
				foreach (T component in _components)
				{
					components.Add (component);
				}
			}

			return components;
		}


		/**
		 * <summary>Gets all components of a given type within a scene.  The components must have a ConstantID or Remember component attached in order to be included in the returned result</summary>
		 * <param name = "scene">The scene to search</param>
		 * <returns>All components of a given type within the scene, provided they have an associated ConstantID</returns>
		 */
		public HashSet<T> GetComponents <T> (Scene scene) where T : Component
		{
			HashSet<T> components = new HashSet<T>();

			foreach (ConstantID constantID in ConstantIDs)
			{
				if (constantID == null || constantID.gameObject == null || constantID.gameObject.scene != scene)
				{
					continue;
				}

				T[] _components = constantID.gameObject.GetComponents <T>();
				foreach (T component in _components)
				{
					components.Add (component);
				}
			}
			
			return components;
		}


		/**
		 * <summary>Gets all scene-surviving components of a particular type, provided that they are not associated with a Player character.</summary>
		 * <returns>All scene-surviving components of the give type</returns>
		 */
		public HashSet<T> GetPersistentButNotPlayerComponents <T> () where T : Component
		{
			HashSet<T> components = new HashSet<T>();

			foreach (ConstantID constantID in ConstantIDs)
			{
				if (constantID == null || constantID.gameObject == null) continue;
				if (!constantID.gameObject.IsPersistent ()) continue;
				if (constantID.gameObject.GetComponent <Player>()) continue;
				if (constantID.gameObject.GetComponentInParent <Player>()) continue;

				T[] _components = constantID.gameObject.GetComponents <T>();
				foreach (T component in _components)
				{
					components.Add (component);
				}
			}

			foreach (ConstantID menuConstantID in MenuConstantIDs)
			{
				if (menuConstantID == null || menuConstantID.gameObject == null) continue;
				T[] _components = menuConstantID.gameObject.GetComponents<T> ();
				foreach (T component in _components)
				{
					components.Add (component);
				}
			}

			return components;
		}


		/**
		 * <summary>Gets a ConstantID component in the Hierarchy</summary>
		 * <param name = "constantIDValue">The Constant ID number to search for</param>
		 * <param name = "prioritisePersistentOrMainScene">If True, then components in the main scene, or those that survive scene-changes, will be prioritised in the search</param>
		 * <returns>The component with a matching Constant ID number</returns>
		 */
		public ConstantID GetConstantID (int constantIDValue, bool prioritisePersistentOrMainScene = true)
		{
			if (!KickStarter.sceneChanger.SubScenesAreOpen ())
			{
				prioritisePersistentOrMainScene = false;
			}

			if (prioritisePersistentOrMainScene)
			{
				foreach (ConstantID constantID in ConstantIDs)
				{
					if (constantID == null || constantID.gameObject == null)
					{
						continue;
					}

					if (constantID.constantID != constantIDValue)
					{
						continue;
					}
					if (!constantID.gameObject.IsPersistent () && constantID.gameObject.scene != SceneChanger.CurrentScene)
					{
						continue;
					}
					return constantID;
				}
			}

			foreach (ConstantID constantID in ConstantIDs)
			{
				if (constantID.constantID != constantIDValue)
				{
					continue;
				}
				return constantID;
			}
			return null;
		}


		/**
		 * <summary>Registers a ConstantID component in the Hierarchy</summary>
		 * <param name = "constantID">The ConstantID to register</param>
		 */
		public void Register (ConstantID constantID)
		{
			if (constantID is Remember)
			{
				Canvas canvas = constantID.transform.root.GetComponent <Canvas>();
				if (canvas && canvas.gameObject.IsPersistent () && !MenuConstantIDs.Contains (constantID))
				{
					MenuConstantIDs.Add (constantID);
					return;
				}
			}

			ConstantIDs.Add (constantID);
		}


		/**
		 * <summary>Unregisters a ConstantID component</summary>
		 * <param name = "constantID">The ConstantID to unregister</param>
		 */
		public void Unregister (ConstantID constantID)
		{
			if (constantID is Remember)
			{
				Canvas canvas = constantID.transform.root.GetComponent<Canvas> ();
				if (canvas && canvas.gameObject.IsPersistent ())
				{
					// Always keep UI ConstantIDs registered
					return;
				}
			}

			ConstantIDs.Remove (constantID);
		}

		#endregion


		#region GetSet

		/** All ConstantID components recorded */
		public HashSet<ConstantID> ConstantIDs
		{
			get
			{
				if (constantIDs == null) constantIDs = new HashSet<ConstantID>();
				return constantIDs;
			}
		}


		/** All ConstantID components recorded that are part of Unity UI-based menus */
		public HashSet<ConstantID> MenuConstantIDs
		{
			get
			{
				if (menuConstantIDs == null) menuConstantIDs = new HashSet<ConstantID> ();
				return menuConstantIDs;
			}
		}

		#endregion

	}

}