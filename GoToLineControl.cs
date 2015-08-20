using System;
using System.Windows.Forms;
using System.Drawing;

namespace Trax {

    class GoToLineControl : ToolStrip {

        public new CodeEditor Parent {
            get { return base.Parent as CodeEditor; }
            set { base.Parent = value; }
        }


        public string LabelText { get; set; }
        private int EditorCaretPeriodCache;
        private bool EditorReadOnlyCache;
        private bool IsBeingRemoved;
        private object RemovalLock = new object();
        private ToolStripTextBox TextBox { get; set; }
        private ToolStripButton CloseButton { get; set; }


        public GoToLineControl(CodeEditor editor) {
            Parent = editor;
            RenderMode = ToolStripRenderMode.System;
            GripStyle = ToolStripGripStyle.Hidden;
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
            Padding = new Padding(5, 5, 0, 5);
            Cursor = Cursors.Arrow;
            LabelText = "Go to line:";
            EditorCaretPeriodCache = Parent.CaretPeriod;
            EditorReadOnlyCache = Parent.ReadOnly;
            Parent.CaretPeriod = 0;
            Items.Add(new ToolStripLabel(LabelText));
            Items.Add(TextBox = new ToolStripTextBox());
            Items.Add(CloseButton = new ToolStripButton(Properties.Resources.CloseInactive));
            CloseButton.Click += CloseButton_Click;
            CloseButton.MouseEnter += CloseButton_MouseEnter;
            CloseButton.MouseLeave += CloseButton_MouseLeave;
            Parent.Controls.Add(this);
            Anchor = AnchorStyles.Right;
            Left = Parent.ClientSize.Width - Width;
            TextBox.Focus();
        }

        private void CloseButton_MouseEnter(object sender, EventArgs e) {
            var button = sender as ToolStripButton;
            button.Image = Properties.Resources.CloseActive;
        }

        private void CloseButton_MouseLeave(object sender, EventArgs e) {
            var button = sender as ToolStripButton;
            button.Image = Properties.Resources.CloseInactive;
        }

        protected override bool ProcessCmdKey(ref Message m, Keys keyData) {
            if (keyData.HasFlag(Keys.Escape)) { RemoveIt(); return true; }
            if (keyData.HasFlag(Keys.Enter)) {
                Parent.GoToLine(TextBox.Text);
                RemoveIt();
            }
            return base.ProcessCmdKey(ref m, keyData);
        }

        private void RemoveIt() {
            lock (RemovalLock) {
                if (!IsBeingRemoved) {
                    IsBeingRemoved = true;
                    TextBox.Leave -= TextBox_Leave;
                    Parent.CaretPeriod = EditorCaretPeriodCache;
                    Parent.ReadOnly = EditorReadOnlyCache;
                    Parent.Focus();
                    Parent.Controls.Remove(this);
                    Dispose();
                }
            }
        }

        private void TextBox_Leave(object sender, EventArgs e) {
            RemoveIt();
        }

        private void CloseButton_Click(object sender, EventArgs e) {
            RemoveIt();
        }

    }

}
