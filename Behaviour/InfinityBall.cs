using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Reflection;
using DigitalRuby.ThunderAndLightning;
using DunGen;
using GameNetcodeStuff;
using LethalLib.Modules;
using Unity.Netcode;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace VarietyPackPlugin.Behaviour
{
    internal class InfinityBall : NetworkBehaviour
    {
        public int dmg = 80;
        public float speed = 3;

        public void OnCollisionEnter(Collision collision){
            ExplodeLocal();
            Destroy(this.gameObject);
        }
        [ClientRpc]
        public void InfinityExplosionClientRpc() {
            Explode();
            Destroy(gameObject);
        }
        
        // does exactly what it says on the tin
        [ServerRpc(RequireOwnership = false)]
        public void ServerInfinityExplosionServerRpc() {
            InfinityExplosionClientRpc();
        }

        // happens locally then calls the server
        public void ExplodeLocal() {
            Explode();
            ServerInfinityExplosionServerRpc();
        }

        // make that shit ROAR
        public void Explode() {
            SpawnExplosion(base.transform.position,true,15f,20f);
        }


        // stole this from the landmine code I have no idea how to do explosions LMAO
        public static void SpawnExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, float killRange = 1f, float damageRange = 1f)
        {
            Debug.Log("Spawning explosion at pos: {explosionPosition}");
            if (spawnExplosionEffect)
            {
                UnityEngine.Object.Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0f, 0f), RoundManager.Instance.mapPropsContainer.transform).SetActive(value: true);
            }
            float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, explosionPosition);
            if (num < 14f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
            else if (num < 25f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            }
            Collider[] array = Physics.OverlapSphere(explosionPosition, 6f, 2621448, QueryTriggerInteraction.Collide);
            PlayerControllerB playerControllerB = null;
            for (int i = 0; i < array.Length; i++)
            {
                float num2 = Vector3.Distance(explosionPosition, array[i].transform.position);
                if (num2 > 4f && Physics.Linecast(explosionPosition, array[i].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }
                if (array[i].gameObject.layer == 3)
                {
                    playerControllerB = array[i].gameObject.GetComponent<PlayerControllerB>();
                    if (playerControllerB != null && playerControllerB.IsOwner)
                    {
                        if (num2 < killRange)
                        {
                            Vector3 bodyVelocity = (playerControllerB.gameplayCamera.transform.position - explosionPosition) * 80f / Vector3.Distance(playerControllerB.gameplayCamera.transform.position, explosionPosition);
                            playerControllerB.KillPlayer(bodyVelocity, spawnBody: true, CauseOfDeath.Blast);
                        }
                        else if (num2 < damageRange)
                        {
                            playerControllerB.DamagePlayer(90);
                        }
                    }
                }
                else if (array[i].gameObject.layer == 19)
                {
                    EnemyAICollisionDetect componentInChildren2 = array[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
                    
                    if (componentInChildren2 != null && componentInChildren2.mainScript.IsOwner && num2 < 4.5f)
                    {
                        componentInChildren2.mainScript.HitEnemyOnLocalClient(6);
                    }
                }
            }
            int num3 = ~LayerMask.GetMask("Room");
            num3 = ~LayerMask.GetMask("Colliders");
            array = Physics.OverlapSphere(explosionPosition, 10f, num3);
            for (int j = 0; j < array.Length; j++)
            {
                Rigidbody component = array[j].GetComponent<Rigidbody>();
                if (component != null)
                {
                    component.AddExplosionForce(70f, explosionPosition, 10f);
                }
            }
        }
    
        


        // Call speed increase and constantly apply speed
        void FixedUpdate() {
            GetComponent<Rigidbody>().AddForce(this.transform.forward);
        }
    }
}
