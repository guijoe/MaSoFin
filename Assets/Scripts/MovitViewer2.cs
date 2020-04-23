using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;
using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Hull;

public class MovitViewer2 : MonoBehaviour {

    int nbOfFrames;
    public int frame = 0;
    public int speed = 1;
    public bool play = false;
    public bool reset = false;
    public bool drawMeshes = true;
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
    Vertex3[][] vertices;
    Vertex3[] centres;
    private DelaunayTriangulation3 delaunay;
    private VoronoiMesh3 voronoi;
    public float size = 50f;

    public bool drawLines;
    private List<Mesh> meshes;

    public CellGO cellPrefab;
    List<CellGO> cells;
    Color[] selectionColors;

    public Mesh mesh;

    // Use this for initialization
    void Start() {
        ViewerStart();
    }

    // Update is called once per frame
    void Update() {

        //nbOfFrames = 3;
        int count = 0;
        if (play)
        {
            while (frame < nbOfFrames - 1 && count++ < speed)
            {
                if (frame % 5 == 0)
                {
                    CreateMeshes(frame);
                    //DrawMeshes();
                }

                PlayResults(frame);
                Debug.Log(frame + ", " + cells.Count);
                frame++;
            }
            if (reset)
            {
                reset = false;
                play = false;
                frame = 0;
                CreateMeshes(frame);
                PlayResults(frame);
            }
        }
    }

    public void ViewerStart()
    {
        mesh = new Mesh();
        selectionColors = new Color[]
        {
            Color.red,
            new Color(255, 165, 0),
            Color.yellow,
            Color.green,
            Color.blue,
            new Color(238,130,238)
        };

        Debug.Log("Reading parameters ...");
        ReadParameters();

        Debug.Log("Reading results ...");
        ReadResults();

        Debug.Log("Creating cell population ...");
        CreateCellPopulation();

        CreateMeshes(0);
        //PlayResults(0);
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

        int start = 0;
        dataPerCellPerFrame = new Dictionary<int, CellData>[nbOfFrames];
        for (int fr = 1; fr <= nbOfFrames; fr++)
        {
            vertices[fr - 1] = new Vertex3[nbOfCellsPerFrame[fr - 1]];
            dataPerCellPerFrame[fr - 1] = new Dictionary<int, CellData>(nbOfCellsPerFrame[fr - 1]);

            if (fr > 1) start += nbOfCellsPerFrame[fr - 2];

            for (int i = 0; i < nbOfCellsPerFrame[fr - 1]; i++)
            {
                int index = start + i;

                //if (index < allData.Length)
                //{
                Vertex3 vert = new Vertex3(float.Parse(allData[index][3]) * Xscale * scale,
                                          float.Parse(allData[index][4]) * Yscale * scale,
                                          float.Parse(allData[index][5]) * Zscale * scale
                              );

                vertices[fr - 1][i] = vert;

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

                //}
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
                    if (item2.Value.mother == item1.Key) {
                        dataPerCellPerFrame[fr][item1.Key].children[++childCount] = item2.Key;
                    }
                }

                //if(childCount == 1)
                //Debug.Log(fr + ", " + dataPerCellPerFrame[fr][item1.Key].children[0] + ", " + dataPerCellPerFrame[fr][item1.Key].children[1]);
            }
        }
    }

    public void ProcessData()
    {

    }

    public void CreateCellPopulation()
    {
        int fr = 0;
        //cells = new Dictionary<int, CellGO>();
        cells = new List<CellGO>();
        int count = 0;
        foreach (KeyValuePair<int, CellData> item in dataPerCellPerFrame[fr])
        {
            CellGO cell = Instantiate(cellPrefab);
            cell.globalId = item.Key;

            CellData cellData = item.Value;
            cell.motherId = cellData.mother;
            cell.position = cellData.position;
            cell.selection = cellData.selection;
            cell.children = new int[2];
            cell.children[0] = cellData.children[0];
            cell.children[1] = cellData.children[1];

            cell.name = count + "_" + cell.globalId + "";
            cell.transform.SetParent(transform);
            cell.transform.localPosition = cellData.position;

            cell.GetComponent<MeshRenderer>().material.color = selectionColors[cellData.selection - 1];

            cells.Add(cell);
            count++;
        }
    }

    public void PlayResults(int fr)
    {
        List<int> toBeAdded = new List<int>();

        //Debug.Log(fr + ": " + dataPerCellPerFrame[fr].Count + ", " + dataPerCellPerFrame[fr + 1].Count);
        for (int i = 0; i < cells.Count; i++)
        {
            CellData c;
            int child1 = -1;
            int child2 = -1;

            if (cells[i].globalId != -1 && cells[i].children[0] != -1)
            {
                child1 = dataPerCellPerFrame[fr][cells[i].globalId].children[0];
                child2 = dataPerCellPerFrame[fr][cells[i].globalId].children[1];
            }

            if (child1 != -1)
            {
                cells[i].motherId = dataPerCellPerFrame[fr + 1][child1].mother;
                cells[i].globalId = child1;
                cells[i].name = i + "_" + child1;
                cells[i].transform.localPosition = dataPerCellPerFrame[fr + 1][child1].position;

                cells[i].children = new int[2];
                cells[i].children[0] = dataPerCellPerFrame[fr + 1][child1].children[0];
                cells[i].children[1] = dataPerCellPerFrame[fr + 1][child1].children[1];
            }

            if (child2 != -1)
            {
                toBeAdded.Add(child2);
            }
        }

        for (int i = 0; i < toBeAdded.Count; i++)
        {
            AddCell(toBeAdded[i], dataPerCellPerFrame[fr + 1][toBeAdded[i]]);
        }
    }

    public void AddCell(int id, CellData cellData)
    {
        CellGO cell = Instantiate(cellPrefab);
        cell.globalId = id;

        cell.motherId = cellData.mother;
        cell.position = cellData.position;
        cell.selection = cellData.selection;

        int fr = frame;
        cell.children = new int[2];
        cell.children[0] = cellData.children[0];
        cell.children[1] = cellData.children[1];

        cell.name = "new_" + cell.globalId;
        cell.transform.SetParent(transform);
        cell.transform.localPosition = cellData.position;

        cell.GetComponent<MeshRenderer>().material.color = selectionColors[cellData.selection - 1];

        cells.Add(cell);
    }

    public void CreateMeshes(int fr)
    {
        // Create Delaunay
        //delaunay = new DelaunayTriangulation3();
        //delaunay.Generate(vertices[fr], true, false, 5000);

        // Create Voronoi
        voronoi = new VoronoiMesh3();
        voronoi.Generate(vertices[fr]);
        RegionsToMeshes();

        /*
        int count = 0;
        foreach (DelaunayCell<Vertex3> cell in delaunay.Cells)
        {
            if(count == 44)
            {
                Vector3 circumCenter = new Vector3(cell.CircumCenter.X, cell.CircumCenter.Y, cell.CircumCenter.Z);
                float radius = cell.Radius;

                foreach (DelaunayCell<Vertex3> c1 in delaunay.Cells)
                {
                    Simplex<Vertex3> f1 = c1.Simplex;

                    Vector3 vec1 = new Vector3(f1.Vertices[0].X, f1.Vertices[0].Y, f1.Vertices[0].Z);
                    Vector3 vec2 = new Vector3(f1.Vertices[1].X, f1.Vertices[1].Y, f1.Vertices[1].Z);
                    Vector3 vec3 = new Vector3(f1.Vertices[2].X, f1.Vertices[2].Y, f1.Vertices[2].Z);
                    Vector3 vec4 = new Vector3(f1.Vertices[3].X, f1.Vertices[3].Y, f1.Vertices[3].Z);

                    Debug.Log(Vector3.Distance(circumCenter, vec1) + "; "
                                + Vector3.Distance(circumCenter, vec2) + "; "
                                + Vector3.Distance(circumCenter, vec3) + "; "
                                + Vector3.Distance(circumCenter, vec4) + "; "
                                + radius
                            );
                    
                }

                //Simplex<Vertex3> f = cell.Simplex;
                //Debug.Log(f.Vertices[0].X + "," + f.Vertices[0].Y + "," + f.Vertices[0].Z);
                //Debug.Log(f.Vertices[1].X + "," + f.Vertices[1].Y + "," + f.Vertices[1].Z);
                //Debug.Log(f.Vertices[2].X + "," + f.Vertices[2].Y + "," + f.Vertices[2].Z);
                //Debug.Log(f.Vertices[3].X + "," + f.Vertices[3].Y + "," + f.Vertices[3].Z);

                //Debug.Log(new Vector3(cell.CircumCenter.X, cell.CircumCenter.Y, cell.CircumCenter.Z));
                //Debug.Log((vec1 - vec2).magnitude + ", " + (vec1 - vec3).magnitude + ", " + (vec1 - vec4).magnitude + ", " + (vec2 - vec3).magnitude + ", " + (vec2 - vec4).magnitude + ", " + (vec3 - vec4).magnitude);
                //Debug.Log(Vector3.Distance(vec1, centre) + ", " + Vector3.Distance(vec2, centre) + ", " + Vector3.Distance(vec3, centre) + ", " + Vector3.Distance(vec4, centre));    
            }

            CellGO cl1 = Instantiate(cellPrefab);
            cl1.transform.localPosition = new Vector3(cell.Simplex.Vertices[0].X, cell.Simplex.Vertices[0].Y, cell.Simplex.Vertices[0].Z);
            CellGO cl2 = Instantiate(cellPrefab);
            cl2.transform.localPosition = new Vector3(cell.Simplex.Vertices[1].X, cell.Simplex.Vertices[1].Y, cell.Simplex.Vertices[1].Z);
            CellGO cl3 = Instantiate(cellPrefab);
            cl3.transform.localPosition = new Vector3(cell.Simplex.Vertices[2].X, cell.Simplex.Vertices[2].Y, cell.Simplex.Vertices[2].Z);
            CellGO cl4 = Instantiate(cellPrefab);
            cl4.transform.localPosition = new Vector3(cell.Simplex.Vertices[3].X, cell.Simplex.Vertices[3].Y, cell.Simplex.Vertices[3].Z);

            CellGO c = Instantiate(cellPrefab);
            c.transform.localPosition = new Vector3(cell.CircumCenter.X, cell.CircumCenter.Y, cell.CircumCenter.Z);
            c.GetComponent<MeshRenderer>().material.color = Color.red;
            c.name = count + "";
            count++;
        }
        //*/
    }

    private void RegionsToMeshes()
    {
        meshes = new List<Mesh>();

        int count = 0;
        //Debug.Log(voronoi.Regions.Count);
        /*
        foreach (VoronoiRegion<Vertex3> region in voronoi.Regions)
        {
            bool draw = true;

            List<Vertex3> verts = new List<Vertex3>();

            foreach (DelaunayCell<Vertex3> cell in region.Cells)
            {
                
                CellGO c = Instantiate(cellPrefab);
                c.transform.localPosition = new Vector3(cell.CircumCenter.X, cell.CircumCenter.Y, cell.CircumCenter.Z);
                c.GetComponent<MeshRenderer>().material.color = Color.red;
                c.name = count + "";
                count++;
            }
        }
        */

        //*
        foreach (VoronoiRegion<Vertex3> region in voronoi.Regions)
        {
            bool draw = true;

            List<Vertex3> verts = new List<Vertex3>();

            foreach (DelaunayCell<Vertex3> cell in region.Cells)
            {
                if (!InBound(cell.CircumCenter))
                {
                    draw = false;
                    break;
                }
                else
                {
                    verts.Add(cell.CircumCenter);
                    //verts.Add(cell.Centroid);
                }
            }

            if (!draw) continue;

            
            //If you find the convex hull of the voronoi region it
            //can be used to make a triangle mesh.

            ConvexHull3 hull = new ConvexHull3();
            hull.Generate(verts, false);
            //Debug.Log(hull.Simplexs.Count);

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            for (int i = 0; i < hull.Simplexs.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 v = new Vector3();
                    v.x = hull.Simplexs[i].Vertices[j].X;
                    v.y = hull.Simplexs[i].Vertices[j].Y;
                    v.z = hull.Simplexs[i].Vertices[j].Z;

                    positions.Add(v);
                }

                Vector3 n = new Vector3();
                n.x = hull.Simplexs[i].Normal[0];
                n.y = hull.Simplexs[i].Normal[1];
                n.z = hull.Simplexs[i].Normal[2];

                if (hull.Simplexs[i].IsNormalFlipped)
                {
                    indices.Add(i * 3 + 2);
                    indices.Add(i * 3 + 1);
                    indices.Add(i * 3 + 0);
                }
                else
                {
                    indices.Add(i * 3 + 0);
                    indices.Add(i * 3 + 1);
                    indices.Add(i * 3 + 2);
                }

                normals.Add(n);
                normals.Add(n);
                normals.Add(n);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetTriangles(indices, 0);

            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();

            meshes.Add(mesh);
            
        }
        //*/
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(100f/255,100f/255,100f/255);
        if (drawMeshes)
        {
            //DrawDelaunaySimplexes();
            DrawVoronoiMeshes();
        }   
    }

    private void DrawDelaunaySimplexes()
    {
        foreach (DelaunayCell<Vertex3> cell in delaunay.Cells)
        {
            Simplex<Vertex3> f = cell.Simplex;
            Gizmos.DrawLine(new Vector3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z),
                                new Vector3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z));

            Gizmos.DrawLine(new Vector3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z),
                                new Vector3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z));

            Gizmos.DrawLine(new Vector3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z),
                                new Vector3(f.Vertices[3].X, f.Vertices[3].Y, f.Vertices[3].Z));

            Gizmos.DrawLine(new Vector3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z),
                                new Vector3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z));

            Gizmos.DrawLine(new Vector3(f.Vertices[3].X, f.Vertices[3].Y, f.Vertices[3].Z),
                                new Vector3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z));

            Gizmos.DrawLine(new Vector3(f.Vertices[3].X, f.Vertices[3].Y, f.Vertices[3].Z),
                            new Vector3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z));
        }
    }

    public void DrawVoronoiMeshes()
    {
        if (meshes != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            bool value = true;
            foreach (Mesh mesh in meshes)
            {
                //Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);//, material, 0, Camera.main, 0, block, true, true);
                Gizmos.DrawMesh(mesh);

                if (frame == 0 && value)
                {
                    value = false;
                    for (int i = 0; i < mesh.vertices.Length; i++)
                    {
                        //Debug.Log(mesh.vertices[i]);
                    }
                }
            }

        }
    }

    private bool InBound(Vertex3 v)
    {
        if (v.X < -size || v.X > size) return false;
        if (v.Y < -size || v.Y > size) return false;
        if (v.Z < -size || v.Z > size) return false;

        return true;
    }
}