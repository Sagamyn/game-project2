    using UnityEngine;
    using System.Collections.Generic;

    [CreateAssetMenu(menuName = "Quest/Quest")]
    public class QuestData : ScriptableObject
    {
        public string questName;

        [TextArea]
        public List<string> startDialogue;
        [TextArea] public string incompleteDialogue;

        [TextArea]
        public List<string> completeDialogue;

        [Header("Quest Requirements")]
        public ItemData requiredItem;
        public int requiredAmount = 1;

        [Header("Rewards")]
        public ItemData rewardItem;
        public int rewardAmount = 1;
    }
