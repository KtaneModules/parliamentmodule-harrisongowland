using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using KModkit;

public class ParliamentModule : MonoBehaviour
{
    public KMSelectable[] buttons;
    public KMBombModule bombModule;

    int correctIndex;
    bool isActivated = false;
    int bills = 0;
    int passed = 0;
    int pollNumber = 0;
    int currentPoll = 0;
    int numberOfPorts = 0;
    int litIndicators = 0;
    int numberOfBatteries = 0;
    string serialNumber = "";
    string indicatorText = "";
    int numberOfVowels = 0;
    int numberOfConsonants = 0;
    bool opposedLastBill = false;
    bool timeToResign = false;
    public enum Party { republican, democratic, conservative, liberal, socialist, communist, birthday, lan };
    public Party party;
    public enum BillOpener { prevent, promote, fund, endorse, condemn, oppose };
    public BillOpener billOpener;
    public enum BillMiddle { healthcare, support, hats, freedom, vaccines, rights }
    public BillMiddle billMiddle;
    public enum BillEnding { veterans, children, dogs, cats, waterfowl, liberals };
    public BillEnding billEnding;

    public MeshRenderer[] roundIndicators;
    public Material[] roundMateria;

    public TextMesh[] texts;
    public TextMesh[] finalButtons;

    bool finalStage = false;
    public int electionMethod = 0;

    public AudioSource ping;
    public AudioSource cheer;
    public AudioSource murmur;
    public AudioSource click;
    //Here's how we work out whether to support or oppose a bill

    void Start()
    {
        Init();
        bombModule = this.gameObject.GetComponent<KMBombModule>();
        GetComponent<KMBombModule>().OnActivate += ActivateModule;
        //pollGet will contain the serial number. But also serialNumber will contain the serial number? I forget why I did it this way.
        string pollGet = KMBombInfoExtensions.GetSerialNumber(FindObjectOfType<KMBombInfo>());
        //We need to know how many lit indicators there are for later.
        foreach (string s in KMBombInfoExtensions.GetOnIndicators(FindObjectOfType<KMBombInfo>()))
        {
            litIndicators++;
        }
        //This takes every character in every indicator and concatenates it into one string. This makes it easier for us to calculate how many vowels and consonants we're dealing with.
        foreach (string s in KMBombInfoExtensions.GetIndicators(FindObjectOfType<KMBombInfo>()))
        {
            indicatorText += s;
        }
        //This is when I assign the serial number to serialNumber for some reason idk
        serialNumber = pollGet;
        Debug.Log("Serial number: " + serialNumber);
        //If there are no indicators, we use the serial number for stage three.
        if (indicatorText.Length == 0)
        {
            indicatorText = serialNumber;
            indicatorText = Regex.Replace(indicatorText, @"[\d-]", "");
        }
        //We need to know how many vowels and consonants are in this resulting string that we've created for stage three.
        foreach (char c in indicatorText)
        {
            if (c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U')
            {
                numberOfVowels++;
            }
            else
            {
                numberOfConsonants++;
            }
        }
        //We also need to know how many ports there are but that's fine 
        foreach (string p in KMBombInfoExtensions.GetPorts(FindObjectOfType<KMBombInfo>()))
        {
            Debug.Log(p);
            numberOfPorts++;
        }
        //Ditto the number of batteries
        numberOfBatteries = KMBombInfoExtensions.GetBatteryCount(FindObjectOfType<KMBombInfo>());
        Debug.Log("Return number string: " + serialNumber + " - " + ReturnNumberString(serialNumber));
        if (ReturnNumberString(pollGet) == "")
        {
            //The serial number has no digits. Odd.
            pollNumber = 0;
        }
        else
        {
            string poll = ReturnNumberString(pollGet);
            string target = "";
            if (poll.Length > 1)
            {
                target += poll[0];
                target += poll[1];
            }
            else
            {
                target = poll;
            }
            pollNumber = int.Parse(ReturnNumberString(target));
        }
        currentPoll = pollNumber;
        while (currentPoll == pollNumber)
        {
            currentPoll = Random.Range(0, 101);
        }
        GenerateNewBill();
        //After between 3 and 5 bills, the poll number will become the serial number's first two digits. This is when its time to resign.
    }

    public void UpdateSquares()
    {
        int index = 0;
        bool complete = false;
        foreach (MeshRenderer r in roundIndicators)
        {
            if (index == passed)
            {
                complete = true;
            }
            if (complete)
            {
                r.material = roundMateria[0];
            }
            else
            {
                r.material = roundMateria[1];
            }
            index++;
        }
        if (passed == 3)
        {
            passed = 0;
        }
    }

    void GenerateNewBill()
    {
        ChangeParty();
        ChangeBill();
        RandomiseModule();
        UpdateSquares();
        if (!timeToResign)
        {
            texts[0].text = "A Bill To " + billOpener.ToString() + " " + billMiddle.ToString() + "\nfor " + billEnding.ToString();
            texts[0].fontSize = 50; 
        }
        else
        {
            texts[0].text = "Election Day";
            texts[0].fontSize = 110;
            texts[0].color = Color.green;
        }
        texts[1].text = party.ToString() + " Party";
        texts[2].text = currentPoll.ToString() + "%";
    }

    void ChangeBill()
    {
        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, 6);
            switch (i)
            {
                case 0:
                    //This is the bill beginning.
                    switch (index)
                    {
                        case 0:
                            billOpener = BillOpener.condemn;
                            break;
                        case 1:
                            billOpener = BillOpener.endorse;
                            break;
                        case 2:
                            billOpener = BillOpener.fund;
                            break;
                        case 3:
                            billOpener = BillOpener.oppose;
                            break;
                        case 4:
                            billOpener = BillOpener.prevent;
                            break;
                        case 5:
                            billOpener = BillOpener.promote;
                            break;
                    }
                    break;
                case 1:
                    //This is the bill middle.
                    switch (index)
                    {
                        case 0:
                            billMiddle = BillMiddle.freedom;
                            break;
                        case 1:
                            billMiddle = BillMiddle.hats;
                            break;
                        case 2:
                            billMiddle = BillMiddle.healthcare;
                            break;
                        case 3:
                            billMiddle = BillMiddle.rights;
                            break;
                        case 4:
                            billMiddle = BillMiddle.support;
                            break;
                        case 5:
                            billMiddle = BillMiddle.vaccines;
                            break;
                    }
                    break;
                case 2:
                    //This is the bill ending.
                    switch (index)
                    {
                        case 0:
                            billEnding = BillEnding.cats;
                            break;
                        case 1:
                            billEnding = BillEnding.children;
                            break;
                        case 2:
                            billEnding = BillEnding.dogs;
                            break;
                        case 3:
                            billEnding = BillEnding.liberals;
                            break;
                        case 4:
                            billEnding = BillEnding.veterans;
                            break;
                        case 5:
                            billEnding = BillEnding.waterfowl;
                            break;
                    }
                    break;
            }
        }
    }

    void ChangeParty()
    {
        int partyToChoose = Random.Range(0, 8);
        switch (partyToChoose)
        {
            case 0:
                party = Party.birthday;
                break;
            case 1:
                party = Party.communist;
                break;
            case 2:
                party = Party.conservative;
                break;
            case 3:
                party = Party.democratic;
                break;
            case 4:
                party = Party.lan;
                break;
            case 5:
                party = Party.liberal;
                break;
            case 6:
                party = Party.republican;
                break;
            case 7:
                party = Party.socialist;
                break;
        }
    }

    string ReturnNumberString(string input)
    {
        string value = Regex.Replace(input, "[A-Za-z ]", "");
        return value;
    }

    void Init()
    {
        buttons[0].OnInteract += delegate { OnPress(0); return false; };
        buttons[2].OnInteract += delegate { OnPress(1); return false; };
        buttons[1].OnInteract += delegate { OnPress(2); return false; };
        buttons[3].OnInteract += delegate { OnPress(3); return false; };
    }

    void ActivateModule()
    {
        isActivated = true;
    }

    void RandomiseModule()
    {
        if (timeToResign)
        {
            //The player messed up.
            timeToResign = false;
        }
        else if (passed == 3)
        {
            timeToResign = true;
        }
        if (timeToResign)
        {
            currentPoll = Random.Range(1, 101);
        }
        else
        {
            currentPoll = Random.Range(0, 101);
        }
    }

    void OnPress(int index)
    {
        click.Play();
        Debug.Log("pressed: " + index);
        switch (index)
        {
            case 0:
                //This is the support button.
                if (timeToResign)
                {
                    //You should not still be voting.

                    opposedLastBill = false;
                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
                else
                {
                    if (currentPoll <= 17)
                    {
                        //Weak
                        passed++;
                        opposedLastBill = false;
                        ping.Play();
                        GenerateNewBill(); 
                    }
                    else if (party == Party.republican && billOpener == BillOpener.oppose)
                    {
                        //Yes, this is a metaphor.
                        passed++;
                        opposedLastBill = false;
                        ping.Play();
                        GenerateNewBill();
                    }
                    else if (billOpener == BillOpener.fund && (billMiddle == BillMiddle.healthcare || billMiddle == BillMiddle.vaccines))
                    {
                        if ((party == Party.socialist || party == Party.communist || party == Party.liberal || party == Party.birthday))
                        {
                            //Yes this is a metaphor
                            passed++;
                            opposedLastBill = false;
                            ping.Play();
                            GenerateNewBill();
                        }
                        else
                        {
                            bombModule.HandleStrike();
                            opposedLastBill = false;
                            GenerateNewBill();
                        }
                    }
                    else if (billMiddle == BillMiddle.hats && numberOfPorts > 2)
                    {
                        if (currentPoll > 51)
                        {
                            passed++;
                            opposedLastBill = false;
                            ping.Play();
                            GenerateNewBill();
                        }
                        else
                        {
                            bombModule.HandleStrike();
                            opposedLastBill = false;
                            GenerateNewBill();
                        }
                    }
                    else if (billOpener == BillOpener.condemn)
                    {
                        if ((currentPoll > 60 || numberOfPorts == 0))
                        {
                            //Support this
                            passed++;
                            ping.Play();
                            opposedLastBill = false;
                            GenerateNewBill();
                        }
                        else
                        {
                            //Do not
                            bombModule.HandleStrike();
                            opposedLastBill = false;
                            GenerateNewBill();
                        }
                    }
                    else if (billEnding == BillEnding.cats)
                    {
                        ProcessCats(0);
                    }
                    else if ((billOpener == BillOpener.oppose || billOpener == BillOpener.prevent) && billEnding == BillEnding.waterfowl)
                    {
                        passed++;
                        ping.Play();
                        opposedLastBill = false;
                        GenerateNewBill();
                    }
                    else if (billOpener == BillOpener.endorse && billMiddle == BillMiddle.freedom)
                    {
                        if (numberOfBatteries > 2)
                        {
                            //You are correct to support this. 
                            passed++;
                            ping.Play();
                            opposedLastBill = false;
                            GenerateNewBill();
                        }
                        else
                        {
                            //You should be supporting this.
                            bombModule.HandleStrike();
                            opposedLastBill = false;
                            GenerateNewBill();
                        }
                    }
                    else
                    {
                        //Now the nebulous guff
                        ProcessAppendix2(0);
                    }
                }
                break;
            case 1:
                //This is the oppose button. 
                if (timeToResign)
                {
                    //You should not still be voting.
                    bombModule.HandleStrike();
                    opposedLastBill = true;
                    GenerateNewBill();
                }
                else
                {
                    if (currentPoll <= 17)
                    {
                        //Weak
                        bombModule.HandleStrike();
                        opposedLastBill = true;
                        GenerateNewBill();
                    }
                    else if (party == Party.republican && billOpener == BillOpener.oppose)
                    {
                        //Yes, this is a metaphor.
                        bombModule.HandleStrike();
                        opposedLastBill = true;
                        GenerateNewBill();
                    }
                    else if (billOpener == BillOpener.fund && (billMiddle == BillMiddle.healthcare || billMiddle == BillMiddle.vaccines))
                    {
                        if ((party == Party.socialist || party == Party.communist || party == Party.liberal || party == Party.birthday))
                        {
                            //Yes this is a metaphor
                            bombModule.HandleStrike();
                            opposedLastBill = true;
                            GenerateNewBill();
                        }
                        else
                        {
                            passed++;
                            opposedLastBill = true;
                            ping.Play();
                            GenerateNewBill();
                        }
                    }
                    else if (billMiddle == BillMiddle.hats && numberOfPorts > 2)
                    {
                        if (currentPoll > 51)
                        {
                            bombModule.HandleStrike();
                            opposedLastBill = true;
                            GenerateNewBill();
                        }
                        else
                        {
                            passed++;
                            opposedLastBill = true;
                            ping.Play();
                            GenerateNewBill();
                        }
                    }
                    else if (billOpener == BillOpener.condemn)
                    {
                        if ((currentPoll > 60 || numberOfPorts == 0))
                        {
                            //Support this
                            bombModule.HandleStrike();
                            opposedLastBill = true;
                            GenerateNewBill();
                        }
                        else
                        {
                            //Do not
                            opposedLastBill = true;
                            passed++;
                            ping.Play();
                            GenerateNewBill();
                        }
                    }
                    else if (billEnding == BillEnding.cats)
                    {
                        ProcessCats(1);
                    }
                    else if ((billOpener == BillOpener.oppose || billOpener == BillOpener.prevent) && billEnding == BillEnding.waterfowl)
                    {
                        bombModule.HandleStrike();
                        opposedLastBill = true;
                        GenerateNewBill();
                    }
                    else if (billOpener == BillOpener.endorse && billMiddle == BillMiddle.freedom)
                    {
                        if (numberOfBatteries > 2)
                        {
                            //You should be voting for this. 
                            bombModule.HandleStrike();
                            opposedLastBill = true;
                            GenerateNewBill();
                        }
                        else
                        {
                            //You are correct to oppose this.
                            passed++;
                            ping.Play();
                            opposedLastBill = true;
                            GenerateNewBill();
                        }
                    }
                    else
                    {
                        ProcessAppendix2(1);
                    }
                }
                break;
            case 2:
                //This is the FPTP/win button.
                if (finalStage)
                {
                    if (electionMethod == 0)
                    {
                        //FPTP:
                        if (((numberOfBatteries + numberOfPorts) * 10) > currentPoll)
                        {
                            //You won!
                            cheer.Play();
                            bombModule.HandlePass();
                        }
                        else
                        {
                            //Wait but you lost tho
                            bombModule.HandleStrike();
                            finalStage = false;
                            finalButtons[0].text = "FPTP";
                            finalButtons[1].text = "MMP";
                            GenerateNewBill();
                        }
                    }
                    else
                    {
                        if (currentPoll >= 40 && !(party == Party.birthday || party == Party.lan))
                        {
                            //You won!
                            cheer.Play();
                            bombModule.HandlePass();
                        }
                        else
                        {
                            //Wait but you lost tho
                            bombModule.HandleStrike();
                            finalStage = false;
                            finalButtons[0].text = "FPTP";
                            finalButtons[1].text = "MMP";
                            GenerateNewBill();
                        }
                    }
                }
                else
                {
                    if (timeToResign)
                    {
                        //Calculate whether this is right
                        if (numberOfVowels >= (numberOfConsonants - 2))
                        {
                            //If the number of vowels is greater than or equal to the number of consonants - 2:
                            if (litIndicators % 2 != 0)
                            {
                                if ((party == Party.socialist || party == Party.lan || party == Party.birthday || party == Party.conservative))
                                {
                                    //You should be using MMP. 
                                    bombModule.HandleStrike();
                                    GenerateNewBill();
                                }
                                else
                                {
                                    //You're correctly using FPTP.
                                    ping.Play();
                                    finalStage = true;
                                    finalButtons[0].text = "Win";
                                    finalButtons[1].text = "Lose";
                                }
                            }
                            else
                            {
                                if (party == Party.liberal || party == Party.communist || party == Party.socialist)
                                {
                                    //You're correctly using FPTP. 
                                    ping.Play();
                                    finalStage = true;
                                    finalButtons[0].text = "Win";
                                    finalButtons[1].text = "Lose";
                                }
                                else
                                {
                                    //You should be using MMP.
                                    bombModule.HandleStrike();
                                    GenerateNewBill();
                                }
                            }
                        }
                        else
                        {
                            if (litIndicators % 2 == 0)
                            {
                                //You should be using MMP. 
                                bombModule.HandleStrike();
                                GenerateNewBill();
                            }
                            else
                            {
                                //You are correctly using FPTP. 
                                ping.Play();
                                finalStage = true;
                                finalButtons[0].text = "Win";
                                finalButtons[1].text = "Lose";
                            }
                        }
                    }
                    else
                    {
                        //You should not be resigning.
                        bombModule.HandleStrike();
                        GenerateNewBill();
                    }
                }
                break;
            case 3:
                //This is the MMP/lose button.
                if (finalStage)
                {
                    if (electionMethod == 0)
                    {
                        //FPTP:
                        if (((numberOfBatteries + numberOfPorts) * 10) > currentPoll)
                        {
                            //Wait but you won tho
                            bombModule.HandleStrike();
                            finalStage = false;
                            finalButtons[0].text = "FPTP";
                            finalButtons[1].text = "MMP";
                            GenerateNewBill();
                        }
                        else
                        {
                            //Lol sucks to be u
                            murmur.Play();
                            bombModule.HandlePass();
                        }
                    }
                    else
                    {
                        if (currentPoll >= 40 && !(party == Party.birthday || party == Party.lan))
                        {
                            //But you won tho
                            bombModule.HandleStrike();
                            finalStage = false;
                            finalButtons[0].text = "FPTP";
                            finalButtons[1].text = "MMP";
                            GenerateNewBill();
                        }
                        else
                        {
                            //Right, you did lose
                            murmur.Play();
                            bombModule.HandlePass();
                        }
                    }
                }
                else
                {
                    if (timeToResign)
                    {
                        //Calculate whether this is right
                        if (numberOfVowels >= (numberOfConsonants - 2))
                        {
                            //If the number of vowels is greater than or equal to the number of consonants - 2:
                            if (litIndicators % 2 != 0)
                            {
                                if ((party == Party.socialist || party == Party.lan || party == Party.birthday || party == Party.conservative))
                                {
                                    //You are correctly using MMP. 
                                    ping.Play();
                                    finalStage = true;
                                    electionMethod = 1; 
                                    finalButtons[0].text = "Win";
                                    finalButtons[1].text = "Lose";
                                }
                                else
                                {
                                    //You should be using FPTP.
                                    bombModule.HandleStrike();
                                    GenerateNewBill();
                                    ping.Play();
                                }
                            }
                            else
                            {
                                if (party == Party.liberal || party == Party.communist || party == Party.socialist)
                                {
                                    //You should be using FPTP. 
                                    bombModule.HandleStrike();
                                    GenerateNewBill();
                                    ping.Play();
                                }
                                else
                                {
                                    //You're correctly using MMP.
                                    ping.Play();
                                    electionMethod = 1; 
                                    finalStage = true;
                                    finalButtons[0].text = "Win";
                                    finalButtons[1].text = "Lose";
                                }
                            }
                        }
                        else
                        {
                            if (litIndicators % 2 == 0)
                            {
                                //You are correctly using MMP.
                                ping.Play();
                                electionMethod = 1; 
                                finalStage = true;
                                finalButtons[0].text = "Win";
                                finalButtons[1].text = "Lose";
                            }
                            else
                            {
                                //You should be using FPTP. 
                                bombModule.HandleStrike();
                                GenerateNewBill();
                            }
                        }
                    }
                    else
                    {
                        //You should not be resigning.
                        bombModule.HandleStrike();
                        GenerateNewBill();
                    }
                }

                break;
        }
    }

    void RunFinalStage()
    {

    }

    void ProcessAppendix2(int support)
    {
        int letterPosition = 0;
        //Step one: calculate letter.
        switch (billOpener)
        {
            case BillOpener.prevent:
                switch (billEnding)
                {
                    case BillEnding.veterans:
                        letterPosition = 0;
                        break;
                    case BillEnding.children:
                        letterPosition = 1;
                        break;
                    case BillEnding.dogs:
                        letterPosition = 2;
                        break;
                    case BillEnding.waterfowl:
                        letterPosition = 3;
                        break;
                    case BillEnding.liberals:
                        letterPosition = 4;
                        break;
                }
                break;
            case BillOpener.promote:
                switch (billEnding)
                {
                    case BillEnding.veterans:
                        letterPosition = 5;
                        break;
                    case BillEnding.children:
                        letterPosition = 6;
                        break;
                    case BillEnding.dogs:
                        letterPosition = 7;
                        break;
                    case BillEnding.waterfowl:
                        letterPosition = 8;
                        break;
                    case BillEnding.liberals:
                        letterPosition = 9;
                        break;
                }
                break;
            case BillOpener.fund:
                switch (billEnding)
                {
                    case BillEnding.veterans:
                        letterPosition = 10;
                        break;
                    case BillEnding.children:
                        letterPosition = 11;
                        break;
                    case BillEnding.dogs:
                        letterPosition = 12;
                        break;
                    case BillEnding.waterfowl:
                        letterPosition = 13;
                        break;
                    case BillEnding.liberals:
                        letterPosition = 14;
                        break;
                }
                break;
            case BillOpener.endorse:
                switch (billEnding)
                {
                    case BillEnding.veterans:
                        letterPosition = 15;
                        break;
                    case BillEnding.children:
                        letterPosition = 16;
                        break;
                    case BillEnding.dogs:
                        letterPosition = 17;
                        break;
                    case BillEnding.waterfowl:
                        letterPosition = 18;
                        break;
                    case BillEnding.liberals:
                        letterPosition = 19;
                        break;
                }
                break;
            case BillOpener.oppose:
                switch (billEnding)
                {
                    case BillEnding.veterans:
                        letterPosition = 20;
                        break;
                    case BillEnding.children:
                        letterPosition = 21;
                        break;
                    case BillEnding.dogs:
                        letterPosition = 22;
                        break;
                    case BillEnding.waterfowl:
                        letterPosition = 23;
                        break;
                    case BillEnding.liberals:
                        letterPosition = 24;
                        break;
                }
                break;
        }
        //Step two: mod that position. 
        switch (party)
        {
            case Party.republican:
                switch (billMiddle)
                {
                    case BillMiddle.healthcare:
                        letterPosition += 2;
                        break;
                    case BillMiddle.support:
                        letterPosition += 1;
                        break;
                    case BillMiddle.hats:
                        break;
                    case BillMiddle.freedom:
                        letterPosition -= 1;
                        break;
                    case BillMiddle.vaccines:
                        letterPosition -= 3;
                        break;
                    case BillMiddle.rights:
                        letterPosition += 5;
                        break;
                }
                break;
            case Party.democratic:
                switch (billMiddle)
                {
                    case BillMiddle.healthcare:
                        letterPosition += 4;
                        break;
                    case BillMiddle.support:
                        letterPosition += 3;
                        break;
                    case BillMiddle.hats:
                        letterPosition += 2;
                        break;
                    case BillMiddle.freedom:
                        letterPosition -= 3;
                        break;
                    case BillMiddle.vaccines:
                        letterPosition += 3;
                        break;
                    case BillMiddle.rights:
                        break;
                }
                break;
            case Party.conservative:
                switch (billMiddle)
                {
                    case BillMiddle.healthcare:
                        letterPosition += 6;
                        break;
                    case BillMiddle.support:
                        letterPosition += 5;
                        break;
                    case BillMiddle.hats:
                        letterPosition += 4;
                        break;
                    case BillMiddle.freedom:
                        letterPosition -= 5;
                        break;
                    case BillMiddle.vaccines:
                        letterPosition -= 2;
                        break;
                    case BillMiddle.rights:
                        letterPosition += 4;
                        break;
                }
                break;
            case Party.liberal:
                switch (billMiddle)
                {
                    case BillMiddle.healthcare:
                        letterPosition += 8;
                        break;
                    case BillMiddle.support:
                        letterPosition += 7;
                        break;
                    case BillMiddle.hats:
                        letterPosition += 6;
                        break;
                    case BillMiddle.freedom:
                        letterPosition -= 7;
                        break;
                    case BillMiddle.vaccines:
                        letterPosition += 2;
                        break;
                    case BillMiddle.rights:
                        letterPosition += 1;
                        break;
                }
                break;
            case Party.socialist:
                switch (billMiddle)
                {
                    case BillMiddle.healthcare:
                        letterPosition += 1;
                        break;
                    case BillMiddle.support:
                        letterPosition += 2;
                        break;
                    case BillMiddle.hats:
                        letterPosition += 1;
                        break;
                    case BillMiddle.freedom:
                        break;
                    case BillMiddle.vaccines:
                        letterPosition -= 1;
                        break;
                    case BillMiddle.rights:
                        letterPosition += 4;
                        break;
                }
                break;
            case Party.communist:
                switch (billMiddle)
                {
                    case BillMiddle.healthcare:
                        letterPosition += 3;
                        break;
                    case BillMiddle.support:
                        letterPosition += 4;
                        break;
                    case BillMiddle.hats:
                        letterPosition += 3;
                        break;
                    case BillMiddle.freedom:
                        letterPosition -= 3;
                        break;
                    case BillMiddle.vaccines:
                        letterPosition -= 2;
                        break;
                    case BillMiddle.rights:
                        letterPosition += 3;
                        break;
                }
                break;
            case Party.birthday:
                switch (billMiddle)
                {
                    case BillMiddle.healthcare:
                        letterPosition += 5;
                        break;
                    case BillMiddle.support:
                        letterPosition += 6;
                        break;
                    case BillMiddle.hats:
                        letterPosition += 6;
                        break;
                    case BillMiddle.freedom:
                        letterPosition -= 2;
                        break;
                    case BillMiddle.vaccines:
                        letterPosition -= 1;
                        break;
                    case BillMiddle.rights:
                        letterPosition += 2;
                        break;
                }
                break;
            case Party.lan:
                switch (billMiddle)
                {
                    case BillMiddle.healthcare:
                        letterPosition += 7;
                        break;
                    case BillMiddle.support:
                        letterPosition += 8;
                        break;
                    case BillMiddle.hats:
                        letterPosition += 4;
                        break;
                    case BillMiddle.freedom:
                        letterPosition -= 4;
                        break;
                    case BillMiddle.vaccines:
                        letterPosition -= 2;
                        break;
                    case BillMiddle.rights:
                        letterPosition += 1;
                        break;
                }
                break;
        }
        //Step three: adjust the calculation if it's below 0 or above 25.
        if (letterPosition < 0)
        {
            //Subtract the result from 26. -1 from 0, for instance, is Z.
            letterPosition = 26 - letterPosition;
        }
        else if (letterPosition > 25)
        {
            //Subtract 26 from the result. 0, for instance, is A.
            letterPosition = letterPosition - 26;
        }
        Debug.Log("Letter position: " + letterPosition);
        //We now have a number from 0 to 25.
        //Now we can use this number to work out what rule to apply.
        if (letterPosition < 5)
        {
            //A-E.
            if (support == 0)
            {
                //Voted to support.
                passed++;
                opposedLastBill = false;
                ping.Play();
                GenerateNewBill();
            }
            else
            {
                //Voted against.

                opposedLastBill = true;
                bombModule.HandleStrike();
                GenerateNewBill();
            }
        }
        else if (letterPosition >= 5 && letterPosition < 10)
        {
            if (numberOfBatteries >= numberOfPorts)
            {
                if (support == 0)
                {
                    //Voted to support.
                    passed++;
                    opposedLastBill = false;
                    ping.Play();

                    GenerateNewBill();
                }
                else
                {
                    //Voted against.
                    opposedLastBill = true;
                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
            }
            else
            {
                if (support == 0)
                {
                    opposedLastBill = false;
                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
                else
                {
                    passed++;
                    opposedLastBill = true;

                    ping.Play();
                    GenerateNewBill();
                }
            }
        }
        else if (letterPosition >= 10 && letterPosition < 15)
        {
            if (serialNumber.Contains("V") || serialNumber.Contains("O") || serialNumber.Contains("T") || serialNumber.Contains("E"))
            {
                if (support == 0)
                {
                    //Voted to support.
                    opposedLastBill = false;

                    passed++;
                    ping.Play();
                    GenerateNewBill();
                }
                else
                {
                    //Voted against.

                    opposedLastBill = true;
                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
            }
            else
            {
                if (support == 0)
                {
                    //Voted to support.
                    opposedLastBill = false;

                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
                else
                {
                    //Voted against.

                    opposedLastBill = true;
                    ping.Play();
                    passed++;
                    GenerateNewBill();
                }
            }
        }
        else if (letterPosition >= 15 && letterPosition < 20)
        {
            if (support == 0)
            {
                //Voted to support.
                opposedLastBill = false;

                bombModule.HandleStrike();
                GenerateNewBill();
            }
            else
            {
                //Voted against.

                opposedLastBill = true;
                ping.Play();
                passed++;
                GenerateNewBill();
            }
        }
        else
        {
            if (opposedLastBill)
            {
                if (support == 0)
                {
                    //Voted to support.
                    opposedLastBill = false;

                    passed++;
                    ping.Play();
                    GenerateNewBill();
                }
                else
                {
                    //Voted against.
                    opposedLastBill = true;
                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
            }
            else
            {
                if (support == 0)
                {
                    //Voted to support.
                    opposedLastBill = false;

                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
                else
                {
                    //Voted against.

                    opposedLastBill = true;
                    ping.Play();
                    passed++;
                    GenerateNewBill();
                }
            }
        }
    }

    void ProcessCats(int support)
    {
        if (serialNumber.Contains("C"))
        {
            if (support == 0)
            {
                //Voted to support.
                opposedLastBill = false;

                ping.Play();
                passed++;
                GenerateNewBill();
            }
            else
            {
                //Voted against.

                opposedLastBill = true;
                bombModule.HandleStrike();
                GenerateNewBill();
            }
        }
        else if (serialNumber.Contains("A"))
        {
            if (support == 0)
            {
                //Voted to support.
                opposedLastBill = false;

                bombModule.HandleStrike();
                GenerateNewBill();
            }
            else
            {
                //Voted against.
                opposedLastBill = true;
                ping.Play();
                passed++;
                GenerateNewBill();
            }
        }
        else if (serialNumber.Contains("T"))
        {
            if (numberOfBatteries % 2 == 0)
            {
                //There are an even number of batteries.
                if (support == 0)
                {
                    //Voted to support.
                    opposedLastBill = false;

                    ping.Play();
                    passed++;
                    GenerateNewBill();
                }
                else
                {
                    //Voted against.
                    opposedLastBill = true;
                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
            }
            else
            {
                if (support == 0)
                {
                    //Voted to support.
                    opposedLastBill = false;
                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
                else
                {
                    //Voted against.

                    opposedLastBill = true;
                    passed++;
                    ping.Play();
                    GenerateNewBill();
                }
            }
        }
        else if (serialNumber.Contains("S"))
        {
            if (support == 0)
            {
                //Voted to support.
                opposedLastBill = false;

                ping.Play();
                passed++;
                GenerateNewBill();
            }
            else
            {
                //Voted against.
                opposedLastBill = true;

                bombModule.HandleStrike();
                GenerateNewBill();
            }
        }
        else
        {
            if (numberOfPorts % 2 == 0)
            {
                if (support == 0)
                {
                    //Voted to support.
                    opposedLastBill = false;

                    ping.Play();
                    passed++;
                    GenerateNewBill();
                }
                else
                {
                    //Voted against.
                    opposedLastBill = true;

                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
            }
            else
            {
                if (support == 0)
                {
                    //Voted to support.
                    opposedLastBill = false;

                    bombModule.HandleStrike();
                    GenerateNewBill();
                }
                else
                {
                    //Voted against.

                    ping.Play();
                    opposedLastBill = true;
                    passed++;
                    GenerateNewBill();
                }
            }
        }
    }
}
