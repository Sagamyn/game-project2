using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    [Header("References")]
    public PlayerInteraction interaction;
    public PlayerFarming farming;
    public DialogueManager dialogue;
    public PlayerToolController toolController;
    public Hotbar hotbar;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        //  Dialogue blocks everything
        if (dialogue != null && dialogue.IsOpen)
            return;

        //  Interactions have priority
        if (interaction != null && interaction.HasInteractable)
        {
            interaction.TryInteract();
            return;
        }

        //  Hotbar check
        if (hotbar == null)
        {
            Debug.LogError("PlayerActionController: Hotbar is NULL");
            return;
        }

        ItemData selected = hotbar.SelectedItem;

        //  Tool usage
        if (selected is ToolItem tool)
        {
            if (toolController == null)
            {
                Debug.LogError("PlayerActionController: ToolController is NULL");
                return;
            }

            toolController.UseTool(tool);
            return;
        }

        //  Farming (seeds / harvesting)
        if (farming != null)
        {
            farming.TryFarm();
        }
        else
        {
            Debug.LogError("PlayerActionController: Farming is NULL");
        }
    }
}
