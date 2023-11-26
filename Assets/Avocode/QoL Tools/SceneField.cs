using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZSTUnity.QoL
{
	[Serializable]
	public class SceneField
	{
		[SerializeField] private UnityEngine.Object m_SceneAsset;
		[SerializeField] private string m_SceneName = "";

		public string SceneName => m_SceneName;
		public int SceneIndex => SceneManager.GetSceneByName(m_SceneName).buildIndex;

		public static implicit operator string(SceneField sceneField)
		{
			return sceneField.SceneName;
		}
	}

}
