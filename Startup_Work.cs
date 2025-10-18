//=============================================================================
// <summary>
// ビルド時のセットアップに関するクラス 
// </summary>
// <author>菊池雅道</author>
//=============================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.dev;

namespace app
{
    public class Startup_Work
    {
        public static void Main(string[] argv)
        {
            via.debug.infoLine("Startup.Main()");

#if VIA_DEVELOP && EXPORT_SETTINGS
            foreach (var widget in WidgetManager.Widgets)
            {
                widget.Enable = false;
            }
#endif
        }
    }
}