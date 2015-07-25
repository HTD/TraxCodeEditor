using System;
using System.Drawing;
using System.Windows.Forms;
using Trax;
using ScintillaNET;
using System.Text;
using System.IO;

namespace Test.WoofEditor {

    public partial class MainForm : Form {

        string SamplesDir = @"..\..\Samples";
        //MenuStrip Menu;


        public MainForm() {
            InitializeComponent();
            Ed.ContainerLexer = new ScnLexer(Ed);
            Ed.SetColorScheme(ColorSchemes.DarkDefault.Default);
            //Ed.SetColorScheme(ColorSchemes.DarkDefault.Default);
            ListSamples();
        }

        private void ListSamples() {
            var samples = new ToolStripMenuItem("&Samples");
            MainMenuStrip = new MenuStrip();
            MainMenuStrip.Items.Add(samples);
            foreach (string filename in Directory.EnumerateFiles(SamplesDir))
                samples.DropDownItems.Add(
                    new ToolStripMenuItem(
                        filename.Replace(SamplesDir + "\\", ""),
                        null,
                        SelectSample
                    )
                );
            Controls.Add(MainMenuStrip);
        }

        private void SelectSample(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            var filename = item.Text;
            if (filename.EndsWith(".js")) JavaScriptTest(filename);
            else ScnTest(filename);
        }

        private async void ScnTest(string filename) {
            await Ed.LoadFile(String.Format("{0}\\{1}", SamplesDir, filename), Encoding.UTF8);
            //Ed.Document = Document.Empty;
            //Ed.DirectMessage(2037, new IntPtr(65001), IntPtr.Zero);
            //Ed.Text = System.IO.File.ReadAllText(String.Format("{0}\\{1}", SamplesDir, filename), Encoding.Default);
            Ed.Lexer = Lexer.Container;
            Ed.ColorScheme.ResetSyntax();
            Ed.ContainerLexerMode = ContainerLexerModes.Visible;
        }

        private async void JavaScriptTest(string filename) {
            await Ed.LoadFile(String.Format("{0}\\{1}", SamplesDir, filename));
            Ed.Lexer = Lexer.Cpp;
            Ed.SetKeywords(0, "var function return typeof for in if else do while switch case break continue default with");
            Ed.SetKeywords(1, "this self");
            Ed.ColorScheme.ResetSyntax();
            //Ed.DwellOnIdentifier += Ed_DwellOnIdentifier;
            Ed.IndentationGuides = IndentView.LookBoth;
            Ed.IndentWidth = 4;
            Ed.VirtualSpaceOptions = VirtualSpace.RectangularSelection;
            // Enable folding
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

        private void MainForm_Load(object sender, EventArgs e) {
        }

        private void Ed_DwellOnIdentifier(object sender, DwellOnIdentifierEventArgs e) {
            e.CallTipText = String.Format(
                "Identifier: {0}\r\nIndex: {1}\r\nLength: {2}",
                e.Identifier, e.IdentifierRange.First, e.IdentifierRange.Length
            );
            var s = sender as CodeEditor;
       }
    }

}
