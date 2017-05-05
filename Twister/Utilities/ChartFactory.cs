using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Color = System.Drawing.Color;
using FontStyle = System.Drawing.FontStyle;

namespace Twister.Utilities
{
    /// <summary>
    ///     Responsible for creating a chart that will display the torque vs.
    ///     angle infomation for a test that is currently running, or has
    ///     been running at one time.
    /// </summary>
    public class ChartFactory
    {
        // really big so not visible on the chart.
        private static readonly DataPoint _dummyPoint = new DataPoint(100000, 100000);

        // Singleton pattern.
        private static ChartFactory _factory;

        private ChartFactory()
        {
        }

        internal static void CreateFullyReversedChart(Chart chart, int minY, int maxY)
        {
            // no chart needed for calibration, todo probably a hack
            if (chart == null)
                return;

            if (chart.ChartAreas.Count == 0)
                chart.ChartAreas.Add(new ChartArea("area1")
                {
                    BackColor = Color.DarkGray
                });

            if (chart.Series.Count == 0)
            {
                // create a new series
                chart.Series.Add(new Series("series1")
                {
                    ChartType = SeriesChartType.FastPoint,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 3,
                    MarkerColor = Color.Blue,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold)
                });

                // just add it once, NEVER remove it.  If it's removed
                // the graph will disappear when all points in the series 
                // are cleared out.  I want the graph to stay visible, so 
                // this is my workaround.
                chart.Series[0].Points.Add(_dummyPoint);
            }

            float chartWidth = chart.ChartAreas[0].InnerPlotPosition.Width;

            // x-axis
            chart.ChartAreas[0].AxisX.Minimum = -8;
            chart.ChartAreas[0].AxisX.Maximum = 8;
            chart.ChartAreas[0].AxisX.Title = "Angle";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Segoe UI", 16, FontStyle.Regular);
            chart.ChartAreas[0].AxisX.LabelStyle.Interval = 1;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "{0:n0}°";
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 1;
            chart.ChartAreas[0].AxisX.MajorGrid.Interval = 1;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Gray;
            chart.ChartAreas[0].AxisX.Crossing = 0;
            chart.ChartAreas[0].AxisX.IsMarksNextToAxis = false;

            // y-axis
            chart.ChartAreas[0].AxisY.Minimum = minY * 1.25;
            chart.ChartAreas[0].AxisY.Maximum = maxY * 1.25;
            chart.ChartAreas[0].AxisY.Title = "Torque (lb-in)";
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Segoe UI", 16, FontStyle.Regular);
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
            chart.ChartAreas[0].AxisY.Crossing = 0;
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "n0";
            chart.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chart.ChartAreas[0].AxisY.Interval = chart.ChartAreas[0].AxisY.Maximum / 5;
            chart.ChartAreas[0].AxisY.IntervalType = DateTimeIntervalType.Number;
            chart.ChartAreas[0].AxisY.IntervalOffset = 0;
            chart.ChartAreas[0].AxisY.IntervalOffsetType = DateTimeIntervalType.Auto;
            chart.ChartAreas[0].AxisY.IsMarksNextToAxis = false;


            // this can be set lower if there are performance issues.
            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

            var chartTitle = "Torque Vs. Angle";
            if (chart.Titles.Count == 0)
            {
                chart.Titles.Add(chartTitle);
                chart.Titles[0].Font = new Font("Segoe UI", 16, FontStyle.Regular);
            }

            chart.Titles[0].Text = chartTitle;
        }

        internal static void CreateUnidirectionalChart(Chart chart, int maxY)
        {
            if (chart.ChartAreas.Count == 0)
                chart.ChartAreas.Add(new ChartArea("area1")
                {
                    BackColor = Color.DarkGray
                });

            if (chart.Series.Count == 0)
            {
                // create a new series
                chart.Series.Add(new Series("series1")
                {
                    ChartType = SeriesChartType.FastPoint,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 3,
                    MarkerColor = System.Drawing.Color.Blue,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold)
                });

                // just add it once, NEVER remove it.  If it's removed
                // the graph will disappear when all points in the series 
                // are cleared out.  I want the graph to stay visible, so 
                // this is my workaround.
                chart.Series[0].Points.Add(new DataPoint(0, 0));
            }

            float chartWidth = chart.ChartAreas[0].InnerPlotPosition.Width;

            // x-axis
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = double.NaN;
            chart.ChartAreas[0].AxisX.Title = "Angle";
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Segoe UI", 16, FontStyle.Regular);
            chart.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.FixedCount;
            chart.ChartAreas[0].AxisX.LabelStyle.Interval = chart.ChartAreas[0].AxisX.Maximum / 5;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "{0:n2}°";
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 1;
            chart.ChartAreas[0].AxisX.MajorGrid.Interval = chart.ChartAreas[0].AxisX.Maximum / 5;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Gray;
            chart.ChartAreas[0].AxisX.Crossing = 0;
            chart.ChartAreas[0].AxisX.IsMarksNextToAxis = false;

            // y-axis
            if (maxY < 0)
            {
                chart.ChartAreas[0].AxisY.Minimum = double.NaN;
                chart.ChartAreas[0].AxisY.Maximum = double.NaN;
            }
            else
            {
                chart.ChartAreas[0].AxisY.Minimum = double.NaN;
                chart.ChartAreas[0].AxisY.Maximum = double.NaN;
            }
            chart.ChartAreas[0].AxisY.Title = "Torque (lb-in)";
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Segoe UI", 16, FontStyle.Regular);
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
            chart.ChartAreas[0].AxisY.Crossing = 0;
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "n0";
            chart.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.FixedCount;
            chart.ChartAreas[0].AxisY.Interval = chart.ChartAreas[0].AxisY.Maximum / 5;
            chart.ChartAreas[0].AxisY.IntervalType = DateTimeIntervalType.Number;
            chart.ChartAreas[0].AxisY.IntervalOffset = 0;
            chart.ChartAreas[0].AxisY.IntervalOffsetType = DateTimeIntervalType.Auto;
            chart.ChartAreas[0].AxisY.IsMarksNextToAxis = false;

            // this can be set lower if there are performance issues.
            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

            var chartTitle = "Torque Vs. Angle";
            if (chart.Titles.Count == 0)
            {
                chart.Titles.Add(chartTitle);
                chart.Titles[0].Font = new Font("Segoe UI", 16, FontStyle.Regular);
            }

            chart.Titles[0].Text = chartTitle;
        }
    }
}