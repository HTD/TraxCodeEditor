
namespace Trax {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.Ed = new Trax.CodeEditor();
            this.SuspendLayout();
            // 
            // Ed
            // 
            this.Ed.AdditionalSelectionTyping = true;
            this.Ed.BackColor = System.Drawing.Color.White;
            this.Ed.CallTipFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Ed.CallTipForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(135)))));
            this.Ed.CaretForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.Ed.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(247)))), ((int)(((byte)(247)))));
            this.Ed.CaretLineVisible = true;
            this.Ed.ContainerLexer = null;
            this.Ed.ContainerLexerMode = Trax.ContainerLexerModes.FullAuto;
            this.Ed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Ed.FoldingFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
            this.Ed.FoldingLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
            this.Ed.FoldingStyle = Trax.FoldingStyles.CurvyTrees;
            this.Ed.FoldMarginColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
            this.Ed.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Ed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(37)))), ((int)(((byte)(40)))));
            this.Ed.GutterBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.Ed.GutterForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(145)))), ((int)(((byte)(175)))));
            this.Ed.IndentationGuides = ScintillaNET.IndentView.Real;
            this.Ed.Keywords = Trax.KeywordSets.None;
            this.Ed.Lexer = ScintillaNET.Lexer.Null;
            this.Ed.Location = new System.Drawing.Point(0, 0);
            this.Ed.MouseDwellTime = 100;
            this.Ed.MouseSelectionRectangularSwitch = true;
            this.Ed.Name = "Ed";
            this.Ed.ShowFoldMargin = false;
            this.Ed.Size = new System.Drawing.Size(870, 604);
            this.Ed.TabIndex = 0;
            this.Ed.VirtualSpaceOptions = ScintillaNET.VirtualSpace.RectangularSelection;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(870, 604);
            this.Controls.Add(this.Ed);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TraxCodeEditor Test";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private Trax.CodeEditor Ed;
    }
}

