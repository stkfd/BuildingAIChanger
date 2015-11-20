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
using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingAIChanger
{
    public class MeowUI : UIPanel
    {
        public delegate void SelectedAIChangedHandler(MeowUI ui, string value);

        private PrefabAITypes aiTypes;
        private UITextureAtlas dropdownButtonAtlas;
        private PrefabInfo prefabInfo;
        private UIComponent propPanel;
        public UIDropDown selectAIDropDown;
        private ApplyButton applyButton;
        private bool resetting;

        public PrefabAIInfo SelectedAIInfo
        {
            get { return aiTypes.GetAIInfo(selectAIDropDown.selectedValue); }
            set { selectAIDropDown.selectedValue = value.type.FullName; }
        }

        public bool IsAIApplied
        {
            set
            {
                if (applyButton != null)
                {
                    applyButton.isEnabled = !value;
                    if (value) applyButton.tooltip = "";
                    applyButton.tooltip = "Set PrefabAI to " + selectAIDropDown.selectedValue;
                }
            }
        }

        public event MouseEventHandler eventApplyAIClick;

        public event SelectedAIChangedHandler eventSelectedAIChanged;

        public override void Start()
        {
            base.Start();
            aiTypes = PrefabAITypes.GetInstance();
            propPanel = parent.parent;

            width = 393;
            height = 26;

            //	so let's steal from one of the appropriate style. 
            dropdownButtonAtlas =
                propPanel.Find<UIPanel>("Size").Find<UIDropDown>("CellWidth").Find<UIButton>("Button").atlas;

            var label = AddUIComponent<Label>();
            label.position = new Vector3(10, 0);

            applyButton = AddUIComponent<ApplyButton>();
            applyButton.eventClick +=
                delegate(UIComponent component, UIMouseEventParameter param)
                {
                    eventApplyAIClick?.Invoke(this, param);
                };

            selectAIDropDown = AddUIComponent<DropDown>();
            selectAIDropDown.eventSelectedIndexChanged +=
                delegate(UIComponent component, int value)
                {
                    if (!resetting)
                    {
                        eventSelectedAIChanged?.Invoke(this, selectAIDropDown.selectedValue);
                    }
                };
            selectAIDropDown.listScrollbar = propPanel.Find<UIScrollbar>("Scrollbar");

            foreach (var aiInfo in aiTypes.All())
            {
                selectAIDropDown.AddItem(aiInfo.type.FullName);
            }
            selectAIDropDown.selectedValue = prefabInfo.GetAI().GetType().FullName;
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
            meowUI.UpdatePrefabInfo(info);
            meowUI.IsAIApplied = true;
            return meowUI;
        }

        public void UpdatePrefabInfo(PrefabInfo info)
        {
            var newAI = info?.gameObject.GetComponent<PrefabAI>();
            if (newAI == null) return;
            resetting = true;
            var oldAI = prefabInfo?.gameObject.GetComponent<PrefabAI>();
            
            bool changing = (oldAI == null) || oldAI.GetType().FullName == newAI.GetType().FullName;
            prefabInfo = info;

            if (changing)
            {
                IsAIApplied = false;
            }
            RefreshUI();
            resetting = false;
        }

        /// <summary>
        /// Reload the property editor panel so the fields of a new AI class appear
        /// </summary>
        /// <param name="prefabInfo">(PrefabInfo) </param>
        public void RefreshUI()
        {
            var decorationPropertiesPanel = propPanel?.GetComponent<DecorationPropertiesPanel>();
            decorationPropertiesPanel?.GetType()
                .InvokeMember("Refresh", BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
                    decorationPropertiesPanel, new object[] {prefabInfo});

            if (selectAIDropDown != null) selectAIDropDown.selectedValue = prefabInfo.GetAI().GetType().FullName;
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
                listWidth = 171;
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