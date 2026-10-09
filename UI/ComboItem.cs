namespace SmartMed.UI
{
    public class ComboItem
    {
        public int Id { get; }
        public string Name { get; }
        public ComboItem(int id, string name) { Id = id; Name = name; }
        public override string ToString() => Name;
    }
}
