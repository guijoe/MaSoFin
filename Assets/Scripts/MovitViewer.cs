using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;

public class MovitViewer : MonoBehaviour
{

    int nbOfFrames;
    public int frame;
    public int speed = 1;
    public bool play = false;
    public bool reset = false;
    public string folder = "C:/Users/18003111/OneDrive - MMU/Documents/movit/application.windows64/MovitData/distant/180420hZ";

    float Xscale = 1;
    float Yscale = 1;
    float Zscale = 1;
    float scale = .25f;

    float XLength = 512;
    float YLength = 512;
    float ZLength = 512;

    Vector3[] positions;

    int[] startOfFrame;
    int[] nbOfCellsPerFrame;
    Dictionary<int, CellData>[] dataPerCellPerFrame;
    string[][] allData;

    public CellGO cellPrefab;
    Dictionary<int, CellGO> cells;
    Color[] selectionColors;

    // Use this for initialization
    void Start()
    {
        ViewerStart();
    }

    // Update is called once per frame
    void Update()
    {

        int count = 0;
        //nbOfFrames = 10;
        if (play)
        {
            while (frame < nbOfFrames && count++ < speed)
            {
                PlayResults(frame);
                frame++;
                Debug.Log(cells.Count);
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

    public void ViewerStart()
    {
        selectionColors = new Color[]
        {
            Color.red,
            Color.magenta,
            Color.yellow,
            Color.green,
            Color.blue,
            Color.gray
        };


        Debug.Log("Reading parameters ...");
        ReadParameters();

        Debug.Log("Reading results ...");
        ReadResults();

        Debug.Log("Creating cell population ...");
        CreateCellPopulation();

        PlayResults(0);
    }

    public void ReadParameters()
    {
        string paramsFile = folder + "/180420hZ_21361.xml";

        XmlReader reader = XmlReader.Create(paramsFile);
        while (reader.Read())
        {
            if (reader.IsStartElement())
            {
                nbOfFrames = int.Parse(reader["nbTimesteps"]);
                Xscale = float.Parse(reader["db_pixelSizeX"]);
                Yscale = float.Parse(reader["db_pixelSizeY"]);
                Zscale = float.Parse(reader["db_pixelSizeZ"]);

                float XLength = float.Parse(reader["db_nbPixelX"]);
                float YLength = float.Parse(reader["db_nbPixelY"]);
                float ZLength = float.Parse(reader["db_nbPixelZ"]);
            }
        }
    }

    public void ReadResults()
    {
        string logFile = folder + "/test.csv";

        string[] results = File.ReadAllLines(logFile);

        nbOfCellsPerFrame = new int[nbOfFrames];
        allData = new string[results.Length - 1][];

        //Debug.Log(results.Length);

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

        //int fr = 1;
        int line = 0;
        while (line < allData.Length)
        {
            int lineFr = int.Parse(allData[line][2]);

            if (lineFr <= nbOfFrames)
                nbOfCellsPerFrame[lineFr - 1]++;

            line++;
        }

        int start = 0;
        dataPerCellPerFrame = new Dictionary<int, CellData>[nbOfFrames];
        for (int fr = 1; fr <= nbOfFrames; fr++)
        {
            dataPerCellPerFrame[fr - 1] = new Dictionary<int, CellData>(nbOfCellsPerFrame[fr - 1]);

            if(fr > 1) start += nbOfCellsPerFrame[fr - 2];
            
            for (int i = 0; i < nbOfCellsPerFrame[fr - 1]; i++)
            {
                int index = start + i;

                if(index < allData.Length)
                {
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
                
                //Debug.Log(index);
            }
            
        }
    }

    public void CreateCellPopulation()
    {
        int fr = 0;
        cells = new Dictionary<int, CellGO>();
        foreach (KeyValuePair<int, CellData> item in dataPerCellPerFrame[fr])
        {
            CellGO cell = Instantiate(cellPrefab);
            cell.globalId = item.Key;

            CellData cellData = item.Value;
            cell.motherId = cellData.mother;
            cell.position = cellData.position;
            cell.selection = cellData.selection;

            //Debug.Log(cellData.selection);

            cell.name = cell.globalId + "";
            cell.transform.SetParent(transform);
            cell.transform.localPosition = cellData.position;

            cell.GetComponent<MeshRenderer>().material.color = selectionColors[cellData.selection - 1];

            cells.Add(cell.globalId, cell);
        }
    }

    public void PlayResults(int fr)
    {
        List<int> toBeRemoved = new List<int>();
        //*
        foreach (KeyValuePair<int, CellData> item in dataPerCellPerFrame[fr])
        {
            CellGO c;
            if (cells.TryGetValue(item.Value.mother, out c))
            {
                Debug.Log("Mother found: " + item.Key);
                c.motherId = c.globalId;
                c.globalId = item.Key;
                c.name = item.Key + "";
                c.transform.localPosition = item.Value.position;

                cells.Add(c.globalId, c);
                toBeRemoved.Add(item.Value.mother);
            }
            else
            {
                Debug.Log("Mother not found: " + item.Key);

                //if (!cells.TryGetValue(item.Key, out c))
                    //AddCell(item.Key, item.Value);
            }
        }
        //*/
        for(int i=0; i < toBeRemoved.Count; i++)
        {
            cells.Remove(toBeRemoved[i]);
        }
    }

    public void AddCell(int id, CellData cellData)
    {
        CellGO cell = Instantiate(cellPrefab);
        cell.globalId = id;

        cell.motherId = cellData.mother;
        cell.position = cellData.position;
        cell.selection = cellData.selection;

        cell.name = cell.globalId + "";
        cell.transform.SetParent(transform);
        cell.transform.localPosition = cellData.position;

        cell.GetComponent<MeshRenderer>().material.color = selectionColors[cellData.selection - 1];

        cells.Add(id, cell);
    }

    public void OnDrawGizmos()
    {
        //DrawLineages();
    }

    public void DrawLineages()
    {
        foreach (KeyValuePair<int, CellGO> item in cells)
        {
            CellGO c;
            if (cells.TryGetValue(item.Value.motherId, out c))
            {
                Gizmos.DrawLine(item.Value.position, c.position);
            }
        }
    }
}