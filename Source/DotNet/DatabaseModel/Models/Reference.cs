namespace DatabaseModel.Models;

using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

[Dapper.Contrib.Extensions.Table("reference")]
public class Reference
{
	public                     long    Count                  { get; set; }
	[ExplicitKey] [Key] public long    Id                     { set; get; }
	public                     string  ReferenceType          { set; get; } = null!;
	public                     string? Organisation           { set; get; }
	public                     string  Code                   { set; get; }
	public                     string? Uri                    { set; get; }
	public                     string? Geometry               { set; get; }
	public                     string? Display                { set; get; }
	public                     string? Description            { set; get; }
	public                     Guid    ExternalKey            { set; get; }
	public                     Guid?   TaxonTypeExternalKey   { set; get; }
	public                     string? TaxonMainType          { set; get; }
	public                     Guid?   TaxonGroupExternalKey  { set; get; }
	public                     Guid?   TaxonParentExternalKey { set; get; }
	public                     string? TaxonRank              { set; get; }
	public                     string? TaxonAuthor            { set; get; }
	public                     string? TaxonParentAuthor      { set; get; }
	public                     string? CasNumber              { set; get; }
	public                     string? ParameterType          { set; get; }
	public                     string? TaxonNameNL            { set; get; }
	public                     string? TaxonStatusCode        { set; get; }

	public static Reference FromDynamic(dynamic item)
	{
		return new Reference()
		{
			Count                  = item.count,
			Id                     = item.id,
			ReferenceType          = item.reference_type,
			Organisation           = item.organisation,
			Code                   = item.code,
			Uri                    = item.uri,
			Geometry               = item.geometry,
			Display                = item.display,
			Description            = item.description,
			ExternalKey            = item.external_key,
			TaxonTypeExternalKey   = item.taxon_type_external_key,
			TaxonMainType          = item.taxon_main_type,
			TaxonGroupExternalKey  = item.taxon_group_external_key,
			TaxonParentExternalKey = item.taxon_parent_external_key,
			TaxonRank              = item.taxon_rank,
			TaxonAuthor            = item.taxon_author,
			TaxonParentAuthor      = item.taxon_parent_author,
			CasNumber              = item.cas_number,
			ParameterType          = item.parameter_type,
			TaxonNameNL            = item.taxon_name_nl,
			TaxonStatusCode        = item.taxon_status_code
		};
	}
}

