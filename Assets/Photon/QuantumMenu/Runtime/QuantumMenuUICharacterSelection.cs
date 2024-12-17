using UnityEngine;

namespace Quantum.Menu
{
    public class QuantumMenuUICharacterSelection : QuantumMenuUIScreen
    {
        [SerializeField] private CharacterModel[] characterModels;
        public override void Awake()
        {
            
        }
        
        /// <summary>
        /// Is called when the <see cref="_backButton"/> is pressed using SendMessage() from the UI object.
        /// </summary>
        public virtual void OnBackButtonPressed() {
            Controller.Show<QuantumMenuUIMain>();
        }
    }
}