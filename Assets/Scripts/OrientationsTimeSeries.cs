using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using System.Linq;

public class OrientationsTimeSeries : DataInterface {

    public OrientationsTimeSeries(string folder, string file, string paramsFile)
    {
        this.folder = folder;
        this.file = file;
        this.paramsFile = paramsFile;

        logFile = folder + "/" + file;
    }

    public override void ProcessMovitData()
    {
        Debug.Log("Reading parameters ...");
        ReadParameters();

        Debug.Log("Reading results ...");
        ReadResults();

        //ViewerStart();
        Duplicate();

        //NrFrames = 50;
        Init();
        for (int fr = 0; fr < NrFrames; fr++)
        {
            FindNeighbours(fr);
            ComputeFinOrientation(fr);
            ComputeOrientation(fr, false);
            for(int n=0; n<dataFrame[fr].Length; n++){
                dataFrame[fr][n].principalAxis =  finMatrix.R.Transpose() * dataFrame[fr][n].principalAxis;
            }
            ComputeOrientationDistribution(fr, false);
        }
        //Debug.Log(meanStr);
        
        ComputeOrientationDispersion();
        
        ComputeBoundingBox();
    }
}