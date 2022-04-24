using UnityEngine;
using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
	public static class MissingReferencesSearchHierarchyGUI
	{
		static Texture _icon = null!;

		[InitializeOnLoadMethod]
		static void OnInitializeOnLoadMethod()
		{
			var icon = EditorGUIUtility.IconContent("console.warnicon").image;
			_icon = icon == null ? Texture2D.whiteTexture : icon;

			EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
		}

		static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
		{
			var target = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (target == null)
			{
				return;
			}

			var pos = selectionRect;
			pos.width = 16;
			pos.height = pos.width;
			pos.x -= pos.width;

			foreach (var component in target.GetComponents<Component>())
			{
				if (MissingReferencesUtil.Exists(component))
				{
					GUI.DrawTexture(pos, _icon);
					break;
				}
			}
		}
	}
}