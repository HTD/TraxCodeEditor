using System.Drawing;
using System.Windows.Forms;

namespace CustomRenderers {

    public class ToolStripRendererOverride : ToolStripProfessionalRenderer {

        public ToolStripRendererOverride() { }
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
            var pen = new Pen(SystemColors.HotTrack);
            var rectangle = e.AffectedBounds;
            rectangle.Offset(0, -1);
            e.Graphics.DrawRectangle(pen, rectangle);
        }

    }

}
