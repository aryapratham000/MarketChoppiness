using System;
using System.Collections.Generic;
using System.Drawing;
using TradingPlatform.BusinessLayer;

namespace ChoppyMarket
{
    public class ChoppyMarket : Indicator
    {
        // Input parameters for EMAs
        [InputParameter("Mini EMA Length", 10)]
        public int MiniEma = 8;

        [InputParameter("Fast EMA Length", 20)]
        public int FastEma = 50;

        [InputParameter("Slow EMA Length", 30)]
        public int SlowEma = 200;

        // Period to calculate choppiness over the last n bars
        [InputParameter("Choppiness Calculation Period (Bars)", 40)]
        public int choppinessPeriod = 15;

        // EMA Indicators
        private Indicator emaMini;
        private Indicator emaFast;
        private Indicator emaSlow;

        // Variables to track choppiness
        private Queue<int> crossoverQueue;

        public ChoppyMarket()
            : base()
        {
            this.Name = "Choppiness Indicator";
            this.Description = "Tracks the choppiness of the market based on EMA crossovers";

            this.AddLineSeries("Choppiness Score", Color.Blue, 2, LineStyle.Solid);

            this.SeparateWindow = true;
        }

        protected override void OnInit()
        {
            // Initialize the EMA indicators
            this.emaMini = Core.Indicators.BuiltIn.EMA(this.MiniEma, PriceType.Close);
            this.emaFast = Core.Indicators.BuiltIn.EMA(this.FastEma, PriceType.Close);
            this.emaSlow = Core.Indicators.BuiltIn.EMA(this.SlowEma, PriceType.Close);

            this.AddIndicator(this.emaMini);
            this.AddIndicator(this.emaFast);
            this.AddIndicator(this.emaSlow);

            // Initialize the crossover tracking queue
            this.crossoverQueue = new Queue<int>();
        }

        protected override void OnUpdate(UpdateArgs args)
        {
            // Get current EMA values
            double previousEmaMini = this.emaMini.GetValue(1);
            double previousEmaFast = this.emaFast.GetValue(1);
            double previousEmaSlow = this.emaSlow.GetValue(1);

            double currentEmaMini = this.emaMini.GetValue();
            double currentEmaFast = this.emaFast.GetValue();
            double currentEmaSlow = this.emaSlow.GetValue(); 
                

            // Track crossovers in this bar
            int crossovers = 0;

            // Check for crossovers between EMA 8 and EMA 50
            if ((previousEmaMini < previousEmaFast && currentEmaMini > currentEmaFast) || (previousEmaMini > previousEmaFast && currentEmaMini < currentEmaFast))
            {
                crossovers++;
            }

            if ((previousEmaMini < previousEmaSlow && currentEmaMini > currentEmaSlow) || (previousEmaMini > previousEmaSlow && currentEmaMini < currentEmaSlow))
            {
                crossovers++;
            }

            if ((previousEmaFast < previousEmaSlow && currentEmaFast > currentEmaSlow) || (previousEmaFast > previousEmaSlow && currentEmaFast < currentEmaSlow))
            {
                crossovers++;
            }


            // Add crossovers for this bar to the queue
            this.crossoverQueue.Enqueue(crossovers);

            // Remove old crossovers that are outside the choppiness period
            if (this.crossoverQueue.Count > this.choppinessPeriod)
            {
                this.crossoverQueue.Dequeue();
            }

            // Calculate the total choppiness score for the last n bars
            int totalChoppinessScore = 0;
            foreach (int crossoverCount in this.crossoverQueue)
            {
                totalChoppinessScore += crossoverCount;
            }

            // Plot the choppiness score
            this.SetValue(totalChoppinessScore, 0);
        }
    }
}
