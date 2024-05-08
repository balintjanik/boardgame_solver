using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tablajatek_AI;

// Plan Rooms-problem-like Q-Learning method
/* Example reward matrix: Goal is to get from 0 (Top Left) to 9 (row 2 col 4)
 * State  Action
 *       [ 0   1   2   3   4   5   6   7   8   9   10   11   12   13   14   15   16   17
 *       
 *  0     -1  -1  -1  -1  -1  -1  -1  -1   0  -1   -1   -1   -1    0   -1   -1   -1   -1  
 *  1     -1  -1  -1  -1  -1  -1  -1  -1  -1  99   -1   -1    0   -1    0   -1   -1   -1
 *  2     -1  -1  -1  -1  -1  -1   0  -1  -1  -1    0   -1   -1    0   -1    0   -1   -1
 *  3     -1  -1  -1  -1  -1  -1  -1   0  -1  -1   -1    0   -1   -1    0   -1    0   -1
 *  4     -1  -1  -1  -1  -1  -1  -1  -1   0  -1   -1   -1   -1   -1   -1    0   -1    0
 *  5     -1  -1  -1  -1  -1  -1  -1  -1  -1  99   -1   -1   -1   -1   -1   -1    0   -1
 *  6     -1  -1   0  -1  -1  -1  -1  -1  -1  -1   -1   -1   -1   -1    0   -1   -1   -1
 *  7     -1  -1  -1   0  -1  -1  -1  -1  -1  -1   -1   -1   -1   -1   -1    0   -1   -1
 *  8      0  -1  -1  -1   0  -1  -1  -1  -1  -1   -1   -1    0   -1   -1   -1    0   -1
 *  9     -1   0  -1  -1  -1   0  -1  -1  -1  -1   -1   -1   -1    0   -1   -1   -1    0
 *  10    -1  -1   0  -1  -1  -1  -1  -1  -1  -1   -1   -1   -1   -1    0   -1   -1   -1
 *  11    -1  -1  -1   0  -1  -1  -1  -1  -1  -1   -1   -1   -1   -1   -1    0   -1   -1
 *  12    -1   0  -1  -1  -1  -1  -1  -1   0  -1   -1   -1   -1   -1   -1   -1   -1   -1
 *  13     0  -1   0  -1  -1  -1  -1  -1  -1  99   -1   -1   -1   -1   -1   -1   -1   -1
 *  14    -1   0  -1   0  -1  -1   0  -1  -1  -1    0   -1   -1   -1   -1   -1   -1   -1
 *  15    -1  -1   0  -1   0  -1  -1   0  -1  -1   -1    0   -1   -1   -1   -1   -1   -1
 *  16    -1  -1  -1   0  -1   0  -1  -1   0  -1   -1   -1   -1   -1   -1   -1   -1   -1
 *  17    -1  -1  -1  -1   0  -1  -1  -1  -1  99   -1   -1   -1   -1   -1   -1   -1   -1
 *      ]
 *  Invalid move: -1
 *  Possible move: 0
 *  Goal move:    99
 *  Problem: the goal is always changing - or is it a problem? Idk yet
 *  
 *  If its not a problem -> Plan:
 *  1. train neuralnetwork (idk yet how)
 *  2. for cycle through samples -> inside that for cycle through field: if color matches samples[i], then make it go to either i1, i2, or i3, and then find next one and so on...
 *  
 */

// TODO: never repeat same actions during training
// TODO: 0=>-1 ; -1=>-5


namespace Tablajatek_AI
{
    public interface QLearningProblem { 
        int NumberOfStates { get; }
        int NumberOfActions { get; }
        int[] GetValidActions(int currentState);
        double GetReward(int currentState, int action);
        bool GoalStateIsReached(int currentState);
    }

    internal class Agent
    {
        private Random _random = new Random();
        private double _gamma;
        private double[][] _qTable1, _qTable2, _qTable3, _qTable4, _qTable5, _qTable6, _qTable7, _qTable8, _qTable9, _qTable10, _qTable11, _qTable12, _qTable13, _qTable14, _qTable15, _qTable16, _qTable17, _qTable18;
        List<double[][]> qtables;
        private TablajatekProblem _qLearningProblem;

        public struct force {
            public bool was;
            public int from1;
            public int from2;
            public int from3;
            public int from4;
        }
        public List<force> forces = new List<force>();

        public void fillForce() {
            force f0 = new force();
            f0.was = false;
            f0.from1 = 8;
            f0.from2 = 12;
            f0.from3 = -1;
            f0.from4 = -1;
            forces.Add(f0);
            force f1 = new force();
            f1.was = false;
            f1.from1 = 12;
            f1.from2 = 9;
            f1.from3 = 14;
            f1.from4 = -1;
            forces.Add(f1);
            force f2 = new force();
            f2.was = false;
            f2.from1 = 6;
            f2.from2 = 13;
            f2.from3 = 15;
            f2.from4 = 10;
            forces.Add(f2);
            force f3 = new force();
            f3.was = false;
            f3.from1 = 7;
            f3.from2 = 14;
            f3.from3 = 16;
            f3.from4 = 11;
            forces.Add(f3);
            force f4 = new force();
            f4.was = false;
            f4.from1 = 8;
            f4.from2 = 15;
            f4.from3 = 17;
            f4.from4 = -1;
            forces.Add(f4);
            force f5 = new force();
            f5.was = false;
            f5.from1 = 9;
            f5.from2 = 16;
            f5.from3 = -1;
            f5.from4 = -1;
            forces.Add(f5);
            force f6 = new force();
            f6.was = false;
            f6.from1 = 2;
            f6.from2 = 14;
            f6.from3 = -1;
            f6.from4 = -1;
            forces.Add(f6);
            force f7 = new force();
            f7.was = false;
            f7.from1 = 3;
            f7.from2 = 15;
            f7.from3 = -1;
            f7.from4 = -1;
            forces.Add(f7);
            force f8 = new force();
            f8.was = false;
            f8.from1 = 0;
            f8.from2 = 12;
            f8.from3 = 4;
            f8.from4 = 16;
            forces.Add(f8);
            force f9 = new force();
            f9.was = false;
            f9.from1 = 1;
            f9.from2 = 13;
            f9.from3 = 5;
            f9.from4 = 17;
            forces.Add(f9);;
            force f10 = new force();
            f10.was = false;
            f10.from1 = 2;
            f10.from2 = 14;
            f10.from3 = -1;
            f10.from4 = -1;
            forces.Add(f10);
            force f11 = new force();
            f11.was = false;
            f11.from1 = 3;
            f11.from2 = 15;
            f11.from3 = -1;
            f11.from4 = -1;
            forces.Add(f11);
            force f12 = new force();
            f12.was = false;
            f12.from1 = 1;
            f12.from2 = 8;
            f12.from3 = -1;
            f12.from4 = -1;
            forces.Add(f12);
            force f13 = new force();
            f13.was = false;
            f13.from1 = 0;
            f13.from2 = 2;
            f13.from3 = 9;
            f13.from4 = -1;
            forces.Add(f13);
            force f14 = new force();
            f14.was = false;
            f14.from1 = 1;
            f14.from2 = 3;
            f14.from3 = 6;
            f14.from4 = 10;
            forces.Add(f14);
            force f15 = new force();
            f15.was = false;
            f15.from1 = 2;
            f15.from2 = 4;
            f15.from3 = 7;
            f15.from4 = 11;
            forces.Add(f15);
            force f16 = new force();
            f16.was = false;
            f16.from1 = 3;
            f16.from2 = 5;
            f16.from3 = 8;
            f16.from4 = -1;
            forces.Add(f16);
            force f17 = new force();
            f17.was = false;
            f17.from1 = 4;
            f17.from2 = 9;
            f17.from3 = -1;
            f17.from4 = -11;
            forces.Add(f17);
        }

        public Agent(double gamma, TablajatekProblem qLearningProblem) {
            _qLearningProblem = qLearningProblem;
            _gamma = gamma;
            _qTable1 = new double[qLearningProblem.NumberOfStates][];
            _qTable2 = new double[qLearningProblem.NumberOfStates][];
            _qTable3 = new double[qLearningProblem.NumberOfStates][];
            _qTable4 = new double[qLearningProblem.NumberOfStates][];
            _qTable5 = new double[qLearningProblem.NumberOfStates][];
            _qTable6 = new double[qLearningProblem.NumberOfStates][];
            _qTable7 = new double[qLearningProblem.NumberOfStates][];
            _qTable8 = new double[qLearningProblem.NumberOfStates][];
            _qTable9 = new double[qLearningProblem.NumberOfStates][];
            _qTable10 = new double[qLearningProblem.NumberOfStates][];
            _qTable11 = new double[qLearningProblem.NumberOfStates][];
            _qTable12 = new double[qLearningProblem.NumberOfStates][];
            _qTable13 = new double[qLearningProblem.NumberOfStates][];
            _qTable14 = new double[qLearningProblem.NumberOfStates][];
            _qTable15 = new double[qLearningProblem.NumberOfStates][];
            _qTable16 = new double[qLearningProblem.NumberOfStates][];
            _qTable17 = new double[qLearningProblem.NumberOfStates][];
            _qTable18 = new double[qLearningProblem.NumberOfStates][];
            for (int i = 0; i < qLearningProblem.NumberOfStates; i++)
            {
                _qTable1[i] = new double[qLearningProblem.NumberOfActions];
                _qTable2[i] = new double[qLearningProblem.NumberOfActions];
                _qTable3[i] = new double[qLearningProblem.NumberOfActions];
                _qTable4[i] = new double[qLearningProblem.NumberOfActions];
                _qTable5[i] = new double[qLearningProblem.NumberOfActions];
                _qTable6[i] = new double[qLearningProblem.NumberOfActions];
                _qTable7[i] = new double[qLearningProblem.NumberOfActions];
                _qTable8[i] = new double[qLearningProblem.NumberOfActions];
                _qTable9[i] = new double[qLearningProblem.NumberOfActions];
                _qTable10[i] = new double[qLearningProblem.NumberOfActions];
                _qTable11[i] = new double[qLearningProblem.NumberOfActions];
                _qTable12[i] = new double[qLearningProblem.NumberOfActions];
                _qTable13[i] = new double[qLearningProblem.NumberOfActions];
                _qTable14[i] = new double[qLearningProblem.NumberOfActions];
                _qTable15[i] = new double[qLearningProblem.NumberOfActions];
                _qTable16[i] = new double[qLearningProblem.NumberOfActions];
                _qTable17[i] = new double[qLearningProblem.NumberOfActions];
                _qTable18[i] = new double[qLearningProblem.NumberOfActions];
            }
            fillForce();
            qtables = new List<double[][]> { _qTable1, _qTable2, _qTable3, _qTable4, _qTable5, _qTable6, _qTable7, _qTable8, _qTable9, _qTable10, _qTable11, _qTable12, _qTable13, _qTable14, _qTable15, _qTable16, _qTable17, _qTable18 };
        }

        public void TrainAgent(int numberOfIterations) {
            for (int i = 0; i < 18*Program.trainCount; i++)
            {
                int initialState;
                if (i>17)
                {
                    initialState = i - (i/18)*18;
                }
                else
                {
                    initialState = i;
                }
                for (int j = 0; j < 18*Program.trainCount; j++)
                {
                    int goal;
                    if (j > 17)
                    {
                        goal = j - (j / 18) * 18;
                    }
                    else
                    {
                        goal = j;
                    }
                    InitializeEpisode(initialState, goal);
                }

                showProgress(i);
            }
            Console.Write(" ");
            
        }

        public void showProgress(int i) {
            // print progress
            int progress = Convert.ToInt32(Math.Round(Remap(i, 0, 18 * Program.trainCount - 1, 0, 100)));
            Console.Write("\r");
            int c = 0;
            for (c = 0; c < progress / 5; c++)
            {
                Console.Write("|");
            }
            for (; c < 20; c++)
            {
                Console.Write("-");
            }
            //Console.Write(" 100%");
            Console.Write(" {0}% ", progress);
        }

        public void showTables() {
            Console.WriteLine();
            Console.WriteLine("QTables: ");
            for (int i = 0; i < qtables.Count; i++)
            {
                Console.WriteLine("QTable " + i);
                for (int j = 0; j < _qLearningProblem.NumberOfStates; j++)
                {
                    for (int k = 0; k < _qLearningProblem.NumberOfActions; k++)
                    {
                        Console.Write(qtables[i][j][k] + " ");
                    }
                    Console.WriteLine();
                }
            }
        }

        public int Run(int initialState, int goal) {
            //if (initialState < 0 || initialState > _qLearningProblem.NumberOfStates) throw new ArgumentException($"The initial state can be between [0-{_qLearningProblem.NumberOfStates}]", nameof(initialState));

            if (initialState >= 0 && Program.showdet) Console.WriteLine("Initialstate: " + initialState);
            int state = Math.Abs(initialState);
            double[][] _qTable = _qTable1;
            switch (goal)
            {
                case 0:
                    _qTable = _qTable1;
                    break;
                case 1:
                    _qTable = _qTable2;
                    break;
                case 2:
                    _qTable = _qTable3;
                    break;
                case 3:
                    _qTable = _qTable4;
                    break;
                case 4:
                    _qTable = _qTable5;
                    break;
                case 5:
                    _qTable = _qTable6;
                    break;
                case 6:
                    _qTable = _qTable7;
                    break;
                case 7:
                    _qTable = _qTable8;
                    break;
                case 8:
                    _qTable = _qTable9;
                    break;
                case 9:
                    _qTable = _qTable10;
                    break;
                case 10:
                    _qTable = _qTable11;
                    break;
                case 11:
                    _qTable = _qTable12;
                    break;
                case 12:
                    _qTable = _qTable13;
                    break;
                case 13:
                    _qTable = _qTable14;
                    break;
                case 14:
                    _qTable = _qTable15;
                    break;
                case 15:
                    _qTable = _qTable16;
                    break;
                case 16:
                    _qTable = _qTable17;
                    break;
                case 17:
                    _qTable = _qTable18;
                    break;
                default:
                    break;
            }
            int steps = 0;
            while (true)
            {
                if (initialState >= 0) Program.curSteps++;
                steps++;
                int action = _qTable[state].ToList().IndexOf(_qTable[state].Max());
                int ia = action / 6;
                int ja = action % 6;
                int ic = state / 6;
                int jc = state % 6;
                if (initialState >= 0)
                {
                    Thread.Sleep(Convert.ToInt32(Math.Round(295 * Program.pace)));
                    Program.clickOnSquare(state);
                    if (Program.pace != 1.5)
                    {
                        Thread.Sleep(Convert.ToInt32(Math.Round(100 * Program.pace)));
                    }
                }
                if (initialState >= 0)
                {
                    Program.clickOnSquare(action);
                }
                if (initialState >= 0 && Program.showdet) Console.WriteLine("From {0} to {1} (goal is {2})", state, action, goal);
                state = action;
                if (initialState < 0 && steps > 1000) break;
                if (_qLearningProblem.GoalStateIsReached(action, goal))
                {
                    if(initialState >= 0 && Program.showdet) Console.WriteLine("EndState: " + action);
                    break;
                }
            }
            return steps;
            
        }

        public int InitializeEpisode(int initialState, int goal) {
            int currentState = initialState;
            currentState = initialState;
            int counter = 0;
            
            while (true)
            {
                
                currentState = TakeAction(currentState, goal, initialState);
                
                if (_qLearningProblem.GoalStateIsReached(currentState, goal)) break;
                counter++;
                if (counter > 100000) break;
            }
            return counter;
        }

        private int TakeAction(int currentState, int goal, int initialState) {  // initial state is only needed for my way of filtering duplicate random actions
            var validActions = _qLearningProblem.GetValidActions(currentState);
            int randomIndexAction = _random.Next(0, validActions.Length);
            int action = validActions[randomIndexAction];

            // force at least 1 good solution for all goals during training
            int i = 0;
            while (i < forces.Count()) {
                if (goal == i && (currentState == forces[i].from1 || currentState == forces[i].from2 || currentState == forces[i].from3 || currentState == forces[i].from4) && !forces[i].was)
                {
                    action = goal;
                    force tmp = new force();
                    tmp.was = true;
                    tmp.from1 = forces[i].from1;
                    tmp.from2 = forces[i].from2;
                    tmp.from3 = forces[i].from3;
                    tmp.from4 = forces[i].from4;
                    forces[i] = tmp;
                    break;
                }
                i++;
            }

            _qLearningProblem.valid[currentState][randomIndexAction] = -1;
            double[][] _qTable = _qTable1; ;
            switch (goal)
            {
                case 0:
                    _qTable = _qTable1;
                    break;
                case 1:
                    _qTable = _qTable2;
                    break;
                case 2:
                    _qTable = _qTable3;
                    break;
                case 3:
                    _qTable = _qTable4;
                    break;
                case 4:
                    _qTable = _qTable5;
                    break;
                case 5:
                    _qTable = _qTable6;
                    break;
                case 6:
                    _qTable = _qTable7;
                    break;
                case 7:
                    _qTable = _qTable8;
                    break;
                case 8:
                    _qTable = _qTable9;
                    break;
                case 9:
                    _qTable = _qTable10;
                    break;
                case 10:
                    _qTable = _qTable11;
                    break;
                case 11:
                    _qTable = _qTable12;
                    break;
                case 12:
                    _qTable = _qTable13;
                    break;
                case 13:
                    _qTable = _qTable14;
                    break;
                case 14:
                    _qTable = _qTable15;
                    break;
                case 15:
                    _qTable = _qTable16;
                    break;
                case 16:
                    _qTable = _qTable17;
                    break;
                case 17:
                    _qTable = _qTable18;
                    break;
                default:
                    break;
            }

            double saReward = _qLearningProblem.GetReward(currentState, action, goal);
            double nsReward = _qTable[action].Max();
            double qCurrentState = saReward + (_gamma * nsReward);
            _qTable[currentState][action] = qCurrentState;
            int newState = action;
            switch (goal)
            {
                case 0:
                    _qTable1 = _qTable;
                    break;
                case 1:
                    _qTable2 = _qTable;
                    break;
                case 2:
                    _qTable3 = _qTable;
                    break;
                case 3:
                    _qTable4 = _qTable;
                    break;
                case 4:
                    _qTable5 = _qTable;
                    break;
                case 5:
                    _qTable6 = _qTable;
                    break;
                case 6:
                    _qTable7 = _qTable;
                    break;
                case 7:
                    _qTable8 = _qTable;
                    break;
                case 8:
                    _qTable9 = _qTable;
                    break;
                case 9:
                    _qTable10 = _qTable;
                    break;
                case 10:
                    _qTable11 = _qTable;
                    break;
                case 11:
                    _qTable12 = _qTable;
                    break;
                case 12:
                    _qTable13 = _qTable;
                    break;
                case 13:
                    _qTable14 = _qTable;
                    break;
                case 14:
                    _qTable15 = _qTable;
                    break;
                case 15:
                    _qTable16 = _qTable;
                    break;
                case 16:
                    _qTable17 = _qTable;
                    break;
                case 17:
                    _qTable18 = _qTable;
                    break;
                default:
                    break;
            }
            return newState;
        }

        private int SetInitialState(int numberOfStates) {
            return _random.Next(0, numberOfStates);
        }

        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
