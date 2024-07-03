using UnityEngine;

namespace Nakama.Helpers
{
    [CreateAssetMenu(menuName = "Nakama/Connection data")]
    public class NakamaConnectionData : ScriptableObject
    {
        #region FIELDS

        [SerializeField] private string scheme = null;
        [SerializeField] public string host = null;
        [SerializeField] public string backuphost = null;

        [SerializeField] private int port = default(int);
        [SerializeField] private string serverKey = null;

        #endregion

        #region PROPERTIES

        public string Scheme { get => scheme; }
        public string Host { get => host; }
        public string backupHost { get => backuphost; }
        public int Port { get => port; }
        public string ServerKey { get => serverKey; }

        #endregion
    }
}
