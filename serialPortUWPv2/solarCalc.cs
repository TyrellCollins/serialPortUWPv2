using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serialPortUWPv2
{
    class SolarCalc
    {
        //Field
        private static int ResistorValue;
        private static double Vref;

        //Constructor that takes no arguements
        public SolarCalc()
        {
            ResistorValue = 100;
            Vref = 5.03; //Voltage Reference 
            //ohms law
            //5.03V/100 ohm = 50.3 mA
        }


        //Method

        public string GetSolarVoltage(int an0)
        {
            double dAn0 = an0 * Vref / 1024.0;  // Vref = 5.03
            return dAn0.ToString("0.0000");

        }

        public string GetBatteryVoltage(int an2)
        {
            double dBatVolt = an2 * Vref / 1024.0;
            return dBatVolt.ToString("0.0000");
        }

        public string GetBatteryCurrent(int an1, int an2)
        {
            int ShuntAnalog = an1 - an2;
            double ShuntVoltage = ShuntAnalog * Vref / 1024.0;
            double dBatCurrent = ShuntVoltage / ResistorValue;
            return dBatCurrent.ToString("0.000000");

        }

        public string GetLEDcurrent(int LEDAnalog, int an1)
        {
            int ShuntAnalog = an1 - LEDAnalog;
            double ShuntVoltage = ShuntAnalog * Vref / 1024.0;
            double dLEDcurrent = ShuntVoltage / ResistorValue;
            if (dLEDcurrent < 0.0001)
            {
                dLEDcurrent = 0;
            }
            return dLEDcurrent.ToString("0.000000");
        }
    }
}
