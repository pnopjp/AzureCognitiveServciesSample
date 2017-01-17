using System.Collections.Generic;

namespace FaceApiClient.Models
{
    public class DetectResult
    {
        public string SourceImagePath { get; set; }
        public List<DetectFaces> Faces { get; set; }
    }
}
