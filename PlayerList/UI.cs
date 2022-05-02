using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VRC.UI.Elements.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;
using VRC.DataModel;

namespace PlayerList
{
    internal class UI //this class looks like a mess lol
    {
        public static void SelectUser(APIUser usr) => SelectUserInternal(usr).Start();
        public static void TogglePlayerList(bool state) => MainObject.SetActive(state);
        internal static GameObject CreateEntry(string text, Action onClick = null)
        {
            var obj = GameObject.Instantiate(TextPrefab, TextHolder.transform);
            obj.transform.Find("text").GetComponent<TextMeshProUGUI>().text = text;
            var btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(onClick);
            return obj;
        }
        internal static void AddPlayerToPList(Player obj)
        {
            var usr = obj.field_Private_APIUser_0;
            playerList.Add(
                obj.field_Private_VRCPlayerApi_0.playerId,
                CreateEntry(
                    PlayerList.DisplayRankColor.Value ?
                        $"[{obj.GetPlatform()}] <color={usr.GetRankColorHEX()}> {usr.displayName}" 
                        :
                        $"[{obj.GetPlatform()}] {usr.displayName}",
                    new Action(() => { SelectUser(usr); })
                )
            );
        }
        internal static void ClearEntries()
        {
            playerList.Values.ToList().ForEach(x => GameObject.Destroy(x.gameObject));
            playerList.Clear();
        }
        internal static void ReloadEntries()
        {
            ClearEntries();
            var players = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;
            for (int i = 0; i < players.Count; i++) AddPlayerToPList(players[i]);
        }
        internal static void DestroyEntry(int key)
        {
            GameObject.Destroy(playerList[key]);
            playerList.Remove(key);
        }
        internal static IEnumerator Setup()
        {
            GameObject ui;
            Transform parent;
            while ((ui = GameObject.Find("UserInterface")) == null) yield return null;
            while ((parent = ui.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/QMParent")) == null) yield return null;
            while ((Wing_Left = ui.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/Wing_Left")) == null) yield return null;
            //while ((Wing_Right = ui.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/Wing_Right")) == null) yield return null;

            MainObject = GameObject.Instantiate(parent.gameObject, Wing_Left);
            var rectTransform = MainObject.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(-875, -1024, 0);
            rectTransform.sizeDelta = new Vector2(675, 1024);

            //destroy unnecessary objects
            var childCount = MainObject.transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = MainObject.transform.GetChild(i);
                if (child.name == "BackgroundLayer01" || child.name == "BackgroundLayer02") continue;
                if (child.name == "Menu_Dashboard")
                {
                    MenuDashboard = child.gameObject;
                    continue;
                }
                GameObject.Destroy(child.gameObject);
            }
            UnityEngine.Object.DestroyImmediate(MenuDashboard.GetComponent<LaunchPadQMMenu>());
            MenuDashboard.SetActive(true);

            MenuDashboard.transform
                .Find("Header_H1/LeftItemContainer/Text_Title")
                .GetComponent<TextMeshProUGUI>()
                .text = "Player List";

            GameObject.Destroy(MenuDashboard.transform.Find("Header_H1/RightItemContainer").gameObject);
            TextHolder = MenuDashboard.transform.Find("ScrollRect/Viewport/VerticalLayoutGroup");
            TextHolder.transform.localPosition += new Vector3(25, 0f, 0f);

            childCount = TextHolder.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = TextHolder.GetChild(i);
                if (child.name == "Header_QuickActions")
                {
                    child.gameObject.SetActive(false);
                    continue;
                }
                GameObject.Destroy(child.gameObject);
            }

            //get text prefab and make it clickable
            TextPrefab = TextHolder.Find("Header_QuickActions/LeftItemContainer/Text_Title").gameObject;
            var childObj = GameObject.Instantiate(TextPrefab, TextPrefab.transform);
            childObj.name = "text";
            childObj.transform.localPosition = new Vector3(0f, 0f, 0f);

            childObj.GetComponent<RectTransform>()
                .sizeDelta = new Vector2(1000f, 50f);

            TextPrefab.transform
                .Find("text")
                .GetComponent<TextMeshProUGUI>()
                .fontSize = 36;

            GameObject.DestroyImmediate(TextPrefab.GetComponent<TextMeshProUGUI>());
            TextPrefab.AddComponent<UIInvisibleGraphic>();

            //setup scroll
            var scrollrect = MenuDashboard.transform.Find("ScrollRect");
            scrollrect.GetComponent<ScrollRect>().enabled = true;
            //scrollrect.Find("Scrollbar").gameObject.SetActive(true);

            //setup collider
            BoxCollider boxcol;
            while ((boxcol = Wing_Left.parent.GetComponent<BoxCollider>()) == null) yield return null;
            boxcol.size = new Vector3(4096f, boxcol.size.y, boxcol.size.z);

            _selectUserButton = ui.transform
                .Find("Canvas_QuickMenu(Clone)/Container/Window/QMParent/Modal_HoveredUser/QMUserProfile_Compact/PanelBG/Cell_QM_User")
                .GetComponent<Button>();

            PlayerList.Logger.Msg("Finished Player List.");
            yield break;
        }
        internal static MethodInfo SelectUserMethod;
        private static IEnumerator SelectUserInternal(APIUser usr)
        {
            var inst = UserSelectionManager.prop_UserSelectionManager_0;
            SelectUserMethod?.Invoke(inst, new object[] { usr });
            yield return new WaitForSecondsRealtime(0.08f);
            _selectUserButton.Press();
            yield break;
        }
        private static Dictionary<int, GameObject> playerList = new Dictionary<int, GameObject>();
        private static Button _selectUserButton;
        private static GameObject MainObject;
        private static GameObject MenuDashboard;
        private static GameObject TextPrefab;
        private static Transform TextHolder;

        private static Transform Wing_Left;
        //private static Transform Wing_Right;
    }
}
