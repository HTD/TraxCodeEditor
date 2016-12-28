using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Trax.Editor.Controls {

    /// <summary>
    /// Find tool.
    /// </summary>
    internal class FindTool : EditorToolStrip {


        #region Keyboard shortcuts

        /// <summary>
        /// Gets or sets the shortcut key for "Find" command.
        /// </summary>
        public Keys FindShortcut { get; set; } = Keys.Control | Keys.F;

        /// <summary>
        /// Gets or sets the shortcut key for "Find previous" command.
        /// </summary>
        public Keys FindPreviousShortcut { get; set; } = Keys.Shift | Keys.F3;

        /// <summary>
        /// Gets or sets the shortcut key for "Find next" command.
        /// </summary>
        public Keys FindNextShortcut { get; set; } = Keys.F3;

        /// <summary>
        /// Gets or sets the shortcut key for "Match case" command.
        /// </summary>
        public Keys MatchCaseShortcut { get; set; } = Keys.Alt | Keys.C;

        /// <summary>
        /// Gets or sets the shortcut key for "Use regular expressions" command.
        /// </summary>
        public Keys UseRegularExpressionsShortcut { get; set; } = Keys.Alt | Keys.E;

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
        /// Occurs when user issues find command either with enter key or a button.
        /// </summary>
        public event EventHandler<FindEventArgs> Find;

        /// <summary>
        /// Occurs when user issues replace command either with enter key or a button.
        /// </summary>
        public event EventHandler<FindEventArgs> Replace;

        /// <summary>
        /// Occurs when user exits the control.
        /// </summary>
        public event EventHandler Exit;

        #endregion

        #region Methods

        /// <summary>
        /// Creates find tool.
        /// </summary>
        public FindTool() : base() {
            FindBox.ToolTipText = I18N.SearchTerm;
            ReplaceBox.ToolTipText = I18N.ReplacementTerm;
            MatchCase.ToolTipText = I18N.MatchCase + ' ' + GetShortcutDescription(MatchCaseShortcut);
            UseRegularExpressions.ToolTipText = I18N.UseRegularExpressions + ' ' + GetShortcutDescription(UseRegularExpressionsShortcut);
            FindPrevious.ToolTipText = I18N.FindPrevious + ' ' + GetShortcutDescription(FindPreviousShortcut);
            FindNext.ToolTipText = I18N.FindNext + ' ' + GetShortcutDescription(FindNextShortcut);
            ReplaceNext.ToolTipText = I18N.ReplaceNext + ' ' + GetShortcutDescription(ReplaceNextShortcut);
            ReplaceAll.ToolTipText = I18N.ReplaceAll + ' ' + GetShortcutDescription(ReplaceAllShortcut);
            FindPrevious.Click += (s, e) => OnFind(new FindEventArgs(this, FindDirection.Previous));
            FindNext.Click += (s, e) => OnFind(new FindEventArgs(this, FindDirection.Next));
        }

        /// <summary>
        /// Executes find event handler with <see cref="FindDirection.Previous"/> argument.
        /// </summary>
        public void GoFindPrevious() => OnFind(new FindEventArgs(this, FindDirection.Previous));

        /// <summary>
        /// Executes find event handler with <see cref="FindDirection.Next"/>.
        /// </summary>
        public void GoFindNext() => OnFind(new FindEventArgs(this, FindDirection.Next));

        /// <summary>
        /// Creates find tool controls.
        /// </summary>
        protected override void CreateItems() {
            FocusedItem = FindBox;
            AddButton(MatchCase, Properties.Resources.MatchCase);
            AddButton(UseRegularExpressions, Properties.Resources.UseRegularExpressions);
            AddButton(FindPrevious, Properties.Resources.FindPrevious);
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
            Items.Add(FindPrevious);
            Items.Add(FindNext);
            Items.Add(ReplaceNext);
            Items.Add(ReplaceAll);
        }

        /// <summary>
        /// Configures find or replace context.
        /// </summary>
        /// <param name="context"></param>
        internal override void OnBeforeShow(string context) {
            if (context == "replace") {
                LabelText = I18N.Replace + ':';
                ReplaceBox.Visible = ReplaceNext.Visible = ReplaceAll.Visible = true;
            } else {
                LabelText = I18N.Find + ':';
                ReplaceBox.Visible = ReplaceNext.Visible = ReplaceAll.Visible = false;
            }
        }

        /// <summary>
        /// Default action on enter key.
        /// </summary>
        protected override void EnterAction() => OnFind(new FindEventArgs(this, FindDirection.Next));

        /// <summary>
        /// Handles internal keyboard shortcuts.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message m, Keys keyData) {
            var baseHandled = base.ProcessCmdKey(ref m, keyData);
            var handled = true;
            if (keyData == MatchCaseShortcut) MatchCase.Checked = !MatchCase.Checked;
            else if (keyData == UseRegularExpressionsShortcut) UseRegularExpressions.Checked = !UseRegularExpressions.Checked;
            else handled = false;

            //if (keyData == ReplaceNextShortcut) OnReplace(new FindEventArgs(this, FindDirection.Next));
            //else if (keyData == ReplaceAllShortcut) OnReplace(new FindEventArgs(this, FindDirection.All));

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
        /// Triggers <see cref="Find"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected void OnFind(FindEventArgs e) => Find?.Invoke(this, e);

        /// <summary>
        /// Triggers <see cref="Replace"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected void OnReplace(FindEventArgs e) => Replace?.Invoke(this, e);

        /// <summary>
        /// Triggers <see cref="Close"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected void OnExit(EventArgs e) => Exit?.Invoke(this, e);

        #endregion

        #region Controls

        internal ToolStripTextBox FindBox = new ToolStripTextBox() { AutoSize = false, Width = 200 };
        internal ToolStripButton MatchCase = new ToolStripButton() { CheckOnClick = true };
        internal ToolStripButton UseRegularExpressions = new ToolStripButton() { CheckOnClick = true };
        internal ToolStripButton FindPrevious = new ToolStripButton();
        internal ToolStripButton FindNext = new ToolStripButton();

        internal ToolStripTextBox ReplaceBox = new ToolStripTextBox() { AutoSize = false, Width = 200 };
        internal ToolStripButton ReplaceNext = new ToolStripButton();
        internal ToolStripButton ReplaceAll = new ToolStripButton();

        #endregion

    }

    /// <summary>
    /// Find operation direction.
    /// </summary>
    internal enum FindDirection { None, Previous, Next, All }

    /// <summary>
    /// Event arguments for <see cref="FindTool.Find"/> event.
    /// </summary>
    internal class FindEventArgs : EventArgs {
        
        /// <summary>
        /// Gets text to find.
        /// </summary>
        public string Search { get; protected set; }
        /// <summary>
        /// Gets replacement text.
        /// </summary>
        public string Replace { get; protected set; }
        /// <summary>
        /// Gets find direction.
        /// </summary>
        public FindDirection Direction { get; protected set; }
        /// <summary>
        /// Gets the flag indicating whether operation should match character case.
        /// </summary>
        public bool MatchCase { get; protected set; }
        /// <summary>
        /// Gets the flag indicating whether operation should match regular expressions.
        /// </summary>
        public bool UseRegularExpressions { get; protected set; }

        /// <summary>
        /// Creates new event arguments for <see cref="FindTool.Find"/> event.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="direction">Find direction.</param>
        public FindEventArgs(FindTool source, FindDirection direction) {
            Search = source.FindBox.Text;
            Direction = direction;
            MatchCase = source.MatchCase.Checked;
            UseRegularExpressions = source.UseRegularExpressions.Checked;
        }

        /// <summary>
        /// Empty constructor for derived classes.
        /// </summary>
        protected FindEventArgs() { }

    }

}