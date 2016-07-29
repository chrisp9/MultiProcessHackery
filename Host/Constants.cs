using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Host
{
    public static class Constants
    {
        internal const int
          WS_CHILD = 0x40000000,
          WS_VISIBLE = 0x10000000,
          LBS_NOTIFY = 0x00000001,
          HOST_ID = 0x00000002,
          LISTBOX_ID = 0x00000001,
          WS_VSCROLL = 0x00200000,
          WS_BORDER = 0x00800000,
          GWL_STYLE = -16,
         GWL_HWNDPARENT = -8;
            
        internal const long WS_CAPTION = 0x00C00000L, WS_THICKFRAME = 0x00040000L, WS_CLIPCHILDREN = 0x02000000L, GWLP_WNDPROC = -4;

    }
}
