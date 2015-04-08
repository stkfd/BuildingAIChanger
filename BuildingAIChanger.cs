using ColossalFramework.UI;
using ICities;

namespace BuildingAIChanger
{
    public class BuildingAIChanger : LoadingExtensionBase, IUserMod
    {
        private SelectAIPanel m_selectAI;

        public string Name
        {
            get { return "Building AI Changer"; }
        }

        public string Description
        {
            get { return "Allows you to change the Building AI in the Asset Editor"; }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset)
            {
                SelectAIPanel.Insert();
            }
        }
    }
}
