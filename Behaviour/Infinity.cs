using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;
using System.Numerics;
using BepInEx;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.TextCore.Text;

namespace VarietyPackPlugin.Behaviour
{
    internal class Infinity : GrabbableObject
    {
        public GameObject infinityBallPrefab;
        public GameObject NukeComponentRed;
        public GameObject NukeComponentBlue;
        public AudioSource infinityAudio;
        public AudioClip purple;
        public AudioClip pew;
        public Transform aimDirection;
        private PlayerControllerB player;

        public AudioClip NukeStartup;
        public AudioClip NukeWhirr;
        public AudioClip NukeExplode;

        // Does two separate things. 1) spawn an explosive orb inside the facility if used inside. 2) Spawn a gigantic explosion if used outside and at 20hp **MIGHT DISABLE CAUSE THAT MIGHT BE TOO MUCH**
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (IsHost && playerHeldBy.isInsideFactory == true)
            {
                playerHeldBy.DamagePlayer(80);
                PurpleSpawnClientRpc();
            }
            else if (!IsHost && playerHeldBy.isInsideFactory == true)
            {
                playerHeldBy.DamagePlayer(80);
                ServerPurpleSpawnServerRpc();
            }
            else if (IsHost && playerHeldBy.isInsideFactory == false && playerHeldBy.health <= 20)
            {
               playerHeldBy.movementSpeed = 0f;
               NukeComponentsClientRpc();

            }
            else if (!IsHost && playerHeldBy.isInsideFactory == false && playerHeldBy.health <= 20) {
                playerHeldBy.movementSpeed = 0f;
                NukeComponentServerRpc();
            }
        }

        [ClientRpc]
        public void PurpleSpawnClientRpc() {

            infinityAudio.PlayOneShot(purple);
            StartCoroutine(WaitForPurple(purple,playerHeldBy.transform.position + (playerHeldBy.transform.forward*3f) + (playerHeldBy.transform.up*2f)));
        }

        [ServerRpc]
        public void ServerPurpleSpawnServerRpc() {
            PurpleSpawnClientRpc();
        }

        [ClientRpc]
        public void NukeComponentsClientRpc() {
            Vector3 offset = new Vector3(0.15f, 0.6f, 0.15f);
            infinityAudio.PlayOneShot(NukeStartup);
            GameObject blue = Instantiate(NukeComponentBlue, playerHeldBy.transform.position + offset, playerHeldBy.transform.rotation);
            Destroy(blue, 11f);
            GameObject red = Instantiate(NukeComponentRed, playerHeldBy.transform.position + offset, playerHeldBy.transform.rotation);
            Destroy(red, 11f);
            GameObject ourple = Instantiate(infinityBallPrefab, playerHeldBy.transform.position + offset + new Vector3(0, 6f, 0), playerHeldBy.transform.rotation);
            ourple.transform.localScale = new Vector3(15, 15, 15);
            Vector3 currentScale = ourple.transform.localScale;
            Destroy(ourple.GetComponent<Rigidbody>());
            Destroy(ourple.GetComponent<BoxCollider>());
            Destroy(ourple.GetComponent<InfinityBall>());
            Destroy(ourple,6f);
            StartCoroutine(WaitforStartup(NukeStartup));



        }

        [ServerRpc]
        public void NukeComponentServerRpc() {
           NukeComponentsClientRpc(); 
        }
        // Nuke the whole outside
        public IEnumerator WaitforStartup(AudioClip wait) {
            Vector3 offset = new Vector3(0.15f, 0.6f, 0.15f);
            yield return new WaitUntil(() => infinityAudio.isPlaying == false);
            infinityAudio.PlayOneShot(NukeWhirr);
            infinityAudio.maxDistance = 100f;
            playerHeldBy.movementSpeed = 4.6f;
            yield return new WaitForSeconds(3f);
            BoomLocal();
            yield return new WaitForSeconds(3f);
            infinityAudio.maxDistance = 15f;

        }

        // go pew
        public IEnumerator WaitForPurple(AudioClip wait, UnityEngine.Vector3 aimPosition) 
        {
            yield return new WaitUntil(() => infinityAudio.isPlaying == false);
            infinityAudio.PlayOneShot(pew);
            yield return new WaitUntil(() => infinityAudio.isPlaying == false);
            GameObject infinityBall = Instantiate(infinityBallPrefab, aimPosition, playerHeldBy.transform.rotation);
        }

        public void BoomLocal() {
            InfinityBall.SpawnExplosion(playerHeldBy.transform.position, true, 500f, 1000f);
            BoomServerRpc();
        }

        [ServerRpc]
        public void BoomServerRpc() {
            BoomClientRpc();
        }

        [ClientRpc] 
        public void BoomClientRpc() {
            InfinityBall.SpawnExplosion(playerHeldBy.transform.position, true, 500f, 1000f);
        }
        public override void EquipItem()
        {
            base.EquipItem();
            player = playerHeldBy;
        }

    }
}
