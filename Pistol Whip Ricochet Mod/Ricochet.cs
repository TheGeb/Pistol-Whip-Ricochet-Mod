using System;
using UnityEngine;
using Harmony;
using MelonLoader;

namespace Pistol_Whip_Ricochet_Mod
{
    public class Ricochet : MelonMod
    {
        
        
            // reset pos
            public static Vector3 reset;
            // reset angle
            public static Quaternion eul;
            // bullet count
            public static int bulls;
            // ricochet activation
            public static bool rick = false;
            // infinite ammo mod check
            public static bool inf = true;

            public static bool bounceShot = false;


        // Harmony patch of the function Fire() in the Gun class
        [HarmonyPatch(typeof(Gun), "Fire", new Type[0])]
        public static class gunric
        {

            static float correctionCheck;
            // Postfix of Fire(). this code runs after the game's code.  A Prefix would run this code before the game's code
            // __instance is used to modify and call functions in the Gun class.  required by harmony
            private static void Prefix(Gun __instance)
            {
                correctionCheck = __instance.aimAssist.correction;
            }

                private static void Postfix(Gun __instance)
            {
                // check to see if the bullet is ricocheting, and if the is ammo
                if (!rick && __instance.BulletCount != 0 && !bounceShot)
                {
                    // create a raycasthit 
                    RaycastHit hit;
                    // cast said raycasthit and see if it hits anything.  ray is fire from the muzzle in it's forward direction
                    if (Physics.Raycast(__instance.trueMuzzle.position, __instance.trueMuzzle.TransformDirection(Vector3.forward), out hit, 200f))
                    {
                        rick = true;
                        // set reset point
                        reset = __instance.attachPoint.position;
                        // get normal of hit object
                        Vector3 norm = hit.normal;
                        // set bulletcount for later
                        bulls = __instance.BulletCount;
                        // set reset rotation
                        eul = __instance.attachPoint.localRotation;
                        // move gun to hit point.  attachPoint is the controller's position, basically
                        __instance.attachPoint.position = hit.point;
                        // rotate the gun based off of the surface normal (see Reflec.png)

                        __instance.attachPoint.forward = Vector3.Reflect(__instance.trueMuzzle.TransformDirection(Vector3.forward), norm);
                        __instance.BulletCount++;

                            if (!(hit.transform.root.name.ToLower().Contains("enemy")
                                || hit.collider.material.name.ToLower().Contains("flesh")
                                || hit.transform.gameObject.name.Contains("mixamorig")
                                || correctionCheck != __instance.aimAssist.correction
                                )) //Don't bounce off of enemies...
                            {
                                bounceShot = true;
                                __instance.Fire();
                                bounceShot = false;
                            }
                    }
                    // this part code will be run during the ricochet shot
                }
                if (rick && !bounceShot)
                {
                    
                    // reset the gun's pos
                    __instance.attachPoint.position = reset;
                    // reset the gun's angle
                    __instance.attachPoint.localRotation = eul;
                    //__instance.attachPoint.localPosition = new Vector3(0, 0, -0.04f);
                    rick = false;
                }
            }
        }
    }
}
