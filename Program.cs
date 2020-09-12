using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ChekingSolution
{ 
    class Program
    {
        static void Main(string[] args)
        {
            // создаем доску 15 на 15
            char[,] board = new char[15, 15];
            // создаем лист который будет содержать все ходы которые были совершенны в игры
            List<string> allMoves = new List<string>();
            // создаем словарь который будет хранит горизонтальные координаты от 'а' до 'o' 
            Dictionary<char, int> horizontalCoordinates = new Dictionary<char, int>(15);
            char horizontalAlphabet = 'a';
            for (int i = 0; i < 15; i++)
            {
                horizontalCoordinates.Add(horizontalAlphabet, i);
                horizontalAlphabet++;
            }

            char currentPlayer = 'X';
            char previousPlayer = 'O';
            int moveCount = 0;

            Initialize(board);
            Console.WriteLine($"Select game mode:");
            Console.WriteLine("1. Player1 vs Player2");
            Console.WriteLine("2. Player vs Computer");
            Console.WriteLine("3. Computer vs Computer");
            Console.Write("Enter the mode: ");
            int mode = int.Parse(Console.ReadLine());

            if (mode == 1)
                PlayerVsPlayer(board, currentPlayer, previousPlayer, moveCount, 
                                    horizontalCoordinates, allMoves);
            else if (mode == 2)
                PlayerVsComputer(board, currentPlayer, previousPlayer, moveCount,
                                    horizontalCoordinates, allMoves);
            else if (mode == 3)
                ComputerVsComputer(board, currentPlayer, previousPlayer, moveCount, 
                                    horizontalCoordinates, allMoves);
            else
                throw new Exception("Wrong input");
        }

        static void PlayerVsPlayer(char[,] board, char currentPlayer, char previousPlayer, int moveCount, 
                                   Dictionary<char, int> horizontalCoordinates, List<string> allMoves)
        {
            string winner = "";
            while (true)
            {
                PlayerMove(board, currentPlayer, horizontalCoordinates, allMoves);

                winner = GetWinner(board, currentPlayer, moveCount, winner);
                if (winner != "")
                    break;

                previousPlayer = currentPlayer;
                currentPlayer = ChangeTurn(currentPlayer);

                moveCount++;
            }

            PrintTheResult(board, winner, horizontalCoordinates, allMoves);
        }

        static void PlayerVsComputer(char[,] board, char currentPlayer, char previousPlayer, int moveCount, 
                                     Dictionary<char, int> horizontalCoordinates, List<string> allMoves)
        {
            string winner = "";
            while (true)
            {
                PlayerMove(board, currentPlayer, horizontalCoordinates, allMoves);

                winner = GetWinner(board, currentPlayer, moveCount, winner);
                if (winner != "")
                    break;

                previousPlayer = currentPlayer;
                currentPlayer = ChangeTurn(currentPlayer);

                ComputerMove(board, currentPlayer, previousPlayer, horizontalCoordinates, allMoves);
                winner = GetWinner(board, currentPlayer, moveCount, winner);
                if (winner != "")
                    break;

                currentPlayer = ChangeTurn(currentPlayer);

                moveCount++;
            }

            PrintTheResult(board, winner, horizontalCoordinates, allMoves);
        }

        static void ComputerVsComputer(char[,] board, char currentPlayer, char previousPlayer, int moveCount, 
                                       Dictionary<char, int> horizontalCoordinates, List<string> allMoves)
        {
            string winner = "";
            while (true)
            {
                ComputerMove(board, currentPlayer, previousPlayer, horizontalCoordinates, allMoves);

                winner = GetWinner(board, currentPlayer, moveCount, winner);
                if (winner != "")
                    break;

                previousPlayer = currentPlayer;
                currentPlayer = ChangeTurn(currentPlayer);

                moveCount++;
            }

            PrintTheResult(board, winner, horizontalCoordinates, allMoves);
        }

        static void PlayerMove(char[,] board, char currentPlayer, 
                               Dictionary<char, int> horizontalCoordinates, List<string> allMoves)
        {
            Console.Clear();
            PrintTheBoard(board, horizontalCoordinates);

            Console.Write("Enter column: ");
            char temp_col = char.Parse(Console.ReadLine());
            int col = horizontalCoordinates[temp_col];
            Console.Write("Enter row: ");
            int row = int.Parse(Console.ReadLine());


            // если выбранное поле пустое, ставим текущий знак, если нет заново вводим координаты 
            if (board[row, col] == '_')
            {
                board[row, col] = currentPlayer;
                string temp_string = "Row: " + row + " " + "Column: " + col;
                allMoves.Add(temp_string);
            }
            else
                PlayerMove(board, currentPlayer, horizontalCoordinates, allMoves);
        }

        static void ComputerMove(char[,] board, char currentPlayer, char previousPlayer,
                                 Dictionary<char, int> horizontalCoordinates, List<string> allMoves)
        {
            Console.Clear();
            PrintTheBoard(board, horizontalCoordinates);
            int bestScore = int.MinValue;
            int indexRow = 0;
            int indexCol = 0;
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (board[row, col] == '_')
                    {
                        board[row, col] = currentPlayer;
                        int score = MyAlgorithm(board, currentPlayer, previousPlayer,
                                        row, col);
                        board[row, col] = '_';
                        if (score > bestScore)
                        {
                            bestScore = score;
                            indexRow = row;
                            indexCol = col;
                        }
                    }
                }
            }
            board[indexRow, indexCol] = currentPlayer;
            string temp_string = "Row: " + indexRow + " " + "Column: " + indexCol;
            allMoves.Add(temp_string);
        }
    
        static int MyAlgorithm(char[,] board, char currentPlayer, char previousPlayer,
                                int indexRow, int indexCol)
        {
            int bestScoreC = int.MinValue;
            int bestScoreO = int.MinValue;

            int bestScoreCH = int.MinValue;
            int bestScoreOH = int.MinValue;
            int bestScoreCV = int.MinValue;
            int bestScoreOV = int.MinValue;
            int bestScoreCD1 = int.MinValue;
            int bestScoreOD1 = int.MinValue;
            int bestScoreCD2 = int.MinValue;
            int bestScoreOD2 = int.MinValue;


            int countHC = 0;
            int countHO = 0;
            int countVC = 0;
            int countVO = 0;
            int countDC1 = 0;
            int countDC2 = 0;
            int countDO1 = 0;
            int countDO2 = 0;

            // finding winner in horizontal lines
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (board[row, col] == currentPlayer)
                        countHC++;
                    else
                        countHC = 0;
                    //
                    
                    if (bestScoreCH < countHC)
                    {
                        bestScoreCH = countHC;
                    }
                }
                countHC = 0;
            }

            // finding winner in vertical lines
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (board[col, row] == currentPlayer)
                        countVC++;
                    else
                        countVC = 0;
                    //
                    if (bestScoreCV < countVC)
                    {
                        bestScoreCV = countVC;
                    }
                        
                }
                if (bestScoreCV >= 5)
                {
                    bestScoreCV = 100;
                }
                countVC = 0;
            }

            // finding winner in left-up to right-down diagonals
            for (int k = 0; k < 29; k++) // 29
            {
                for (int col = 0; col < 15; col++)
                {
                    int row = k - col;
                    if ((row < 15 && col < 15) && row >= 0 && col >= 0) // 15 15
                    {
                        if (board[row, col] == currentPlayer)
                            countDC1++;
                        else
                            countDC1 = 0;
                        //
                        if (bestScoreCD1 < countDC1)
                        {
                            bestScoreCD1 = countDC1;
                        }
                    }
                }
                if (bestScoreCD1 >= 5)
                {
                    bestScoreCD1 = 100;
                }
                countDC1 = 0;
            }

            // finding winner in right-up to left-down diagonals
            int len = 14; // 14
            for (int k = 0; k < 29; k++) // 29
            {
                int temp_row = 0;
                for (int col = len; col <= 14; col++) // 14
                {
                    if ((temp_row < 15 && col < 15) && temp_row >= 0 && col >= 0)
                    { // 15 15 
                        if (board[temp_row, col] == currentPlayer)
                        {
                            countDC2++;
                        }
                        else
                            countDC2 = 0;
                        //
                        if (bestScoreCD2 < countDC2)
                        {
                            bestScoreCD2 = countDC2;
                        }
                    }
                    temp_row++;
                }
                if (bestScoreCD2 >= 5)
                {
                    bestScoreCD2 = 100;
                }
                len--;
                countDC2 = 0;
            }


            board[indexRow, indexCol] = previousPlayer;

            // finding winner in horizontal lines
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (board[row, col] == previousPlayer)
                        countHO++;
                    else
                        countHO = 0;
                    //
                    if (bestScoreOH < countHO)
                    {
                        bestScoreOH = countHO;
                    }
                }
                if (bestScoreOH >= 5)
                {
                    bestScoreOH = 80;
                }
                countHO = 0;
            }

            // finding winner in vertical lines
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (board[col, row] == previousPlayer)
                        countVO++;
                    else
                        countVO = 0;
                    //
                    if (bestScoreOV < countVO)
                    {
                        bestScoreOV = countVO;
                    }
                }
                if (bestScoreOV >= 5)
                {
                    bestScoreOV = 80;
                }
                countVO = 0;
            }

            // finding winner in left-up to right-down diagonals
            for (int k = 0; k < 29; k++) // 29
            {
                for (int col = 0; col < 15; col++)
                {
                    int row = k - col;
                    if ((row < 15 && col < 15) && row >= 0 && col >= 0) // 15 15
                    {
                        if (board[row, col] == previousPlayer)
                            countDO1++;
                        else
                            countDO1 = 0;
                        //
                        if (bestScoreOD1 < countDO1)
                        {
                            bestScoreOD1 = countDO1;
                        }
                    }
                }
                if (bestScoreOD1 >= 5)
                {
                    bestScoreOD1 = 80;
                }
                countDO1 = 0;
            }

            // finding winner in right-up to left-down diagonals
            len = 14; // 14
            for (int k = 0; k < 29; k++) // 29
            {
                int temp_row = 0;
                for (int col = len; col <= 14; col++) // 14
                {
                    if ((temp_row < 15 && col < 15) && temp_row >= 0 && col >= 0)
                    { // 15 15 
                        if (board[temp_row, col] == previousPlayer)
                        {
                            countDO2++;
                        }
                        else
                            countDO2 = 0;
                        //
                        if (bestScoreOD2 < countDO2)
                        {
                            bestScoreOD2 = countDO2;
                        }
                    }
                    temp_row++;
                }
                if (bestScoreOD2 >= 5)
                {
                    bestScoreOD2 = 80;
                }
                len--;
                countDO2 = 0;
            }

            List<int> sortScoresC = new List<int>();
            List<int> sortScoresO = new List<int>();

            sortScoresC.Add(bestScoreCH);
            sortScoresC.Add(bestScoreCV);
            sortScoresC.Add(bestScoreCD1);
            sortScoresC.Add(bestScoreCD2);
            sortScoresC.Sort();

            sortScoresO.Add(bestScoreOH);
            sortScoresO.Add(bestScoreOV);
            sortScoresO.Add(bestScoreOD1);
            sortScoresO.Add(bestScoreOD2);
            sortScoresO.Sort();

            return (sortScoresC[3] * 10 + sortScoresO[3] * 9) +
                   (sortScoresC[2] * 3 + sortScoresO[2] * 2) +
                   (sortScoresC[1] + sortScoresO[1]) +
                   (sortScoresC[0] + sortScoresO[0]);
        }

        // Initialize board
        static void Initialize(char[,] board)
        {
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (row == 7 && col == 7)
                        board[row, col] = 'O';
                    else
                        board[row, col] = '_';
                }
            }
        }

        // Printing board
        static void PrintTheBoard(char[,] board, Dictionary<char, int> horizontalCoordinates)
        {
            int cnt = 0;
            Console.Write("   ");
            foreach(var elements in horizontalCoordinates)
            {
                Console.Write(elements.Key);
                Console.Write("  ");
            }
            Console.WriteLine();

            for (int row = 0; row < 15; row++)
            {
                if (cnt <= 9)
                    Console.Write("0" + cnt + " ");
                else
                    Console.Write(cnt + " ");
                for (int col = 0; col < 15; col++)
                {
                    Console.Write(board[row, col]);
                    Console.Write("  ");
                }
                cnt++;
                Console.WriteLine();
            }
        }


        static string GetWinner(char[,] board, char currentPlayer, int moveCount, string winner)
        {
            if (moveCount == 255)
            {
                return "Draw";
            }
            int horizontalCountWinner = 0;
            int verticalCountWinner = 0;
            int diagonalCountWinner_1 = 0; // left-up to right-down
            int diagonalCountWinner_2 = 0; // right-up to left-down

            // finding winner in horizontal lines
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (board[row, col] == currentPlayer)
                        horizontalCountWinner++;
                        
                    else
                        horizontalCountWinner = 0;
                    //
                    if (horizontalCountWinner >= 5)
                    {
                        return "Winner is " + currentPlayer;
                    }
                }
                horizontalCountWinner = 0;
            }

            // finding winner in vertical lines
            for (int row = 0; row < 15; row++)
            {
                for (int col = 0; col < 15; col++)
                {
                    if (board[col, row] == currentPlayer)
                        verticalCountWinner++;
                    else
                        verticalCountWinner = 0;
                    //
                    if (verticalCountWinner >= 5)
                    {
                        return winner = "Winner is " + currentPlayer;
                    }
                }
                verticalCountWinner = 0;
            }

            // finding winner in left-up to right-down diagonals
            for (int k = 0; k < 29; k++) // 29
            {
                for (int col = 0; col < 15; col++)
                {
                    int row = k - col;
                    if ((row < 15 && col < 15) && row >= 0 && col >= 0) // 15 15
                    {
                        if (board[row, col] == currentPlayer)
                            diagonalCountWinner_1++;
                        else
                            diagonalCountWinner_1 = 0;
                        //
                        if (diagonalCountWinner_1 >= 5)
                        {
                            return winner = "Winner is " + currentPlayer;
                        }
                    }
                }
                diagonalCountWinner_1 = 0;
            }

            // finding winner in right-up to left-down diagonals
            int len = 14; // 14
            for (int k = 0; k < 29; k++) // 29
            {
                int temp_row = 0;
                for (int col = len; col <= 14; col++) // 14
                {
                    if ((temp_row < 15 && col < 15) && temp_row >= 0 && col >= 0)
                    { // 15 15 
                        if (board[temp_row, col] == currentPlayer)
                        {
                            diagonalCountWinner_2++;
                        }
                        else
                            diagonalCountWinner_2 = 0;
                        //
                        if (diagonalCountWinner_2 >= 5)
                        {
                            return winner = "Winner is " + currentPlayer;
                        }
                    }
                    temp_row++;
                }
                len--;
                diagonalCountWinner_2 = 0;
            }

            return "";
        }

        static void PrintTheResult(char[,] board, string winner, 
                                   Dictionary<char, int> horizontalCoordinates, List<string> allMoves)
        {
            Console.Clear();
            PrintTheBoard(board, horizontalCoordinates);
            Console.WriteLine();
            Console.WriteLine(winner);
            Console.WriteLine("All moves:");
            int cnt = 1;
            foreach (string elements in allMoves)
            {
                Console.Write(cnt + ". ");
                Console.Write(elements);
                cnt++;
                Console.WriteLine();
            }
        }

        static char ChangeTurn(char currentPlayer)
        {
            if (currentPlayer == 'X')
                return 'O';
            else
                return 'X';
        }

        
    }
}
