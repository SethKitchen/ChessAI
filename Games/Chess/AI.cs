// This is where you build your AI for the Chess game.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Joueur.cs.Games.Chess
{
    /// <summary>
    /// This is where you build your AI for the Chess game.
    /// </summary>
    class AI : BaseAI
    {
        #region Properties
#pragma warning disable 0169 // the never assigned warnings between here are incorrect. We set it for you via reflection. So these will remove it from the Error List.
#pragma warning disable 0649
        /// <summary>
        /// This is the Game object itself, it contains all the information about the current game
        /// </summary>
        public readonly Chess.Game Game;
        /// <summary>
        /// This is your AI's player. This AI class is not a player, but it should command this Player.
        /// </summary>
        public readonly Chess.Player Player;
#pragma warning restore 0169
#pragma warning restore 0649
        #endregion


        #region Methods
        /// <summary>
        /// This returns your AI's name to the game server. Just replace the string.
        /// </summary>
        /// <returns>string of you AI's name.</returns>
        public override string GetName()
        {
            return "SethRocksSocks"; // REPLACE THIS WITH YOUR TEAM NAME!
        }

        bool is64Bit;
        Process p;

        /// <summary>
        /// This is automatically called when the game first starts, once the Game object and all GameObjects have been initialized, but before any players do anything.
        /// </summary>
        /// <remarks>
        /// This is a good place to initialize any variables you add to your AI, or start tracking game objects.
        /// </remarks>
        public override void Start()
        {
            base.Start();
            Console.WriteLine("---LET\'S GO LEEEEEEEERROOOOOOOOOOY JENKINNNNNNNNNNS!!!!!---");
            if (IntPtr.Size == 4)
            {
                is64Bit = false;
                Console.WriteLine("Computer is " + (IsLinux ? "linux" : "windows") + " 32 bit with " + Environment.ProcessorCount + " processors.");
            }
            else if (IntPtr.Size == 8)
            {
                is64Bit = true;
                Console.WriteLine("Computer is " + (IsLinux ? "linux" : "windows") + " 64 bit with " + Environment.ProcessorCount + " processors.");
            }
            else
            {
                //DAS FUTURE
                is64Bit = false;
            }

            if (IsLinux)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();

                var psi = new ProcessStartInfo
                {
                    FileName = "Games/Chess/SethRocksEngine.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                Console.WriteLine("Starting process...");
                p = Process.Start(psi);
                if (p != null)
                {
                    Console.WriteLine("Process is not null setting threads...");
                    p.StandardInput.WriteLine("setoption name Threads value 1");
                }
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();

                var psi = new ProcessStartInfo
                {
                    FileName = @"Games\Chess\SethRocksEngine.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                Console.WriteLine("Starting process...");
                p = Process.Start(psi);
                if (p != null)
                {
                    Console.WriteLine("Process is not null setting threads...");
                    p.StandardInput.WriteLine("setoption name Threads value 1");
                }
            }
        }


        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        /// <summary>
        /// This is automatically called every time the game (or anything in it) updates.
        /// </summary>
        /// <remarks>
        /// If a function you call triggers an update this will be called before that function returns.
        /// </remarks>
        public override void GameUpdated()
        {
            base.GameUpdated();
        }

        /// <summary>
        /// This is automatically called when the game ends.
        /// </summary>
        /// <remarks>
        /// You can do any cleanup of you AI here, or do custom logging. After this function returns the application will close.
        /// </remarks>
        /// <param name="won">true if your player won, false otherwise</param>
        /// <param name="reason">a string explaining why you won or lost</param>
        public override void Ended(bool won, string reason)
        {
            base.Ended(won, reason);
            p.StandardInput.WriteLine("setoption name Threads value 0");
            Thread.Sleep(30);
            p.Kill();
            p.Dispose();
        }

        public bool doLoop()
        {
            string bestMove = "";
            Console.WriteLine("Setting position");
            p.StandardInput.WriteLine("position fen " + Game.Fen);
            Console.WriteLine("setting movetime");
            p.StandardInput.WriteLine("go movetime 15000 ");
            string output = "";
            Console.WriteLine("Looping until best move");
            IAsyncResult result;
            Action action = () =>
            {
                while (!output.Contains("bestmove"))
                {
                    output = p.StandardOutput.ReadLine();
                }
            };
            result = action.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(16000))
            {
                Console.WriteLine("got bestMove");
                bestMove = output.Substring(9, 4);
                Console.WriteLine("Best move is: " + bestMove);

                // 2) print the opponent's last move to the console
                if (this.Game.Moves.Count > 0)
                {
                    Console.WriteLine("Opponent's Last Move: '" + this.Game.Moves.Last().San + "'");
                }

                // 3) print how much time remaining this AI has to calculate moves
                Console.WriteLine("Time Remaining: " + this.Player.TimeRemaining + " ns");

                // 4) make a random (and probably invalid) move.
                Piece toMove = Player.Pieces[0];
                foreach (Piece p in Player.Pieces)
                {
                    if (p.File == bestMove[0] + "" && p.Rank == int.Parse(bestMove[1] + ""))
                    {
                        toMove = p;
                    }
                }
                string fileToMoveTo = bestMove[2] + "";
                int rankToMoveTo = int.Parse(bestMove[3] + "");
                toMove.Move(fileToMoveTo, rankToMoveTo, "Queen");
                return true;
            }
            else
            {
                Console.WriteLine("Process timed out...Stopping old process");
                p.StandardInput.WriteLine("setoption name Threads value 0");
                Thread.Sleep(30);
                p.Kill();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                var psi = new ProcessStartInfo
                {
                    FileName = "Games/Chess/SethRocksEngine.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                Console.WriteLine("Starting process...");
                p = Process.Start(psi);
                if (p != null)
                {
                    Console.WriteLine("Process is not null setting threads...");
                    p.StandardInput.WriteLine("setoption name Threads value " + Environment.ProcessorCount);
                }
                return false;
            }
        }

        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn. True means end your turn, False means to keep your turn going and re-call this function.</returns>
        public bool RunTurn()
        {
            // Here is where you'll want to code your AI.

            // We've provided sample code that:
            //    1) prints the board to the console
            //    2) prints the opponent's last move to the console
            //    3) prints how much time remaining this AI has to calculate moves
            //    4) makes a random (and probably invalid) move.

            // 1) print the board to the console
            this.PrintCurrentBoard();

            while(!doLoop())
            {

            }


            return true; // to signify we are done with our turn.
        }

    /// <summary>
    /// Prints the current board using pretty ASCII art
    /// </summary>
    /// <remarks>
    /// Note: you can delete this function if you wish
    /// </remarks>
    public void PrintCurrentBoard()
    {
        for (int rank = 9; rank >= -1; rank--)
        {
            string str = "";
            if (rank == 9 || rank == 0) // then the top or bottom of the board
            {
                str = "   +------------------------+";
            }
            else if (rank == -1) // then show the ranks
            {
                str = "     a  b  c  d  e  f  g  h";
            }
            else // board
            {
                str += " " + rank + " |";
                // fill in all the files with pieces at the current rank
                for (int fileOffset = 0; fileOffset < 8; fileOffset++)
                {
                    string file = "" + (char)(((int)"a"[0]) + fileOffset); // start at a, with with file offset increasing the char;
                    Piece currentPiece = null;
                    foreach (var piece in this.Game.Pieces)
                    {
                        if (piece.File == file && piece.Rank == rank) // then we found the piece at (file, rank)
                        {
                            currentPiece = piece;
                            break;
                        }
                    }

                    char code = '.'; // default "no piece";
                    if (currentPiece != null)
                    {
                        code = currentPiece.Type[0];

                        if (currentPiece.Type == "Knight") // 'K' is for "King", we use 'N' for "Knights"
                        {
                            code = 'N';
                        }

                        if (currentPiece.Owner.Id == "1") // the second player (black) is lower case. Otherwise it's upppercase already
                        {
                            code = Char.ToLower(code);
                        }
                    }

                    str += " " + code + " ";
                }

                str += "|";
            }

            Console.WriteLine(str);
        }
    }

    #endregion
}
}
