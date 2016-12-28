using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;
using Trax.Editor.Controls;

namespace Trax.Editor {

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
        /// A buffer size which is used for loading files as byte streams
        /// </summary>
        private const int LoadingBufferSize = 1048576; // 1MB

        /// <summary>
        /// A number of margin where the folding symbols are displayed
        /// </summary>
        private const int FoldMarginIndex = 2;

        /// <summary>
        /// A string used as unit of indentation
        /// </summary>
        private string IndentationUnit;

        #region Property cache

        private static PropertyInfo[] _StyleProperties;
        private int _MaxLineNumberCharLength;
        private bool _ShowLineNumbers;
        IContainerLexer _ContainerLexer;
        FoldingStyles _FoldingStyle;
        Color _FoldMarginColor;
        Color _FoldingFillColor;
        Color _FoldingLineColor;
        private bool _ShowFoldMargin;
        private KeywordSets _Keywords;
        private Presets _Preset;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets encoding used for file operations.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets current file path
        /// </summary>
        public string Path { get; protected set; }

        /// <summary>
        /// Container lexer module (for custom lexers).
        /// </summary>
        [Browsable(true)]
        public IContainerLexer ContainerLexer {
            get { return _ContainerLexer; }
            set {
                _ContainerLexer = value;
                Lexer = Lexer.Container;
                if (ColorScheme != null) ColorScheme.ResetSyntax();
                if (StyleScheme != null) StyleScheme.ResetSyntax();
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
        /// Gets style scheme module, used to load global style scheme from object's FontStyle properties
        /// </summary>
        [Browsable(false)]
        public StyleScheme StyleScheme { get; protected set; }
        /// <summary>
        /// Sets predefined build in color scheme, style scheme and line folding scheme or gets the current one
        /// </summary>
        [Category("Appearance")]
        [Description("Sets predefined build in color scheme, style scheme and line folding scheme or gets the current one")]
        [DefaultValue(typeof(Presets), "Google")]
        public Presets Preset {
            get {
                return _Preset;
            }
            set {
                switch (_Preset = value) {
                    case Presets.Google:
                        SetColorScheme(BuiltInColorSchemes.Google.Default);
                        SetStyleScheme(BuiltInStyleSchemes.SemiBold.Default);
                        FoldingStyle = FoldingStyles.CurvyTrees;
                        break;
                    case Presets.VSLight:
                        SetColorScheme(BuiltInColorSchemes.VSLight.Default);
                        SetStyleScheme(BuiltInStyleSchemes.SemiBold.Default);
                        FoldingStyle = FoldingStyles.SquareTrees;
                        break;
                    case Presets.VSDark:
                        SetColorScheme(BuiltInColorSchemes.VSDark.Default);
                        SetStyleScheme(BuiltInStyleSchemes.SemiBold.Default);
                        FoldingStyle = FoldingStyles.SquareTrees;
                        break;
                    case Presets.Trax:
                        SetColorScheme(BuiltInColorSchemes.Trax.Default);
                        SetStyleScheme(BuiltInStyleSchemes.SemiBold.Default);
                        FoldingStyle = FoldingStyles.CurvyTrees;
                        break;
                    case Presets.Oblivion:
                        SetColorScheme(BuiltInColorSchemes.Oblivion.Default);
                        SetStyleScheme(BuiltInStyleSchemes.SemiBold.Default);
                        FoldingStyle = FoldingStyles.Arrows;
                        break;
                    case Presets.Zenburn:
                        SetColorScheme(BuiltInColorSchemes.Zenburn.Default);
                        SetStyleScheme(BuiltInStyleSchemes.SemiBold.Default);
                        FoldingStyle = FoldingStyles.CurvyTrees;
                        break;
                }
            }
        }

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
        [DefaultValue(typeof(Font), "Consolas, 9.75pt")]
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
        [DefaultValue(typeof(Color), "WindowText")]
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

        [Category("Appearance")]
        [Description("Gets or sets fold margin color")]
        [DefaultValue(typeof(Color), "Window")]
        public Color FoldMarginColor {
            get { return _FoldMarginColor; }
            set {
                SetFoldMarginColor(true, _FoldMarginColor = value);
                SetFoldMarginHighlightColor(true, value);
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

        /// <summary>
        /// Gets or sets folding symbols.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets folding symbols.")]
        [DefaultValue(typeof(FoldingStyles), "SquareTrees")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public FoldingStyles FoldingStyle {
            get { return _FoldingStyle; }
            set {
                _FoldingStyle = value;
                switch (value) {
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

        /// <summary>
        /// Gets or sets fold margin display state.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets fold margin display state.")]
        [DefaultValue(false)]
        public bool ShowFoldMargin {
            get {
                return _ShowFoldMargin;
            }
            set {
                if (_ShowFoldMargin = value) {
                    AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
                    SetProperty("fold", "1");
                    SetProperty("fold.compact", "1");
                    Margins[FoldMarginIndex].Type = MarginType.Symbol;
                    Margins[FoldMarginIndex].Mask = Marker.MaskFolders;
                    Margins[FoldMarginIndex].Sensitive = true;
                    Margins[FoldMarginIndex].Width = 13;
                }
                else {
                    AutomaticFold = AutomaticFold.None;
                    SetProperty("fold", "0");
                    SetProperty("fold.compact", "0");
                    Margins[FoldMarginIndex].Mask = Marker.MaskAll;
                    Margins[FoldMarginIndex].Sensitive = false;
                    Margins[FoldMarginIndex].Width = 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets keywords sets to be assigned for current lexer.
        /// </summary>
        [Category("Behavior")]
        [Description("Gets or sets keywords sets to be assigned for current lexer.")]
        [DefaultValue(typeof(KeywordSets), "None")]
        public KeywordSets Keywords {
            get {
                return _Keywords;
            }
            set {
                switch (_Keywords = value) {
                    case KeywordSets.None:
                    default:
                        for (int i = 0; i < 8; i++) SetKeywords(i, null);
                        break;
                    case KeywordSets.ECMAScript:
                        SetKeywords(0, "abstract await break case class catch const continue debugger default delete do else enum export extends final finally for function goto if implements import in instanceof interface let native new number object package private protected public return super static string switch synchronized this throw try typeof var void while with yield");
                        SetKeywords(1, "null true false undefined NaN");
                        SetKeywords(2, "Array ArrayBuffer Boolean DataView Date Error EvalError Float32Array Float64Array Function Generator GeneratorFunction Infinity Int16Array Int32Array Int8Array InternalError Intl Iterator JSON Map Math NaN Number Object ParallelArray Promise Proxy RangeError ReferenceError Reflect RegExp SIMD Set StopIteration String Symbol SyntaxError TypeError TypedArray URIError Uint16Array Uint32Array Uint8Array Uint8ClampedArray WeakMap WeakSet");
                        SetKeywords(3, "decodeURI decodeURIComponent encodeURI encodeURIComponent escape eval isFinite isNaN parseFloat parseInt unescape uneval");
                        break;
                    case KeywordSets.TypeScript:
                        SetKeywords(0, "abstract any await break case class catch const continue debugger declare default delete do else enum export extends final finally for function goto if implements import in instanceof interface let module native new number object package private protected public return super static string switch synchronized this throw try typeof var void while with yield");
                        SetKeywords(1, "null true false undefined NaN");
                        SetKeywords(2, "Array ArrayBuffer Boolean DataView Date Error EvalError Float32Array Float64Array Function Generator GeneratorFunction Infinity Int16Array Int32Array Int8Array InternalError Intl Iterator JSON Map Math NaN Number Object ParallelArray Promise Proxy RangeError ReferenceError Reflect RegExp SIMD Set StopIteration String Symbol SyntaxError TypeError TypedArray URIError Uint16Array Uint32Array Uint8Array Uint8ClampedArray WeakMap WeakSet");
                        SetKeywords(3, "decodeURI decodeURIComponent encodeURI encodeURIComponent escape eval isFinite isNaN parseFloat parseInt unescape uneval");
                        break;

                }
            }
        }

        /// <summary>
        /// Gets or sets the width of a tab as a multiple of a space character.
        /// </summary>
        [DefaultValue(4)]
        public new int TabWidth {
            get {
                return base.TabWidth;
            }
            set {
                base.TabWidth = value;
                IndentationUnit = base.UseTabs ? "\t" : "".PadRight(value);
            }
        }

        /// <summary>
        /// Gets or sets whether to use a mixture of tabs and spaces for indentation or purely spaces.
        /// </summary>
        [DefaultValue(false)]
        public new bool UseTabs {
            get {
                return base.UseTabs;
            }
            set {
                base.UseTabs = value;
                IndentationUnit = value ? "\t" : "".PadRight(base.TabWidth);
            }
        }

        /// <summary>
        /// Gets or sets automatic indentation feature.
        /// </summary>
        [Category("Behavior")]
        [Description("Gets or sets automatic indentation feature.")]
        [DefaultValue(true)]
        public bool AutoIndent { get; set; }

        /// <summary>
        /// Gets or sets current lexer.
        /// </summary>
        public new Lexer Lexer {
            get {
                return base.Lexer;
            }
            set {
                base.Lexer = value;
                if (ColorScheme != null) ColorScheme.Reset();
                if (StyleScheme != null) StyleScheme.Reset();
            }
        }

        /// <summary>
        /// Gets presets menu.
        /// </summary>
        public ToolStripItem PresetsMenu {
            get {
                var schemes = new ToolStripMenuItem("Schemes");
                var current = Preset.ToString();
                foreach (var preset in Enum.GetNames(typeof(Presets))) {
                    schemes.DropDownItems.Add(
                        new ToolStripMenuItem(
                            preset,
                            null,
                            OnSelectScheme
                        )
                    );
                }
                return schemes;
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

        #region Constructors

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
            AdditionalSelectionTyping = true;
            AutoIndent = true;
            BackColor = SystemColors.Window;
            CallTipFont = new Font("Microsoft Sans-Serif", 8f);
            CallTipBackColor = ColorTranslator.FromHtml("#f7f7f7");
            CallTipForeColor = ColorTranslator.FromHtml("#222222");
            CaretLineVisible = true;
            CaretLineBackColorAlpha = 256;
            Encoding = Encoding.UTF8;
            FoldMarginColor = SystemColors.Window;
            FoldingStyle = FoldingStyles.SquareTrees;
            Font = new Font("Consolas", 9.75f);
            ForeColor = SystemColors.WindowText;
            GutterBackColor = ColorTranslator.FromHtml("#eeeeee");
            GutterForeColor = ColorTranslator.FromHtml("#aaaaaa");
            IndentationGuides = IndentView.Real;
            Lexer = Lexer.Null;
            LineEndTypesAllowed = LineEndType.Default;
            MouseDwellTime = 100;
            MouseSelectionRectangularSwitch = true;
            ShowLineNumbers = true;
            TabWidth = 4;
            UseTabs = false;
            VirtualSpaceOptions = VirtualSpace.RectangularSelection;
            Preset = Presets.Trax;
            GoToLineTool = new GoToLineTool();
            GoToLineTool.GoToLine += (s, e) => OnGoToLine(e);
            FindTool = new FindTool();
            FindTool.Find += (s, e) => OnFind(e);
            FindTool.Replace += (s, e) => OnReplace(e);
            FindTool.Exit += (s, e) => { Find_LastIndex = -1; Find_LastLength = 0; };
        }

        #endregion Constructors

        #region Public methods

        /// <summary>
        /// Sets current color scheme from object with Color properties
        /// </summary>
        /// <param name="source"></param>
        public void SetColorScheme(object source) {
            ColorScheme = new ColorScheme(this, source);
        }

        /// <summary>
        /// Sets current style scheme from object with FontStyle properties
        /// </summary>
        /// <param name="source"></param>
        public void SetStyleScheme(object source) {
            StyleScheme = new StyleScheme(this, source);
        }

        /// <summary>
        /// Loads a file using background document loader, background task (outside UI thread)
        /// </summary>
        /// <param name="loader">ILoader instance created with CreateLoader() method</param>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="encoding"></param>
        /// <param name="detectBOM"></param>
        /// <returns></returns>
        private async Task<Document> LoadDocumentAsync(ILoader loader, string path, CancellationToken cancellationToken, Encoding encoding = null, bool detectBOM = true) {
            var buffer = new char[LoadingBufferSize];
            var count = 0;
            try {
                using (var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize: LoadingBufferSize, useAsync: true)) {
                    if (encoding != null) Encoding = encoding;
                    else if (Encoding == null) Encoding = Encoding.UTF8;
                    using (var reader = new StreamReader(file, Encoding, detectBOM, LoadingBufferSize)) {
                        while ((count = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0) {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (!loader.AddData(buffer, count)) throw new IOException("The data could not be added to the loader.");
                        }
                        return loader.ConvertToDocument();
                    }
                }
            }
            catch {
                loader.Release();
                throw;
            }
        }

        /// <summary>
        /// Loads a file using background document loader
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <param name="detectBOM"></param>
        /// <returns></returns>
        public async Task LoadFileAsync(string path, Encoding encoding = null, bool detectBOM = true) {
            var _enabled = Enabled;
            var _readonly = ReadOnly;
            Enabled = false;
            ReadOnly = true;
            try {
                ClearAll();
                Document = Document.Empty;
                var loader = CreateLoader(LoadingBufferSize);
                if (loader == null) throw new ApplicationException("Unable to create loader.");
                var cts = new CancellationTokenSource();
                if (encoding != null) Encoding = encoding;
                else if (Encoding == null) Encoding = Encoding.UTF8;
                var document = await LoadDocumentAsync(loader, Path = path, cts.Token, Encoding, detectBOM);
                Document = document;
                ReleaseDocument(document);
                if (ShowLineNumbers) LineNumbersShow();
                if (Lexer == Lexer.Container && ContainerLexer != null) {
                    if (ColorScheme != null) ColorScheme.ResetSyntax();
                    if (StyleScheme != null) StyleScheme.ResetSyntax();
                    ApplyContainerLexer();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception x) {
                MessageBox.Show(this, x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                Enabled = _enabled;
                ReadOnly = _readonly;
            }
        }

        /// <summary>
        /// Loads a file with specified or default encoding
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        public void LoadFile(string path, Encoding encoding = null) {
            ClearAll();
            Document = Document.Empty;
            if (encoding != null) Encoding = encoding;
            else if (Encoding == null) Encoding = Encoding.UTF8;
            Text = File.ReadAllText(Path = path, Encoding);

            EmptyUndoBuffer();
            if (ShowLineNumbers) LineNumbersShow();
            if (Lexer == Lexer.Container && ContainerLexer != null) {
                if (ColorScheme != null) ColorScheme.ResetSyntax();
                if (StyleScheme != null) StyleScheme.ResetSyntax();
                ApplyContainerLexer();
            }
        }

        /// <summary>
        /// Saves a file with specified or default encoding
        /// </summary>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        public void SaveFile(string path = null, Encoding encoding = null) {
            if (path == null) path = Path;
            if (encoding != null) Encoding = encoding;
            else if (Encoding == null) Encoding = Encoding.UTF8;
            File.WriteAllText(path, Text, Encoding);
        }

        /// <summary>
        /// Handles scheme selection from menu.
        /// </summary>
        /// <param name="sender"><see cref="ToolStripMenuItem"/>.</param>
        /// <param name="e">Ignored.</param>
        private void OnSelectScheme(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            var scheme = (Presets)Enum.Parse(typeof(Presets), item.Text);
            Preset = scheme;
        }

        /// <summary>
        /// Scrolls editor view to show selected line and maximum context around it.
        /// </summary>
        /// <param name="lineNumber">1-based line index.</param>
        public void GoToLine(int lineNumber) {
            var lastPosition = CurrentPosition;
            var lineCount = Lines.Count;
            if (lineNumber < 0) lineNumber = 0;
            if (lineNumber > lineCount) lineNumber = lineCount;
            var headroom = (LinesOnScreen - 1) / 2;
            var position = Lines[lineNumber - 1].Position;
            var firstLine = lineNumber - headroom;
            var lastLine = lineNumber + headroom;
            if (firstLine < 0) firstLine = 0;
            if (lastLine >= lineCount) lastLine = lineCount - 1;
            GotoPosition(position);
            ScrollRange(Lines[firstLine].Position, Lines[lastLine].Position);
            var visibleRange = GetVisibleRange();
            var offset = firstLine - LineFromPosition(visibleRange.First) - 2;
            LineScroll(offset, 0);
        }

        int Find_LastIndex = -1;
        int Find_LastLength = 0;
        bool Find_NowReplace = false;
        Regex Find_Regex = null;

        #endregion Public methods

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
                }
                else {
                    start = Lines[FirstVisibleLine].Position;
                    end = Lines[FirstVisibleLine + LinesOnScreen - 1].EndPosition;
                }
                _ContainerLexer.ApplyStyles(start, end);
            }
        }

        /// <summary>
        /// Performs automatic indentation of line inserted based on current line indentation
        /// Increases indentation if the trimmed line ends with opening brace or colon
        /// </summary>
        /// <param name="e"></param>
        private void AutoIndentInsertCheck(InsertCheckEventArgs e) {
            var inserted = e.Text;
            if (inserted[0] != '\r' && inserted[0] != '\n') return;
            var line = Lines[CurrentLine].Text;
            var lineIndent = "";
            var lineTrimmed = line.TrimEnd();
            var lineTrimmedLength = lineTrimmed.Length;
            var lineEnd = lineTrimmedLength > 0 ? lineTrimmed[lineTrimmedLength - 1] : '*';
            char c;
            for (int i = 0; i < line.Length; i++) {
                c = line[i];
                if (c == ' ') lineIndent += c;
                else if (c == '\t') lineIndent += IndentationUnit;
                else break;
            }
            if (Lexer == Lexer.Cpp || CurrentLine > 0) { // switch case
                lineTrimmed = lineTrimmed.Trim().TrimEnd(';');
                var unitLength = IndentationUnit.Length;
                if (lineTrimmed == "break" && lineIndent.Length > unitLength) {
                    lineIndent = lineIndent.Substring(unitLength);
                }
            }
            e.Text += lineIndent;
            if (lineEnd == '{' || lineEnd == '[' || lineEnd == '(' || lineEnd == ':') e.Text += IndentationUnit;
        }

        /// <summary>
        /// Performs automatic decrase of indentation if brace is closed as the only non-whitespace character in line
        /// </summary>
        /// <param name="e"></param>
        private void AutoIndentCharAdded(CharAddedEventArgs e) {
            var c = e.Char;
            if (c == '}' || c == ']' || c == ')') {
                var line = Lines[CurrentLine];
                var lineNonWhitespace = line.Text.Trim();
                if (lineNonWhitespace.Length == 1) line.Indentation -= TabWidth;
            }
        }

        /// <summary>
        /// Shows specified tool.
        /// </summary>
        /// <param name="tool">Editor tool strip.</param>
        /// <param name="context">Optional tool context.</param>
        /// <returns>Always true.</returns>
        private bool ShowTool(EditorToolStrip tool, string context = null) {
            foreach (Control ctl in Controls) if (ctl == tool) return true;
            SuspendLayout();
            tool.OnBeforeShow(context);
            Controls.Add(tool);
            ResumeLayout();
            Task.Delay(16).ContinueWith(_ => Invoke(new MethodInvoker(() => tool.Focus())));
            return true;
        }

        /// <summary>
        /// Scrolls editor view to show selected line and maximum context around it.
        /// </summary>
        /// <param name="e">Event arguments containing line number.</param>
        private void OnGoToLine(GoToLineEventArgs e) => GoToLine(e.LineNumber);

        /// <summary>
        /// Finds specified text.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        private void OnFind(FindEventArgs e) {
            Find_GetRegex(e);
            Find_LastIndex = (e.Direction != FindDirection.Previous)
                ? this.CurrentPosition + Find_LastLength
                : this.CurrentPosition - Find_LastLength;
            var index = 0;
            var startIndex = 0;
            var endIndex = Text.Length - 1;
            if (Find_Regex != null) {
                try {
                    if (e.Direction == FindDirection.Previous) {
                        Find_Regex = new Regex(Find_Regex.ToString(), Find_Regex.Options | RegexOptions.RightToLeft);
                        endIndex = Find_LastIndex >= 0 ? Find_LastIndex : CurrentPosition;
                    } else {
                        if (Find_LastIndex >= 0) startIndex = Find_LastIndex + Find_LastLength;
                        Find_Regex = new Regex(Find_Regex.ToString(), Find_Regex.Options & ~RegexOptions.RightToLeft);
                    }
                    var match = Find_Regex.Match(Text, startIndex, endIndex - startIndex + 1);
                    if (match != null && match.Success) OnFound(match.Index, match.Length);
                    else OnNotFound();
                }
                catch (ArgumentException) {
                    OnInvalidExpression();
                }
            }
            else {
                var stringComparison = e.MatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
                if (e.Direction == FindDirection.Previous) {
                    endIndex = Find_LastIndex >= 0 ? Find_LastIndex : CurrentPosition;
                    index = Text.LastIndexOf(e.Search, endIndex, endIndex - startIndex + 1, stringComparison);
                }
                else {
                    if (Find_LastIndex >= 0) startIndex = Find_LastIndex + Find_LastLength;
                    index = Text.IndexOf(e.Search, startIndex, endIndex - startIndex + 1, stringComparison);
                }
                if (index >= 0) OnFound(index, e.Search.Length);
                else OnNotFound();
            }
        }

        /// <summary>
        /// Replaces term found.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        private void OnReplace(FindEventArgs e) {
            ;
            // TODO: Add replace / replace all code.
        }

        private void OnFound(int start, int length) {
            GoToLine(LineFromPosition(start));
            SelectionStart = Find_LastIndex = start;
            SelectionEnd = SelectionStart + (Find_LastLength = length);
        }

        private void OnNotFound() {
            Find_LastIndex = -1;
            Find_LastLength = 0;
            MessageBox.Show("Not found.");
        }

        private void OnInvalidExpression() {
            Find_LastIndex = -1;
            Find_LastLength = 0;
            MessageBox.Show("Invalid expression.", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Gets optional regular expression from <see cref="FindEventArgs"/>.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>True if no regular expression used or valid regular expression built. False on invalid regular expression given.</returns>
        private bool Find_GetRegex(FindEventArgs e) {
            if (e.UseRegularExpressions) {
                var regexOptions = RegexOptions.Compiled;
                if (!e.MatchCase) regexOptions |= RegexOptions.IgnoreCase;
                try {
                    Find_Regex = new Regex(e.Search, regexOptions);
                    return true;
                }
                catch {
                    Find_Regex = null;
                    OnInvalidExpression();
                    return false;
                }
            }
            Find_Regex = null;
            return true;
        }

        /// <summary>
        /// Searches for the next match using regular expression.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>True if match found. Else otherwise.</returns>
        private bool Find_SearchRegexForward(FindEventArgs e) {
            var startIndex = 0;
            var endIndex = Text.Length - 1;
            if (Find_LastIndex >= 0) startIndex = Find_LastIndex + Find_LastLength;
            var match = Find_Regex.Match(Text, startIndex, endIndex - startIndex + 1);
            if (match != null && match.Success) {
                OnFound(match.Index, match.Length);
                return true;
            }
            else {
                OnNotFound();
                return false;
            }
        }


        #endregion Private methods

        #region Overrides

        /// <summary>
        /// Processes shortcuts for internal ToolStrips
        /// </summary>
        /// <param name="msg">Windows message.</param>
        /// <param name="keyData">Key presses detected.</param>
        /// <returns>True if the shortcut was handled.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == GoToLineTool.GoToLineShortcut) return ShowTool(GoToLineTool);
            if (keyData == FindTool.FindShortcut) return ShowTool(FindTool);
            if (keyData == FindTool.FindPreviousShortcut) { FindTool.GoFindPrevious(); return true; }
            if (keyData == FindTool.FindNextShortcut) { FindTool.GoFindNext(); return true; }
            if (keyData == FindTool.ReplaceShortcut) return ShowTool(FindTool, "replace");
            if (keyData == FindTool.ReplaceNextShortcut) { FindTool.GoReplaceNext(); return true; }
            if (keyData == FindTool.ReplaceAllShortcut) { FindTool.GoReplaceAll(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Triggered when a character is added to current document
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCharAdded(CharAddedEventArgs e) {
            base.OnCharAdded(e);
            if (AutoIndent) AutoIndentCharAdded(e);
        }

        /// <summary>
        /// Triggered when some text (like line ending) is added to current document
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInsertCheck(InsertCheckEventArgs e) {
            base.OnInsertCheck(e);
            if (AutoIndent) AutoIndentInsertCheck(e);
        }

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
            bool isDebug = false;
            System.Diagnostics.Debug.Assert(isDebug = true);
            if (DwellOnIdentifier != null || isDebug) {
                var index = CharPositionFromPointClose(e.X, e.Y);
                if (index >= 0) {
                    //if (isDebug) System.Diagnostics.Debug.Print("Lexer style @{0}: {1}", index, GetStyleAt(index));
                    if (DwellOnIdentifier == null) return;
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
                    }
                    else CallTipCancel();
                }
                else CallTipCancel();
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

        #region Tools instances

        internal readonly GoToLineTool GoToLineTool;
        internal readonly FindTool FindTool;

        #endregion

    }

    #region Enumerations

    /// <summary>
    /// ContainerLexer modes.
    /// </summary>
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

    /// <summary>
    /// Styles of folding symbols sets
    /// </summary>
    public enum FoldingStyles {
        None, SquareTrees, CurvyTrees, PlusMinus, Arrows
    }

    /// <summary>
    /// Language specific keywords sets
    /// </summary>
    public enum KeywordSets {
        None, ECMAScript, TypeScript
    }

    /// <summary>
    /// Available built in editor color and style presets
    /// </summary>
    public enum Presets {
        Google, VSLight, VSDark, Trax, Oblivion, Zenburn
    }

    #endregion

}