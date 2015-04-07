using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace BuildingAIChanger
{
    public class EditorController : MonoBehaviour
    {
        private readonly UIView m_view;
        private SelectAIPanel m_selectAIPanel;
        private ToolController m_toolController;
        private UIPanel m_propPanel;
        private UIComponent m_uiContainer;
        
        private EditorController()
        {
            m_view = UIView.GetAView();
            InsertUI();
            m_toolController = ToolsModifierControl.toolController;
            m_toolController.eventEditPrefabChanged += OnEditPrefabChanged;
        }

        private void OnEditPrefabChanged(PrefabInfo info)
        {
            Debug.Log("prefab changed");
            m_selectAIPanel.value = info.GetAI().GetType().FullName;
            RefreshUIPosition();
        }

        private void InsertUI()
        {
            m_uiContainer = m_view.FindUIComponent("FullScreenContainer");
            m_propPanel = m_uiContainer.Find<UIPanel>("DecorationProperties");
            m_selectAIPanel = m_uiContainer.AddUIComponent<SelectAIPanel>();
            m_selectAIPanel.eventValueChanged += OnAIFieldChanged;
            RefreshUIPosition();
        }

        private void OnAIFieldChanged(UIComponent component, string value)
        {
            var prefabInfo = m_toolController.m_editPrefabInfo;
            if (m_selectAIPanel.IsValueValid())
            {
                DestroyImmediate(prefabInfo.gameObject.GetComponent<PrefabAI>());
                var type = m_selectAIPanel.TryGetAIType();
                prefabInfo.gameObject.AddComponent(type);

                m_toolController.GetType()
                    .InvokeMember("eventEditPrefabChanged", BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
                        m_toolController, new object[] {prefabInfo});
                
                RefreshUIPosition();
            }

            var components = prefabInfo.GetComponents<Component>();
            String compList = "";
            components.ForEach((c) => compList += c.GetType().FullName + " ");
            Debug.Log(compList);
        }

        private void RefreshUIPosition()
        {
            m_selectAIPanel.transformPosition = m_propPanel.transformPosition;
            m_selectAIPanel.transform.Translate(0, 0.3f, 0);
            m_selectAIPanel.PerformLayout();
        }

        public static EditorController Create()
        {
            return new EditorController();
        }
    }
}
