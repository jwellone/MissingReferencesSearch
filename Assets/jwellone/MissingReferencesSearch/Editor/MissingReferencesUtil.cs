using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

#nullable enable

namespace jwelloneEditor
{
	[Serializable]
	public class MissingReferencesData
	{
		[SerializeField] UnityEngine.Object _target = null!;
		[SerializeField] List<string> _componentNames = null!;
		[SerializeField] List<string> _propertyPaths = null!;

		public UnityEngine.Object target => _target;
		public List<string> componentNames => _componentNames;
		public List<string> propertyPaths => _propertyPaths;

		public MissingReferencesData(in UnityEngine.Object target)
		{
			_target = target;
			_componentNames = new List<string>();
			_propertyPaths = new List<string>();
		}
	}

	[Serializable]
	public class MissingReferences
	{
		[SerializeField] int _count;
		[SerializeField] UnityEngine.Object? _target;
		[SerializeField] List<MissingReferencesData> _data = null!;

		public bool isMissing => count > 0;
		public int count => _count;
		public UnityEngine.Object? target => _target;
		public IReadOnlyList<MissingReferencesData> data => _data;

		public MissingReferences(in GameObject target)
		{
			_data = new List<MissingReferencesData>();
			_target = target;
			CalcMissingCount(target);
		}

		public MissingReferences(string guid)
		{
			_data = new List<MissingReferencesData>();

			var path = AssetDatabase.GUIDToAssetPath(guid);
			var prefab = PrefabUtility.LoadPrefabContents(path);
			CalcMissingCount(prefab);

			if (isMissing)
			{
				_target = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			}

			PrefabUtility.UnloadPrefabContents(prefab);
		}

		public MissingReferences(in Scene scene)
		{
			_data = new List<MissingReferencesData>();
			_target = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
			foreach (var rootObject in scene.GetRootGameObjects())
			{
				CalcMissingCount(rootObject);
			}
		}

		void CalcMissingCount(in GameObject gameOject)
		{
			_count += OnCalcMissingCount(gameOject);
			for (var i = 0; i < gameOject.transform.childCount; ++i)
			{
				var child = gameOject.transform.GetChild(i);
				CalcMissingCount(child.gameObject);
			}
		}

		int OnCalcMissingCount(in GameObject target)
		{
			var missingCount = 0;
			if (PrefabUtility.GetPrefabInstanceStatus(target) == PrefabInstanceStatus.MissingAsset)
			{
				++missingCount;
			}

			var missingData = new MissingReferencesData(target);
			foreach (var component in target.GetComponents<Component>())
			{
				if (component == null)
				{
					missingData.componentNames.Add("Missing script");
					missingData.propertyPaths.Add(MissingReferencesUtil.MakeHierarchyPath(target));
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

					missingData.componentNames.Add(component.GetType().Name);
					missingData.propertyPaths.Add(property.propertyPath);
					++missingCount;
				}
			}

			if (missingCount > 0)
			{
				_data.Add(missingData);
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

		public static List<MissingReferences> FindHierarchy()
		{
			var list = new List<MissingReferences>();

			var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			if (prefabStage != null)
			{
				var data = new MissingReferences(prefabStage.prefabContentsRoot);
				if (data.isMissing)
				{
					list.Add(data);
				}
			}
			else
			{
				var scene = SceneManager.GetActiveScene();
				var data = new MissingReferences(scene);
				if (data.isMissing)
				{
					list.Add(data);
				}
			}

			return list;

		}

		public static List<MissingReferences> FindAll()
		{
			var list = new List<MissingReferences>();

			var prefabs = AssetDatabase.FindAssets("t:Prefab");
			for (var i = 0; i < prefabs.Length; ++i)
			{
				var guid = prefabs[i];
				var path = AssetDatabase.GUIDToAssetPath(guid);
				EditorUtility.DisplayProgressBar("Search prefab", path, (i + 1) / (float)prefabs.Length);
				var data = new MissingReferences(guid);
				if (data.isMissing)
				{
					list.Add(data);
				}
			}

			EditorUtility.ClearProgressBar();

			var activeScene = SceneManager.GetActiveScene();
			var scenes = AssetDatabase.FindAssets("t:Scene");
			for (var i = 0; i < scenes.Length; ++i)
			{
				var guid = scenes[i];
				var path = AssetDatabase.GUIDToAssetPath(guid);
				EditorUtility.DisplayProgressBar("Search scene", path, (i + 1) / (float)scenes.Length);

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

			EditorUtility.ClearProgressBar();

			return list;
		}

		public static string MakeHierarchyPath(in GameObject target)
		{
			var sb = new StringBuilder(target.name);
			var current = target.transform.parent;
			while (current != null)
			{
				sb.Append("/").Append(current.name);
				current = current.parent;
			}

			return sb.ToString();
		}
	}
}