using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// architecture:
// Client controls credits and commercials (+ view count). Don't care about cheaters yet. Big win for simplicity.
// Client controls level/rank: Increases as you view comm (carrot) and win money.
// Client keeps track of your winnings per donee.

// Stateless requests on any GamePlayServer.
//    CreateUser(some platform info) (if not found on client).
//    RequestSequence(user, ) (ask MoneyServer for clearance).
//    RequestWin(user, donee, ) (Simple check, or skip entirely in v1. Send update to MoneyServer.
//    Tables: User. (id) Could skip in theory, but I want it to keep track of activity :-)

// MoneyServer
//    GetGlobalStats() (total wins per donee and summed).
//    Real-time view of available prices/balance. ACID. Deny requests if nothing available.
//    Stop before 0 to account for client delay.
//    Transfers from bank to donee balance and logs win-details.
// Tables:
//    bank (1kr, 10kr, 100kr, 1000kr, etc.)
//    donees_paid (Røde Kors, Kræftens..., etc.).
//    donees_pending (waiting for worthwhile amount to make payment).
//    log (noSql). All actions.

// den en-armede velgører. Spillet, hvor ingen taber. Komma?
// make things easier:
// private donations always 1kr (evt. skriv anonym i kommentar for anon).
// Reklame penge -> større præmier.
// flow: sats på kredits skal gå ca. lige op. Stadig reklamr ind imellem. Sats på penge giver reklame når kredit = 0 (+ x).
// Just cut any wins when money has run out (money server).
namespace Assets.Script
{
    // Probabilities. Easier to keep probalities per win combination? Use that to infer the random
    // icons passing by? Calc potential win as a win. It's not that hard to get them all.
    class Provider
    {
        //public AAA = 0.1, bbb = 0.05, etc.
    }
}
