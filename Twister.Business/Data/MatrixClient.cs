using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Twister.Business.Data
{
    public class MatrixClient
    {
        public MatrixClient()
        {
            Matrix<double> A = DenseMatrix.OfArray(new[,]
            {
                {Math.PI, 1, 1, 1},
                {1, 2, 3, 4},
                {4, 3, 2, 1}
            });

            A.Inverse().Multiply(A);
        }
    }
}