using Microsoft.ProjectOxford.Face;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceApiClient.Models
{
    public class FaceIdentifyService : IDisposable
    {
        private readonly string SubscriptionKey;

        private FaceServiceClient client = null;

        public FaceIdentifyService(string subscriptionKey)
        {
            SubscriptionKey = subscriptionKey;
            client = new FaceServiceClient(subscriptionKey);
        }

        public async Task CreatePersonGroupAsync(string groupId, string groupName)
        {
            try
            {
                await client.CreatePersonGroupAsync(groupId, groupName).ConfigureAwait(false);
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task<bool> ExistPersonGroupAsync(string groupId)
        {
            try
            {
                var group = await client.GetPersonGroupAsync(groupId).ConfigureAwait(false);
                if (group != null) return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task AddPersonAsync(string groupId, string name)
        {
            try
            {
                var ret = await client.CreatePersonAsync(groupId, name).ConfigureAwait(false);
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task<List<Person>> GetPersonsAsync(string groupId)
        {
            try
            {
                var ret = await client.GetPersonsAsync(groupId).ConfigureAwait(false);
                return ret.Select(x =>
                    new Models.Person
                    {
                        Name = x.Name,
                        PersonId = x.PersonId,
                        PersistedFaceIds = x.PersistedFaceIds.ToList()
                    }).ToList();
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task<bool> ExistPersonAsync(string groupId, string name)
        {
            try
            {
                var ret = await client.GetPersonsAsync(groupId).ConfigureAwait(false);
                if (ret.Any(x => string.Compare(x.Name, name, false) == 0)) return true;
                else return false;
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task<Person> GetPersonAsync(string groupId, Guid personId)
        {
            try
            {
                var ret = await client.GetPersonAsync(groupId, personId).ConfigureAwait(false);
                return new Person
                {
                    Name = ret.Name,
                    PersistedFaceIds = new List<Guid>(ret.PersistedFaceIds),
                    PersonId = ret.PersonId
                };
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task RemovePersonAsync(string groupId, Guid personId)
        {
            try
            {
                await client.DeletePersonAsync(groupId, personId).ConfigureAwait(false);
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task<Guid> AddPersonFaceAsync(string groupId, Guid personId, Stream imageStream)
        {
            try
            {
                var ret = await client.AddPersonFaceAsync(groupId, personId, imageStream).ConfigureAwait(false);
                return ret.PersistedFaceId;
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task ClearPersonFaceAsync(string groupId, Guid personId)
        {
            try
            {
                var p = await client.GetPersonAsync(groupId, personId).ConfigureAwait(false);
                foreach (var id in p.PersistedFaceIds)
                {
                    await client.DeletePersonFaceAsync(groupId, personId, id).ConfigureAwait(false);
                }
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task TrainGroupAsync(string groupId)
        {
            try
            {
                await client.TrainPersonGroupAsync(groupId).ConfigureAwait(false);
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task<TrainStatus> GetTrainStatusAsync(string groupId)
        {
            try
            {
                var ret = await client.GetPersonGroupTrainingStatusAsync(groupId).ConfigureAwait(false);
                return new TrainStatus
                {
                    Created = ret.CreatedDateTime,
                    LastAction = ret.LastActionDateTime,
                    Message = ret.Message,
                    Status = ret.Status.ToString()
                };
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public async Task<List<DetectFaces>> DetectFacesAsync(string groupId, Stream imagePath)
        {
            try
            {
                var faces = await client.DetectAsync(imagePath).ConfigureAwait(false);
                var results = await client.IdentifyAsync(groupId, faces.Select(x => x.FaceId).ToArray());
                if (!results.Any()) return new List<DetectFaces>();
                return results.Select(x => new DetectFaces
                {
                    FaceId = x.FaceId,
                    PersonId = x.Candidates?.OrderByDescending(y => y.Confidence).Select(z => z.PersonId).FirstOrDefault(),
                    Confidence = x.Candidates?.OrderByDescending(y => y.Confidence).Select(z => z.Confidence).FirstOrDefault(),
                    Position = faces.Where(f => f.FaceId == x.FaceId).Select(fx => new FacePosition
                    {
                        Height = fx.FaceRectangle.Height,
                        Left = fx.FaceRectangle.Left,
                        Top = fx.FaceRectangle.Top,
                        Width = fx.FaceRectangle.Width
                    }).FirstOrDefault()
                }).ToList();
            }
            catch (FaceAPIException ex)
            {
                throw new Exception(ex.ErrorCode + "\r\n" + ex.ErrorMessage, ex);
            }
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
            }
        }
    }
}
