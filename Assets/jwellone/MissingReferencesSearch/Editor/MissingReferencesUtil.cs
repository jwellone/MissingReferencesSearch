using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

#nullable enable

namespace jwelloneEditor
{
	[Serializable]
	public class MissingReferences
	{
		[SerializeField] int _count;
		[SerializeField] UnityEngine.Object? _target;

		public bool isMissing => count > 0;
		public int count => _count;
		public UnityEngine.Object? target => _target;


		public MissingReferences(in GameObject target)
		{
			_target = target;
			CalcMissingCount(target);
		}

		public MissingReferences(in Scene scene)
		{
			_target = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
			foreach (var rootObject in scene.GetRootGameObjects())
			{
				CalcMissingCount(rootObject);
			}
		}

		void CalcMissingCount(in GameObject gameOject)
		{
			_count += GetMissingCount(gameOject);
			for (var i = 0; i < gameOject.transform.childCount; ++i)
			{
				var child = gameOject.transform.GetChild(i);
				CalcMissingCount(child.gameObject);
			}
		}

		int GetMissingCount(in GameObject target)
		{
			var missingCount = 0;
			if (PrefabUtility.IsAnyPrefabInstanceRoot(target) && PrefabUtility.IsPrefabAssetMissing(target))
			{
				++missingCount;
			}

			foreach (var component in target.GetComponents<Component>())
			{
				if (component == null)
				{
					++missingCount;
					continue;
				}

				var property = new SerializedObject(component).GetIterator();
				while (property.NextVisible(true))
				{
					if (property.propertyType != SerializedPropertyType.ObjectReference ||
						property.objectReferenceValue != null)
					{
						continue;
					}

					var fileId = property.FindPropertyRelative("m_FileID");
					if (fileId == null || fileId.intValue == 0)
					{
						continue;
					}

					++missingCount;
				}
			}

			return missingCount;
		}
	}

	public class MissingReferencesUtil
	{
		public static bool Exists(in Component component)
		{
			if (component == null)
			{
				return true;
			}

			if (PrefabUtility.IsAnyPrefabInstanceRoot(component.gameObject) && PrefabUtility.IsPrefabAssetMissing(component.gameObject))
			{
				return true;
			}

			var property = new SerializedObject(component).GetIterator();
			while (property.NextVisible(true))
			{
				if (property.propertyType != SerializedPropertyType.ObjectReference ||
					property.objectReferenceValue != null)
				{
					continue;
				}

				var fileId = property.FindPropertyRelative("m_FileID");
				if (fileId != null && fileId.intValue != 0)
				{
					return true;
				}
			}

			return false;
		}

		public static List<MissingReferences> FindAll()
		{
			var list = new List<MissingReferences>();


			foreach (var guid in AssetDatabase.FindAssets("t:GameObject"))
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var target = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				var data = new MissingReferences(target);
				if (data.isMissing)
				{
					list.Add(data);
				}
			}

			var activeScene = SceneManager.GetActiveScene();
			foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (!path.StartsWith("Assets"))
				{
					continue;
				}

				if (path == activeScene.path)
				{
					var data = new MissingReferences(activeScene);
					if (data.isMissing)
					{
						list.Add(data);
					}
				}
				else
				{
					var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
					var data = new MissingReferences(scene);
					if (data.isMissing)
					{
						list.Add(data);
					}

					EditorSceneManager.CloseScene(scene, true);
				}
			}

			return list;
		}
	}
}