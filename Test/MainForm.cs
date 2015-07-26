using ScintillaNET;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Trax {

    public partial class MainForm : Form {

        string SamplesDir = @"..\..\Samples";
        int Mode = 0; // Text
        Encoding ScnEncoding = Encoding.GetEncoding(1250);

        /// <summary>
        /// Initializes the editor and loads a color scheme
        /// </summary>
        public MainForm() {
            InitializeComponent();
            Ed.ContainerLexer = new ScnLexer(Ed);
            Ed.SetColorScheme(ColorSchemes.DarkDefault.Default);
            Ed.Lexer = Lexer.Null;
            ListSamples();
        }

        /// <summary>
        /// Creates test menu
        /// </summary>
        private void ListSamples() {
            var samples = new ToolStripMenuItem("&Samples");
            var mode = new ToolStripMenuItem("&Mode");
            MainMenuStrip = new MenuStrip();
            MainMenuStrip.Items.Add(samples);
            MainMenuStrip.Items.Add(mode);
            foreach (string filename in Directory.EnumerateFiles(SamplesDir))
                samples.DropDownItems.Add(
                    new ToolStripMenuItem(
                        filename.Replace(SamplesDir + "\\", ""),
                        null,
                        SelectSample
                    )
                );
            mode.DropDownItems.Add(new ToolStripMenuItem("Text", null, SwitchMode) { Tag = 0, Checked = true });
            mode.DropDownItems.Add(new ToolStripMenuItem("Loader", null, SwitchMode) { Tag = 1 });
            Controls.Add(MainMenuStrip);
        }

        /// <summary>
        /// Handles mode switching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchMode(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            Mode = (int)item.Tag;
            var toolStrip = item.GetCurrentParent();
            foreach (ToolStripMenuItem i in toolStrip.Items) i.Checked = !i.Checked;
        }

        /// <summary>
        /// Selects a sample based on file extension
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectSample(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            var filename = item.Text;
            if (filename.EndsWith(".js")) JavaScriptTest(filename);
            else ScnTest(filename);
        }

        /// <summary>
        /// Loads a file as custom format
        /// </summary>
        /// <param name="filename"></param>
        private async void ScnTest(string filename) {
            Ed.Lexer = Lexer.Null;
            if (Mode == 0) Ed.Text = File.ReadAllText(String.Format("{0}\\{1}", SamplesDir, filename), ScnEncoding);
            else await Ed.LoadFile(String.Format("{0}\\{1}", SamplesDir, filename), ScnEncoding);
            Ed.ContainerLexerMode = ContainerLexerModes.Visible;
            Ed.Lexer = Lexer.Container;
        }

        /// <summary>
        /// Loads a file as JavaScript
        /// </summary>
        /// <param name="filename"></param>
        private async void JavaScriptTest(string filename) {
            if (Mode == 0) Ed.Text = File.ReadAllText(String.Format("{0}\\{1}", SamplesDir, filename));
            else await Ed.LoadFile(String.Format("{0}\\{1}", SamplesDir, filename));
            Ed.Lexer = Lexer.Cpp;
            Ed.SetKeywords(0, "var function return typeof for in if else do while switch case break continue default with");
            Ed.SetKeywords(1, "this");
            Ed.ColorScheme.ResetSyntax();
            // Folding / indentation tests, irrelevant for now
            Ed.IndentationGuides = IndentView.LookBoth;
            Ed.IndentWidth = 4;
            Ed.VirtualSpaceOptions = VirtualSpace.RectangularSelection;
            Ed.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
            Ed.SetProperty("fold", "1");
            Ed.SetProperty("fold.compact", "1");
            Ed.Margins[2].Type = MarginType.Symbol;
            Ed.Margins[2].Mask = Marker.MaskFolders;
            Ed.Margins[2].Sensitive = true;
            Ed.Margins[2].Width = 13;
            Ed.SetFoldMarginColor(true, Ed.BackColor);
            Ed.SetFoldMarginHighlightColor(true, Ed.BackColor);
            Ed.FoldingLineColor = ColorTranslator.FromHtml("#444444");
            Ed.FoldingFillColor = Ed.BackColor;
            Ed.FoldingStyle = FoldingStyles.SquareTrees;
        }

        /// <summary>
        /// Quick test of new DwellOnIdentifier event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ed_DwellOnIdentifier(object sender, DwellOnIdentifierEventArgs e) {
            e.CallTipText = String.Format(
                "Identifier: {0}\r\nIndex: {1}\r\nLength: {2}",
                e.Identifier, e.IdentifierRange.First, e.IdentifierRange.Length
            );
            var s = sender as CodeEditor;
       }
    }

}
