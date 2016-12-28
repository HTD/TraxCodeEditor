using System;
using System.Drawing;
using System.Windows.Forms;
using Trax.Helpers.Bitmaps;

namespace Trax.Editor.Controls {

    /// <summary>
    /// Editor tool strip base class.
    /// </summary>
    abstract public class EditorToolStrip : ToolStrip {

        /// <summary>
        /// Gets or sets tool strip label.
        /// </summary>
        public virtual string LabelText {
            get { return Label.Text; }
            set { Label.Text = value; }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether the tool should be closed when enter key is pressed.
        /// </summary>
        protected virtual bool CloseOnEnter { get; set; } = true;

        /// <summary>
        /// Creates new editor tool strip.
        /// </summary>
        public EditorToolStrip() {
            DoubleBuffered = true;
            SuspendLayout();
            RenderMode = ToolStripRenderMode.System;
            BackColor = SystemColors.Control;
            ImageList = new ImageList() { ColorDepth = ColorDepth.Depth32Bit };
            AddButton(CloseButton, Properties.Resources.Close, true);
            GripStyle = ToolStripGripStyle.Hidden;
            Padding = new Padding(5, 5, 0, 5);
            Cursor = Cursors.Arrow;
            Items.Add(Label = new ToolStripLabel());
            CreateItems();
            AddItems();
            Items.Add(CloseButton);
            CloseButton.Click += CloseButton_Click;
            Anchor = AnchorStyles.Right;
            ParentChanged += EditorToolStrip_ParentChanged;
        }

        /// <summary>
        /// Handles adding this tool strip to the parent control and removing the tool when no longer used.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditorToolStrip_ParentChanged(object sender, EventArgs e) {
            if (Editor != null) {
                EditorCaretPeriodCache = Editor.CaretPeriod;
                EditorReadOnlyCache = Editor.ReadOnly;
                Editor.CaretPeriod = 0;
                Left = Editor.ClientSize.Width - Width;
                ResumeLayout();
            }
        }

        /// <summary>
        /// In derived class control creation (with optional event handling) should be defined here.
        /// </summary>
        protected abstract void CreateItems();

        /// <summary>
        /// In derived class items created in <see cref="CreateItems"/> method should be added to the strip here.
        /// </summary>
        protected abstract void AddItems();

        /// <summary>
        /// In derived class enter key action for <see cref="FocusedItem"/> should be defined here.
        /// </summary>
        protected abstract void EnterAction();

        #region Non-public

        /// <summary>
        /// Adds a new button to the strip.
        /// </summary>
        /// <param name="button">Button control.</param>
        /// <param name="image">Bitmap image.</param>
        /// <param name="dimmed">Should the image be dimmed by default.</param>
        protected void AddButton(ToolStripButton button, Bitmap image, bool dimmed = false) {
            ImageList.Images.Add(image);
            ImageList.Images.Add(FX.GrayScale(image, 0.4));
            button.AutoSize = false;
            button.Height = 24;
            button.Margin = new Padding(0, 4, 0, 0);
            button.ImageIndex = AddButtonIndex * 2 + (dimmed ? 1 : 0);
            button.Tag = dimmed;
            button.MouseLeave += Button_MouseLeave;
            button.MouseEnter += Button_MouseEnter;
            button.CheckedChanged += Button_CheckedChanged;
            AddButtonIndex++;
        }

        /// <summary>
        /// Closes the control by removing it from the parent control's controls.
        /// </summary>
        protected virtual void Close() {
            lock (CloseLock) {
                if (Editor != null) {
                    var editor = Editor;
                    editor.CaretPeriod = EditorCaretPeriodCache;
                    editor.ReadOnly = EditorReadOnlyCache;
                    editor.Focus();
                    editor.Controls.Remove(this);
                }
            }
        }

        /// <summary>
        /// Handles focus event of the strip - passes the focus to the <see cref="FocusedItem"/>.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(EventArgs e) {
            base.OnGotFocus(e);
            FocusedItem?.Focus();
            var textBox = FocusedItem as ToolStripTextBox;
            textBox?.SelectAll();
        }

        /// <summary>
        /// Handles Escape and Enter keys default actions.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message m, Keys keyData) {
            if (keyData == Keys.Escape) { Close(); return true; }
            if (keyData == Keys.Enter) {
                EnterAction();
                if (CloseOnEnter) Close();
                return true;
            }
            return base.ProcessCmdKey(ref m, keyData);
        }

        /// <summary>
        /// Handles events of the mouse leaving the buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseLeave(object sender, EventArgs e) {
            var button = sender as ToolStripButton;
            button.BackColor = SystemColors.Control;
            if ((bool)button.Tag) if (!button.Checked) button.ImageIndex |= 1;
        }

        /// <summary>
        /// Handles events of the mouse entering the buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseEnter(object sender, EventArgs e) {
            var button = sender as ToolStripButton;
            button.BackColor = SystemColors.ButtonHighlight;
            if ((bool)button.Tag) button.ImageIndex &= ~1;
        }

        /// <summary>
        /// Handles events of checking and unchecking the buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_CheckedChanged(object sender, EventArgs e) {
            var button = sender as ToolStripButton;
            if (button.Checked) button.ImageIndex &= ~1;
        }

        /// <summary>
        /// Handles close button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, EventArgs e) => Close();

        #region Data

        /// <summary>
        /// Contains control reference which should be focussed by default.
        /// </summary>
        protected virtual ToolStripControlHost FocusedItem { get; set; }

        /// <summary>
        /// Contains code editor reference.
        /// </summary>
        protected CodeEditor Editor {
            get { return base.Parent as CodeEditor; }
            set { base.Parent = value; }
        }

        /// <summary>
        /// The tool text label control.
        /// </summary>
        private readonly ToolStripLabel Label;

        /// <summary>
        /// The close button control.
        /// </summary>
        private readonly ToolStripButton CloseButton = new ToolStripButton();

        /// <summary>
        /// Since editor caret blinking must be disabled before entering <see cref="ToolStripTextBox"/>, here the original blink rate is stored.
        /// </summary>
        private int EditorCaretPeriodCache;

        /// <summary>
        /// Editor ReadOnly property is stored here.
        /// </summary>
        private bool EditorReadOnlyCache;

        /// <summary>
        /// A lock used to protect closing operation.
        /// </summary>
        private readonly object CloseLock = new object();

        /// <summary>
        /// Current index for a new button. Used to calculate ImageIndex for buttons.
        /// </summary>
        private int AddButtonIndex;

        #endregion

        #endregion

    }

}
