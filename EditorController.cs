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

        private EditorController()
        {
            m_view = UIView.GetAView();
            InsertUI();
        }

        private void InsertUI()
        {
            var container = m_view.FindUIComponent("FullScreenContainer");
            var propPanel = container.Find<UIPanel>("DecorationProperties");
            m_selectAIPanel = container.AddUIComponent<SelectAIPanel>();

            m_selectAIPanel.transformPosition = propPanel.transformPosition;
            m_selectAIPanel.transform.Translate(0, 0.1f, 0);
            m_selectAIPanel.PerformLayout();
        }

        public static EditorController Create()
        {
            return new EditorController();
        }

        public void OnEditPrefabChanged()
        {
            
        }
    }
}
