using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HorseRace
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandAnalyzer();
        }

        static void CommandAnalyzer()
        {
            bool _ExitProgram = false;
            do
            {
                Console.WriteLine("Enter command: ");
                string[] args = Console.ReadLine().Split(' ');

                //Logic to execute the correct command
                switch (args[0])
                {
                    case "create":
                        HorseRace.CreateHorseRace(Convert.ToInt32(args[2]), Convert.ToInt32(args[3]));
                        break;

                    case "start":
                        HorseRace.Start();
                        break;

                    case "kick":
                        HorseRace.KickHorse(Int32.Parse(args[2]));
                        break;

                    case "stop":
                        HorseRace.Stop();
                        break;

                    case "exit":
                        _ExitProgram = HorseRace.Exit();
                        break;

                    case "show":
                        HorseRace.Show();
                        break;

                }
            } while (!_ExitProgram);
        }
    }

    static class HorseRace
    {
        static int _NumHorses;
        public static int _RaceDistMetres;
        static bool _raceStarted;
        static bool _raceStopped;
        static Dictionary<int, Horse> _HorseList;
        static List<Horse> _Ranking;

        //Create the race - inputting the number of horses and race distance
        public static void CreateHorseRace(int NumHorses, int RaceDistMetres)
        {
            _NumHorses = NumHorses;
            _RaceDistMetres = RaceDistMetres;
            _raceStarted = false;
            _HorseList = new Dictionary<int, Horse>();
            _raceStopped = false;
            _Ranking = new List<Horse>();
        }

        //Start the race. Start threads for each horse in this method
        public static void Start()
        {
            Horse horse;
            Thread horseThread;
            for (int i = 0; i < _NumHorses; i++)
            {
                //Start the threads
                horse = new Horse(i);
                _HorseList.Add(i, horse);
                horseThread = new Thread(horse.HorseRunning);
                horseThread.Name = i.ToString();
                horseThread.Start();
            }
            _raceStarted = true;

            //Separate thread to continuously check the status of the race
            new Thread(CheckHorsesStatus).Start();

        }

        //Stop the race. Stop all the threads if running in this method. Print the appropriate status messages to the console
        public static void Stop()
        {
            for (int i = 0; i < _NumHorses; i++)
            {
                _HorseList[i].StopHorse();
            }
            _raceStopped = true;
            Console.WriteLine("All horses have stopped running. The race has been stopped");
        }

        public static bool Exit()
        {
            if (_raceStopped)
            {
                return true;
            }
            else
            {
                Console.WriteLine("You cannot exit if a race is going on. You have to first stop the race.");
                return false;
            }
        }

        public static void KickHorse(int HorseID)
        {
            Horse horseToBeKicked = _HorseList[HorseID];
            horseToBeKicked.KickHorse();
        }

        public static void Show()
        {
            Horse horseDisplay;
            string horseStatus;
            string raceStatus;
            int runningHorses = _NumHorses;
            _Ranking.Clear();

            if (!_raceStarted)
            {
                raceStatus = "Race: Ready to start";
                Console.WriteLine(raceStatus);
                horseStatus = "*".PadRight(_RaceDistMetres, ' ');
                for (int i = 0; i < _NumHorses; i++)
                {
                    Console.WriteLine("horse {0}  [{1}]", i, horseStatus);
                }
            }
            else
            {
                for (int i = 0; i < _NumHorses; i++)
                {
                    horseDisplay = _HorseList[i];
                    if (!horseDisplay._horseRunning)
                    {
                        runningHorses--;
                        if (!_HorseList[i]._horseKicked)
                        {
                            _Ranking.Add(_HorseList[i]);
                        }
                    }

                    _Ranking.Sort(delegate(Horse h1, Horse h2)
                    {
                        return h1.timeTaken.CompareTo(h2.timeTaken);
                    });
                }
                if (runningHorses == 0)
                {
                    raceStatus = "Race: Completed! - Final standings:-";
                }
                else
                {
                    raceStatus = "In Progress";
                }
                Console.WriteLine(raceStatus);
                for (int i = 0; i < _NumHorses; i++)
                {
                    horseDisplay = _HorseList[i];
                    horseStatus = "".PadRight(horseDisplay._HorseDist, '*');
                    horseStatus = horseStatus.PadRight(_RaceDistMetres, ' ');
                    if (horseDisplay._horseRunning)
                    {
                        Console.WriteLine("horse {0}  [{1}]", i, horseStatus);
                    }
                    else
                    {
                        if (horseDisplay._horseKicked)
                        {
                            horseStatus = "".PadRight(horseDisplay._HorseDist, '*') + "k";
                            horseStatus = horseStatus.PadRight(_RaceDistMetres, ' ');
                            Console.WriteLine("horse {0}  [{1}] Stopped running after {2} seconds", i, horseStatus, horseDisplay.timeTaken);
                        }
                        else
                        {
                            Console.WriteLine("horse {0}  [{1}] {2} (Time: {3} seconds)", i, horseStatus, _Ranking.FindIndex(h => h == horseDisplay), horseDisplay.timeTaken);
                        }
                    }
                }
            }
        }

        public static void CheckHorsesStatus()
        {
            int runningHorses = _NumHorses;
            while (!_raceStopped)
            {
                for (int i = 0; i < _NumHorses; i++)
                {
                    if (!_HorseList[i]._horseRunning)
                    {
                        runningHorses--;
                    }
                }
               
                if (runningHorses <= 0)
                {
                    _raceStopped = true;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }

    class Horse
    {
        public int _HorseID { get; private set; }
        public int _HorseDist { get; private set; }
        public Random _LeapMetre { get; private set; }
        public bool _horseKicked { get; private set; }
        public bool _horseRunning { get; private set; }
        public int timeTaken { get; private set; }
        Stopwatch timeMeasure;

        public Horse(int HorseID)
        {
            this._HorseID = HorseID;
            this._HorseDist = 1;
            this._horseKicked = false;
            this._LeapMetre = new Random();
            this._horseRunning = true;
            this.timeMeasure = new Stopwatch();
        }

        //This method has to continuosly run for the horse threads, until KickHorse 
        public void HorseRunning()
        {
            int nextLeap = _LeapMetre.Next(1, 9);
            this.timeMeasure.Start();

            while (((this._HorseDist + nextLeap) < HorseRace._RaceDistMetres) && this._horseRunning)
            {
                this._HorseDist += nextLeap;
                //Console.WriteLine("Thread num {0} and horse distance {1} and horse num {2}", Thread.CurrentThread.Name, this._HorseDist, this._HorseID);
                Thread.Sleep(1000);
                nextLeap = this._LeapMetre.Next(1, 9);
            }
            if ((this._HorseDist < HorseRace._RaceDistMetres) && this._horseRunning)
            {
                this._HorseDist = HorseRace._RaceDistMetres;
            }
            this.StopHorse();
        }

        public void KickHorse()
        {
            if (!this._horseKicked)
            {
                this._horseKicked = true;
                Console.WriteLine("Horse {0} has been kicked!", this._HorseID);
                this.StopHorse();
            }
        }

        public void StopHorse()
        {
            if (this._horseRunning)
            {
                //Thread.CurrentThread.Abort();
                this._horseRunning = false;
                this.timeMeasure.Stop();
                this.timeTaken = (int)timeMeasure.Elapsed.TotalSeconds;
            }
        }
    }
}
