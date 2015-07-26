using ScintillaNET;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trax {

    /// <summary>
    /// Advanced extensible Scintilla editor control
    /// </summary>
    public partial class CodeEditor : Scintilla {

        #region WinAPI constants

        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;

        private const int SB_ENDSCROLL = 8;

        #endregion

        #region Private fields

        /// <summary>
        /// Static constructor sets static fields to default values
        /// </summary>
        private static PropertyInfo[] _StyleProperties;
        /// <summary>
        /// MaxLineNumberCharLength property cache
        /// </summary>
        private int _MaxLineNumberCharLength;
        /// <summary>
        /// ShowLineNumbers property cache
        /// </summary>
        private bool _ShowLineNumbers;
        /// <summary>
        /// Container lexer cache
        /// </summary>
        IContainerLexer _ContainerLexer;

        FoldingStyles _FoldingStyle;

        Color _FoldingBackColor;
        Color _FoldingFillColor;
        Color _FoldingLineColor;

        #endregion

        #region Properties

        /// <summary>
        /// Container lexer module (for custom lexers)
        /// </summary>
        [Browsable(true)]
        public IContainerLexer ContainerLexer {
            get { return _ContainerLexer; }
            set {
                _ContainerLexer = value;
                Lexer = Lexer.Container;
                if (ColorScheme != null) ColorScheme.ResetSyntax();
            }
        }

        /// <summary>
        /// Container lexer mode: FullAuto or Visible
        /// </summary>
        [Browsable(true)]
        public ContainerLexerModes ContainerLexerMode { get; set; }

        /// <summary>
        /// Gets color scheme module, used to load global color scheme from object's Color properties
        /// </summary>
        [Browsable(false)]
        public ColorScheme ColorScheme { get; protected set; }

        /// <summary>
        /// Indicates if line number should be shown
        /// </summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool ShowLineNumbers {
            get {
                return _ShowLineNumbers;
            }
            set {
                _MaxLineNumberCharLength = 0;
                _ShowLineNumbers = value;
                if (value) LineNumbersShow(); else LineNumbersHide();
            }
        }

        /// <summary>
        /// Gets or sets common font for all editor styles
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Font), "Consolas, 11.50pt")]
        public override Font Font {
            get {
                return base.Font;
            }
            set {
                base.Font = value;
                for (int i = 0; i < Styles.Count; i++) if (i != Style.CallTip) SetStyleFont(i, value);
            }
        }

        /// <summary>
        /// Gets or sets the font used for calltips
        /// </summary>
        [Category("Appearance")]
        [Description("Font used for calltips")]
        [DefaultValue(typeof(Font), "Microsoft Sans Serif, 9pt")]
        public Font CallTipFont {
            get {
                return GetStyleFont(Style.CallTip);
            }
            set {
                SetStyleFont(Style.CallTip, value);
            }
        }

        /// <summary>
        /// Gets or sets common background color for all editor styles
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "Window")]
        public override Color BackColor {
            get {
                return base.BackColor;
            }
            set {
                base.BackColor = value;
                for (int i = 0; i < Styles.Count; i++)
                    if (i != Style.LineNumber && i != Style.CallTip) Styles[i].BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets common text color for all editor styles
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "ControlText")]
        public override Color ForeColor {
            get {
                return base.ForeColor;
            }
            set {
                base.ForeColor = value;
                for (int i = 0; i < Styles.Count; i++)
                    if (i != Style.LineNumber && i != Style.CallTip) Styles[i].ForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets gutter background color
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets gutter background color")]
        [DefaultValue(typeof(Color), "#eeeeee")]
        public Color GutterBackColor {
            get {
                return Styles[Style.LineNumber].BackColor;
            }
            set {
                Styles[Style.LineNumber].BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets gutter text color
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets gutter text color")]
        [DefaultValue(typeof(Color), "#aaaaaa")]
        public Color GutterForeColor {
            get {
                return Styles[Style.LineNumber].ForeColor;
            }
            set {
                Styles[Style.LineNumber].ForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets calltip background color
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets calltip background color")]
        [DefaultValue(typeof(Color), "#f7f7f7")]
        public Color CallTipBackColor {
            get {
                return Styles[Style.CallTip].BackColor;
            }
            set {
                Styles[Style.CallTip].BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets calltip text color
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets calltip text color")]
        [DefaultValue(typeof(Color), "#222222")]
        public Color CallTipForeColor {
            get {
                return Styles[Style.CallTip].ForeColor;
            }
            set {
                Styles[Style.CallTip].ForeColor = value;
            }
        }

        public Color FoldingBackColor {
            get { return _FoldingBackColor; }
            set {
                _FoldingBackColor = value;
            }
        }

        public Color FoldingFillColor {
            get { return _FoldingFillColor; }
            set {
                _FoldingFillColor = value;
                Markers[Marker.Folder].SetForeColor(value);
                Markers[Marker.FolderOpen].SetForeColor(value);
                Markers[Marker.FolderEnd].SetForeColor(value);
                Markers[Marker.FolderMidTail].SetForeColor(value);
                Markers[Marker.FolderOpenMid].SetForeColor(value);
                Markers[Marker.FolderSub].SetForeColor(value);
                Markers[Marker.FolderTail].SetForeColor(value);
            }
        }

        public Color FoldingLineColor {
            get { return _FoldingFillColor; }
            set {
                _FoldingLineColor = value;
                Markers[Marker.Folder].SetBackColor(value);
                Markers[Marker.FolderOpen].SetBackColor(value);
                Markers[Marker.FolderEnd].SetBackColor(value);
                Markers[Marker.FolderMidTail].SetBackColor(value);
                Markers[Marker.FolderOpenMid].SetBackColor(value);
                Markers[Marker.FolderSub].SetBackColor(value);
                Markers[Marker.FolderTail].SetBackColor(value);
            }
        }

        public FoldingStyles FoldingStyle {
            get { return _FoldingStyle; }
            set {
                _FoldingStyle = value;
                switch(value) {
                    case FoldingStyles.None:
                        Markers[Marker.Folder].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderOpen].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderEnd].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderSub].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderTail].Symbol = MarkerSymbol.Empty;
                        break;
                    case FoldingStyles.SquareTrees:
                        Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
                        Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
                        Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
                        Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
                        Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
                        Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
                        Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
                        break;
                    case FoldingStyles.CurvyTrees:
                        Markers[Marker.Folder].Symbol = MarkerSymbol.CirclePlus;
                        Markers[Marker.FolderOpen].Symbol = MarkerSymbol.CircleMinus;
                        Markers[Marker.FolderEnd].Symbol = MarkerSymbol.CirclePlusConnected;
                        Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCornerCurve;
                        Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.CircleMinusConnected;
                        Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
                        Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCornerCurve;
                        break;
                    case FoldingStyles.PlusMinus:
                        Markers[Marker.Folder].Symbol = MarkerSymbol.Plus;
                        Markers[Marker.FolderOpen].Symbol = MarkerSymbol.Minus;
                        Markers[Marker.FolderEnd].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderSub].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderTail].Symbol = MarkerSymbol.Empty;
                        break;
                    case FoldingStyles.Arrows:
                        Markers[Marker.Folder].Symbol = MarkerSymbol.Arrow;
                        Markers[Marker.FolderOpen].Symbol = MarkerSymbol.ArrowDown;
                        Markers[Marker.FolderEnd].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderSub].Symbol = MarkerSymbol.Empty;
                        Markers[Marker.FolderTail].Symbol = MarkerSymbol.Empty;
                        break;
                }
            }

        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Occurs when mouse cursors dwells over an identifier in text, calltip text could be set and displayed at identifier's position
        /// </summary>
        public event DwellOnIdentifierEventHandler DwellOnIdentifier;

        /// <summary>
        /// Occurs when vertical page scrolling is done (vertical scrollbar released)
        /// </summary>
        public event EventHandler VScrollEnd;

        /// <summary>
        /// Occurs when horizontal page scrolling is done (horizontal scrollbar released)
        /// </summary>
        public event EventHandler HScrollEnd;

        #endregion Events

        /// <summary>
        /// Initializes static properties of the control
        /// </summary>
        static CodeEditor() {
            _StyleProperties = typeof(Style).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Creates new Scintilla editor and sets its properties to defaults
        /// </summary>
        public CodeEditor() {
            Lexer = Lexer.Null;
            Font = new Font("Consolas", 11.50f);
            CallTipFont = new Font("Microsoft Sans-Serif", 8f);
            CallTipBackColor = ColorTranslator.FromHtml("#f7f7f7");
            CallTipForeColor = ColorTranslator.FromHtml("#222222");
            ShowLineNumbers = true;
            BackColor = SystemColors.Window;  //ColorTranslator.FromHtml("#ffffff");
            ForeColor = SystemColors.ControlText; //ColorTranslator.FromHtml("#000000");
            GutterBackColor = ColorTranslator.FromHtml("#eeeeee");
            GutterForeColor = ColorTranslator.FromHtml("#aaaaaa");
            //LineEndTypesAllowed = LineEndType.Default;
            //MouseDwellTime = 100;
            //CaretLineVisible = true;
            //CaretLineBackColorAlpha = 256;
            //MouseSelectionRectangularSwitch = true;
            //AdditionalSelectionTyping = true;
            //VirtualSpaceOptions = VirtualSpace.RectangularSelection;
            //IndentationGuides = IndentView.Real;
            //ViewWhitespace = WhitespaceMode.VisibleAlways;
        }

        /// <summary>
        /// Sets current color scheme from object with color properties
        /// </summary>
        /// <param name="source"></param>
        public void SetColorScheme(object source) {
            ColorScheme = new ColorScheme(this, source);
        }

        public async Task<Document> LoadFileAsync(ILoader loader, string path, CancellationToken cancellationToken, Encoding encoding = null, bool detectBOM = true) {
            const int bufferSize = 1024 * 1024;
            var buffer = new char[bufferSize];
            var count = 0;
            try {
                using (var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize: bufferSize, useAsync: true))
                using (var reader = new StreamReader(file, encoding ?? Encoding.UTF8, detectBOM, bufferSize)) {
                    while ((count = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0) {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!loader.AddData(buffer, count)) throw new IOException("The data could not be added to the loader.");
                    }
                    return loader.ConvertToDocument();
                }
            } catch {
                loader.Release();
                throw;
            }
        }

        public async Task LoadFile(string path, Encoding encoding = null, bool detectBOM = true) {
            var _enabled = Enabled;
            var _readonly = ReadOnly;
            Enabled = false;
            ReadOnly = true;
            try {
                ClearAll();
                var loader = CreateLoader(1024 * 1024);
                if (loader == null) throw new ApplicationException("Unable to create loader.");
                var cts = new CancellationTokenSource();
                var document = await LoadFileAsync(loader, path, cts.Token, encoding, detectBOM);
                Document = document;
                ReleaseDocument(document);
                

                // The code below is a very naive workaround for the bug which actually works, but it's ridiculously slow for large files:

                //var text = Text;
                //Document = Document.Empty;
                //ReadOnly = false;
                //Text = text;

                if (ShowLineNumbers) LineNumbersShow();
                if (Lexer == Lexer.Container && ContainerLexer != null) {
                    if (ColorScheme != null) ColorScheme.ResetSyntax();
                    ApplyContainerLexer();
                }
            } catch (OperationCanceledException) { }
            catch (Exception x) {
                MessageBox.Show(this, x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                Enabled = _enabled;
                ReadOnly = _readonly;
            }
        }

        #region Private methods

        /// <summary>
        /// Returns true if specified character is considered identifier separator
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsTextSeparator(char c) {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == ';' || c == '"' || c == '\'';
        }

        /// <summary>
        /// Find substring's start position
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int FindIdentifierStart(string text, int index) {
            var length = text.Length;
            while (index > 0 && index < length && !IsTextSeparator(text[index--])) ;
            if (index >= 0) index += 2;
            return index;

        }

        /// <summary>
        /// Find substring's end position
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int FindIdentifierEnd(string text, int index) {
            var length = text.Length;
            while (index >= 0 && index < length && !IsTextSeparator(text[index++])) ;
            if (index < length) index -= 2;
            return index;
        }

        /// <summary>
        /// Returns visible range of the text
        /// </summary>
        /// <returns></returns>
        private CharacterRange GetVisibleRange() {
            var firstLine = FirstVisibleLine;
            var lines = LinesOnScreen;
            var first = Lines[firstLine].Position;
            var last = Lines[firstLine + lines].EndPosition;
            return new CharacterRange(first, last - first + 1);
            
        }

        /// <summary>
        /// Finds an identifier in visible text context
        /// </summary>
        /// <param name="index"></param>
        /// <param name="identifierRange"></param>
        /// <param name="visibleRange"></param>
        /// <param name="visibleText"></param>
        /// <returns></returns>
        private string FindIdentifierAt(int index, out CharacterRange identifierRange, out CharacterRange visibleRange, out string visibleText) {
            visibleRange = GetVisibleRange();
            visibleText = GetTextRange(visibleRange.First, visibleRange.Length);
            index -= visibleRange.First;
            if (index < 0) {
                identifierRange = default(CharacterRange);
                return null;
            }
            var start = FindIdentifierStart(visibleText, index);
            var end = FindIdentifierEnd(visibleText, index);
            if (start < 0 || end < 0 || end < start) {
                identifierRange = default(CharacterRange);
                return null;
            }
            identifierRange = new CharacterRange(start + visibleRange.First, end - start + 1);
            return visibleText.Substring(start, end - start + 1);
        }

        /// <summary>
        /// Copies all style properties from one style to another
        /// </summary>
        /// <param name="sourceIndex"></param>
        /// <param name="targetIndex"></param>
        private void CopyStyleProperties(int sourceIndex, int targetIndex) {
            foreach (PropertyInfo property in _StyleProperties)
                property.SetValue(Styles[targetIndex], property.GetValue(Styles[sourceIndex]));
        }

        /// <summary>
        /// Gets font from style properties
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Font GetStyleFont(int index) {
            var style = Styles[index];
            var fontStyle = FontStyle.Regular;
            if (style.Bold) fontStyle |= FontStyle.Bold;
            if (style.Italic) fontStyle |= FontStyle.Italic;
            if (style.Underline) fontStyle |= FontStyle.Underline;
            return new Font(style.Font, style.SizeF, fontStyle);
        }

        /// <summary>
        /// Sets font style properties
        /// </summary>
        /// <param name="index"></param>
        /// <param name="font"></param>
        private void SetStyleFont(int index, Font font) {
            Styles[index].Font = font.Name;
            Styles[index].SizeF = font.SizeInPoints;
            Styles[index].Bold = font.Bold;
            Styles[index].Italic = font.Italic;
            Styles[index].Underline = font.Underline;
        }

        /// <summary>
        /// Shows line numbers
        /// </summary>
        private void LineNumbersShow() {
            var maxLineNumberCharLength = Lines.Count.ToString().Length;
            if (maxLineNumberCharLength == _MaxLineNumberCharLength) return;
            const int padding = 2;
            Margins[0].Type = MarginType.Number;
            Margins[0].Width = TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
            _MaxLineNumberCharLength = maxLineNumberCharLength;
        }

        /// <summary>
        /// Hides line numbers
        /// </summary>
        private void LineNumbersHide() {
            Margins[0].Type = MarginType.Symbol;
            Margins[0].Width = 2;
        }

        /// <summary>
        /// Deterimines leging range and applies ContainerLexer if defined
        /// </summary>
        /// <param name="e"></param>
        void ApplyContainerLexer(StyleNeededEventArgs e = null) {
            if (_ContainerLexer != null) {
                int start, end;
                if (e != null) {
                    start = Lines[LineFromPosition(GetEndStyled())].Position;
                    end = e.Position;
                    if (ContainerLexerMode == ContainerLexerModes.Visible) {
                        var visibleStart = Lines[FirstVisibleLine].Position;
                        var visibleEnd = Lines[FirstVisibleLine + LinesOnScreen - 1].EndPosition;
                        if (start < visibleStart) start = visibleStart;
                        if (end > visibleEnd) end = visibleEnd;
                    }
                } else {
                    start = Lines[FirstVisibleLine].Position;
                    end = Lines[FirstVisibleLine + LinesOnScreen - 1].EndPosition;
                }
                _ContainerLexer.ApplyStyles(start, end);
            }
        }

        #endregion Private methods

        #region Overrides

        /// <summary>
        /// Updates gutter width when line count changes
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(EventArgs e) {
            if (ShowLineNumbers) LineNumbersShow();
            base.OnTextChanged(e);
        }

        /// <summary>
        /// Extends OnDwellStart to provide DwellOnIdentifier event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDwellStart(DwellEventArgs e) {
            if (DwellOnIdentifier != null) {
                var index = CharPositionFromPointClose(e.X, e.Y);
                if (index >= 0) {
                    string identifier = null;
                    var identifierRange = default(CharacterRange);
                    var visibleRange = default(CharacterRange);
                    string visibleText = null;
                    if (index >= 0) identifier = FindIdentifierAt(index, out identifierRange, out visibleRange, out visibleText);
                    if (identifier != null) {
                        var eventArgs = new DwellOnIdentifierEventArgs(identifier, identifierRange, visibleRange, visibleText);
                        OnDwellOnIdentifier(eventArgs);
                        if (!String.IsNullOrEmpty(eventArgs.CallTipText))
                            CallTipShow(identifierRange.First, eventArgs.CallTipText);
                        else CallTipCancel();
                    } else CallTipCancel();
                } else CallTipCancel();
            }
            base.OnDwellStart(e);
        }

        /// <summary>
        /// Provides DwellOnIdentifier event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDwellOnIdentifier(DwellOnIdentifierEventArgs e) {
            if (DwellOnIdentifier != null) DwellOnIdentifier.Invoke(this, e);
        }

        /// <summary>
        /// Provides VScrollEnd event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnVScrollEnd(EventArgs e) {
            if (VScrollEnd != null) VScrollEnd.Invoke(this, e);
        }

        /// <summary>
        /// Provides HScrollEnd event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHScrollEnd(EventArgs e) {
            if (HScrollEnd != null) HScrollEnd.Invoke(this, e);
        }

        /// <summary>
        /// Provides some missing events from window messages
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_VSCROLL && (int)m.WParam == SB_ENDSCROLL) OnVScrollEnd(EventArgs.Empty);
            if (m.Msg == WM_HSCROLL && (int)m.WParam == SB_ENDSCROLL) OnHScrollEnd(EventArgs.Empty);
            base.WndProc(ref m);
        }

        /// <summary>
        /// Invokes ContainerLexer in FullAuto mode
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStyleNeeded(StyleNeededEventArgs e) {
            if (Lexer == Lexer.Container && ContainerLexerMode == ContainerLexerModes.FullAuto)
                ApplyContainerLexer(e);
            base.OnStyleNeeded(e);
        }

        /// <summary>
        /// Invokes ContainerLexer in Visible mode
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUpdateUI(UpdateUIEventArgs e) {
            if (Lexer == Lexer.Container && ContainerLexerMode == ContainerLexerModes.Visible)
                ApplyContainerLexer();
            base.OnUpdateUI(e);
        }

        #endregion Overrides

    }

    public enum ContainerLexerModes {
        /// <summary>
        /// Document is parsed from last unscanned position to last visible line
        /// </summary>
        FullAuto,
        /// <summary>
        /// Visible part of the document is parsed on each UI change
        /// </summary>
        Visible
    }

    public enum FoldingStyles {
        None, SquareTrees, CurvyTrees, PlusMinus, Arrows
    }

}