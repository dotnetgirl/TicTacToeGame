using System;
namespace TicTacToeGame
{
    public class GameSituation
    {
        public Player[,] GameGrid { get; private set; }
        public Player CurrentlyPlayer { get; private set; }
        public int TurnsWhichIsPassed { get; private set; }
        public bool IsGameOver { get; private set; }

        public event Action<int, int> MoveMade;
        public event Action<ResultOfTheGame> GameEnded;
        public event Action GameRestarted;

        public GameSituation()
        {
            GameGrid = new Player[3, 3];
            CurrentlyPlayer = Player.X;
            TurnsWhichIsPassed = 0;
            IsGameOver = false;
        }

        private bool CanMakeMove(int row, int column)
        {
            return !IsGameOver && GameGrid[row, column] == Player.None;
        }

        private bool IsPlaceFully()
        {
            return TurnsWhichIsPassed == 9;
        }

        private void TakeTurnToPlayer()
        {
            if (CurrentlyPlayer == Player.X)
            {
                CurrentlyPlayer = Player.O;
            }
            else
            {
                CurrentlyPlayer = Player.X;
            }
        }

        private bool AreSquaresMarked((int, int)[] squares, Player player)
        {
            foreach((int row, int column) in squares)
            {
                if (GameGrid[row, column] != player)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsThisMoveIsWin(int rowNumber, int columnNumber, out WinInformation winInformation)
        {
            (int, int)[] row = new[] { (rowNumber, 0), (rowNumber, 1), (rowNumber, 2) };
            (int, int)[] column = new[] { (0, columnNumber), (1, columnNumber), (2, columnNumber) };
            (int, int)[] mainDiagonal = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] notMainDiagonal = new[] { (0, 2), (1, 1), (2, 0) };

            if (AreSquaresMarked(row, CurrentlyPlayer))
            {
                winInformation = new WinInformation { Type = WinTypeHowTheWinnerWon.Row, Number = rowNumber };
                return true;
            }
            if (AreSquaresMarked(column, CurrentlyPlayer))
            {
                winInformation = new WinInformation { Type = WinTypeHowTheWinnerWon.Column, Number = columnNumber };
                return true;
            }
            if (AreSquaresMarked(mainDiagonal, CurrentlyPlayer))
            {
                winInformation = new WinInformation { Type = WinTypeHowTheWinnerWon.MainDiagonal};
                return true;
            }
            if(AreSquaresMarked(notMainDiagonal, CurrentlyPlayer))
            {
                winInformation = new WinInformation { Type = WinTypeHowTheWinnerWon.NotMainDiagonal };
                return true;
            }
            winInformation = null;
            return false;
        }

        private bool IsGameEnded(int rowNumber, int calumnNumber, out ResultOfTheGame resultOfTheGame)
        {
            if (IsThisMoveIsWin(rowNumber, calumnNumber, out WinInformation winInformation))
            {
                resultOfTheGame = new ResultOfTheGame { Winner = CurrentlyPlayer, WinInformation = winInformation };
                return true;
            }
            if (IsPlaceFully())
            {
                resultOfTheGame = new ResultOfTheGame { Winner = Player.None };
                return true;
            }
            resultOfTheGame = null;
            return false;
        }

        public void MakeMove(int rowNumber, int columnNumber)
        {
            if (!CanMakeMove(rowNumber, columnNumber))
            {
                return;
            }
            GameGrid[rowNumber, columnNumber] = CurrentlyPlayer;
            TurnsWhichIsPassed++;

            if (IsGameEnded(rowNumber, columnNumber, out ResultOfTheGame resultOfTheGame))
            {
                IsGameOver = true;
                if (MoveMade is not null)
                {
                    MoveMade(rowNumber, columnNumber);
                }
                GameEnded?.Invoke(resultOfTheGame);
            }
            else
            {
                TakeTurnToPlayer();
                MoveMade?.Invoke(rowNumber, columnNumber);
            }
        }
        
        public void NewGameReset()
        {
            GameGrid = new Player[3,3];
            CurrentlyPlayer = Player.X;
            TurnsWhichIsPassed = 0;
            IsGameOver = false;
            GameRestarted?.Invoke();
        }
    }
}
