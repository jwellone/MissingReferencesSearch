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

		void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Asset");
			EditorGUILayout.LabelField("Missing Count");
			EditorGUILayout.EndHorizontal();

			var style = new GUIStyle(EditorStyles.textField);
			style.alignment = TextAnchor.MiddleRight;

			using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos))
			{
				_scrollPos = scrollView.scrollPosition;
				using (new EditorGUI.DisabledScope(true))
				{
					foreach (var reference in _references)
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.ObjectField(string.Empty, reference.target, typeof(Object), true);
						EditorGUILayout.TextField(reference.count.ToString(), style);
						EditorGUILayout.EndHorizontal();
					}
				}
			}

			EditorGUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Find", GUILayout.Width(64)))
			{
				_references = MissingReferencesUtil.FindAll();
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}
