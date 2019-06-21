/** PrefabAIChanger
 * @author Stefan Kaufhold
 * @author D Lue Choy
 * 
 * Copyright (c) 2015, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License
 * as published by the Free Software Foundation; either 
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 * See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser 
 * General Public License along with this library.
 **/

using System;
using System.Collections.Generic;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace PrefabAIChanger
{
    public class MeowUI : UIPanel
    {
        public delegate void SelectedAIChangedHandler(MeowUI ui, string value);

        public delegate void PrefabInfoChanged(MeowUI ui, PrefabInfo oldInfo, PrefabInfo newInfo);

        private UITextureAtlas dropdownButtonAtlas;

        private PrefabInfo prefabInfo;

        public PrefabInfo PrefabInfo
        {
            get { return prefabInfo; }
            set
            {
                var oldInfo = prefabInfo;
                var newAI = value?.GetAI();
                prefabChanging = true;
                if (newAI == null)
                {
                    bool changing = oldInfo != null;
                    prefabInfo = null;
                    if (changing) eventPrefabInfoUpdated?.Invoke(this, oldInfo, prefabInfo);
                }
                else
                {
                    var oldAI = oldInfo?.GetAI();

                    bool changing = oldAI?.GetType().FullName == newAI.GetType().FullName;
                    prefabInfo = value;

                    if (changing) eventPrefabInfoUpdated?.Invoke(this, oldInfo, prefabInfo);
                }

                prefabChanging = false;
            }
        }

        public PrefabAIInfo SelectedAIInfo
        {
            get => Singleton<PrefabAITypes>.instance.GetAIInfo(selectAIDropDown.selectedValue);
            set { selectAIDropDown.selectedValue = value.type.FullName; }
        }

        private UIComponent propPanel;

        public UIDropDown selectAIDropDown;

        private ApplyButton applyButton;

        private bool prefabChanging;

        public event MouseEventHandler eventApplyAIClick;

        public event SelectedAIChangedHandler eventSelectedAIChanged;

        public event PrefabInfoChanged eventPrefabInfoUpdated;

        public override void Start()
        {
            base.Start();
            propPanel = parent.parent.parent;

            width = 393;
            height = 26;

            //	so let's steal from one of the appropriate style.
            dropdownButtonAtlas =
                propPanel.Find<UIDropDown>("Value").Find<UIButton>("Button").atlas;

            var label = AddUIComponent<Label>();
            label.position = new Vector3(10, 0);

            applyButton = AddUIComponent<ApplyButton>();

            selectAIDropDown = AddUIComponent<DropDown>();
            selectAIDropDown.listScrollbar = propPanel.Find<UIScrollbar>("Scrollbar");

            List<PrefabAIInfo> loadedTypes;
            try
            {
                loadedTypes = Singleton<PrefabAITypes>.instance.All();
            }
            catch (TypeLoadException e)
            {
                Debug.LogException(e);
                Debug.Log(e.Message);
                Debug.Log(e.TypeName);
                Debug.Log(e.Source);
                Debug.LogException(e.InnerException);
                return;
            }

            Debug.Log("add AI options");
            // Add AI options to dropdown
            foreach (var aiInfo in loadedTypes)
            {
                Debug.Log(aiInfo);
                selectAIDropDown.AddItem(aiInfo.type.FullName);
            }

            Debug.Log(prefabInfo.GetAI());
            selectAIDropDown.selectedValue = prefabInfo.GetAI().GetType().FullName;

            // Events
            applyButton.eventClick += delegate(UIComponent component, UIMouseEventParameter param)
            {
                eventApplyAIClick?.Invoke(this, param);
            };

            selectAIDropDown.eventSelectedIndexChanged += delegate
            {
                if (!prefabChanging)
                {
                    eventSelectedAIChanged?.Invoke(this, selectAIDropDown.selectedValue);
                    UpdateApplyButton();
                }
            };

            eventPrefabInfoUpdated += delegate
            {
                RefreshDecorationProperties();
                UpdateApplyButton();
            };

            RefreshDecorationProperties();
            UpdateApplyButton();
        }

        public static MeowUI InsertInstance(PrefabInfo info)
        {
            var view = UIView.GetAView();
            if (view == null) return null;

            var uiContainer = view.FindUIComponent("FullScreenContainer");
            var meowUI =
                uiContainer.Find<UIPanel>("DecorationProperties")
                    .Find<UIScrollablePanel>("Container")
                    .AddUIComponent<MeowUI>();
            meowUI.PrefabInfo = info;
            return meowUI;
        }

        private void UpdateApplyButton()
        {
            var ai = prefabInfo?.GetAI();
            bool canBeApplied = ai != null && ai.GetType().FullName != selectAIDropDown.selectedValue;
            if (applyButton != null)
            {
                applyButton.tooltipBox.Hide();
                applyButton.isEnabled = canBeApplied;
                applyButton.tooltip = canBeApplied ? "Set PrefabAI to " + selectAIDropDown.selectedValue : "";
            }
        }

        public void ResetDropDown()
        {
            selectAIDropDown.selectedValue = prefabInfo.GetAI().GetType().FullName;
        }

        /// <summary>
        /// Reload the property editor panel so the fields of a new AI class appear
        /// </summary>
        /// <param name="prefabInfo">(PrefabInfo) </param>
        private void RefreshDecorationProperties()
        {
            var decorationPropertiesPanel = propPanel?.GetComponent<DecorationPropertiesPanel>();
            decorationPropertiesPanel?.GetType()
                .InvokeMember("Refresh", BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
                    decorationPropertiesPanel, new object[] {prefabInfo});
        }

        private class Label : UILabel
        {
            public override void OnEnable()
            {
                base.OnEnable();
                text = "PrefabAI";
                tooltip = "Determines the behaviour and availble properties of the prefab.";
                width = 181;
                height = 18;
                autoSize = false;
                textColor = new Color32(125, 185, 255, 255);
                disabledTextColor = new Color32(255, 255, 255, 255);
            }
        }

        private class ApplyButton : UIButton
        {
            public override void Start()
            {
                base.Start();

                name = "ApplyButton";
                text = "Apply";
                size = new Vector3(50, 20);
                position = new Vector3(145, 0);

                normalBgSprite = "ButtonMenu";
                isEnabled = false;
                pressedTextColor = new Color32(30, 30, 44, 255);
                hoveredTextColor = new Color32(7, 132, 255, 255);
                disabledTextColor = new Color32(150, 150, 150, 255);
            }
        }

        private class DropDown : UIDropDown
        {
            public override void Start()
            {
                base.Start();
                var parent = (MeowUI) this.parent;

                name = "SelectAIDropDown";
                position = new Vector3(200, 0);
                size = new Vector2(190, 20);
                pivot = UIPivotPoint.TopRight;

                normalBgSprite = "TextFieldPanel";
                itemHeight = 27;
                itemHover = "ListItemHover";
                itemHighlight = "ListItemHighlight";
                itemPadding = new RectOffset(5, 0, 5, 0);
                m_TextFieldPadding = new RectOffset(7, 0, 1, 0);
                disabledBgSprite = "SubBarButtonBaseDisabled";
                listBackground = "InfoDisplay";
                listWidth = (int) width;
                listHeight = 400;
                listPosition = PopupListPosition.Above;
                popupColor = new Color32(255, 255, 255, 255);
                popupTextColor = new Color32(12, 21, 22, 255);
                color = new Color32(255, 255, 255, 255);
                textColor = new Color32(12, 21, 22, 255);
                useOutline = false;
                bottomColor = new Color32(255, 255, 255, 255);
                verticalAlignment = UIVerticalAlignment.Middle;
                horizontalAlignment = UIHorizontalAlignment.Center;

                var dropDownButton = AddUIComponent<UIButton>();
                triggerButton = dropDownButton;
                dropDownButton.size = size;
                dropDownButton.relativePosition = new Vector3(0, -1);
                dropDownButton.textVerticalAlignment = UIVerticalAlignment.Middle;
                dropDownButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
                dropDownButton.horizontalAlignment = UIHorizontalAlignment.Right;
                dropDownButton.verticalAlignment = UIVerticalAlignment.Middle;
                dropDownButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;

                dropDownButton.normalFgSprite = "IconUpArrow";
                dropDownButton.hoveredFgSprite = "IconUpArrowHovered";
                dropDownButton.pressedFgSprite = "IconUpArrowPressed";
                dropDownButton.focusedFgSprite = "IconUpArrowFocused";
                dropDownButton.disabledFgSprite = "IconUpArrowDisabled";
                dropDownButton.atlas = parent.dropdownButtonAtlas;
                //selectAIDropDownButton.normalFgSprite = widthButton.normalFgSprite; //"IconUpArrow";
            }
        }
    }
}