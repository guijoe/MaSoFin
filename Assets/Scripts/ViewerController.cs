using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewerController : MonoBehaviour {

    protected int NrPlots = 13;//6;//
    protected float[] AxisLengths;
    public float[][][] tuples;

    protected DataInterface timeSeries;

    protected Plot4D[] plots;
    public Plot4D plot4DPrefab;

    //public int[][] metricIndices;

    public bool play = false;
    public bool reset = false;

    public bool enhancePlot = true;

    public int frame = 0;

    protected int[] dim;
    public static Vector3[] NSquaredInN;

    public string folder = "C:/Users/18003111/OneDrive - MMU/Documents/movit/application.windows64/MovitData/distant/180420hZ";
    public string dataFile = "test_06012020.csv";
    public string paramsFile = "180420hZ_21361.xml";

    public virtual void InitPlots(){}

    public virtual void GetData(){}

    // Use this for initialization
    void Awake () {
        GetData();

        dim = new int[2];
        dim[1] = (int)Mathf.Floor(Mathf.Sqrt(NrPlots));
        dim[0] = dim[1]  + 1;
        dim[0] = 5;
        dim[1] = 3;
        SetNSquaredInN(dim[0], dim[1]);

        plots = new Plot4D[NrPlots];
        AxisLengths = new float[3] { 10, 10, 10 };
        for (int i = 0; i < NrPlots; i++)
        {
            plots[i] = Instantiate(plot4DPrefab);
            plots[i].AxisLengths = AxisLengths;

            plots[i].transform.SetParent(transform);
            //plots[i].transform.position = (AxisLengths[0] + 3) * NSquaredInN[i];
            plots[i].transform.position = new Vector3 (20 * NSquaredInN[i].x, 15 * NSquaredInN[i].y, 0);

            plots[i].speed = 1;
        }

        resetValues = new int[15]{0,258, 515, 0, 258, 515, 0, 258, 515, 515, 515, 515, 515, 515, 515};
        tuples = timeSeries.DataFrameToTuples();
        InitPlots();
    }

    // Update is called once per frame

    public int[] resetValues;
        
    void Update () {

        //play = Input.GetKeyDown("p");
        if (reset)
        {
            play = false;
            reset = false;

            for (int i = 0; i < NrPlots; i++)
            {
                frame = 0;
                //plots[i].frame = 0;
                plots[i].frame = resetValues[i];
                plots[i].PlayMetrics(plots[i].frame);
            }
        }

        if (enhancePlot)
        {
            for (int i = 0; i < NrPlots; i++)
            {
                plots[i].enhancePlot = enhancePlot;
            }
        }else{
            for (int i = 0; i < NrPlots; i++)
            {
                //plots[i].enhancePlot = enhancePlot;
            }
        }

        for (int i = 0; i < NrPlots; i++)
        {
            plots[i].play = play;
            plots[i].reset = reset;
            frame = plots[i].frame;
        }
    }

    public void SetNSquaredInN(int dimX, int dimY)
    {
        NSquaredInN = new Vector3[dimX * dimY];
        int i = 0;
        for (int x = 0; x < dimX; x++)
        {
            for (int y = 0; y < dimY; y++)
            {
                //NSquaredInN[i] = new Vector3(x, -y, 0);
                NSquaredInN[i] = new Vector3(y, -x, 0);
                i++;
            }
        }
    }
}