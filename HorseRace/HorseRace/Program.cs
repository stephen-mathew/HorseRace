using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HorseRace
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        //Create a Horse race object
        public void CreateRace(int NumHorses, int RaceDistMetres)
        {
            HorseRace horseRace = new HorseRace(NumHorses, RaceDistMetres);
        }
    }

    class HorseRace
    {
        int _NumHorses;
        int _RaceDistMetres;
        bool _raceStarted;
        Dictionary<int, Horse> _HorseList;

        //Create the race - inputting the number of horses and race distance
        public HorseRace(int NumHorses, int RaceDistMetres)
        {
            this._NumHorses = NumHorses;
            this._RaceDistMetres = RaceDistMetres;
            this._raceStarted = false;
            _HorseList = new Dictionary<int, Horse>();
        }

        //Start the race. Start threads for each horse in this method
        public void Start()
        {
            Horse horse;
            Thread horseThread;
            for (int i = 0; i < _NumHorses; i++)
            {
                //Start the threads
                horse = new Horse(i);
                _HorseList.Add(i, horse);
                horseThread = new Thread(horse.HorseRunning);
                horseThread.Start();
            }
            _raceStarted = true;
        }

        //Stop the race. Stop all the threads if running in this method. Print the appropriate status messages to the console
        public void Stop()
        {
            for (int i = 0; i < _NumHorses; i++)
            {
                _HorseList[i].StopHorse();
            }
            _raceStarted = false;
            Console.WriteLine("All horses have stopped running. The race has been stopped");
        }

        public void Exit()
        {
            if (!_raceStarted)
            {
                //exit the program
            }
            else
            {
                Console.WriteLine("You cannot exit if a race is going on. You have to first stop the race.");
            }
        }

        public void KickHorse(int HorseID)
        {
            Horse horseToBeKicked = _HorseList[HorseID];
            horseToBeKicked.KickHorse();
        }

        public void Show()
        {
            Horse horseDisplay;
            if (!_raceStarted)
            {
                Console.WriteLine("Race: Ready to start");

            }
            else
            {
                for (int i = 0; i < _NumHorses; i++)
                {
                    horseDisplay = _HorseList[i];
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

        public Horse(int HorseID)
        {
            this._HorseID = HorseID;
            this._HorseDist = 1;
            this._horseKicked = false;
            this._LeapMetre = new Random();
            this._horseRunning = true;
        }

        //This method has to continuosly run for the horse threads, until KickHorse 
        public void HorseRunning()
        {
            this._HorseDist += _LeapMetre.Next(1, 9);
            Thread.Sleep(1000);
        }

        public void KickHorse()
        {
            if (!_horseKicked)
            {
                //Thread.CurrentThread.Abort();
                _horseKicked = true;
                Console.WriteLine("Horse {0} has been kicked!", this._HorseID);
                StopHorse();
            }
        }

        public void StopHorse()
        {
            if (_horseRunning)
            {
                //Thread.CurrentThread.Abort();
                _horseRunning = false;
                Console.WriteLine("horse {0} stopped running", this._HorseID);
            }
        }
    }
}
