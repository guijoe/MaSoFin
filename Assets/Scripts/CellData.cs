using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellData {

    public int ID { get; set; }
    public int fr;
    public int globalId { get; set; }
    public int mother { get; set; }
    public int uniqueMotherID = -1;
    public int twinID = -1;
    public int selection { get; set; }
    public int[] children;
    public Vector3 position = Vector3.zero;
    public Vector3 rotatedPosition = Vector3.zero;
    public Vector3 velocity = Vector3.zero;

    public int frame = 0;
    public Vector3 relativeVelocity = Vector3.zero;
    public Matrix3x3 matrix;
    public Vector3 principalAxis;
    public Vector3 secondaryAxis;
    public Vector3 tertiaryAxis;
    public float axisLength;
    public float secondaryAxisLength;
    public float tertiaryAxisLength;
    public int[] neighbourhood;
    public List<int> neighbours;
    public int active;
    public int timeActive = 0;
    public float elongationAngle = 0;
    public float elongationAngleClass = 0;
    public float elongationAngleClassFrequency = 0;
    public float elongationAngleMean = 0;
    public float elongationAngleSD = 0;
    public int cluster = 0;
    public int color = 0;
    public float XClass = 0;
    public float XClassFrequency = 0;
    public float YClass = 0;
    public float YClassFrequency = 0;
    public float ZClass = 0;
    public float ZClassFrequency = 0;
    public float zero = 0;
    public float one = 1f;
    public Vector3 globalPrincipalAxis = Vector3.zero;
    public Vector3 globalCentre = Vector3.zero;
    public Vector3 globalSecondaryAxis = Vector3.zero;
    public Vector3 globalTertiaryAxis = Vector3.zero;
    public float polarityVsVelocityAngle = 0;
    public float polarityVsVelocityAngleClass = 0;
    public float polarityVsVelocityAngleClassFrequency = 0;
    public float polarityVsVelocityAngleMean = 0;
    public float polarityVsVelocityAngleSD = 0;

    public float XRelativeClass = 0;
    public float YRelativeClass = 0;
    public float ZRelativeClass = 0;

    public float XClassRelativeFrequency = 0;
    public float YClassRelativeFrequency = 0;
    public float ZClassRelativeFrequency = 0;

    public float XClassIndex;
    public float YClassIndex;
    public float ZClassIndex;

    public float XClassRelativeIndex;
    public float YClassRelativeIndex;
    public float ZClassRelativeIndex;

    public float colorGradient = 1f;
    public float colorGradient1;
    public float colorGradient2;

    public float PDAxisLength;
    public float APAxisLength;
    public float DVAxisLength;

    public Vector3 boundsMin;
    
    public Vector3 gradient;

    public float XClassFrequencyVsInitial = 0;
    public float YClassFrequencyVsInitial = 0;
    public float ZClassFrequencyVsInitial = 0;

    public float XClassFrequencyVsCumulated = 0;
    public float YClassFrequencyVsCumulated = 0;
    public float ZClassFrequencyVsCumulated = 0;

    public float sin = 0;
    public float angle = 0;

    public Vector3 line;

    public CellData()
    {
        this.velocity = Vector3.zero;
        this.active = 0;
        this.children = new int[2];
        this.children[0] = this.children[1] = -1;
        neighbours = new List<int>();
    }

    public CellData(int fr, int j)
    {
        this.velocity = Vector3.zero;
        this.active = 0;
        this.children = new int[2];
        this.children[0] = this.children[1] = -1;
        this.ID = j;
        this.fr = fr;
        neighbours = new List<int>();
    }

    public CellData(int globalId, int mother, int selection, Vector3 position){
        this.mother = mother;
        this.selection = selection;
        this.position = position;
        this.children = new int[2];
        this.children[0] = this.children[1] = -1;
        neighbours = new List<int>();
        this.globalId = globalId;
    }

    public CellData(int globalid, int mother, int selection, Vector3 position, int fr)
    {
        this.mother = mother;
        this.selection = selection;
        this.position = position;
        this.globalId = globalId;
        neighbours = new List<int>();
    }

    public CellData(CellData cd)
    {
        this.mother = cd.mother;
        this.selection = cd.selection;
        this.position = new Vector3(cd.position.x, cd.position.y, cd.position.z);
        this.velocity = new Vector3(cd.velocity.x, cd.velocity.y, cd.velocity.z);
        //this.neighbourhood = cd.neighbourhood;
        this.globalId = cd.globalId;
        this.fr = cd.fr;
        this.children = new int[2];
        this.children[0] = cd.children[0];
        this.children[1] = cd.children[1];
        neighbours = new List<int>();
    }

    public string ToString()
    {
        string str = "";
        //str = fr + "," + ID + "," + VectorToString(velocity) //+ "," + VectorToString(position)
        //                + "," + VectorToString(principalAxis) + "," + axisLength + "," + active;// + ",";

        str = globalId + ";" + selection + ";" + (fr+1) + ";" + VectorToString(rotatedPosition) + ";" + mother + ";1;" 
                        + PDAxisLength + ";" + APAxisLength + ";" + DVAxisLength; 
                        //+ VectorToString(globalCentre) + ";" + VectorToString(globalPrincipalAxis) + ";" + VectorToString(globalSecondaryAxis)
                        //+ ";" + VectorToString(globalTertiaryAxis);

        //for(int i=0; i<neighbourhood.Length-1; i++)
        //{
            //str += neighbourhood[i] + ",";
        //}
        //str += neighbourhood[neighbourhood.Length - 1];

        return str;
    }

    public static string Header(int cellCount)
    {
        string str = "";
        str = "fr,ID"+//,X.x,X.y,X.z +
            ",V.x,V.y,V.z,U.x,U.y,U.z,L,On";

        str = "id_center;selection;timestep;x;y;z;id_mother;validationPDLength;APLength;DVLength;";
        //"gc.x;gc.y;gc.z;pa1.x;pa1.y;pa1.z;pa2.x;pa2.y;pa2.z;pa3.x;pa3.y;pa3.z";
        for (int i = 0; i < cellCount - 1; i++)
        {
            //str += i + ",";
        }
        //str += cellCount - 1;
        return str;
    }

    public string VectorToString(Vector3 v)
    {
        string str = v.ToString("0.0000");

        return str.Replace("(", "").Replace(")", "").Replace(" ", "");
        return v.x + ";" + v.y + ";" + v.z;
    }

    public void ComputePrincipalAxis(Vector3 finAxis, Vector3 centre)
    {
        matrix.sym_eigen();
        matrix.compute_matrix();
        

        float maxEigen = Mathf.Abs(matrix.eigenValues.x);
        int maxEigenIndex = 0;
        if (Mathf.Abs(matrix.eigenValues.y) > maxEigen)
        {
            maxEigen = Mathf.Abs(matrix.eigenValues.y);
            maxEigenIndex = 1;
        }
        if (Mathf.Abs(matrix.eigenValues.z) > maxEigen)
        {
            maxEigen = Mathf.Abs(matrix.eigenValues.z);
            maxEigenIndex = 2;
        }

        principalAxis = new Vector3(matrix.R[0, maxEigenIndex],
                            matrix.R[1, maxEigenIndex],
                            matrix.R[2, maxEigenIndex]);

        float dot = Vector3.Dot(principalAxis, finAxis);
        if(dot < 0)
        {
            principalAxis = -principalAxis;
            dot = -dot;
        }
        //principalAxis = dot > 0 ? principalAxis : -principalAxis;

        axisLength = Mathf.Sqrt(maxEigen);
        elongationAngle = Mathf.Acos(dot);// * 180 / Mathf.PI;

        Vector3 rdVec = UnityEngine.Random.onUnitSphere;
        while(rdVec.y<0){
            rdVec = UnityEngine.Random.onUnitSphere;
        }
        angle = Mathf.Acos(Vector3.Dot(rdVec, Vector3.up));
        //angle = Mathf.Acos(Vector3.Dot(UnityEngine.Random.onUnitSphere, Vector3.up));
        //elongationAngle = elongationAngle <= 90 ? elongationAngle : 180 - elongationAngle;

        //polarityVsVelocityAngle = Mathf.Acos(Vector3.Dot(velocity, finAxis)) * 180 / Mathf.PI;
        //polarityVsVelocityAngle = Mathf.Acos(Vector3.Dot(relativeVelocity, principalAxis));// * 180 / Mathf.PI;
        //polarityVsVelocityAngle = polarityVsVelocityAngle == 90 ? 0 : polarityVsVelocityAngle;

        //Debug.Log(elongationAngle);
    }

    public void ComputePrincipalAxis(Vector3[] finAxis, Vector3 centre)
    {
        matrix.sym_eigen();
        matrix.compute_matrix();
        
        float maxEigen = Mathf.Abs(matrix.eigenValues.x);
        int maxEigenIndex = 0;
        if (Mathf.Abs(matrix.eigenValues.y) > maxEigen)
        {
            maxEigen = Mathf.Abs(matrix.eigenValues.y);
            maxEigenIndex = 1;
        }
        if (Mathf.Abs(matrix.eigenValues.z) > maxEigen)
        {
            maxEigen = Mathf.Abs(matrix.eigenValues.z);
            maxEigenIndex = 2;
        }

        float minEigen = Mathf.Abs(matrix.eigenValues.x);
        int minEigenIndex = 0;
        if (Mathf.Abs(matrix.eigenValues.y) < minEigen)
        {
            minEigen = Mathf.Abs(matrix.eigenValues.y);
            minEigenIndex = 1;
        }
        if (Mathf.Abs(matrix.eigenValues.z) < minEigen)
        {
            minEigen = Mathf.Abs(matrix.eigenValues.z);
            minEigenIndex = 2;
        }

        principalAxis = new Vector3(matrix.R[0, maxEigenIndex],
                            matrix.R[1, maxEigenIndex],
                            matrix.R[2, maxEigenIndex]);

        tertiaryAxis = new Vector3(matrix.R[0, minEigenIndex],
                            matrix.R[1, minEigenIndex],
                            matrix.R[2, minEigenIndex]);
        
        int intermediateEigenIndex = 3 - minEigenIndex - maxEigenIndex;
        if(intermediateEigenIndex == 0)
            secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.x));
        if(intermediateEigenIndex == 1)
            secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.y));
        if(intermediateEigenIndex == 2)
            secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.z));
        
        secondaryAxis = new Vector3(matrix.R[0, intermediateEigenIndex],
                            matrix.R[1, intermediateEigenIndex],
                            matrix.R[2, intermediateEigenIndex]);

        float dot = Vector3.Dot(principalAxis, finAxis[0]);
        if(dot < 0)
        {
            principalAxis = -principalAxis;
            dot = -dot;
        }

        axisLength = Mathf.Sqrt(maxEigen);
        elongationAngle = Mathf.Acos(dot);// * 180 / Mathf.PI;

        Vector3 rdVec = UnityEngine.Random.onUnitSphere;
        while(rdVec.y<0){
            rdVec = UnityEngine.Random.onUnitSphere;
        }
        angle = Mathf.Acos(Vector3.Dot(rdVec, Vector3.up));

        tertiaryAxisLength = Mathf.Sqrt(Mathf.Abs(minEigen));

        //float dot1 = 0;

        /*
        if(maxEigenIndex == 0){
            secondaryAxis = new Vector3(matrix.R[0, 1], matrix.R[1, 1], matrix.R[2, 1]);
            tertiaryAxis = new Vector3(matrix.R[0, 2], matrix.R[1, 2], matrix.R[2, 2]);
            secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.y));
            tertiaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.z));
            if(Vector3.Dot(Vector3.Cross(principalAxis, secondaryAxis), tertiaryAxis) < 0){
                secondaryAxis = new Vector3(matrix.R[0, 2], matrix.R[1, 2], matrix.R[2, 2]);
                tertiaryAxis = new Vector3(matrix.R[0, 1], matrix.R[1, 1], matrix.R[2, 1]);
                secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.z));
                tertiaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.y));
            }
        }else if(maxEigenIndex == 1){
            secondaryAxis = new Vector3(matrix.R[0, 0], matrix.R[1, 0], matrix.R[2, 0]);
            tertiaryAxis = new Vector3(matrix.R[0, 2], matrix.R[1, 2], matrix.R[2, 2]);
            secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.x));
            tertiaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.z));
            if(Vector3.Dot(Vector3.Cross(principalAxis, secondaryAxis), tertiaryAxis) < 0){
                secondaryAxis = new Vector3(matrix.R[0, 2], matrix.R[1, 2], matrix.R[2, 2]);
                tertiaryAxis = new Vector3(matrix.R[0, 0], matrix.R[1, 0], matrix.R[2, 0]);
                secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.z));
                tertiaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.x));
            }
        }else{
            secondaryAxis = new Vector3(matrix.R[0, 0], matrix.R[1, 0], matrix.R[2, 0]);
            tertiaryAxis = new Vector3(matrix.R[0, 1], matrix.R[1, 1], matrix.R[2, 1]);
            secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.x));
            tertiaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.y));
            if(Vector3.Dot(Vector3.Cross(principalAxis, secondaryAxis), tertiaryAxis) < 0){
                secondaryAxis = new Vector3(matrix.R[0, 1], matrix.R[1, 1], matrix.R[2, 1]);
                tertiaryAxis = new Vector3(matrix.R[0, 0], matrix.R[1, 0], matrix.R[2, 0]);
                secondaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.y));
                tertiaryAxisLength = Mathf.Sqrt(Mathf.Abs(matrix.eigenValues.x));
            }
        }
        */
    }

    public float ComputeElongationAngle(Vector3 finAxis, Matrix3x3 mat)
    {
        mat.sym_eigen();
        mat.compute_matrix();
        
        float maxEigen = Mathf.Abs(mat.eigenValues.x);
        int maxEigenIndex = 0;
        if (Mathf.Abs(mat.eigenValues.y) > maxEigen)
        {
            maxEigen = Mathf.Abs(mat.eigenValues.y);
            maxEigenIndex = 1;
        }
        if (Mathf.Abs(mat.eigenValues.z) > maxEigen)
        {
            maxEigen = Mathf.Abs(mat.eigenValues.z);
            maxEigenIndex = 2;
        }

        principalAxis = new Vector3(mat.R[0, maxEigenIndex],
                            mat.R[1, maxEigenIndex],
                            mat.R[2, maxEigenIndex]);

        float dot = Vector3.Dot(principalAxis, finAxis);
        if(dot < 0)
        {
            principalAxis = -principalAxis;
            dot = -dot;
        }
        
        float elongationAngle1 = Mathf.Acos(dot);// * 180 / Mathf.PI;

        return elongationAngle1;
    }

    public float[] ToTuple()
    {
        float[] tuple = new float[78];
        tuple[0] = position.x;
        tuple[1] = position.y;
        tuple[2] = position.z;
        tuple[3] = velocity.x;
        tuple[4] = velocity.y;
        tuple[5] = velocity.z;
        tuple[6] = principalAxis.x;
        tuple[7] = principalAxis.y;
        tuple[8] = principalAxis.z;
        tuple[9] = axisLength * 4 * Mathf.Sqrt(3);
        tuple[10] = elongationAngle  * 180 / Mathf.PI;
        tuple[11] = elongationAngleClass * 180 / Mathf.PI;
        tuple[12] = elongationAngleClassFrequency;
        tuple[13] = cluster;
        tuple[14] = active;
        tuple[15] = selection;
        tuple[16] = (rotatedPosition.x - boundsMin.x) * 4; //rotatedPosition.x; //
        tuple[17] = (rotatedPosition.y - boundsMin.y) * 4; //rotatedPosition.y; //
        tuple[18] = (rotatedPosition.z - boundsMin.z) * 4; //rotatedPosition.z; //
        tuple[19] = (XClass) * 4;
        tuple[20] = XClassFrequency;
        tuple[21] = elongationAngleMean  * 180 / Mathf.PI;
        tuple[22] = elongationAngleSD * 180 / Mathf.PI;
        tuple[23] = 28 + fr * (47.85f - 28)/518;
        tuple[24] = zero;
        tuple[25] = globalPrincipalAxis.x;
        tuple[26] = globalPrincipalAxis.y;
        tuple[27] = globalPrincipalAxis.z;
        tuple[28] = (globalCentre.x - boundsMin.x) * 4;
        tuple[29] = (globalCentre.y - boundsMin.y) * 4;
        tuple[30] = (globalCentre.z - boundsMin.z) * 4;
        tuple[31] = globalSecondaryAxis.x;
        tuple[32] = globalSecondaryAxis.y;
        tuple[33] = globalSecondaryAxis.z;
        tuple[34] = globalTertiaryAxis.x;
        tuple[35] = globalTertiaryAxis.y;
        tuple[36] = globalTertiaryAxis.z;
        tuple[37] = polarityVsVelocityAngle;
        tuple[38] = polarityVsVelocityAngleClass;
        tuple[39] = polarityVsVelocityAngleClassFrequency;
        tuple[40] = polarityVsVelocityAngleMean;
        tuple[41] = polarityVsVelocityAngleSD;
        tuple[42] = one;
        tuple[43] = (YClass) * 4;
        tuple[44] = YClassFrequency;
        tuple[45] = (ZClass) * 4;
        tuple[46] = ZClassFrequency;
        tuple[47] = XClassIndex;
        tuple[48] = YClassIndex;
        tuple[49] = ZClassIndex;
        tuple[50] = colorGradient;
        tuple[51] = colorGradient1;
        tuple[52] = colorGradient2;
        tuple[53] = PDAxisLength * 4;
        tuple[54] = APAxisLength * 4;
        tuple[55] = DVAxisLength * 4;
        tuple[56] = XClassFrequencyVsInitial*100;
        tuple[57] = YClassFrequencyVsInitial*100;
        tuple[58] = ZClassFrequencyVsInitial*100;
        tuple[59] = sin;//*100;
        tuple[60] = angle * 180 / Mathf.PI;
        tuple[61] = XClassFrequencyVsCumulated*1000;
        tuple[62] = YClassFrequencyVsCumulated*1000;
        tuple[63] = ZClassFrequencyVsCumulated*1000;
        
        tuple[64] = XRelativeClass;
        tuple[65] = YRelativeClass;
        tuple[66] = ZRelativeClass;

        tuple[67] = XClassRelativeIndex * 5; 
        tuple[68] = YClassRelativeIndex * 5;
        tuple[69] = ZClassRelativeIndex * 5;

        tuple[70] = XClassRelativeFrequency;//*100;
        tuple[71] = YClassRelativeFrequency;//*100;
        tuple[72] = ZClassRelativeFrequency;//*100;

        tuple[73] = line.x * 180 / Mathf.PI;//*100;
        tuple[74] = line.y;//*100;
        tuple[75] = line.z;//*100;

        tuple[76] = (elongationAngleMean - elongationAngleSD)  * 180 / Mathf.PI;
        tuple[77] = (elongationAngleMean + elongationAngleSD)  * 180 / Mathf.PI;

        return tuple;
    }
}