namespace Server
{
    public class Call
    {
        public string Answerer { get; set; }
        public IPUser Caller { get; set; }

        public Call(string answerer, IPUser caller)
        {
            Answerer = answerer;
            Caller = caller;
        }

        public Call()
        {

        }
    }
}
