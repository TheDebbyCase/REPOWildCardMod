using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
namespace REPOWildCardMod.Source.Items
{
    public class SmithNote : MonoBehaviour
    {
        public PhysGrabObject physGrabObject;
        public ItemBattery itemBattery;
        public Sound bookSound;
        public Animator animator;
        public TextMeshPro titleText;
        public TextMeshPro[] pageText;
        public bool opened;
        public bool charged;
        public Vector3 tempVector = new Vector3(-60f, 0f, 15f);
        public void FixedUpdate()
        {
            if (physGrabObject.grabbed)
            {
                int nonRotatingGrabbers = physGrabObject.playerGrabbing.Count;
                for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
                {
                    if (physGrabObject.playerGrabbing[i].isRotating)
                    {
                        nonRotatingGrabbers--;
                    }
                }
                if (nonRotatingGrabbers < physGrabObject.playerGrabbing.Count)
                {
                    physGrabObject.TurnXYZ(Quaternion.Euler(tempVector.x, 0f, 0f), Quaternion.Euler(0f, tempVector.y, 0f), Quaternion.Euler(0f, 0f, tempVector.z));
                }
            }
        }
        public void Update()
        {
            if (physGrabObject.grabbed && !opened)
            {
                opened = true;
                List<string> playerNames = StatsManager.instance.playerNames.Values.ToList();
                playerNames.Sort();
                string pageOneString = string.Empty;
                string pageTwoString = string.Empty;
                if (playerNames.Count <= 6)
                {
                    for (int i = 0; i < Mathf.Min(playerNames.Count, 3); i++)
                    {
                        pageOneString += $"{playerNames[i]}\n";
                    }
                    pageOneString.Remove(pageOneString.Length - 2);
                    pageText[0].text = pageOneString;
                    if (playerNames.Count > 3)
                    {
                        for (int i = 3; i < Mathf.Min(playerNames.Count, 6); i++)
                        {
                            pageTwoString += $"{playerNames[i]}\n";
                        }
                        pageTwoString.Remove(pageTwoString.Length - 2);
                        pageText[1].text = pageTwoString;
                    }
                    else
                    {
                        pageText[1].text = string.Empty;
                    }
                }
                else
                {
                    for (int i = 0; i < Mathf.Max(Mathf.FloorToInt((float)playerNames.Count / 2f), 6); i++)
                    {
                        pageOneString += $"{playerNames[i]}\n";
                    }
                    pageOneString.Remove(pageOneString.Length - 2);
                    pageText[0].text = pageOneString;
                    for (int i = Mathf.Max(Mathf.FloorToInt((float)playerNames.Count / 2f), 6); i < playerNames.Count; i++)
                    {
                        pageTwoString += $"{playerNames[i]}\n";
                    }
                    pageTwoString.Remove(pageTwoString.Length - 2);
                    pageText[1].text = pageTwoString;
                }
                animator.SetTrigger("Flip");
            }
            else if (!physGrabObject.grabbed && opened)
            {
                opened = false;
                animator.SetTrigger("Flip");
            }
            if (itemBattery.batteryLife <= 0f && charged)
            {
                titleText.text = ":(";
                for (int i = 0; i < pageText.Length; i++)
                {
                    pageText[i].gameObject.SetActive(false);
                }
                charged = false;
            }
            else if (itemBattery.batteryLife > 0f && !charged)
            {
                titleText.text = "SMITH\nNOTE";
                for (int i = 0; i < pageText.Length; i++)
                {
                    pageText[i].gameObject.SetActive(true);
                }
                charged = true;
            }
        }
    }
}