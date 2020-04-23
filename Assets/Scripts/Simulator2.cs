using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using System.Linq;

public class Simulator2 : MonoBehaviour{

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

    Vector3[][] axis;
    Vector3[] centre;

	public CellGO cellPrefab;
    List<CellGO> cells;

	Vector3 dL;
	int mitosisPeriod = 258;
    int NrFrames = 0;
	int nbOfFrames;
    [Range(0, 516)]
    public int frame = 0;
    public int speed = 1;
    public bool play = false;
    public bool reset = false;
    public bool displayCells = false;

    Vertex3[][] vertices;
    private DelaunayTriangulation3 delaunay;
    public float size = 50f;

    List<CellData> persistantCellData;
	List<int> nonActiveList;
	int activeCount;

    Color[] selectionColors;
	
	string folder = "C:/Users/18003111/OneDrive - MMU/Documents/movit/application.windows64/MovitData/distant/180420hZ";

	void Start(){

        selectionColors = new Color[]
        {
            Color.green,
            Color.yellow,
            Color.blue,
            new Color(255, 165, 0),
            new Color(238,130,238),
            Color.white,
            Color.red
        };

		//NrFrames = 10;
		ProcessMovitData();

        //axis[0][0] = UnityEngine.Random.onUnitSphere;
		for(int fr=0; fr < NrFrames - 1; fr++){
			CustomUpdate(fr);
            //ComputeGrowthOrientation(fr);	
		}
		CreateCellPopulation();
	}

	// Update is called once per frame
    void Update() {
        int count = 0;
		
        if (play)
        {
            while (frame < NrFrames - 1 && count++ < speed)
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
    }

	public void PlayResults(int fr)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].active = dataFrame[fr][i].active;
			if(dataFrame[fr][i].active == 0){
                cells[i].GetComponent<MeshRenderer>().material.color = Color.white;
                cells[i].GetComponent<MeshRenderer>().enabled = false;
			}
            else{
				cells[i].GetComponent<MeshRenderer>().material.color = Color.green;
                cells[i].GetComponent<MeshRenderer>().enabled = true;
			}
            cells[i].transform.localPosition = dataFrame[fr][i].position;
        }
    }
	
	public void ProcessMovitData()
    {
        Debug.Log("Reading parameters ...");
        ReadParameters();

        Debug.Log("Reading results ...");
        ReadResults();

        Duplicate();
        FindNeighbours(NrFrames - 1);
		ComputeFinOrientation(0);

		for(int i=0; i<dataFrame[0].Length; i++){
			if(dataFrame[0][i].active == 1){
				dataFrame[0][i].frame = UnityEngine.Random.Range(0,mitosisPeriod);
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
                NrFrames = int.Parse(reader["nbTimesteps"]) - 1;
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

    public void ReadResults()
    {
        string logFile = folder + "/test_06012020.csv"; //"/test.csv";// 

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
                                            new CellData(int.Parse(allData[index][0]), int.Parse(allData[index][6]),
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
            //dataFrame[NrFrames - 1][i].velocity = new Vector3();
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
                if (dataPerCellPerFrame[fr].ContainsKey(dataFrame[fr + 1][j].mother))
                {
                    dataFrame[fr][j] = new CellData(dataPerCellPerFrame[fr][dataFrame[fr + 1][j].mother]);
                    dataFrame[fr][j].ID = j;
                    dataFrame[fr][j].fr = fr;
                    dataFrame[fr][j].uniqueMotherID = j;
                    //dataFrame[fr][j].velocity = dataFrame[fr + 1][j].position - dataFrame[fr][j].position;
                    dataFrame[fr][j].globalId = dataPerCellPerFrame[fr][dataFrame[fr + 1][j].mother].globalId;
                    dataFrame[fr][j].active = dataFrame[fr + 1][j].active;
					dataFrame[fr][j].selection = dataFrame[fr + 1][j].selection;
                    //dataPerCellPerFrame[fr][dataFrame[fr][j].globalId].ID = j;

                    if(fr+1 != NrFrames - 1){
                        dataFrame[fr + 1][j].active = 0;
                        dataFrame[fr + 1][j].position = new Vector3();
                    }

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

		// Initialise non active list
		nonActiveList = new List<int>();
		for (int j = 0; j < dataFrame[0].Length; j++)
        {
			if(dataFrame[0][j].active == 0){
				nonActiveList.Add(j);
			}
		}
    }

    public Vector3 lowestPoint = new Vector3();
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
            //principalAxis = matrix.ComputeMinAxis();
            axis[fr] = matrix.ComputeMinAxis();
        }
        else if (fr <= 400)
        {
            axis[fr] = matrix.ComputePrincipalAxis(axis[fr-1]);
        }
        else
        {
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
    }

	public void ComputeOrientation(int fr)
    {
        int countActive = 0;
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            Vector3 meanPoint = dataFrame[fr][i].position;
            //Debug.Log(fr + ", " + i + ", " + dataFrame[fr][i].neighbours.Count);
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


                dataFrame[fr][i].ComputePrincipalAxis(axis[0][0], centre[fr]);

                countActive++;
            }
        }
    }

    public void ComputeGrowthOrientation(int fr)
    {
        centre[fr] = new Vector3();
        int countActive = 0;

        float nCovXY = 0;
        float nCovXZ = 0;
        float nCovYZ = 0;
        float nVarX = 0;
        float nVarY = 0;
        float nVarZ = 0;

        //*
        //Vector3 meanVelocity = new Vector3();        
        Vector3 meanPoint = new Vector3();
        Vector3 meanPoint2 = new Vector3();
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            if (dataFrame[fr][i].active == 1)
            {
                Vector3 meanVelocity = new Vector3();
                /*
                for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
                {
                    meanVelocity += dataFrame[fr][dataFrame[fr][i].neighbours[j]].velocity;
                }
                meanVelocity /= (dataFrame[fr][i].neighbours.Count);
                //*/

                dataFrame[fr][i].relativeVelocity = dataFrame[fr][i].velocity - meanVelocity;
                meanPoint += dataFrame[fr][i].relativeVelocity;
                //meanPoint2 += dataFrame[fr][i].position;
                countActive++;
            }
        }
        meanPoint /= countActive;
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

        //Debug.Log(fr + matrix.ToString());
        axis[0] = matrix.ComputeMinAxis();
        axis[0][0] = axis[0][2];
    }

	public Matrix3x3 ComputeOrientationMatrix(int fr, int i, int Xi, Vector3 newPosition){
		
		Vector3 iPosition = dataFrame[fr][i].position;
		if(i == Xi){
			iPosition = newPosition;
		}


		Vector3 meanPoint = iPosition;
		for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
		{
			if(dataFrame[fr][i].neighbours[j] == Xi){
				meanPoint += newPosition;
			}else{
				meanPoint += dataFrame[fr][dataFrame[fr][i].neighbours[j]].position;
			}
		}
		meanPoint /= (1 + dataFrame[fr][i].neighbours.Count);

		float nCovXY = (iPosition.x - meanPoint.x) * (iPosition.y - meanPoint.y);
		float nCovXZ = (iPosition.x - meanPoint.x) * (iPosition.z - meanPoint.z);
		float nCovYZ = (iPosition.y - meanPoint.y) * (iPosition.z - meanPoint.z);
		float nVarX = (iPosition.x - meanPoint.x) * (iPosition.x - meanPoint.x);
		float nVarY = (iPosition.y - meanPoint.y) * (iPosition.y - meanPoint.y);
		float nVarZ = (iPosition.z - meanPoint.z) * (iPosition.z - meanPoint.z);

		for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
		{
			int k = dataFrame[fr][i].neighbours[j];

			if(k == Xi){
				nCovXY += (newPosition.x - meanPoint.x) * (newPosition.y - meanPoint.y);
				nCovXZ += (newPosition.x - meanPoint.x) * (newPosition.z - meanPoint.z);
				nCovYZ += (newPosition.y - meanPoint.y) * (newPosition.z - meanPoint.z);

				nVarX += (newPosition.x - meanPoint.x) * (newPosition.x - meanPoint.x);
				nVarY += (newPosition.y - meanPoint.y) * (newPosition.y - meanPoint.y);
				nVarZ += (newPosition.z - meanPoint.z) * (newPosition.z - meanPoint.z);
			}
			else{
				nCovXY += (dataFrame[fr][k].position.x - meanPoint.x) * (dataFrame[fr][k].position.y - meanPoint.y);
				nCovXZ += (dataFrame[fr][k].position.x - meanPoint.x) * (dataFrame[fr][k].position.z - meanPoint.z);
				nCovYZ += (dataFrame[fr][k].position.y - meanPoint.y) * (dataFrame[fr][k].position.z - meanPoint.z);

				nVarX += (dataFrame[fr][k].position.x - meanPoint.x) * (dataFrame[fr][k].position.x - meanPoint.x);
				nVarY += (dataFrame[fr][k].position.y - meanPoint.y) * (dataFrame[fr][k].position.y - meanPoint.y);
				nVarZ += (dataFrame[fr][k].position.z - meanPoint.z) * (dataFrame[fr][k].position.z - meanPoint.z);
			}
		}

		int n = 1 + dataFrame[fr][i].neighbours.Count;
		
		Matrix3x3 matrix = new Matrix3x3(nVarX / n, nCovXY / n, nCovXZ / n,
									nCovXY / n, nVarY / n, nCovYZ / n,
									nCovXZ / n, nCovYZ / n, nVarZ / n);
		
		return matrix;
	}

	public void CreateCellPopulation()
    {
        int fr = 0;
        int count = 0;
		cells = new List<CellGO>();
        for(int i=0; i<dataFrame[fr].Length; i++)
        {
            CellGO cell = Instantiate(cellPrefab);
			
			cell.frame = dataFrame[fr][i].frame;
            cell.globalId = dataFrame[fr][i].globalId;

            cell.motherId = dataFrame[fr][i].mother;
            cell.position = dataFrame[fr][i].position;
            cell.selection = dataFrame[fr][i].selection;

            cell.name = "Cell_" + count;
            cell.transform.SetParent(transform);
            cell.transform.localPosition = dataFrame[fr][i].position;
            
			cells.Add(cell);
            cells[i].active = dataFrame[fr][i].active;
			if(dataFrame[fr][i].active == 1){
				//cells[i].GetComponent<MeshRenderer>().enabled = true;	
			}else{
				//cells[i].GetComponent<MeshRenderer>().enabled = false;
			}
            
            count++;
        }
    }
	
    public float ComputeEnergy(int fr, int i, float sumAngleBefore, float sumAngleNeighboursI, Vector3 dX){
		// Energy when cell i is moved by dX: H(Xi + dX)
		Vector3 position = dataFrame[fr][i].position + dX;
		float sumAngleAfter = sumAngleBefore - sumAngleNeighboursI - dataFrame[fr][i].elongationAngle;
		for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
		{
			int k = dataFrame[fr][i].neighbours[j];
			Matrix3x3 matrix = ComputeOrientationMatrix(fr, k, i, position);
			sumAngleAfter += dataFrame[fr][k].ComputeElongationAngle(axis[0][0], matrix);
		}
		Matrix3x3 matrix1 = ComputeOrientationMatrix(fr, i, i, position);
		sumAngleAfter += dataFrame[fr][i].ComputeElongationAngle(axis[0][0], matrix1);
		
		float energyAfterDX = (sumAngleAfter/activeCount - Mathf.PI/3) * (sumAngleAfter/activeCount - Mathf.PI/3);
		//Debug.Log(fr + ", " + (sumAngleBefore/nbOfCellsPerFrame[fr]) + ", " + (sumAngleAfter/nbOfCellsPerFrame[fr]) + ", " + dL);
		return energyAfterDX;
	}

	public Vector3 ComputePotentialGradient(int fr, int i){
		
		// Sum of elongation angles influenced by i
		float sumAngleNeighboursI = 0;
		for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
		{
			int k = dataFrame[fr][i].neighbours[j];
			sumAngleNeighboursI += dataFrame[fr][k].elongationAngle;
		}

		// Energy with cell i at current position: H(Xi)
		float sumAngleBefore = 0;
		Vector3 position = dataFrame[fr][i].position;
		for (int j = 0; j < dataFrame[fr].Length; j++)
		{
			sumAngleBefore += dataFrame[fr][j].elongationAngle;
		}
		float energyBefore = (sumAngleBefore/activeCount - Mathf.PI/3) * (sumAngleBefore/activeCount - Mathf.PI/3);
        
		// Energy when cell i is moved by dL.x: H(Xi + dx)
		float energyAfterDx = ComputeEnergy(fr, i, sumAngleBefore, sumAngleNeighboursI, new Vector3(dL.x, 0, 0));

		// Energy when cell i is moved by dL.y: H(Xi + dy)
		float energyAfterDy = ComputeEnergy(fr, i, sumAngleBefore, sumAngleNeighboursI, new Vector3(0, dL.y, 0));

		// Energy when cell i is moved by dL.z: H(Xi + dz)
		float energyAfterDz = ComputeEnergy(fr, i, sumAngleBefore, sumAngleNeighboursI, new Vector3(0, 0, dL.z));

        dataFrame[fr][i].gradient = new Vector3((energyAfterDx - energyBefore)/dL.x, 
							(energyAfterDy - energyBefore)/dL.y, 
							(energyAfterDz - energyBefore)/dL.z);

		return dataFrame[fr][i].gradient;    
	}

    List<int>[] miniBatches;
    List<int> activeList;
    public void GenerateMiniBatches(int maxPerBatch){
        
        List<int> contextualActiveList = new List<int>(activeList);
        int NrBatches = contextualActiveList.Count / maxPerBatch + 1;
        miniBatches = new List<int>[NrBatches];
        
        System.Random rand = new System.Random(0);
        for(int i=0; i<NrBatches; i++){
            miniBatches[i] = new List<int>();

            int j=0;
            while(j < maxPerBatch && contextualActiveList.Any()){
                //rand.Next()
                int k = rand.Next(contextualActiveList.Count);
                miniBatches[i].Add(contextualActiveList[k]);
                contextualActiveList.RemoveAt(k);
                j++;
            }
        }
    }

    public void ComputePotentialGradient(int fr, List<int> miniBatch){
        Vector3 miniBatchGradient = new Vector3();
        for(int i=0; i<miniBatch.Count; i++){
            miniBatchGradient += ComputePotentialGradient(fr, i);
        }
        miniBatchGradient /= miniBatch.Count;

        for(int i=0; i<miniBatch.Count; i++){
            dataFrame[fr][miniBatch[i]].gradient = miniBatchGradient;
        }
    }

    public Vector3 ComputeAttRepForces(int fr, int i, float req){
        float D = 1f;
        Vector3 FaR = new Vector3();
        Vector3 dirIJ = new Vector3();
        for (int k = 0; k < dataFrame[fr][i].neighbours.Count; k++)
		{
			int j = dataFrame[fr][i].neighbours[k];
            dirIJ = dataFrame[fr][i].position - dataFrame[fr][j].position;

            float exponent = -(dirIJ.magnitude - req);
            if(exponent != 0 && dirIJ.magnitude != 0)
			    FaR += 2 * D * (Mathf.Exp(2*exponent) - Mathf.Exp(exponent)) * dirIJ/dirIJ.magnitude;
		}

        return FaR;
    }

	float neta = 1f; // Gradient Descent Rate
	int aCount = 0;
	public void CustomUpdate(int fr){
		aCount = 0;
        activeList = new List<int>();

		// Proliferate
		for(int i=0; i<dataFrame[fr].Length; i++){
			if(dataFrame[fr][i].active == 1){
                if(dataFrame[fr][i].frame == mitosisPeriod && nonActiveList.Count > 0){
					int k = nonActiveList[0];
					nonActiveList.RemoveAt(0);

                    dataFrame[fr][k].active = 1;
					dataFrame[fr][k].frame = 0;
					dataFrame[fr][k].position = dataFrame[fr-1][i].position 
                                                + dataFrame[fr-1][i].principalAxis 
                                                * dataFrame[fr-1][i].axisLength/6;
                    
                    //Debug.Log(fr + ", " + i +"," + dataFrame[fr-1][i].principalAxis);

					dataFrame[fr][i].position += - dataFrame[fr-1][i].principalAxis 
                                                    * dataFrame[fr-1][i].axisLength/6;

					dataFrame[fr][i].frame = 0;
					aCount++;	
				}
			}
		}

        // Use all active
		int countActive = 0;
		centre[fr] = new Vector3();
		for(int i=0; i<dataFrame[fr].Length; i++){
			if(dataFrame[fr][i].active == 1){
                activeList.Add(i);
				centre[fr] += dataFrame[fr][i].position;
                countActive++;
			}
		}
		activeCount = countActive;
		centre[fr] /= countActive;
		
        // Update Cell Positions
        FindNeighbours(fr);
		ComputeOrientation(fr);
		
        /*
        GenerateMiniBatches(10);
        for(int i=0; i<miniBatches.Length; i++){
            ComputePotentialGradient(fr, miniBatches[i]);
        }
        //*/

		for(int i=0; i<dataFrame[fr].Length; i++){
			if(dataFrame[fr][i].active == 1){
				//Vector3 gradient = dataFrame[fr][i].gradient; 
                Vector3 gradient = ComputePotentialGradient(fr, i);
                Vector3 Fmig = -gradient;

                Vector3 FaR = ComputeAttRepForces(fr, i, 2*dL.magnitude);

                Vector3 force = neta * Fmig + .1f * FaR;//.001f * FaR;
				//Vector3 updatedPosition = dataFrame[fr][i].position + force;
                Vector3 relativePosition = dataFrame[fr][i].position - lowestPoint;
                
                //force = 10*force;
                force = 1f*force;
                //*
                float force3 = Vector3.Dot(force + relativePosition, axis[0][0]);
                if(force3 < 0){

                    float force1 = Vector3.Dot(force, axis[0][2]);
                    float force2 = Vector3.Dot(force, axis[0][1]);
                    force3 = -force3;
                    
                    force = new Matrix3x3(axis[0][2], axis[0][1], axis[0][0]) 
                                * new Vector3(force1, force2, force3);
                }
                //*/
                
                dataFrame[fr][i].velocity = force;
                dataFrame[fr+1][i].position = dataFrame[fr][i].position + force;
                dataFrame[fr+1][i].active = 1;
				dataFrame[fr+1][i].frame = dataFrame[fr][i].frame + 1;
			}
		}
	}

    float meanNeighbourDistance = 0;
    public void FindNeighbours(int fr)
    {
        // Compute Double Mean Distance Square
        float doubleMeanDistanceSquare = 0;
        for(int i=0; i<dataFrame[fr].Length; i++){
            if(dataFrame[fr][i].active == 1){
                for(int j=0; j<dataFrame[fr].Length; j++){
                    if(dataFrame[fr][j].active == 1)
                        doubleMeanDistanceSquare += (dataFrame[fr][i].position - dataFrame[fr][j].position).sqrMagnitude;
                }
            }
        }
        doubleMeanDistanceSquare /= (activeCount * (activeCount - 1));
        doubleMeanDistanceSquare *= 2;

        if(fr > 0){
            //Debug.Log(fr + ", activeCount: " + activeCount);
            if(fr == NrFrames-1){
                vertices[fr] = new Vertex3[nbOfCellsPerFrame[NrFrames - 1]];
            }else{
                vertices[fr] = new Vertex3[activeCount];
            }

            int j=0;
            for(int i=0; i<dataFrame[fr].Length; i++){

                if(dataFrame[fr][i].active == 1){
                    vertices[fr][j] = new Vertex3(
                        dataFrame[fr][i].position.x,
                        dataFrame[fr][i].position.y,
                        dataFrame[fr][i].position.z
                    );
                    j++;
                }
            }
        }

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
            //Debug.Log(fr + ", " + dataFrame[fr][j].neighbours.Count);
        }
        //*/

		// Compute dL = 0.5 * meanNeighbourDistance
        //*
		int countActive = 0;
		meanNeighbourDistance = 0;
        for (int i = 0; i < dataFrame[fr].Length; i++)
        {
            float mND = 0;
            if (dataFrame[fr][i].active == 1)
            {
                for (int j = 0; j < dataFrame[fr][i].neighbours.Count; j++)
                {
                    int k = dataFrame[fr][i].neighbours[j];
                    mND += Vector3.Distance(dataFrame[fr][i].position, dataFrame[fr][k].position);
                }
                mND /= dataFrame[fr][i].neighbours.Count;
                //Debug.Log(i + ", " + mND);
                countActive++;
            }
            meanNeighbourDistance += mND;
        }
        

        if(fr == NrFrames - 1){
            meanNeighbourDistance /= nbOfCellsPerFrame[NrFrames - 1];
			dL = Vector3.one * (.5f * meanNeighbourDistance) / Mathf.Sqrt(3);
            //Debug.Log(dataFrame[fr][0].position + ", " + dataFrame[fr][10].position + ", " + meanNeighbourDistance + ", " + dL);
		}else{
            meanNeighbourDistance /= countActive;
        }
        //*/
    }

	void OnDrawGizmos()
    {
        DrawOrientation();
    }

    public void DrawOrientation()
    {
        int fr = frame;

		if(fr>=NrFrames-1){
			//frame = NrFrames-2;
			fr = NrFrames-2;
		}

        Gizmos.color = Color.red;
        Vector3 dir = 8 * axis[0][0];
        Gizmos.DrawLine(centre[fr] - dir, centre[fr]+dir);

        Gizmos.color = Color.green;
        //Vector3 dir1 = 8 * axis[0][1];
        //Gizmos.DrawLine(centre[fr] - dir1, centre[fr] + dir1);

        Gizmos.color = Color.yellow;
        //Vector3 dir2 = 8 * axis[0][2];
        //Gizmos.DrawLine(centre[fr] - dir2, centre[fr] + dir2);
    }
}