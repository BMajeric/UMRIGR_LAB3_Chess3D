using System.Collections.Generic;
using UnityEngine;

namespace ChessReplay
{
    public class BoardStateReplay : Singleton<BoardStateReplay>
    {
        [SerializeField] private int _boardSize;
        [SerializeField] private List<ReplayPiece> _blackPieces;
        [SerializeField] private List<ReplayPiece> _whitePieces;
        [Header("ReplayPiece prefabs")]
        [SerializeField] private ReplayPiece _whiteQueen;
        [SerializeField] private ReplayPiece _blackQueen;
        [SerializeField] private ReplayPiece _whiteBishop;
        [SerializeField] private ReplayPiece _blackBishop;
        [SerializeField] private ReplayPiece _whiteRook;
        [SerializeField] private ReplayPiece _blackRook;
        [SerializeField] private ReplayPiece _whiteKnight;
        [SerializeField] private ReplayPiece _blackKnight;
        private ReplayPiece[,] _gridState;
        private Dictionary<int, ReplayPiece> _killedDict;
        private Dictionary<int, ReplayPiece[]> _promotedOnesDict;

        public static float Offset = 1.5f;
        public int BoardSize { get => _boardSize; }
        

        /// <summary>
        /// Resets grid state and positions of all pieces.
        /// </summary>
        public void InitializeGrid()
        {
            _promotedOnesDict = new Dictionary<int, ReplayPiece[]>();
            _gridState = new ReplayPiece[_boardSize, _boardSize];
            _killedDict = new Dictionary<int, ReplayPiece>();

            for (int i = 0; i < _gridState.GetLength(0); i++)
            {
                for (int j = 0; j < _gridState.GetLength(1); j++)
                {
                    _gridState[i, j] = null;
                }
            }

            Vector3 position = new Vector3();

            for (int i = 0; i < _blackPieces.Count; i++)
            {
                ReplayPiece piece = _blackPieces[i];
                var location = piece.Location;
                _gridState[location.Row, location.Column] = piece;
                position.x = piece.Location.Row;
                position.z = piece.Location.Column;
                position *= Offset;
                position.y = piece.transform.localPosition.y;
                piece.transform.localPosition = position;
            }

            for (int i = 0; i < _whitePieces.Count; i++)
            {
                ReplayPiece piece = _whitePieces[i];
                var location = piece.Location;
                _gridState[location.Row, location.Column] = piece;
                position.x = piece.Location.Row;
                position.z = piece.Location.Column;
                position *= Offset;
                position.y = piece.transform.localPosition.y;
                piece.transform.localPosition = position;
            }
        }

        /// <summary>
        /// Moves piece located on first paramter to location of second parameter. Stores pieces killed or promoted by turn counter parameter.
        /// </summary>
        /// If end position vector is negative, based on the value chooses a piece to replace pawn with for promotion or kills en passanted piece
        public void MovePiece(Vector2 startPosition, Vector2 endPosition, int turnCount)
        {
            if (_gridState == null)
            {
                InitializeGrid();
            }

            int startX = (int)startPosition.x;
            int startY = (int)startPosition.y;
            int endX = (int)endPosition.x;
            int endY = (int)endPosition.y;

            //Parameter -1 signifies piece got passanted, and from -2 to -9 resembles index
            //of piece mesh to replace pawn with
            if (endPosition.x == -1)
            {

                _killedDict.Add(turnCount, _gridState[startX, startY]);
                _gridState[startX, startY].gameObject.SetActive(false);
                _gridState[startX, startY] = null;
                return;
            }
            if(startPosition.x == -1)
            {
                return;
            }
            else if (endPosition.x < -1 || startPosition.x < -1)
            {
                PromotePawn(startPosition, _gridState[startX, startY],
                    (ChessPieceType)endPosition.x, turnCount);
                return;
            }
            else
            {
                if (_gridState[endX, endY] != null)
                {
                    _killedDict.Add(turnCount, _gridState[endX, endY]);
                    _gridState[endX, endY].gameObject.SetActive(false);
                }

                _gridState[endX, endY] = _gridState[startX, startY];
                _gridState[startX, startY] = null;
                _gridState[endX, endY].transform.localPosition = new Vector3(endPosition.x * Offset,
                    _gridState[endX, endY].transform.localPosition.y, endPosition.y * Offset);
            }
        }

        public void PromotePawn(Vector3 endPosition, ReplayPiece pawn, ChessPieceType pieceType, int turnCount)
        {
            /*
             * Nadopuniti metodu logikom koja izvodi akciju promocije pijuna u odrabranu figuru definiranu parametrom pieceType.
             */
            ReplayPiece newPiece = null;

            switch (pieceType)
            {
                case ChessPieceType.BlackQueen: newPiece = Instantiate(_blackQueen); break;
                case ChessPieceType.WhiteQueen: newPiece = Instantiate(_whiteQueen); break;
                case ChessPieceType.BlackRook: newPiece = Instantiate(_blackRook); break;
                case ChessPieceType.WhiteRook: newPiece = Instantiate(_whiteRook); break;
                case ChessPieceType.BlackBishop: newPiece = Instantiate(_blackBishop); break;
                case ChessPieceType.WhiteBishop: newPiece = Instantiate(_whiteBishop); break;
                case ChessPieceType.BlackKnight: newPiece = Instantiate(_blackKnight); break;
                case ChessPieceType.WhiteKnight: newPiece = Instantiate(_whiteKnight); break;
            }

            if (newPiece != null)
            {
                int row = (int)endPosition.x;
                int col = (int)endPosition.z;

                newPiece.transform.localPosition = new Vector3(row * Offset, pawn.transform.localPosition.y, col * Offset);
                _gridState[row, col] = newPiece;
                pawn.gameObject.SetActive(false);

                _promotedOnesDict[turnCount] = new ReplayPiece[] { pawn, newPiece };
            }
        }

        /// <summary>
        /// Replays last move by playing it backwards.
        /// </summary>
        public void UndoMove(Vector2 startPosition, Vector2 endPosition, int turnCount)
        {
            /*
             * Nadopuniti metodu logikom za vraćanje poteza unazad. Također je potrebno vratiti figure koje su potencijalno bile
             * pojedene taj potez ili pijuna koji se pomovirao u novu figuru.
             */
            int startX = (int)startPosition.x;
            int startY = (int)startPosition.y;
            int endX = (int)endPosition.x;
            int endY = (int)endPosition.y;

            if (_promotedOnesDict.ContainsKey(turnCount))
            {
                ReplayPiece[] promotedPieces = _promotedOnesDict[turnCount];
                ReplayPiece pawn = promotedPieces[0];
                ReplayPiece promotedPiece = promotedPieces[1];

                _gridState[endX, endY] = pawn;
                _gridState[startX, startY] = null;

                pawn.gameObject.SetActive(true);
                promotedPiece.gameObject.SetActive(false);

                _promotedOnesDict.Remove(turnCount);
                return;
            }

            if (_killedDict.ContainsKey(turnCount))
            {
                ReplayPiece killedPiece = _killedDict[turnCount];
                _gridState[endX, endY] = killedPiece;
                killedPiece.gameObject.SetActive(true);

                _killedDict.Remove(turnCount);
            }
            else
            {
                _gridState[endX, endY] = null;
            }

            ReplayPiece movedPiece = _gridState[startX, startY];
            _gridState[startX, startY] = movedPiece;
            movedPiece.transform.localPosition = new Vector3(startX * Offset, movedPiece.transform.localPosition.y, startY * Offset);

        }
    }
}