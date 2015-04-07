using System;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingAIChanger
{
    class SelectAIPanel : UIPanel
    {
        public event PropertyChangedEventHandler<string> eventValueChanged;

        private const String ColossalAssemblyInfo = ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
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

        public String value
        {
            get { return m_input.text; }
            set { m_input.text = value; }
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

            m_input.eventTextSubmitted += OnTextChanged;
        }

        private void OnTextChanged(UIComponent component, string value)
        {
            Verify();

            if (eventValueChanged != null)
                eventValueChanged(component, value);
        }

        public override void PerformLayout()
        {
            if (m_label != null && m_input != null)
            {
                m_label.position = new Vector3(0, 0);

                m_input.position = new Vector3(0, 0);
                m_input.transform.Translate(.2f, -.01f, 0);
                m_input.width = width * 0.67f;
            }
            base.PerformLayout();
        }

        public void Verify()
        {
            if (IsValueValid())
            {
                m_input.textColor = new Color32(0, 255, 0, 255);
            }
            else
            {
                m_input.textColor = new Color32(255, 0, 0, 255);
            }
        }

        /**
         * Check if the current Value is the Name of an existing AI class
         */
        public bool IsValueValid()
        {
            var type = TryGetAIType();
            try
            {
                type.Equals(null);
            }
            catch (NullReferenceException)
            {
                return false;
            }

            return true;
        }

        public Type TryGetAIType()
        {
            Type type;
            try
            {
                // Search in colossal assembly first
                try
                {
                    type = Type.GetType(value + ColossalAssemblyInfo, true);

                    return (type.IsSubclassOf(typeof(PrefabAI))) ? type : null;
                }
                catch (TypeLoadException)
                {
                    // If not found, look in all loaded Assemblies
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            type = a.GetType(value, true);

                            // on success
                            return (type.IsSubclassOf(typeof(PrefabAI))) ? type : null;
                        }
                        catch (TypeLoadException) { }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
