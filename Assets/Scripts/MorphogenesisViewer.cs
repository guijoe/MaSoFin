using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorphogenesisViewer : ViewerController{

    public override void GetData(){
        NrPlots = 9;

        timeSeries = new MorphogenesisTimeSeries(folder, dataFile, paramsFile);
        timeSeries.ProcessMovitData();
        //timeSeries.Log();
    }

    public override void InitPlots(){
        //*
        // ***** Development description ***** //
        
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
    }
}