using UnityEngine;
using TMPro;

public class QuestTrackerUI : MonoBehaviour
{
    public GameObject panel;

    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questName;
    public TextMeshProUGUI questProgress;

    PlayerInventory inventory;

    void Start()
    {
        inventory = FindObjectOfType<PlayerInventory>();
        panel.SetActive(false);

        if (inventory != null)
            inventory.OnInventoryChanged += Refresh;
    }

    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        QuestManager qm = QuestManager.Instance;

        if (qm == null || !qm.HasQuest)
        {
            panel.SetActive(false);
            return;
        }

        panel.SetActive(true);

        QuestData quest = qm.CurrentQuest;

        questTitle.text = "Quest";
        questName.text = quest.questName;

        int have = inventory.GetAmount(quest.requiredItem);
        questProgress.text =
            have + " / " + quest.requiredAmount;
    }
}
