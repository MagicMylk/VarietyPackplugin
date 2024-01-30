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

namespace VarietyPackPlugin.Behaviour
{
    internal class Lucky_Coin : GrabbableObject
    {
        public AudioSource CoinAudio;
        public AudioClip Slot;
        public AudioClip JackpotSuccess;
        public AudioClip JackpotMusic;
        public AudioClip JackpotFail;
        private PlayerControllerB previousPlayerHeldBy = null;
        private PlayerControllerB currentPlayer = null;


        private int jackpot = 0;
        private bool jackpotOn = false;
        private bool isRegenHealth = false;
        private bool currentlyHeld = false;
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            jackpot = UnityEngine.Random.RandomRange(0, 30);
       
            if (jackpot == 30)
            {
                if (IsHost)
                {
                    JackpotClientRpc();
                }
                else if (!IsHost) {
                    JackpotServerRpc();
                }
            }
            else if (jackpot < 30 && jackpot >15)
            {
                if (IsHost)
                {
                    JackpotMidClientRpc();
                }
                else if (!IsHost) {
                    JackpotMidServerRpc();
                }
            }
            else if (jackpot < 15)
            {
                CoinAudio.PlayOneShot(Slot);
                StartCoroutine(Womp(Slot));
                
            }

        }

       
        //This is probably the worst way to handle all these checks but I dont fuckin know anything about networking LMAO
        [ClientRpc]
        public void JackpotClientRpc() {
            CoinAudio.PlayOneShot(Slot);
            StartCoroutine(WaitForSound(Slot));
            playerHeldBy.movementSpeed = 8;
            jackpotOn = true;
            StartCoroutine(JackpotTimer(JackpotMusic));
        }

        [ServerRpc]
        public void JackpotServerRpc() {
            JackpotClientRpc();
        }

        [ClientRpc]
        public void JackpotMidClientRpc() {
            CoinAudio.PlayOneShot(Slot);
            playerHeldBy.DamagePlayer(-30);
        }

        [ServerRpc]
        public void JackpotMidServerRpc() {
            JackpotMidClientRpc();
        }

        public override void Update()
        {
            base.Update();  
            if(currentlyHeld == true)
            {
                if (isRegenHealth == false && jackpotOn == true && playerHeldBy.health < 100)
                {
                    StartCoroutine(RegainHealthOverTime());
                }
                if (playerHeldBy.health > 100 && currentlyHeld) { playerHeldBy.health = 100; }
            }   
        }

        
        // I dont think I need so many of these but I dont dare remove them lest I break everything
        public IEnumerator WaitForSound(AudioClip waitAudio)
        {
            yield return new WaitUntil(() => CoinAudio.isPlaying == false);
            if (this.gameObject != null)
            {
                CoinAudio.PlayOneShot(JackpotSuccess, 60f);
                CoinAudio.PlayOneShot(JackpotMusic, 40f);
            }
        }

        public IEnumerator JackpotTimer(AudioClip wait)
        {
            yield return new WaitUntil(() => CoinAudio.isPlaying == false);
            if (this.gameObject != null)
            {
                playerHeldBy.movementSpeed = 4.6f;
                jackpotOn = false;

            }
        }

        public IEnumerator Womp(AudioClip waitAudio){
            yield return new WaitUntil(()=>CoinAudio.isPlaying == false);
            if(this.gameObject != null) 
            {
                CoinAudio.PlayOneShot(JackpotFail);
                playerHeldBy.DamagePlayer(50);
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
            previousPlayerHeldBy = playerHeldBy;
            currentPlayer = playerHeldBy;
            currentlyHeld = true;
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            jackpotOn = false;
            StopAllCoroutines();
            CoinAudio.Stop();
            previousPlayerHeldBy.movementSpeed = 4.6f;
            currentlyHeld = false;

        }

        private IEnumerator RegainHealthOverTime()
        {
            isRegenHealth = true;
            while (playerHeldBy.health < 100)
            {
                playerHeldBy.DamagePlayer(-20);
                yield return new WaitForSeconds(0.2f);
            }

            isRegenHealth = false;
        }
    }
}
