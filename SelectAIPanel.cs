using ColossalFramework.UI;
using UnityEngine;

namespace BuildingAIChanger
{
    class SelectAIPanel : UIPanel
    {
        private UILabel m_label;
        private UITextField m_input;

        public override void Start()
        {
            base.Start();
            backgroundSprite = "GenericPanel";
            color = new Color32(150, 150, 150, 255);

            width = 383;
            height = 38;

            AddLabel();
            AddInput();

            PerformLayout();
        }

        private void AddLabel()
        {
            m_label = AddUIComponent<UILabel>();
            m_label.text = "Building AI";
            m_label.padding = new RectOffset(10, 0, 12, 0);
        }

        private void AddInput()
        {
            m_input = AddUIComponent<UITextField>();

            m_input.padding = new RectOffset(10, 10, 6, 6);
            m_input.height = 26;

            m_input.builtinKeyNavigation = true;
            m_input.isInteractive = true;
            m_input.readOnly = false;

            m_input.horizontalAlignment = UIHorizontalAlignment.Left;
            m_input.selectionSprite = "EmptySprite";
            m_input.selectionBackgroundColor = new Color32(0, 171, 234, 255);
            m_input.normalBgSprite = "TextFieldPanel";
            m_input.textColor = new Color32(174, 197, 211, 255);
            m_input.disabledTextColor = new Color32(254, 254, 254, 255);
            m_input.color = new Color32(58, 88, 104, 255);
            m_input.disabledColor = new Color32(254, 254, 254, 255);
        }

        public override void PerformLayout()
        {
            m_label.position = new Vector3(0, 0);

            m_input.position = new Vector3(0, -.01f);
            m_input.transform.Translate(.18f, 0, 0);
            m_input.width = width * 0.67f;
            base.PerformLayout();
        }
    }
}
