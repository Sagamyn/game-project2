using UnityEngine;
using System.Collections.Generic;

public class NPC : Interactable
{
    [Header("Quest List (in order)")]
    public List<QuestData> quests;

    int questIndex = 0;

    public override void Interact()
    {
        if (quests == null || quests.Count == 0)
        {
            DialogueManager.Instance.ShowDialogue("Hello!");
            return;
        }

        if (questIndex >= quests.Count)
        {
            DialogueManager.Instance.ShowDialogue("I have nothing more for you.");
            return;
        }

        QuestManager qm = QuestManager.Instance;
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();

        QuestData quest = quests[questIndex];

        // No active quest → start
        if (!qm.HasQuest)
        {
            qm.StartQuest(quest);
            DialogueManager.Instance.ShowDialogue(quest.startDialogue);
            return;
        }

        // Active quest but not completed
        if (!qm.IsCompleted)
        {
            if (qm.CanComplete(inv))
            {
                qm.CompleteQuest(inv);
                DialogueManager.Instance.ShowDialogue(quest.completeDialogue);

                questIndex++;
                qm.ClearQuest();
            }
            else
            {
                DialogueManager.Instance.ShowDialogue(
                    quest.incompleteDialogue
                );
            }
        }
    }
}
