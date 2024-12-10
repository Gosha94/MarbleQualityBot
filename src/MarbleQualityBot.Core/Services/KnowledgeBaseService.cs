using MarbleQualityBot.Core.Domain.Enums;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace MarbleQualityBot.Core.Services;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly IGraph _graph;
    private readonly SparqlQueryParser _queryParser;
    private readonly TripleStore _store;
    
    public KnowledgeBaseService()
    {
        _graph = new VDS.RDF.Graph();
        _queryParser = new SparqlQueryParser();

        _store = new TripleStore();

        SeedKnowledgeBase();
    }

    private void SeedKnowledgeBase()
    {
        _graph.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

        // Add classes and suggestions
        AddSuggestion(classId: 0, confidenceLevel: ConfidenceLevel.HIGH, suggestion: "Stone should be crushed for premium countertops, flooring, and decorative sculptures.");
        AddSuggestion(classId: 0, confidenceLevel: ConfidenceLevel.MEDIUM, suggestion: "Stone can be used for interior wall cladding or medium-quality tiles.");
        AddSuggestion(classId: 0, confidenceLevel: ConfidenceLevel.LOW, suggestion: "Stone quality is uncertain; consider using it for prototypes or rough structures.");

        AddSuggestion(classId: 1, confidenceLevel: ConfidenceLevel.HIGH, suggestion: "Stone is suitable for paving pathways or constructing garden borders.");
        AddSuggestion(classId: 1, confidenceLevel: ConfidenceLevel.MEDIUM, suggestion: "Stone may be used for temporary structures or low-traffic areas like patios.");
        AddSuggestion(classId: 1, confidenceLevel: ConfidenceLevel.LOW, suggestion: "Stone may be ground into aggregate for basic concrete or filler material.");

        _store.Add(_graph);
    }

    private void AddSuggestion(int classId, ConfidenceLevel confidenceLevel, string suggestion)
    {
        var exNs = new Uri("http://example.org/");

        var rock = _graph.CreateBlankNode();

        _graph.Assert(rock, _graph.CreateUriNode(new Uri(exNs + "ClassId")), _graph.CreateLiteralNode(classId.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
        _graph.Assert(rock, _graph.CreateUriNode(new Uri(exNs + "Confidence")), _graph.CreateLiteralNode(confidenceLevel.ToString().ToLower()));
        _graph.Assert(rock, _graph.CreateUriNode(new Uri(exNs + "Suggestion")), _graph.CreateLiteralNode(suggestion));
    }

    public Task<string> GetSuggestion(int classId, ConfidenceLevel confidenceLevel, CancellationToken ct)
    {

        var query = _queryParser.ParseFromString($@"
        PREFIX ex: <http://example.org/>
        SELECT ?suggestion
        WHERE {{
            ?rock ex:ClassId '{classId}'^^<http://www.w3.org/2001/XMLSchema#integer> ;
                  ex:Confidence '{confidenceLevel.ToString().ToLower()}' ;
                  ex:Suggestion ?suggestion .
        }}");

        var queryProcessor = new LeviathanQueryProcessor(_store);
        var results = queryProcessor.ProcessQuery(query) as SparqlResultSet;

        var rawResult = results?.FirstOrDefault()?["suggestion"]?.ToString() ?? "No suggestion available.";
        var trimmedResult = System.Text.RegularExpressions.Regex.Replace(rawResult, @"\^\^.*$", string.Empty);

        return Task.FromResult(trimmedResult);
    }
}