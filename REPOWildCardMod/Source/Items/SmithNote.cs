//using Photon.Pun;
//using REPOWildCardMod.Utils;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//namespace REPOWildCardMod.Items
//{
//    public class SmithNote : MonoBehaviour
//    {
//        public PhotonView photonView;
//        public PhysGrabObject physGrabObject;
//        public ItemBattery itemBattery;
//        public ItemToggle itemToggle;
//        public Sound bookSound;
//        public Animator animator;
//        public TextMeshPro titleText;
//        public TextMeshPro[] pageText;
//        public List<string> playerNames = new List<string>();
//        public float animNormal = 0f;
//        public bool opened;
//        public bool charged;
//        public Vector3 forceRotation = new Vector3(-60f, 0f, 15f);
//        public void FixedUpdate()
//        {
//            if (physGrabObject.grabbed)
//            {
//                int nonRotatingGrabbers = physGrabObject.playerGrabbing.Count;
//                for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
//                {
//                    if (physGrabObject.playerGrabbing[i].isRotating)
//                    {
//                        nonRotatingGrabbers--;
//                    }
//                }
//                if (nonRotatingGrabbers == physGrabObject.playerGrabbing.Count)
//                {
//                    physGrabObject.TurnXYZ(Quaternion.Euler(forceRotation.x, 0f, 0f), Quaternion.Euler(0f, forceRotation.y, 0f), Quaternion.Euler(0f, 0f, forceRotation.z));
//                }
//            }
//        }
//        public void Update()
//        {
//            if (physGrabObject.grabbed && !opened && itemBattery.batteryLife > 0f)
//            {
//                RefreshPlayerList();
//                if (SemiFunc.IsMasterClientOrSingleplayer())
//                {
//                    string pageOneString = string.Empty;
//                    string pageTwoString = string.Empty;
//                    if (playerNames.Count <= 6)
//                    {
//                        for (int i = 0; i < Mathf.Min(playerNames.Count, 3); i++)
//                        {
//                            pageOneString += $"{playerNames[i]}\n";
//                        }
//                        pageOneString.Remove(pageOneString.Length - 2);
//                        SetPageText(0, pageOneString);
//                        if (playerNames.Count > 3)
//                        {
//                            for (int i = 3; i < Mathf.Min(playerNames.Count, 6); i++)
//                            {
//                                pageTwoString += $"{playerNames[i]}\n";
//                            }
//                            pageTwoString.Remove(pageTwoString.Length - 2);
//                            SetPageText(1, pageTwoString);
//                        }
//                        else
//                        {
//                            SetPageText(1, string.Empty);
//                        }
//                    }
//                    else
//                    {
//                        for (int i = 0; i < Mathf.Max(Mathf.FloorToInt((float)playerNames.Count / 2f), 6); i++)
//                        {
//                            pageOneString += $"{playerNames[i]}\n";
//                        }
//                        pageOneString.Remove(pageOneString.Length - 2);
//                        SetPageText(0, pageOneString);
//                        for (int i = Mathf.Max(Mathf.FloorToInt((float)playerNames.Count / 2f), 6); i < playerNames.Count; i++)
//                        {
//                            pageTwoString += $"{playerNames[i]}\n";
//                        }
//                        pageTwoString.Remove(pageTwoString.Length - 2);
//                        SetPageText(1, pageTwoString);
//                    }
//                }
//                opened = true;
//            }
//            else if ((!physGrabObject.grabbed || itemBattery.batteryLife <= 0f) && opened)
//            {
//                opened = false;
//            }
//            if (itemBattery.batteryLife <= 0f && charged)
//            {
//                titleText.text = ":(";
//                for (int i = 0; i < pageText.Length; i++)
//                {
//                    pageText[i].gameObject.SetActive(false);
//                }
//                charged = false;
//            }
//            else if (itemBattery.batteryLife > 0f && !charged)
//            {
//                titleText.text = "SMITH\nNOTE";
//                for (int i = 0; i < pageText.Length; i++)
//                {
//                    pageText[i].gameObject.SetActive(true);
//                }
//                charged = true;
//            }
//            if (opened)
//            {
//                if (animNormal < 1f)
//                {
//                    animNormal += Time.deltaTime;
//                }
//                else if (animNormal > 1f)
//                {
//                    animNormal = 1f;
//                }
//                if (itemToggle.toggleState)
//                {

//                }
//            }
//            else
//            {
//                if (animNormal > 0f)
//                {
//                    animNormal -= Time.deltaTime;
//                }
//                else if (animNormal < 0f)
//                {
//                    animNormal = 0f;
//                }
//                if (itemToggle.toggleState)
//                {
//                    itemToggle.ToggleItem(toggle: false);
//                }
//            }
//            if (animator.GetFloat("Normal") != animNormal)
//            {
//                animator.SetFloat("Normal", animNormal);
//            }
//        }
//        public void RefreshPlayerList()
//        {
//            playerNames.Clear();
//            for (int i = 0; i < GameDirector.instance.PlayerList.Count; i++)
//            {
//                playerNames.Add(GameDirector.instance.PlayerList[i].playerName);
//            }
//            playerNames.Sort();
//        }
//        public void SetPageText(int id, string text)
//        {
//            if (GameManager.Multiplayer())
//            {
//                photonView.RPC("SetTextRPC", RpcTarget.All, id, text);
//            }
//            else
//            {
//                SetPageTextRPC(id, text);
//            }
//        }
//        public IEnumerator KillCoroutine()
//        {
//            yield return new WaitForSeconds(5f);
//            if (SemiFunc.IsMultiplayer())
//            {
//                ChatManager.instance.PossessSelfDestruction();
//            }
//            else
//            {
//                PlayerAvatar.instance.playerHealth.health = 0;
//                PlayerAvatar.instance.playerHealth.Hurt(1, savingGrace: false);
//            }
//        }
//        [PunRPC]
//        public void SetPageTextRPC(int id, string text)
//        {
//            pageText[id].text = text;
//        }
//        public void KillPlayer(string player)
//        {
//            if (GameManager.Multiplayer())
//            {
//                photonView.RPC("KillPlayerRPC", RpcTarget.All, player);
//            }
//            else
//            {
//                KillPlayerRPC(player);
//            }
//        }
//        [PunRPC]
//        public void KillPlayerRPC(string player)
//        {
//            RefreshPlayerList();
//            player = playerNames.Find((x) => WildCardUtils.instance.TextIsSimilar(player, x));
//            if (player != null && playerNames.Contains(player))
//            {
//                if (ChatManager.instance.playerAvatar == SemiFunc.PlayerGetFromName(player))
//                {
//                    StartCoroutine(KillCoroutine());
//                }
//                itemBattery.SetBatteryLife(0);
//            }
//        }
//    }
//}