namespace SWENG.UserInterface
{
    public class GuiChartOptions
    {
        /* Axes names for the various chart types.
         * Values currently are:
         * Deviation | Repitition | Time
         */
        public string[] AxesName { get; set; }

        /* Current chart types are:
         * Repetition & Time
         */
        public string ChartType { get; set; }

        /* Use tick marks in Chart: Yes / No */
        public bool TickMarks { get; set; }

        /* Data points received from Summary screen */
        public float[] DataPoints { get; set; }

        /* Marker size for data points */
        public float Scale { get; set; }

        /* Time Interval for x-axis tick marks */
        public float TimeInterval { get; set; }

        /* Repetition Duration to be displayed a x-axis length */
        public float RepDuration { get; set; }

        /* Whether or not to display chart lines */
        public bool ChartLines { get; set; }

        /// <summary>
        /// Various chart options to be used for chart data display
        /// </summary>
        /// <param name="axesNames"></param>
        /// <param name="chartType"></param>
        /// <param name="chartLines"></param>
        /// <param name="tickMarks"></param>
        /// <param name="scale"></param>
        /// <param name="dataPoints"></param>
        /// <param name="timeInterval"></param>
        /// <param name="repDuration"></param>
        public GuiChartOptions(string[] axesNames, string chartType, bool chartLines, bool tickMarks, float scale, float[] dataPoints, float timeInterval, float repDuration)
        {
            AxesName = axesNames;
            ChartType = chartType;
            TickMarks = tickMarks;
            DataPoints = dataPoints;
            Scale = scale;
            TimeInterval = timeInterval;
            RepDuration = repDuration;
            ChartLines = chartLines;
        }
    }
}
