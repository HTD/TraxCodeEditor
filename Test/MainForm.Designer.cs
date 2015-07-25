
namespace Test.WoofEditor {
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
            this.Ed.CallTipBackColor = System.Drawing.Color.White;
            this.Ed.CallTipFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Ed.CallTipForeColor = System.Drawing.Color.Black;
            this.Ed.CaretLineVisible = true;
            this.Ed.ContainerLexer = null;
            this.Ed.ContainerLexerMode = Trax.ContainerLexerModes.FullAuto;
            this.Ed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Ed.FoldingBackColor = System.Drawing.Color.Empty;
            this.Ed.FoldingFillColor = System.Drawing.Color.Empty;
            this.Ed.FoldingLineColor = System.Drawing.Color.Empty;
            this.Ed.FoldingStyle = Trax.FoldingStyles.None;
            this.Ed.IndentationGuides = ScintillaNET.IndentView.Real;
            this.Ed.Location = new System.Drawing.Point(0, 0);
            this.Ed.MouseDwellTime = 100;
            this.Ed.MouseSelectionRectangularSwitch = true;
            this.Ed.Name = "Ed";
            this.Ed.Size = new System.Drawing.Size(870, 604);
            this.Ed.TabIndex = 0;
            this.Ed.TabWidth = 8;
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
            this.Text = "Woof Editor Test";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Trax.CodeEditor Ed;
    }
}

