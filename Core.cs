using MelonLoader;
using UnityEngine;
using Il2CppScheduleOne.Trash;
using HarmonyLib;

[assembly: MelonInfo(typeof(TrashGrabPlus.Core), "TrashDestroyer", "1.1.0", "heimy", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace TrashGrabPlus
{
    [HarmonyPatch(typeof(TrashItem))]
    [HarmonyPatch("AddTrash")]
    [HarmonyPatch(new Type[]
        {
        typeof(TrashItem),
    })]

    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.RightBracket)) // ']' key
            {
                UseTrashGrabber();
            }

            if (Input.GetKeyDown(KeyCode.LeftBracket)) // '[' key
            {
                UseTrashGrabberNearPlayer();
            }
        }

        private void UseTrashGrabber()
        {
            Vector3 barnPosition = new Vector3(190.2984f, 1.065f, -11.6897f);
            Vector3 docksPosition = new Vector3(-86.7962f, -1.255f, -48.1173f);
            Vector3 sweatshopPosition = new Vector3(-61.8378f, 0.715f, 138.1508f);
            Vector3 housePosition = new Vector3(-172.1976f, -2.735f, 114.9906f);
            float radius = 20.0f;


            int barnCount = CheckAndPickUpTrashItems(barnPosition, radius);
            LoggerInstance.Msg($"Deleted {barnCount} trash items at the barn.");

            int docksCount = CheckAndPickUpTrashItems(docksPosition, radius);
            LoggerInstance.Msg($"Deleted {docksCount} trash items at the docks.");

            int sweatshopCount = CheckAndPickUpTrashItems(sweatshopPosition, radius);
            LoggerInstance.Msg($"Deleted {sweatshopCount} trash items at the sweatshop.");

            int houseCount = CheckAndPickUpTrashItems(housePosition, radius);
            LoggerInstance.Msg($"Deleted {houseCount} trash items at the house.");
        }

        private void UseTrashGrabberNearPlayer()
        {
            GameObject player = GameObject.Find("Player_Local");
            if (player == null)
            {
                LoggerInstance.Msg("Player_Local GameObject not found.");
                return;
            }

            Vector3 playerPosition = player.transform.position;
            float radius = 10.0f;

            int playerCount = CheckAndPickUpTrashItems(playerPosition, radius);
            LoggerInstance.Msg($"Deleted {playerCount} trash items near the player.");
        }

        private int CheckAndPickUpTrashItems(Vector3 position, float radius)
        {

            TrashItem[] allTrashItems = GameObject.FindObjectsOfType<TrashItem>();
            int count = 0;
            foreach (var trashItem in allTrashItems)
            {
                float distance = Vector3.Distance(position, trashItem.transform.position);
                if (distance <= radius)
                {
                    PickUpTrashItem(trashItem);
                    count++;
                }
            }
            return count;
        }

        private void PickUpTrashItem(TrashItem trashItem)
        {
            GameObject.Destroy(trashItem.gameObject);
        }
    }
}


