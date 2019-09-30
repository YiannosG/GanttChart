using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Microsoft.VisualBasic;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;



namespace GanttChart
{
    public class GanttChart : UserControl
    {

        private enum MouseOverPart
        {
            Empty,
            Bar,
            BarLeftSide,
            BarRightSide
        }

        #region proprties
        private List<string> _toolTipText = new List<string>();
        private string _toolTipTextTitle = "";
        private Point MyPoint = new Point(0, 0);
        private bool _allowEditBarWithMouse = false;
        private bool _allowManualEditBar = false;
        private bool _allowChange = false;
        private DateTime headerFromDate;
        private DateTime headerToDate;
        private string _mouseOverRowText = "";
        private DateTime _mouseOverColumnValue;
        private object _mouseOverRowValue = null;
        private Pen lineColor = Pens.Bisque;
        private Font rowTextFont = new Font("Arial", 8, FontStyle.Regular);
        private Font dateTextFont = new Font("Arial", 8, FontStyle.Regular);
        private Font timeTextFont = new Font("Arial", 8, FontStyle.Regular);

        public List<string> ToolTipText
        {
            get { return _toolTipText; }
            set
            {
                _toolTipText = value;
                Point LocalMousePositon = PointToClient(Cursor.Position);
                if (LocalMousePositon == MyPoint)
                    return;
                MyPoint = LocalMousePositon;
                ToolTip.SetToolTip(this, ".");

            }
        }

        public string ToolTipTextTitle
        {
            get { return _toolTipTextTitle; }
            set { _toolTipTextTitle = value; }
        }

        public bool AllowEditBarWithMouse
        {
            get { return _allowEditBarWithMouse; }
            set { _allowEditBarWithMouse = value; }
        }

        public bool AllowManualEditBar
        {
            get { return _allowManualEditBar; }
            set { _allowManualEditBar = value; }
        }

        public bool AllowChange
        {
            get { return _allowChange; }
            set { _allowChange = value; }
        }

        public Font TimeFont
        {
            get { return timeTextFont; }
            set { timeTextFont = value; }
        }

        public DateTime FromDate
        {
            get { return headerFromDate; }
            set { headerFromDate = value; }
        }

        public DateTime ToDate
        {
            get { return headerToDate; }
            set { headerToDate = value; }
        }

        public string MouseOverRowText
        {
            get { return _mouseOverRowText; }
            set { _mouseOverRowText = value; }
        }

        public Object MouseOverRowValue
        {
            get { return _mouseOverRowValue; }
            set { _mouseOverRowValue = value; }
        }

        public DateTime MouseOverColumnDate
        {
            get { return _mouseOverColumnValue; }
            set { _mouseOverColumnValue = value; }
        }

        public Pen GridColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        public Font RowFont
        {
            get { return rowTextFont; }
            set { rowTextFont = value; }
        }

        public Font DateFont
        {
            get { return dateTextFont; }
            set { dateTextFont = value; }
        }


        private int ScrollPositionY
        {
            get
            {
                if (scroll == null)
                    return -1;
                return ((scroll.Height / 2) + scroll.Location.Y) + 19;
            }
            set
            {
                int barCount = GetIndexChartBar("QQQWWW");
                int maxHeight = Height - 30;
                decimal scrollHeight = (maxHeight / barCount) * barsViewable;

                decimal divideBy = barCount - barsViewable;
                if (divideBy == 0) divideBy = 1;

                decimal scrollSpeed = (maxHeight - scrollHeight) / divideBy;
                int index = 0;
                dynamic distanceFromLastPosition = 9999;

                // Tests to see what scrollposition is the closest to the set position

                while (index < barCount)
                {
                    int newPositionTemp = (index * (int)scrollSpeed) + ((int)scrollHeight / 2) + (30 / 2);
                    dynamic distanceFromCurrentPosition = newPositionTemp - value;

                    if (distanceFromLastPosition < 0)
                    {
                        if (distanceFromCurrentPosition < distanceFromLastPosition)
                        {
                            scrollPosition = index - 1;
                            PaintChart();
                            return;
                        }
                    }
                    else
                    {
                        if (distanceFromCurrentPosition > distanceFromLastPosition)
                        {
                            scrollPosition = index - 1;

                            // A precaution to make sure the scroll bar doesn't go too far down

                            if (scrollPosition + barsViewable > GetIndexChartBar("QQQWWW"))
                            {
                                scrollPosition = GetIndexChartBar("QQQWWW") - barsViewable;
                            }

                            PaintChart();
                            return;
                        }
                    }

                    distanceFromLastPosition = distanceFromCurrentPosition;

                    index += 1;
                }
            }
        }


        #endregion

        MouseOverPart mouseHoverPart = MouseOverPart.Empty;
        int mouseHoverBarIndex = -1;
        int lastLineStop = 0;

        List<ChartBarDate> bars = new List<ChartBarDate>();

        int barIsChanging = -1;
        int barStartRight = 20;
        int barStartLeft = 100;
        int headerTimeStartTop = 30;
        List<Header> shownHeaderList;

        int barStartTop = 50;
        int barHeight = 9;
        int barSpace = 5;
        int widthPerItem;

        int barsViewable = -1;
        int scrollPosition = 0;
        Rectangle topPart;
        Rectangle BottomPart;
        Rectangle scroll;
        Rectangle scrollBarArea;

        bool mouseOverTopPart = false;
        bool mouseOverBottomPart = false;
        bool mouseOverScrollBar = false;
        bool mouseOverScrollBarArea = false;

        internal ToolTip ToolTip = new ToolTip();
        private int x = 1;
        Bitmap objBmp;
        Graphics objGraphics;

        public event MouseEventHandler MouseDragged;
        public event EventHandler BarChanged;

        public GanttChart()
        {
            ToolTip.AutoPopDelay = 15000;
            ToolTip.InitialDelay = 250;
            ToolTip.OwnerDraw = true;

            objBmp = new Bitmap(1280, 1024);
            objGraphics = Graphics.FromImage(objBmp);

            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            ToolTip.Draw += new DrawToolTipEventHandler(ToolTipText_Draw);
            ToolTip.Popup += new PopupEventHandler(ToolTipText_Popup);
        }

        #region Resize

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            scrollPosition = 0;

            if (lastLineStop > 0)
            {
                objBmp = new Bitmap(Width - barStartRight, lastLineStop);
                objGraphics = Graphics.FromImage(objBmp);
            }

            PaintChart();
        }

        #endregion


        #region Bars

        private void SetBarStartLeft(string rowText)
        {
            Graphics gfx = CreateGraphics();

            int length = (int)gfx.MeasureString(rowText, rowTextFont, 500).Width;

            if (length > barStartLeft)
            {
                barStartLeft = length;
            }
        }

        //Adds a bar to the list
        //rowText = Text for the row
        //barValue = Value for the row
        //fromTime = The date/time the bar starts
        //toTime = The date/time the bar ends
        //color = The color of the bar
        //hoverColor = The hover color of the bar</param>
        //rowIndex = The rowindex of the bar (useful if you want several bars on the same row)

        public void AddChartBar(string rowText, object barValue, DateTime fromTime, DateTime toTime, Color color, Color hoverColor, int rowIndex)
        {
            ChartBarDate bar = new ChartBarDate();
            bar.Text = rowText;
            bar.Value = barValue;
            bar.StartValue = fromTime;
            bar.EndValue = toTime;
            bar.Color = color;
            bar.HoverColor = hoverColor;
            bar.RowIndex = rowIndex;
            bars.Add(bar);

            SetBarStartLeft(rowText);
        }

        public void AddChartBar(string rowText, object barValue, DateTime fromTime, DateTime toTime, Color color, Color hoverColor, int rowIndex, bool hideFromMouseMove)
        {
            ChartBarDate bar = new ChartBarDate();
            bar.Text = rowText;
            bar.Value = barValue;
            bar.StartValue = fromTime;
            bar.EndValue = toTime;
            bar.Color = color;
            bar.HoverColor = hoverColor;
            bar.RowIndex = rowIndex;
            bar.HideFromMouseMove = hideFromMouseMove;
            bars.Add(bar);

            SetBarStartLeft(rowText);
        }

        public int GetIndexChartBar(string rowText)
        {
            int index = -1;

            foreach (ChartBarDate bar in bars)
            {
                if (bar.Text.Equals(rowText) == true)
                {
                    return bar.RowIndex;
                }
                if (bar.RowIndex > index)
                {
                    index = bar.RowIndex;
                }
            }

            return index + 1;
        }

        //public void RemoveBars()
        //{
        //    bars = new List<ChartBarDate>();

        //    barStartLeft = 100;
        //}

        #endregion

        #region Draw the Gantt Chart

        public void PaintChart()
        {
            Invalidate();
        }

        private void PaintChart(Graphics gfx)
        {
            gfx.Clear(BackColor);

            if (headerFromDate == null | headerToDate == null)
                return;

            DrawScrollBar(gfx);
            DrawHeader(gfx, null);
            DrawNetHorizontal(gfx);
            DrawNetVertical(gfx);
            DrawBars(gfx);

            objBmp = new Bitmap(Width - barStartRight, lastLineStop);
            objGraphics = Graphics.FromImage(objBmp);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            PaintChart(e.Graphics);
        }

        private void DrawHeader(Graphics gfx, List<Header> headerList)
        {
            if (headerList == null)
            {
                headerList = GetFullHeaderList();
            }

            if (headerList.Count == 0)
                return;

            dynamic availableWidth = Width - 10 - barStartLeft - barStartRight;
            widthPerItem = availableWidth / headerList.Count;

            if (widthPerItem < 40)
            {
                List<Header> newHeaderList = new List<Header>();

                bool showNext = true;

                // If there's not enough room for all headers remove 50%

                foreach (Header header in headerList)
                {
                    if (showNext == true)
                    {
                        newHeaderList.Add(header);
                        showNext = false;
                    }
                    else
                    {
                        showNext = true;
                    }
                }

                DrawHeader(gfx, newHeaderList);
                return;
            }

            int index = 0;
            Header lastHeader = null;

            foreach (Header header in headerList)
            {
                int startPos = barStartLeft + (index * widthPerItem);
                bool showDateHeader = false;

                header.StartLocation = startPos;

                // Checks whether to show the date or not

                if (lastHeader == null)
                {
                    showDateHeader = true;
                }
                else if (header.Time.Hour < lastHeader.Time.Hour)
                {
                    showDateHeader = true;
                }
                else if (header.Time.Minute == lastHeader.Time.Minute)
                {
                    showDateHeader = true;
                }

                // Show date

                if (showDateHeader == true)
                {
                    string str = "";

                    if (header.HeaderTextInsteadOfTime.Length > 0)
                    {
                        str = header.HeaderTextInsteadOfTime;
                    }
                    else
                    {
                        str = header.Time.ToString("d-MMM");
                    }
                    gfx.DrawString(str, dateTextFont, Brushes.Black, startPos, 0);
                }

                // Show time

                gfx.DrawString(header.HeaderText, timeTextFont, Brushes.Black, startPos, headerTimeStartTop);
                index += 1;

                lastHeader = header;
            }

            shownHeaderList = headerList;
            widthPerItem = (Width - 10 - barStartLeft - barStartRight) / shownHeaderList.Count;
        }

        private void DrawBars(Graphics grfx, bool ignoreScrollAndMousePosition = false)
        {
            if (shownHeaderList == null)
                return;
            if (shownHeaderList.Count == 0)
                return;

            int index = 0;

            // Finds pixels per minute

            TimeSpan timeBetween = shownHeaderList[1].Time - shownHeaderList[0].Time;
            int minutesBetween = (int)timeBetween.TotalMinutes;
            dynamic widthBetween = (shownHeaderList[1].StartLocation - shownHeaderList[0].StartLocation);
            float perMinute = (float)widthBetween / (float)minutesBetween;

            // Draws each bar

            foreach (ChartBarDate bar in bars)
            {
                index = bar.RowIndex;

                int startLocation = 0;
                int width = 0;
                int startMinutes = 0;
                // Number of minutes from start of the gantt chart
                TimeSpan startTimeSpan = default(TimeSpan);
                int lengthMinutes = 0;
                // Number of minutes from bar start to bar end
                TimeSpan lengthTimeSpan = default(TimeSpan);

                int scrollPos = 0;

                if (ignoreScrollAndMousePosition == false)
                {
                    scrollPos = scrollPosition;
                }

                // Calculates where the bar should be located

                startTimeSpan = bar.StartValue - FromDate;
                startMinutes = (startTimeSpan.Days * 1440) + (startTimeSpan.Hours * 60) + startTimeSpan.Minutes;

                startLocation = (int)(perMinute * startMinutes);

                DateTime endValue = bar.EndValue;

                if (endValue == null)
                {
                    endValue = DateTime.Now;
                }

                lengthTimeSpan = endValue - bar.StartValue;
                lengthMinutes = (lengthTimeSpan.Days * 1440) + (lengthTimeSpan.Hours * 60) + lengthTimeSpan.Minutes;

                width = (int)(perMinute * lengthMinutes);

                int a = barStartLeft + startLocation;
                int b = barStartTop + (barHeight * (index - scrollPos)) + (barSpace * (index - scrollPos)) + 2;
                int c = width;
                int d = barHeight;

                if (c == 0)
                    c = 1;

                // Stops a bar from going into the row-text area

                if (a - barStartLeft < 0)
                {
                    a = barStartLeft;
                }

                Color color = default(Color);

                // If mouse is over bar, set the color to be hovercolor

                if (MouseOverRowText == bar.Text & bar.StartValue <= _mouseOverColumnValue & bar.EndValue >= _mouseOverColumnValue)
                {
                    color = bar.HoverColor;
                }
                else
                {
                    color = bar.Color;
                }

                // Set the location for the graphics

                bar.TopLocation.Left = new Point(a, b);
                bar.TopLocation.Right = new Point(a + c, b);
                bar.BottomLocation.Left = new Point(a, b + d);
                bar.BottomLocation.Right = new Point(a, b + d);

                LinearGradientBrush obBrush = default(LinearGradientBrush);
                Rectangle obRect = new Rectangle(a, b, c, d);

                if (bar.StartValue != null & endValue != null)
                {

                    if ((index >= scrollPos & index < barsViewable + scrollPos) | ignoreScrollAndMousePosition == true)
                    {
                        // Makes the bar gradient

                        obBrush = new LinearGradientBrush(obRect, color, Color.Gray, LinearGradientMode.Vertical);

                        // Draws the bar

                        grfx.DrawRectangle(Pens.Black, obRect);
                        grfx.FillRectangle(obBrush, obRect);

                        // Draws the rowtext

                        grfx.DrawString(bar.Text, rowTextFont, Brushes.Black, 0, barStartTop + (barHeight * (index - scrollPos)) + (barSpace * (index - scrollPos)));

                        obBrush = null;
                        obRect = Rectangle.Empty;
                        obBrush = null;
                    }
                }

                color = Color.Empty;
            }
        }

        public void DrawNetVertical(Graphics grfx)
        {
            if (shownHeaderList == null)
                return;
            if (shownHeaderList.Count == 0)
                return;

            int index = 0;
            //int availableWidth = Width - 10 - barStartLeft - barStartRight;
            Header lastHeader = null;

            foreach (Header header in shownHeaderList)
            {
                int headerLocationY = 0;

                if (lastHeader == null)
                {
                    headerLocationY = 0;
                }
                else if (header.Time.Hour < lastHeader.Time.Hour)
                {
                    headerLocationY = 0;
                }
                else
                {
                    headerLocationY = headerTimeStartTop;
                }

                grfx.DrawLine(Pens.Bisque, barStartLeft + (index * widthPerItem), headerLocationY, barStartLeft + (index * widthPerItem), lastLineStop);
                index += 1;

                lastHeader = header;
            }

            grfx.DrawLine(lineColor, barStartLeft + (index * widthPerItem), headerTimeStartTop, barStartLeft + (index * widthPerItem), lastLineStop);
        }

        public void DrawNetHorizontal(Graphics grfx)
        {
            if (shownHeaderList == null)
                return;
            if (shownHeaderList.Count == 0)
                return;

            int index = 0;
            int width = (widthPerItem * shownHeaderList.Count) + barStartLeft;

            // Last used index. Hopefully nobody will make a row named QQQ :o)
            for (index = 0; index <= GetIndexChartBar("QQQQQQ"); index++)
            {
                foreach (ChartBarDate bar in bars)
                {
                    grfx.DrawLine(lineColor, 0, barStartTop + (barHeight * index) + (barSpace * index), width, barStartTop + (barHeight * index) + (barSpace * index));
                }
            }

            lastLineStop = barStartTop + (barHeight * (index - 1)) + (barSpace * (index - 1));
        }



        #endregion

        #region Header List

        private List<Header> GetFullHeaderList()
        {
            List<Header> result = new List<Header>();
            DateTime newFromTime = new DateTime(FromDate.Year, FromDate.Month, FromDate.Day);
            string item = null;

            TimeSpan interval = ToDate - FromDate;

            if (interval.TotalDays < 1)
            {
                DateTime tmpTime = newFromTime;
                newFromTime = tmpTime.AddHours(FromDate.Hour);

                if (headerFromDate.Minute < 59 & headerFromDate.Minute > 29)
                {
                    tmpTime = newFromTime;
                    newFromTime = tmpTime.AddMinutes(30);
                }
                else
                {
                    tmpTime = newFromTime;
                    newFromTime = tmpTime.AddMinutes(0);
                }

                while (newFromTime <= ToDate)
                {
                    item = newFromTime.Hour + ":";

                    if (newFromTime.Minute < 10)
                    {
                        item += "0" + newFromTime.Minute;
                    }
                    else
                    {
                        item += "" + newFromTime.Minute;
                    }

                    Header header = new Header();

                    header.HeaderText = item;
                    header.HeaderTextInsteadOfTime = "";
                    header.Time = new DateTime(newFromTime.Year, newFromTime.Month, newFromTime.Day, newFromTime.Hour, newFromTime.Minute, 0);
                    result.Add(header);

                    newFromTime = newFromTime.AddMinutes(5);
                    // The minimum interval of time between the headers
                }
            }
            else if (interval.TotalDays < 60)
            {
                while (newFromTime <= ToDate)
                {
                    Header header = new Header();

                    header.HeaderText = "";
                    header.HeaderTextInsteadOfTime = "";
                    header.Time = new System.DateTime(newFromTime.Year, newFromTime.Month, newFromTime.Day, 0, 0, 0);
                    result.Add(header);

                    newFromTime = newFromTime.AddDays(1);
                    // The minimum interval of time between the headers
                }
            }
            else
            {
                while (newFromTime <= ToDate)
                {
                    Header header = new Header();

                    header.HeaderText = "";
                    header.Time = new System.DateTime(newFromTime.Year, newFromTime.Month, newFromTime.Day, 0, 0, 0);
                    header.HeaderTextInsteadOfTime = newFromTime.ToString("MMM");
                    result.Add(header);

                    newFromTime = newFromTime.AddMonths(1);
                    // The minimum interval of time between the headers
                }
            }

            return result;
        }


        #endregion

        #region Mouse Move

        public void GanttChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (shownHeaderList == null)
                return;
            if (shownHeaderList.Count == 0)
                return;

            if (e.Button != System.Windows.Forms.MouseButtons.Left)
            {
                //mouseHoverPart = MouseOverPart.Empty;

                // If bar has changed manually, but left mouse button is no longer pressed the BarChanged event will be raised

                if (AllowManualEditBar == true)
                {
                    if (barIsChanging >= 0)
                    {
                        if (AllowChange)
                        {
                            if (BarChanged != null)
                                BarChanged(bars[barIsChanging].Value, new EventArgs());

                            BarInformation barInfo = bars[barIsChanging].Value as BarInformation;
                            DateTime tempValue = _mouseOverColumnValue;
                            if (mouseHoverPart == MouseOverPart.BarLeftSide)
                            {
                                barInfo.FromTime = tempValue;
                            }
                            else if (mouseHoverPart == MouseOverPart.BarRightSide)
                            {
                                barInfo.ToTime = tempValue;
                            }
                        }
                        barIsChanging = -1;
                    }
                }

                mouseHoverPart = MouseOverPart.Empty;

            }

            mouseHoverBarIndex = -1;

            Point LocalMousePosition = default(Point);

            LocalMousePosition = PointToClient(Cursor.Position);

            // Finds pixels per minute

            TimeSpan timeBetween = shownHeaderList[1].Time - shownHeaderList[0].Time;
            int minutesBetween = (timeBetween.Days * 1440) + (timeBetween.Hours * 60) + timeBetween.Minutes;
            dynamic widthBetween = (shownHeaderList[1].StartLocation - shownHeaderList[0].StartLocation);
            float perMinute = (float)widthBetween / (float)minutesBetween;

            if (perMinute == 0) perMinute = 1;

            // Finds the time at mousepointer

            int minutesAtCursor = 0;

            if (LocalMousePosition.X > barStartLeft)
            {
                minutesAtCursor = (int)((LocalMousePosition.X - barStartLeft) / perMinute);
                _mouseOverColumnValue = FromDate.AddMinutes(minutesAtCursor);
            }
            else
            {
                _mouseOverColumnValue = DateTime.MinValue;
            }

            // Finds the row at mousepointer

            string rowText = "";
            object rowValue = null;

            // Tests to see if the mouse pointer is hovering above the scrollbar

            bool scrollBarStatusChanged = false;

            // Tests to see if the mouse is hovering over the scroll-area bottom-arrow

            if (LocalMousePosition.X > BottomPart.Left & LocalMousePosition.Y < BottomPart.Right & LocalMousePosition.Y < BottomPart.Bottom & LocalMousePosition.Y > BottomPart.Top)
            {
                if (mouseOverBottomPart == false)
                {
                    scrollBarStatusChanged = true;
                }

                mouseOverBottomPart = true;
            }
            else
            {
                if (mouseOverBottomPart == false)
                {
                    scrollBarStatusChanged = true;
                }

                mouseOverBottomPart = false;
            }

            // Tests to see if the mouse is hovering over the scroll-area top-arrow

            if (LocalMousePosition.X > topPart.Left & LocalMousePosition.Y < topPart.Right & LocalMousePosition.Y < topPart.Bottom & LocalMousePosition.Y > topPart.Top)
            {
                if (mouseOverTopPart == false)
                {
                    scrollBarStatusChanged = true;
                }

                mouseOverTopPart = true;
            }
            else
            {
                if (mouseOverTopPart == false)
                {
                    scrollBarStatusChanged = true;
                }

                mouseOverTopPart = false;
            }

            // Tests to see if the mouse is hovering over the scroll

            if (LocalMousePosition.X > scroll.Left & LocalMousePosition.Y < scroll.Right & LocalMousePosition.Y < scroll.Bottom & LocalMousePosition.Y > scroll.Top)
            {
                if (mouseOverScrollBar == false)
                {
                    scrollBarStatusChanged = true;
                }

                mouseOverScrollBar = true;
                mouseOverScrollBarArea = true;
            }
            else
            {
                if (mouseOverScrollBar == false)
                {
                    scrollBarStatusChanged = true;
                }

                mouseOverScrollBar = false;
                mouseOverScrollBarArea = false;
            }

            // If the mouse is not above the scroll, test if it's over the scroll area (no need to test if it's not above the scroll)

            if (mouseOverScrollBarArea == false)
            {
                if (LocalMousePosition.X > scrollBarArea.Left & LocalMousePosition.Y < scrollBarArea.Right & LocalMousePosition.Y < scrollBarArea.Bottom & LocalMousePosition.Y > scrollBarArea.Top)
                {
                    mouseOverScrollBarArea = true;
                }
            }


            // Tests to see if the mouse pointer is hovering above a bar

            int index = 0;


            foreach (ChartBarDate bar in bars)
            {
                // If the bar is set to be hidden from mouse move, the current bar will be ignored

                if (bar.HideFromMouseMove == false)
                {
                    if (bar.EndValue == null)
                    {
                        bar.EndValue = DateTime.Now;
                    }

                    // Mouse pointer needs to be inside the X and Y positions of the bar

                    if (LocalMousePosition.Y > bar.TopLocation.Left.Y & LocalMousePosition.Y < bar.BottomLocation.Left.Y)
                    {

                        if (LocalMousePosition.X > bar.TopLocation.Left.X & LocalMousePosition.X < bar.TopLocation.Right.X)
                        {
                            // If the current bar is the one where the mouse is above, the rowText and rowValue needs to be set correctly

                            rowText = bar.Text;
                            rowValue = bar.Value;
                            mouseHoverBarIndex = index;

                            if (mouseHoverPart != MouseOverPart.BarLeftSide & mouseHoverPart != MouseOverPart.BarRightSide)
                            {
                                mouseHoverPart = MouseOverPart.Bar;
                            }
                        }

                        // If mouse pointer is near the edges of the bar it will open up for editing the bar

                        if (AllowManualEditBar == true)
                        {
                            int areaSize = 5;

                            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                            {
                                areaSize = 50;
                            }

                            if (LocalMousePosition.X > bar.TopLocation.Left.X - areaSize & LocalMousePosition.X < bar.TopLocation.Left.X + areaSize & mouseHoverPart != MouseOverPart.BarRightSide)
                            {
                                Cursor = Cursors.VSplit;
                                mouseHoverPart = MouseOverPart.BarLeftSide;
                                mouseHoverBarIndex = index;
                            }
                            else if (LocalMousePosition.X > bar.TopLocation.Right.X - areaSize & LocalMousePosition.X < bar.TopLocation.Right.X + areaSize & mouseHoverPart != MouseOverPart.BarLeftSide)
                            {
                                Cursor = Cursors.VSplit;
                                mouseHoverPart = MouseOverPart.BarRightSide;
                                mouseHoverBarIndex = index;
                            }
                            else
                            {
                                Cursor = Cursors.Default;
                            }
                        }
                    }
                }

                index += 1;
            }

            // Sets the mouseover row value and text

            _mouseOverRowText = rowText;
            _mouseOverRowValue = rowValue;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (MouseDragged != null)
                {
                    MouseDragged(sender, e);
                }

            }
            else
            {
                // A simple test to see if the mousemovement has caused any changes to how it should be displayed 
                // It only redraws if mouse moves from a bar to blank area or from blank area to a bar
                // This increases performance compared to having a redraw every time a mouse moves

                if ((_mouseOverRowValue == null & (rowValue != null)) | ((_mouseOverRowValue != null) & rowValue == null) | scrollBarStatusChanged == true)
                {
                    PaintChart();
                }
            }
        }


        public void GanttChart_MouseLeave(Object sender, EventArgs e)
        {
            _mouseOverRowText = null;
            _mouseOverRowValue = null;
            mouseHoverPart = MouseOverPart.Empty;

            PaintChart();
        }


        public void GanttChart_MouseDragged(object sender, MouseEventArgs e)
        {
            if (mouseOverScrollBarArea == true)
            {
                ScrollPositionY = e.Location.Y;
            }

            if (AllowManualEditBar == true)
            {
                if (mouseHoverBarIndex > -1)
                {
                    if (mouseHoverPart == MouseOverPart.BarLeftSide)
                    {
                        barIsChanging = mouseHoverBarIndex;
                        bars[mouseHoverBarIndex].StartValue = _mouseOverColumnValue;
                        PaintChart();
                    }
                    else if (mouseHoverPart == MouseOverPart.BarRightSide)
                    {
                        barIsChanging = mouseHoverBarIndex;
                        bars[mouseHoverBarIndex].EndValue = _mouseOverColumnValue;
                        PaintChart();
                    }
                }
            }
        }

        #endregion

        #region ToolTipText

        public void ToolTipText_Draw(Object sender, DrawToolTipEventArgs e)
        {
            if (ToolTipText == null)
            {
                ToolTipText = new List<string>();
                return;
            }

            if (ToolTipText.Count == 0)
            {
                return;
            }
            else if (ToolTipText[0].Length == 0)
            {
                return;
            }

            int x = 0;
            int y = 0;

            e.Graphics.FillRectangle(Brushes.AntiqueWhite, e.Bounds);
            e.DrawBorder();

            int titleHeight = 14;
            int fontHeight = 12;

            // Draws the line just below the title

            e.Graphics.DrawLine(Pens.Black, 0, titleHeight, e.Bounds.Width, titleHeight);

            int lines = 1;
            string text = ToolTipTextTitle;

            // Draws the title

            using (Font font = new Font(e.Font, FontStyle.Bold))
            {
                x = (int)(e.Bounds.Width - e.Graphics.MeasureString(text, font).Width) / 2;
                y = (int)(titleHeight - e.Graphics.MeasureString(text, font).Height) / 2;
                e.Graphics.DrawString(text, font, Brushes.Black, x, y);
            }

            // Draws the lines
            List<string> newttList = new List<string>();
            string newStr = "";
            foreach (string str in ToolTipText)
            {
                Font font = new Font(e.Font, FontStyle.Regular);
                newStr = str;

                if (str.Contains("[b]"))
                {
                    font = new Font(font.FontFamily, font.Size, FontStyle.Bold, font.Unit);
                    //str = str.Replace("[b]", "");
                    newStr = str.Replace("[b]", "");
                }

                using (font)
                {
                    x = 5;
                    y = (int)(titleHeight - fontHeight - e.Graphics.MeasureString(str, font).Height) / 2 + 10 + (lines * 14);
                    e.Graphics.DrawString(str, font, Brushes.Black, x, y);
                }

                lines += 1;

                newttList.Add(newStr);

            }
            ToolTipText = newttList;
        }

        public void ToolTipText_Popup(Object sender, PopupEventArgs e)
        {
            if (ToolTipText == null)
            {
                ToolTipText = new List<string>();
            }

            if (ToolTipText.Count == 0)
            {
                e.ToolTipSize = new Size(0, 0);
                return;
            }
            else if (ToolTipText[0].Length == 0)
            {
                e.ToolTipSize = new Size(0, 0);
                return;
            }

            // resizes the ToolTip window

            int height = 18 + (ToolTipText.Count * 15);
            e.ToolTipSize = new Size(200, height);
        }

        #endregion


        #region Scrollbar

        private void DrawScrollBar(Graphics grfx)
        {
            barsViewable = (Height - barStartTop) / (barHeight + barSpace);
            int barCount = GetIndexChartBar("QQQWWW");
            if (barCount == 0)
                return;

            int maxHeight = Height - 30;
            decimal scrollHeight = (maxHeight / barCount) * barsViewable;

            // If the scroll area is filled there's no need to show the scrollbar

            if (scrollHeight >= maxHeight)
                return;

            decimal divideBy = barCount - barsViewable;
            if (divideBy == 0) divideBy = 1;


            decimal scrollSpeed = (maxHeight - scrollHeight) / divideBy;

            scrollBarArea = new Rectangle(Width - 20, 19, 12, maxHeight);
            scroll = new Rectangle(Width - 20, 19 + (int)(scrollPosition * scrollSpeed), 12, (int)scrollHeight);

            if (scroll.Height == 0 || scroll.Width == 0) return;

            topPart = new Rectangle(Width - 20, 10, 12, 8);
            BottomPart = new Rectangle(Width - 20, Height - 10, 12, 8);

            Brush colorTopPart = default(Brush);
            Brush colorBottomPart = default(Brush);
            Brush colorScroll = default(Brush);

            if (mouseOverTopPart == true)
            {
                colorTopPart = Brushes.Black;
            }
            else
            {
                colorTopPart = Brushes.Gray;
            }

            if (mouseOverBottomPart == true)
            {
                colorBottomPart = Brushes.Black;
            }
            else
            {
                colorBottomPart = Brushes.Gray;
            }

            if (mouseOverScrollBar == true)
            {
                colorScroll = new LinearGradientBrush(scroll, Color.Bisque, Color.Gray, LinearGradientMode.Horizontal);
            }
            else
            {
                colorScroll = new LinearGradientBrush(scroll, Color.White, Color.Gray, LinearGradientMode.Horizontal);
            }

            // Draws the top and bottom part of the scrollbar

            grfx.DrawRectangle(Pens.Black, topPart);
            grfx.FillRectangle(Brushes.LightGray, topPart);

            grfx.DrawRectangle(Pens.Black, BottomPart);
            grfx.FillRectangle(Brushes.LightGray, BottomPart);

            // Draws arrows

            PointF[] points = new PointF[3];
            points[0] = new PointF(topPart.Left, topPart.Bottom - 1);
            points[1] = new PointF(topPart.Right, topPart.Bottom - 1);
            points[2] = new PointF((topPart.Left + topPart.Right) / 2, topPart.Top + 1);

            grfx.FillPolygon(colorTopPart, points);

            points[0] = new PointF(BottomPart.Left, BottomPart.Top + 1);
            points[1] = new PointF(BottomPart.Right, BottomPart.Top + 1);
            points[2] = new PointF((BottomPart.Left + BottomPart.Right) / 2, BottomPart.Bottom - 1);

            grfx.FillPolygon(colorBottomPart, points);

            // Draws the scroll area

            grfx.DrawRectangle(Pens.Black, scrollBarArea);
            grfx.FillRectangle(Brushes.DarkGray, scrollBarArea);

            // Draws the actual scrollbar

            grfx.DrawRectangle(Pens.Black, scroll);
            grfx.FillRectangle(colorScroll, scroll);
        }


        #endregion

        #region Save

        public void SaveImage(string filePath)
        {
            objGraphics.SmoothingMode = SmoothingMode.HighSpeed;
            objGraphics.Clear(this.BackColor);

            if (headerFromDate == null | headerToDate == null)
                return;

            DrawHeader(objGraphics, null);
            DrawNetHorizontal(objGraphics);
            DrawNetVertical(objGraphics);
            DrawBars(objGraphics, true);

            objBmp.Save(filePath);
        }

        #endregion



    }


}

