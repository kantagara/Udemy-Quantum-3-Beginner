using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quantum.Menu.CharacterSelection
{
    public class UI_SelectableCharacter : MonoBehaviour
    {
        [SerializeField] private Image characterImage;
        [SerializeField] private TMP_Text characterName;
        [SerializeField] private GameObject characterSelected;

        private CharacterModel _model;

        public static event Action<CharacterModel> OnCharacterSelected; 

        public void Initialize(CharacterModel model)
        {
            characterImage.sprite = model.CharacterImage;
            characterName.text = model.CharacterName;
            _model = model;
        }

        public void CharacterSelected()
        {
            OnCharacterSelected?.Invoke(_model);
        }

        public void SetSelected(bool isSelected)
        {
            characterSelected.SetActive(isSelected);
        }
    }
}