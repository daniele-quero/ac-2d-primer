﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"Objective.cs"
 * 
 *	Stores data for an Objective, which can be viewed/read in a Menu
 * 
 */

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** Stores data for an Objective, which can be viewed/read in a Menu */
	[System.Serializable]
	public class Objective : ITranslatable
	{

		#region Variables

		/** A unique identifier */
		public int ID;
		[SerializeField] protected string title;
		/** The translation ID for the title text, generated by the Speech Manager */
		public int titleLineID = -1;
		/** An overall description for the objective */
		public string description;
		/** The translation ID for the description text, generated by the Speech Manager */
		public int descriptionLineID = -1;
		/** If True, and player switching is enabled, then this Objective's state will be limited to the current Player */
		public bool perPlayer = false;
		/** An associated texture */
		public Texture2D texture;
		/** A list of all possible states the Objective can be */
		public List<ObjectiveState> states;
		/** If True, the state cannot be changed once it is considered complete */
		public bool lockStateWhenComplete = true;
		/** If True, the state cannot be changed once it is considered failed */
		public bool lockStateWhenFail = true;
		/** The Objective's Category ID */
		public int binID = 0;
		/** The Objective's properties */
		public List<InvVar> vars = new List<InvVar>();

		#endregion


		#region Constructors

		public Objective (int[] idArray)
		{
			title = string.Empty;
			titleLineID = -1;
			description = string.Empty;
			descriptionLineID = -1;
			perPlayer = false;
			texture = null;
			states = new List <ObjectiveState>();
			states.Add (new ObjectiveState (0, "Started", ObjectiveStateType.Active));
			states.Add (new ObjectiveState (1, "Completed", ObjectiveStateType.Complete));

			ID = 0;
			if (idArray != null)
			{
				// Update id based on array
				foreach (int _id in idArray)
				{
					if (ID == _id)
						ID ++;
				}
			}
		}

		#endregion


		#region PublicFunctions

		public void RebuildProperties ()
		{
			// Which properties are available?
			List<int> availableVarIDs = new List<int> ();
			foreach (InvVar invVar in KickStarter.inventoryManager.invVars)
			{
				if (invVar.limitToCategories)
				{
					foreach (int categoryID in invVar.categoryIDs)
					{
						if (categoryID != binID) continue;

						InvBin invBin = KickStarter.inventoryManager.GetCategory (categoryID);
						if (invBin != null && invBin.forObjectives)
						{
							availableVarIDs.Add (invVar.id);
						}
					}
				}
			}

			// Create new properties / transfer existing values
			List<InvVar> newInvVars = new List<InvVar> ();
			foreach (InvVar invVar in KickStarter.inventoryManager.invVars)
			{
				if (availableVarIDs.Contains (invVar.id))
				{
					InvVar newInvVar = new InvVar (invVar);
					InvVar oldInvVar = GetProperty (invVar.id);
					if (oldInvVar != null)
					{
						newInvVar.TransferValues (oldInvVar);
					}
					newInvVar.popUpID = invVar.popUpID;
					newInvVars.Add (newInvVar);
				}
			}

			vars = newInvVars;
		}


		/**
		 * <summary>Gets a property of the Document.</summary>
		 * <param name = "ID">The ID number of the property to get</param>
		 * <returns>The property of the Document</returns>
		 */
		public InvVar GetProperty (int ID)
		{
			if (vars.Count > 0 && ID >= 0)
			{
				foreach (InvVar var in vars)
				{
					if (var.id == ID)
					{
						return var;
					}
				}
			}
			return null;
		}


		/**
		 * <summary>Gets a property of the Document.</summary>
		 * <param name = "propertyName">The name of the property to get</param>
		 * <returns>The property of the Document</returns>
		 */
		public InvVar GetProperty (string propertyName)
		{
			if (vars.Count > 0 && !string.IsNullOrEmpty (propertyName))
			{
				foreach (InvVar var in vars)
				{
					if (var.label == propertyName)
					{
						return var;
					}
				}
			}
			return null;
		}


		/**
		 * <summary>Gets a state with a particular ID number</summary>
		 * <param>The ID number of the state to get</param>
		 * <returns>The Objective state</returns>
		 */
		public ObjectiveState GetState (int stateID)
		{
			foreach (ObjectiveState state in states)
			{
				if (state.ID == stateID)
				{
					return state;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets the objective's title text in a given language</summary>
		 * <param name = "languageNumber">The language index, where 0 = the game's default language</param>
		 * <returns>The title text</returns>
		 */
		public string GetTitle (int languageNumber = 0)
		{
			return KickStarter.runtimeLanguages.GetTranslation (title, titleLineID, languageNumber, GetTranslationType (0));
		}


		/**
		 * <summary>Gets the objective's description text in a given language</summary>
		 * <param name = "languageNumber">The language index, where 0 = the game's default language</param>
		 * <returns>The description text</returns>
		 */
		public string GetDescription (int languageNumber = 0)
		{
			return KickStarter.runtimeLanguages.GetTranslation (description, descriptionLineID, languageNumber, GetTranslationType (0));
		}


		public override string ToString ()
		{
			if (!string.IsNullOrEmpty (title))
			{
				return "Objective ID " + ID + "; " + title;
			}
			return "Objective ID " + ID;
		}

		#endregion


		#region GetSet

		/** The objective's title */
		public string Title
		{
			get
			{
				if (string.IsNullOrEmpty (title))
				{
					title = "(Untitled)";
				}
				return title;
			}
			set
			{
				title = value;
			}
		}


		/** How many states the objective can take */
		public int NumStates
		{
			get
			{
				return states.Count;
			}
		}

		#endregion


		#if UNITY_EDITOR

		protected int sideState;
		protected int selectedState;
		protected Vector2 scrollPos;
		protected bool showStateGUI = true;
		protected bool showPropertiesGUI = true;

		private ObjectiveState lastDragStateOver;
		private const string DragObjectiveStateKey = "AC.InventoryObjectiveStates";
		private int lastSwapIndex;
		private bool ignoreDrag;


		public void ShowGUI (string apiPrefix, System.Action<ActionListAsset> showALAEditor)
		{
			CustomGUILayout.BeginVertical ();

			CustomGUILayout.UpdateDrag (DragObjectiveStateKey, lastDragStateOver, lastDragStateOver != null ? lastDragStateOver.Label : string.Empty, ref ignoreDrag, OnCompleteDragState);
			if (Event.current.type == EventType.Repaint)
			{
				lastDragStateOver = null;
				lastSwapIndex = -1;
			}

			if (Application.isPlaying && KickStarter.runtimeObjectives)
			{
				ObjectiveState currentState = KickStarter.runtimeObjectives.GetObjectiveState (ID);
				if (currentState != null)
				{
					EditorGUILayout.LabelField ("Current state::", currentState.ID + ": " + currentState.Label, EditorStyles.boldLabel);
				}
				else
				{
					EditorGUILayout.LabelField ("Current state:", "INACTIVE", EditorStyles.boldLabel);
				}
				EditorGUILayout.Space ();
			}

			title = CustomGUILayout.TextField ("Title:", title, apiPrefix + ".title");
			if (titleLineID > -1)
			{
				EditorGUILayout.LabelField ("Speech Manager ID:", titleLineID.ToString ());
			}

			binID = KickStarter.inventoryManager.ChooseCategoryGUI ("Category:", binID, false, false, true, apiPrefix + ".binID", "The Objective's category");

			EditorGUILayout.BeginHorizontal ();
			CustomGUILayout.LabelField ("Description:", GUILayout.Width (140f), apiPrefix + ".description");
			EditorStyles.textField.wordWrap = true;
			description = CustomGUILayout.TextArea (description, GUILayout.MaxWidth (800f), apiPrefix + ".description");
			EditorGUILayout.EndHorizontal ();

			if (descriptionLineID > -1)
			{
				EditorGUILayout.LabelField ("Speech Manager ID:", descriptionLineID.ToString ());
			}

			if (KickStarter.settingsManager && KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				perPlayer = CustomGUILayout.Toggle ("Per-Player?", perPlayer, apiPrefix + ".perPlayer");
			}

			texture = (Texture2D) CustomGUILayout.ObjectField <Texture2D> ("Texture:", texture, false, apiPrefix + ".texture");
			lockStateWhenComplete = CustomGUILayout.Toggle ("Lock state when complete?", lockStateWhenComplete, apiPrefix + ".lockStateWhenComplete");
			lockStateWhenFail = CustomGUILayout.Toggle ("Lock state when fail?", lockStateWhenFail, apiPrefix + ".lockStateWhenFail");

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("Objective states:");

			CustomGUILayout.BeginScrollView (ref scrollPos, states.Count);
			for (int i=0; i<states.Count; i++)
			{
				EditorGUILayout.BeginHorizontal ();

				ObjectiveState thisState = states[i];
				if (GUILayout.Toggle (selectedState == i, thisState.ID.ToString () + ": " + thisState.Label, "Button"))
				{
					if (selectedState != i)
					{
						selectedState = i;
						EditorGUIUtility.editingTextField = false;
					}
				}

				Rect buttonRect = GUILayoutUtility.GetLastRect ();
				if (buttonRect.Contains (Event.current.mousePosition) && Event.current.type == EventType.Repaint)
				{
					lastDragStateOver = thisState;
				}

				if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
				{
					sideState = i;
					EditorGUIUtility.editingTextField = false;
					SideStateMenu ();
				}
				EditorGUILayout.EndHorizontal ();

				if (IsDraggingState ())
				{
					CustomGUILayout.DrawDragLine (i, ref lastSwapIndex);
				}
			}
			CustomGUILayout.EndScrollView ();

			if (GUILayout.Button ("Create new state"))
			{
				Undo.RecordObject (KickStarter.inventoryManager, "Add Objective state");
				states.Add (new ObjectiveState (GetStateIDArray ()));
			}
			CustomGUILayout.EndVertical ();

			EditorGUILayout.Space ();

			if (selectedState >= 0 && states.Count > selectedState)
			{
				showStateGUI = CustomGUILayout.ToggleHeader (showStateGUI, "State #" + states[selectedState].ID.ToString () + ": " + states[selectedState].Label);
				if (showStateGUI)
				{
					CustomGUILayout.BeginVertical ();
					states[selectedState].ShowGUI (this, apiPrefix + ".states[" + selectedState.ToString () + "].", showALAEditor);
					CustomGUILayout.EndVertical ();
				}
			}

			RebuildProperties ();

			if (vars.Count > 0)
			{
				EditorGUILayout.Space ();
				showPropertiesGUI = CustomGUILayout.ToggleHeader (showPropertiesGUI, "Objective properties");
				if (showPropertiesGUI)
				{
					CustomGUILayout.BeginVertical ();
					foreach (InvVar invVar in vars)
					{
						invVar.ShowGUI (apiPrefix + ".GetProperty (" + invVar.id + ")");
					}
					CustomGUILayout.EndVertical ();
				}
			}
		}


		private void OnCompleteDragState (object data)
		{
			ObjectiveState state = (ObjectiveState) data;
			if (state == null) return;

			int dragIndex = states.IndexOf (state);
			if (dragIndex >= 0 && lastSwapIndex >= 0)
			{
				ObjectiveState tempState = state;

				states.RemoveAt (dragIndex);

				if (lastSwapIndex > dragIndex)
				{
					states.Insert (lastSwapIndex - 1, tempState);
				}
				else
				{
					states.Insert (lastSwapIndex, tempState);
				}

				Event.current.Use ();
				EditorUtility.SetDirty (KickStarter.inventoryManager);

				selectedState = states.IndexOf (tempState);
			}
		}



		private bool IsDraggingState ()
		{
			object dragObject = DragAndDrop.GetGenericData (DragObjectiveStateKey);
			if (dragObject != null && dragObject is ObjectiveState)
			{
				return true;
			}
			return false;
		}


		protected int[] GetStateIDArray ()
		{
			List<int> idArray = new List<int>();
			
			foreach (ObjectiveState state in states)
			{
				idArray.Add (state.ID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		protected void SideStateMenu ()
		{
			GenericMenu menu = new GenericMenu ();

			menu.AddItem (new GUIContent ("Insert after"), false, StateCallback, "Insert after");
			if (states.Count > 1 && states[sideState].ID >= 2)
			{
				menu.AddItem (new GUIContent ("Delete"), false, StateCallback, "Delete");
			}

			if (sideState > 0 && sideState < states.Count-1)
			{
				menu.AddSeparator (string.Empty);
				if (sideState > 1)
				{
					menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, StateCallback, "Move to top");
					menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, StateCallback, "Move up");
				}
				if (sideState < states.Count-1)
				{
					menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, StateCallback, "Move down");
					menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, StateCallback, "Move to bottom");
				}
			}

			if (Application.isPlaying)
			{
				menu.AddItem (new GUIContent ("Set as active"), false, StateCallback, "Set as active");
			}
			
			menu.ShowAsContext ();
		}


		protected void StateCallback (object obj)
		{
			if (sideState >= 0)
			{
				switch (obj.ToString ())
				{
					case "Insert after":
						Undo.RecordObject (KickStarter.inventoryManager, "Insert state");
						states.Insert (sideState+1, new ObjectiveState (GetStateIDArray ()));
						break;
						
					case "Delete":
						Undo.RecordObject (KickStarter.inventoryManager, "Delete state");
						if (sideState == selectedState)
						{
							selectedState = -1;
						}
						states.RemoveAt (sideState);
						break;
						
					case "Move up":
						Undo.RecordObject (KickStarter.inventoryManager, "Move state up");
						if (sideState == selectedState)
						{
							selectedState --;
						}
						SwapStates (sideState, sideState-1);
						break;
						
					case "Move down":
						Undo.RecordObject (KickStarter.inventoryManager, "Move state down");
						if (sideState == selectedState)
						{
							selectedState ++;
						}
						SwapStates (sideState, sideState+1);
						break;

					case "Move to top":
						Undo.RecordObject (KickStarter.inventoryManager, "Move state to top");
						if (sideState == selectedState)
						{
							selectedState --;
						}
						MoveStateToTop (sideState);
						break;
					
					case "Move to bottom":
						Undo.RecordObject (KickStarter.inventoryManager, "Move state to bottom");
						if (sideState == selectedState)
						{
							selectedState ++;
						}
						MoveStateToBottom (sideState);
						break;

					case "Set as active":
						if (Application.isPlaying && KickStarter.runtimeObjectives)
						{
							KickStarter.runtimeObjectives.SetObjectiveState (ID, states[sideState].ID);
						}
						break;
				}
			}
			
			sideState = -1;
		}


		protected void MoveStateToTop (int a1)
		{
			ObjectiveState tempState = states[a1];
			states.Insert (1, tempState);
			states.RemoveAt (a1+1);
		}


		protected void MoveStateToBottom (int a1)
		{
			ObjectiveState tempState = states[a1];
			states.Add (tempState);
			states.RemoveAt (a1);
		}
		

		protected void SwapStates (int a1, int a2)
		{
			ObjectiveState tempState = states[a1];
			states[a1] = states[a2];
			states[a2] = tempState;
		}


		public int StateSelectorList (int selectedID, string label = "State:")
		{
			int tempNumber = -1;

			string[] labelList = new string[states.Count];
			for (int i=0; i<states.Count; i++)
			{
				labelList[i] = states[i].ID.ToString () + ": " + states[i].Label;

				if (states[i].ID == selectedID)
				{
					tempNumber = i;
				}
			}

			if (tempNumber == -1)
			{
				// Wasn't found (was deleted?), so revert to zero
				if (selectedID != 0)
					ACDebug.LogWarning ("Previously chosen Objective State no longer exists!");
				tempNumber = 0;
				selectedID = 0;
			}

			tempNumber = UnityEditor.EditorGUILayout.Popup (label, tempNumber, labelList);
			selectedID = states [tempNumber].ID;

			return selectedID;
		}


		public string[] GenerateEditorStateLabels ()
		{
			List<string> labelsList = new List<string>();
			foreach (ObjectiveState state in states)
			{
				labelsList.Add (state.ID.ToString () + ": " + state.Label);
			}
			return labelsList.ToArray ();
		}

#endif


		#region ITranslatable

		public string GetTranslatableString (int index)
		{
			if (index == 0)
			{
				return title;
			}
			else if (index == 1)
			{
				return description;
			}
			else
			{
				return vars[index-2].TextValue;
			}
		}


		public int GetTranslationID (int index)
		{
			if (index == 0)
			{
				return titleLineID;
			}
			else if (index == 1)
			{
				return descriptionLineID;
			}
			else
			{
				return vars[index-2].textValLineID;
			}
		}


		public AC_TextType GetTranslationType (int index)
		{
			if (index < 2)
			{
				return AC_TextType.Objective;
			}
			else
			{
				return AC_TextType.InventoryProperty;
			}
		}


		#if UNITY_EDITOR

		public void UpdateTranslatableString (int index, string updatedText)
		{
			if (index == 0)
			{
				title = updatedText;
			}
			else if (index == 1)
			{
				description = updatedText;
			}
			else
			{
				vars[index-2].TextValue = updatedText;
			}
		}


		public int GetNumTranslatables ()
		{
			int numProperties = (vars != null) ? vars.Count : 0;
			return 2 + numProperties;
		}


		public bool HasExistingTranslation (int index)
		{
			if (index == 0)
			{
				return titleLineID > -1;
			}
			else if (index == 1)
			{
				return descriptionLineID > -1;
			}
			else
			{
				return vars[index-2].textValLineID > -1;
			}
		}



		public void SetTranslationID (int index, int _lineID)
		{
			if (index == 0)
			{
				titleLineID = _lineID;
			}
			else if (index == 1)
			{
				descriptionLineID = _lineID;
			}
			else
			{
				vars[index-2].textValLineID = _lineID;
			}
		}


		public string GetOwner (int index)
		{
			return string.Empty;
		}


		public bool OwnerIsPlayer (int index)
		{
			return false;
		}


		public bool CanTranslate (int index)
		{
			if (index == 0)
			{
				return !string.IsNullOrEmpty (title);
			}
			else if (index == 1)
			{
				return !string.IsNullOrEmpty (description);
			}
			else
			{
				if (vars[index-2].type == VariableType.String && !string.IsNullOrEmpty (vars[index-2].TextValue))
				{
					return true;
				}
				return false;
			}
		}

		#endif

		#endregion

	}

}