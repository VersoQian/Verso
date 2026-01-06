using System;
using TMPro;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class BuildInfoView : MonoBehaviour
    {
        private const string _infoFormat = "v{0} ({1})";

        [SerializeField] private TMP_Text _buildInfoText;

        private void Start()
        {
            var version = Application.version;
            var revisionNumber = LoadRevisionNumber();

            var buildInfo = String.Format(_infoFormat, version, revisionNumber);
            _buildInfoText.text = buildInfo;
        }

        [Serializable]
        private class SimpleJson
        {
            public string RevisionNumber;
        }

        private string LoadRevisionNumber()
        {
            var result = "0";

            TextAsset jsonFile = Resources.Load<TextAsset>("PlayerSettings");
            if (jsonFile != null)
            {
                result = JsonUtility.FromJson<SimpleJson>(jsonFile.text).RevisionNumber;
            }

            return result;
        }
    }
}

