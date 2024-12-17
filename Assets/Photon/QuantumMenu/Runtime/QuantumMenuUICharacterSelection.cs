using System;
using System.Collections.Generic;
using Quantum.Menu.CharacterSelection;
using UnityEngine;

namespace Quantum.Menu
{
    public class QuantumMenuUICharacterSelection : QuantumMenuUIScreen
    {
        [SerializeField] private CharacterModel[] characterModels;
        [SerializeField] private UI_SelectableCharacter selectableCharacter;
        [SerializeField] private Transform characterSelectionParent;
        [SerializeField] private QuantumMenuUIController quantumMenuUIController;
        private Dictionary<CharacterModel, UI_SelectableCharacter> _selectableCharacterMap = new ();
        private CharacterModel _characterModelCurrentlySelected;
        
        public override void Awake()
        {
            UI_SelectableCharacter.OnCharacterSelected += CharacterSelected;
            InitalizeCharacterSelection();
        }

        private void InitalizeCharacterSelection()
        {
            for (int i = 0; i < characterModels.Length; i++)
            {
                var characterModel = characterModels[i];
                var uiSelectableCharacter = Instantiate(selectableCharacter, characterSelectionParent);
                uiSelectableCharacter.Initialize(characterModel);
                _selectableCharacterMap.Add(characterModel, uiSelectableCharacter);
            }
            
            CharacterSelected(characterModels[0]);
        }

        private void OnDestroy()
        {
            UI_SelectableCharacter.OnCharacterSelected -= CharacterSelected;
        }

        private void CharacterSelected(CharacterModel model)
        {
            if(_characterModelCurrentlySelected == model)
                return;
            if (_characterModelCurrentlySelected != null)
            {
                _selectableCharacterMap[_characterModelCurrentlySelected].SetSelected(false);
            }
            
            _selectableCharacterMap[model].SetSelected(true);
            _characterModelCurrentlySelected = model;
            quantumMenuUIController.ConnectArgs.RuntimePlayers[0].PlayerAvatar = model.EntityPrototype;
        }

        /// <summary>
        /// Is called when the <see cref="_backButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed() {
            Controller.Show<QuantumMenuUIMain>();
        }
    }
}