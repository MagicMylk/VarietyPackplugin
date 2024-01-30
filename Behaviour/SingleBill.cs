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

namespace VarietyPackPlugin.Behaviour
{

     
    internal class SingleBill : PhysicsProp
    {

        private PlayerControllerB previousPlayerHeldBy;
        public override void PocketItem()
        {
            base.PocketItem();
            playerHeldBy.activatingItem = false;
        }

        public override void EquipItem()
        {
            base.EquipItem();
            previousPlayerHeldBy = playerHeldBy;
        }
    }
}
