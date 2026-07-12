using UnityEngine;

namespace PawVoyage.Systems
{
    /// <summary>
    /// 현재 전투 루프에서 사용하는 효과음을 한 곳에서 재생합니다.
    /// </summary>
    public class GameSfx : MonoBehaviour
    {
        [SerializeField] private AudioClip attackClip = null;
        [SerializeField] private AudioClip damageClip = null;
        [SerializeField] private AudioClip experienceClip = null;
        [SerializeField] private AudioClip levelUpClip = null;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.7f;
        [SerializeField] private float minimumAttackInterval = 0.08f;

        private AudioSource audioSource;
        private float nextAttackSoundTime;

        public static GameSfx Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void PlayAttack()
        {
            if (Instance == null || Time.unscaledTime < Instance.nextAttackSoundTime)
            {
                return;
            }

            Instance.nextAttackSoundTime = Time.unscaledTime + Instance.minimumAttackInterval;
            Instance.Play(Instance.attackClip);
        }

        public static void PlayDamage()
        {
            Instance?.Play(Instance.damageClip);
        }

        public static void PlayExperience()
        {
            Instance?.Play(Instance.experienceClip);
        }

        public static void PlayLevelUp()
        {
            Instance?.Play(Instance.levelUpClip);
        }

        private void Play(AudioClip clip)
        {
            if (clip == null || audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, masterVolume);
        }
    }
}
