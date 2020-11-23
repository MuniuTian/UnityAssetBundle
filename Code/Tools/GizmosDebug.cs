namespace Model
{
    using UnityEngine;
    using System.Collections.Generic;

    public class GizmosDebug : MonoBehaviour
    {
        public static GizmosDebug Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDrawGizmos()
        {
            if (Path.Count < 2)
            {
                return;
            }

            for (var i = 0; i < Path.Count - 1; ++i)
            {
                Gizmos.DrawLine(Path[i], Path[i + 1]);
            }
        }

        public List<Vector3> Path = new List<Vector3>();
    }
}