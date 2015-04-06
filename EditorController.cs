using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;

namespace BuildingAIChanger
{
    public class EditorController
    {
        private readonly UIView m_view;
        private SelectAIPanel m_selectAIPanel;
        private ToolController toolController;
        private UIPanel m_propPanel;

        private EditorController()
        {
            m_view = UIView.GetAView();
            InsertUI();
            toolController = ToolsModifierControl.toolController;
            toolController.eventEditPrefabChanged += onEditPrefabChanged;
        }

        private void onEditPrefabChanged(PrefabInfo info)
        {
            m_selectAIPanel.value = info.GetAI().GetType().FullName;
            RefreshUIPosition();
        }

        private void InsertUI()
        {
            var container = m_view.FindUIComponent("FullScreenContainer");
            m_propPanel = container.Find<UIPanel>("DecorationProperties");
            m_selectAIPanel = container.AddUIComponent<SelectAIPanel>();
            RefreshUIPosition();
        }

        private void RefreshUIPosition()
        {
            m_selectAIPanel.transformPosition = m_propPanel.transformPosition;
            m_selectAIPanel.transform.Translate(0, 0.2f, 0);
            m_selectAIPanel.PerformLayout();
        }

        public static EditorController Create()
        {
            return new EditorController();
        }
    }
}
