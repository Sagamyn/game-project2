using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    QuestData currentQuest;
    bool completed;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool HasQuest => currentQuest != null;
    public bool IsCompleted => completed;
    public QuestData CurrentQuest => currentQuest;

    public void StartQuest(QuestData quest)
    {
        currentQuest = quest;
        completed = false;
    }

    public bool CanComplete(PlayerInventory inventory)
    {
        if (currentQuest == null) return false;

        return inventory.GetAmount(currentQuest.requiredItem)
            >= currentQuest.requiredAmount;
    }

    public void CompleteQuest(PlayerInventory inventory)
    {
        Debug.Log("CompleteQuest() called");

        if (!CanComplete(inventory))
        {
            Debug.Log("Cannot complete quest");
            return;
        }

        inventory.ConsumeItem(
            currentQuest.requiredItem,
            currentQuest.requiredAmount
        );

        if (currentQuest.rewardItem != null)
        {
            Debug.Log($"Giving reward: {currentQuest.rewardItem.itemName} x{currentQuest.rewardAmount}");
            inventory.AddItem(
                currentQuest.rewardItem,
                currentQuest.rewardAmount
            );
        }
        else
        {
            Debug.LogWarning("Reward item is NULL!");
        }

        completed = true;
    }

    public void ClearQuest()
    {
        currentQuest = null;
        completed = false;
    }
}
