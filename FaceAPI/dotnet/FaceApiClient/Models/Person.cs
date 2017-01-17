using System;
using System.Collections.Generic;

namespace FaceApiClient.Models
{
    public class Person
    {
        public string Name { get; set; }
        public Guid PersonId { get; set; }
        public List<Guid> PersistedFaceIds { get; set; }
    }
}
