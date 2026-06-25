
[Serializable]
public class Card
{
    public int Number { get; set; }
    public string Color { get; set; }
    public bool IsOpen { get; set; }

    public Card(int number, string color)
    {
        Number = number;
        Color = color;
        IsOpen = false;
    }
}
