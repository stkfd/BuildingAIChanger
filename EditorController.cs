/* BuildingAIChanger
 * Copyright (c) 2015 Stefan Kaufhold, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingAIChanger
{
    public class EditorController : MonoBehaviour
    {
        private UIPanel m_propPanel;
        private SelectAIPanel m_selectAIPanel;
        private ToolController m_toolController;
        private UIComponent m_uiContainer;
        private UIView m_view;

        public void Start()
        {
            m_view = UIView.GetAView();

            m_selectAIPanel = m_view.FindUIComponent<SelectAIPanel>("SelectAIPanel");
            m_uiContainer = m_view.FindUIComponent("FullScreenContainer");
            m_propPanel = m_uiContainer.Find<UIPanel>("DecorationProperties");

            m_toolController = ToolsModifierControl.toolController;
            m_selectAIPanel.eventValueChanged += OnAIFieldChanged;
            m_toolController.eventEditPrefabChanged += OnEditPrefabChanged;
        }

        private void OnEditPrefabChanged(PrefabInfo info)
        {
            var ai = info.GetAI();
            if (ai != null)
                m_selectAIPanel.value = ai.GetType().FullName;
        }

        private void OnAIFieldChanged(UIComponent component, string value)
        {
            var buildingInfo = (BuildingInfo) m_toolController.m_editPrefabInfo;
            if (m_selectAIPanel.IsValueValid())
            {
                // remove old ai
                var oldAI = buildingInfo.gameObject.GetComponent<PrefabAI>();
                DestroyImmediate(oldAI);

                // add new ai
                var type = m_selectAIPanel.TryGetAIType();
                var newAI = (PrefabAI) buildingInfo.gameObject.AddComponent(type);

                TryCopyAttributes(oldAI, newAI);

                buildingInfo.DestroyPrefabInstance();
                buildingInfo.InitializePrefabInstance();
                RefreshPropertiesPanel(buildingInfo);
            }
        }

        /**
         * Copies all attributes that share the same name and type from one AI object to another
         */

        private void TryCopyAttributes(PrefabAI oldAI, PrefabAI newAI)
        {
            var oldAIFields =
                oldAI.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                               BindingFlags.FlattenHierarchy);
            var newAIFields = newAI.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                           BindingFlags.FlattenHierarchy);

            var newAIFieldDic = new Dictionary<String, FieldInfo>(newAIFields.Length);
            foreach (var field in newAIFields)
            {
                newAIFieldDic.Add(field.Name, field);
            }

            foreach (var fieldInfo in oldAIFields)
            {
                if (fieldInfo.IsDefined(typeof (CustomizablePropertyAttribute), true))
                {
                    FieldInfo newAIField;
                    newAIFieldDic.TryGetValue(fieldInfo.Name, out newAIField);

                    try
                    {
                        if (newAIField.GetType().Equals(fieldInfo.GetType()))
                        {
                            newAIField.SetValue(newAI, fieldInfo.GetValue(oldAI));
                        }
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
            }
        }

        private void RefreshPropertiesPanel(BuildingInfo prefabInfo)
        {
            var decorationPropertiesPanel = m_propPanel.GetComponent<DecorationPropertiesPanel>();
            decorationPropertiesPanel.GetType()
                .InvokeMember("Refresh", BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
                    decorationPropertiesPanel, new object[] {prefabInfo});
        }
    }
}