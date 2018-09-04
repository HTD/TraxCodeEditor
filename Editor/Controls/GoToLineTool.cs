using System;
using System.Windows.Forms;

namespace Trax.Editor.Controls {

    /// <summary>
    /// Go to line tool.
    /// </summary>
    internal class GoToLineTool : EditorToolStrip {

        #region Keyboard shortcuts

        /// <summary>
        /// Gets or sets the shortcut key for "Go to line" command.
        /// </summary>
        public Keys GoToLineShortcut { get; set; } = Keys.Control | Keys.G;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when user issues go to line command.
        /// </summary>
        public event EventHandler<GoToLineEventArgs> GoToLine;

        #endregion

        #region Methods

        /// <summary>
        /// Creates go to line tool.
        /// </summary>
        public GoToLineTool() : base() {
            LabelText = I18N.GoToLine + ':';
            LineBox.ToolTipText = I18N.LineNumber;
        }

        /// <summary>
        /// Creates go to line controls.
        /// </summary>
        protected override void CreateItems() {
            LineBox = new ToolStripTextBox();
            LineBox.Leave += (s, e) => Close();
            LineBox.KeyDown += LineBox_KeyDown;
            FocusedItem = LineBox;
        }

        /// <summary>
        /// Filters possible input to integer numbers only.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LineBox_KeyDown(object sender, KeyEventArgs e) {
            var valid = false;
            valid |= e.KeyData >= Keys.D0 && e.KeyData <= Keys.D9;
            valid |= e.KeyData >= Keys.NumPad0 && e.KeyData <= Keys.NumPad9;
            valid |= e.KeyData == Keys.Enter;
            valid |= e.KeyData == Keys.Escape;
            valid |= e.KeyData == Keys.Back;
            valid |= e.KeyData == Keys.Delete;
            valid |= e.KeyData == Keys.Left;
            valid |= e.KeyData == Keys.Right;
            valid |= e.KeyData == (Keys.Shift | Keys.Left);
            valid |= e.KeyData == (Keys.Shift | Keys.Right);
            valid |= e.KeyData == (Keys.Control | Keys.X);
            valid |= e.KeyData == (Keys.Control | Keys.C);
            valid |= e.KeyData == (Keys.Control | Keys.V);
            e.SuppressKeyPress = e.Handled = !valid;
        }

        /// <summary>
        /// Adds controls to the strip.
        /// </summary>
        protected override void AddItems() => Items.Add(LineBox);

        /// <summary>
        /// Default action on enter key.
        /// </summary>
        protected override void EnterAction() {
            if (!String.IsNullOrEmpty(LineBox.Text)) {
                bool isParsed = int.TryParse(LineBox.Text, out var lineNumber);
                if (isParsed) OnGoToLine(new GoToLineEventArgs(lineNumber));
            }
        }

        /// <summary>
        /// Triggers <see cref="GoToLine"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnGoToLine(GoToLineEventArgs e) => GoToLine?.Invoke(this, e);

        #endregion

        #region Controls

        ToolStripTextBox LineBox;

        #endregion

    }

    /// <summary>
    /// Event arguments for <see cref="GoToLineTool.GoToLine"/> event.
    /// </summary>
    internal class GoToLineEventArgs : EventArgs {

        /// <summary>
        /// Gets the line number entered.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Creates new event arguments for <see cref="GoToLineTool.GoToLine"/> event.
        /// </summary>
        /// <param name="lineNumber">Line number entered.</param>
        public GoToLineEventArgs(int lineNumber) => LineNumber = lineNumber;

    }

}