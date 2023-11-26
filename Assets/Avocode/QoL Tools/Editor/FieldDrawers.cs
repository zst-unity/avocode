using UnityEditor;
using UnityEngine;

namespace ZSTUnity.QoL.Editor
{
	[CustomPropertyDrawer(typeof(SceneField))]
	public class SceneFieldPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
		{
			EditorGUI.BeginProperty(_position, GUIContent.none, _property);

			SerializedProperty sceneAsset = _property.FindPropertyRelative("m_SceneAsset");
			SerializedProperty sceneName = _property.FindPropertyRelative("m_SceneName");

			_position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
			if (sceneAsset != null)
			{
				sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);

				if (sceneAsset.objectReferenceValue != null)
				{
					sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
				}
			}
			EditorGUI.EndProperty();
		}
	}

}