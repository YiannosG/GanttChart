using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GanttChart
{
    public partial class Form1 : Form
    {

        TextBox txtLog;
        GanttChart ganttChart1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SaveImageToolStripMenuItem.Click += new EventHandler(SaveImageToolStripMenuItem_Click);

            txtLog = new TextBox();
            txtLog.Dock = DockStyle.Fill;
            txtLog.Multiline = true;
            txtLog.Enabled = false;
            txtLog.ScrollBars = ScrollBars.Horizontal;
            tableLayoutPanel1.Controls.Add(txtLog, 0, 3);

            //first Gantt Chart
            ganttChart1 = new GanttChart();
            ganttChart1.AllowChange = false;
            ganttChart1.Dock = DockStyle.Fill;
            ganttChart1.FromDate = new DateTime(2015, 12, 12, 0, 0, 0);
            ganttChart1.ToDate = new DateTime(2015, 12, 24, 0, 0, 0);
            tableLayoutPanel1.Controls.Add(ganttChart1, 0, 1);

            ganttChart1.MouseMove += new MouseEventHandler(ganttChart1.GanttChart_MouseMove);
            ganttChart1.MouseMove += new MouseEventHandler(GanttChart1_MouseMove);
            ganttChart1.MouseDragged += new MouseEventHandler(ganttChart1.GanttChart_MouseDragged);
            ganttChart1.MouseLeave += new EventHandler(ganttChart1.GanttChart_MouseLeave);
            ganttChart1.ContextMenuStrip = ContextMenuGanttChart1;

            List<BarInformation> lst1 = new List<BarInformation>();

            lst1.Add(new BarInformation("Row 1", new DateTime(2015, 12, 12), new DateTime(2015, 12, 16), Color.Aqua, Color.Khaki, 0));
            lst1.Add(new BarInformation("Row 2", new DateTime(2015, 12, 13), new DateTime(2015, 12, 20), Color.AliceBlue, Color.Khaki, 1));
            lst1.Add(new BarInformation("Row 3", new DateTime(2015, 12, 14), new DateTime(2015, 12, 24), Color.Violet, Color.Khaki, 2));
            lst1.Add(new BarInformation("Row 2", new DateTime(2015, 12, 21), new DateTime(2015, 12, 22, 12, 0, 0), Color.Yellow, Color.Khaki, 1));
            lst1.Add(new BarInformation("Row 1", new DateTime(2015, 12, 17), new DateTime(2015, 12, 24), Color.LawnGreen, Color.Khaki, 0));

            foreach (BarInformation bar in lst1)
            {
                ganttChart1.AddChartBar(bar.RowText, bar, bar.FromTime, bar.ToTime, bar.Color, bar.HoverColor, bar.Index);
            }
           

        }

        private void GanttChart1_MouseMove(Object sender, MouseEventArgs e)
        {
            List<string> toolTipText = new List<string>();

            if (ganttChart1.MouseOverRowText.Length > 0)
            {
                BarInformation val = (BarInformation)ganttChart1.MouseOverRowValue;
                toolTipText.Add("[b]Date:");
                toolTipText.Add("From ");
                toolTipText.Add(val.FromTime.ToLongDateString() + " - " + val.FromTime.ToString("HH:mm"));
                toolTipText.Add("To ");
                toolTipText.Add(val.ToTime.ToLongDateString() + " - " + val.ToTime.ToString("HH:mm"));
            }
            else
            {
                toolTipText.Add("");
            }

            ganttChart1.ToolTipTextTitle = ganttChart1.MouseOverRowText;
            ganttChart1.ToolTipText = toolTipText;

        }


        
        private void SaveImageToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            Control chart = null;
            if (menuItem != null)
            {
                ContextMenuStrip calendarMenu = menuItem.Owner as ContextMenuStrip;

                if (calendarMenu != null)
                {
                    chart = calendarMenu.SourceControl;
                }
            }

            SaveImage(chart);
        }

        private void SaveImage(Control control)
        {
            GanttChart gantt = control as GanttChart;
            string filePath = Interaction.InputBox("Where to save the file?", "Save image", "C:\\Temp\\GanttChartTester.jpg");
            if (filePath.Length == 0)
                return;
            gantt.SaveImage(filePath);
            Interaction.MsgBox("Picture saved", MsgBoxStyle.Information);
        }

    }
}
