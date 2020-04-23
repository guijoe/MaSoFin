using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plot4D : MonoBehaviour {

    //*
    public bool temporal = false;
    public float[][] XYZbounds;
    float[][] m_c;
    public float[] AxisLengths;
    float[][][] metrics; 

    public CellGO cellPrefab1;
    public CellGO cellPrefab2;
    public bool defaultPrefab = true;
    public bool flipXYAxis = false;
    List<CellGO> cells;

    int NrFrames = 0;
    public int[] NrClasses;//5;

    [Range(0, 516)]
    public int frame = 0;
    public int speed = 1;
    public bool play = true;
    public bool reset = false;
    public bool displayCells = true;
    public bool enhancePlot = false;

    public bool enabled = true;
    Vector3[] principalAxis;

    public int[] metricIndices;
    List<Color> selectionColors;

    public bool displayAxis = true;

    public bool colorGradient = false;

    int activeCell = 0;

    public bool userSetBounds = false;
    public bool whiteBackground = false;

    public string[] axisLabels;
    public string[] significantPlaces;

    public int nbOfSeries = 1;
    public int[][] complementarySeries;

    public bool drawLines=false;

    void Awake(){
        XYZbounds = new float[3][];

        for (int i = 0; i < XYZbounds.Length; i++){
            XYZbounds[i] = new float[2] {0, 0};
        }

        axisLabels = new string[3]{"x(μm)", "y(μm)", "z(μm)" };
        NrClasses = new int[3]{3, 3, 3};
        significantPlaces = new string[3]{"0", "0", "0"};

        complementarySeries = new int[nbOfSeries-1][];
    }

    // Use this for initialization
    void Start()
    {
        selectionColors = new List<Color>()
        {
            Color.white,
            Color.green,
            Color.yellow,
            Color.blue,
            new Color(255, 165, 0),
            new Color(238,130,238),
            Color.white,
            Color.red
        };

        if(whiteBackground){
            selectionColors = new List<Color>()
            {
                Color.black,
                Color.green,
                Color.yellow,
                Color.blue,
                new Color(255, 165, 0),
                new Color(238,130,238),
                Color.black,
                Color.red
            };
            //Debug.Log("hello");
        }

        metrics = GetComponentInParent<ViewerController>().tuples;
        NrFrames = metrics.Length;

        PreparePlot();
        SetPlotScale();
        CreateCellPopulation(defaultPrefab);

        activeCell = Random.Range(0, metrics[frame].Length);
        while ((int)metrics[frame][activeCell][metricIndices[4]] == 0)
        {
            activeCell = Random.Range(0, metrics[frame].Length);
        }

        PlayMetrics(0);

        Transform timeLabel = transform.GetChild(1).GetChild(7);
        float time = 28 + frame * (47.85f - 28)/518;
        string timeStr = time.ToString("0.0");
        int nbChars = timeStr.Length;
        timeLabel.GetComponent<TextMesh>().text = timeStr + "hpf";

        if(nbChars > 3 && frame == 0){
            Vector3 pos = timeLabel.transform.localPosition;
            timeLabel.transform.localPosition += new Vector3(-.1f-(nbChars-3), 0,0);
        }

        //Debug.Log("Hello Start Plot4D");
    }

    // Update is called once per frame
    void Update()
    {
        int count = 0;
        if (play)
        {
            while (frame < NrFrames-1 && count++ < speed)
            {
                PlayMetrics(frame);
                frame++;
            }
            if (reset)
            {
                reset = false;
                play = false;
                //frame = 0;
                PlayMetrics(frame);
            }
        }
        if(slice){
            HideCells();   
        }else{
            for(int i=0; i<cells.Count; i++){
                if(cells[i].active == 1){
                    cells[i].GetComponent<MeshRenderer>().enabled = displayCells;;
                }else{
                    cells[i].GetComponent<MeshRenderer>().enabled = false;;
                }
            }
        }
    }

    public Vector3 sliceNormal;
    public bool slice = false;
    public void HideCells(){
        Vector3 centre = new Vector3();
        int countActive = 0;

        for(int i=0; i<cells.Count; i++){
            if(cells[i].active == 1){
                centre += cells[i].transform.localPosition;
                countActive++;
            }
        }
        centre /= countActive;

        centre = new Vector3(0,0,2.5f);
        for(int i=0; i<cells.Count; i++){
            if(Vector3.Dot(cells[i].transform.localPosition - centre, sliceNormal) < 0){
                cells[i].GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    int currentPoint = 0;
    public void CreateCellPopulation(bool defaultPrefab)
    {
        int fr = 0;
        int count = 0;
        cells = new List<CellGO>();
        
        if (!temporal)
        {
            for (int n = 0; n < metrics[fr].Length; n++)
            {
                CellGO cell;
                if(defaultPrefab){
                    cell = Instantiate(cellPrefab1);
                }else{
                    cell = Instantiate(cellPrefab2);
                }
                
                cell.transform.SetParent(transform.GetChild(0));
                cell.transform.localPosition = Vector3.zero;
                cell.GetComponent<MeshRenderer>().enabled = displayCells;
                cells.Add(cell);

                count++;
            }
            if(nbOfSeries > 1){
                for(int i=0; i<nbOfSeries-1; i++){
                    for (int n = 0; n < metrics[fr].Length; n++)
                    {
                        CellGO cell;
                        if(defaultPrefab){
                            cell = Instantiate(cellPrefab1);
                        }else{
                            cell = Instantiate(cellPrefab2);
                        }
                        
                        cell.transform.SetParent(transform.GetChild(0));
                        cell.transform.localPosition = Vector3.zero;
                        //cell.GetComponent<MeshRenderer>().material.color = selectionColors[(int)metrics[fr][n][complementarySeries[i][3]]];
                        //cell.GetComponent<MeshRenderer>().enabled = displayCells;
                        cells.Add(cell);

                        count++;
                    }
                }
            }
        }
        else
        {
            for (int n = 0; n < NrFrames; n++)
            {
                CellGO cell;
                if(defaultPrefab){
                    cell = Instantiate(cellPrefab1);
                }
                else{
                    cell = Instantiate(cellPrefab2);
                }

                cell.transform.SetParent(transform.GetChild(0));
                cell.transform.localPosition = Vector3.zero;

                //cell.GetComponent<MeshRenderer>().material.color = selectionColors[(int)metrics[fr][activeCell][metricIndices[3]]];
                cells.Add(cell);
                count++;
            }
            if(nbOfSeries > 1){
                for(int i=0; i<nbOfSeries-1; i++){
                    for (int n = 0; n < NrFrames; n++)
                    {
                        CellGO cell;
                        cell = Instantiate(cellPrefab1);
                        
                        cell.transform.SetParent(transform.GetChild(0));
                        cell.transform.localPosition = Vector3.zero;

                        cell.GetComponent<MeshRenderer>().material.color = Color.red;
                        cells.Add(cell);
                        count++;
                    }
                }
            }
        }
    }

    void PreparePlot()
    {
        Transform axis = transform.GetChild(1);
        Transform XYPlane = axis.GetChild(6);

        Transform XAxis = axis.GetChild(0);
        Transform XLabel = axis.GetChild(1);
        Transform YAxis = axis.GetChild(2);
        Transform YLabel = axis.GetChild(3);
        Transform ZAxis = axis.GetChild(4);
        Transform ZLabel = axis.GetChild(5);

        XLabel.GetComponent<TextMesh>().text = axisLabels[0];
        YLabel.GetComponent<TextMesh>().text = axisLabels[1];
        ZLabel.GetComponent<TextMesh>().text = axisLabels[2];

        if(whiteBackground){
            XYPlane.GetComponent<MeshRenderer>().material.color = Color.white;
            XYPlane.GetComponent<MeshRenderer>().enabled = true;
            
            XAxis.GetComponent<MeshRenderer>().material.color = Color.black;
            YAxis.GetComponent<MeshRenderer>().material.color = Color.black;
            ZAxis.GetComponent<MeshRenderer>().material.color = Color.black;

            XLabel.GetComponent<TextMesh>().color = Color.black;
            YLabel.GetComponent<TextMesh>().color = Color.black;
            ZLabel.GetComponent<TextMesh>().color = Color.black;
            
            //Debug.Log(axis);
        }
    }

    void SetPlotScale()
    {
        if(!userSetBounds){
            
            int activeC = Random.Range(0, metrics[0].Length);
            while ((int)metrics[0][activeC][metricIndices[4]] == 0)
            {
                activeC = Random.Range(0, metrics[0].Length);
            }

            for (int i = 0; i < XYZbounds.Length; i++)
            {
                XYZbounds[i] = new float[2] { metrics[0][activeC][metricIndices[i]], -metrics[0][activeC][metricIndices[i]] };
            }

            for (int fr = 0; fr < metrics.Length; fr++)
            {
                for (int n = 0; n < metrics[fr].Length; n++)
                {
                    for (int j = 0; j < XYZbounds.Length; j++)
                    {
                        if (metrics[fr][n][metricIndices[j]] < XYZbounds[j][0]) //metrics[frame][n][metricIndices[4]] == 1 && 
                        {
                            XYZbounds[j][0] = metrics[fr][n][metricIndices[j]];
                        }
                        if (metrics[fr][n][metricIndices[j]] > XYZbounds[j][1])
                        {
                            XYZbounds[j][1] = metrics[fr][n][metricIndices[j]];
                        }
                        //Debug.Log(fr + ", " + XYZbounds[j][0] + ", " + XYZbounds[j][1]);
                    }
                }
            }
        }
        
        /*
        if(userSetBounds){
            XYZbounds[0][0] = 0;
            XYZbounds[1][0] = 0;
            XYZbounds[2][0] = 0;
        }
        //*/

        //Debug.Log(XYZbounds[0][0] + ", " + XYZbounds[0][0]);

        //Debug.Log("M_C... ");
        m_c = new float[3][];
        for (int i = 0; i < XYZbounds.Length; i++)
        {
            if (XYZbounds[i][1] == XYZbounds[i][0])
            {
                m_c[i] = new float[2] { 1f, 0 };
            }
            else
            {
                m_c[i] = new float[2] { AxisLengths[i] / (XYZbounds[i][1] - XYZbounds[i][0]), 
                                        AxisLengths[i] - XYZbounds[i][1] * AxisLengths[i] / (XYZbounds[i][1] - XYZbounds[i][0]) };
            }

            
        }
    }

    public void PlayMetrics(int frame)
    {
        Vector3 metric = new Vector3();

        
        if (!temporal)
        {

            Transform timeLabel = transform.GetChild(1).GetChild(7);
            float time = 28 + frame * (47.85f - 28)/518;
            timeLabel.GetComponent<TextMesh>().text = time.ToString("0.0") + "hpf";

            for (int n = 0; n < metrics[frame].Length; n++)
            //for (int n = 0; n < cells.Count; n++)
            {
                metric = new Vector3(metrics[frame][n][metricIndices[0]],
                                     metrics[frame][n][metricIndices[1]],
                                     metrics[frame][n][metricIndices[2]]);

                metric = new Vector3(m_c[0][0] * metric.x + m_c[0][1],
                                     m_c[1][0] * metric.y + m_c[1][1],
                                     m_c[2][0] * metric.z + m_c[2][1]
                                                                );
                //*
                if(defaultPrefab){
                    cells[n].transform.localPosition = new Vector3(metric.x,metric.y,metric.z);
                }else if(!flipXYAxis){
                    cells[n].transform.localPosition = new Vector3(metric.x,metric.y/2,metric.z);
                    cells[n].transform.localScale = new Vector3(.2f,metric.y,.2f);  
                }else{
                    cells[n].transform.localPosition = new Vector3(metric.x/2,metric.y,metric.z);
                    cells[n].transform.localScale = new Vector3(metric.x,.2f,.2f);
                }
                
                
                cells[n].active = (int)metrics[frame][n][metricIndices[4]];
                
                if(!colorGradient){
                    //Debug.Log("Selection color: " + (int)metrics[frame][n][metricIndices[3]]);
                    cells[n].GetComponent<MeshRenderer>().material.color = selectionColors[(int)metrics[frame][n][metricIndices[3]]];
                }else if(colorGradient && cells[n].active == 1){
                    cells[n].GetComponent<MeshRenderer>().material.color = new Color(1f, metrics[frame][n][metricIndices[5]], 0);
                    //Debug.Log(new Color(1f, metrics[frame][n][metricIndices[5]],0));
                }else{
                    cells[n].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0);
                }
                //*/
            }
        
            if(nbOfSeries > 1){
                for(int i=0; i<nbOfSeries-1; i++){
                    for (int n = 0; n < metrics[frame].Length; n++){
                        metric = new Vector3(metrics[frame][n][complementarySeries[i][0]],
                                     metrics[frame][n][complementarySeries[i][1]],
                                     metrics[frame][n][complementarySeries[i][2]]);

                        metric = new Vector3(m_c[0][0] * metric.x + m_c[0][1],
                                            m_c[1][0] * metric.y + m_c[1][1],
                                            m_c[2][0] * metric.z + m_c[2][1]
                                                                        );
                        //*
                        cells[(i+1) * metrics[frame].Length + n].transform.localPosition = new Vector3(metric.x,metric.y,metric.z);
                        cells[(i+1) * metrics[frame].Length + n].active = (int)metrics[frame][n][metricIndices[4]];
                        
                        cells[(i+1) * metrics[frame].Length + n].GetComponent<MeshRenderer>().material.color = Color.red;
                        //*/
                    }
                }
            }
        }
        else
        {
            if(frame == 0)
            {
                //int proportionateCellCount = cells.Count / (nbOfSeries);
                for (int n = 0; n < NrFrames; n++)
                //for (int n = 0; n < cells.Count; n++)
                {
                    metric = new Vector3(metrics[frame][activeCell][metricIndices[0]],
                                         metrics[frame][activeCell][metricIndices[1]],
                                         metrics[frame][activeCell][metricIndices[2]]);

                    //*
                    cells[n].transform.localPosition = new Vector3(m_c[0][0] * metric.x + m_c[0][1],
                                                                    m_c[1][0] * metric.y + m_c[1][1],
                                                                    m_c[2][0] * metric.z + m_c[2][1]
                                                                    );
                    //*/
                    cells[n].GetComponent<MeshRenderer>().material.color = selectionColors[(int)metrics[frame][activeCell][metricIndices[3]]];
                    cells[frame].active = 1;
                }
            }
            else if (frame < NrFrames)
            {
                metric = new Vector3(metrics[frame][activeCell][metricIndices[0]],
                                     metrics[frame][activeCell][metricIndices[1]],
                                     metrics[frame][activeCell][metricIndices[2]]);

                cells[frame].transform.localPosition = new Vector3( m_c[0][0] * metric.x + m_c[0][1],
                                                                    m_c[1][0] * metric.y + m_c[1][1],
                                                                    m_c[2][0] * metric.z + m_c[2][1]
                                                                    );
                cells[frame].active = 1;
                cells[frame].GetComponent<MeshRenderer>().material.color = selectionColors[(int)metrics[frame][activeCell][metricIndices[3]]];
            }
        
            if(nbOfSeries > 1){
                for(int i=0; i<nbOfSeries-1; i++){
                    if(frame == 0)
                    {
                        //for (int n = 0; n < cells.Count; n++)
                        for (int n = 0; n < NrFrames; n++)
                        {
                            int k = (i+1) * frame;
                            metric = new Vector3(metrics[frame][activeCell][complementarySeries[i][0]],
                                                 metrics[frame][activeCell][complementarySeries[i][1]],
                                                 metrics[frame][activeCell][complementarySeries[i][2]]);

                            //*
                            cells[(i+1)*NrFrames+n].transform.localPosition = new Vector3(m_c[0][0] * metric.x + m_c[0][1],
                                                                                m_c[1][0] * metric.y + m_c[1][1],
                                                                                m_c[2][0] * metric.z + m_c[2][1]
                                                                            );
                            //*/
                            cells[(i+1)*NrFrames+n].GetComponent<MeshRenderer>().material.color = Color.red;
                            cells[(i+1)*NrFrames+n].active = 1;
                        }   
                    }
                    else if (frame < NrFrames)
                    {
                        metric = new Vector3(metrics[frame][activeCell][complementarySeries[i][0]],
                                             metrics[frame][activeCell][complementarySeries[i][1]],
                                             metrics[frame][activeCell][complementarySeries[i][2]]);

                        cells[(i+1)*NrFrames+frame].transform.localPosition = new Vector3( m_c[0][0] * metric.x + m_c[0][1],
                                                                            m_c[1][0] * metric.y + m_c[1][1],
                                                                            m_c[2][0] * metric.z + m_c[2][1]
                                                                            );
                        cells[(i+1)*NrFrames+frame].active = 1;
                        cells[(i+1)*NrFrames+frame].GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if(enhancePlot)
                EnhancePlot();
            if(metricIndices.Length > 6 || drawLines)
                DrawOrientation();
        }
    }

    public void EnhancePlot()
    {
        Gizmos.color = new Color(255f, 255f, 255f, .1f);
        if(whiteBackground)
            Gizmos.color = new Color(0f, 0f, 0f, .1f);

        for (int i = 0; i <= 10; i++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(i, 0, 0), transform.position + new Vector3(i, 0, 10));
            Gizmos.DrawLine(transform.position + new Vector3(0, i, 0), transform.position + new Vector3(0, i, 10));

            Gizmos.DrawLine(transform.position + new Vector3(i, 0, 0), transform.position + new Vector3(i, 10, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0, 0, i), transform.position + new Vector3(0, 10, i));

            Gizmos.DrawLine(transform.position + new Vector3(0, i, 0), transform.position + new Vector3(10, i, 0));
            Gizmos.DrawLine(transform.position + new Vector3(0, 0, i), transform.position + new Vector3(10, 0, i));
        }

        //Gizmos.color = new Color(255f, 255f, 255f);
        GUIStyle style = new GUIStyle();
        style.fontSize = 10;
        style.normal.textColor = Color.white;
        if(whiteBackground)
            style.normal.textColor = Color.black;
        
        for (int i = 0; i <= NrClasses[0]; i++)
        {
            float text = (XYZbounds[0][0] + i * (XYZbounds[0][1] - XYZbounds[0][0]) / NrClasses[0]);// * (180 / Mathf.PI);
            string textStr = text.ToString("0");
            int nbChars = textStr.Length; //Debug.Log(nbChars);
            UnityEditor.Handles.Label(transform.position + new Vector3(AxisLengths[0]/NrClasses[0]*i-.5f * (nbChars-1) , -.2f, 0), textStr, style);
        }

        for (int i = 0; i <= NrClasses[1]; i++)
        {
            float text = XYZbounds[1][0] + i * (XYZbounds[1][1] - XYZbounds[1][0]) / NrClasses[1];
            string textStr = text.ToString(significantPlaces[1]);
            int nbChars = textStr.Length; //Debug.Log(nbChars);
            UnityEditor.Handles.Label(transform.position + new Vector3(-.5f - nbChars*.5f, AxisLengths[1]/NrClasses[1]*i+.75f, .3f), textStr, style);
            //UnityEditor.Handles.Label(transform.position + new Vector3(-1f, AxisLengths[1]/NrClasses[1]*i+.6f, .3f), text.ToString("0"), style);
        }

        for (int i = 0; i <= NrClasses[2]; i++)
        {
            float text = XYZbounds[2][0] + i * (XYZbounds[2][1] - XYZbounds[2][0]) / NrClasses[2];
            //UnityEditor.Handles.Label(transform.position + new Vector3(-1f, 0, AxisLengths[2]/NrClasses*i-.3f), text.ToString("0"), style);
        }
    }

    public void DrawOrientation()
    {
        //Vector3 zeroImage = new Vector3(m_c[0][1], m_c[1][1], m_c[2][1]);
        
        int fr = frame;
        if (frame >= NrFrames - 1)
        {
            fr = frame - 2;
        }
        
        Gizmos.color = Color.white;
        //for (int n = 0; n < cells.Count; n++)
        if(metricIndices.Length > 6){
            for (int n = 0; n < metrics[fr].Length; n++)
        {
            //Gizmos.color = selectionColors[(int)metrics[fr][n][metricIndices[3]]];
            //Gizmos.color = new Color(metrics[fr][n][metricIndices[5]], 1f, 0);
            Vector3 orientation = new Vector3(m_c[0][0] * metrics[fr][n][metricIndices[6]],
                                                m_c[1][0] * metrics[fr][n][metricIndices[7]],
                                                m_c[2][0] * metrics[fr][n][metricIndices[8]])
                                                * metrics[fr][n][metricIndices[9]]/2;

            Gizmos.DrawLine(transform.position + cells[n].transform.localPosition - orientation,
                                transform.position + cells[n].transform.localPosition + orientation);    
        }
        }
        

        Gizmos.color = Color.red;
        if(nbOfSeries>1 && drawLines){
            //Debug.Log("Hello");
            for(int i=0; i<nbOfSeries-1; i++){
                for (int n = 0; n < metrics[NrFrames-2].Length; n++)
                {
                    Vector3 orientation2 = new Vector3(m_c[0][0] * metrics[fr][n][complementarySeries[i][6]],
                                                    m_c[1][0] * metrics[fr][n][complementarySeries[i][7]],
                                                    m_c[2][0] * metrics[fr][n][complementarySeries[i][8]]);
                    if(cells[n].active == 1){
                        Gizmos.DrawLine(transform.position + cells[(i+1)*metrics[fr].Length + n].transform.localPosition,
                                    transform.position + cells[(i+1)*metrics[fr].Length + n].transform.localPosition + orientation2);
                    }
                }
            }   
        }
        
        if(metricIndices.Length > 9 && displayAxis)
        {
            Vector3 axis = new Vector3(m_c[0][0] * metrics[fr][activeCell][metricIndices[10]],
                                     m_c[1][0] * metrics[fr][activeCell][metricIndices[11]],
                                     m_c[2][0] * metrics[fr][activeCell][metricIndices[12]]);

            Vector3 centre = transform.localPosition + //new Vector3(transform.localPosition.x, 0, 0) +
                                        new Vector3(m_c[0][0] * metrics[fr][activeCell][metricIndices[13]] + m_c[0][1],
                                         m_c[1][0] * metrics[fr][activeCell][metricIndices[14]] + m_c[1][1],
                                         m_c[2][0] * metrics[fr][activeCell][metricIndices[15]] + m_c[2][1]);
            //Debug.Log(fr + ", " + centre);
            
            Gizmos.color = Color.red;
            axis.Normalize();
            axis = AxisLengths[0] * axis/2;
            //Gizmos.DrawLine(centre - axis,
            //                    centre + axis);

            /*
            Vector3 axis1 = new Vector3(m_c[0][0] * metrics[fr][activeCell][metricIndices[16]],
                                         m_c[1][0] * metrics[fr][activeCell][metricIndices[17]],
                                         m_c[2][0] * metrics[fr][activeCell][metricIndices[18]]);
            Gizmos.color = Color.green;
            axis1.Normalize();
            axis1 = AxisLengths[0] * axis1/2;
            Gizmos.DrawLine(centre - axis1,
                                centre + axis1);

            Gizmos.color = Color.yellow;
            Vector3 axis2 = new Vector3(m_c[0][0] * metrics[fr][activeCell][metricIndices[19]],
                                         m_c[1][0] * metrics[fr][activeCell][metricIndices[20]],
                                         m_c[2][0] * metrics[fr][activeCell][metricIndices[21]]);
            axis2.Normalize();
            axis2 = AxisLengths[0] * axis2/2;
            Gizmos.DrawLine(centre - axis2,
                                centre + axis2);
            //*/
        }
    }
}