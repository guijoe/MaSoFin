using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProliferationViewer : ViewerController {

    public override void GetData(){
        NrPlots = 15;

        timeSeries = new ProliferationTimeSeries(folder, "test_06012020.csv");
        timeSeries.ProcessMovitData();
        timeSeries.Log();
    }

    public override void InitPlots(){
        //*
        // ***** Proliferation Distribution ***** //
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

}