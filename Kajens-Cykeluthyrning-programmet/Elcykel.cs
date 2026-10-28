public class Elcykel : Fordon
{
    public int Batteri { get; set; }

    public Elcykel(int nummer, int batteri)
    {
        Nummer = nummer;
        TimPris = 70m; // Timpris för elcykel
        Batteri = batteri;
    }
}