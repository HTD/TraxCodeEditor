using System;
using System.Windows.Forms;

namespace Trax.Editor.Controls {

    internal class ReplaceTool : FindTool {

        #region Keyboard shortcuts

        /// <summary>
        /// Gets or sets the shortcut key for "Replace" command.
        /// </summary>
        public Keys ReplaceShortcut { get; set; } = Keys.Control | Keys.H;

        /// <summary>
        /// Gets or sets the shortcut key for "Replace next" command.
        /// </summary>
        public Keys ReplaceNextShortcut { get; set; } = Keys.Alt | Keys.R;

        /// <summary>
        /// Gets or sets the shortcut key for "Replace all" command.
        /// </summary>
        public Keys ReplaceAllShortcut { get; set; } = Keys.Alt | Keys.A;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when user issues replace command either with enter key or a button.
        /// </summary>
        public event EventHandler<FindEventArgs> Replace;

        #endregion

        #region Methods

        /// <summary>
        /// Creates replace tool.
        /// </summary>
        public ReplaceTool() {
            CloseOnEnter = false;
            LabelText = "Replace:";
            FindBox.ToolTipText = "Find text";
            ReplaceBox.ToolTipText = "Replace with";
            MatchCase.ToolTipText = "Match case (Alt+C)";
            UseRegularExpressions.ToolTipText = "Match regular expressions (Alt+E)";
            FindNext.ToolTipText = "Find next (F3)";
            ReplaceNext.ToolTipText = "Replace next (Alt+R)";
            ReplaceAll.ToolTipText = "Replace all (Alt+A)";
        }

        /// <summary>
        /// Creates replace tool controls.
        /// </summary>
        protected override void CreateItems() {
            FocusedItem = FindBox;
            AddButton(MatchCase, Properties.Resources.MatchCase);
            AddButton(UseRegularExpressions, Properties.Resources.UseRegularExpressions);
            
            AddButton(FindNext, Properties.Resources.FindNext);
            AddButton(ReplaceNext, Properties.Resources.ReplaceNext);
            AddButton(ReplaceAll, Properties.Resources.ReplaceAll);
        }

        /// <summary>
        /// Adds controls to the strip.
        /// </summary>
        protected override void AddItems() {
            Items.Add(FindBox);
            Items.Add(ReplaceBox);
            Items.Add(MatchCase);
            Items.Add(UseRegularExpressions);
            Items.Add(new ToolStripSeparator());
            Items.Add(FindNext);
            Items.Add(ReplaceNext);
            Items.Add(ReplaceAll);
        }

        /// <summary>
        /// Default action on enter key.
        /// </summary>
        protected override void EnterAction() => OnFind(new FindEventArgs(this, FindDirection.Next));

        /// <summary>
        /// Default action for enter key pressed in replace box.
        /// </summary>
        protected void ReplaceEnterAction() => OnReplace(new FindEventArgs(this, FindDirection.All));

        /// <summary>
        /// Handles internal keyboard shortcuts.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message m, Keys keyData) {
            var baseHandled = base.ProcessCmdKey(ref m, keyData);
            var handled = true;
            if (keyData == ReplaceNextShortcut) OnReplace(new FindEventArgs(this, FindDirection.Next));
            else if (keyData == ReplaceAllShortcut) OnReplace(new FindEventArgs(this, FindDirection.All));
            else handled = false;
            return handled ? true : baseHandled;
        }

        /// <summary>
        /// Closes the control and performs aditional actions on close.
        /// </summary>
        protected override void Close() {
            base.Close();
            OnExit(EventArgs.Empty);
        }

        /// <summary>
        /// Triggers <see cref="Replace"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected void OnReplace(FindEventArgs e) => Replace?.Invoke(this, e);

        #endregion

        #region Controls

        public ToolStripTextBox ReplaceBox = new ToolStripTextBox() { AutoSize = false, Width = 200 };
        public ToolStripButton ReplaceNext = new ToolStripButton();
        public ToolStripButton ReplaceAll = new ToolStripButton();

        #endregion

        
    }

}