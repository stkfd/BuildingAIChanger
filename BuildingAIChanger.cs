using ColossalFramework.UI;
using ICities;

namespace BuildingAIChanger
{
    public class BuildingAIChanger : LoadingExtensionBase, IUserMod
    {
        public EditorController controller;

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
                controller = EditorController.Create();
            }
        }
    }
}
