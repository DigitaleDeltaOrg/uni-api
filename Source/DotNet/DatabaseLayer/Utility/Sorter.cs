namespace DatabaseLayer.Utility;

using Models;
using Microsoft.OData.UriParser;

public class Sorter : QueryNodeVisitor<string>
{
	private readonly        Dictionary<string, FieldMap?> _map;

	public Sorter(Dictionary<string, FieldMap?> map)
	{
		_map        = map;
	}
	
	public string TranslateOrderByClause(OrderByClause orderByClause)
	{
		var orderBy = string.Format("[{0}] {1}", Bind(orderByClause.Expression), GetDirection(orderByClause.Direction));
		if (orderByClause.ThenBy != null)
		{
			orderBy += "," + TranslateOrderByClause(orderByClause.ThenBy);
		}

		return orderBy;
	}
	
	private static string GetDirection(OrderByDirection dir)
	{
		return dir == OrderByDirection.Ascending ? "asc" : "desc";
	}
	
	private string Bind(QueryNode node)
	{
		if (node is not SingleValueNode singleValueNode)
		{
			return string.Empty;
		}

		return singleValueNode.Kind switch
		{
			QueryNodeKind.ResourceRangeVariableReference => BindRangeVariable((node as ResourceRangeVariableReferenceNode)?.RangeVariable),
			QueryNodeKind.SingleValuePropertyAccess => BindPropertyAccessQueryNode(node as SingleValuePropertyAccessNode),
			_ => string.Empty
		};
	}
	private string BindPropertyAccessQueryNode(SingleValuePropertyAccessNode? singleValuePropertyAccessNode)
	{
		if (singleValuePropertyAccessNode == null)
		{
			return string.Empty;
		}
		
		var mapped = _map.TryGetValue(singleValuePropertyAccessNode.Property.Name, out var fieldMap) ? fieldMap : null;
		return string.Format("[{0}]", mapped == null ? singleValuePropertyAccessNode.Property.Name : mapped.FieldName);
	}
	private string BindRangeVariable(ResourceRangeVariable? entityRangeVariable)
	{
		if (entityRangeVariable == null)
		{
			return string.Empty;
		}
		
		var mapped = _map.TryGetValue(entityRangeVariable.Name, out var fieldMap) ? fieldMap : null;
		return string.Format("[{0}]", mapped == null ? entityRangeVariable.Name : mapped.FieldName);
	}
}