namespace ConvCrmContracts.Common
{
    public class UserDetails
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public string type { get; set; } = default!;
        public string codigo_cliente { get; set; } = default!;
    }
}