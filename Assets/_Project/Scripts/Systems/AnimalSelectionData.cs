using UnityEngine;

namespace PawVoyage.Systems
{
    public enum SelectedAnimalType
    {
        Dog,
        Cat
    }

    /// <summary>
    /// 메인 메뉴에서 고른 플레이어 동물 선택값을 저장합니다.
    /// </summary>
    public static class AnimalSelectionData
    {
        private const string SelectedAnimalKey = "Player.SelectedAnimal";
        private const string DogId = "dog";
        private const string CatId = "cat";

        public static SelectedAnimalType SelectedAnimal
        {
            get
            {
                string savedValue = PlayerPrefs.GetString(SelectedAnimalKey, DogId);
                return savedValue == CatId ? SelectedAnimalType.Cat : SelectedAnimalType.Dog;
            }
        }

        public static string SelectedAnimalName => SelectedAnimal == SelectedAnimalType.Cat ? "CAT" : "DOG";

        /// <summary>
        /// 선택한 동물을 저장합니다.
        /// </summary>
        public static void SelectAnimal(SelectedAnimalType animalType)
        {
            PlayerPrefs.SetString(SelectedAnimalKey, animalType == SelectedAnimalType.Cat ? CatId : DogId);
            PlayerPrefs.Save();
        }
    }
}
