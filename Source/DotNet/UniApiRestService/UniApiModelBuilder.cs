namespace UniApiRestService;

using DigitaleDelta;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

public static class UniApiModelBuilder
{
	public static IEdmModel GetEdmModel()
	{
		var oDataConventionModelBuilder = new ODataConventionModelBuilder();
		oDataConventionModelBuilder.AddComplexType(typeof(ODataNamedValueDictionary<string>));
		oDataConventionModelBuilder.EntityType<Reference>().HasKey(_ => _.Id);
		oDataConventionModelBuilder.EntityType<Reference>().HasOptional(a => a.TaxonGroup, (a,  b) => a.TaxonGroupId == b!.Id);
		oDataConventionModelBuilder.EntityType<Reference>().HasOptional(a => a.TaxonType, (a,   b) => a.TaxonTypeId == b!.Id);
		oDataConventionModelBuilder.EntityType<Reference>().HasOptional(a => a.TaxonParent, (a, b) => a.TaxonParentId == b!.Id);
		oDataConventionModelBuilder.EntityType<Observation>().HasKey(_ => _.Id);
		oDataConventionModelBuilder.EntityType<Observation>().HasMany(a => a.RelatedObservations);
		oDataConventionModelBuilder.EntitySet<Reference>("reference");
		oDataConventionModelBuilder.EntitySet<Observation>("observation");

		return oDataConventionModelBuilder.GetEdmModel();
	}

}