using UnityEngine;
using UnityEngine.SceneManagement;

namespace Syrinj
{
    public class SceneInjector : MonoBehaviour
    {
        public static SceneInjector Instance;

        void Awake()
        {
            Instance = this;
            DependencyContainer.Instance.Reset();
            InjectScene();
            SceneManager.sceneLoaded += OnLevelWasLoadedEvent;
        }

        void OnLevelWasLoadedEvent(Scene scene, LoadSceneMode mode)
        {
            InjectScene();
        }

        public void InjectScene()
        {
            var behaviours = GetAllBehavioursInScene();

            InjectBehaviours(behaviours);
        }

        private MonoBehaviour[] GetAllBehavioursInScene()
        {
            return GameObject.FindObjectsOfType<MonoBehaviour>();
        }

        private void InjectBehaviours(MonoBehaviour[] behaviours)
        {
            DependencyContainer.Instance.Inject(behaviours);
        }
    }
}
