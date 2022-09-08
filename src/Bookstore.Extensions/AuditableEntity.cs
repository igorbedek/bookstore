using Rhetos.Dsl;
using Rhetos.Dsl.DefaultConcepts;
using System.ComponentModel.Composition;

namespace Bookstore.Extensions
{
    [Export(typeof(IConceptInfo))]
    [ConceptKeyword("Auditable")]
    public class AuditPropertyInfo : IConceptInfo
    {
        [ConceptKey]
        public EntityInfo Entity { get; set; }
    }

    [Export(typeof(IConceptMacro))]
    public class AuditMacro : IConceptMacro<AuditPropertyInfo>
    {
        public IEnumerable<IConceptInfo> CreateNewConcepts(AuditPropertyInfo conceptInfo, IDslModel existingConcepts)
        {
            return new List<IConceptInfo>
                {
                    new DateTimePropertyInfo { Name = "CreatedAt", DataStructure = conceptInfo.Entity },
                    new DateTimePropertyInfo { Name = "ModifiedAt", DataStructure = conceptInfo.Entity },
                    new ShortStringPropertyInfo { Name = "ModifiedBy", DataStructure = conceptInfo.Entity },
                    new ShortStringPropertyInfo { Name = "CreatedBy", DataStructure = conceptInfo.Entity },
                    new EntityLoggingInfo { Entity = conceptInfo.Entity }
                };
        }
    }
}
