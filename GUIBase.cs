//=============================================================================
// <summary>
// TutorialGUIBase_ JIn_test_TutorialGUIBase
// </summary>
// <author>（情報を取得できませんでした）</author>
//=============================================================================
using via.gui;

namespace app
{
    public class GUIBase : via.Behavior
    {
        protected GUIController _controller;
        protected View _root;
        protected bool IsReady => _controller.Component.Ready;


        public override void awake()
        {
            _controller = new GUIController(GameObject);
        }

        public T getObjectByObjectPath<T>(GUIParamVarObjectPath<T> paramVarDefine) where T : PlayObject
        {
            var objectPath = _root.getParameter(paramVarDefine);
            return _controller.getObject<T>(objectPath.Value);
        }

        public void playAnimation(GUIParamVarDefine<string> paramVarDefine, string stateName)
        {
            GameObject.DrawSelf = true;
            var anim = _root.getParameter(paramVarDefine);
            anim.Value = stateName;
        }

        protected T getSelectedItemChild<T>(SelectItem selectItem, ref uint[] hashPath, GUIParamVarObjectPath<T> paramVarDefine) where T : PlayObject
        {
            if (hashPath == null)
            {
                var objectPath = selectItem.getParameter(paramVarDefine);
                hashPath = GUIHashPath<Panel>.createFast(objectPath.Value);
            }
            return selectItem.getObject<T>(hashPath);
        }

        protected bool tryGetParameter<T>(Control control, GUIParamVarDefine<T> paramVarDefine, out GUIParamVar<T> parameter)
        {
            parameter = control.getParameter(paramVarDefine);
            return parameter != null;
        }
    }
}

