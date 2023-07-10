using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace LaserGRBL {
    public partial class GenerateForm : Form {

        GrblCore mCore = null;

        public GenerateForm(GrblCore core) {
            InitializeComponent();
            mCore = core;
        }
        internal static void CreateAndShowDialog(Form parent, GrblCore core) {
            using (GenerateForm f = new GenerateForm(core)) {
                f.ShowDialog(parent);
            }
        }

        private void Btn_GenerateLines_Click(object sender, EventArgs e) {
            StringBuilder gcodeString = new StringBuilder();

            for (int n = 0; n < (int)numericUpDown__NumLines.Value; n++) {
                gcodeString.AppendLine(";Line" + n.ToString());
                // Fast travel to bottom left corner
                gcodeString.AppendLine(String.Format("G0 X0Y{0}", n * numericUpDown_lineSpacing.Value));
                // Turn on the laser
                gcodeString.AppendLine(String.Format("M3 S{0}", numericUpDown_linePower.Value));
                // Move right at defined speed
                gcodeString.AppendLine(String.Format("G1 X{0}Y{1} F{2}", numericUpDown_lineLength.Value, n * numericUpDown_lineSpacing.Value, numericUpDown_lineSpeed.Value));
                // Turn off the laser
                gcodeString.AppendLine("M5 S0");
            }

            mCore.OpenString(gcodeString.ToString());
        }
 
        private void Btn_GenerateSquares_Click(object sender, EventArgs e) {
            decimal speedIncrement = (numericUpDown_SquaresSpeedMax.Value - numericUpDown_SquaresSpeedMin.Value) / numericUpDown_SquaresSpeedSteps.Value;
            decimal powerIncrement = (numericUpDown_SquaresPowerMax.Value - numericUpDown_SquaresPowerMin.Value) / numericUpDown_SquaresPowerSteps.Value;

            StringBuilder gcodeString = new StringBuilder();

            //float squareWidth = 8, squareHeight = 3;
            //float padding = 1;

            string cutMode = radioButton_CutMode_M4.Checked ? "M4" : "M3";


            float squareWidth = (float)numericUpDown_SquaresWidth.Value;
            float squareHeight = (float)numericUpDown_SquaresHeight.Value;
            float padding = (float)numericUpDown_SquaresSpacing.Value;


            // Draw the squares
            for (int F = 0; F < numericUpDown_SquaresSpeedSteps.Value + 1; F++) {
                for (int S = 0; S < numericUpDown_SquaresPowerSteps.Value + 1; S++) {
                    gcodeString.AppendLine(";Square" + S.ToString());
                    // Fast travel to bottom left corner
                    gcodeString.AppendLine(String.Format("G0 X{0:F}Y{1:F}", S*(squareWidth+padding), F*(squareHeight + padding)));
                    // Turn on the laser
                    gcodeString.AppendLine(String.Format("{0} S{1:F}", cutMode, numericUpDown_SquaresPowerMin.Value + S*powerIncrement));
                    // Move right at defined speed
                    gcodeString.AppendLine(String.Format("G1 X{0:F} F{1:F}", S*(squareWidth+padding) + squareWidth, numericUpDown_SquaresSpeedMin.Value + F*speedIncrement));
                    // Up
                    gcodeString.AppendLine(String.Format("Y{0:F}", F*(squareHeight + padding) + squareHeight));
                    // Left
                    gcodeString.AppendLine(String.Format("X{0:F}", S*(squareWidth + padding)));
                    // Down
                    gcodeString.AppendLine(String.Format("Y{0:F}",  F*(squareHeight + padding)));
                    // Turn off
                    gcodeString.AppendLine("M5 S0");
                }
            }

            // Add the labels
            // X axis labels
            for (int S = 0; S < numericUpDown_SquaresPowerSteps.Value + 1; S++) {
                gcodeString.AppendLine(CreateGcodeFromText(string.Format("{0:0.#}", (decimal)numericUpDown_SquaresPowerMin.Value + S * powerIncrement), new FontFamily("Microsoft Sans Serif"), 0, 6, new PointF(S*(squareWidth+padding) + squareWidth/2, (float)(numericUpDown_SquaresSpeedSteps.Value+1) * (squareHeight+ padding)), ContentAlignment.BottomCenter, 100, 2000, "M4"));
            }
            // X axis title
            gcodeString.AppendLine(CreateGcodeFromText("Power", new FontFamily("Microsoft Sans Serif"), 0, 8, new PointF((float)(numericUpDown_SquaresPowerSteps.Value) * (squareWidth + padding)/2, (float)(numericUpDown_SquaresSpeedSteps.Value + 2) * (squareHeight + padding)), ContentAlignment.BottomCenter, 100, 2000, "M4"));

            // Y axis labels
            for (int F = 0; F < numericUpDown_SquaresSpeedSteps.Value + 1; F++) {
                gcodeString.AppendLine(CreateGcodeFromText(string.Format("{0:0.#}", (decimal)numericUpDown_SquaresSpeedMin.Value + F * speedIncrement), new FontFamily("Microsoft Sans Serif"), 0, 6, new PointF((float)(numericUpDown_SquaresPowerSteps.Value + 1)*(squareWidth + padding), (float)(F * (squareHeight + padding) + squareHeight / 2)), ContentAlignment.MiddleLeft, 100, 2000, "M4"));
            }
            // X axis title
            gcodeString.AppendLine(CreateGcodeFromText("Speed", new FontFamily("Microsoft Sans Serif"), 0, 8, new PointF((float)(numericUpDown_SquaresPowerSteps.Value + 2) * (squareWidth + padding), (float)(numericUpDown_SquaresSpeedSteps.Value + 1) / 2 * (squareHeight + padding)), ContentAlignment.BottomLeft, 100, 2000, "M4"));

            mCore.OpenString(gcodeString.ToString());
        }

        private void Btn_GenerateEngravingSquares_Click(object sender, EventArgs e)
        {
            decimal speedIncrement = (numericUpDown_EngraveSpeedMax.Value - numericUpDown_EngraveSpeedMin.Value) / numericUpDown_EngraveSpeedSteps.Value;
            decimal powerIncrement = (numericUpDown_EngravePowerMax.Value - numericUpDown_EngravePowerMin.Value) / numericUpDown_EngravePowerSteps.Value;

            StringBuilder gcodeString = new StringBuilder();

            // Retrieve form values
            float squareWidth = (float)numericUpDown_EngraveSquareWidth.Value;
            float squareHeight = (float)numericUpDown_EngraveSquareHeight.Value;
            float padding = (float)numericUpDown_EngraveSquareSpacing.Value;
            string engravingMode = radioButton_EngraveMode_M4.Checked ? "M4" : "M3";
            float y_increment = 1.0f / (float)numericUpDown_EngraveLineInterval.Value;

            // Draw the squares
            for (int F = 0; F < numericUpDown_EngraveSpeedSteps.Value + 1; F++)
            {
                for (int S = 0; S < numericUpDown_EngravePowerSteps.Value + 1; S++)
                {
                    gcodeString.AppendLine(";Square" + S.ToString());

                    // Fast travel to bottom left corner
                    gcodeString.AppendLine(String.Format("G0 X{0:F}Y{1:F}", S * (squareWidth + padding), F * (squareHeight + padding)));
                    // Turn on the laser
                    gcodeString.AppendLine(String.Format("{0} S{1:F}", engravingMode, numericUpDown_EngravePowerMin.Value + S * powerIncrement));

                    float dir = 0;

                    // Draw first line across
                    gcodeString.AppendLine(String.Format("G1 X{0} F{1:F}", S * (squareWidth + padding) + squareWidth, numericUpDown_EngraveSpeedMin.Value + F * speedIncrement));

                    for (float h = y_increment; h < squareHeight; h += y_increment)
                    {
                        // Up
                        gcodeString.AppendLine(String.Format("G1 Y{0:F}", F * (squareHeight + padding) + h));
                        // Across
                        gcodeString.AppendLine(String.Format("G1 X{0} F{1:F}", S * (squareWidth + padding) + (dir * squareWidth), numericUpDown_EngraveSpeedMin.Value + F * speedIncrement));
                        // Toggle Direction
                        dir = 1 - dir;
                    }

                    // Turn off
                    gcodeString.AppendLine("M5 S0");
                }
            }

            // Add the labels
            // X axis labels
            for (int S = 0; S < numericUpDown_EngravePowerSteps.Value + 1; S++)
            {
                gcodeString.AppendLine(CreateGcodeFromText(string.Format("{0:0.#}", (decimal)numericUpDown_EngravePowerMin.Value + S * powerIncrement), new FontFamily("Microsoft Sans Serif"), 0, 6, new PointF(S * (squareWidth + padding) + squareWidth / 2, (float)(numericUpDown_EngraveSpeedSteps.Value + 1) * (squareHeight + padding)), ContentAlignment.BottomCenter, 100, 2000, "M4"));
            }
            // X axis title
            gcodeString.AppendLine(CreateGcodeFromText("Power", new FontFamily("Microsoft Sans Serif"), 0, 8, new PointF((float)(numericUpDown_EngravePowerSteps.Value) * (squareWidth + padding) / 2, (float)(numericUpDown_EngraveSpeedSteps.Value + 2) * (squareHeight + padding)), ContentAlignment.MiddleCenter, 100, 2000, "M4"));

            gcodeString.AppendLine(CreateGcodeFromText(string.Format("{0:N} lines/mm", numericUpDown_EngraveLineInterval.Value), new FontFamily("Microsoft Sans Serif"), 0, 8, new PointF(0, 6+(float)(numericUpDown_EngraveSpeedSteps.Value + 1) * (squareHeight + padding)), ContentAlignment.MiddleLeft, 100, 2000));


            // Y axis labels
            for (int F = 0; F < numericUpDown_EngraveSpeedSteps.Value + 1; F++)
            {
                gcodeString.AppendLine(CreateGcodeFromText(string.Format("{0:0.#}", (decimal)numericUpDown_EngraveSpeedMin.Value + F * speedIncrement), new FontFamily("Microsoft Sans Serif"), 0, 6, new PointF((float)(numericUpDown_EngravePowerSteps.Value + 1) * (squareWidth + padding), (float)(F * (squareHeight + padding) + squareHeight / 2)), ContentAlignment.MiddleLeft, 100, 2000, "M4"));
            }
            // Y axis title
            gcodeString.AppendLine(CreateGcodeFromText("Speed", new FontFamily("Microsoft Sans Serif"), 0, 8, new PointF((float)(numericUpDown_EngravePowerSteps.Value + 2) * (squareWidth + padding), (float)(numericUpDown_EngraveSpeedSteps.Value + 1) / 2 * (squareHeight + padding)), ContentAlignment.BottomLeft, 100, 2000, "M4"));

            mCore.OpenString(gcodeString.ToString());
        }



        private void textBox_Font_Click(object sender, EventArgs e) {
            System.Windows.Forms.FontDialog fontSelectionForm = new System.Windows.Forms.FontDialog();
            if (fontSelectionForm.ShowDialog() == DialogResult.OK) {
                string fontName = fontSelectionForm.Font.Name.ToString();
                int fontStyle = 0;
                if (fontSelectionForm.Font.Bold) { fontStyle += 1; }
                if (fontSelectionForm.Font.Italic) { fontStyle += 2; }
                if (fontSelectionForm.Font.Underline) { fontStyle += 4; }
                if (fontSelectionForm.Font.Strikeout) { fontStyle += 8; }
                float fontSize = fontSelectionForm.Font.SizeInPoints;

                textBox_Font.Text = fontName;
                numericUpDown_FontSize.Value = (decimal)fontSize;
            }
        }
        

        private string CreateGcodeFromText(string text, FontFamily fontFamily, int fontStyle, float fontSize, PointF origin, ContentAlignment alignment = ContentAlignment.BottomLeft, int power= 500, int speed = 800, string mode = "M4") {

            if (text == "") { return ""; }

            Graphics g = this.CreateGraphics();
            //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddString(text, fontFamily, fontStyle, fontSize, new Point(0, 0), StringFormat.GenericTypographic);
            graphicsPath.CloseAllFigures();
            g.DrawPath(new Pen(Color.Black, 0), graphicsPath);

            string gcode = CreateGcodeFromGraphicsPath(graphicsPath, origin, alignment, power, speed, mode);

            // Cleanup
            graphicsPath.Dispose();
            g.Dispose();

            return gcode;
        }

        private static string CreateGcodeFromGraphicsPath(GraphicsPath gp, PointF origin, ContentAlignment alignment = ContentAlignment.BottomLeft, int power=500, int speed=800, string mode="M4") {

            PointF[] PathPoints = gp.PathPoints;
            byte[] pathtypes = gp.PathTypes;

            // "Position" is gcode anchor position (in mm)

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0} S{1}", mode, 0));

            // Loop through all the points to find the max/min extents (surely there's got to be a better way to do this?!)
            double xMin = double.MaxValue, xMax = double.MinValue, yMin = double.MaxValue, yMax = double.MinValue;
            for (int i = 0; i < PathPoints.Length; i++) {
                xMin = Math.Min(PathPoints[i].X, xMin);
                xMax = Math.Max(PathPoints[i].X, xMax);
                yMin = Math.Min(PathPoints[i].Y, yMin);
                yMax = Math.Max(PathPoints[i].Y, yMax);
            }

            // Calculate offset based on extents of path, and chosen alignment
            double XOffSet = 0, YOffSet = 0;
            switch (alignment) {
                case ContentAlignment.BottomLeft:
                    XOffSet = -xMin;
                    YOffSet = -yMax;
                    break;
                case ContentAlignment.BottomCenter:
                    XOffSet = -xMin - (xMax - xMin) / 2;
                    YOffSet = -yMax;
                    break;
                case ContentAlignment.BottomRight:
                    XOffSet = -xMax;
                    YOffSet = -yMax;
                    break;
                case ContentAlignment.MiddleLeft:
                    XOffSet = -xMin;
                    YOffSet = -yMax + (yMax - yMin) / 2;
                    break;
                case ContentAlignment.MiddleCenter:
                    XOffSet = -xMin - (xMax - xMin) / 2;
                    YOffSet = -yMax + (yMax - yMin) / 2;
                    break;
                case ContentAlignment.MiddleRight:
                    XOffSet = -xMax;
                    YOffSet = -yMax + (yMax - yMin) / 2;
                    break;
                case ContentAlignment.TopLeft:
                    XOffSet = -xMin;
                    YOffSet = -yMin;
                    break;
                case ContentAlignment.TopCenter:
                    XOffSet = -xMin - (xMax - xMin) / 2;
                    YOffSet = -yMin;
                    break;
                case ContentAlignment.TopRight:
                    XOffSet = -xMax;
                    YOffSet = -yMin;
                    break;
            }

            // Font is measured in "points", but gcode is in mm
            double Xscale = 0.352778, Yscale = 0.352778;

            // We store the start coords of each segment in order to be able close paths back to start point after final segment 
            double XSave = 0, YSave = 0;
            byte pathType;

            // Loop through and process all points in the path
            for (int i = 0; i < PathPoints.Length; i++) {
                if (PathPoints[i].IsEmpty == false) {

                    double x = PathPoints[i].X;
                    // Offset is measured in points, calcaulted from the original graphic data
                    x = x + XOffSet;
                    x = x * Xscale;
                    // Position is measured in mm, as per gcode standards
                    x += origin.X;
                    x = Math.Round(x, 4); // Don't need full precision

                    double y = PathPoints[i].Y;
                    y = y + YOffSet;
                    y = y * Yscale;
                    y -= origin.Y;
                    y = Math.Round(y, 4); // Don't need full precision
                    y = 0 - y; // Invert y

                    // https://docs.microsoft.com/en-us/dotnet/api/system.drawing.drawing2d.pathpointtype?view=netframework-4.8
                    pathType = pathtypes[i]; // 0=Start of line, 1=Straight line, 3=Curve
                    // Start point
                    if (pathType == 0) {
                        // Save the coordinate value so we can join back again at the end
                        XSave = x;
                        YSave = y;

                        sb.AppendLine("; Figure Start");
                        // Turn the laser off
                        sb.AppendLine("M5 S0");
                        // Move to the start position
                        sb.AppendLine(string.Format("G00X{0}Y{1}", x, y));
                        // Now turn the laser on
                        sb.AppendLine(string.Format("{0} S{1}", mode, power));
                    }
                    // Straight line or curve segment
                    else if (pathType < 128) {
                        sb.AppendLine(string.Format("G01X{0}Y{1}F{2}", x, y, speed));
                    }
                    // End segment
                    else {
                        sb.AppendLine("; Figure End");
                        sb.AppendLine(string.Format("G01X{0}Y{1}F{2}", x, y, speed));
                        // Connect back to the start point of this figure
                        sb.AppendLine(string.Format("G01X{0}Y{1}F{2}", XSave, YSave, speed));
                        sb.AppendLine("M5 S0");
                    }
                }
            }
            return sb.ToString();
        }

        private void BtnGenerate_Text_Click(object sender, EventArgs e) {

            if (textBox_Input.Text == "") { return; }

            var anchorPosition = ((Button)sender).Parent.Controls["groupBox1"].Controls["tableLayoutPanel1"].Controls.OfType<RadioButton>().FirstOrDefault(n=>n.Checked);
            ContentAlignment contentAlignment = ContentAlignment.MiddleCenter;
            switch (anchorPosition.Name){
                case "radioButton_TL":
                    contentAlignment = ContentAlignment.TopLeft;
                    break;
                case "radioButton_TC":
                    contentAlignment = ContentAlignment.TopCenter;
                    break;
                case "radioButton_TR":
                    contentAlignment = ContentAlignment.TopRight;
                    break;
                case "radioButton_ML":
                    contentAlignment = ContentAlignment.MiddleLeft;
                    break;
                case "radioButton_MC":
                    contentAlignment = ContentAlignment.MiddleCenter;
                    break;
                case "radioButton_MR":
                    contentAlignment = ContentAlignment.MiddleRight;
                    break;
                case "radioButton_BL":
                    contentAlignment = ContentAlignment.BottomLeft;
                    break;
                case "radioButton_BC":
                    contentAlignment = ContentAlignment.BottomCenter;
                    break;
                case "radioButton_BR":
                    contentAlignment = ContentAlignment.BottomRight;
                    break;
            }

            string gcode = CreateGcodeFromText(
               textBox_Input.Text,
                new FontFamily(textBox_Font.Text),
                0,
                (float)numericUpDown_FontSize.Value,
                new PointF((float)numericUpDown_TextX.Value, (float)numericUpDown_TextY.Value),
                contentAlignment);

            mCore.OpenString(gcode);
        }

        private void numericUpDown_EngraveLineInterval_ValueChanged(object sender, EventArgs e)
        {
            label_EngraveLineInterval.Text = (1.0f / (float)numericUpDown_EngraveLineInterval.Value).ToString();
            label_EngraveDPI.Text = ((float)numericUpDown_EngraveLineInterval.Value * 25.4f).ToString();
        }
    }
}
