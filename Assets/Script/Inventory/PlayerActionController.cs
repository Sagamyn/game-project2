    using UnityEngine;

    public class PlayerActionController : MonoBehaviour
    {
        public PlayerInteraction interaction;
        public PlayerFarming farming;
        public DialogueManager dialogue;

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.E))
                return;

            
            if (dialogue != null && dialogue.IsOpen)
                return;
        
            
            if (interaction != null && interaction.HasInteractable)
            {
                interaction.TryInteract();
                return;
            }

            
            if (farming != null)
            {
                
                farming.TryFarm();
            }
        }
    }
