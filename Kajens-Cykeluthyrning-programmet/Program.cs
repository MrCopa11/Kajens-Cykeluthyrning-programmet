using System;
using System.Collections.Generic;

class Program
{
    static List<Fordon> fordonLista = new List<Fordon>();
    static List<Kund> kundLista = new List<Kund>();

    // Håll koll på vem som hyrt vilket fordon (Fordonsnummer -> Kund-objekt och antal timmar)
    static Dictionary<int, (Kund kund, int timmar)> uthyrningar = new Dictionary<int, (Kund, int)>();

    static void Main(string[] args)
    {
        // Startdata: minst två av varje typ
        fordonLista.Add(new Cykel(1));
        fordonLista.Add(new Cykel(2));
        fordonLista.Add(new Elcykel(3, 80));
        fordonLista.Add(new Elcykel(4, 25));

        bool körProgram = true;
        while (körProgram)
        {
            Console.WriteLine("\n=== Kajens Cykeluthyrning ===");
            Console.WriteLine("1. Visa fordon  2. Registrera kund  3. Hyr ut");
            Console.WriteLine("4. Lämna tillbaka  5. Ladda  6. Avsluta");
            Console.Write("Val: ");

            string val = Console.ReadLine();
            Console.WriteLine();

            switch (val)
            {
                case "1":
                    VisaFordon();
                    break;
                case "2":
                    RegistreraKund();
                    break;
                case "3":
                    HyrUt();
                    break;
                case "4":
                    LamnaTillbaka();
                    break;
                case "5":
                    LaddaFordon();
                    break;
                case "6":
                    körProgram = false;
                    break;
                default:
                    Console.WriteLine("Ogiltigt val, försök igen.");
                    break;
            }
        }
    }

    static void VisaFordon()
    {
        Console.WriteLine($"{"Nr",-5} {"Typ",-10} {"Status",-10} {"Batteri"}");
        foreach (var f in fordonLista)
        {
            string typ = f is Elcykel ? "Elcykel" : "Cykel";
            string status = f.ArUthyrt ? "uthyrd" : "ledig";
            string batteriInfo = "";

            if (f is Elcykel elcykel)
            {
                batteriInfo = $"{elcykel.Batteri} %";
            }

            Console.WriteLine($"{f.Nummer,-5} {typ,-10} {status,-10} {batteriInfo}");
        }
    }

    static void RegistreraKund()
    {
        Console.Write("Namn: ");
        string namn = Console.ReadLine();

        Console.Write("Ålder: ");
        if (!int.TryParse(Console.ReadLine(), out int alder))
        {
            Console.WriteLine("Ogiltig ålder.");
            return;
        }

        Console.Write("Medlem (j/n): ");
        string medlemSvar = Console.ReadLine()?.Trim().ToLower();
        bool arMedlem = medlemSvar == "j";

        kundLista.Add(new Kund(namn, alder, arMedlem));
        Console.WriteLine($"{namn} är registrerad.");
    }

    static void HyrUt()
    {
        Console.Write("Kund: ");
        string kundNamn = Console.ReadLine();
        Kund valdKund = kundLista.Find(k => k.Namn.Equals(kundNamn, StringComparison.OrdinalIgnoreCase));

        if (valdKund == null)
        {
            Console.WriteLine("Kunden hittades inte. Registrera kunden först.");
            return;
        }

        Console.Write("Fordonsnummer: ");
        if (!int.TryParse(Console.ReadLine(), out int fordonsNummer))
        {
            Console.WriteLine("Ogiltigt fordonsnummer.");
            return;
        }

        Fordon valdFordon = fordonLista.Find(f => f.Nummer == fordonsNummer);
        if (valdFordon == null)
        {
            Console.WriteLine("Fordonet finns inte.");
            return;
        }

        // Kontrollera om fordonet redan är uthyrt
        if (valdFordon.ArUthyrt)
        {
            Console.WriteLine("Fordonet är redan uthyrt.");
            return;
        }

        // Åldersgränser (18 för elcykel, 15 för vanlig cykel)
        if (valdFordon is Elcykel)
        {
            if (valdKund.Alder < 18)
            {
                Console.WriteLine($"{valdKund.Namn} är för unga för att hyra elcykel (minst 18 år krävs).");
                return;
            }
        }
        else
        {
            if (valdKund.Alder < 15)
            {
                Console.WriteLine($"{valdKund.Namn} är för unga för att hyra cykel (minst 15 år krävs).");
                return;
            }
        }

        // Batterikoll för elcykel (< 30%)
        if (valdFordon is Elcykel elcykel && elcykel.Batteri < 30)
        {
            Console.WriteLine($"Elcykel {elcykel.Nummer} har för lite batteri ({elcykel.Batteri}%). Ladda den först.");
            return;
        }

        Console.Write("Antal timmar: ");
        if (!int.TryParse(Console.ReadLine(), out int timmar) || timmar <= 0)
        {
            Console.WriteLine("Ogiltigt antal timmar.");
            return;
        }

        // Utför uthyrning
        valdFordon.ArUthyrt = true;

        // Om det är en elcykel, förbruka batteri (10 procentenheter per timme)
        if (valdFordon is Elcykel ec)
        {
            ec.Batteri -= timmar * 10;
            if (ec.Batteri < 0) ec.Batteri = 0;
        }

        // Spara uthyrningen så vi kan räkna ut kvitto vid återlämning
        uthyrningar[valdFordon.Nummer] = (valdKund, timmar);

        string fordonsTypNamn = valdFordon is Elcykel ? "Elcykel" : "Cykel";
        Console.WriteLine($"{valdKund.Namn} hyr {fordonsTypNamn} {valdFordon.Nummer} i {timmar} timmar.");
    }

    static void LamnaTillbaka()
    {
        Console.Write("Fordonsnummer: ");
        if (!int.TryParse(Console.ReadLine(), out int fordonsNummer))
        {
            Console.WriteLine("Ogiltigt fordonsnummer.");
            return;
        }

        Fordon valdFordon = fordonLista.Find(f => f.Nummer == fordonsNummer);
        if (valdFordon == null || !valdFordon.ArUthyrt)
        {
            Console.WriteLine("Fordonet är inte uthyrt eller finns inte.");
            return;
        }

        if (!uthyrningar.ContainsKey(fordonsNummer))
        {
            Console.WriteLine("Hittar ingen aktiv uthyrning för detta fordon.");
            return;
        }

        var uthyrningInfo = uthyrningar[fordonsNummer];
        Kund kund = uthyrningInfo.kund;
        int timmar = uthyrningInfo.timmar;

        // Beräkna pris
        decimal bruttoPris = valdFordon.Timpris * timmar;
        decimal rabatt = 0;

        if (kund.ArMedlem)
        {
            rabatt = bruttoPris * 0.10m;
        }

        decimal attBetala = bruttoPris - rabatt;
        string fordonsTypNamn = valdFordon is Elcykel ? "Elcykel" : "Cykel";

        // Visa kvitto
        Console.WriteLine("KVITTO");
        Console.WriteLine($"{fordonsTypNamn}, {timmar} timmar à {valdFordon.Timpris} kr \t {bruttoPris:F2} kr");
        if (kund.ArMedlem)
        {
            Console.WriteLine($"Medlemsrabatt (10 %) \t\t -{rabatt:F2} kr");
        }
        Console.WriteLine($"Att betala \t\t\t {attBetala:F2} kr");

        // Återställ fordonet
        valdFordon.ArUthyrt = false;
        uthyrningar.Remove(fordonsNummer);
    }

    static void LaddaFordon()
    {
        Console.Write("Fordonsnummer att ladda: ");
        if (!int.TryParse(Console.ReadLine(), out int fordonsNummer))
        {
            Console.WriteLine("Ogiltigt fordonsnummer.");
            return;
        }

        Fordon valdFordon = fordonLista.Find(f => f.Nummer == fordonsNummer);
        if (valdFordon == null)
        {
            Console.WriteLine("Fordonet finns inte.");
            return;
        }

        if (valdFordon is Elcykel elcykel)
        {
            elcykel.Batteri = 100;
            Console.WriteLine($"Elcykel {elcykel.Nummer} är nu fulladdad (100 %).");
        }
        else
        {
            Console.WriteLine("Vanliga cyklar har inget batteri att ladda!");
        }
    }
}