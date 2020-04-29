using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using System.Linq;

public class MorphogenesisTimeSeries : DataInterface {

    public MorphogenesisTimeSeries(string folder, string file, string paramsFile)
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

        Duplicate();

        Init();
        for (int fr = 0; fr < NrFrames; fr++)
        {
            //FindNeighbours(fr);
            ComputeFinOrientation(fr);
        }

        ComputeBoundingBox();
    }    
}