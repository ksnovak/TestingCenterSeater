/* This tool is used to find the best seat for a student to take a test in. 
 * It will find the place furthest away from all other students, wherever possible.
 * 
 * If there are multiple options equi-distant from other students, we place the new student further down the row.
 * If there is a case where a student can be at the very beginning/end of the row, we prioritize that (as long as it doesn't put them any closer to another student).
 * 
 */


// Print out a visual representation of where the new student should be seated
String displaySeats(bool[] Seats, int OptimalSpot)
{
    //If we could not find a spot for the student, inform them as such
    if (OptimalSpot >= Seats.Length)
    {
        return "Apologies, there are no available seats, please wait for one to open up.";
    }

    else
    {
        String ArrangementDisplay = "The student shall sit in seat #" + (OptimalSpot+1) + "\n"; //Tell them the specific desk number; 1-based, not 0-based

        for (int i = 0; i < Seats.Length; i++)
        {
            //Display O for where the student should go, X where other students are, and a space for any other open spots
            String DisplayChar = (i == OptimalSpot) ? "O" : (Seats[i] ? "X" : " ");

            ArrangementDisplay += "[" + DisplayChar + "] ";
        }

        ArrangementDisplay += " -- X = seat taken, O = Optimal desk";

        return ArrangementDisplay;
    }
}

// Return the index of where the incoming student should be seated.
int findOptimalSpot(bool[] Seats)
{
    int potentialSpot = 0;  //As we go through the list of seats, store what we see as the currently-best selection
    double largestGap = 0;  //Paired with the currently-best selection, this is how many open spots there are per side.


    //Go through our list of seats
    for (int i = 0; i < Seats.Length; i++)
    {

        //If this current seat is taken, then we can just ignore it and move ahead.
        if (Seats[i] == true)
        {
            //If our current index matches our potential best spot (and it is already taken), then we need to move that up as well
            if (potentialSpot == i) 
                potentialSpot++;
        }
        
        //Otherwise, we have an open seat, so begin to test how ideal it is
        else
        {

            //Test to see if the current index is inside of a span we've already measured; skipping over it if so.
            if (largestGap == 0 || i > (potentialSpot + (int)(largestGap * 2))) { 


                int finalOpenSeat = i; //Use this to keep track of the index of the last *open* seat in the span.

                //Starting at 1 seat ahead, and going until we find another taken seat, count how many open seats there are.
                for (int j = i+1; j < Seats.Length; j++)
                {
                    //If this seat is taken, then we've finished our search; break away early
                    if (Seats[j] == true) 
                        break;


                    finalOpenSeat = j;  //If seat isn't taken, then update this counter.
                }


                //At this point, we know both the beginning and ending indices (i and finalOpenSeat, respectively) of an acceptable area.
                //So next we start testing it against our previous findings.


                //Two edge cases: Open range includes the beginning and/or end of the row
                //In most scenarios, we care about desks on either side; but this scenario ONLY ever cares about 1 side
                if (i == 0 || finalOpenSeat == Seats.Length - 1)
                {

                    double emptySpaceOnRelevantSide = (finalOpenSeat - i);

                    //If this gap is equal or greater to whatever our previous-best is, then update it.
                    if (emptySpaceOnRelevantSide >= largestGap) //>= rather than just > here means that it will prioritize placing students at the edge
                    {
                        potentialSpot = (i == 0) ? 0 : finalOpenSeat;   //Edge scenario: if the room is COMPLETELY open, we prioritize the left-most desk.
                        largestGap = emptySpaceOnRelevantSide;
                    }
                }

                //Otherwise, we are looking at a span of desks that have other desks on both the left and right of them.
                else
                {

                    int totalGap = (finalOpenSeat - i); //Figure out how big the total gap is (e.g. from index 3 to 5, will have 1 taken spot and 2 open spots; the gap is 2)
                    double gapOnEitherSide = (double)(totalGap)/ 2; //Cut the gap in half - because there must be both a left and a right side
                
                    //If the current gap is bigger than the best we've found so far, then pick it instead
                    //Due to double precision, we treat "n on one side and n+1 on the other" as being better than just "n on either side"
                    if (gapOnEitherSide >= largestGap)
                    {
                        //Edge case: If our previously-best choice is at the very start of the row (i.e. no neighbors to left),
                        //Then ONLY update to this new spot if it has more space (rather than just an equal amount)
                        if (potentialSpot != 0 || (gapOnEitherSide > largestGap))
                        {
                            //If all of the checks pass, we finally update our index and width
                            potentialSpot = i + (int)gapOnEitherSide; //Casting as int always rounds down
                            largestGap = gapOnEitherSide;
                        }
                    }
                }
            }
        }

    }
    return potentialSpot;
}

/* A list of different test cases and the logic behind what we expect the result to be.
 * case 0 is the one straight out of the prompt, and the rest are different cases I had thought to make sure were functional.
 * 
 */
bool[][] TestCases = new bool[][]
{
    //Default seating arrangement is based off of the offered prompt
    //[X] [X] [ ] [ ] [ ] [X] [ ] [X] [X] [ ] -- seat them into index 3
    new bool[] { true, true, false, false, false, true, false, true, true, false },

    new bool[] {false},     //A single seat - slot them right into it
    new bool[] {true},      //No open seats - tell them to wait
    new bool[] {false, false, false, false}, //Room is entirely open - sit them at the very beginning [0]
    new bool[] {true, false, false, false, false}, //One other student, already at the beginning - seat them at the very end [4]
    new bool[] {true, false, false, false, true}, //Students at both ends - seat them in the middle [2]
    new bool[] {true, false, false, true},  //An even amount of open seats - make sure the student isn't placed between a seat
    
    //EDGE SCENARIO: Placing student at index 0 or index 4 will both have the closest other student be 1 desk away
    //But placing them at index 0 means that *only* 1 student is near them; placing them at index 4 means that there's a student 1 desk away on *both* sides
    new bool[] {false, false, true, false, false, false, true},

    new bool[] {false, true, false}, //Three seats, with the middle taken - seat them at the end

    new bool[] {true, false, true, false, true, false, true, false}, //A bunch of staggered students, with the end available - place this student at the end of the row

    new bool[] {true, false, true, false, true, false, true} //A bunch of staggered students, with end NOT available - place them in any open spot
};

int desiredTestCase = 0;

int OptimalStudentSpot = findOptimalSpot(TestCases[desiredTestCase]);

Console.WriteLine("Welcome to the Testing Center Seater!!");
Console.WriteLine(displaySeats(TestCases[desiredTestCase], OptimalStudentSpot));