using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TerrainAnalysis.ThreadSafeControls
{
    public static class ThreadSafe
    {
        private delegate void AddContainerItemDelegate(ListBox container, object item);
        public static void AddContainerItem(ListBox container, object item)
        {
            if (container != null && item != null)
            {
                if (container.InvokeRequired)
                {
                    AddContainerItemDelegate d = new AddContainerItemDelegate(AddContainerItem);
                    container.Invoke(d, new object[] { container, item });
                }
                else
                {
                    container.Items.Add(item);
                }
            }
        }

        private delegate int SliderValue(TrackBar tb);
        public static int GetSliderValue(TrackBar tb)
        {
            if (tb != null)
            {
                if (tb.InvokeRequired)
                {
                    SliderValue d = new SliderValue(GetSliderValue);
                    return (int)tb.Invoke(d, new object[] { tb });
                }
                else
                {
                    return tb.Value;
                }
            }
            else
                return 1;
        }

        private delegate void ItemText(Control control, string text);
        public static void UpdateItemText(Control control, string text)
        {
            if (control != null && text != null)
            {
                if (control.InvokeRequired)
                {
                    ItemText d = new ItemText(UpdateItemText);
                    control.Invoke(d, new object[] { control, text });
                }
                else
                {
                    control.Text = text;
                }
            }
        }

        private delegate void ControlVisibility(Control control, bool visible);
        public static void UpdateControlVisibility(Control control, bool visible)
        {
            if (control != null)
            {
                if (control.InvokeRequired && !control.Disposing)
                {
                    ControlVisibility d = new ControlVisibility(UpdateControlVisibility);
                    control.Invoke(d, new object[] { control, visible });
                }
                else
                {
                    control.Visible = visible;
                }
            }
        }

        private delegate void ControlEnabled(Control control, bool enabled);
        public static void UpdateControlEnabled(Control control, bool enabled)
        {
            if (control != null)
            {
                if (control.InvokeRequired)
                {
                    ControlVisibility d = new ControlVisibility(UpdateControlEnabled);
                    control.Invoke(d, new object[] { control, enabled });
                }
                else
                {
                    control.Enabled = enabled;
                }
            }
        }
    }
}
