using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Windows.Forms.Design;

namespace Tablajatek_AI
{
    internal class Program
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int ExtraInfo);

        [Flags]
        public enum MouseEventFlags
        { 
            LEFTDOWN = 0x00000002,
            LEFTUP =   0x00000004
        }

        public enum ColorDef
        {
            PINK,
            BLUE,
            YELLOW,
            GREEN,
            RED,
            GREY
        }
        public static Square[] samples = new Square[6];
        public static Square[,] field = new Square[3,6];
        static Square newgame;
        static Square ok;

        public static int startX, startY;
        public static int trainCount = 1;
        public static double pace = 2;

        public struct Square { 
            public int x;
            public int y;
            public ColorDef color;
        }

        static bool trained = false;
        public static bool showdet = false;
        static int rounds = 0;
        static int besttime = 0;
        static int leastswaps = 0;

        static int curTime = 0;
        public static int curSteps = 0;
        static int avgTime = 0;
        static int avgSteps = 0;
        static List<int[]> games = new List<int[]>();

        static void Main(string[] args)
        {
            initText();

            var agent = new Agent(0.8, new TablajatekProblem());
            bool quit = false;
            while (!quit)
            {
                Console.Write("User$ ");
                string response = Console.ReadLine();
                if (response.Split(" ")[0] == "-train")
                {
                    try
                    {
                        try
                        {
                            trainCount = Convert.ToInt32(response.Split()[1]);
                        }
                        catch (Exception)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR: Invalid syntax");
                            Console.ForegroundColor = ConsoleColor.White;
                            continue;
                        }

                        if (train(agent) == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\nSUCCESS\nTraining was succesful.", Console.ForegroundColor);
                            Console.ForegroundColor = ConsoleColor.White;
                            trained = true;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nERROR: Training has run into an error.\nCheck if the whole game is visible on screen and try again.", Console.ForegroundColor);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: Training rounds' number must be an integer");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                /*else if (response == "-game")
                {
                    if (trained)
                    {
                        if (findData() == 0)
                        {
                            Thread.Sleep(80);
                            clickAt(newgame.x, newgame.y);
                            Thread.Sleep(80);
                            showGame(agent);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nERROR: Data was not found.\nCheck if the whole game is visible on screen and try again.", Console.ForegroundColor);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: Neural network must be trained first");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }*/
                else if (response.Split()[0] == "-game") {
                    int n = 0;
                    try
                    {
                        n = Convert.ToInt32(response.Split()[1]);
                    }
                    catch (Exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: Invalid syntax", Console.ForegroundColor);
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }
                    if (trained)
                    {
                        if (findData() == 0)
                        {
                            bool wasErr = false;
                            for (int i = 0; i < n; i++)
                            {
                                if (wasErr)
                                {
                                    Console.WriteLine("Due to previous errors, regaining data is necessary!");
                                    if (findData()==0)
                                    {
                                        Thread.Sleep(80);
                                        clickAt(newgame.x, newgame.y);
                                        Thread.Sleep(80);
                                        showGame(agent);
                                        Thread.Sleep(1000);
                                        clickAt(ok.x, ok.y);
                                        Thread.Sleep(100);
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\nERROR: Data was not found.\nCheck if the whole game is visible on screen and try again.", Console.ForegroundColor);
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                }
                                else
                                {
                                    Thread.Sleep(80);
                                    clickAt(newgame.x, newgame.y);
                                    Thread.Sleep(80);
                                    if (showGame(agent) != 0)
                                    {
                                        wasErr= true;
                                    }
                                    Thread.Sleep(1000);
                                    clickAt(ok.x, ok.y);
                                    Thread.Sleep(100);
                                }
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nERROR: Data was not found.\nCheck if the whole game is visible on screen and try again.", Console.ForegroundColor);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: Neural network must be trained first");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else if (response == "-showtables")
                {
                    agent.showTables();
                }
                else if (response.Split(" ")[0] == "-pace")
                {
                    try
                    {
                        pace = Convert.ToDouble(response.Split(" ")[1]);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: Invalid syntax");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }
                    if (pace < 1.5)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: Pace variable must have a value of at least 1.5");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }
                    Console.Write("Pace has been updated to " + pace);
                    if (pace == 1.5) Console.Write(" (speedmode)");
                    Console.WriteLine();
                }
                else if (response == "-exit")
                {
                    Console.WriteLine("Closing...");
                    quit = true;
                }
                else if (response == "-showdetails")
                {
                    showdet = true;
                    Console.WriteLine("Details are now going to be visible");
                }
                else if (response == "-hidedetails")
                {
                    showdet = false;
                    Console.WriteLine("Details are now going to be hidden");
                }
                else if (response == "-data")
                {
                    Console.WriteLine("Game pace: " + pace);
                    Console.WriteLine("Rounds trained: " + trainCount);
                }
                else if (response == "-help")
                {
                    help();
                }
                else if (response == "-stats")
                {
                    stats();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR: Invalid command");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine();
            }
            
        }

        // play a game
        static int showGame(Agent agent) { // GOOD BUT PROBLEM: I remove the placed ones, and then use the indexing, but the indexing is now different
            curTime= 0;
            curSteps= 0;
            
            TimerCallback callback = new TimerCallback(Tick);
            System.Threading.Timer stateTimer = new System.Threading.Timer(callback, null, 0, 1000);
            curTime--;

            bool error = false;

            if (readData() == 0)
            {
                int counter = 1;
                while (counter != 18 && !error)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        while (!error)
                        {
                            if (readData() == 0)
                            {
                                List<int> curFields = new List<int>();
                                List<int> goals = new List<int>();
                                // fill goals with free goals
                                for (int i = 0; i < 3; i++)
                                {
                                    if (field[i, k].color != samples[k].color)
                                    {
                                        goals.Add(i * 6 + k);
                                    }
                                }
                                // fill curFields with not yet placed k colored fields
                                for (int i = 0; i < 3; i++)
                                {
                                    for (int j = 0; j < 6; j++)
                                    {
                                        if (samples[k].color == field[i, j].color && k != j)
                                        {
                                            curFields.Add(i * 6 + j);
                                        }
                                    }
                                }

                                // find least steps (and the goal for that) for all curFields and store data in moves list ([initstate, steps, goal], [initstate2, steps2, goal2])
                                List<int[]> moves = new List<int[]>();
                                for (int i = 0; i < curFields.Count; i++)
                                {
                                    int steps = 100000;
                                    int curGoal = -1;
                                    int curInitialState = -curFields[i];
                                    for (int j = 0; j < goals.Count; j++)
                                    {
                                        int curStep = agent.Run(curInitialState, goals[j]);
                                        if (curStep < steps)
                                        {
                                            steps = curStep;
                                            curGoal = goals[j];
                                        }
                                    }
                                    int[] tmp = { -curInitialState, steps, curGoal }; // invert back the inverted initialstate
                                    moves.Add(tmp);
                                }

                                // break if there are no possible moves
                                if (moves.Count() == 0) break;

                                // find move with least step
                                int minStep = moves[0][1];
                                int minInd = 0;
                                for (int i = 0; i < moves.Count; i++)
                                {
                                    if (moves[i][1] <= minStep)
                                    {
                                        minStep = moves[i][1];
                                        minInd = i;
                                    }
                                }

                                // perform step
                                int initialState = moves[minInd][0];
                                int goal = moves[minInd][2];
                                try
                                {
                                    agent.Run(initialState, goal);
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"\nERROR: {ex.Message}");
                                    Console.WriteLine("The game has run into an error and stopped");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    error = true;
                                }
                            }
                            else if (readData() == -1)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\nERROR: Cannot convert rgb data");
                                Console.WriteLine("The game has run into an error and stopped");
                                Console.ForegroundColor = ConsoleColor.White;
                                error = true;
                            }
                            else if (readData() == -2)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\nERROR: Board is not fully visible");
                                Console.WriteLine("The game has run into an error and stopped");
                                Console.ForegroundColor = ConsoleColor.White;
                                error = true;
                            }
                        }
                    }

                    counter = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            if (samples[j].color == field[i, j].color)
                            {
                                counter++;
                            }
                        }
                    }
                    if (showdet) Console.WriteLine("Matching places: " + counter);
                }

                stateTimer.Dispose();
                // save time and steps
                if (!error)
                {
                    Console.WriteLine();
                    if (curTime < besttime || besttime == 0)
                    {
                        besttime = curTime;
                    }
                    if (curSteps < leastswaps || leastswaps == 0)
                    {
                        leastswaps = curSteps;
                    }
                    rounds++;
                    int[] curGame = { rounds, curTime, curSteps };
                    games.Add(curGame);
                }
                
            }
            else if (readData() == -1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nERROR: Cannot convert rgb data");
                Console.WriteLine("The game has run into an error and stopped");
                Console.ForegroundColor = ConsoleColor.White;
                error = true;
            }
            else if (readData() == -2) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nERROR: Board is not fully visible");
                Console.WriteLine("The game has run into an error and stopped");
                Console.ForegroundColor = ConsoleColor.White;
                error = true;
            }

            if (error)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        // train neural network through several games (games' number can be specified in trainCount variable)
        static int train(Agent agent) {
            Console.WriteLine("Training has started...");
            agent.TrainAgent(trainCount);
            Console.WriteLine("Training done");
            return 0;
        }

        // find game on screen to save a starting position for reading data
        static int findData() {
            // read screen size
            Console.Write("Screen width: ");
            int width = 0;
            while (width == 0)
            {
                try
                {
                    width = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception)
                {
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine("ERROR: Screen's width must be an integer");
                    Console.ForegroundColor= ConsoleColor.White;
                }
            }
            Console.Write("Screen height: ");
            int height = 0;
            while (height == 0)
            {
                try
                {
                    height = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR: Screen's height must be an integer");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            // find top left square
            Console.Write("Searching for data on screen...");
            int i = 50;
            int j = 50;
            bool foundApp = false;
            startX = 0;
            startY = 0;
            while (i < height-500 && !foundApp)
            {
                while (j < width-700 && !foundApp)
                {
                    string[] rgb = getColorAt(j, i).Split();
                    if (rgb[0] == "ERROR")
                    {
                        
                        return -1;
                    }
                    int r = Convert.ToInt32(rgb[0]);
                    int g = Convert.ToInt32(rgb[1]);
                    int b = Convert.ToInt32(rgb[2]);
                    if ((r == 255 && g == 0 && b == 255) || (r == 0 && g == 255 && b == 255) || (r == 255 && g == 255 && b == 0) || (r == 0 && g == 255 && b == 0) || (r == 255 && g == 0 && b == 0) || (r == 192 && g == 192 && b == 192))
                    {
                        string[] rgb2 = getColorAt(j + 60, i).Split();
                        int r2 = Convert.ToInt32(rgb2[0]);
                        int g2 = Convert.ToInt32(rgb2[1]);
                        int b2 = Convert.ToInt32(rgb2[2]);
                        if ((r2 == 255 && g2 == 0 && b2 == 255) || (r2 == 0 && g2 == 255 && b2 == 255) || (r2 == 255 && g2 == 255 && b2 == 0) || (r2 == 0 && g2 == 255 && b2 == 0) || (r2 == 255 && g2 == 0 && b2 == 0) || (r2 == 192 && g2 == 192 && b2 == 192))
                        {
                            foundApp = true;
                            startX = j;
                            startY = i;
                        }
                    }
                    j += 10;
                }
                i += 10;
                j = 50;
            }
            Console.WriteLine();
            if (foundApp)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Found data!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                return -1;
            }

            // save newgame btn
            newgame.x = startX + 500;
            newgame.y = startY;
            newgame.color = ColorDef.GREY;

            // save ok btn
            ok.x = Convert.ToInt32((startX + 955)*0.8);
            ok.y = Convert.ToInt32((startY + 485)*0.8);
            ok.color = ColorDef.GREY;

            return 0;
        }

        // read data relative to previously found starting position
        static int readData() {
            // save sample row
            int x = startX;
            for (int i = 0; i < 6; i++)
            {
                string[] rgb = getColorAt(x, startY).Split();
                try
                {
                    Convert.ToInt32(rgb[0]);
                    Convert.ToInt32(rgb[1]);
                    Convert.ToInt32(rgb[2]);
                }
                catch (Exception)
                {
                    return -1;
                }
                int r = Convert.ToInt32(rgb[0]);
                int g = Convert.ToInt32(rgb[1]);
                int b = Convert.ToInt32(rgb[2]);
                Square tmp = new Square();
                tmp.x = x;
                tmp.y = startY;
                if (r == 255 && g == 0 && b == 255) {
                    tmp.color = ColorDef.PINK;
                }
                else if (r == 0 && g == 255 && b == 255)
                {
                    tmp.color = ColorDef.BLUE;
                }
                else if (r == 255 && g == 255 && b == 0)
                {
                    tmp.color = ColorDef.YELLOW;
                }
                else if (r == 0 && g == 255 && b == 0)
                {
                    tmp.color = ColorDef.GREEN;
                }
                else if (r == 255 && g == 0 && b == 0)
                {
                    tmp.color = ColorDef.RED;
                }
                else if (r == 192 && g == 192 && b == 192)
                {
                    tmp.color = ColorDef.GREY;
                }
                samples[i] = tmp;
                x += 60;
            }

            // save field
            x = startX;
            int y = startY + 120;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    string[] rgb = getColorAt(x, y).Split();
                    
                    try
                    {
                        Convert.ToInt32(rgb[0]);
                        Convert.ToInt32(rgb[1]);
                        Convert.ToInt32(rgb[2]);
                    }
                    catch (Exception)
                    {
                        return -1;
                    }
                    int r = Convert.ToInt32(rgb[0]);
                    int g = Convert.ToInt32(rgb[1]);
                    int b = Convert.ToInt32(rgb[2]);
                    Square tmp = new Square();
                    tmp.x = x;
                    tmp.y = y;
                    if (r == 255 && g == 0 && b == 255)
                    {
                        tmp.color = ColorDef.PINK;
                    }
                    else if (r == 0 && g == 255 && b == 255)
                    {
                        tmp.color = ColorDef.BLUE;
                    }
                    else if (r == 255 && g == 255 && b == 0)
                    {
                        tmp.color = ColorDef.YELLOW;
                    }
                    else if (r == 0 && g == 255 && b == 0)
                    {
                        tmp.color = ColorDef.GREEN;
                    }
                    else if (r == 255 && g == 0 && b == 0)
                    {
                        tmp.color = ColorDef.RED;
                    }
                    else if (r == 192 && g == 192 && b == 192)
                    {
                        tmp.color = ColorDef.GREY;
                    }
                    field[i,j] = tmp;
                    x += 60;
                }
                y += 60;
                x = startX;
            }
            int pinks = 0;
            int blues = 0;
            int yellows = 0;
            int greens = 0;
            int reds = 0;
            int greys = 0;
            for (int i = 0; i < 6; i++)
            {
                switch (samples[i].color)
                {
                    case ColorDef.PINK:
                        pinks++;
                        break;
                    case ColorDef.BLUE:
                        blues++;
                        break;
                    case ColorDef.YELLOW:
                        yellows++;
                        break;
                    case ColorDef.GREEN:
                        greens++;
                        break;
                    case ColorDef.RED:
                        reds++;
                        break;
                    case ColorDef.GREY:
                        greys++;
                        break;
                    default:
                        break;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    switch (field[i,j].color)
                    {
                        case ColorDef.PINK:
                            pinks++;
                            break;
                        case ColorDef.BLUE:
                            blues++;
                            break;
                        case ColorDef.YELLOW:
                            yellows++;
                            break;
                        case ColorDef.GREEN:
                            greens++;
                            break;
                        case ColorDef.RED:
                            reds++;
                            break;
                        case ColorDef.GREY:
                            greys++;
                            break;
                        default:
                            break;
                    }
                }
            }
            if (pinks != 4 || blues != 4 || yellows != 4 || greens != 4 || reds != 4 || greys != 4)
            {
                return -2;
            }

            // print read table if showdetails is turned on
            if (showdet)
            {
                // print samples
                Console.WriteLine(" _________________________________________________________________________________________________________________");
                Console.Write("| ");
                for (int i = 0; i < 6; i++)
                {
                    writeColor(samples[i].color);
                    Console.Write(samples[i].x + " " + samples[i].y + "   | ");
                }
                Console.WriteLine("\n|__________________|__________________|__________________|__________________|__________________|__________________|");
                // print field
                Console.WriteLine(" _________________________________________________________________________________________________________________");
                for (int i = 0; i < 3; i++)
                {
                    Console.Write("| ");
                    for (int j = 0; j < 6; j++)
                    {
                        writeColor(field[i, j].color);
                        Console.Write(field[i, j].x + " " + field[i, j].y + "   | ");
                    }
                    Console.WriteLine("\n|__________________|__________________|__________________|__________________|__________________|__________________|");
                }
            }
            return 0;
        }

        // get color at given x and y coordinates (enum ColorDef)
        static string getColorAt(int x, int y)
        {
            Bitmap bmp = new Bitmap(1, 1);
            Rectangle bounds = new Rectangle(x,y,1,1);
            try
            {
                using (Graphics g = Graphics.FromImage(bmp)) g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            catch (Exception)
            {
                return "ERROR";
            }
            
            Color c = bmp.GetPixel(0,0);

            return c.R.ToString() + " " + c.G.ToString() + " " + c.B.ToString();
        }

        // print value of enum ColorDef
        static void writeColor(ColorDef c) {
            switch (c)
            {
                case ColorDef.PINK:
                    Console.Write("PINK   ");
                    break;
                case ColorDef.BLUE:
                    Console.Write("BLUE   ");
                    break;
                case ColorDef.YELLOW:
                    Console.Write("YELLOW ");
                    break;
                case ColorDef.GREEN:
                    Console.Write("GREEN  ");
                    break;
                case ColorDef.RED:
                    Console.Write("RED    ");
                    break;
                case ColorDef.GREY:
                    Console.Write("GREY   ");
                    break;
                default:
                    Console.Write("NA     ");
                    break;
            }
        }

        // click at given x and y coordinates
        static void clickAt(int x, int y) { 
            // save cursors pos, move it to xy, click, move back to saved pos
            Cursor.Position = new Point(x,y);
            mouse_event((int) (MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        // click on index indexed square
        public static void clickOnSquare(int index)
        {
            // save cursors pos, move it to xy, click, move back to saved pos
            int i = index / 6;
            int j = index % 6;
            Cursor.Position = new Point(Convert.ToInt32(field[i, j].x * 0.8), Convert.ToInt32(field[i, j].y * 0.8));
            mouse_event((int)(MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
            if (pace==1.5)
            {
                Thread.Sleep(80);
            }
            else
            {
                Thread.Sleep(Convert.ToInt32(Math.Round(100*pace)));
            }
            mouse_event((int)(MouseEventFlags.LEFTUP), 0, 0, 0, 0);
        }

        private static void initText() {
            Console.WriteLine("Tablajatek AI");
            Console.WriteLine("(c) Bálint Áron Janik. All rights reserved.");
            Console.WriteLine("For commands type \"-help\"");
            Console.WriteLine();
        }

        private static void help() {
            Console.WriteLine(" ________________________________________________________________________________________________");
            Console.WriteLine("|  Command name  |                        Usage                           |        Example       |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -train n      | Trains the neural network for n cycles. More cycles    |  -train 5            |");
            Console.WriteLine("|                | make the training slower, but provide better results.  |  (trains 5 cycles)   |");
            Console.WriteLine("|                | (A number between 3 and 10 is advised)                 |                      |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -game n       | Starts n games after providing the screen resolution.  |  -game 5             |");
            Console.WriteLine("|                | The game must be opened, and fully visible on the      |  (plays 5 games)     |");
            Console.WriteLine("|                | screen (preferably top left corner).                   |                      |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -showtables   | Shows neural network's weights. (developer purposes)   |  -showtables         |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -pace n       | Sets the game's pace. The higher the n value is, the   |  -pace 1,7           |");
            Console.WriteLine("|                | slower the game will be. A value of 1.5 is the fastest |  (must be written    |");
            Console.WriteLine("|                | mode, with added extra speed. (A decimal number        |   with comma)        |");
            Console.WriteLine("|                | between 1.5 and 3 is advised)                          |                      |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -showdetails  | Shows details in console during a game. (developer     |  -showdetails        |");
            Console.WriteLine("|                | purposes)                                              |                      |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -hidedetails  | Hides details from console during a game. (developer   |  -hidedetails        |");
            Console.WriteLine("|                | purposes) [set by default]                             |                      |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -stats        | Shows statistics of previously played games since the  |  -stats              |");
            Console.WriteLine("|                | program was started. (rounds played, best time, least  |                      |");
            Console.WriteLine("|                | swaps)                                                 |                      |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -help         | Shows all commands.                                    |  -help               |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
            Console.WriteLine("|  -exit         | Exits the program.                                     |  -exit               |");
            Console.WriteLine("|________________|________________________________________________________|______________________|");
        }

        private static void stats() {

            // calculate average time and average steps
            avgTime= 0;
            avgSteps= 0;
            Console.WriteLine(" _______________________________________________");
            Console.WriteLine("|\t\t      Games\t\t\t|");
            Console.WriteLine("|_______________________________________________|");
            if (games.Count == 0)
            {
                Console.WriteLine("|\t  No games have been played yet.\t|");
            }
            else
            {
                for (int i = 0; i < games.Count; i++)
                {
                    Console.WriteLine("|    Game {0}\t{1} seconds,\t{2} swaps\t|", games[i][0], games[i][1], games[i][2]);
                    avgTime += games[i][1];
                    avgSteps += games[i][2];
                }
                avgTime = avgTime / games.Count;
                avgSteps = avgSteps / games.Count;
            }
            Console.WriteLine("|_______________________________________________|");

            // print stats
            int tmp;
            Console.WriteLine(" ________________________");
            tmp = rounds.ToString().Length;
            if (tmp == 1)
            {
                Console.WriteLine("| Rounds: \t    {0}    |", rounds);
            }
            else if (tmp == 2)
            {
                Console.WriteLine("| Rounds: \t    {0}   |", rounds);
            }
            else if (tmp == 3)
            {
                Console.WriteLine("| Rounds: \t    {0}  |", rounds);
            }
            Console.WriteLine("|________________________|");

            tmp = besttime.ToString().Length;
            if (tmp == 1)
            {
                Console.WriteLine("| Best time (s):    {0}    |", besttime);
            }
            else if (tmp == 2)
            {
                Console.WriteLine("| Best time (s):    {0}   |", besttime);
            }
            else if (tmp == 3)
            {
                Console.WriteLine("| Best time (s):    {0}  |", besttime);
            }
            Console.WriteLine("|________________________|");

            tmp = leastswaps.ToString().Length;
            if (tmp == 1)
            {
                Console.WriteLine("| Least swaps: \t    {0}    |", leastswaps);
            }
            else if (tmp == 2)
            {
                Console.WriteLine("| Least swaps: \t    {0}   |", leastswaps);
            }
            else if (tmp == 3)
            {
                Console.WriteLine("| Least swaps: \t    {0}  |", leastswaps);
            }
            Console.WriteLine("|________________________|");

            tmp = avgTime.ToString().Length;
            if (tmp == 1)
            {
                Console.WriteLine("| Average time (s): {0}    |", avgTime);
            }
            else if (tmp == 2)
            {
                Console.WriteLine("| Average time (s): {0}   |", avgTime);
            }
            else if (tmp == 3)
            {
                Console.WriteLine("| Average time (s): {0}  |", avgTime);
            }
            Console.WriteLine("|________________________|");

            tmp = avgSteps.ToString().Length;
            if (tmp == 1)
            {
                Console.WriteLine("| Average steps:    {0}    |", avgSteps);
            }
            else if (tmp == 2)
            {
                Console.WriteLine("| Average steps:    {0}   |", avgSteps);
            }
            else if (tmp == 3)
            {
                Console.WriteLine("| Average steps:    {0}  |", avgSteps);
            }
            Console.WriteLine("|________________________|");
        }

        static public void Tick(Object stateInfo)
        {
            curTime++;
            if (!showdet) Console.Write("\rTime elapsed: {0} seconds ({1} swaps)", curTime, curSteps);
        }
    }
}