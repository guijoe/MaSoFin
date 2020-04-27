/********************************** Credits *******************************/
// Code for matrix diagonalisation was adapted from Maarten Kronenburg paper
// "A Method for Fast Diagonalization of a 2x2 or 3x3 Real Symmetric Matrix"
// Original paper and C++ source code can be found
// https://arxiv.org/abs/1306.6291
/**************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix3x3
{
    // Constants
    const float pi = 3.14159265f;
    const int outprecision = 17;
    const int outwidth = 26;

    // dlambda_limit, below which two lambdas are relatively equal
    float dlambda_limit = 1.0E-3f;
    float iszero_limit = 1.0E-20f;

    // Some globals to record global statistics
    int n_all_lambdas_equal = 0, n_two_lambdas_equal = 0,
    n_all_lambdas_different = 0;

    private float[,] mat;
    public Vector3 eigenValues;
    public Vector3 eulerAngles;
    public Matrix3x3 R;
    public Matrix3x3 D;
    public Vector3 pA1;
    public Vector3 pA2;
    public Vector3 pA3;

    //constructor
    public Matrix3x3()
    {
        //create identity matrix
        int row, col;
        mat = new float[3, 3];
        for (row = 0; row < 3; row++)
        {
            for (col = 0; col < 3; col++)
            {
                if (row == col)
                    mat[row, col] = 0;
                else
                    mat[row, col] = 0;
            }
        }
    }

    public Matrix3x3(float v11, float v12, float v13,
                    float v21, float v22, float v23,
                        float v31, float v32, float v33)
    {
        mat = new float[3, 3];

        mat[0, 0] = v11; mat[0, 1] = v12; mat[0, 2] = v13;
        mat[1, 0] = v21; mat[1, 1] = v22; mat[1, 2] = v23;
        mat[2, 0] = v31; mat[2, 1] = v32; mat[2, 2] = v33;

        eigenValues = new Vector3();
        eulerAngles = new Vector3();
        R = new Matrix3x3();
        D = new Matrix3x3();
    }

    public Matrix3x3(Vector3 e1, Vector3 e2, Vector3 e3)
    {
        mat = new float[3, 3];

        mat[0, 0] = e1.x; mat[0, 1] = e2.x; mat[0, 2] = e3.x;
        mat[1, 0] = e1.y; mat[1, 1] = e2.y; mat[1, 2] = e3.y;
        mat[2, 0] = e1.z; mat[2, 1] = e2.z; mat[2, 2] = e3.z;
    }

    public Matrix3x3(Vector3 e1, Vector3 e2, Vector3 e3, int i1, int i2, int i3)
    {
        mat = new float[3, 3];

        mat[0, i1] = e1.x; mat[0, i2] = e2.x; mat[0, i3] = e3.x;
        mat[1, i1] = e1.y; mat[1, i2] = e2.y; mat[1, i3] = e3.y;
        mat[2, i1] = e1.z; mat[2, i2] = e2.z; mat[2, i3] = e3.z;
    }

    public Matrix3x3(Vector3 u, float theta)
    {
        mat = new float[3, 3];

        float cos = Mathf.Cos(theta);
        float sin = Mathf.Sin(theta);

        mat[0, 0] = cos + u.x * u.x * (1 - cos);
        mat[0, 1] = -u.z * sin + u.x * u.y * (1 - cos);
        mat[0, 2] = u.y * sin + u.x * u.z * (1 - cos);
        mat[1, 0] = u.z * sin + u.x * u.y * (1 - cos);
        mat[1, 1] = cos + u.y * u.y * (1 - cos);
        mat[1, 2] = -u.x * sin + u.y * u.z * (1 - cos);
        mat[2, 0] = -u.y * sin + u.x * u.z * (1 - cos);
        mat[2, 1] = u.x * sin + u.y * u.z * (1 - cos);
        mat[2, 2] = cos + u.z * u.z * (1 - cos);
    }

    public void SetZero()
    {
        int row, col;
        for (row = 0; row < 3; row++)
        {
            for (col = 0; col < 3; col++)
            {
                if (row == col)
                    mat[row, col] = 0;
                else
                    mat[row, col] = 0;
            }
        }
    }

    public object Clone()
    {
        Matrix3x3 m = new Matrix3x3();
        int row, col;
        for (row = 0; row < 3; row++)
        {
            for (col = 0; col < 3; col++)
            {
                m[row, col] = mat[row, col];
            }
        }
        return (object)m;
    }

    public override bool Equals(object b)
    {
        Matrix3x3 m = (Matrix3x3)b;
        int row, col;
        for (row = 0; row < 3; row++)
        {
            for (col = 0; col < 3; col++)
            {
                if (m[row, col] != mat[row, col]) return false;
            }
        }
        return true;
    }

    //index operator, to retrieve and set values 
    //  in the matrix
    public float this[int row, int col]
    {
        get { return mat[row, col]; }
        set { mat[row, col] = value; }
    }

    //arithmetic operators
    public static Matrix3x3 operator +(Matrix3x3 a, Matrix3x3 b)
    {
        int row, col;
        Matrix3x3 r = new Matrix3x3();
        for (row = 0; row < 3; row++)
        {
            for (col = 0; col < 3; col++)
            {
                r[row, col] = a[row, col] + b[row, col];
            }
        }
        return r;
    }

    public static Matrix3x3 operator -(Matrix3x3 a, Matrix3x3 b)
    {
        int row, col;
        Matrix3x3 r = new Matrix3x3();
        for (row = 0; row < 3; row++)
        {
            for (col = 0; col < 3; col++)
            {
                r[row, col] = a[row, col] - b[row, col];
            }
        }
        return r;
    }

    public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
    {
        int row, col, i;
        Matrix3x3 r = new Matrix3x3();
        for (row = 0; row < 3; row++)
        {
            for (col = 0; col < 3; col++)
            {
                r[row, col] = 0;
                for (i = 0; i < 3; i++)
                {
                    r[row, col] += a[row, i] * b[i, col];
                }
            }
        }
        return r;
    }

    public static Vector3 operator *(Matrix3x3 a, Vector3 v)
    {
        Vector3 r = new Vector3();
        r.x = v.x * a[0, 0] + v.y * a[0, 1] + v.z * a[0, 2];// + v.w * a[0, 3];
        r.y = v.x * a[1, 0] + v.y * a[1, 1] + v.z * a[1, 2];// + v.w * a[1, 3];
        r.z = v.x * a[2, 0] + v.y * a[2, 1] + v.z * a[2, 2];// + v.w * a[2, 3];
        //r.w = v.x * a[3, 0] + v.y * a[3, 1] + v.z * a[3, 2] + v.w * a[3, 3];
        //r.w = 0;

        return r;
    }

    public Matrix3x3 Transpose()
    {
        int row, col;
        Matrix3x3 m = new Matrix3x3();
        for (row = 0; row < 3; row++)
        {
            for (col = 0; col < 3; col++)
            {
                m[row, col] = mat[col, row];
            }
        }
        return m;
    }

    public static Matrix3x3 Translate(Vector3 t)
    {
        Matrix3x3 m = new Matrix3x3();
        m[0, 3] = t.x;
        m[1, 3] = t.y;
        m[2, 3] = t.z;
        return m;
    }

    public static Matrix3x3 Scale(float s)
    {
        Matrix3x3 m = new Matrix3x3();
        m[0, 0] = s;
        m[1, 1] = s;
        m[2, 2] = s;
        return m;
    }

    public static Matrix3x3 Scale(float sx, float sy, float sz)
    {
        Matrix3x3 m = new Matrix3x3();
        m[0, 0] = sx;
        m[1, 1] = sy;
        m[2, 2] = sz;
        return m;
    }

    public static Matrix3x3 InvertDiagonalMatrix(Matrix3x3 diag)
    {
        Matrix3x3 diagInverse = new Matrix3x3();

        diagInverse.mat[0, 0] = 1 / diag[0, 0];
        diagInverse.mat[1, 1] = 1 / diag[1, 1];
        diagInverse.mat[2, 2] = 1 / diag[2, 2];

        return diagInverse;
    }

    //general rotation around the Vector3 axis
    public static Matrix3x3 Rotate(Vector3 axis, float angle)
    {
        Matrix3x3 r = new Matrix3x3();
        float c = Mathf.Cos(angle);
        float s = Mathf.Sin(angle);

        axis = axis / axis.magnitude;
        r[0, 0] = (1 - c) * axis.x * axis.x + c;
        r[1, 0] = (1 - c) * axis.x * axis.y + s * axis.z;
        r[2, 0] = (1 - c) * axis.x * axis.z - s * axis.y;
        r[3, 0] = 0;

        r[0, 1] = (1 - c) * axis.x * axis.y - s * axis.z;
        r[1, 1] = (1 - c) * axis.y * axis.y + c;
        r[2, 1] = (1 - c) * axis.y * axis.z + s * axis.x;
        r[3, 1] = 0;

        r[0, 2] = (1 - c) * axis.x * axis.z + s * axis.y;
        r[1, 2] = (1 - c) * axis.y * axis.z - s * axis.x;
        r[2, 2] = (1 - c) * axis.z * axis.z + c;
        r[3, 2] = 0;

        r[0, 3] = 0;
        r[1, 3] = 0;
        r[2, 3] = 0;
        r[3, 3] = 1;

        return r;
    }

    public static Matrix3x3 Projection(float n, float f,
        float t, float b,
        float l, float r)
    {
        Matrix3x3 m = new Matrix3x3();

        m[0, 0] = 2 * n / (r - l);
        m[1, 0] = 0;
        m[2, 0] = 0;
        m[3, 0] = 0;

        m[0, 1] = 0;
        m[1, 1] = 2 * n / (t - b);
        m[2, 1] = 0;
        m[3, 1] = 0;

        m[0, 2] = 0;
        m[1, 2] = 2 * n / (t - b);
        m[2, 2] = -(f + n) / (f - n);
        m[3, 2] = -1;

        m[0, 3] = (r + l) / (r - l);
        m[1, 3] = (t + b) / (t - b);
        m[2, 3] = -2 * f * n / (f - n);
        m[3, 3] = 0;

        return m;
    }

    //provide a method to display the matrix	
    public override string ToString()
    {
        int i;
        String s = "\n";
        for (i = 0; i < 3; i++)
        {
            //s += String.Format("[{0},{1},{2},{3}]\n",
            //    mat[i, 0], mat[i, 1], mat[i, 2], mat[i, 3]);

            s += String.Format("[{0},{1},{2}]\n",
                mat[i, 0], mat[i, 1], mat[i, 2]);
        }
        return s;
    }

    float sqr(float val)
    {
        return val * val;
    }

    float angle(float x, float y)
    {
        if (x == 0.0f)
            return (y == 0.0f ? 0.0f : 0.5f * pi * Mathf.Sign(y));
        return (x < 0.0f ? Mathf.Atan(y / x) + pi * Mathf.Sign(y)
                        : Mathf.Atan(y / x));
    }

    // The functions trunc_sqrt and trunc_acos prevent a domain error
    // because of rounding off errors:

    float trunc_sqrt(float x)
    {
        return (x <= 0.0f ? 0.0f : Mathf.Sqrt(x));
    }

    float trunc_acos(float x)
    {
        if (x >= 1.0f)
            return 0.0f;
        if (x <= -1.0f)
            return pi;
        return Mathf.Acos(x);
    }

    // Solve the lambdas from the matrix:
    void solve_lambdas()
    {
        float p, q, b, t, delta;
        b = mat[0, 0] + mat[1, 1] + mat[2, 2];
        t = sqr(mat[0, 1]) + sqr(mat[0, 2]) + sqr(mat[1, 2]);
        p = 0.5f * (sqr(mat[0, 0] - mat[1, 1]) + sqr(mat[0, 0] - mat[2, 2])
            + sqr(mat[1, 1] - mat[2, 2]));
        p += 3.0f * t;
        q = 18.0f * (mat[0, 0] * mat[1, 1] * mat[2, 2] + 3.0f * mat[0, 1] * mat[0, 2] * mat[1, 2]);
        q += 2.0f * (mat[0, 0] * sqr(mat[0, 0]) + mat[1, 1] * sqr(mat[1, 1]) +
                        mat[2, 2] * sqr(mat[2, 2]));

        q += 9.0f * b * t;
        q -= 3.0f * (mat[0, 0] + mat[1, 1]) * (mat[0, 0] + mat[2, 2]) *
                    (mat[1, 1] + mat[2, 2]);
        q -= 27.0f * (mat[0, 0] * sqr(mat[1, 2]) + mat[1, 1] * sqr(mat[0, 2]) +
                        mat[2, 2] * sqr(mat[0, 1]));
        if (p < iszero_limit)
            eigenValues.x = eigenValues.y = eigenValues.z = b / 3.0f;
        else
        {
            delta = trunc_acos(0.5f * q / Mathf.Sqrt(p * sqr(p)));
            p = 2.0f * Mathf.Sqrt(p);
            // Changing the order in result yields different angles but identical matrix
            eigenValues.x = (b + p * Mathf.Cos(delta / 3.0f)) / 3.0f;
            eigenValues.y = (b + p * Mathf.Cos((delta + 2.0f * pi) / 3.0f)) / 3.0f;
            eigenValues.z = (b + p * Mathf.Cos((delta - 2.0f * pi) / 3.0f)) / 3.0f;
        };
    }

    // Determine which type of solution is needed:
    //  0: all lambdas equal
    //  1: two lambdas equal
    //  2: all lambdas different

    int solve_type(float[] lambdas)
    {
        int i1 = 0, i2 = 0, isum = 0;
        float t, lambdasum = 0.0f;

        for (int i = 0; i < 3; i++)
            lambdasum += sqr(lambdas[i]);

        lambdasum = Mathf.Sqrt(lambdasum);
        for (int i = 0; i < 2; i++)
            for (int j = i + 1; j < 3; j++)
            {
                t = Mathf.Abs(lambdas[i] - lambdas[j]);
                if (lambdasum > iszero_limit)
                    t /= lambdasum;
                if (t < dlambda_limit)
                {
                    isum++;
                    i1 = i;
                    i2 = j;
                };
            };
        if (isum == 0)
            return 2;
        if (isum >= 2)
            return 0;
        t = 0.5f * (lambdas[i1] + lambdas[i2]);
        lambdas[2] = lambdas[3 - i1 - i2];
        lambdas[0] = lambdas[1] = t;

        eigenValues.x = lambdas[0];
        eigenValues.y = lambdas[1];
        eigenValues.z = lambdas[2];

        return 1;
    }

    void set_diagonal(Vector3 arg)
    {
        D[0, 0] = arg.x;
        D[1, 1] = arg.y;
        D[2, 2] = arg.z;
    }

    // Solve the angles from the matrix and the solved lambdas:
    //  solve_angles_0: all lambdas equal
    //  solve_angles_1: two lambdas equal
    //  solve_angles_2: all lambdas different
    public Matrix3x3 sym_eigen()//Vector3 lambdas, Vector3 angles)
    {
        solve_lambdas();
        switch (solve_type(new float[] { eigenValues.x, eigenValues.y, eigenValues.z }))
        {
            case 0:
                solve_angles_0(new float[] { eulerAngles.x, eulerAngles.y, eulerAngles.z }, new float[] { eigenValues.x, eigenValues.y, eigenValues.z });
                n_all_lambdas_equal++;
                break;
            case 1:
                solve_angles_1(new float[] { eulerAngles.x, eulerAngles.y, eulerAngles.z }, new float[] { eigenValues.x, eigenValues.y, eigenValues.z });
                n_two_lambdas_equal++;
                break;
            case 2:
                solve_angles_2(new float[] { eulerAngles.x, eulerAngles.y, eulerAngles.z }, new float[] { eigenValues.x, eigenValues.y, eigenValues.z });
                n_all_lambdas_different++;
                break;
        };

        set_diagonal(eigenValues);

        return this;
    }

    public void set_rotation(int axis, float phi)
    {
        if (axis == 0 || axis > 3)
            throw new Exception("set_rotation: axis number out of bounds.");

        SetZero();

        switch (axis)
        {
            case 1:
                mat[0, 0] = 1.0f;
                mat[1, 1] = mat[2, 2] = Mathf.Cos(phi);
                mat[1, 2] = mat[2, 1] = Mathf.Sin(phi);
                mat[1, 2] = -mat[1, 2];
                break;
            case 2:
                mat[1, 1] = 1.0f;
                mat[0, 0] = mat[2, 2] = Mathf.Cos(phi);
                mat[0, 2] = mat[2, 0] = Mathf.Sin(phi);
                mat[2, 0] = -mat[2, 0];
                break;
            case 3:
                mat[2, 2] = 1.0f;
                mat[0, 0] = mat[1, 1] = Mathf.Cos(phi);
                mat[0, 1] = mat[1, 0] = Mathf.Sin(phi);
                mat[0, 1] = -mat[0, 1];
                break;
        }
    }

    // Generate the symmetric matrix A from lambdas and angles:
    public void compute_matrix()//Vector3 lambdas, Vector3 angles )
    {
        Matrix3x3 rot = new Matrix3x3(), temp = new Matrix3x3();

        R.set_rotation(3, eulerAngles.z);

        rot.set_rotation(2, eulerAngles.y);
        rot *= R;
        R = (Matrix3x3)rot.Clone();

        rot.set_rotation(1, eulerAngles.x);
        rot *= R;
        R = (Matrix3x3)rot.Clone();
    }

    void solve_angles_0(float[] res, float[] lambdas)
    {
        res[0] = 0.0f;
        res[1] = 0.0f;
        res[2] = 0.0f;

        eulerAngles = new Vector3();
    }

    void solve_angles_1(float[] res, float[] lambdas)
    {
        float phi1a, phi1b, phi2, absdif, delta = 1.0E10f;
        float g12, g21, t1, t2;
        t1 = lambdas[0] - lambdas[2];
        t2 = mat[0, 0] - lambdas[2];
        phi2 = trunc_acos(trunc_sqrt(t2 / t1)); // + pi for symmetry
        g12 = 0.5f * t1 * Mathf.Sin(2.0f * phi2);
        g21 = t2;
        t1 = angle(mat[1, 1] - mat[2, 2], -2.0f * mat[1, 2]);
        t2 = Mathf.Sin(t1);
        t1 = Mathf.Cos(t1);
        phi1b = 0.5f * angle(g21 * t1, -g21 * t2);
        t1 = angle(mat[0, 1], -1.0f * mat[0, 2]);
        t2 = Mathf.Sin(t1);
        t1 = Mathf.Cos(t1);
        bool big = sqr(mat[1, 1] - mat[2, 2]) + sqr(2.0f * mat[1, 2])
                    > sqr(mat[0, 1]) + sqr(mat[0, 2]);
        for (int i = 0; i < 2; i++)
        {
            phi1a = angle(g12 * t2, g12 * t1);
            absdif = Mathf.Abs(phi1a - phi1b);
            if (absdif < delta)
            {
                delta = absdif;
                res[0] = big ? phi1b : phi1a;
                res[1] = phi2;
            };
            phi2 = -phi2;
            g12 = -g12;
        };
        res[2] = 0.0f;

        eulerAngles.x = res[0];
        eulerAngles.y = res[1];
        eulerAngles.z = res[2];
    }

    void solve_angles_2(float[] res, float[] lambdas)
    {
        float phi1a, phi1b, phi2, phi3, v, w, absdif, delta = 1.0E10f;
        float g11, g12, g21, g22, t1, t2, t3, t4;
        t1 = lambdas[0] - lambdas[1];
        t2 = lambdas[1] - lambdas[2];
        t3 = lambdas[2] - lambdas[0];
        t4 = mat[0, 0] - lambdas[2];
        v = sqr(mat[0, 1]) + sqr(mat[0, 2]);
        v += t4 * (mat[0, 0] + t3 - lambdas[1]);
        v /= t2 * t3;
        if (Mathf.Abs(v) < iszero_limit) w = 1.0f;
        else w = (t4 - t2 * v) / (t1 * v);
        phi2 = trunc_acos(trunc_sqrt(v)); // + pi for symmetry
        phi3 = trunc_acos(trunc_sqrt(w)); // + pi for symmetry
        g11 = 0.5f * t1 * Mathf.Cos(phi2) * Mathf.Sin(2.0f * phi3);
        g12 = 0.5f * (t1 * w + t2) * Mathf.Sin(2.0f * phi2);
        g21 = t1 * (1.0f + (v - 2.0f) * w) + t2 * v;
        g22 = t1 * Mathf.Sin(phi2) * Mathf.Sin(2.0f * phi3);
        t1 = angle(mat[0, 1], -1.0f * mat[0, 2]);
        t3 = angle(mat[1, 1] - mat[2, 2], -2.0f * mat[1, 2]);
        t2 = Mathf.Sin(t1);
        t1 = Mathf.Cos(t1);
        t4 = Mathf.Sin(t3);
        t3 = Mathf.Cos(t3);
        bool big = sqr(mat[1, 1] - mat[2, 2]) + sqr(2.0f * mat[1, 2])
                    > sqr(mat[0, 1]) + sqr(mat[0, 2]);
        for (int i = 0; i < 4; i++)
        {
            phi1a = angle(g11 * t1 + g12 * t2, -g11 * t2 + g12 * t1);
            phi1b = 0.5f * angle(g21 * t3 + g22 * t4, -g21 * t4 + g22 * t3);
            absdif = Mathf.Abs(phi1a - phi1b);
            if (absdif < delta)
            {
                delta = absdif;
                res[0] = big ? phi1b : phi1a;
                res[1] = phi2;
                res[2] = phi3;
            };
            phi3 = -phi3;
            g11 = -g11;
            g22 = -g22;
            if (i == 1)
            {
                phi2 = -phi2;
                g12 = -g12;
                g22 = -g22;
            }
        }

        eulerAngles.x = res[0];
        eulerAngles.y = res[1];
        eulerAngles.z = res[2];
    }

    public Vector3 ComputePrincipalAxis()
    {
        sym_eigen();
        compute_matrix();

        float maxEigen = Mathf.Abs(eigenValues.x);
        int maxEigenIndex = 0;
        if (Mathf.Abs(eigenValues.y) > maxEigen)
        {
            maxEigen = Mathf.Abs(eigenValues.y);
            maxEigenIndex = 1;
        }
        if (Mathf.Abs(eigenValues.z) > maxEigen)
        {
            maxEigen = Mathf.Abs(eigenValues.z);
            maxEigenIndex = 2;
        }

        Vector3 principalAxis = new Vector3(R[0, maxEigenIndex],
                            R[1, maxEigenIndex],
                            R[2, maxEigenIndex]);

        float axisLength = Mathf.Sqrt(maxEigen);

        return axisLength * principalAxis;
    }

    public Vector3[] ComputeMinAxis()
    {
        sym_eigen();
        compute_matrix();

        float minEigen = Mathf.Abs(eigenValues.x);
        float maxEigen = minEigen;
        int minEigenIndex = 0;
        int maxEigenIndex = 0;
        
        if (Mathf.Abs(eigenValues.y) < minEigen)
        {
            minEigen = Mathf.Abs(eigenValues.y);
            minEigenIndex = 1;
        }
        else if (Mathf.Abs(eigenValues.y) > maxEigen)
        {
            maxEigen = Mathf.Abs(eigenValues.y);
            maxEigenIndex = 1;
        }

        if (Mathf.Abs(eigenValues.z) < minEigen)
        {
            minEigen = Mathf.Abs(eigenValues.z);
            minEigenIndex = 2;
        }
        else if (Mathf.Abs(eigenValues.z) > maxEigen)
        {
            maxEigen = Mathf.Abs(eigenValues.z);
            maxEigenIndex = 2;
        }

        int otherEigenIndex = 3 - maxEigenIndex - minEigenIndex;

        Vector3 minAxis = new Vector3(R[0, minEigenIndex],
                            R[1, minEigenIndex],
                            R[2, minEigenIndex]);

        //Debug.Log(otherEigenIndex);
        Vector3 otherAxis = new Vector3(R[0, otherEigenIndex],
                            R[1, otherEigenIndex],
                            R[2, otherEigenIndex]);

        Vector3 maxAxis = new Vector3(R[0, maxEigenIndex],
                            R[1, maxEigenIndex],
                            R[2, maxEigenIndex]);

        //float axisLength = Mathf.Sqrt(minEigen);
        /*
        if(minAxis.z < 0){
            minAxis = -minAxis;
            otherAxis = -otherAxis;
            maxAxis = maxAxis;
        }
        */

        Vector3 secondAxis = new Vector3();
        Vector3 thirdAxis = new Vector3();
        if(minEigenIndex == 0){
            secondAxis = new Vector3(R[0, 1], R[1, 1], R[2, 1]);
            thirdAxis = new Vector3(R[0, 2], R[1, 2], R[2, 2]);
        }
        if(minEigenIndex == 1){
            secondAxis = new Vector3(R[0, 2], R[1, 2], R[2, 2]);
            thirdAxis = new Vector3(R[0, 0], R[1, 0], R[2, 0]);
        }
        if(minEigenIndex == 2){
            secondAxis = new Vector3(R[0, 0], R[1, 0], R[2, 0]);
            thirdAxis = new Vector3(R[0, 1], R[1, 1], R[2, 1]);
        }

        pA1 = new Vector3(R[0, 0], R[1, 0], R[2, 0]);
        pA2 = new Vector3(R[0, 1], R[1, 1], R[2, 1]);
        pA3 = new Vector3(R[0, 2], R[1, 2], R[2, 2]);

        //Debug.Log(pA1 + "; " + pA2 + "; " + pA3);
        //Debug.Log(Vector3.Cross(pA2,pA3) + "; " + Vector3.Cross(pA3,pA1) + "; " + Vector3.Cross(pA1, pA2));

        //return new Vector3[] { minAxis, otherAxis, maxAxis };
        return new Vector3[] { minAxis, secondAxis, thirdAxis, pA1, pA2, pA3 };
    }

    public Vector3[] ComputePrincipalAxis(Vector3[] pAs)
    {
        //Compute R
        sym_eigen();
        compute_matrix();

        // Identify axis of interest
        int maxDotIndex=0;
        List<int> indices = new List<int>(){0,1,2};
        Vector3 maxAxis = MostColinear(indices, pAs[0], out maxDotIndex);

        // Reorient axis of interest
        if(Vector3.Dot(maxAxis, pAs[0]) < 0)
            maxAxis = -maxAxis;

        //Compute and Keep 2nd and 3rd axis consistent with previous: 
        //Project old 2nd and 3rd axis onto current 2nd and 3rd axis plane
        //https://www.euclideanspace.com/maths/geometry/elements/plane/lineOnPlane/index.htm
        Vector3 B = maxAxis;
        Vector3 A1 = pAs[1];
        Vector3 A2 = pAs[2];

        Vector3 secondAxis = Vector3.Cross(B, Vector3.Cross(A1, B));
        secondAxis.Normalize();
        Vector3 thirdAxis = Vector3.Cross(B, secondAxis);

        if(maxDotIndex == 0){
            pA1 = maxAxis;
            pA2 = secondAxis;
            pA3 = thirdAxis;
        }
        if(maxDotIndex == 1){
            pA1 = thirdAxis;
            pA2 = maxAxis;
            pA3 = secondAxis;
        }
        if(maxDotIndex == 0){
            pA1 = secondAxis;
            pA2 = thirdAxis;
            pA3 = maxAxis;
        }
        
        R = new Matrix3x3(pA1, pA2, pA3);

        return new Vector3[]{maxAxis, secondAxis, thirdAxis, pA1, pA2, pA3};
    }

    public Vector3 ComputePrincipalAxis(Vector3 pA)
    {
        sym_eigen();
        compute_matrix();

        Vector3 e1 = new Vector3(R[0, 0], R[1, 0], R[2, 0]);
        Vector3 e2 = new Vector3(R[0, 1], R[1, 1], R[2, 1]);
        Vector3 e3 = new Vector3(R[0, 2], R[1, 2], R[2, 2]);

        float maxDot = Mathf.Abs(Vector3.Dot(e1,pA));
        int maxDotIndex = 0;

        float dot = Mathf.Abs(Vector3.Dot(e2, pA));
        if (dot > maxDot)
        {
            maxDot = dot;
            maxDotIndex = 1;
        }

        dot = Mathf.Abs(Vector3.Dot(e3, pA));
        if (dot > maxDot)
        {
            maxDot = dot;
            maxDotIndex = 2;
        }

        Vector3 principalAxis = new Vector3(R[0, maxDotIndex],
                            R[1, maxDotIndex],
                            R[2, maxDotIndex]);

        return principalAxis;
    }

    public Vector3 MostColinear(List<int> indices, Vector3 pA, out int maxDotIndex){
        //Debug.Log("Count: " + indices.Count);
        maxDotIndex = indices[0];
        if(indices.Count <= 1){
            return new Vector3(R[0, maxDotIndex],
                            R[1, maxDotIndex],
                            R[2, maxDotIndex]);
        }
        

        Vector3[] vecs = new Vector3[indices.Count];
        for(int i=0; i<indices.Count; i++){
            vecs[i] = new Vector3(R[0, indices[i]], R[1, indices[i]], R[2, indices[i]]);
        }

        float maxDot = Mathf.Abs(Vector3.Dot(vecs[0], pA));
        for(int i=1; i<indices.Count; i++){
            float dot = Mathf.Abs(Vector3.Dot(vecs[i], pA));
            if (dot > maxDot)
            {
                maxDot = dot;
                maxDotIndex = indices[i];
            }
        }

        //Debug.Log("maxDotIndex: " + maxDotIndex);
        return new Vector3(R[0, maxDotIndex],
                            R[1, maxDotIndex],
                            R[2, maxDotIndex]);
    }
};