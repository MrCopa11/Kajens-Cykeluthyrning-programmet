public class Kund
{
    public string Namn { get; set; }
    public int Alder { get; set; }
    public bool ArMedlem { get; set; }

    public Kund(string namn, int alder, bool arMedlem)
    {
        Namn = namn;
        Alder = alder;
        ArMedlem = arMedlem;
    }
}