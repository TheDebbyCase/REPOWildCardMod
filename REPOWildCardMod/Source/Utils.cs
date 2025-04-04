using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
namespace REPOWildCardMod.Utils
{
    public class WildCardUtils
    {
        public bool TextIsSimilar(string first, string second)
        {
            string firstReplaced = first.Replace(" ", string.Empty);
            string secondReplaced = second.Replace(" ", string.Empty);
            if (first == second)
            {
                return true;
            }
            else if (first == second.ToLower() || first == second.ToUpper())
            {
                return true;
            }
            else if (first == second.TrimStart() || first == second.TrimEnd())
            {
                return true;
            }
            else if (first == second[1..] || first == second[..(second.Length - 2)])
            {
                return true;
            }
            else if (firstReplaced == secondReplaced)
            {
                return true;
            }
            else if (firstReplaced == secondReplaced.ToLower() || firstReplaced == secondReplaced.ToUpper())
            {
                return true;
            }
            else if (firstReplaced == secondReplaced.TrimStart() || firstReplaced == secondReplaced.TrimEnd())
            {
                return true;
            }
            else if (firstReplaced == secondReplaced[1..] || firstReplaced == secondReplaced[..(secondReplaced.Length - 2)])
            {
                return true;
            }
            return false;
        }
        public string CleanText(string text)
        {
            string newText = Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
            if (newText == string.Empty)
            {
                newText = $"this is nicer {text.Length}";
            }
            return newText;
        }
    }
    public class SuperSonic : MonoBehaviour
    {
        public float overrideTimer;
        public Sound sonicLoop;
        public PlayerAvatar[] playersList;
        public void Start()
        {
            overrideTimer = 120f;
            playersList = SemiFunc.PlayerGetAll().ToArray();
            sonicLoop.LowPassIgnoreColliders.Add(PlayerController.instance.col);
        }
        public void Update()
        {
            for (int i = 0; i < playersList.Length; i++)
            {
                if (playersList[i] != null)
                {
                    if (playersList[i].isLocal)
                    {
                        sonicLoop.PlayLoop(overrideTimer > 10f, 2f, 0.5f);
                        PlayerAvatar.instance.voiceChat.OverridePitch(1.5f, 1f, 0.25f);
                        PlayerAvatar.instance.OverridePupilSize(0.3f, 4, 0.25f, 1f, 5f, 0.5f);
                        PlayerController.instance.OverrideSpeed(2.5f);
                        PlayerController.instance.OverrideLookSpeed(2f, 0.5f, 1f);
                        PlayerController.instance.OverrideAnimationSpeed(2.5f, 1f, 0.5f);
                        PlayerController.instance.OverrideTimeScale(2.5f);
                        if (PhysGrabber.instance.grabbedPhysGrabObject != null)
                        {
                            PhysGrabber.instance.grabbedPhysGrabObject.OverrideTorqueStrength(1.5f);
                        }
                        CameraZoom.Instance.OverrideZoomSet(90f, 0.1f, 1f, 0.5f, null, 0);
                        PostProcessing.Instance.SaturationOverride(-30f, 0.5f, 0.1f, 0.1f, null);
                    }
                    else
                    {
                        playersList[i].voiceChat.OverridePitch(1.5f, 1f, 0.25f);
                    }
                }
            }
            overrideTimer -= Time.deltaTime;
            if (!SemiFunc.RunIsLevel() && overrideTimer > 5f)
            {
                overrideTimer = 5f;
            }
            if (overrideTimer <= 0f)
            {
                Destroy(this.gameObject);
            }
        }
    }
}