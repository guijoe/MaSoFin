using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewerController : MonoBehaviour {

    int NrPlots = 13;//6;//
    float[] AxisLengths;
    public float[][][] tuples;

    DataInterface timeSeries;

    Plot4D[] plots;
    public Plot4D plot4DPrefab;

    //public int[][] metricIndices;

    public bool play = false;
    public bool reset = false;

    public bool enhancePlot = false;

    public int frame = 0;

    int[] dim;
    public static Vector3[] NSquaredInN;

    public string folder = "C:/Users/18003111/OneDrive - MMU/Documents/movit/application.windows64/MovitData/distant/180420hZ";

    // Use this for initialization
    void Awake () {
        //timeSeries = new MovitTimeSeries(folder, "test_06012020.csv");//"test.csv");//
        timeSeries = new Simulator(folder, "test_06012020.csv");
        timeSeries.ProcessMovitData();
        //timeSeries.Log();

        dim = new int[2];
        dim[1] = (int)Mathf.Floor(Mathf.Sqrt(NrPlots));
        dim[0] = dim[1]  + 1;
        dim[0] = 5;
        dim[1] = 3;
        SetNSquaredInN(dim[0], dim[1]);

        plots = new Plot4D[NrPlots];
        AxisLengths = new float[3] { 10, 10, 10 };
        for (int i = 0; i < NrPlots; i++)
        {
            plots[i] = Instantiate(plot4DPrefab);
            plots[i].AxisLengths = AxisLengths;

            plots[i].transform.SetParent(transform);
            //plots[i].transform.position = (AxisLengths[0] + 3) * NSquaredInN[i];
            plots[i].transform.position = new Vector3 (20 * NSquaredInN[i].x, 15 * NSquaredInN[i].y, 0);

            plots[i].speed = 1;
        }

        resetValues = new int[15]{0,258, 515, 0, 258, 515, 0, 258, 515, 515, 515, 515, 515, 515, 515};
        //resetValues = new int[15]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

        /*
        // ***** Development description ***** //
        tuples = timeSeries.DataFrameToTuples();
        
        // 1st Plot: Rotated Points
        plots[0].metricIndices = new int[6] { 16, 17, 18, 13, 14, 50 };
        plots[0].userSetBounds = true;
        plots[0].XYZbounds[0][0] = 0; plots[0].XYZbounds[1][0] = 0; plots[0].XYZbounds[2][0] = 0;
        plots[0].XYZbounds[0][1] = 80; plots[0].XYZbounds[1][1] = 56; plots[0].XYZbounds[2][1] = 80;
        plots[0].NrClasses = new int[3]{4, 4, 4};

        // 1st Plot: Rotated Points
        plots[1].metricIndices = new int[6] { 16, 17, 18, 13, 14, 50 };
        plots[1].userSetBounds = true;
        plots[1].XYZbounds[0][0] = 0; plots[1].XYZbounds[1][0] = 0; plots[1].XYZbounds[2][0] = 0;
        plots[1].XYZbounds[0][1] = 80; plots[1].XYZbounds[1][1] = 56; plots[1].XYZbounds[2][1] = 80;
        plots[1].NrClasses = new int[3]{4, 4, 4};

        // 1st Plot: Rotated Points
        plots[2].metricIndices = new int[6] { 16, 17, 18, 13, 14, 50 };
        plots[2].userSetBounds = true;
        plots[2].XYZbounds[0][0] = 0; plots[2].XYZbounds[1][0] = 0; plots[2].XYZbounds[2][0] = 0;
        plots[2].XYZbounds[0][1] = 80; plots[2].XYZbounds[1][1] = 56; plots[2].XYZbounds[2][1] = 80;
        plots[2].NrClasses = new int[3]{4, 4, 4};

        // 2nd Plot: Orientation Axis Distribution - vectors
        plots[3].metricIndices = new int[6] { 18, 16, 17, 13, 14, 50 };
        plots[3].userSetBounds = true;
        plots[3].XYZbounds[0][0] = 0; plots[3].XYZbounds[1][0] = 0; plots[3].XYZbounds[2][0] = 0;
        plots[3].XYZbounds[0][1] = 80; plots[3].XYZbounds[1][1] = 80; plots[3].XYZbounds[2][1] = 56;
        plots[3].NrClasses = new int[3]{4, 4, 4};

        // 2nd Plot: Orientation Axis Distribution - vectors
        plots[4].metricIndices = new int[6] { 18, 16, 17, 13, 14, 50 };
        plots[4].userSetBounds = true;
        plots[4].XYZbounds[0][0] = 0; plots[4].XYZbounds[1][0] = 0; plots[4].XYZbounds[2][0] = 0;
        plots[4].XYZbounds[0][1] = 80; plots[4].XYZbounds[1][1] = 80; plots[4].XYZbounds[2][1] = 56;
        plots[4].NrClasses = new int[3]{4, 4, 4};

        // 2nd Plot: Orientation Axis Distribution - vectors
        plots[5].metricIndices = new int[6] { 18, 16, 17, 13, 14, 50 };
        plots[5].userSetBounds = true;
        plots[5].XYZbounds[0][0] = 0; plots[5].XYZbounds[1][0] = 0; plots[5].XYZbounds[2][0] = 0;
        plots[5].XYZbounds[0][1] = 80; plots[5].XYZbounds[1][1] = 80; plots[5].XYZbounds[2][1] = 56;
        plots[5].NrClasses = new int[3]{4, 4, 4};

        // 3rd Plot: PD Axis Length
        plots[6].metricIndices = new int[6] { 23, 53, 24, 13, 14, 50 };
        plots[6].temporal = true;
        plots[6].userSetBounds = true;
        plots[6].XYZbounds[0][0] = 28; plots[6].XYZbounds[1][0] = 0; plots[6].XYZbounds[2][0] = 0;
        plots[6].XYZbounds[0][1] = 47.85f; plots[6].XYZbounds[1][1] = 80; plots[6].XYZbounds[2][1] = 80;
        plots[6].whiteBackground = true;
        plots[6].axisLabels = new string[3]{"t(hpf)", "PD(μm)", "" };
        plots[6].NrClasses = new int[3]{5, 5, 5};

        // 4th Plot: AP Axis Length
        plots[7].metricIndices = new int[6] { 23, 54, 24, 13, 14, 50 };
        plots[7].temporal = true;
        plots[7].userSetBounds = true;
        plots[7].XYZbounds[0][0] = 28; plots[7].XYZbounds[1][0] = 0; plots[7].XYZbounds[2][0] = 0;
        plots[7].XYZbounds[0][1] = 47.85f; plots[7].XYZbounds[1][1] = 80; plots[7].XYZbounds[2][1] = 80;
        plots[7].whiteBackground = true;
        plots[7].axisLabels = new string[3]{"t(hpf)", "AP(μm)", "" };
        plots[7].NrClasses = new int[3]{5, 5, 5};

        // 5th Plot: DV Axis Length
        plots[8].metricIndices = new int[6] { 23, 55, 24, 13, 14, 50 };
        plots[8].temporal = true;
        plots[8].userSetBounds = true;
        plots[8].XYZbounds[0][0] = 28; plots[8].XYZbounds[1][0] = 0; plots[8].XYZbounds[2][0] = 0;
        plots[8].XYZbounds[0][1] = 47.85f; plots[8].XYZbounds[1][1] = 80; plots[8].XYZbounds[2][1] = 80;
        plots[8].whiteBackground = true;
        plots[8].axisLabels = new string[3]{"t(hpf)", "DV(μm)", "" };
        plots[8].NrClasses = new int[3]{5, 5, 5};
        //*/

        //*
        // ***** Orientation angle distribution ***** //
        tuples = timeSeries.DataFrameToTuples();
        
        // 1st Plot: Rotated Points
        plots[0].metricIndices = new int[6] { 16, 17, 18, 13, 14, 50 };
        plots[0].axisLabels = new string[3]{"AP(μm)", "PD(μm)", "" };
        plots[0].NrClasses = new int[3]{5, 5, 5};
        plots[0].XYZbounds[0][0] = 0; plots[0].XYZbounds[1][0] = 0; plots[0].XYZbounds[2][0] = 0;
        plots[0].XYZbounds[0][1] = 80; plots[0].XYZbounds[1][1] = 56; plots[0].XYZbounds[2][1] = 80;
        plots[0].userSetBounds = true;
        //plots[0].colorGradient = true;
        
        // 1st Plot: Rotated Points
        plots[1].metricIndices = new int[6] { 16, 17, 18, 13, 14, 50 };
        plots[1].axisLabels = new string[3]{"AP(μm)", "PD(μm)", "" };
        plots[1].NrClasses = new int[3]{5, 5, 5};
        plots[1].XYZbounds[0][0] = 0; plots[1].XYZbounds[1][0] = 0; plots[1].XYZbounds[2][0] = 0;
        plots[1].XYZbounds[0][1] = 80; plots[1].XYZbounds[1][1] = 56; plots[1].XYZbounds[2][1] = 80;
        plots[1].userSetBounds = true;
        //plots[1].colorGradient = true;
        
        // 1st Plot: Rotated Points
        plots[2].metricIndices = new int[6] { 16, 17, 18, 13, 14, 50 };
        plots[2].axisLabels = new string[3]{"AP(μm)", "PD(μm)", "" };
        plots[2].NrClasses = new int[3]{5, 5, 5};
        plots[2].XYZbounds[0][0] = 0; plots[2].XYZbounds[1][0] = 0; plots[2].XYZbounds[2][0] = 0;
        plots[2].XYZbounds[0][1] = 80; plots[2].XYZbounds[1][1] = 56; plots[2].XYZbounds[2][1] = 80;
        plots[2].userSetBounds = true;
        //plots[2].colorGradient = true;
        
        // 2nd Plot: Orientation Axis Distribution - vectors
        plots[3].metricIndices = new int[22] { 16, 17, 18, 13, 14, 50, 6, 7, 8, 9, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        plots[3].displayCells = false;
        plots[3].axisLabels = new string[3]{"AP(μm)", "PD(μm)", "" };
        plots[3].NrClasses = new int[3]{5, 5, 5};
        plots[3].XYZbounds[0][0] = 0; plots[3].XYZbounds[1][0] = 0; plots[3].XYZbounds[2][0] = 0;
        plots[3].XYZbounds[0][1] = 80; plots[3].XYZbounds[1][1] = 56; plots[3].XYZbounds[2][1] = 80;
        plots[3].userSetBounds = true;
        //plots[3].colorGradient = true;
        
        // 2nd Plot: Orientation Axis Distribution - vectors
        plots[4].metricIndices = new int[22] { 16, 17, 18, 13, 14, 50, 6, 7, 8, 9, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        plots[4].displayCells = false;
        plots[4].axisLabels = new string[3]{"AP(μm)", "PD(μm)", "" };
        plots[4].NrClasses = new int[3]{5, 5, 5};
        plots[4].XYZbounds[0][0] = 0; plots[4].XYZbounds[1][0] = 0; plots[4].XYZbounds[2][0] = 0;
        plots[4].XYZbounds[0][1] = 80; plots[4].XYZbounds[1][1] = 56; plots[4].XYZbounds[2][1] = 80;
        plots[4].userSetBounds = true;
        //plots[4].colorGradient = true;
        
        // 2nd Plot: Orientation Axis Distribution - vectors
        plots[5].metricIndices = new int[22] { 16, 17, 18, 13, 14, 50, 6, 7, 8, 9, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        plots[5].displayCells = false;
        plots[5].axisLabels = new string[3]{"AP(μm)", "PD(μm)", "" };
        plots[5].NrClasses = new int[3]{5, 5, 5};
        plots[5].XYZbounds[0][0] = 0; plots[5].XYZbounds[1][0] = 0; plots[5].XYZbounds[2][0] = 0;
        plots[5].XYZbounds[0][1] = 80; plots[5].XYZbounds[1][1] = 56; plots[5].XYZbounds[2][1] = 80;
        plots[5].userSetBounds = true;
        //plots[5].colorGradient = true;
        
        // 3rd Plot: Orientation Angle Distribution
        plots[6].metricIndices = new int[6] { 11, 12, 24, 13, 14, 50 };
        plots[6].defaultPrefab = false;
        plots[6].axisLabels = new string[3]{"θ", "Frequency of θ", "" };
        plots[6].whiteBackground = true;
        plots[6].XYZbounds[0][0] = 0; plots[6].XYZbounds[1][0] = 0; plots[6].XYZbounds[2][0] = 0;
        plots[6].XYZbounds[0][1] = 90; plots[6].XYZbounds[1][1] = 78; plots[6].XYZbounds[2][1] = 80;
        plots[6].userSetBounds = true;
        plots[6].nbOfSeries = 2;
        plots[6].complementarySeries = new int[1][];
        plots[6].complementarySeries[0] = new int[9]{60,59,24,24,13,50,73,74,75};
        plots[6].drawLines = true;
        
        // 3rd Plot: Orientation Angle Distribution
        plots[7].metricIndices = new int[6] { 11, 12, 24, 13, 14, 50 };
        plots[7].defaultPrefab = false;
        plots[7].axisLabels = new string[3]{"θ", "Frequency of θ", "" };
        plots[7].whiteBackground = true;
        plots[7].XYZbounds[0][0] = 0; plots[7].XYZbounds[1][0] = 0; plots[7].XYZbounds[2][0] = 0;
        plots[7].XYZbounds[0][1] = 90; plots[7].XYZbounds[1][1] = 78; plots[7].XYZbounds[2][1] = 80;
        plots[7].userSetBounds = true;
        plots[7].nbOfSeries = 2;
        plots[7].complementarySeries = new int[1][];
        plots[7].complementarySeries[0] = new int[9]{60,59,24,24,13,50,73,74,75};
        plots[7].drawLines = true;
        
        // 3rd Plot: Orientation Angle Distribution
        plots[8].metricIndices = new int[6] { 11, 12, 24, 13, 14, 50 };
        plots[8].defaultPrefab = false;
        plots[8].axisLabels = new string[3]{"θ", "Frequency of θ", "" };
        plots[8].whiteBackground = true;
        plots[8].XYZbounds[0][0] = 0; plots[8].XYZbounds[1][0] = 0; plots[8].XYZbounds[2][0] = 0;
        plots[8].XYZbounds[0][1] = 90; plots[8].XYZbounds[1][1] = 78; plots[8].XYZbounds[2][1] = 80;
        plots[8].userSetBounds = true;
        plots[8].nbOfSeries = 2;
        plots[8].complementarySeries = new int[1][];
        plots[8].complementarySeries[0] = new int[9]{60,59,24,24,13,50,73,74,75};
        plots[8].drawLines = true;
        
        // 4th Plot: Orientation Angle Distribution - Mean
        plots[12].metricIndices = new int[6] { 23, 21, 24, 13, 14, 50 };
        plots[12].temporal = true;
        plots[12].axisLabels = new string[3]{"t(hpf)", "Mean of θ", "" };
        plots[12].whiteBackground = true;
        plots[12].XYZbounds[0][0] = 28; plots[12].XYZbounds[1][0] = 30; plots[12].XYZbounds[2][0] = 0;
        plots[12].XYZbounds[0][1] = 47.85f; plots[12].XYZbounds[1][1] = 90; plots[12].XYZbounds[2][1] = 80;
        plots[12].userSetBounds = true;
        plots[12].NrClasses = new int[3]{4, 4, 3};
        plots[12].nbOfSeries = 3;
        plots[12].complementarySeries = new int[2][];
        plots[12].complementarySeries[0] = new int[6]{23,76,24,24,13,50};
        plots[12].complementarySeries[1] = new int[6]{23,77,24,24,13,50};
        plots[12].drawLines = false;


        // 5th Plot: Orientation Angle Distribution - StandardDeviation
        //plots[10].metricIndices = new int[6] { 23, 22, 24, 13, 14, 50 };
        //plots[10].temporal = true;
        //plots[10].axisLabels = new string[3]{"t(hpf)", "SD of θ", "" };
        //plots[10].whiteBackground = true;

        // 4th Plot: Random Angle Distribution in 3D
        //plots[11].metricIndices = new int[6] { 60, 59, 24, 13, 14, 50 };
        //plots[11].axisLabels = new string[3]{"θ", "P(θ)(%)", "" };
        //plots[11].whiteBackground = true;
        //plots[11].significantPlaces = new string[3]{"0", "0", "0"};
        //plots[11].NrClasses = new int[3]{3, 5, 5};
        //plots[11].defaultPrefab = false;
        //plots[11].XYZbounds[0][0] = 0; plots[11].XYZbounds[1][0] = 0; plots[11].XYZbounds[2][0] = 0;
        //plots[11].XYZbounds[0][1] = 90; plots[11].XYZbounds[1][1] = 78; plots[11].XYZbounds[2][1] = 80;
        //plots[11].userSetBounds = true;
        
        
        // 6th Plot: PD Axis Length
        plots[9].metricIndices = new int[6] { 23, 53, 24, 13, 14, 50 };
        plots[9].temporal = true;
        plots[9].userSetBounds = true;
        plots[9].XYZbounds[0][0] = 28; plots[9].XYZbounds[1][0] = 0; plots[9].XYZbounds[2][0] = 0;
        plots[9].XYZbounds[0][1] = 47.85f; plots[9].XYZbounds[1][1] = 80; plots[9].XYZbounds[2][1] = 80;
        plots[9].whiteBackground = true;
        plots[9].axisLabels = new string[3]{"t(hpf)", "PD(μm)", "" };
        plots[9].NrClasses = new int[3]{4, 5, 5};
        
        // 7th Plot: AP Axis Length
        plots[10].metricIndices = new int[6] { 23, 54, 24, 13, 14, 50 };
        plots[10].temporal = true;
        plots[10].userSetBounds = true;
        plots[10].XYZbounds[0][0] = 28; plots[10].XYZbounds[1][0] = 0; plots[10].XYZbounds[2][0] = 0;
        plots[10].XYZbounds[0][1] = 47.85f; plots[10].XYZbounds[1][1] = 80; plots[10].XYZbounds[2][1] = 80;
        plots[10].whiteBackground = true;
        plots[10].axisLabels = new string[3]{"t(hpf)", "AP(μm)", "" };
        plots[10].NrClasses = new int[3]{4, 5, 5};
        
        // 7th Plot: DV Axis Length
        plots[11].metricIndices = new int[6] { 23, 55, 24, 13, 14, 50 };
        plots[11].temporal = true;
        plots[11].userSetBounds = true;
        plots[11].XYZbounds[0][0] = 28; plots[11].XYZbounds[1][0] = 0; plots[11].XYZbounds[2][0] = 0;
        plots[11].XYZbounds[0][1] = 47.85f; plots[11].XYZbounds[1][1] = 80; plots[11].XYZbounds[2][1] = 80;
        plots[11].whiteBackground = true;
        plots[11].axisLabels = new string[3]{"t(hpf)", "DV(μm)", "" };
        plots[11].NrClasses = new int[3]{4, 5, 5};
        //*/

        /*
        // ***** Proliferation Distribution ***** //
        tuples = timeSeries.DataFrameToTuples();
        // 1st Plot: Rotated Points
        //plots[0].metricIndices = new int[6] { 16, 17, 18, 13, 14, 50 };
        //plots[0].defaultPrefab = false;

        // 5th Plot: Proliferation Distribution 3D
        plots[0].metricIndices = new int[6] { 16, 17, 18, 13, 14, 50 }; //19, 43, 35
        plots[0].colorGradient = true;
        plots[0].axisLabels = new string[3]{"AP(μm)", "PD(μm)", "" };
        plots[0].NrClasses = new int[3]{4, 4, 3};
        plots[0].XYZbounds[0][0] = 0; plots[0].XYZbounds[1][0] = 0; plots[0].XYZbounds[2][0] = 0;
        plots[0].XYZbounds[0][1] = 80; plots[0].XYZbounds[1][1] = 56; plots[0].XYZbounds[2][1] = 80;
        plots[0].userSetBounds = true;
        

        // 6th Plot: Proliferation Distribution 3D
        plots[1].metricIndices = new int[6] { 17, 16, 18, 13, 14, 51 };
        plots[1].colorGradient = true;
        plots[1].axisLabels = new string[3]{"PD(μm)", "AP(μm)", "" };
        plots[1].NrClasses = new int[3]{4, 4, 3};
        plots[1].XYZbounds[0][0] = 0; plots[1].XYZbounds[1][0] = 0; plots[1].XYZbounds[2][0] = 0;
        plots[1].XYZbounds[0][1] = 56; plots[1].XYZbounds[1][1] = 80; plots[1].XYZbounds[2][1] = 80;
        plots[1].userSetBounds = true;
        

        // 7th Plot: Proliferation Distribution 3D
        plots[2].metricIndices = new int[6] { 18, 17, 16, 13, 14, 52 };
        plots[2].colorGradient = true;
        plots[2].axisLabels = new string[3]{"DV(μm)", "PD(μm)", "" };
        plots[2].NrClasses = new int[3]{4, 4, 3};
        plots[2].XYZbounds[0][0] = 0; plots[2].XYZbounds[1][0] = 0; plots[2].XYZbounds[2][0] = 0;
        plots[2].XYZbounds[0][1] = 80; plots[2].XYZbounds[1][1] = 56; plots[2].XYZbounds[2][1] = 80;
        plots[2].userSetBounds = true;

        // 2nd Plot: Proliferation Distribution X
        plots[3].metricIndices = new int[6] { 19, 20, 24, 13, 14, 50 };
        plots[3].defaultPrefab = false;
        plots[3].whiteBackground = true;
        plots[3].axisLabels = new string[3]{"AP(μm)", "Number of divisions", "" };
        plots[3].NrClasses = new int[3]{4, 3, 3};
        plots[3].XYZbounds[0][0] = 0; plots[3].XYZbounds[1][0] = 0; plots[3].XYZbounds[2][0] = 0;
        plots[3].XYZbounds[0][1] = 80; plots[3].XYZbounds[1][1] = 25; plots[3].XYZbounds[2][1] = 80;
        plots[3].userSetBounds = true;
        
        // 3rd Plot: Proliferation Distribution Y
        plots[4].metricIndices = new int[6] { 43, 44, 24, 13, 14, 51 };
        plots[4].defaultPrefab = false;
        plots[4].whiteBackground = true;
        plots[4].axisLabels = new string[3]{"PD(μm)", "Number of divisions", "" };
        plots[4].NrClasses = new int[3]{4, 3, 3};
        //plots[4].flipXYAxis = true;
        plots[4].XYZbounds[0][0] = 0; plots[4].XYZbounds[1][0] = 0; plots[4].XYZbounds[2][0] = 0;
        plots[4].XYZbounds[0][1] = 56; plots[4].XYZbounds[1][1] = 25; plots[4].XYZbounds[2][1] = 80;
        plots[4].userSetBounds = true;

        // 4th Plot: Proliferation Distribution Z
        plots[5].metricIndices = new int[6] { 45, 46, 24, 13, 14, 52 };
        plots[5].defaultPrefab = false;
        plots[5].whiteBackground = true;
        plots[5].axisLabels = new string[3]{"DV(μm)", "Number of divisions", "" };
        plots[5].NrClasses = new int[3]{4, 3, 3};
        plots[5].XYZbounds[0][0] = 0; plots[5].XYZbounds[1][0] = 0; plots[5].XYZbounds[2][0] = 0;
        plots[5].XYZbounds[0][1] = 80; plots[5].XYZbounds[1][1] = 25; plots[5].XYZbounds[2][1] = 80;
        plots[5].userSetBounds = true;

        // 2nd Plot: Proliferation Distribution X - Relative
        plots[6].metricIndices = new int[6] { 67, 70, 24, 13, 14, 50 };
        plots[6].defaultPrefab = false;
        plots[6].whiteBackground = true;
        plots[6].axisLabels = new string[3]{"AP(%)", "Number of divisions", "" };
        plots[6].NrClasses = new int[3]{4, 3, 3};
        plots[6].XYZbounds[0][0] = 0; plots[6].XYZbounds[1][0] = 0; plots[6].XYZbounds[2][0] = 0;
        plots[6].XYZbounds[0][1] = 100; plots[6].XYZbounds[1][1] = 25; plots[6].XYZbounds[2][1] = 80;
        plots[6].userSetBounds = true;

        // 3rd Plot: Proliferation Distribution Y - Relative
        plots[7].metricIndices = new int[6] { 68, 71, 24, 13, 14, 51 };
        plots[7].defaultPrefab = false;
        plots[7].whiteBackground = true;
        plots[7].axisLabels = new string[3]{"PD(%)", "Number of divisions", "" };
        plots[7].NrClasses = new int[3]{4, 3, 3};
        //plots[7].flipXYAxis = true;
        plots[7].XYZbounds[0][0] = 0; plots[7].XYZbounds[1][0] = 0; plots[7].XYZbounds[2][0] = 0;
        plots[7].XYZbounds[0][1] = 100; plots[7].XYZbounds[1][1] = 25; plots[7].XYZbounds[2][1] = 80;
        plots[7].userSetBounds = true;

        // 4th Plot: Proliferation Distribution Z - Relative
        plots[8].metricIndices = new int[6] { 69, 72, 24, 13, 14, 52 };
        plots[8].defaultPrefab = false;
        plots[8].whiteBackground = true;
        plots[8].axisLabels = new string[3]{"DV(%)", "Number of divisions", "" };
        plots[8].NrClasses = new int[3]{4, 3, 3};
        plots[8].XYZbounds[0][0] = 0; plots[8].XYZbounds[1][0] = 0; plots[8].XYZbounds[2][0] = 0;
        plots[8].XYZbounds[0][1] = 100; plots[8].XYZbounds[1][1] = 25; plots[8].XYZbounds[2][1] = 80;
        plots[8].userSetBounds = true;

        // 2nd Plot: Proliferation Distribution X Vs Cumulated
        plots[9].metricIndices = new int[6] { 19, 61, 24, 13, 14, 50 };
        plots[9].defaultPrefab = false;
        plots[9].whiteBackground = true;
        plots[9].axisLabels = new string[3]{"AP(μm)", "Frequency of divisions (‰)", "" };
        plots[9].NrClasses = new int[3]{4, 3, 3};
        plots[9].XYZbounds[0][0] = 0; plots[9].XYZbounds[1][0] = 0; plots[9].XYZbounds[2][0] = 0;
        plots[9].XYZbounds[0][1] = 80; plots[9].XYZbounds[1][1] = 10; plots[9].XYZbounds[2][1] = 80;
        plots[9].userSetBounds = true;

        // 3rd Plot: Proliferation Distribution Y Vs Cumulated
        plots[10].metricIndices = new int[6] { 43, 62, 24, 13, 14, 51 };
        plots[10].defaultPrefab = false;
        plots[10].whiteBackground = true;
        plots[10].axisLabels = new string[3]{"PD(μm)", "Frequency of divisions (‰)", "" };
        plots[10].NrClasses = new int[3]{4, 3, 3};
        //plots[10].flipXYAxis = true;
        plots[10].XYZbounds[0][0] = 0; plots[10].XYZbounds[1][0] = 0; plots[10].XYZbounds[2][0] = 0;
        plots[10].XYZbounds[0][1] = 56; plots[10].XYZbounds[1][1] = 10; plots[10].XYZbounds[2][1] = 80;
        plots[10].userSetBounds = true;

        // 4th Plot: Proliferation Distribution Z Vs Cumulated
        plots[11].metricIndices = new int[6] { 45, 63, 24, 13, 14, 52 };
        plots[11].defaultPrefab = false;
        plots[11].whiteBackground = true;
        plots[11].axisLabels = new string[3]{"DV(μm)", "Frequency of divisions (‰)", "" };
        plots[11].NrClasses = new int[3]{4, 3, 3};
        plots[11].XYZbounds[0][0] = 0; plots[11].XYZbounds[1][0] = 0; plots[11].XYZbounds[2][0] = 0;
        plots[11].XYZbounds[0][1] = 80; plots[11].XYZbounds[1][1] = 10; plots[11].XYZbounds[2][1] = 80;
        plots[11].userSetBounds = true;

        // 2nd Plot: Proliferation Distribution X Vs Initial
        plots[12].metricIndices = new int[6] { 19, 56, 24, 13, 14, 50 };
        plots[12].defaultPrefab = false;
        plots[12].whiteBackground = true;
        plots[12].axisLabels = new string[3]{"AP(μm)", "Frequency of divisions", "" };
        plots[12].NrClasses = new int[3]{4, 3, 3};
        plots[12].XYZbounds[0][0] = 0; plots[12].XYZbounds[1][0] = 0; plots[12].XYZbounds[2][0] = 0;
        plots[12].XYZbounds[0][1] = 80; plots[12].XYZbounds[1][1] = 600; plots[12].XYZbounds[2][1] = 80;
        plots[12].userSetBounds = true;

        // 3rd Plot: Proliferation Distribution Y Vs Initial
        plots[13].metricIndices = new int[6] { 43, 57, 24, 13, 14, 51 };
        plots[13].defaultPrefab = false;
        plots[13].whiteBackground = true;
        plots[13].axisLabels = new string[3]{"PD(μm)", "Frequency of divisions", "" };
        plots[13].NrClasses = new int[3]{4, 3, 3};
        //plots[13].flipXYAxis = true;
        plots[13].XYZbounds[0][0] = 0; plots[13].XYZbounds[1][0] = 0; plots[13].XYZbounds[2][0] = 0;
        plots[13].XYZbounds[0][1] = 56; plots[13].XYZbounds[1][1] = 600; plots[13].XYZbounds[2][1] = 80;
        plots[13].userSetBounds = true;

        // 4th Plot: Proliferation Distribution Z Vs Initial
        plots[14].metricIndices = new int[6] { 45, 58, 24, 13, 14, 52 };
        plots[14].defaultPrefab = false;
        plots[14].whiteBackground = true;
        plots[14].axisLabels = new string[3]{"DV(μm)", "Frequency of divisions", "" };
        plots[14].NrClasses = new int[3]{4, 3, 3};
        plots[14].XYZbounds[0][0] = 0; plots[14].XYZbounds[1][0] = 0; plots[14].XYZbounds[2][0] = 0;
        plots[14].XYZbounds[0][1] = 80; plots[14].XYZbounds[1][1] = 600; plots[14].XYZbounds[2][1] = 80;
        plots[14].userSetBounds = true;
        //*/
    }

    // Update is called once per frame

    public int[] resetValues;
        
    void Update () {
        if (reset)
        {
            play = false;
            reset = false;

            for (int i = 0; i < NrPlots; i++)
            {
                frame = 0;
                //plots[i].frame = 0;
                plots[i].frame = resetValues[i];
                plots[i].PlayMetrics(plots[i].frame);
            }
        }

        if (enhancePlot)
        {
            for (int i = 0; i < NrPlots; i++)
            {
                plots[i].enhancePlot = enhancePlot;
            }
        }else{
            for (int i = 0; i < NrPlots; i++)
            {
                //plots[i].enhancePlot = enhancePlot;
            }
        }

        for (int i = 0; i < NrPlots; i++)
        {
            plots[i].play = play;
            plots[i].reset = reset;
            frame = plots[i].frame;
        }
    }

    public void SetNSquaredInN(int dimX, int dimY)
    {
        NSquaredInN = new Vector3[dimX * dimY];
        int i = 0;
        for (int x = 0; x < dimX; x++)
        {
            for (int y = 0; y < dimY; y++)
            {
                //NSquaredInN[i] = new Vector3(x, -y, 0);
                NSquaredInN[i] = new Vector3(y, -x, 0);
                i++;
            }
        }
    }
}