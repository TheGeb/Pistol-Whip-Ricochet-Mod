using Harmony;
using MelonLoader;
using System;
using UnityEngine;

namespace Pistol_Whip_Ricochet_Mod
{
    public class Ricochet : MelonMod
    {

        static Gun gun;

        [HarmonyPatch(typeof(Gun), "Fire", new Type[0])]
        public static class gunricEnd
        {
            //Must run after gun spinning! Set priority to last
            private static void Prefix(Gun __instance)
            {
                gun = __instance;   //Use the same gun that fired the original shot to fire a ricochet
            }
        }

        //CalculateHit is called on misses? Dont bounce on UI
        [HarmonyPatch(typeof(Bullet), "CalculateHit", new Type[] { })]
        public static class BulletCalcHit
        {

            // reset pos
            public static Vector3 initialPoint;
            // reset angle
            public static Quaternion initialAngle;

            public static bool isBounceShot = false;

            static bool patchPost = true;

            private static void Prefix(Bullet __instance)
            {

                if (isBounceShot == false)
                {
                    // create a raycasthit 
                    RaycastHit hit;
                    // cast said raycasthit and see if it hits anything.  ray is fire from the muzzle in it's forward direction
                    if (Physics.Raycast(__instance.ray.origin, __instance.ray.direction, out hit, 200f) //bounce only if we hit something...
                        && !(hit.transform.root.name.ToLower().Contains("enemy")    //and we didn't hit an enemy or UI element
                            || hit.transform.name.ToLower().Contains("chui")))
                    {
                        //MelonLogger.Log(hit.transform.name);
                        //MelonLogger.Log(hit.transform.root.name);

                            initialAngle = gun.attachPoint.localRotation; // set reset rotation
                            initialPoint = gun.attachPoint.position; // set reset point

                            gun.attachPoint.forward = Vector3.Reflect(gun.trueMuzzle.TransformDirection(Vector3.forward), hit.normal); // rotate the gun based off of the surface normal
                            gun.attachPoint.position = hit.point; // move gun to hit point.  attachPoint is the controller's position, basically

                            gun.BulletCount++;

                            isBounceShot = true;
                            gun.Fire();  //technically the ricochet happens before the original bullet
                            isBounceShot = false;
                            patchPost = true;
                        
                    }
                    else
                    {
                        patchPost = false;
                    }
                }
            }

            private static void Postfix()
            {
                if (!isBounceShot && patchPost)
                {
                    gun.attachPoint.position = initialPoint; // set reset point
                    gun.attachPoint.localRotation = initialAngle; // set reset rotation
                    isBounceShot = false;
                }
            }
        }

        //HitTarget is only called on hits!
        [HarmonyPatch(typeof(Bullet), "HitTarget", new Type[] { })]
        public static class BulletFireAtTarget
        {
            private static void Prefix(Bullet __instance)
            {
                //MelonLogger.Log("Enemy Hit!");
            }

        }
    }
}
