using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSpectrometerReader.Classes
{
    public class Parameters
    {
        public static double deltaEResult1 = 0;
        public static double deltaEResult2 = 0;
        public static double deltaEResult3 = 0;
        public static double deltaECMC11Result1 = 0;
        public static double deltaECMC11Result2 = 0;
        public static double deltaECMC11Result3 = 0;
        public static double deltaECMC21Result1 = 0;
        public static double deltaECMC21Result2 = 0;
        public static double deltaECMC21Result3 = 0;
        public static double deltaEH1 = 0;
        public static double deltaEH2 = 0;
        public static double deltaEH3 = 0;
        public static double deltaE12 = 0;
        public static double deltaE13 = 0;
        public static double deltaE23 = 0;

        public static double lStandartSample1 = 0;
        public static double aStandartSample1 = 0;
        public static double bStandartSample1 = 0;
        public static double lStandartSample2 = 0;
        public static double aStandartSample2 = 0;
        public static double bStandartSample2 = 0;
        public static double lStandartSample3 = 0;
        public static double aStandartSample3 = 0;
        public static double bStandartSample3 = 0;

        public static double cSample1 = 0;
        public static double hSample1 = 0;
        public static double SLSample1 = 0;
        public static double SCSample1 = 0;
        public static double SHSample1 = 0;
        public static double cTest1 = 0;
        public static double hTest1 = 0;
        public static double cSample2 = 0;
        public static double hSample2 = 0;
        public static double SLSample2 = 0;
        public static double SCSample2 = 0;
        public static double SHSample2 = 0;
        public static double cTest2 = 0;
        public static double hTest2 = 0;
        public static double cSample3 = 0;
        public static double hSample3 = 0;
        public static double SLSample3 = 0;
        public static double SCSample3 = 0;
        public static double SHSample3 = 0;
        public static double cTest3 = 0;
        public static double hTest3 = 0;

        public static double aNmArguman = 0;
        public static double bNmArguman = 0;
        public static double cNmArguman = 0;
        public static double dNmArguman = 0;
        public static double eNmArguman = 0;

        public static int dataLenght = 2049;
        public static int dataCount = 61;
        public static int firstNm = 400;

        public static SQLiteConnection connection;
        public static SQLiteCommand command;
        public static string nameDatabase = "ShadeBoxBar";
        public static string nameTable = "Datas";
        public static int id = 1;

        public static int leftCount = 0;
        public static double currentMeter = 0;
        public static double currentMeterOld = 0;
        public static bool externalInput = false;
        public static bool externalInputOld = false;
        public static bool machineRunning = false;
        public static bool machineRunningOld = false;

        public static string jsonFilePath = ""; // @"C:\Json\JsonFile\";
        public static string jsonName = "Settings.json";

        public static string logTime = "";
        public static string logName = "";
        public static string logMeasFile = "";
    }
}
