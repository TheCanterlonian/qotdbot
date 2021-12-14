//Question of the Day Bot by Tiffany Erika Darling
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace qotdbot
{
    //make this public so it can be called from outside
    public class Program
    {
        //creates a new instance of DiscordSocketClient
        private DiscordSocketClient _client = new DiscordSocketClient();
        //variable to be assigned a token at runtime
        public static string botToken = ("notYet");
        //main program starting method is also public
        public static void Main(string[] args)
        {
            //startup variable
            bool answerIsNotValid = true;
            //while the user has not given an answer yet
            while (answerIsNotValid)
            {
                //ask permision to startup
                Console.WriteLine(@"Start qotdbot?");
                Console.WriteLine("");
                bool startupTime = answerHandlerYesOrNo();
                //check if the user said to close the program
                if (startupTime == false)
                {
                    //close the program
                    Environment.Exit(499);
                }
                answerIsNotValid = false;
            }
            Console.WriteLine("starting qotdbot...");
            Console.WriteLine("");
            //check if there is not a ticket file already
            if (!(File.Exists("tickets.txt")))
            {
                Console.WriteLine("Creating tickets file...");
                //if not, make it
                File.Create("tickets.txt");
                Console.WriteLine("Tickets file created.");
                Console.WriteLine("");
            }
            //ask user for a token
            Console.WriteLine("qotdbot needs a token:");
            //puts the token in the holder
            botToken = Console.ReadLine();
            Console.Clear();
            //checks if the user wants to end here
            if (botToken == ("notYet"))
            {
                //exits the program
                Environment.Exit(498);
            }
            //checks if the user wants to read the token from a file
            if (botToken == ("read"))
            {
                //reads the token from the token file
                botToken = File.ReadAllText("token.txt");
            }
            //checks if the user wants to write to the token file
            if (botToken == ("write"))
            {
                Console.WriteLine("");
                //checks if the file already exists
                if (File.Exists("token.txt"))
                {
                    //asks the user to overwrite the file
                    Console.WriteLine("A token file already exists, overwrite?");
                    Console.WriteLine("");
                    bool doOverWriting = answerHandlerYesOrNo();
                    //if user says no
                    if (doOverWriting == false)
                    {
                        //exit the program
                        Console.WriteLine("exiting...");
                        Environment.Exit(497);
                    }
                    //asks the user for a token to write to the file
                    Console.WriteLine("");
                    Console.WriteLine("Enter a token to write to the file:");
                    Console.WriteLine("");
                    string tokenWrite = Console.ReadLine();
                    //write token into file (still untested)
                    File.WriteAllText("token.txt", tokenWrite);
                }
                else
                {
                    //creates a token file
                    File.Create("token.txt");
                    //asks the user for a token to write to the file
                    Console.WriteLine("");
                    Console.WriteLine("Enter a token to write to the file:");
                    Console.WriteLine("");
                    string tokenWrite = Console.ReadLine();
                    //write token into file  (still untested)
                    File.WriteAllText("token.txt", tokenWrite);
                }
            }
            //checks if the token is null
            if ((botToken == (null)) || (botToken == ("")))
            {
                //exits the program
                Environment.Exit(496);
            }
            Console.WriteLine("");
            Console.WriteLine("Logging in qotdbot...");
            Console.WriteLine("");
            //sets up to catch exceptions
            try
            {
                //run the async threading method which starts the bot
                new Program().MainAsync().GetAwaiter().GetResult();
            }
            //if an exception occurs
            catch (Exception errorOutput)
            {
                //let the user know
                Console.WriteLine("");
                Console.WriteLine("An error occured: ");
                Console.WriteLine("");
                Console.WriteLine(errorOutput);
                Console.WriteLine("");
                Console.WriteLine("End of Line.");
                Console.WriteLine("");
                Console.WriteLine("Quitting Program...");
                Console.WriteLine("Press any key to close.");
                Console.ReadKey();
                Environment.Exit(495);
            }
        }
        //async threading method
        public async Task MainAsync()
        {
            //hooks log event to log handler method
            _client.Log += Log;
            //logs the bot in to discord
            await _client.LoginAsync(TokenType.Bot, botToken);
            //start connection-reconnection logic
            await _client.StartAsync();
            //activates message receiver when a message is received
            _client.MessageReceived += MessageReceived;
            //activates the timer message sender
            await timerSender();
            //block the async main method from returning until after the application is exited
            await Task.Delay(-1);
        }
        //message receiver activates when a message is recieved
        private async Task MessageReceived(SocketMessage message)
        {
            //activates if the bot is pinged
            String mescon = message.Content;
            //makes the content a string
            mescon = mescon.ToString();
            //checks if it is a command for question of the day bot
            if (((mescon.StartsWith("!q")) == true) && (!(message.Author.IsBot)))
            {
                //lowers the case of the input
                mescon = mescon.ToLower();
                //command handlers go here
                if (mescon == ("!q ping"))
                {
                    await message.Channel.SendMessageAsync("pong");
                }
                //lists all current questions
                else if (mescon == ("!q list"))
                {
                    string messageToSend = ("listing all questions:");
                    await message.Channel.SendMessageAsync(messageToSend);
                    //grab all tickets
                    string ticketsList = File.ReadAllText("questionList.txt");
                    //check if the file is blank
                    if (string.IsNullOrEmpty(ticketsList))
                    {
                        ticketsList = ("no questions available, please add some soon");
                    }
                    //send results
                    await message.Channel.SendMessageAsync(ticketsList);
                }
                //creates a question
                else if (mescon.StartsWith("!q add "))
                {
                    //ignore leading length
                    int openOrCreate = 7;
                    //grab subject line for ticket
                    string ticksubj = mescon;
                    ticksubj = ticksubj.Remove(0, openOrCreate);
                    //check the number of tickets (lines) in the file
                    int newTicketNumber = File.ReadAllLines("questionList.txt").Length;
                    //assign the next number in  line to the new ticket
                    newTicketNumber = newTicketNumber + 1;
                    //create the leading string
                    string leadingZeroes = leadingZeroFinder(newTicketNumber);
                    //open stream to the tickets file and append to the end of it
                    StreamWriter ticketStream = new StreamWriter("questionList.txt", true);
                    //add the ticket to the file
                    ticketStream.WriteLine(leadingZeroes + newTicketNumber + ":" + "unsed-" + ticksubj);
                    //close the stream (always do this)
                    ticketStream.Close();
                    //show user that the ticket has been created
                    string confirmMsg = ("question added: #" + newTicketNumber);
                    await message.Channel.SendMessageAsync(confirmMsg);
                }
                //asks the first question in the list
                else if (mescon.StartsWith("!q asknow"))
                {
                    //find a question
                    string foundQuestion = findMessage();
                    //ask the question
                    await message.Channel.SendMessageAsync(foundQuestion);
                }
                //asks a chosen question
                else if (mescon.StartsWith("!q askthis "))
                {
                    //ignore the leading length
                    string whichToClose = mescon;
                    whichToClose = whichToClose.Remove(0, 11);
                    //turn the string into an integer
                    int numberToClose = (0);
                    bool result = int.TryParse(whichToClose, out numberToClose);
                    //if that fails...
                    if (!result)
                    {
                        //tell the user they fucked up
                        await message.Channel.SendMessageAsync("invalid question number");
                    }
                    //otherwise...
                    else if (result)
                    {
                        //read all file content into an array
                        string[] fullList = File.ReadAllLines("questionList.txt");
                        //also read it into a string
                        string fullString = File.ReadAllText("questionList.txt");
                        //get the leading zeroes value
                        string leadingZeroes = leadingZeroFinder(numberToClose);
                        //initialize the line variable
                        string lineSingle = string.Empty;
                        //create a string reader using the fullString as input
                        using (StringReader reader = new StringReader(fullString))
                        {
                            //this is a do loop, i never use these, see the condition below
                            do
                            {
                                //move the line into it's own variable alone
                                lineSingle = reader.ReadLine();
                                //only continue if the line is not null or empty
                                if ((lineSingle != null) && (lineSingle != ""))
                                {
                                    //only continue if the legitimate ticket number was used
                                    if ((lineSingle.StartsWith(whichToClose + ":")) || (lineSingle.StartsWith(leadingZeroes + whichToClose + ":")))
                                    {
                                        //check if the ticket is open still
                                        if (lineSingle.Contains("unsed"))
                                        {
                                            //replace the in-line determinant
                                            lineSingle = lineSingle.Replace("unsed", "asked");
                                            //make a zero-based integer to determine which position in the array to edit
                                            int integerToClose = numberToClose - 1;
                                            //edit the array element to contain the same value as the line
                                            fullList[integerToClose] = (lineSingle);
                                            //write the array to the file
                                            File.WriteAllLines("questionList.txt", fullList);
                                            //format the question
                                            string lineFormatted = lineSingle.Remove(0, 11);
                                            //ask the question
                                            await message.Channel.SendMessageAsync(lineFormatted);
                                        }
                                        //if the ticket is not open
                                        else
                                        {
                                            //tell the user about this folly
                                            await message.Channel.SendMessageAsync("question has already been asked");
                                        }
                                    }
                                    //if line can't be found
                                    else
                                    {
                                        //do nothing
                                    }
                                }
                                //if the line to edit is null or empty
                                else
                                {
                                    //do nothing
                                }
                            } while (lineSingle != null);
                            //above is the condition for the do loop, i hate these
                        }
                        //there's probably a bug somewhere in there, idk where if there is one though...
                    }
                }
                //clears out closed questions
                else if (mescon == ("!q clean"))
                {
                    //read file into an array
                    string[] fullFileArray = File.ReadAllLines("questionList.txt");
                    //send array to an array cleaner
                    string fullLinesString = cleaner(fullFileArray);
                    //write the string returned from the cleaner back to the file
                    File.WriteAllText("questionList.txt", fullLinesString);
                    await message.Channel.SendMessageAsync("question list cleaned");
                }
                //if no matching command is found
                else
                {
                    //let the user know they fucked up
                    await message.Channel.SendMessageAsync("invalid  or malformed command");
                }
            }
            //checks if it is a command for ticketbot
            if (((mescon.StartsWith("!ticket")) == true) && (!(message.Author.IsBot)))
            {
                //lowers the case of the input
                mescon = mescon.ToLower();
                //command handlers go here
                if (mescon == ("!ticket ping"))
                {
                    await message.Channel.SendMessageAsync("pong");
                }
                //lists all current tickets
                else if (mescon == ("!ticket list"))
                {
                    string messageToSend = ("listing all tickets:");
                    await message.Channel.SendMessageAsync(messageToSend);
                    //grab all tickets
                    string ticketsList = File.ReadAllText("tickets.txt");
                    //check if the file is blank
                    if (string.IsNullOrEmpty(ticketsList))
                    {
                        ticketsList = ("no tickets");
                    }
                    //send results
                    await message.Channel.SendMessageAsync(ticketsList);
                }
                //creates a ticket
                else if (mescon.StartsWith("!ticket open "))
                {
                    //ignore leading length
                    int openOrCreate = 15;
                    //find out if open or create was used
                    if (mescon.StartsWith("!ticket open "))
                    {
                        //if open was used make leading length larger
                        openOrCreate = 13;
                    }
                    //grab subject line for ticket
                    string ticksubj = mescon;
                    ticksubj = ticksubj.Remove(0, openOrCreate);
                    //check the number of tickets (lines) in the file
                    int newTicketNumber = File.ReadAllLines("tickets.txt").Length;
                    //assign the next number in  line to the new ticket
                    newTicketNumber = newTicketNumber + 1;
                    //create the leading string
                    string leadingZeroes = leadingZeroFinder(newTicketNumber);
                    //open stream to the tickets file and append to the end of it
                    StreamWriter ticketStream = new StreamWriter("tickets.txt", true);
                    //find the user that sent the message
                    var userObject = message.Author;
                    //grab just their name, nothing else
                    string userName = userObject.ToString();
                    //add the ticket to the file
                    ticketStream.WriteLine(leadingZeroes + newTicketNumber + ": " + "open - " + ticksubj + "  --  " + userName);
                    //close the stream (always do this)
                    ticketStream.Close();
                    //show user that the ticket has been created
                    string confirmMsg = ("ticket created: " + newTicketNumber + ": " + ticksubj);
                    await message.Channel.SendMessageAsync(confirmMsg);
                }
                //closes a ticket
                else if (mescon.StartsWith("!ticket close "))
                {
                    //ignore the leading length
                    string whichToClose = mescon;
                    whichToClose = whichToClose.Remove(0, 14);
                    //turn the string into an integer
                    int numberToClose = (0);
                    bool result = int.TryParse(whichToClose, out numberToClose);
                    //if that fails...
                    if (!result)
                    {
                        //tell the user they fucked up
                        await message.Channel.SendMessageAsync("invalid ticket integer");
                    }
                    //otherwise...
                    else if (result)
                    {
                        //read all file content into an array
                        string[] fullList = File.ReadAllLines("tickets.txt");
                        //also read it into a string
                        string fullString = File.ReadAllText("tickets.txt");
                        //get the leading zeroes value
                        string leadingZeroes = leadingZeroFinder(numberToClose);
                        //initialize the line variable
                        string lineSingle = string.Empty;
                        //create a string reader using the fullString as input
                        using (StringReader reader = new StringReader(fullString))
                        {
                            //this is a do loop, i never use these, see the condition below
                            do
                            {
                                //move the line into it's own variable alone
                                lineSingle = reader.ReadLine();
                                //only continue if the line is not null or empty
                                if ((lineSingle != null) && (lineSingle != ""))
                                {
                                    //only continue if the legitimate ticket number was used
                                    if ((lineSingle.StartsWith(whichToClose + ": ")) || (lineSingle.StartsWith(leadingZeroes + whichToClose + ": ")))
                                    {
                                        //check if the ticket is open still
                                        if (lineSingle.Contains("open"))
                                        {
                                            //replace the in-line determinant
                                            lineSingle = lineSingle.Replace("open", "closed");
                                            //make a zero-based integer to determine which position in the array to edit
                                            int integerToClose = numberToClose - 1;
                                            //edit the array element to contain the same value as the line
                                            fullList[integerToClose] = (lineSingle);
                                            //write the array to the file
                                            File.WriteAllLines("tickets.txt", fullList);
                                            //tell the user that the task has been completed
                                            await message.Channel.SendMessageAsync("ticket " + whichToClose + " has been closed");
                                        }
                                        //if the ticket is not open
                                        else
                                        {
                                            //tell the user about this folly
                                            await message.Channel.SendMessageAsync("ticket is not open");
                                        }
                                    }
                                    //if line can't be found
                                    else
                                    {
                                        //do nothing
                                    }
                                }
                                //if the line to edit is null or empty
                                else
                                {
                                    //do nothing
                                }
                            } while (lineSingle != null);
                            //above is the condition for the do loop, i hate these
                        }
                        //there's probably a bug somewhere in there, idk where if there is one though...
                    }
                }
                //clears out closed tickets
                else if (mescon == ("!ticket clean"))
                {
                    //read file into an array
                    string[] fullFile = File.ReadAllLines("tickets.txt");
                    //create a counter and set it to negative one
                    int counter = (-1);
                    //loop through all elements in the array
                    foreach (string lineItself in fullFile)
                    {
                        counter = counter + (1);
                        //only continue if the line is closed
                        if (lineItself.Contains(": closed - "))
                        {
                            //delete the contents of the line
                            fullFile[counter] = ("");
                        }
                        //otherwise
                        else
                        {
                            //do nothing
                        }
                    }
                    //put the array into a string
                    string fullLines = string.Join("\r\n", fullFile);
                    //regular expression i stole from the internet, i don't know how or why it works
                    fullLines = Regex.Replace(fullLines, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                    //write the string back to the file
                    File.WriteAllText("tickets.txt", fullLines);
                    await message.Channel.SendMessageAsync("ticket list cleaned");
                }
                //if no matching command is found
                else
                {
                    //let the user know they fucked up
                    await message.Channel.SendMessageAsync("invalid  or malformed command");
                }
            }
        }
        //log handler method
        private Task Log(LogMessage logmsg)
        {
            //writes log to the console
            Console.WriteLine(logmsg.ToString());
            //tells caller that the task was completed
            return Task.FromResult(1);
        }
        //console yes or no handler
        public static bool answerHandlerYesOrNo()
        {
            //returns true if yes, false if no
            //endlessly loops until an answer is given
            while (true)
            {
                //ask the user for input
                Console.WriteLine("Y/N:");
                Console.WriteLine("");
                //take in the answer and assign it into a single-character variable
                ConsoleKeyInfo userEntry = Console.ReadKey();
                Console.WriteLine("");
                //check to see if the answer is a no
                if ((userEntry.KeyChar == 'n') || (userEntry.KeyChar == 'N'))
                {
                    return false;
                }
                //check to see if the answer is a yes
                if ((userEntry.KeyChar == 'y') || (userEntry.KeyChar == 'Y'))
                {
                    return true;
                }
                //if neither is chosen
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine(@"Invalid option, please press 'Y' or 'N'.");
                    Console.WriteLine("");
                }
            }
        }
        //finds out how many leading zeroes to add, takes an int and returns a string
        public static string leadingZeroFinder(int nTN)
        {
            //assign leading zeros variable to nothing
            string leaderN = ("");
            //if new line number doesn't have four digits
            if (nTN < 1000)
            {
                //give it a leading zero
                leaderN = ("0");
            }
            //if new line number doesn't have three digits
            if (nTN < 100)
            {
                //give it two leading zeros
                leaderN = ("00");
            }
            //if it has fewer than two digits
            if (nTN < 10)
            {
                //give it three leading zeroes
                leaderN = ("000");
            }
            //return the leading zeroes
            return leaderN;
        }
        //finds the first question in the list of questions that hasn't been asked yet
        public static int nextQuestionFinder(string[] listOfQuestions)
        {
            //initialize a counter to keep track of where we are in the foreach loop
            int foreachCounter = 0;
            //for each question in the array, starting with string[0]
            foreach (string individualQuestion in listOfQuestions)
            {
                //check the question to see if it has not been asked yet
                if (individualQuestion.Contains("unsed"))
                {
                    //return the element number (starts from zero) corresponding to the current question being looked at
                    return foreachCounter;
                }
                //last thing to do in the foreach loop, advance the counter
                foreachCounter = foreachCounter + 1;
            }
            //ran out of unasked questions, return a negative
            return -1;
        }
        //cleans an array and turns it into a string
        public static string cleaner(string[] fullFile)
        {
            //create a counter and set it to negative one
            int counter = (-1);
            //loop through all elements in the array
            foreach (string lineItself in fullFile)
            {
                counter = counter + (1);
                //only continue if the line is closed
                if (lineItself.Contains(":asked-"))
                {
                    //delete the contents of the line
                    fullFile[counter] = ("");
                }
                //otherwise
                else
                {
                    //do nothing
                }
            }
            //put the array into a string
            string fullLines = string.Join("\r\n", fullFile);
            //regular expression i stole from the internet, i don't know how or why it works
            fullLines = Regex.Replace(fullLines, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
            //return the cleaned string
            return fullLines;
        }
        //timer message sender
        public async Task timerSender()
        {
            //assign the channel id
            ulong id = 302131706142654466;
            //set to go repeatedly
            while (true)
            {
                //set timer length to 24 hours (86400000 miliseconds is 24 hours, 60000 miliseconds is 1 minute for debugging purposes)
                Thread.Sleep(86400000);
                //no fucking clue why this stuff works
                var chnl = _client.GetChannel(id);
                var chnl2 = (IMessageChannel)chnl;
                //find a message to send to the channel now that the timer is up
                string timedMessageToSend = findMessage();
                //send updates to the channel
                await chnl2.SendMessageAsync(timedMessageToSend);
            }
        }
        //message finder finds the earliest unasked question and returns it as a message and marks it as asked in the question list
        public static string findMessage()
        {
            //read all file content into an array
            string[] fullList = File.ReadAllLines("questionList.txt");
            //also read it into a string
            string fullString = File.ReadAllText("questionList.txt");
            //find the first question in the list of questions that hasn't been asked yet
            int numberToAsk = nextQuestionFinder(fullList);
            //if the number to ask is negative, then we're all out of unasked questions
            if (numberToAsk < 0)
            {
                //clean the used questions away to make room for new questions
                string[] fullFileArrayneg = File.ReadAllLines("questionList.txt");
                string fullLinesStringneg = cleaner(fullFileArrayneg);
                File.WriteAllText("questionList.txt", fullLinesStringneg);
                //tell the user we ran out of questions
                await message.Channel.SendMessageAsync("there are no more questions left to ask");
            }
            else
            {
                //grab the line to ask
                string singleLineToAsk = fullList[numberToAsk];
                //mark the question as asked
                singleLineToAsk = singleLineToAsk.Replace("unsed", "asked");
                //put the question back in the array
                fullList[numberToAsk] = singleLineToAsk;
                //write the array back to the file
                File.WriteAllLines("questionList.txt", fullList);
                //format the question
                string formattedSingleLineToAsk = singleLineToAsk.Remove(0, 11);
                //return the question
                return formattedSingleLineToAsk;
            }
        }
    }
}

