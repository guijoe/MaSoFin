using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using System.Linq;

public class ProliferationTimeSeries : DataInterface {

    public ProliferationTimeSeries(string folder, String file)
    {
        this.folder = folder;
        this.file = file;

        logFile = folder + "/" + file;
    }

    public override void ProcessMovitData()
    {
        Debug.Log("Reading parameters ...");
        ReadParameters();

        Debug.Log("Reading results ...");
        ReadResults();

        Duplicate();

        //NrFrames = 50;
        Init();
        for (int fr = 0; fr < NrFrames; fr++)
        {
            ComputeFinOrientation(fr);
        }

        

        //Debug.Log(meanStr);
        
        ComputeBoundingBox();
        
        //*
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
        
        //*
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

                //Debug.Log(fr + ", " + dataFrame[fr][n].XClassFrequencyVsCumulated + ", " + dataFrame[fr][n].YClassFrequencyVsCumulated);
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
}