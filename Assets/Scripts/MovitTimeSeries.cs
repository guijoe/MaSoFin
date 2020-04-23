using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using System.Linq;

public class MovitTimeSeries : DataInterface {

    public MovitTimeSeries(string folder, String file)
    {
        this.folder = folder;
        this.file = file;
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
        Debug.Log(meanStr);
        
        ComputeOrientationDispersion();
        
        //*
        for(int fr=NrFrames-2; fr>=0; fr--){
            for(int i=0; i<dataFrame[fr].Length; i++){
                if(dataFrame[fr][i].active == 1){
                    //dataFrame[fr][i].angle = dataFrame[NrFrames-2][i].angle;
                    //dataFrame[fr][i].sin = dataFrame[NrFrames-2][i].sin;
                }
            }
        }
        //*/

        ComputeBoundingBox();
        
        /*
        proliferationDistribution = new float[NrFrames][,,];
        proliferationDistribution[0] = new float[resolution+1,resolution+1,resolution+1];
        for(int i=0; i<resolution+1;i++){
            for(int j=0; j<resolution+1; j++){
                for(int k=0; k<resolution+1; k++){
                    proliferationDistribution[0][i,j,k] = 0;
                }
            }
        }
        //*/
        
        /*
        proliferationNumbersPerArea = new int[NrFrames][][];
        for (int fr = 0; fr < NrFrames; fr++)
        {
            ComputeProliferationFrequencies(fr);
            ComputeProliferationFrequencies2(fr);
        }
        ComputeProliferationDistribution();
        ComputeProliferationMarginalDistributions();

        //Debug.Log(distributions);
        //*/
    }

    //*
    
    string distributions = "";
    float[][,,] proliferationDistribution;

    
    public void ComputeProliferationFrequencies(int fr)
    {
        proliferationDistribution[fr] = new float[resolution+1,resolution+1,resolution+1];
        for(int i=0; i<resolution+1;i++){
            for(int j=0; j<resolution+1; j++){
                for(int k=0; k<resolution+1; k++){
                    if(fr==0){
                        proliferationDistribution[fr][i,j,k] = 0;
                    }else{
                        proliferationDistribution[fr][i,j,k] = proliferationDistribution[fr-1][i,j,k];
                    }
                }
            }
        }

        for (int n = 0; n < dataFrame[fr].Length; n++)
        {
            Vector3 relPosition = dataFrame[fr][n].rotatedPosition - boundsMin[fr];
            int i = Mathf.RoundToInt(relPosition.x / thau.x);
            int j = Mathf.RoundToInt(relPosition.y / thau.y);
            int k = Mathf.RoundToInt(relPosition.z / thau.z);

            float zoneX = i * thau.x;
            float zoneY = j * thau.y;
            float zoneZ = k * thau.z;

            // If condition was not there previously
            if(dataFrame[fr][n].active == 1){
                dataFrame[fr][n].XClass = zoneX;
                dataFrame[fr][n].YClass = zoneY;
                dataFrame[fr][n].ZClass = zoneZ;

                //This bloc was not there previously
                dataFrame[fr][n].XClassIndex = i;
                dataFrame[fr][n].YClassIndex = j;
                dataFrame[fr][n].ZClassIndex = k;

                if(fr == 0){
                    //Debug.Log(fr + ", " + i + ", " + j + ", " + k + ", " + (relPosition));
                    frequenciesVsInitial[i][0]++;
                    frequenciesVsInitial[j][1]++;
                    frequenciesVsInitial[k][2]++;
                }

                // new
                //if(fr = NrFrames - 1){
                    frequenciesVsCumulated[i][0]++;
                    frequenciesVsCumulated[j][1]++;
                    frequenciesVsCumulated[k][2]++;
                //}
            }
            

            if (fr > 0 && dataFrame[fr][n].active == 1 && dataFrame[fr-1][n].active == 0)
            {
                frequencies[i][0]++;
                frequencies[j][1]++;
                frequencies[k][2]++;

                proliferationDistribution[fr][i,j,k]++;
                //Debug.Log(i + ", " + j + ", " + k + ", " + proliferationDistribution[fr][i,j,k]);

                distributions += relPosition.x + "," + relPosition.y + ", " + relPosition.z + "\n"; 
            }
        }

        for (int n = 0; n < dataFrame[fr].Length; n++)
        {
            if(dataFrame[fr][n].active == 1){
                dataFrame[fr][n].XClassFrequency = frequencies[(int)dataFrame[fr][n].XClassIndex][0];
                dataFrame[fr][n].YClassFrequency = frequencies[(int)dataFrame[fr][n].YClassIndex][1];
                dataFrame[fr][n].ZClassFrequency = frequencies[(int)dataFrame[fr][n].ZClassIndex][2];

                //*
                if(frequenciesVsInitial[(int)dataFrame[fr][n].XClassIndex][0] != 0)
                    dataFrame[fr][n].XClassFrequencyVsInitial = dataFrame[fr][n].XClassFrequency/ frequenciesVsInitial[(int)dataFrame[fr][n].XClassIndex][0];
                if(frequenciesVsInitial[(int)dataFrame[fr][n].YClassIndex][1] != 0)
                    dataFrame[fr][n].YClassFrequencyVsInitial = dataFrame[fr][n].YClassFrequency/ frequenciesVsInitial[(int)dataFrame[fr][n].YClassIndex][1];
                if(frequenciesVsInitial[(int)dataFrame[fr][n].ZClassIndex][2] != 0)
                    dataFrame[fr][n].ZClassFrequencyVsInitial = dataFrame[fr][n].ZClassFrequency/ frequenciesVsInitial[(int)dataFrame[fr][n].ZClassIndex][2];
                //*/

                // New
                dataFrame[fr][n].XClassFrequencyVsCumulated = dataFrame[fr][n].XClassFrequency/ frequenciesVsCumulated[(int)dataFrame[fr][n].XClassIndex][0];
                dataFrame[fr][n].YClassFrequencyVsCumulated = dataFrame[fr][n].YClassFrequency/ frequenciesVsCumulated[(int)dataFrame[fr][n].YClassIndex][1];
                dataFrame[fr][n].ZClassFrequencyVsCumulated = dataFrame[fr][n].ZClassFrequency/ frequenciesVsCumulated[(int)dataFrame[fr][n].ZClassIndex][2];

                Debug.Log(fr + ", " + dataFrame[fr][n].XClassFrequencyVsCumulated + ", " + dataFrame[fr][n].YClassFrequencyVsCumulated);
            }
        }
    }
    //*/

    public void ComputeProliferationFrequencies2(int fr)
    {
        for (int n = 0; n < dataFrame[fr].Length; n++)
        {
            Vector3 relPosition = dataFrame[fr][n].rotatedPosition - boundsMin[fr];
            int i = Mathf.RoundToInt(relPosition.x / thauFr[fr].x);
            int j = Mathf.RoundToInt(relPosition.y / thauFr[fr].y);
            int k = Mathf.RoundToInt(relPosition.z / thauFr[fr].z);

            float zoneX = i * thauFr[fr].x;
            float zoneY = j * thauFr[fr].y;
            float zoneZ = k * thauFr[fr].z;

            // If condition was not there previously
            if(dataFrame[fr][n].active == 1){
                dataFrame[fr][n].XRelativeClass = zoneX;
                dataFrame[fr][n].YRelativeClass = zoneY;
                dataFrame[fr][n].ZRelativeClass = zoneZ;

                //This bloc was not there previously
                dataFrame[fr][n].XClassRelativeIndex = i;
                dataFrame[fr][n].YClassRelativeIndex = j;
                dataFrame[fr][n].ZClassRelativeIndex = k;
            }
            

            if (fr > 0 && dataFrame[fr][n].active == 1 && dataFrame[fr-1][n].active == 0)
            {
                relativeFrequencies[i][0]++;
                relativeFrequencies[j][1]++;
                relativeFrequencies[k][2]++;

                //proliferationDistribution[fr][i,j,k]++;
                
                //distributions += relPosition.x + "," + relPosition.y + ", " + relPosition.z + "\n"; 
            }
        }

        for (int n = 0; n < dataFrame[fr].Length; n++)
        {
            if(dataFrame[fr][n].active == 1){
                dataFrame[fr][n].XClassRelativeFrequency = relativeFrequencies[(int)dataFrame[fr][n].XClassRelativeIndex][0];
                dataFrame[fr][n].YClassRelativeFrequency = relativeFrequencies[(int)dataFrame[fr][n].YClassRelativeIndex][1];
                dataFrame[fr][n].ZClassRelativeFrequency = relativeFrequencies[(int)dataFrame[fr][n].ZClassRelativeIndex][2];
            }
        }
    }
    

    public void ComputeProliferationMarginalDistributions(){

        float[][] classes = new float[resolution+1][];
        for(int i=0; i<resolution+1; i++){
            classes[i] = new float[3];
            classes[i][0] = i * thau.x;
            classes[i][1] = i * thau.y;
            classes[i][2] = i * thau.z;
        }

        int sumX=0, sumY=0, sumZ=0;
        float maxX=0, maxY=0, maxZ=0;
        string distribution = "";
        for(int i=0; i<frequencies.Length; i++){

            if(maxX < frequencies[i][0])
                maxX = frequencies[i][0];
            if(maxY < frequencies[i][1])
                maxY = frequencies[i][1];
            if(maxZ < frequencies[i][2])
                maxZ = frequencies[i][2];

            //distribution += classes[i][0] + "," + frequencies[i][0] + ", " + 
            //            classes[i][1] + "," + frequencies[i][1] + ", " + 
            //            classes[i][2] + "," + frequencies[i][2] + "\n";
        }
        //Debug.Log(distribution);

        for(int fr=0; fr<NrFrames; fr++){
            for (int n = 0; n < dataFrame[fr].Length; n++)
            {
                if(dataFrame[fr][n].active == 1){
                    dataFrame[fr][n].colorGradient = 1 - dataFrame[fr][n].XClassFrequency/maxX;
                    dataFrame[fr][n].colorGradient1 = 1 - dataFrame[fr][n].YClassFrequency/maxY;
                    dataFrame[fr][n].colorGradient2 = 1 - dataFrame[fr][n].ZClassFrequency/maxZ;

                    //Debug.Log(dataFrame[fr][n].colorGradient + ", " + dataFrame[fr][n].colorGradient1 + ", " + dataFrame[fr][n].colorGradient2);
                }
            }
        }
        
    }

    public void ComputeProliferationMarginalDistributions2(){

        float[][] classes = new float[resolution+1][];
        for(int i=0; i<resolution+1; i++){
            //classes[i] = new float[3];
            //classes[i][0] = i * thau.x;
            //classes[i][1] = i * thau.y;
            //classes[i][2] = i * thau.z;
        }

        int sumX=0, sumY=0, sumZ=0;
        float maxX=0, maxY=0, maxZ=0;
        string distribution = "";
        for(int i=0; i<frequencies.Length; i++){

            if(maxX < frequencies[i][0])
                maxX = frequencies[i][0];
            if(maxY < frequencies[i][1])
                maxY = frequencies[i][1];
            if(maxZ < frequencies[i][2])
                maxZ = frequencies[i][2];

            //distribution += classes[i][0] + "," + frequencies[i][0] + ", " + 
            //            classes[i][1] + "," + frequencies[i][1] + ", " + 
            //            classes[i][2] + "," + frequencies[i][2] + "\n";
        }
        //Debug.Log(distribution);

        for(int fr=0; fr<NrFrames; fr++){
            for (int n = 0; n < dataFrame[fr].Length; n++)
            {
                if(dataFrame[fr][n].active == 1){
                    dataFrame[fr][n].colorGradient = 1 - dataFrame[fr][n].XClassFrequency/maxX;
                    dataFrame[fr][n].colorGradient1 = 1 - dataFrame[fr][n].YClassFrequency/maxY;
                    dataFrame[fr][n].colorGradient2 = 1 - dataFrame[fr][n].ZClassFrequency/maxZ;

                    //Debug.Log(dataFrame[fr][n].colorGradient + ", " + dataFrame[fr][n].colorGradient1 + ", " + dataFrame[fr][n].colorGradient2);
                }
            }
        }
        
    }

    public void ComputeProliferationDistribution(){

        float maxFrequency = 0;
        
        for(int i=0; i<resolution+1;i++){
            for(int j=0; j<resolution+1; j++){
                for(int k=0; k<resolution+1; k++){
                    if(proliferationDistribution[NrFrames-1][i,j,k] > maxFrequency){
                        maxFrequency = proliferationDistribution[NrFrames-1][i,j,k];
                    }
                }
            }
        }
        //Debug.Log("Max Frequency: " + maxFrequency);


        for(int fr=0; fr<NrFrames; fr++){
            for(int i=0; i<resolution+1;i++){
                for(int j=0; j<resolution+1; j++){
                    for(int k=0; k<resolution+1; k++){
                        proliferationDistribution[fr][i,j,k] /= maxFrequency;
                        //Debug.Log(i + ", " + j + ", " + k + ", " + proliferationDistribution[fr][i,j,k]);
                    }
                }
            }
        }

        for(int fr=0; fr<NrFrames; fr++){
            for (int n = 0; n < dataFrame[fr].Length; n++)
            {
                if(dataFrame[fr][n].active == 1){
                    int i = (int)dataFrame[fr][n].XClassIndex;
                    int j = (int)dataFrame[fr][n].YClassIndex;
                    int k = (int)dataFrame[fr][n].ZClassIndex;
                    //Debug.Log(i + ", " + j + ", " + k + ", " + proliferationDistribution[fr][i,j,k]);
                    dataFrame[fr][n].colorGradient = (1-proliferationDistribution[fr][i,j,k]);
                    //Debug.Log(fr + ", " + i + ", " + j + ", " + k + ", " + proliferationDistribution[fr][i,j,k] + ", " + dataFrame[fr][n].colorGradient);
                    
                }
            }
        }
    }

    /*
    public void ComputeProliferationDistribution2(int fr)
    {
        //
        int[][] classes = new int[resolution+1][];
        //int[][] frequencies = new int[resolution+1][];
        //proliferationNumbersPerArea[fr] = new int[resolution][];
        for(int i=0; i<resolution+1; i++){
            //
            classes[i] = new int[3];
            classes[i][0] = Mathf.RoundToInt(i * thau.x);
            classes[i][1] = Mathf.RoundToInt(i * thau.y);
            classes[i][2] = Mathf.RoundToInt(i * thau.z);

            //frequencies[i] = new int[3];
            //proliferationNumbersPerArea[fr][i] = new int[3];
        }

        for (int n = 0; n < dataFrame[fr].Length; n++)
        {
            Vector3 relPosition = dataFrame[fr][n].rotatedPosition - boundsMin;
            int i = Mathf.RoundToInt(relPosition.x / thau.x);
            int j = Mathf.RoundToInt(relPosition.y / thau.y);
            int k = Mathf.RoundToInt(relPosition.z / thau.z);

            float zoneX = i * thau.x;
            float zoneY = j * thau.y;
            float zoneZ = k * thau.z;

            dataFrame[fr][n].XClass = zoneX;
            dataFrame[fr][n].YClass = zoneY;
            dataFrame[fr][n].ZClass = zoneZ;

            if (dataFrame[fr][n].active == 1 && dataFrame[fr-1][n].active == 0)
            {
                frequencies[i][0]++;
                frequencies[j][1]++;
                frequencies[k][2]++;
            }
        }

        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            dataFrame[fr][i].XClassFrequency = frequencies[Mathf.RoundToInt(dataFrame[fr][i].XClass/thau.x)][0];
            dataFrame[fr][i].YClassFrequency = frequencies[Mathf.RoundToInt(dataFrame[fr][i].YClass/thau.y)][1];
            dataFrame[fr][i].ZClassFrequency = frequencies[Mathf.RoundToInt(dataFrame[fr][i].ZClass/thau.z)][2];
        }

        //*
        for(int i=0; i<dataFrame[fr].Length; i++){
            bool found = false;
            for(int j=0; j<dataFrame[fr-1].Length; j++){
                if(dataFrame[fr-1][j].active == 1 && dataFrame[fr][i].XClass == dataFrame[fr-1][j].XClass && !found){
                    //Debug.Log("Hello: " + dataFrame[fr-1][j].XClassFrequency);
                    dataFrame[fr][i].XClassFrequency += dataFrame[fr-1][j].XClassFrequency;
                    found = true;
                }
            }
        }
        //
        if(fr==NrFrames-1){
            int sumX=0, sumY=0, sumZ=0;
            for(int i=0; i<frequencies.Length; i++){
                sumX += frequencies[i][0];
                sumY += frequencies[i][1];
                sumZ += frequencies[i][2];
            }
            Debug.Log(sumX + ", " + sumY + ", " + sumZ);
        }
    }
    //*/

    public void Duplicate()
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
}