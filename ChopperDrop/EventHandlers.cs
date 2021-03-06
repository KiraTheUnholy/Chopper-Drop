using System;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EXILED;

namespace ChopperDrop
{
    public class EventHandlers
    {
        public Plugin pl;
        public ChopperDrops allowedItems;

        public int time;

        public List<CoroutineHandle> coroutines = new List<CoroutineHandle>();

        public bool roundStarted = false;

        public EventHandlers(Plugin plugin, ChopperDrops drops, int tim) 
        { 
            pl = plugin;
            allowedItems = drops;
            time = tim;
        }

        internal void RoundStart()
        {
            roundStarted = true;
            foreach (CoroutineHandle handle in coroutines)
                Timing.KillCoroutines(handle);
            Plugin.Info("Starting the ChopperThread.");
            coroutines.Add(Timing.RunCoroutine(ChopperThread(), "ChopperThread"));
        }

        internal void WaitingForPlayers()
        {
            foreach (CoroutineHandle handle in coroutines)
                Timing.KillCoroutines(handle);
        }

        public IEnumerator<float> ChopperThread()
        {
            while(roundStarted)
            {
                // Unity GARBAGE
                Plugin.Info("Chopper thread waiting 10 minutes.");
                yield return Timing.WaitForSeconds(time); // Wait seconds (10 minutes by defualt)
                Plugin.Info("Spawning chopper!");
                ChopperAutostart ca = UnityEngine.Object.FindObjectOfType<ChopperAutostart>(); // Get the chopper
                ca.SetState(true); // Call the chopper to come

                foreach (ReferenceHub h in Plugin.GetHubs()) // Broadcast to everyone that a supply drop was called down.
                    h.Broadcast(5, "<color=yellow>ALERT: A supply drop helicopter has been called down, all available MTF Units proceeded to the surface for supplys</color>");

                yield return Timing.WaitForSeconds(15); // Wait 15 seconds

                Vector3 spawn = Plugin.GetRandomSpawnPoint(RoleType.NtfCadet); // Get the spawn point of the chopper to you know, spawn em.

                foreach (KeyValuePair<ItemType, int> drop in allowedItems.drops) // Drop items
                {
                    Plugin.Info("Spawning " + drop.Value + " " + drop.Key.ToString() + "'s");
                    for (int i = 0; i < drop.Value; i++)
                    {
                        SpawnItem(drop.Key, spawn, spawn);
                    }
                }
                ca.SetState(false); // Call the chopper to leave
                yield return Timing.WaitForSeconds(15); // Wait 15 seconds to let the chopper leave.
            }
        }

        public int ItemDur(ItemType weapon)
        {
            
            int COM15Ammo = 12;
            int USPAmmo = 18;
            int ARAmmo = 40;
            int LogicerAmmo = 100;
            int P90Ammo = 50;
            int SMGAmmo = 35;
            int Ammo762 = 25;
            int Ammo9mm = 25;
            int Ammo556 = 25;

            switch (weapon)
            {
                case ItemType.GunCOM15:
                    return COM15Ammo;
                case ItemType.GunE11SR:
                    return ARAmmo;
                case ItemType.GunProject90:
                    return P90Ammo;
                case ItemType.GunMP7:
                    return SMGAmmo;
                case ItemType.GunLogicer:
                    return LogicerAmmo;
                case ItemType.GunUSP:
                    return USPAmmo;
                case ItemType.Ammo762:
                    return Ammo762;
                case ItemType.Ammo9mm:
                    return Ammo9mm;
                case ItemType.Ammo556:
                    return Ammo556;
                default:
                    return 50;
            }
        }

        public void SpawnItem(ItemType type, Vector3 pos, Vector3 rot)
        {
            PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(type, ItemDur(type), pos, Quaternion.Euler(rot), 0, 0, 0);
        }
    }
}
