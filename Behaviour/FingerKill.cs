using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
namespace VarietyPackPlugin.Behaviour
{
    internal class FingerKill : PhysicsProp
    {

        public AudioSource FingerAudio;
        public AudioClip cut;
        public AudioClip laugh;
        public AudioClip damn;
        private int randomNumber;
        private bool failTimer = false;
        private bool successTimer = false;

        //You may be wondering "why no networking?" Well all this shit is meant to be in your head so i decided to be lazy
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
           
            
            base.ItemActivate(used, buttonDown);
            randomNumber = UnityEngine.Random.Range(0, 100);

            if (buttonDown && failTimer == false && randomNumber <= 99) {
                FingerAudio.PlayOneShot(cut);
                StartCoroutine(WaitForSound(cut));
                failTimer = true;
            }

            if (buttonDown && successTimer == false && randomNumber == 100) {
                playerHeldBy.health = 200;
                playerHeldBy.movementSpeed = 13.8f;
                FingerAudio.PlayOneShot(laugh);
                HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", true);
                StartCoroutine(WaitSeconds(300f));
                successTimer = true;
                
            }

        }

        public IEnumerator WaitForSound(AudioClip waitAudio)
        {
            yield return new WaitUntil(() => FingerAudio.isPlaying == false);
            if (this.gameObject != null) {
                playerHeldBy.DamagePlayer(90);
                failTimer = false;

            }
        }

        public IEnumerator WaitSeconds(float seconds) { 
            yield return new WaitForSeconds(seconds);
            playerHeldBy.DamagePlayer(100);
            playerHeldBy.movementSpeed = 4.6f;
            HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", false);
            FingerAudio.PlayOneShot(damn);
            successTimer = false;
        }

    }
}
