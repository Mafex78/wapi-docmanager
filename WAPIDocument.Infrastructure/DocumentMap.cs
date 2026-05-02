using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using WAPIDocument.Domain.Entities.Documents;

namespace WAPIDocument.Infrastructure;

public static class DocumentMap
{
    public static void Register()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(Document)))
            return;

        BsonClassMap.RegisterClassMap<Document>(cm =>
        {
            cm.AutoMap();

            // Map Id → _id, auto-generate ObjectId as string
            cm.MapIdMember(c => c.Id)
                .SetIdGenerator(StringObjectIdGenerator.Instance)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
            
            // // Explicit field name mapping
            // cm.MapMember(c => c.CreatedAtUtc).SetElementName("createdAt");
            // cm.MapMember(c => c.UpdatedAtUtc).SetElementName("updatedAt");
            // cm.MapMember(c => c.Version).SetElementName("version");
            // cm.MapMember(c => c.IsActive).SetElementName("isActive");
            // cm.MapMember(c => c.Email).SetElementName("email");
            // cm.MapMember(c => c.Password).SetElementName("password");
            // cm.MapMember(c => c.Roles).SetElementName("roles");

            // // Ignore any extra fields coming from MongoDB
            // cm.SetIgnoreExtraElements(true);
        });
    }
}