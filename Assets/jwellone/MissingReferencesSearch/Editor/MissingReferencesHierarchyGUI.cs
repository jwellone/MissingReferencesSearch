using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
	public static class MissingReferencesSearchHierarchyGUI
	{
		static Texture _icon = null!;
		static double _prevTimeSinceStartup;
		static readonly Dictionary<int, bool> _dictionary = new Dictionary<int, bool>();

		[InitializeOnLoadMethod]
		static void OnInitializeOnLoadMethod()
		{
			var icon = EditorGUIUtility.IconContent("console.warnicon").image;
			_icon = icon == null ? Texture2D.whiteTexture : icon;

			EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;

			EditorApplication.hierarchyChanged -= OnReset;
			EditorApplication.hierarchyChanged += OnReset;
		}

		static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
		{
			var target = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (target == null)
			{
				return;
			}

			if (EditorApplication.timeSinceStartup - _prevTimeSinceStartup > 1f)
			{
				OnReset();
			}

			if (_dictionary.ContainsKey(instanceID))
			{
				Draw(target, selectionRect, _dictionary[instanceID]);
				return;
			}

			foreach (var component in target.GetComponents<Component>())
			{
				if (MissingReferencesUtil.Exists(component))
				{
					_dictionary.Add(instanceID, true);
					Draw(target, selectionRect, true);
					return;
				}
			}

			bool existsMissing = false;
			foreach (var child in target.GetComponentsInChildren<Transform>(true))
			{
				if (_dictionary.ContainsKey(child.GetInstanceID()))
				{
					existsMissing = true;
					continue;
				}

				foreach (var component in child.GetComponents<Component>())
				{
					if (MissingReferencesUtil.Exists(component))
					{
						existsMissing = true;
						_dictionary.Add(child.GetInstanceID(), true);
						break;
					}
				}
			}

			if (existsMissing)
			{
				_dictionary.Add(instanceID, false);
			}
		}

		static void Draw(in GameObject target, in Rect selectionRect, bool isDispIcon)
		{
			var color = Color.yellow;
			color.a = 0.1f;

			var drawRect = selectionRect;
			drawRect.x = 32;
			drawRect.width *= 2;
			EditorGUI.DrawRect(drawRect, color);

			if (!isDispIcon)
			{
				return;
			}

			var iconPos = selectionRect;
			iconPos.width = 16;
			iconPos.height = iconPos.width;
			iconPos.x -= (target.transform.childCount > 0 ? iconPos.width + 10 : iconPos.width);
			GUI.DrawTexture(iconPos, _icon);
		}

		static void OnReset()
		{
			_dictionary.Clear();
			_prevTimeSinceStartup = EditorApplication.timeSinceStartup;
		}
	}
}