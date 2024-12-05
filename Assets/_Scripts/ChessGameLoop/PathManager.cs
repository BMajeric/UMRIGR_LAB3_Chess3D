using UnityEngine;

namespace ChessMainLoop
{
    /// <summary>
    /// Contains methods for calculating viable positions piece can move to, and placing path fields on them with appropriate color
    /// </summary>
    public static class PathManager
    {
        #region Lookup tables for movement directions
        private static readonly int[,] DiagonalLookup =
        {
           { 1, 1 },
           { 1, -1 },
           { -1, 1 },
           { -1, -1 }
        };

        private static readonly int[,] VerticalLookup =
        {
           { 1, 0 },
           { -1, 0 },
           { 0, 1 },
           { 0, -1 }
        };
        #endregion

        public static void CreateDiagonalPath(Piece caller)
        {
            CreatePathOnDirection(caller, DiagonalLookup);
        }

        public static void CreateVerticalPath(Piece caller)
        {
            CreatePathOnDirection(caller, VerticalLookup);
        }

        /// <summary>
        /// Checks for available spots for directions specified in lookup table and sets path field on them. Stops at first enemy or unavailable field in each direction.
        /// </summary>
        private static void CreatePathOnDirection(Piece caller, int[,] lookupTable)
        {
            /*
             * Potrebno je nadopuniti metodu logikom za pomicanje figure u danom smijeru definiranom parametrom lookupTable.
             *      - iteriraj kroz lookup table
             *      - za svaki smjer pozivaj CreatePathInSpotDirection dok se moze
             */
            //Debug.Log(lookupTable.Length);
            int startRow;
            int startColumn;
            int newRow;
            int newColumn;
            bool isPathAvailable;
            for (int i = 0; i < lookupTable.Length / 2; i++)
            {
                startRow = caller.Location.Row;
                startColumn = caller.Location.Column;
                //Debug.Log(lookupTable[i, 0].ToString() + lookupTable[i, 1].ToString());
                do
                {
                    newRow = startRow + lookupTable[i, 0];
                    newColumn = startColumn + lookupTable[i, 1];
                    //Debug.Log("ROWS: " + startRow.ToString() + " " + startColumn.ToString() + " " + newRow.ToString() + " " + newColumn.ToString());

                    isPathAvailable = CreatePath(caller, startRow, startColumn, newRow, newColumn);

                    startRow = newRow;
                    startColumn = newColumn;
                } while (isPathAvailable);
            }
        }

        /// <summary>
        /// Checks if the field located at callers position translated by direction parameters is free to move. 
        /// </summary>
        public static void CreatePathInSpotDirection(Piece caller, int rowDirection, int columnDirection)
        {
            int startRow = caller.Location.Row;
            int startColumn = caller.Location.Column;

            int newRow = startRow + rowDirection;
            int newColumn = startColumn + columnDirection;
            CreatePath(caller, startRow, startColumn, newRow, newColumn);
        }

        private static bool CreatePath(Piece caller, int startRow, int startColumn, int newRow, int newColumn)
        {
            bool isEnemyInPath = false;
            if (!BoardState.Instance.IsInBorders(newRow, newColumn)) return false;
            SideColor checkSide = BoardState.Instance.SimulateCheckState(startRow, startColumn, newRow, newColumn);

            // Ako micanje ove figure rezultira sahom za mene, nemoj iscrtavati
            if (checkSide == caller.PieceColor || checkSide == SideColor.Both) return false;

            Piece piece = BoardState.Instance.GetField(newRow, newColumn);
            GameObject path;
            if (piece == null)
            {
                // Ako je slobodno polje, iscrtavaj zuto
                path = ObjectPool.Instance.GetHighlightPath(PathPieceType.PathYellow);
            }
            else if (piece.PieceColor != caller.PieceColor)
            {
                // Ako je na polju protivnicka figura, iscrtavaj crveno
                path = ObjectPool.Instance.GetHighlightPath(PathPieceType.PathRed);
                path.GetComponent<PathPiece>().AssignPiece(piece);
                isEnemyInPath = true;
            }
            else return false;

            path.GetComponent<PathPiece>().Location = (newRow, newColumn);

            Vector3 position = new Vector3();

            position.x = newRow * BoardState.Offset;
            position.z = newColumn * BoardState.Offset;
            position.y = path.transform.localPosition.y;

            path.transform.localPosition = position;
            return !isEnemyInPath;
        }

        public static void CreatePassantSpot(Piece target, int row, int column)
        {
            PathPiece path = ObjectPool.Instance.GetHighlightPath(PathPieceType.PathRed).GetComponent<PathPiece>();
            path.AssignPiece(target);
            path.Location = (row, column);

            Vector3 _position = new Vector3();
            _position.x = row * BoardState.Offset;
            _position.z = column * BoardState.Offset;
            _position.y = path.transform.localPosition.y;

            path.transform.localPosition = _position;
        }

        /// <summary>
        /// Checks if there is a piece that can be castled with at target location and if that castle action would result in check for turn player.
        /// </summary>
        public static void CreateCastleSpot(Piece caller, Piece target)
        {
            if (GameManager.Instance.CheckedSide == caller.PieceColor) return;

            int rowCaller = caller.Location.Row;
            int columnCaller = caller.Location.Column;
            int rowTarget = target.Location.Row;
            int columnTarget = target.Location.Column;

            //Check to see if there are any pieces between rook and king
            columnCaller += columnTarget > columnCaller ? 1 : -1;
            while (columnCaller != columnTarget)
            {
                if (BoardState.Instance.GetField(rowCaller, columnCaller) != null) return;
                columnCaller += columnTarget > columnCaller ? 1 : -1;
            }

            columnCaller = caller.Location.Column;
            int columnMedian = (int)Mathf.Ceil((columnCaller + columnTarget) / 2f);

            if (BoardState.Instance.SimulateCheckState(rowCaller, columnCaller, rowCaller, columnMedian) == caller.PieceColor)
            {
                return;
            }
            if (BoardState.Instance.SimulateCheckState(rowTarget, columnTarget, rowTarget, columnMedian) == caller.PieceColor)
            {
                return;
            }

            PathPiece path = ObjectPool.Instance.GetHighlightPath(PathPieceType.PathYellow).GetComponent<PathPiece>();
            path.Location = (rowTarget, columnTarget);

            Vector3 position = new Vector3();
            path.AssignCastle(target);
            position.x = rowTarget * BoardState.Offset;
            position.z = columnTarget * BoardState.Offset;
            position.y = path.transform.localPosition.y;

            path.transform.localPosition = position;
        }
    }
}
