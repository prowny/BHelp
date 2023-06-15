namespace BHelp.Models
{
    public class AssistancePayment
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Action { get; set; }
        public int AmountInCents { get; set; }
        public string Note { get; set; }

        // string.Format("${0}.{1:00}", AmountInCents / 100, AmountInCents % 100);
        // means format with $sign amount/100, amount modulo 100
        // where modulo is 'the remainder when divided by'  100
    }
}
