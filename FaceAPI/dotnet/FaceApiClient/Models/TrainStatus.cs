using System;

namespace FaceApiClient.Models
{
    public class TrainStatus
    {
        public DateTime Created { get; set; }
        public DateTime LastAction { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }
}
