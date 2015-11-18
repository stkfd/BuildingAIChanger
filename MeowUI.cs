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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingAIChanger
{
    class MeowUI
    {
        private UIPanel m_propPanel;
        private UIPanel m_prefabAIPanel;
        private PrefabInfo m_prefabInfo;
        public UICheckBox m_check;
        public UIDropDown m_selectAIDropDown;
        public IOrderedEnumerable<MuAIInfo> m_aiInfo;
        private UITextureAtlas m_checkboxAtlas;
        private UITextureAtlas m_dropdownButtonAtlas;
        private bool m_resetting;

        private MeowUI(PrefabInfo info, UIPanel decorationPropertiesPanel)
        {
            m_propPanel = decorationPropertiesPanel;
            var propPanelContainer = m_propPanel.Find<UIScrollablePanel>("Container");
            m_checkboxAtlas = m_propPanel.Find<UISlicedSprite>("Caption").atlas;

            var label = BuildLabel();
            m_check = BuildCheckBox();
            m_selectAIDropDown = BuildDropDown();
            m_prefabAIPanel = BuildPrefabAIPanel(label, m_check, m_selectAIDropDown);
            propPanelContainer.AttachUIComponent(m_prefabAIPanel.gameObject);

            //	so let's steal from one of the appropriate style. 
            m_dropdownButtonAtlas =
                m_propPanel.Find<UIPanel>("Size").Find<UIDropDown>("CellWidth").Find<UIButton>("Button").atlas;
            

            m_aiInfo = CollectAIInfo();
            foreach (var mew in m_aiInfo)
            {
                /*
                    string nextcat = string.Format("{0}{1}{2}{3}{4}",mew.fullname.Substring(trindex),mew.buildingAI?",b":"",mew.citizenAI?",c":"",mew.netAI?",n":"",mew.vehicleAI?",v":"");
                    m_selectAIDropDown.AddItem(nextcat);
                */
                m_selectAIDropDown.AddItem(mew.type.FullName);
            }
            m_selectAIDropDown.selectedIndex = 0;
            // Some how I can't help but feel that this is an invitation to disaster.
            m_selectAIDropDown.position = new Vector3(210.0f, 0.0f);
            Reset(info);
        }

        public delegate void CheckChanged(MeowUI ui, bool value);

        public delegate void SelectedAIChanged(MeowUI ui, string value);

        public event CheckChanged eventCheckChanged;

        public event SelectedAIChanged eventSelectedAIChanged;

        public void Reset(PrefabInfo info)
        {
            m_resetting = true;
            m_prefabInfo = info;
            m_check.isChecked = false;
            if (m_prefabInfo != null)
            {
                var assetAI = info.GetAI();
                if (assetAI != null)
                {
                    m_selectAIDropDown.selectedValue = assetAI.GetType().FullName;
                    m_check.isChecked = true;
                }
                else
                {
                    m_selectAIDropDown.selectedValue = "PrefabAI";
                    m_prefabAIPanel.Hide();
                }
            }
            else
            {
                m_selectAIDropDown.selectedValue = "PrefabAI";
                m_prefabAIPanel.Hide();
            }
            RefreshPropertiesPanel(info);
            m_resetting = false;
        }

        /// <summary>
        /// Reload the property editor panel so the fields of a new AI class appear
        /// </summary>
        /// <param name="prefabInfo">(PrefabInfo) </param>
        public void RefreshPropertiesPanel(PrefabInfo prefabInfo)
        {
            var decorationPropertiesPanel = m_propPanel.GetComponent<DecorationPropertiesPanel>();
            decorationPropertiesPanel.GetType()
                .InvokeMember("Refresh", BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
                    decorationPropertiesPanel, new object[] { prefabInfo });
        }

        private UIPanel BuildPrefabAIPanel(UILabel label, UICheckBox checkBox, UIDropDown selectAIDropDown)
        {
            var prefabAIPanel = new UIPanel
            {
                width = 393,
                height = 25
            };
            prefabAIPanel.AttachUIComponent(label.gameObject);
            prefabAIPanel.AttachUIComponent(checkBox.gameObject);
            return prefabAIPanel;
        }

        private UILabel BuildLabel()
        {
            return new UILabel
            {
                text = "PrefabAI",
                tooltip = "Determines the behaviour and availble properties of the prefab.",
                width = 181,
                height = 18,
                position = new Vector3(0.0f, -4.0f), // buggy invert from unknown source.
                autoSize = false,
                textColor = new Color32(125, 185, 255, 255),
                disabledTextColor = new Color32(255, 255, 255, 255)
            };
        }

        private UICheckBox BuildCheckBox()
        {
            var check = new UICheckBox
            {
                tooltip = "",
                width = 18,
                height = 18,
                position = new Vector3(190.0f, 0.0f)
            };

            var checkUncheck = check.AddUIComponent<UISprite>();
            //UITextureAtlas "Ingame"
            checkUncheck.atlas = m_checkboxAtlas;
            checkUncheck.spriteName = "check-unchecked";
            checkUncheck.width = 16;
            checkUncheck.height = 16;
            checkUncheck.position = new Vector3(0.0f, 0.0f);

            var checkCheck = (UISprite)checkUncheck.AddUIComponent<UISprite>();
            checkCheck.atlas = m_checkboxAtlas;
            checkCheck.spriteName = "check-checked";
            checkCheck.width = 16;
            checkCheck.height = 16;
            checkCheck.position = new Vector3(0.0f, 0.0f);

            check.checkedBoxObject = checkCheck;
            check.eventCheckChanged +=
                delegate(UIComponent component, bool value) { if (!m_resetting) eventCheckChanged?.Invoke(this, value); };
            return check;
        }

        private IOrderedEnumerable<MuAIInfo> CollectAIInfo()
        {
            //get all subclasses of PrefabAI 
            var aiInfo = new List<MuAIInfo>();

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = a.GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].IsSubclassOf(typeof(PrefabAI)))
                    {
                        MuAIInfo temp = new MuAIInfo();
                        temp.type = assemblyTypes[j];
                        temp.fullname = assemblyTypes[j].FullName;
                        var derived = assemblyTypes[j];
                        do
                        {
                            derived = derived.BaseType;
                            if (derived != null)
                            {
                                // temp.fullname.Insert(0,derived.FullName);
                                temp.fullname = String.Concat(derived.FullName, ".", temp.fullname);
                            }
                        } while (derived != null);
                        if (assemblyTypes[j].IsSubclassOf(typeof(BuildingAI)))
                        {
                            temp.buildingAI = true;
                        }
                        if (assemblyTypes[j].IsSubclassOf(typeof(CitizenAI)))
                        {
                            temp.citizenAI = true;
                        }
                        if (assemblyTypes[j].IsSubclassOf(typeof(NetAI)))
                        {
                            temp.netAI = true;
                        }
                        if (assemblyTypes[j].IsSubclassOf(typeof(VehicleAI)))
                        {
                            temp.vehicleAI = true;
                        }
                        aiInfo.Add(temp);
                    }
                }
            }

            var tempo = new MuAIInfo();
            tempo.type = typeof(PrefabAI);
            tempo.fullname = tempo.type.FullName;
            var derivedo = tempo.type;
            do
            {
                derivedo = derivedo.BaseType;
                if (derivedo != null)
                {
                    tempo.fullname = String.Concat(derivedo.FullName, ".", tempo.fullname);
                }
            } while (derivedo != null);
            aiInfo.Add(tempo);

            /*if(null != Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo as BuildingInfo) {
                AIInfosort = aiInfo.OrderBy(s => s.buildingAI).ThenBy(s => s.citizenAI).ThenBy(s => s.netAI).ThenBy(s => s.vehicleAI).ThenBy(s => s.type.FullName);
            } else if(null != Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo as NetInfo) {
                AIInfosort = aiInfo.OrderBy(s => s.netAI).ThenBy(s => s.buildingAI).ThenBy(s => s.citizenAI).ThenBy(s => s.vehicleAI).ThenBy(s => s.type.FullName);
            } else if(null != Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo as VehicleInfo) {
                AIInfosort = aiInfo.OrderBy(s => s.vehicleAI).ThenBy(s => s.netAI).ThenBy(s => s.buildingAI).ThenBy(s => s.citizenAI).ThenBy(s => s.type.FullName);
            } else if(null != Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo as CitizenInfo) {
                AIInfosort = aiInfo.OrderBy(s => s.citizenAI).ThenBy(s => s.vehicleAI).ThenBy(s => s.netAI).ThenBy(s => s.buildingAI).ThenBy(s => s.type.FullName);
            }*/
            var aiInfosort = aiInfo.OrderBy(s => s.fullname);

            /*	//	might be useful for display purposes.
            int trimcomp = Math.Min(AIInfosort.First().fullname.Length,AIInfosort.Last().fullname.Length);
            int trindex = 0;
            for (int i = 0; i < trimcomp; i ++) {
                if (AIInfosort.First().fullname[i] != AIInfosort.Last().fullname[i]){
                    trindex = i;
                    i = trimcomp;
                }
            }
            */

            return aiInfosort;
        }

        private UIDropDown BuildDropDown()
        {
           var dropDown = new UIDropDown
            {
                name = "SelectAIDropDown",
                size = new Vector2(171.0f, 20.0f),
                itemHeight = 25,
                itemHover = "ListItemHover",
                itemHighlight = "ListItemHighlight",
                normalBgSprite = "InfoDisplay",
                disabledBgSprite = "SubBarButtonBaseDisabled",
                listBackground = "InfoDisplay",
                listWidth = 171,
                listHeight = 400,
                listPosition = ColossalFramework.UI.UIDropDown.PopupListPosition.Above,
                foregroundSpriteMode = UIForegroundSpriteMode.Stretch,
                popupColor = new Color32(255, 255, 255, 255),
                popupTextColor = new Color32(12, 21, 22, 255),
                color = new Color32(255, 255, 255, 255),
                textColor = new Color32(12, 21, 22, 255),
                useOutline = true,
                outlineColor = new Color32(255, 255, 255, 64),
                bottomColor = new Color32(255, 255, 255, 255),
                verticalAlignment = UIVerticalAlignment.Middle,
                horizontalAlignment = UIHorizontalAlignment.Center,
                listScrollbar = new UIScrollbar()
            };
            //m_selectAIDropDown.listScrollbar = m_propPanel.Find<UIScrollbar>("Scrollbar");
            dropDown.eventSelectedIndexChanged +=
                delegate { if (!m_resetting) eventSelectedAIChanged?.Invoke(this, m_selectAIDropDown.selectedValue); };

            var dropDownButton = dropDown.AddUIComponent<UIButton>();
            dropDown.triggerButton = dropDownButton;
            dropDownButton.size = dropDown.size;
            dropDownButton.relativePosition = new Vector3(0.0f, 0.0f);
            dropDownButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            dropDownButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            dropDownButton.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            dropDownButton.horizontalAlignment = UIHorizontalAlignment.Right;
            dropDownButton.verticalAlignment = UIVerticalAlignment.Middle;
            
            dropDownButton.normalFgSprite = "IconUpArrow";
            dropDownButton.hoveredFgSprite = "IconUpArrowHovered";
            dropDownButton.pressedFgSprite = "IconUpArrowPressed";
            dropDownButton.focusedFgSprite = "IconUpArrowFocused";
            dropDownButton.disabledFgSprite = "IconUpArrowDisabled";
            dropDownButton.atlas = m_dropdownButtonAtlas;
            //selectAIDropDownButton.normalFgSprite = widthButton.normalFgSprite; //"IconUpArrow";
            return dropDown;
        }

        public static MeowUI BuildInstance(PrefabInfo info)
        {
            var view = UIView.GetAView();
            if (view == null) return null;

            var uiContainer = view.FindUIComponent("FullScreenContainer");
            var decorationPropertiesPanel = uiContainer.Find<UIPanel>("DecorationProperties");
            var ui = new MeowUI(info, decorationPropertiesPanel);
            return ui;
        }
    }
}
