using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using System.Linq;

public class MovitTimeSeries2 : MonoBehaviour {

    int nbOfFrames;
    [Range(0, 516)]
    public int frame = 0;
    public int speed = 1;
    public bool play = true;
    public bool reset = false;
    public bool displayCells = false;

    string folder = "C:/Users/18003111/OneDrive - MMU/Documents/movit/application.windows64/MovitData/distant/180420hZ";
    string file = "MovitTimeSeries";

    float Xscale = 1;
    float Yscale = 1;
    float Zscale = 1;
    float scale = .25f;

    float XLength = 512;
    float YLength = 512;
    float ZLength = 512;

    int[] startOfFrame;
    int[] nbOfCellsPerFrame;
    Dictionary<int, CellData>[] dataPerCellPerFrame;
    string[][] allData;
    CellData[][] dataFrame;

    //Vector3[] axis;
    Vector3[][] axis;
    Vector3[][] axis1;
    Vector3[] centre;

    Vertex3[][] vertices;
    private DelaunayTriangulation3 delaunay;
    public float size = 50f;

    public CellGO cellPrefab;
    List<CellGO> cells;

    List<CellData> persistantCellData;
    Color[] selectionColors;

    // Use this for initialization
    void Start() {
        ViewerStart();
        Duplicate();

        //nbOfFrames = 50;

        for (int fr=0; fr<nbOfFrames-1; fr++)
        {
            FindNeighbours(fr);
            ComputeFinOrientation(fr);
            //ComputeGrowthOrientation(fr);
            //ComputeAxis(fr);

            //ComputeOrientation(fr);
            //ComputeOrientationDistribution(fr);
        }
        
        //Debug.Log("Hello: " + nbOfFrames);    
        CreateCellPopulation();
        //Log();
    }

    // Update is called once per frame
    void Update() {
        int count = 0;
        if (play)
        {
            while (frame < nbOfFrames-1 && count++ < speed)
            {
                PlayResults(frame);
                frame++;
            }
            if (reset)
            {
                reset = false;
                play = false;
                frame = 0;
                PlayResults(frame);
            }
        }

        if (frame == 0)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                //cells[i].GetComponent<MeshRenderer>().enabled = displayCells;
            }
        }
    }

    public void ViewerStart()
    {
        //Debug.Log("Hello");
        selectionColors = new Color[]
        {
            Color.white,
            Color.white,
            Color.white,
            Color.white,
            Color.white,
            Color.white,
            Color.yellow,
            Color.green,
            Color.blue,
            new Color(255, 165, 0),
            new Color(238,130,238)
        };

        Debug.Log("Reading parameters ...");
        ReadParameters();

        Debug.Log("Reading results ...");
        ReadResults();
    }

    Vector3[] principalAxis;
    Matrix3x3 finMatrix;
    public Vector3 lowestPoint = new Vector3();
    public void ComputeFinOrientation(int fr){
        centre[fr] = new Vector3();

        int countActive = 0;

        float nCovXY = 0;
        float nCovXZ = 0;
        float nCovYZ = 0;
        float nVarX = 0;
        float nVarY = 0;
        float nVarZ = 0;

        Vector3 meanPoint = new Vector3();
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                meanPoint += dataFrame[fr][i].position;
                countActive++;
            }
        }
        meanPoint /= countActive;
        centre[fr] = meanPoint;

        if(fr <= 400){
            countActive = 0;
            for (int i=0; i<dataFrame[fr].Length; i++)
            {
                if(dataFrame[fr][i].active == 1)
                {
                    nCovXY += (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.y - meanPoint.y);
                    nCovXZ += (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.z - meanPoint.z);
                    nCovYZ += (dataFrame[fr][i].position.y - meanPoint.y) * (dataFrame[fr][i].position.z - meanPoint.z);
                    nVarX += (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.x - meanPoint.x);
                    nVarY += (dataFrame[fr][i].position.y - meanPoint.y) * (dataFrame[fr][i].position.y - meanPoint.y);
                    nVarZ += (dataFrame[fr][i].position.z - meanPoint.z) * (dataFrame[fr][i].position.z - meanPoint.z);

                    countActive++;
                }
            }

            finMatrix = new Matrix3x3(nVarX / countActive, nCovXY / countActive, nCovXZ / countActive,
                                                nCovXY / countActive, nVarY / countActive, nCovYZ / countActive,
                                                nCovXZ / countActive, nCovYZ / countActive, nVarZ / countActive);

            if(fr == 0){
                axis[fr] = finMatrix.ComputeMinAxis();
            }else{
                axis[fr] = finMatrix.ComputePrincipalAxis(axis[fr - 1]);
            }        
        }else{
            axis[fr] = axis[400];
        }

        /*
		for (int i=0; i<dataFrame[fr].Length; i++)
        {
            float minDot = 1000f;
            if(dataFrame[fr][i].active == 1)
            {
                float dot = Vector3.Dot(axis[fr][0], dataFrame[fr][i].position);
                if(dot < minDot){
                    minDot = dot;
                    lowestPoint = dataFrame[fr][i].position;
                }
            }
        }
        */

        for(int i=0; i<dataFrame[fr].Length; i++)
        {
            dataFrame[fr][i].fr = fr;
            dataFrame[fr][i].globalCentre = meanPoint;
            dataFrame[fr][i].globalPrincipalAxis = axis[fr][0];
            dataFrame[fr][i].globalSecondaryAxis = axis[fr][1];
            dataFrame[fr][i].globalTertiaryAxis = axis[fr][2];
            dataFrame[fr][i].rotatedPosition = finMatrix.R.Transpose() * (dataFrame[fr][i].position - centre[fr]) + centre[fr];
            //float tmp = dataFrame[fr][i].rotatedPosition.y;
            //dataFrame[fr][i].rotatedPosition.y = dataFrame[fr][i].rotatedPosition.z;
            //dataFrame[fr][i].rotatedPosition.z = tmp;
        }
    }

    public void ComputeFinOrientation2(int fr)
    {
        centre[fr] = new Vector3();

        int countActive = 0;

        float nCovXY = 0;
        float nCovXZ = 0;
        float nCovYZ = 0;
        float nVarX = 0;
        float nVarY = 0;
        float nVarZ = 0;

        Vector3 meanPoint = new Vector3();
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                meanPoint += dataFrame[fr][i].position;
                countActive++;
            }
        }
        meanPoint /= countActive;
        centre[fr] = meanPoint;

        countActive = 0;
        for (int i=0; i<dataFrame[fr].Length; i++)
        {
            if(dataFrame[fr][i].active == 1)
            {
                nCovXY += (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.y - meanPoint.y);
                nCovXZ += (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.z - meanPoint.z);
                nCovYZ += (dataFrame[fr][i].position.y - meanPoint.y) * (dataFrame[fr][i].position.z - meanPoint.z);
                nVarX += (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.x - meanPoint.x);
                nVarY += (dataFrame[fr][i].position.y - meanPoint.y) * (dataFrame[fr][i].position.y - meanPoint.y);
                nVarZ += (dataFrame[fr][i].position.z - meanPoint.z) * (dataFrame[fr][i].position.z - meanPoint.z);

                countActive++;
            }
        }

        Matrix3x3 matrix = new Matrix3x3(nVarX / countActive, nCovXY / countActive, nCovXZ / countActive,
                                            nCovXY / countActive, nVarY / countActive, nCovYZ / countActive,
                                            nCovXZ / countActive, nCovYZ / countActive, nVarZ / countActive);

        if (fr == 0)
        {
            principalAxis = matrix.ComputeMinAxis();
            axis[fr] = principalAxis;
        }
        else if (fr <= 400)
        {
            axis[fr] = matrix.ComputePrincipalAxis(axis[fr-1]);
            //axis[fr][2] = matrix.ComputePrincipalAxis(axis[fr-1][2]);
        }
        else
        {
            axis[fr] = axis[400];
        }
        //400
        
        for(int i=0; i<dataFrame[fr].Length; i++)
        {
            dataFrame[fr][i].fr = fr;
            dataFrame[fr][i].globalCentre = meanPoint;
            dataFrame[fr][i].globalPrincipalAxis = axis[fr][0];
            dataFrame[fr][i].globalSecondaryAxis = axis[fr][1];
            dataFrame[fr][i].globalTertiaryAxis = axis[fr][2];
            dataFrame[fr][i].position = matrix.R.Transpose() * (dataFrame[fr][i].position - centre[fr]) + centre[fr];
        }
    }

    public void ComputeGrowthOrientation(int fr)
    {
        //Debug.Log(fr);
        centre[fr] = new Vector3();
        int countActive = 0;

        float nCovXY = 0;
        float nCovXZ = 0;
        float nCovYZ = 0;
        float nVarX = 0;
        float nVarY = 0;
        float nVarZ = 0;

        //*
        Vector3 meanVelocity = new Vector3();
        Vector3 meanPoint2 = new Vector3();
        Vector3 meanPoint = new Vector3();
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                meanVelocity += dataFrame[fr][i].velocity;
                meanPoint2 += dataFrame[fr][i].position;
                countActive++;
            }
        }
        meanVelocity /= countActive;
        meanPoint2 /= countActive;
        meanPoint = meanVelocity;
        centre[fr] = meanPoint2;
        //*/

        //*
        //Vector3 meanVelocity = new Vector3();        
        //Vector3 meanPoint = new Vector3();
        //Vector3 meanPoint2 = new Vector3();
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                /*
                Vector3 meanVelocity = new Vector3();
                for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
                {
                    meanVelocity += dataFrame[fr][dataFrame[fr][i].neighbours[j]].velocity;
                }
                meanVelocity /= (dataFrame[fr][i].neighbours.Count);
                //*/

                dataFrame[fr][i].relativeVelocity = dataFrame[fr][i].velocity - meanVelocity;
                meanPoint += dataFrame[fr][i].relativeVelocity;
                meanPoint2 += dataFrame[fr][i].position;
                countActive++;
            }
        }
        //meanPoint /= countActive;
        //meanPoint2 /= countActive;
        //centre[fr] = meanPoint2;
        //*/

        countActive = 0;
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                nCovXY += (dataFrame[fr][i].relativeVelocity.x - meanPoint.x) * (dataFrame[fr][i].relativeVelocity.y - meanPoint.y);
                nCovXZ += (dataFrame[fr][i].relativeVelocity.x - meanPoint.x) * (dataFrame[fr][i].relativeVelocity.z - meanPoint.z);
                nCovYZ += (dataFrame[fr][i].relativeVelocity.y - meanPoint.y) * (dataFrame[fr][i].relativeVelocity.z - meanPoint.z);
                nVarX += (dataFrame[fr][i].relativeVelocity.x - meanPoint.x) * (dataFrame[fr][i].relativeVelocity.x - meanPoint.x);
                nVarY += (dataFrame[fr][i].relativeVelocity.y - meanPoint.y) * (dataFrame[fr][i].relativeVelocity.y - meanPoint.y);
                nVarZ += (dataFrame[fr][i].relativeVelocity.z - meanPoint.z) * (dataFrame[fr][i].relativeVelocity.z - meanPoint.z);

                countActive++;
            }
        }

        Matrix3x3 matrix = new Matrix3x3(nVarX / countActive, nCovXY / countActive, nCovXZ / countActive,
                                            nCovXY / countActive, nVarY / countActive, nCovYZ / countActive,
                                            nCovXZ / countActive, nCovYZ / countActive, nVarZ / countActive);


        axis1[fr] = matrix.ComputeMinAxis();
        //axis[fr] = meanPoint;// matrix.ComputePrincipalAxis();
    }

    public void ComputeAxis(int fr){
        if(fr > 0){
            float staticDot = Vector3.Dot(axis[fr][0], axis[fr-1][0]);
            float growthDot = Vector3.Dot(axis1[fr][2], axis[fr-1][0]);
            //Debug.Log(axis[fr][0]);
            /*
            if(Mathf.Abs(staticDot) < Mathf.Abs(growthDot)){
                axis[fr][2] = axis1[fr][0];
                axis[fr][1] = axis1[fr][1];
                axis[fr][0] = axis1[fr][2];
            }
            */

            if(fr==500){
                axis[fr][2] = axis1[fr][0];
                axis[fr][1] = axis1[fr][1];
                axis[fr][0] = axis1[fr][2];
            }
            //*
            if(fr > 0){// && Mathf.Abs(staticDot) < Mathf.Abs(growthDot)){
                //axis[fr][2] = axis1[fr][0];
                //axis[fr][1] = axis1[fr][1];
                //axis[fr][0] = axis1[fr][2];
            }
            //*/
        }
    }

    public void ComputeOrientation(int fr)
    {
        //axis[fr] = new Vector3();
        //centre[fr] = new Vector3();
        int countActive = 0;
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            Vector3 meanPoint = dataFrame[fr][i].position;
            //Debug.Log(fr + ", " + i + ", " + dataFrame[fr][i].neighbours.Count);
            for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
            {
                //Debug.Log(dataFrame[fr][i].neighbours[j]);
                meanPoint += dataFrame[fr][dataFrame[fr][i].neighbours[j]].position;
            }
            meanPoint /= (1 + dataFrame[fr][i].neighbours.Count);

            if (dataFrame[fr][i].active == 1)
            {
                float nCovXY = (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.y - meanPoint.y);
                float nCovXZ = (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.z - meanPoint.z);
                float nCovYZ = (dataFrame[fr][i].position.y - meanPoint.y) * (dataFrame[fr][i].position.z - meanPoint.z);
                float nVarX = (dataFrame[fr][i].position.x - meanPoint.x) * (dataFrame[fr][i].position.x - meanPoint.x);
                float nVarY = (dataFrame[fr][i].position.y - meanPoint.y) * (dataFrame[fr][i].position.y - meanPoint.y);
                float nVarZ = (dataFrame[fr][i].position.z - meanPoint.z) * (dataFrame[fr][i].position.z - meanPoint.z);

                for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
                {
                    int k = dataFrame[fr][i].neighbours[j];
                    nCovXY += (dataFrame[fr][k].position.x - meanPoint.x) * (dataFrame[fr][k].position.y - meanPoint.y);
                    nCovXZ += (dataFrame[fr][k].position.x - meanPoint.x) * (dataFrame[fr][k].position.z - meanPoint.z);
                    nCovYZ += (dataFrame[fr][k].position.y - meanPoint.y) * (dataFrame[fr][k].position.z - meanPoint.z);

                    nVarX += (dataFrame[fr][k].position.x - meanPoint.x) * (dataFrame[fr][k].position.x - meanPoint.x);
                    nVarY += (dataFrame[fr][k].position.y - meanPoint.y) * (dataFrame[fr][k].position.y - meanPoint.y);
                    nVarZ += (dataFrame[fr][k].position.z - meanPoint.z) * (dataFrame[fr][k].position.z - meanPoint.z);
                }

                int n = 1 + dataFrame[fr][i].neighbours.Count;
                //n = 1 + dataFrame[fr][i].neighbours.Count;

                dataFrame[fr][i].matrix = new Matrix3x3(nVarX / n, nCovXY / n, nCovXZ / n,
                                                        nCovXY / n, nVarY / n, nCovYZ / n,
                                                        nCovXZ / n, nCovYZ / n, nVarZ / n);


                dataFrame[fr][i].ComputePrincipalAxis(axis[fr][2], centre[fr]);
                countActive++;
            }
        }
    }

    public void ComputeOrientationDistribution(int fr)
    {
        int numberOfClasses = 30;
        
        float[] classes = new float[numberOfClasses + 1];
        int[] frequencies = new int[numberOfClasses];
        for(int i=0; i<numberOfClasses+1; i++)
        {
            classes[i] = Mathf.PI * i / (2*numberOfClasses);
        }

        double minAngle = 0;
        double maxAngle = Mathf.PI/2;
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if(dataFrame[fr][i].active == 1)
            {
                int j = 0;
                bool isInClass = false;
                while (j < numberOfClasses && !isInClass)
                {
                    if (dataFrame[fr][i].elongationAngle >= classes[j]
                        && dataFrame[fr][i].elongationAngle < classes[j + 1])
                    {
                        isInClass = true;
                        dataFrame[fr][i].position.x = 10 * (classes[j] + classes[j + 1]) / 2;
                        dataFrame[fr][i].cluster = j;
                        frequencies[j]++;
                    }
                    j++;
                }
            }
        }

        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            dataFrame[fr][i].position.y = frequencies[dataFrame[fr][i].cluster];
            dataFrame[fr][i].position.z = 0;
        }

        int max = 0;
        int maxIndex = 0;
        for(int i=0; i<frequencies.Length; i++)
        {
            if(frequencies[i] > max)
            {
                max = frequencies[i];
                maxIndex = i;
            }
        }

        //Debug.Log(fr + ", " + maxIndex + ":" + max);
    }
    
    public void CreateCellPopulation()
    {
        int fr = 0;
        //cells = new Dictionary<int, CellGO>();
        cells = new List<CellGO>();
        int count = 0;
        Debug.Log(dataFrame[fr].Length);
        for(int i=0; i<dataFrame[fr].Length; i++)
        {
            CellGO cell = Instantiate(cellPrefab);
            cell.globalId = dataFrame[fr][i].globalId;

            cell.motherId = dataFrame[fr][i].mother;
            //cell.position = dataFrame[fr][i].position;
            cell.position = new Vector3(dataFrame[fr][i].elongationAngle,0,0);
            cell.selection = dataFrame[fr][i].selection;
            
            cell.name = "Cell_" + count;
            cell.transform.SetParent(transform);
            cell.transform.localPosition = dataFrame[fr][i].position;
            //cell.transform.localPosition = new Vector3(dataFrame[fr][i].elongationAngle, 0, 0);

            cell.GetComponent<MeshRenderer>().material.color = selectionColors[dataFrame[fr][i].selection - 1];

            //cell.GetComponent<MeshRenderer>().enabled = displayCells;

            cells.Add(cell);
            //Debug.Log(count);
            count++;
        }
    }
    
    Vector3[] matrices;
    public int activePeriod = 50;
    
    public void PlayResults(int fr)
    {
        if (fr == 0)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }

        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].motherId = dataFrame[fr][i].mother;
            cells[i].globalId = dataFrame[fr][i].mother;
            cells[i].transform.localPosition = dataFrame[fr][i].position;
            //cells[i].transform.localPosition = new Vector3 (dataFrame[fr][i].elongationAngle,0,0);

            if (fr > 0 && dataFrame[fr][i].active == 0)
            {
                //cells[i].GetComponent<MeshRenderer>().enabled = false;
            }

            if (fr > 0 && dataFrame[fr-1][i].active == 1)
            {
                //dataFrame[fr][i].timeActive = dataFrame[fr-1][i].timeActive + 1;
            }

            //Debug.Log(dataFrame[fr][i].timeActive);

            if (dataFrame[fr][i].timeActive > activePeriod){
                //cells[i].GetComponent<MeshRenderer>().material.color = Color.white;
                //cells[i].GetComponent<MeshRenderer>().enabled = false;
            }

            if (fr > 0 && dataFrame[fr][i].active > dataFrame[fr - 1][i].active)
            {
                //cells[i].GetComponent<MeshRenderer>().material.color = Color.green;
                //cells[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public void ReadParameters()
    {
        string paramsFile = folder + "/180420hZ_21361.xml";

        XmlReader reader = XmlReader.Create(paramsFile);
        while (reader.Read())
        {
            if (reader.IsStartElement())
            {
                nbOfFrames = int.Parse(reader["nbTimesteps"]) - 1;
                //nbOfFrames = 423;
                Xscale = float.Parse(reader["db_pixelSizeX"]);
                Yscale = float.Parse(reader["db_pixelSizeY"]);
                Zscale = float.Parse(reader["db_pixelSizeZ"]);

                float XLength = float.Parse(reader["db_nbPixelX"]);
                float YLength = float.Parse(reader["db_nbPixelY"]);
                float ZLength = float.Parse(reader["db_nbPixelZ"]);
                //nbOfFrames = 2;
            }
        }
    }

    public void ReadResults()
    {
        string logFile = folder + "/test_06012020.csv";//"/test_06012020.csv";
        //string logFile = folder + "/190425aZ_t_all_T.csv";//"/test.csv";

        string[] results = File.ReadAllLines(logFile);

        vertices = new Vertex3[nbOfFrames][];
        nbOfCellsPerFrame = new int[nbOfFrames];
        allData = new string[results.Length - 1][];

        for (int i = 1; i < results.Length; i++)
        {
            if (!String.IsNullOrEmpty(results[i]))
            {
                allData[i - 1] = new string[8];
                allData[i - 1][0] = results[i].Split(';')[0];
                allData[i - 1][1] = results[i].Split(';')[1];
                allData[i - 1][2] = results[i].Split(';')[2];
                allData[i - 1][3] = results[i].Split(';')[3];
                allData[i - 1][4] = results[i].Split(';')[4];
                allData[i - 1][5] = results[i].Split(';')[5];
                allData[i - 1][6] = results[i].Split(';')[6];
                allData[i - 1][7] = results[i].Split(';')[7];
            }
        }

        int line = 0;
        while (line < allData.Length)
        {
            int lineFr = int.Parse(allData[line][2]);

            if (lineFr <= nbOfFrames)
                nbOfCellsPerFrame[lineFr - 1]++;

            line++;
        }
        //Debug.Log("Cells per frame : " + nbOfCellsPerFrame[nbOfFrames - 1]);

        int start = 0;
        dataPerCellPerFrame = new Dictionary<int, CellData>[nbOfFrames];
        for (int fr = 1; fr <= nbOfFrames; fr++)
        {
            //Debug.Log(" fr: " + (fr - 1) + ", " + nbOfCellsPerFrame[fr-1]);
            vertices[fr - 1] = new Vertex3[nbOfCellsPerFrame[fr - 1]];
            dataPerCellPerFrame[fr - 1] = new Dictionary<int, CellData>(nbOfCellsPerFrame[fr - 1]);

            if (fr > 1) start += nbOfCellsPerFrame[fr - 2];

            for (int i = 0; i < nbOfCellsPerFrame[fr - 1]; i++)
            {
                int index = start + i;

                vertices[fr - 1][i] = new Vertex3(float.Parse(allData[index][3]) * Xscale * scale,
                                          float.Parse(allData[index][4]) * Yscale * scale,
                                          float.Parse(allData[index][5]) * Zscale * scale
                              );

                dataPerCellPerFrame[fr - 1].Add(int.Parse(allData[index][0]),
                                            new CellData(int.Parse(allData[index][0]),int.Parse(allData[index][6]),
                                                          int.Parse(allData[index][1]),
                                                            new Vector3(
                                                                float.Parse(allData[index][3]) * Xscale * scale,
                                                                float.Parse(allData[index][4]) * Yscale * scale,
                                                                float.Parse(allData[index][5]) * Zscale * scale
                                                            )
                                            )
                                      );
            }
        }

        for (int fr = 0; fr < nbOfFrames - 1; fr++)
        {
            int count = 0;
            foreach (KeyValuePair<int, CellData> item1 in dataPerCellPerFrame[fr])
            {
                int childCount = -1;

                foreach (KeyValuePair<int, CellData> item2 in dataPerCellPerFrame[fr + 1])
                {
                    if (item2.Value.mother == item1.Key)
                    {
                        dataPerCellPerFrame[fr][item1.Key].children[++childCount] = item2.Key;
                    }
                }
            }
        }
    }

    public void Duplicate()
    {
        //nbOfFrames = 5;
        dataFrame = new CellData[nbOfFrames][];
        axis = new Vector3[nbOfFrames][];
        axis1 = new Vector3[nbOfFrames][];
        centre = new Vector3[nbOfFrames];
        for (int fr = nbOfFrames - 1; fr >= 0; fr--)
        {
            dataFrame[fr] = new CellData[nbOfCellsPerFrame[nbOfFrames - 1]];
        }

        // Last frame
        int i = 0;
        foreach (KeyValuePair<int, CellData> item1 in dataPerCellPerFrame[nbOfFrames - 1])
        {
            dataFrame[nbOfFrames - 1][i] = dataPerCellPerFrame[nbOfFrames - 1][item1.Key];
            dataFrame[nbOfFrames - 1][i].globalId = item1.Key;
            dataFrame[nbOfFrames - 1][i].velocity = new Vector3();
            dataFrame[nbOfFrames - 1][i].ID = i;
            dataFrame[nbOfFrames - 1][i].active = 1;
            dataFrame[nbOfFrames - 1][i].fr = nbOfFrames - 1;
            dataFrame[nbOfFrames - 1][i].selection = item1.Value.selection;
            dataPerCellPerFrame[nbOfFrames - 1][item1.Key].ID = i;

            int j = 0;
            foreach (KeyValuePair<int, CellData> item2 in dataPerCellPerFrame[nbOfFrames - 2])
            {
                if (item1.Value.mother == item2.Key)
                {
                    dataFrame[nbOfFrames - 1][i].uniqueMotherID = j;
                }
                j++;
            }
            i++;
        }

        for (int fr = nbOfFrames - 2; fr >= 0; fr--)
        {
            List<int> activeList = new List<int>();
            for (int j = 0; j < dataFrame[fr + 1].Length; j++)
            {
                //405: 1152713952
                //404: 1121106983
                //Debug.Log(fr + ": " + dataFrame[fr + 1][j].mother);
                if (dataPerCellPerFrame[fr].ContainsKey(dataFrame[fr + 1][j].mother))
                {
                    dataFrame[fr][j] = new CellData(dataPerCellPerFrame[fr][dataFrame[fr + 1][j].mother]);
                    dataFrame[fr][j].ID = j;
                    dataFrame[fr][j].fr = fr;
                    dataFrame[fr][j].uniqueMotherID = j;
                    dataFrame[fr][j].velocity = dataFrame[fr + 1][j].position - dataFrame[fr][j].position;
                    dataFrame[fr][j].globalId = dataPerCellPerFrame[fr][dataFrame[fr + 1][j].mother].globalId;
                    dataFrame[fr][j].active = dataFrame[fr + 1][j].active;
                    dataFrame[fr][j].selection = dataFrame[fr + 1][j].selection;
                    //dataPerCellPerFrame[fr][dataFrame[fr][j].globalId].ID = j;

                    if (dataFrame[fr][j].children[1] != -1)
                    {
                        //int child1ID = dataFrame[fr][j].children[0];
						//int child2ID = dataFrame[fr][j].children[1];
                        //Debug.Log(child1ID + "" + child2ID);
                        //dataFrame[fr][child1ID].twinID = child2ID;
                        //dataFrame[fr][child2ID].twinID = child1ID;

                        activeList.Add(j);
                    }
                }
                else
                {
                    dataFrame[fr][j] = new CellData(dataFrame[fr + 1][j]);
                    dataFrame[fr][j].fr = fr;
                    dataFrame[fr][j].uniqueMotherID = j;
                    dataFrame[fr][j].velocity = Vector3.zero;
                    dataFrame[fr][j].children[0] = dataFrame[fr][j].children[1] = -1;
                }

            }

            for (int j = 0; j < activeList.Count; j++)
            {
                for (int k = j + 1; k < activeList.Count; k++)
                {
                    if (dataFrame[fr][activeList[j]].children[0] == dataFrame[fr][activeList[k]].children[0])
                    {
                        dataFrame[fr][activeList[k]].active = 0;
                    }
                }
            }
        }

        //*
        // Initialise neighbourhood
        for (int fr = 0; fr < nbOfFrames; fr++)
        {
            for (int k = 0; k < nbOfCellsPerFrame[nbOfFrames - 1]; k++)
            {
                dataFrame[fr][k].neighbourhood = new int[nbOfCellsPerFrame[nbOfFrames - 1]];
            }
        }
        //*/

        //*
        // Compute Velocities
        int step = 1;
        for (int fr = 0; fr < nbOfFrames - step; fr++)
        {
            for (int j = 0; j < nbOfCellsPerFrame[nbOfFrames - 1]; j++)
            {
                dataFrame[fr][j].velocity = dataFrame[fr + step][j].position - dataFrame[fr][j].position;
            }
        }
        //*/
    }

    public void FindNeighbours(int fr)
    {
        // Compute Double Mean Distance Square
        float doubleMeanDistanceSquare = 0;
        foreach (KeyValuePair<int, CellData> item1 in dataPerCellPerFrame[fr])
        {
            foreach (KeyValuePair<int, CellData> item2 in dataPerCellPerFrame[fr])
            {
                doubleMeanDistanceSquare += (item1.Value.position - item2.Value.position).magnitude;
            }
        }
        doubleMeanDistanceSquare /= (nbOfCellsPerFrame[fr] * (nbOfCellsPerFrame[fr] - 1));
        doubleMeanDistanceSquare *= 2;
        doubleMeanDistanceSquare *= doubleMeanDistanceSquare;

        //Debug.Log(fr + ": " + doubleMeanDistanceSquare);

        //*
        // Create Delaunay
        delaunay = new DelaunayTriangulation3();
        delaunay.Generate(vertices[fr], true, false, doubleMeanDistanceSquare);

        for (int j = 0; j < nbOfCellsPerFrame[nbOfFrames - 1]; j++)
        {
            foreach (DelaunayCell<Vertex3> cell in delaunay.Cells)
            {
                Simplex<Vertex3> f = cell.Simplex;
                Vector3 vec = new Vector3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z);
                if ((vec - dataFrame[fr][j].position).sqrMagnitude == 0 && dataFrame[fr][j].active == 1)
                {
                    cell.Simplex.Vertices[0].Id = dataFrame[fr][j].ID;
                }

                vec = new Vector3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z);
                if ((vec - dataFrame[fr][j].position).sqrMagnitude == 0 && dataFrame[fr][j].active == 1)
                {
                    cell.Simplex.Vertices[1].Id = dataFrame[fr][j].ID;
                }

                vec = new Vector3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z);
                if ((vec - dataFrame[fr][j].position).sqrMagnitude == 0 && dataFrame[fr][j].active == 1)
                {
                    cell.Simplex.Vertices[2].Id = dataFrame[fr][j].ID;
                }

                vec = new Vector3(f.Vertices[3].X, f.Vertices[3].Y, f.Vertices[3].Z);
                if ((vec - dataFrame[fr][j].position).sqrMagnitude == 0 && dataFrame[fr][j].active == 1)
                {
                    cell.Simplex.Vertices[3].Id = dataFrame[fr][j].ID;
                }
            }
        }

        for (int j = 0; j < nbOfCellsPerFrame[nbOfFrames - 1]; j++)
        {
            foreach (DelaunayCell<Vertex3> cell in delaunay.Cells)
            {
                Simplex<Vertex3> f = cell.Simplex;
                Vector3 vec = new Vector3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z);
                if (f.Vertices[0].Id == dataFrame[fr][j].ID && dataFrame[fr][j].active == 1)
                {
                    dataFrame[fr][j].neighbourhood[f.Vertices[1].Id] = 1;
                    dataFrame[fr][j].neighbourhood[f.Vertices[2].Id] = 1;
                    dataFrame[fr][j].neighbourhood[f.Vertices[3].Id] = 1;

                    dataFrame[fr][j].neighbours.AddRange(new int[] { f.Vertices[1].Id, f.Vertices[2].Id, f.Vertices[3].Id });
                }

                if (f.Vertices[1].Id == dataFrame[fr][j].ID && dataFrame[fr][j].active == 1)
                {
                    dataFrame[fr][j].neighbourhood[f.Vertices[0].Id] = 1;
                    dataFrame[fr][j].neighbourhood[f.Vertices[2].Id] = 1;
                    dataFrame[fr][j].neighbourhood[f.Vertices[3].Id] = 1;

                    dataFrame[fr][j].neighbours.AddRange(new int[] { f.Vertices[0].Id, f.Vertices[2].Id, f.Vertices[3].Id });
                }

                if (f.Vertices[2].Id == dataFrame[fr][j].ID && dataFrame[fr][j].active == 1)
                {
                    dataFrame[fr][j].neighbourhood[f.Vertices[0].Id] = 1;
                    dataFrame[fr][j].neighbourhood[f.Vertices[1].Id] = 1;
                    dataFrame[fr][j].neighbourhood[f.Vertices[3].Id] = 1;

                    dataFrame[fr][j].neighbours.AddRange(new int[] { f.Vertices[0].Id, f.Vertices[1].Id, f.Vertices[3].Id });
                }

                if (f.Vertices[3].Id == dataFrame[fr][j].ID && dataFrame[fr][j].active == 1)
                {
                    dataFrame[fr][j].neighbourhood[f.Vertices[0].Id] = 1;
                    dataFrame[fr][j].neighbourhood[f.Vertices[1].Id] = 1;
                    dataFrame[fr][j].neighbourhood[f.Vertices[2].Id] = 1;

                    dataFrame[fr][j].neighbours.AddRange(new int[] { f.Vertices[0].Id, f.Vertices[1].Id, f.Vertices[2].Id });
                }
            }

            //Debug.Log(j + ", " + dataFrame[fr][j].neighbours.Count);
            dataFrame[fr][j].neighbours = dataFrame[fr][j].neighbours.Distinct().ToList();
            //Debug.Log(j + ", " + dataFrame[fr][j].neighbours.Count);
        }
        //*/
    }

    public void Log(){
        string dataframeFile = "dataframe.csv";
        dataframeFile = folder + "/" + dataframeFile;
        FileStream fs = new FileStream(dataframeFile, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fs);

        sw.WriteLine(CellData.Header(nbOfCellsPerFrame[nbOfFrames - 1]));

        int frequency = 10;
        int start = nbOfFrames % frequency;

        for (int fr = start; fr < nbOfFrames; fr += frequency)
        {
            for (int j = 0; j < nbOfCellsPerFrame[nbOfFrames - 1]; j++)
            {
                sw.WriteLine(dataFrame[fr][j].ToString());
            }
        }
        sw.Close();
        fs.Close();
    }

    void OnDrawGizmos()
    {
        DrawOrientation();
    }

    public bool displayAxis = true;
    public void DrawOrientation()
    {
        
        int fr = frame;
        if(fr >= nbOfFrames - 1){
            fr = nbOfFrames - 2;
        }

        //if(play){
            Gizmos.color = Color.blue;
            for (int i=0; i<dataFrame[fr].Length; i++)
            {
                /*
                Vector3 orientation = dataFrame[fr][i].axisLength * dataFrame[fr][i].principalAxis / 2;
                Gizmos.DrawLine(dataFrame[fr][i].position - orientation, 
                                    dataFrame[fr][i].position + orientation);
                //*/
            }

            Gizmos.color = Color.green;
            for (int i = 0; i < dataFrame[fr].Length; i++)
            {
                /*
                Vector3 velocity = dataFrame[fr][i].velocity;
                Gizmos.DrawLine(dataFrame[fr][i].position,
                                    dataFrame[fr][i].position + velocity);
                //*/
            }

            if(displayAxis){
                Gizmos.color = Color.red;
                Vector3 dir = 8 * axis[fr][0]; //Vector3.up;//
                //dir = 8*Vector3.up;
                Gizmos.DrawLine(centre[fr] - dir, centre[fr]+dir);

                Gizmos.color = Color.green;
                Vector3 dir1 = 8 * axis[fr][1];
                //dir1 = 8*new Vector3(0,0,1);
                //Gizmos.DrawLine(centre[fr] - dir1, centre[fr] + dir1);

                Gizmos.color = Color.yellow;
                Vector3 dir2 = 8 * axis[fr][2];
                //dir2 = 8*Vector3.right;
                //Gizmos.DrawLine(centre[fr] - dir2, centre[fr] + dir2);
            }
            
        //}
        
    }
}