using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationViewer : ViewerController {

    public override void GetData(){
        NrPlots = 13;

        timeSeries = new OrientationsTimeSeries(folder, "test_06012020.csv");
        timeSeries.ProcessMovitData();
        //timeSeries.Log();
    }

    public override void InitPlots(){
        //*
        // ***** Orientation angle distribution ***** //
        
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
    }        
}