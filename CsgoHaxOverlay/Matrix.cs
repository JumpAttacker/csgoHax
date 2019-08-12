using System;

namespace CsgoHaxOverlay
{
    public class Matrix
    {
        #region VARIABLES
        private readonly float[] data;
        private readonly int rows;
        private readonly int columns;

        #endregion

        #region CONSTRUCTOR
        public Matrix(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            data = new float[rows * columns];
        }
        #endregion

        #region METHODS
        public void Read(byte[] data)
        {
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                    this[y, x] = BitConverter.ToSingle(data, sizeof(float) * ((y * columns) + x));
        }
        #endregion

        #region OPERANDS
        public float this[int i]
        {
            get { return data[i]; }
            set { data[i] = value; }
        }
        public float this[int row, int column]
        {
            get { return data[row * columns + column]; }
            set { data[row * columns + column] = value; }
        }
        #endregion
    }
}
