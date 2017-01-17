using System;

namespace FaceApiClient.Models
{
    public class DetectFaces
    {
        /// <summary>
        /// 一時的なId
        /// </summary>
        public Guid FaceId { get; set; }

        /// <summary>
        /// グループ内に存在するId
        /// </summary>
        public Guid? PersonId { get; set; }

        /// <summary>
        /// 合致率
        /// </summary>
        public double? Confidence { get; set; }

        public FacePosition Position { get; set; }

        public string Name { get; set; }
    }
}
