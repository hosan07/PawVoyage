using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 한 번의 플레이 세션에서 누적되는 진행 정보를 관리합니다.
    /// </summary>
    public class RunStats : MonoBehaviour
    {
        public static RunStats Instance { get; private set; }

        public float ElapsedSeconds { get; private set; }
        public int KillCount { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            ElapsedSeconds += Time.deltaTime;
        }

        /// <summary>
        /// 적 처치 수를 1 증가시킵니다.
        /// </summary>
        public void AddKill()
        {
            KillCount++;
        }
    }
}
