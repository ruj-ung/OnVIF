namespace CamView
{
    class Camera
    {
        public string  Name { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int Channel { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public string Direction { get; set; }
        public string Company { get; set; }
        public uint Owner { get; set; }
        public int stationID { get; set; }
    }
}
