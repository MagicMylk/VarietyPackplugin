using System;
using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using System.Numerics;

namespace VarietyPackPlugin.Behaviour
{
    internal class Money : GrabbableObject
    {
        public AudioSource MoneyAudio;
        public AudioClip Embarrassing;
        public AudioClip Laugh;
        public GameObject billPrefab;
        private int ammo = 0;
        private UnityEngine.Vector3 billVector;
        private GameObject billInstance = null;
        private PlayerControllerB previousPlayerHeldBy;
        private bool hasAmmo = false;
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (ammo >= 1)
            {
                if (IsHost && ammo >= 1)
                {
                    SpawnBillClientRpc();

                    ammo = ammo - 1;
                }
                else if (!IsHost && ammo >= 1)
                {
                    SpawnBillServerRpc();

                    ammo = ammo - 1;
                }
            }
            else 
            {
                DestroyObjectInHand(playerHeldBy);
            }
        }

        [ClientRpc]
        public void SpawnBillClientRpc()
        {
            MoneyAudio.PlayOneShot(Embarrassing);
            StartCoroutine(WaitForSound(Embarrassing));
            billVector = base.transform.position + UnityEngine.Vector3.up * 0.25f;
            billInstance = Instantiate(billPrefab, billVector, UnityEngine.Quaternion.identity);
            SingleBill component = billInstance.GetComponent<SingleBill>();
            component.grabbable = true;
            component.grabbableToEnemies = true;
            component.fallTime = 0f;
            component.startFallingPosition = billVector;
            component.NetworkObject.Spawn();
            component.hasHitGround = false;
            component.reachedFloorTarget = false;
        }

        [ServerRpc]
        public void SpawnBillServerRpc() {
            SpawnBillClientRpc();
        }

        //I really dont think this is necessary but I dont yet know another solution
        public IEnumerator WaitForSound(AudioClip waitAudio)
        {
            yield return new WaitUntil(() => MoneyAudio.isPlaying == false);
            if (this.gameObject != null)
            {
                MoneyAudio.PlayOneShot(Laugh);
            }

        }
        public override void PocketItem()
        {
            base.PocketItem();
            playerHeldBy.activatingItem = false;
        }

        public override void EquipItem()
        {
            base.EquipItem();
            if (hasAmmo == false)
            {
                ammo = UnityEngine.Random.RandomRange(3, 12);
                hasAmmo = true;
            }
            else {
                return;
            }
            previousPlayerHeldBy = playerHeldBy;
        }
    }
}
