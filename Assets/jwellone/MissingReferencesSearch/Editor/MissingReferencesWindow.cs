using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace jwelloneEditor
{
	public class MissingReferencesWindow : EditorWindow
	{
		[SerializeField] Vector2 _scrollPos;
		[SerializeField] List<MissingReferences> _references = new List<MissingReferences>();

		[MenuItem("jwellone/window/MissingReferencesWindow")]
		static void Open()
		{
			EditorWindow.GetWindow<MissingReferencesWindow>("Missing References");
		}

		private void OnGUI()
		{
			try
			{
				using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos))
				{
					_scrollPos = scrollView.scrollPosition;
					EditorGUI.BeginDisabledGroup(true);

					foreach (var reference in _references)
					{
						var text = $"{reference.target.name}({reference.count})";
						EditorGUILayout.ObjectField(text, reference.target, typeof(Object), true);
					}

					EditorGUI.EndDisabledGroup();
				}


				EditorGUILayout.BeginHorizontal();

				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Find", GUILayout.Width(64)))
				{
					_references = MissingReferencesUtil.FindAll();
				}

				EditorGUILayout.EndHorizontal();
			}
			catch
			{
				_references.Clear();
			}
		}
	}
}
