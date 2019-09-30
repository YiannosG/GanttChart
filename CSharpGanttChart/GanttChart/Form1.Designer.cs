namespace GanttChart
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ContextMenuGanttChart1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SaveImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ContextMenuGanttChart1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContextMenuGanttChart1
            // 
            this.ContextMenuGanttChart1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ContextMenuGanttChart1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SaveImageToolStripMenuItem});
            this.ContextMenuGanttChart1.Name = "ContextMenuGanttChart1";
            this.ContextMenuGanttChart1.Size = new System.Drawing.Size(162, 30);
            // 
            // SaveImageToolStripMenuItem
            // 
            this.SaveImageToolStripMenuItem.Name = "SaveImageToolStripMenuItem";
            this.SaveImageToolStripMenuItem.Size = new System.Drawing.Size(161, 26);
            this.SaveImageToolStripMenuItem.Text = "Save image";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1113, 615);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1113, 615);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "C# Gantt Chart";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ContextMenuGanttChart1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ContextMenuStrip ContextMenuGanttChart1;
        internal System.Windows.Forms.ToolStripMenuItem SaveImageToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

