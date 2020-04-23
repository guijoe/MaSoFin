using System;
using System.Collections.Generic;

using HullDelaunayVoronoi.Hull;
using HullDelaunayVoronoi.Primitives;
using UnityEngine;

namespace HullDelaunayVoronoi.Delaunay
{

    public class DelaunayTriangulation3 : DelaunayTriangulation3<Vertex3>
    {

    }

    public class DelaunayTriangulation3<VERTEX> : DelaunayTriangulation<VERTEX>
        where VERTEX : class, IVertex, new()
    {
        private float[,] m_matrixBuffer;

        public DelaunayTriangulation3() : base(3)
        {
            m_matrixBuffer = new float[4, 4];
        }

        public override void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false)
        {
            Clear();
            if (input.Count <= Dimension + 1) return;

            int count = input.Count;
            for (int i = 0; i < count; i++)
            {
                float lenSq = input[i].SqrMagnitude;

                float[] v = input[i].Position;
                Array.Resize(ref v, Dimension + 1);
                input[i].Position = v;

                input[i].Position[Dimension] = (float)lenSq;
            }

            var hull = new ConvexHull<VERTEX>(Dimension + 1);
            hull.Generate(input, assignIds, checkInput);

            for (int i = 0; i < count; i++)
            {
                float[] v = input[i].Position;
                Array.Resize(ref v, Dimension);
                input[i].Position = v;
            }

            Vertices = new List<VERTEX>(hull.Vertices);

            Centroid.Position[0] = hull.Centroid[0];
            Centroid.Position[1] = hull.Centroid[1];
            Centroid.Position[2] = hull.Centroid[2];

            count = hull.Simplexs.Count;

            for (int i = 0; i < count; i++)
            {
                Simplex<VERTEX> simplex = hull.Simplexs[i];

                if (simplex.Normal[Dimension] >= 0.0f)
                {
                    for (int j = 0; j < simplex.Adjacent.Length; j++)
                    {
                        if (simplex.Adjacent[j] != null)
                        {
                            simplex.Adjacent[j].Remove(simplex);
                        }
                    }
                }
                else
                {
                    DelaunayCell<VERTEX> cell = CreateCell(simplex);
                    cell.CircumCenter.Id = i;
                    Cells.Add(cell);
                }

            }

        }

        public override void Generate(IList<VERTEX> input, bool assignIds = true, bool checkInput = false, float thresholdSqrDistance=50f)
        {
            Clear();
            if (input.Count <= Dimension + 1) return;

            int count = input.Count;
            for (int i = 0; i < count; i++)
            {
                float lenSq = input[i].SqrMagnitude;

                float[] v = input[i].Position;
                Array.Resize(ref v, Dimension + 1);
                input[i].Position = v;

                input[i].Position[Dimension] = (float)lenSq;
            }

            var hull = new ConvexHull<VERTEX>(Dimension + 1);
            hull.Generate(input, assignIds, checkInput);

            for (int i = 0; i < count; i++)
            {
                float[] v = input[i].Position;
                Array.Resize(ref v, Dimension);
                input[i].Position = v;
            }

            Vertices = new List<VERTEX>(hull.Vertices);

            Centroid.Position[0] = hull.Centroid[0];
            Centroid.Position[1] = hull.Centroid[1];
            Centroid.Position[2] = hull.Centroid[2];

            count = hull.Simplexs.Count;

            for (int i = 0; i < count; i++)
            {

                Simplex<VERTEX> simplex = hull.Simplexs[i];

                VERTEX[] verts = simplex.Vertices;
                float dist1 = verts[0].SqrDistance(verts[1]);
                float dist2 = verts[0].SqrDistance(verts[2]);
                float dist3 = verts[0].SqrDistance(verts[3]);
                
                if (simplex.Normal[Dimension] >= 0.0f)
                {
                    for (int j = 0; j < simplex.Adjacent.Length; j++)
                    {
                        if (simplex.Adjacent[j] != null)
                        {
                            simplex.Adjacent[j].Remove(simplex);
                        }
                    }
                }
                else if (dist1 > thresholdSqrDistance || dist2 > thresholdSqrDistance || dist3 > thresholdSqrDistance)
                {
                    for (int j = 0; j < simplex.Adjacent.Length; j++)
                    {
                        if (simplex.Adjacent[j] != null)
                        {
                            simplex.Adjacent[j].Remove(simplex);
                        }
                    }
                    //Debug.Log("Hello");
                }
                else
                {
                    DelaunayCell<VERTEX> cell = CreateCell(simplex);
                    cell.CircumCenter.Id = i;
                    Cells.Add(cell);
                }
            }

        }

        private float MINOR(int r0, int r1, int r2, int c0, int c1, int c2)
        {
            return m_matrixBuffer[r0, c0] * (m_matrixBuffer[r1, c1] * m_matrixBuffer[r2, c2] - m_matrixBuffer[r2, c1] * m_matrixBuffer[r1, c2]) -
                    m_matrixBuffer[r0, c1] * (m_matrixBuffer[r1, c0] * m_matrixBuffer[r2, c2] - m_matrixBuffer[r2, c0] * m_matrixBuffer[r1, c2]) +
                    m_matrixBuffer[r0, c2] * (m_matrixBuffer[r1, c0] * m_matrixBuffer[r2, c1] - m_matrixBuffer[r2, c0] * m_matrixBuffer[r1, c1]);
        }

        private float Determinant()
        {
            return (m_matrixBuffer[0, 0] * MINOR(1, 2, 3, 1, 2, 3) -
                    m_matrixBuffer[0, 1] * MINOR(1, 2, 3, 0, 2, 3) +
                    m_matrixBuffer[0, 2] * MINOR(1, 2, 3, 0, 1, 3) -
                    m_matrixBuffer[0, 3] * MINOR(1, 2, 3, 0, 1, 2));
        }

        private DelaunayCell<VERTEX> CreateCell2(Simplex<VERTEX> simplex)
        {
            // From MathWorld: http://mathworld.wolfram.com/Circumsphere.html

            VERTEX[] verts = simplex.Vertices;

            // x, y, z, 1
            for (int i = 0; i < 4; i++)
            {
                //Debug.Log(verts[i].Position[0] + ", " + verts[i].Position[1] + ", " + verts[i].Position[2]);
                m_matrixBuffer[i, 0] = verts[i].Position[0];
                m_matrixBuffer[i, 1] = verts[i].Position[1];
                m_matrixBuffer[i, 2] = verts[i].Position[2];
                m_matrixBuffer[i, 3] = 1;
            }
            float a = Determinant();

            // size, y, z, 1
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 0] = verts[i].SqrMagnitude;
            }
            float dx = Determinant();

            // size, x, z, 1
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 1] = verts[i].Position[0];
            }
            float dy = -Determinant();

            // size, x, y, 1
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 2] = verts[i].Position[1];
            }
            float dz = Determinant();

            //size, x, y, z
            for (int i = 0; i < 4; i++)
            {
                m_matrixBuffer[i, 3] = verts[i].Position[2];
            }
            float c = Determinant();

            float s = -1.0f / (2.0f * a);
            float radius = Math.Abs(s) * (float)Math.Sqrt(dx * dx + dy * dy + dz * dz - 4 * a * c);

            float[] circumCenter = new float[3];
            circumCenter[0] = s * dx;
            circumCenter[1] = s * dy;
            circumCenter[2] = s * dz;

            return new DelaunayCell<VERTEX>(simplex, circumCenter, radius);
        }

        private DelaunayCell<VERTEX> CreateCell(Simplex<VERTEX> simplex)
        {

            // https://en.wikipedia.org/wiki/Tetrahedron

            VERTEX[] verts = simplex.Vertices;
            Vector3 vecA = new Vector3(verts[0].Position[0], verts[0].Position[1], verts[0].Position[2]);
            Vector3 vecB = new Vector3(verts[1].Position[0], verts[1].Position[1], verts[1].Position[2]);
            Vector3 vecC = new Vector3(verts[2].Position[0], verts[2].Position[1], verts[2].Position[2]);
            Vector3 vecD = new Vector3(verts[3].Position[0], verts[3].Position[1], verts[3].Position[2]);

            //*
            float a = Vector3.Distance(vecD, vecA);
            float b = Vector3.Distance(vecD, vecB);
            float c = Vector3.Distance(vecD, vecC);

            float A = Vector3.Distance(vecB, vecC);
            float B = Vector3.Distance(vecA, vecC);
            float C = Vector3.Distance(vecB, vecA);

            //float[] circumCenter = new float[3];

            Vector3 x0x1 = vecA - vecD;
            Vector3 x0x2 = vecB - vecD;
            Vector3 x0x3 = vecC - vecD;

            Matrix4x4 U = new Matrix4x4();
            U[0, 0] = x0x1.x; U[0, 1] = x0x1.y; U[0, 2] = x0x1.z; U[0, 3] = 0;
            U[1, 0] = x0x2.x; U[1, 1] = x0x2.y; U[1, 2] = x0x2.z; U[1, 3] = 0;
            U[2, 0] = x0x3.x; U[2, 1] = x0x3.y; U[2, 2] = x0x3.z; U[2, 3] = 0;
            U[3, 0] = 0; U[3, 1] = 0; U[3, 2] = 0; U[3, 3] = 1;

            //Debug.Log(U);
            //Debug.Log(U * U.inverse );

            //U = U.transpose;
            Matrix4x4 Uinverse = U.inverse;

            float vecDSqrMagnitude = vecD.sqrMagnitude;
            Vector4 P = .5f * new Vector4(vecA.sqrMagnitude - vecDSqrMagnitude,
                                            vecB.sqrMagnitude - vecDSqrMagnitude,
                                            vecC.sqrMagnitude - vecDSqrMagnitude);

            Vector3 circumCenter = Uinverse.MultiplyPoint3x4(P);

            float V = Mathf.Abs(Vector3.Dot(vecA - vecD, Vector3.Cross(vecB - vecD, vecC - vecD))) / 6;
            float radius = Mathf.Sqrt((a * A + b * B + c * C) * (a * A + b * B - c * C) * (a * A - b * B + c * C) * (-a * A + b * B + c * C))/(24*V);
            //*/

            float maxDistance = 0;
            float rad = Vector3.Distance(vecA, circumCenter);
            float[] distances = new float[] { a, b, c, A, B, C };

            for(int j=0; j<distances.Length; j++)
            {
                if(distances[j] > maxDistance)
                {
                    maxDistance = distances[j];
                }
            }
            if (rad > maxDistance)
            {
                Vector3 centroid = (vecA + vecB + vecC + vecD) / 4;
                circumCenter = centroid;
            }

            //Debug.Log(vecA + "; " + vecB + "; " + vecC + "; " + vecD + "; " + circumCenter + "; " + radius + "; " + Vector3.Distance(vecA, circumCenter) + "; " + Vector3.Distance(vecB, circumCenter));

            //return new DelaunayCell<VERTEX>(simplex, new float[] { centroid.x, centroid.y, centroid.z }, Vector3.Distance(vecA, centroid));
            return new DelaunayCell<VERTEX>(simplex, new float[] { circumCenter.x, circumCenter.y, circumCenter.z }, radius);
        }
    }
}