# What?

Define your API using protobuf:

**api.protobuf**

```
syntax = "proto3";
message SearchRequest {
    string query = 1;
    int32 page_number = 2;
    int32 results_per_page = 3;
}
message SearchResponse {
    repeated string result = 1;
}
service SearchService {
    rpc Search(SearchRequest) returns (SearchResponse);
}
```

**Controllers/ServiceServiceController.vs**
```csharp
class SearchServiceController
{
    public SearchResponse Search(SearchRequest request)
    {
        return new SearchResponse();
    }
}

class SearchRequest
{
    public string Query { get; set; }
    public int PageNumber { get; set; }
    public int ResultsPerPage { get; set; }
}

class SearchResponse
{
    public List<string> Result { get; set; }
}
```