using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System;
using UnityEngine;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using System.Linq;

public class Simulator : DataInterface{
	
    Vector3 dL;
	int mitosisPeriod = 258;
    
    List<int> nonActiveList;
	int activeCount;

	public Simulator(string folder, String file)
    {
        this.folder = folder;
        this.file = file;

        logFile = folder + "/" + file;
    }

	public override void ProcessMovitData()
    {
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

        Debug.Log("Reading parameters ...");
        ReadParameters();

        Debug.Log("Reading results ...");
        ReadResults();

        //NrFrames = 40;

        Duplicate();
        FindNeighbours(NrFrames - 1);
		ComputeFinOrientation(0);
        Init();

        UnityEngine.Random.seed = 0;
        for(int i=0; i<dataFrame[0].Length; i++){
			if(dataFrame[0][i].active == 1){
				dataFrame[0][i].frame = UnityEngine.Random.Range(0,mitosisPeriod);
                //UnityEngine.Random();
			}
		}

        for(int fr=0; fr < NrFrames - 1; fr++){
			CustomUpdate(fr);
            
            for(int n=0; n<dataFrame[fr].Length; n++){
                dataFrame[fr][n].rotatedPosition = finMatrix.R.Transpose() * (dataFrame[fr][n].position - centre[fr]) + centre[fr];
                dataFrame[fr][n].principalAxis =  finMatrix.R.Transpose() * dataFrame[fr][n].principalAxis;
            }
            
            ComputeOrientationDistribution(fr, true);
		}

        //*
        int frm = NrFrames-1;
        int frmActiveCount = 0;
        for(int n=0; n<dataFrame[frm].Length; n++){
            if(dataFrame[frm][n].active == 1){
                centre[frm] += dataFrame[frm][n].position;
                frmActiveCount++;
            }
        }
        centre[frm] /= frmActiveCount;

        for(int n=0; n<dataFrame[frm].Length; n++){
            FindNeighbours(frm);
            ComputeOrientation(frm, true);
            if(dataFrame[frm][n].active == 1){
                dataFrame[frm][n].rotatedPosition = finMatrix.R.Transpose() * (dataFrame[frm][n].position - centre[frm]) + centre[frm];
                dataFrame[frm][n].principalAxis =  finMatrix.R.Transpose() * dataFrame[frm][n].principalAxis;
            }
        }
        ComputeOrientationDistribution(frm, true);
        Debug.Log(meanStr);

        //*/
		ComputeOrientationDispersion();
        ComputeBoundingBox();
    }

    public override void Duplicate()
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

                    if(fr+1 != NrFrames - 1)
					    dataFrame[fr + 1][j].active = 0;
                    //dataFrame[fr + 1][j].position = new Vector3();

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
                                                //+ dataFrame[fr-1][i].secondaryAxis 
                                                //* dataFrame[fr-1][i].secondaryAxisLength/6;

                    //Debug.Log(fr + ", " + i +"," + dataFrame[fr-1][i].principalAxis);

					dataFrame[fr][i].position += - dataFrame[fr-1][i].principalAxis 
                                                    * dataFrame[fr-1][i].axisLength/6;
                                                    //dataFrame[fr-1][i].secondaryAxis 
                                                    //* dataFrame[fr-1][i].secondaryAxisLength/6;

					dataFrame[fr][i].frame = 0;
					aCount++;	
				}
			}
		}

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
        //Debug.Log(fr);
		FindNeighbours(fr);
		ComputeOrientation(fr, true);

        // Stochastic Gradient Descent
        /*
        GenerateMiniBatches(10);
        for(int i=0; i<miniBatches.Length; i++){
            ComputePotentialGradient(fr, miniBatches[i]);
        }
        //*/
		
        // shuffle
        int[] perm = new int[dataFrame[fr].Length];
        for(int i=0; i<perm.Length; i++){perm[i]=i;}
        //perm

		for(int i=0; i<dataFrame[fr].Length; i++){
			if(dataFrame[fr][i].active == 1){
				Vector3 gradient = ComputePotentialGradient(fr, i);
                //Vector3 gradient = dataFrame[fr][i].gradient; 
                
                Vector3 Fmig = -gradient;

                Vector3 FaR = ComputeAttRepForces(fr, i, 2*dL.magnitude);

                Vector3 force = neta * Fmig + .001f * FaR;
				//Vector3 updatedPosition = dataFrame[fr][i].position + force;
                Vector3 relativePosition = dataFrame[fr][i].position - lowestPoint;
                
                //force = 10*force;
                force = 5*force;
                //force = force;
                //*
                //float force3 = Vector3.Dot(force + relativePosition, axis[0][0]);
                //if(force3 < 0){

                    //float force1 = Vector3.Dot(force, axis[0][2]);
                    //float force2 = Vector3.Dot(force, axis[0][1]);
                    //force3 = -force3;
                    
                    //force = new Matrix3x3(axis[0][2], axis[0][1], axis[0][0]) 
                    //            * new Vector3(force1, force2, force3);
                //}
                //*/
                
                
                dataFrame[fr][i].velocity = force;
                //dataFrame[fr][i].position = dataFrame[fr][i].position + force;
                dataFrame[fr+1][i].position = dataFrame[fr][i].position + force;
                dataFrame[fr+1][i].active = 1;
				dataFrame[fr+1][i].frame = dataFrame[fr][i].frame + 1;
			}
		}
	}

    float meanNeighbourDistance = 0;
    public override void FindNeighbours(int fr)
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
}