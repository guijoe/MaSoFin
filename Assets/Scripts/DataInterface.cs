using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using System.Linq;

public abstract class DataInterface {

	protected string folder = "";
    protected string file = "";

    protected float Xscale = 1;
    protected float Yscale = 1;
    protected float Zscale = 1;
    protected float scale = .25f;

    protected float XLength = 512;
    protected float YLength = 512;
    protected float ZLength = 512;

    protected int[] startOfFrame;
    protected int[] nbOfCellsPerFrame;
    protected Dictionary<int, CellData>[] dataPerCellPerFrame;
    protected string[][] allData;
    protected CellData[][] dataFrame;

    protected Vector3[][] axis;
    protected Vector3[] centre;

    protected int NrFrames = 0;

    protected int[][][] proliferationNumbersPerArea;

    protected Vertex3[][] vertices;
    protected DelaunayTriangulation3 delaunay;
    public float size = 50f;

	Vector3 principalAxis;
    public Matrix3x3 finMatrix;

    protected List<CellData> persistantCellData;
    protected Color[] selectionColors;

    protected Vector3[] boundsMax, boundsMin, boundsSize, boundsExtents;
    protected int[][] frequencies;
    protected int[][] relativeFrequencies;

    protected int[][] frequenciesVsInitial;

    protected int[][] frequenciesVsCumulated;
    
    protected Vector3 thau;
    protected Vector3[] thauFr;
    protected int resolution = 20;
    

	public Vector3 lowestPoint = new Vector3();
    public int[] activePerFrame;


	public DataInterface()
    {
    }

	public DataInterface(string folder, String file)
    {
        this.folder = folder;
        this.file = file;

        logFile = folder + "/" + file;
    }

	public abstract void ProcessMovitData();

    public void Init(){
        overallFrequencies = new int[NrFrames][];
        activePerFrame = new int[NrFrames];
    }

	public void ReadParameters()
    {
        string paramsFile = folder + "/180420hZ_21361.xml";

        XmlReader reader = XmlReader.Create(paramsFile);
        while (reader.Read())
        {
            if (reader.IsStartElement())
            {
                NrFrames = int.Parse(reader["nbTimesteps"]) - 1;
                //NrFrames = 323;
                Xscale = float.Parse(reader["db_pixelSizeX"]);
                Yscale = float.Parse(reader["db_pixelSizeY"]);
                Zscale = float.Parse(reader["db_pixelSizeZ"]);

                float XLength = float.Parse(reader["db_nbPixelX"]);
                float YLength = float.Parse(reader["db_nbPixelY"]);
                float ZLength = float.Parse(reader["db_nbPixelZ"]);
                //NrFrames = 2;
            }
        }
    }

    protected string logFile;
    public void ReadResults()
    {
        string[] results = File.ReadAllLines(logFile);

        vertices = new Vertex3[NrFrames][];
        nbOfCellsPerFrame = new int[NrFrames];
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

            if (lineFr <= NrFrames)
                nbOfCellsPerFrame[lineFr - 1]++;

            line++;
        }
        
        int start = 0;
        dataPerCellPerFrame = new Dictionary<int, CellData>[NrFrames];
        for (int fr = 1; fr <= NrFrames; fr++)
        {
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
                                            new CellData(
                                                int.Parse(allData[index][0]),
                                                int.Parse(allData[index][6]),
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

        for (int fr = 0; fr < NrFrames - 1; fr++)
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

    public void ComputeBoundingBox()
    {
        boundsMin = new Vector3[NrFrames];
        boundsMax = new Vector3[NrFrames];
        boundsSize = new Vector3[NrFrames];
        boundsExtents = new Vector3[NrFrames];
        thauFr = new Vector3[NrFrames];

        for (int frm = 0; frm < NrFrames; frm++)
        {
            boundsMin[frm] = new Vector3(dataFrame[0][0].rotatedPosition.x,
                                    dataFrame[0][0].rotatedPosition.y,
                                    dataFrame[0][0].rotatedPosition.z);

            boundsMax[frm] = new Vector3(dataFrame[0][0].rotatedPosition.x,
                                    dataFrame[0][0].rotatedPosition.y,
                                    dataFrame[0][0].rotatedPosition.z);

            for (int i = 0; i < nbOfCellsPerFrame[NrFrames - 1]; i++)
            {
                if (boundsMin[frm].x > dataFrame[frm][i].rotatedPosition.x)
                    boundsMin[frm].x = dataFrame[frm][i].rotatedPosition.x;
                if (boundsMin[frm].y > dataFrame[frm][i].rotatedPosition.y)
                    boundsMin[frm].y = dataFrame[frm][i].rotatedPosition.y;
                if (boundsMin[frm].z > dataFrame[frm][i].rotatedPosition.z)
                    boundsMin[frm].z = dataFrame[frm][i].rotatedPosition.z;

                if (boundsMax[frm].x < dataFrame[frm][i].rotatedPosition.x)
                    boundsMax[frm].x = dataFrame[frm][i].rotatedPosition.x;
                if (boundsMax[frm].y < dataFrame[frm][i].rotatedPosition.y)
                    boundsMax[frm].y = dataFrame[frm][i].rotatedPosition.y;
                if (boundsMax[frm].z < dataFrame[frm][i].rotatedPosition.z)
                    boundsMax[frm].z = dataFrame[frm][i].rotatedPosition.z;
            }

            boundsSize[frm] = boundsMax[frm] - boundsMin[frm];
            boundsExtents[frm] = boundsSize[frm] / 2;
            thauFr[frm] = boundsSize[frm]/resolution;
        }

        // Set Axis Lenghts
        string str = "PD,AP,DV\n";
        for(int fr=0; fr<NrFrames; fr++){
            for(int n=0; n<dataFrame[fr].Length; n++){
                dataFrame[fr][n].PDAxisLength = boundsSize[fr].y;
                dataFrame[fr][n].APAxisLength = boundsSize[fr].x;
                dataFrame[fr][n].DVAxisLength = boundsSize[fr].z;
            }
            str += boundsSize[fr].ToString("0.000") + "\n";
        }
        

        // Compute thau
        int maxFrX=0, maxFrY=0, maxFrZ=0; 
        float dimX=0, dimY=0, dimZ=0;
        for(int fr = 0; fr<NrFrames; fr++){
            if(boundsSize[fr].x > dimX){
                dimX = boundsSize[fr].x;
                maxFrX = fr;
            }
            if(boundsSize[fr].y > dimY){
                dimY = boundsSize[fr].y;
                maxFrY = fr;
            }
            if(boundsSize[fr].z > dimZ){
                dimZ = boundsSize[fr].z;
                maxFrZ = fr;
            }
        }

        thau = new Vector3(boundsSize[maxFrX].x/resolution,
                                boundsSize[maxFrY].y/resolution, 
                                boundsSize[maxFrZ].z/resolution);

        Debug.Log("Thau: "+ thau);

        frequencies = new int[resolution+1][];
        relativeFrequencies = new int[resolution+1][];
        frequenciesVsInitial = new int[resolution+1][];
        frequenciesVsCumulated = new int[resolution+1][];
        for(int i=0; i<resolution+1; i++){
            frequencies[i] = new int[3];
            relativeFrequencies[i] = new int[3]; 
            frequenciesVsInitial[i] = new int[3];
            frequenciesVsCumulated[i] = new int[3];
        }

        // Compute overall bounds Min
        Vector3 overallBoundsMin = 1000*Vector3.one;
        int minFrX = 0, minFrY = 0, minFrZ = 0;
        for(int fr = 0; fr<NrFrames; fr++){
            if(boundsMin[fr].x <= overallBoundsMin.x){
                overallBoundsMin.x = boundsMin[fr].x;
                minFrX = fr;
            }
            if(boundsMin[fr].y <= overallBoundsMin.y){
                overallBoundsMin.y = boundsMin[fr].y;
                minFrY = fr;
            }
            if(boundsMin[fr].z <= overallBoundsMin.z){
                overallBoundsMin.z = boundsMin[fr].z;
                minFrZ = fr;
            }
        }
        //Debug.Log("overall bounds min:" + overallBoundsMin + ", " + minFrX + ", " + minFrY + ", " + minFrZ);

        for(int fr = 0; fr<NrFrames; fr++){
            for(int n=0; n<dataFrame[fr].Length; n++){
                dataFrame[fr][n].boundsMin = overallBoundsMin;
            }
        }
    }

	public float[][][] DataFrameToTuples()
    {
        float [][][] tuples = new float[NrFrames][][];
        for(int fr = 0; fr < NrFrames; fr++)
        {
            tuples[fr] = new float[dataFrame[fr].Length][];
            for(int i=0; i<dataFrame[fr].Length; i++)
            {
                tuples[fr][i] = dataFrame[fr][i].ToTuple();
            }
        }
        return tuples;
    }

	public void ComputeFinOrientation(int fr)
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
        
        for(int i=0; i<dataFrame[fr].Length; i++)
        {
            dataFrame[fr][i].fr = fr;
            dataFrame[fr][i].globalCentre = meanPoint;
            //dataFrame[fr][i].globalPrincipalAxis = Vector3.up;// axis[fr][0];
            //dataFrame[fr][i].globalSecondaryAxis = Vector3.right;//axis[fr][1];
            //dataFrame[fr][i].globalTertiaryAxis = new Vector3(0,0,1);//axis[fr][2];
            dataFrame[fr][i].globalPrincipalAxis = new Vector3(finMatrix.R[0,0], finMatrix.R[1,0], finMatrix.R[2,0]);
            dataFrame[fr][i].globalSecondaryAxis = new Vector3(finMatrix.R[0,1], finMatrix.R[1,1], finMatrix.R[2,1]);
            dataFrame[fr][i].globalTertiaryAxis = new Vector3(finMatrix.R[0,2], finMatrix.R[1,2], finMatrix.R[2,2]);
            dataFrame[fr][i].rotatedPosition = finMatrix.R.Transpose() * (dataFrame[fr][i].position - centre[fr]) + centre[fr];
            //Debug.Log(fr + ", " + i + ", " + dataFrame[fr][i].rotatedPosition);
            dataFrame[fr][i].principalAxis = finMatrix.R.Transpose() * dataFrame[fr][i].principalAxis;
        }
    }

	public void ComputeOrientation(int fr, bool relativeToFirst)
    {
        int countActive = 0;
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            Vector3 meanPoint = dataFrame[fr][i].position;
            for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
            {
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
                
                dataFrame[fr][i].matrix = new Matrix3x3(nVarX / n, nCovXY / n, nCovXZ / n,
                                            nCovXY / n, nVarY / n, nCovYZ / n,
                                            nCovXZ / n, nCovYZ / n, nVarZ / n);

				if(relativeToFirst)
                	dataFrame[fr][i].ComputePrincipalAxis(axis[0], centre[fr]);//axis[0][0], centre[fr]);
				else
					dataFrame[fr][i].ComputePrincipalAxis(axis[fr][0], centre[fr]);//axis[fr][0], centre[fr]);

                countActive++;
            }
        }
    }

    public string meanStr = "";
    float[] randomFrequencies;
    int[][] overallFrequencies;
    public void ComputeOrientationDistribution(int fr, bool relativeToFirst)
    {
        
        int numberOfClasses = 30;
        if(fr==0){
            randomFrequencies = new float[numberOfClasses];
        }
        
        float[] classes = new float[numberOfClasses + 1];
        int[] frequencies = new int[numberOfClasses];
        overallFrequencies[fr] = new int[numberOfClasses];
        for(int i=0; i<numberOfClasses+1; i++)
        {
            classes[i] = Mathf.PI * i / (numberOfClasses);
        }

        float mean = 0;
        float standardDeviation = 0;

        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if(dataFrame[fr][i].active == 1)
            {
                int j = 0;
                bool isInClass = false;
                while (j < numberOfClasses)// && !isInClass)
                {
                    if (dataFrame[fr][i].elongationAngle >= classes[j]
                        && dataFrame[fr][i].elongationAngle < classes[j + 1])
                    {
                        isInClass = true;
                        dataFrame[fr][i].elongationAngleClass = (classes[j] + classes[j + 1]) / 2;
                        dataFrame[fr][i].cluster = j;

                        //Debug.Log(fr + ", " + dataFrame[fr][i].elongationAngle);
                        frequencies[j]++;
                        overallFrequencies[fr][j]++;
                        
                    }

                    if (dataFrame[fr][i].angle >= classes[j]
                        && dataFrame[fr][i].angle < classes[j + 1])
                    {
                        dataFrame[fr][i].angle = (classes[j] + classes[j + 1]) / 2;
                        randomFrequencies[j]++;
                        //dataFrame[fr][i].cluster = j;
                    }

                    j++;
                }
            }
        }

        int countActive = 0;
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                countActive++;
                dataFrame[fr][i].elongationAngleClassFrequency = frequencies[dataFrame[fr][i].cluster];
                mean += dataFrame[fr][i].elongationAngleClass;
            }
        }
        //mean /= dataFrame[fr].Length;

        for(int i=0; i<dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                standardDeviation += dataFrame[fr][i].elongationAngleClass * dataFrame[fr][i].elongationAngleClass;
            }
        }
        mean /= countActive;
        standardDeviation /= countActive;
        standardDeviation -= mean * mean;
        activePerFrame[fr] = countActive;

        meanStr += mean + "\n";
        
        standardDeviation = Mathf.Sqrt(standardDeviation);

        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            dataFrame[fr][i].elongationAngleMean = mean;
            dataFrame[fr][i].elongationAngleSD = standardDeviation;
            dataFrame[fr][i].cluster = 0;
        }

        //*
        if(fr == NrFrames-2){
            float sum = 0;
            float max = 0;
            float rdmean = 0;
            int maxIndex = 0;
            for(int i=0; i<randomFrequencies.Length; i++)
            {
                sum += randomFrequencies[i];
                rdmean = randomFrequencies[i]*classes[i];
                if(max < randomFrequencies[i]){
                    maxIndex = i;
                    max = randomFrequencies[i];
                }
                
            }
            rdmean /= (numberOfClasses/2);
            Debug.Log("Random Distribution: " + sum+", "+rdmean);

            for(int i=0; i<randomFrequencies.Length; i++){
                //randomFrequencies[i] /= max;//
                //randomFrequencies[i] *= frequencies[14];

                randomFrequencies[i] /= sum;//max;//
                //Debug.Log(i+", " + randomFrequencies[i]);
            }
            
            // added
            float[][] randomFrequenciesT = new float[NrFrames][];
            for(int t=0; t<NrFrames; t++){
                randomFrequenciesT[t] = new float[randomFrequencies.Length];
                for(int i=0; i<randomFrequencies.Length; i++){
                    randomFrequenciesT[t][i] = randomFrequencies[i] * activePerFrame[t];
                }
            } 
                

            int count=0;
            for(int t=0; t<NrFrames; t++){
                for(int i=0; i<dataFrame[t].Length; i++){
                    if(dataFrame[t][i].active==1){
                        count++;
                        int j = 0;
                        while (j < numberOfClasses)// && !isInClass)
                        {
                            //count++;
                            if (dataFrame[t][i].angle >= classes[j]
                                && dataFrame[t][i].angle < classes[j + 1])
                            {
                                //dataFrame[t][i].sin = randomFrequencies[j];
                                //dataFrame[t][i].line = new Vector3(classes[j + 1] - classes[j], randomFrequencies[j+1]-randomFrequencies[j], 0);
                                
                                dataFrame[t][i].sin = randomFrequenciesT[t][j];
                                dataFrame[t][i].line = new Vector3(classes[j + 1] - classes[j], randomFrequenciesT[t][j+1]-randomFrequenciesT[t][j], 0);
                                
                                if(j==(numberOfClasses/2)-1) dataFrame[t][i].line = Vector3.zero;
                            
                            }
                            j++;
                        }
                    }
                }
            }



            Debug.Log("Count: " + count);
        }
        
        //Debug.Log(fr + ", " + maxIndex + ", " + max);
        //*/
    }

    public void ComputeOrientationDispersion(){
        float angleMean = dataFrame[NrFrames-1][0].elongationAngleMean;

        for(int fr=0; fr<NrFrames; fr++){
            for(int n=0; n<dataFrame[fr].Length; n++){
                dataFrame[fr][n].colorGradient = (angleMean-dataFrame[fr][n].elongationAngleClass)*(angleMean-dataFrame[fr][n].elongationAngleClass); 
            }
        }

        int maxFr =0, maxN = 0;
        float maxGradient = 0;
        for(int fr=0; fr<NrFrames; fr++){
            for(int n=0; n<dataFrame[fr].Length; n++){
                if(dataFrame[fr][n].active == 1 && maxGradient < dataFrame[fr][n].colorGradient){
                    maxGradient = dataFrame[fr][n].colorGradient;
                    maxFr = fr;
                    maxN = n;
                } 
            }
        }

        Debug.Log("Angle mean: " + angleMean + "maxGradient: " + (maxGradient * 180/Mathf.PI) + ", Max Fr: " + maxFr + ", maxN: " + maxN);
        for(int fr=0; fr<NrFrames; fr++){
            for(int n=0; n<dataFrame[fr].Length; n++){
                dataFrame[fr][n].colorGradient = dataFrame[fr][n].colorGradient / maxGradient; 
            }
        }
    }

    public void ComputePolarityVsVelocityDistribution(int fr)
    {
        int numberOfClasses = 30;

        float[] classes = new float[numberOfClasses + 1];
        int[] frequencies = new int[numberOfClasses];
        for (int i = 0; i < numberOfClasses + 1; i++)
        {
            classes[i] = 180 * i / (numberOfClasses);
        }

        float mean = 0;
        float standardDeviation = 0;

        ///Console.WriteLine("Hello");
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                //Debug.Log(fr + ", " + i + ", " + dataFrame[fr][i].polarityVsVelocityAngle);

                int j = 0;
                bool isInClass = false;
                while (j < numberOfClasses && !isInClass)
                {
                    if (dataFrame[fr][i].polarityVsVelocityAngle >= classes[j]
                        && dataFrame[fr][i].polarityVsVelocityAngle < classes[j + 1])
                    {
                        isInClass = true;
                        dataFrame[fr][i].polarityVsVelocityAngleClass = (classes[j] + classes[j + 1]) / 2;
                        dataFrame[fr][i].cluster = j;

                        frequencies[j]++;
                    }
                    j++;
                }
            }
        }

        int countActive = 0;
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                countActive++;
                dataFrame[fr][i].polarityVsVelocityAngleClassFrequency = frequencies[dataFrame[fr][i].cluster];
                mean += dataFrame[fr][i].polarityVsVelocityAngleClass;
            }
        }
        
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                standardDeviation += dataFrame[fr][i].polarityVsVelocityAngleClass * dataFrame[fr][i].polarityVsVelocityAngleClass;
            }
        }
        mean /= countActive;
        standardDeviation /= countActive;
        standardDeviation -= mean * mean;
        standardDeviation = Mathf.Sqrt(standardDeviation);

        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            dataFrame[fr][i].polarityVsVelocityAngleMean = mean;
            dataFrame[fr][i].polarityVsVelocityAngleSD = standardDeviation;

            //if (dataFrame[fr][i].active == 1) {
            if (dataFrame[fr][i].polarityVsVelocityAngleClass < mean - standardDeviation)
            {
                dataFrame[fr][i].cluster = 1;
            }
            else if (dataFrame[fr][i].polarityVsVelocityAngleClass > mean + standardDeviation)
            {
                dataFrame[fr][i].cluster = 2;
            }
            else
            {
                dataFrame[fr][i].cluster = 0;
            }
            //}
        }

        int max = 0;
        int maxIndex = 0;
        for (int i = 0; i < frequencies.Length; i++)
        {
            if (frequencies[i] > max)
            {
                max = frequencies[i];
                maxIndex = i;
            }
        }
        //Debug.Log(maxIndex + ", " + max + ", " + (classes[maxIndex] + classes[maxIndex]) / 2);
    }

    public virtual void Duplicate()
    {
        //NrFrames = 5;
        dataFrame = new CellData[NrFrames][];
        axis = new Vector3[NrFrames][];
        centre = new Vector3[NrFrames];
        for (int fr = NrFrames - 1; fr >= 0; fr--)
        {
            dataFrame[fr] = new CellData[nbOfCellsPerFrame[NrFrames - 1]];
            axis[fr] = new Vector3[3];
        }

        // Last frame
        int i = 0;
        foreach (KeyValuePair<int, CellData> item1 in dataPerCellPerFrame[NrFrames - 1])
        {
            dataFrame[NrFrames - 1][i] = dataPerCellPerFrame[NrFrames - 1][item1.Key];
            dataFrame[NrFrames - 1][i].globalId = item1.Key;
            dataFrame[NrFrames - 1][i].velocity = new Vector3();
            dataFrame[NrFrames - 1][i].ID = i;
            dataFrame[NrFrames - 1][i].active = 1;
            dataFrame[NrFrames - 1][i].fr = NrFrames - 1;
            dataFrame[NrFrames - 1][i].selection = item1.Value.selection;
            dataPerCellPerFrame[NrFrames - 1][item1.Key].ID = i;

            int j = 0;
            foreach (KeyValuePair<int, CellData> item2 in dataPerCellPerFrame[NrFrames - 2])
            {
                if (item1.Value.mother == item2.Key)
                {
                    dataFrame[NrFrames - 1][i].uniqueMotherID = j;
                }
                j++;
            }
            i++;
        }

        for (int fr = NrFrames - 2; fr >= 0; fr--)
        {
            List<int> activeList = new List<int>();
            for (int j = 0; j < dataFrame[fr + 1].Length; j++)
            {
                //405: 1152713952
                //404: 1121106983
                //Debug.Log(fr + ": " + dataFrame[fr + 1][j].mother);
                CellData currentCD = new CellData();
                //if (dataPerCellPerFrame[fr].ContainsKey(dataFrame[fr + 1][j].mother))
                if(dataPerCellPerFrame[fr].TryGetValue(dataFrame[fr + 1][j].mother, out currentCD))
                {
                    dataFrame[fr][j] = new CellData(dataPerCellPerFrame[fr][dataFrame[fr + 1][j].mother]);
                    dataFrame[fr][j].ID = j;
                    dataFrame[fr][j].fr = fr;
                    dataFrame[fr][j].uniqueMotherID = j;
                    //dataFrame[fr][j].velocity = dataFrame[fr + 1][j].position - dataFrame[fr][j].position;
                    //dataFrame[fr][j].globalId = dataPerCellPerFrame[fr][dataFrame[fr + 1][j].mother].globalId;
                    dataFrame[fr][j].globalId = currentCD.globalId;
                    //Debug.Log(dataPerCellPerFrame[fr][dataFrame[fr + 1][j].mother].globalId);
                    dataFrame[fr][j].active = dataFrame[fr + 1][j].active;
                    dataFrame[fr][j].selection = dataFrame[fr + 1][j].selection;
                    //dataPerCellPerFrame[fr][dataFrame[fr][j].globalId].ID = j;

                    if (dataFrame[fr][j].children[1] != -1)
                    {
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
        for (int fr = 0; fr < NrFrames; fr++)
        {
            for (int k = 0; k < nbOfCellsPerFrame[NrFrames - 1]; k++)
            {
                dataFrame[fr][k].neighbourhood = new int[nbOfCellsPerFrame[NrFrames - 1]];
            }
        }
        //*/

        //*
        // Compute Velocities
        int step = 1;
        for (int fr = 0; fr < NrFrames - step; fr++)
        {
            for (int j = 0; j < nbOfCellsPerFrame[NrFrames - 1]; j++)
            {
                dataFrame[fr][j].velocity = dataFrame[fr + step][j].position - dataFrame[fr][j].position;
            }
        }
        //*/
    }

    public virtual void FindNeighbours(int fr)
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

        for (int j = 0; j < nbOfCellsPerFrame[NrFrames - 1]; j++)
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

        for (int j = 0; j < nbOfCellsPerFrame[NrFrames - 1]; j++)
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
            dataFrame[fr][j].neighbours = dataFrame[fr][j].neighbours.Distinct().ToList();
        }
        //*/

        // Compute Relative Velocities
        //*
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                Vector3 meanVelocity = new Vector3();
                for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
                {
                    meanVelocity += dataFrame[fr][dataFrame[fr][i].neighbours[j]].velocity;
                }
                meanVelocity /= (dataFrame[fr][i].neighbours.Count);
                
                dataFrame[fr][i].relativeVelocity = dataFrame[fr][i].velocity - meanVelocity;
            }
        }
        //*/
    }

	public void Log(){
        string dataframeFile = "dataframe_LongAxis.csv";
        dataframeFile = folder + "/" + dataframeFile;
        FileStream fs = new FileStream(dataframeFile, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fs);

        sw.WriteLine(CellData.Header(nbOfCellsPerFrame[NrFrames - 1]));

        int frequency = 1;
        int start = NrFrames % frequency;
        start = 0;

        for (int fr = start; fr < NrFrames; fr += frequency)
        {
            //for (int j = 0; j < nbOfCellsPerFrame[NrFrames - 1]; j++)
            for (int j = 0; j < dataFrame[fr].Length; j++)
            {
                //if(dataFrame[fr][j].active == 1)
                    sw.WriteLine(dataFrame[fr][j].ToString());
            }
        }
        sw.Close();
        fs.Close();
    }
}
