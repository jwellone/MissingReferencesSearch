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
			if(EditorApplication.isCompiling||EditorApplication.isPlaying)
			{
				return;
			}

			var target = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if (target == null)
			{
				return;
			}

			foreach (var component in target.GetComponents<Component>())
			{
				if (MissingReferencesUtil.Exists(component))
				{
					var iconPos = selectionRect;
					iconPos.width = 16;
					iconPos.height = iconPos.width;
					iconPos.x -= (target.transform.childCount > 0 ? iconPos.width + 10 : iconPos.width);
					GUI.DrawTexture(iconPos, _icon);
					return;
				}
			}
		}
	}
}