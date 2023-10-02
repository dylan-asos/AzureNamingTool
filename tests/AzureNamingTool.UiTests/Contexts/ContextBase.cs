using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;

namespace AzureNamingTool.UiTests.Contexts;

public abstract class ContextBase
{
    public const string PageContentSelector = "body";

    private IHtmlDocument? _document;
    private IElement? _foundElement;

    protected HttpResponseMessage? Response;

    protected ContextBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected HttpClient HttpClient { get; }

    protected void SetDefaultSecurityRequestHeaders()
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "true");
    }

    protected IHtmlDocument GetDocument()
    {
        Debug.Assert(_document != null, nameof(_document) + " != null");
        return _document;
    }

    public async Task Given_the_route_is_requested(string route)
    {
        await Given_the_route_is_requested(route, "text/html");
    }

    public async Task Given_the_route_is_requested(string route, string contentType)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, route);
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));

        Response = await HttpClient.SendAsync(message);
    }

    public Task Then_response_code_should_be(HttpStatusCode statusCode)
    {
        Response.Should().NotBeNull("A response should be returned");
        Response!.StatusCode.Should().Be(statusCode);
        return Task.CompletedTask;
    }

    public async Task When_the_form_is_submitted(string formId, IDictionary<string, string> fields)
    {
        var form = GetDocument().Forms.FirstOrDefault(d => d.Id == formId);
        var elements = form?.Elements ?? Enumerable.Empty<IHtmlElement>();
        foreach (var element in elements.OfType<IHtmlInputElement>())
        {
            if (fields.TryGetValue(element.Name ?? string.Empty, out var value))
            {
                element.Value = value;
            }
        }

        Debug.Assert(form != null, nameof(form) + " != null");
        var message = new HttpRequestMessage(HttpMethod.Post, form.Action);
        var submission = form.GetSubmission();
        var items = submission!.Target.Query!.Split(new[] {'&'});
        var dict = items.Select(item => item.Split(new[] {'='})).ToDictionary(pair => pair[0], pair => pair[1]);

        message.Content = new FormUrlEncodedContent(dict);
        Response = await HttpClient.SendAsync(message);
    }

    public async Task When_the_response_content_is_parsed()
    {
        Debug.Assert(Response != null, nameof(Response) + " != null");

        var content = await Response.Content.ReadAsStringAsync();
        var parser = new HtmlParser();
        _document = await parser.ParseDocumentAsync(content);
    }

    public Task Then_the_document_should_contain(string content)
    {
        GetDocument().DocumentElement.InnerHtml.Should().Contain(content);
        return Task.CompletedTask;
    }

    public Task When_search_for_element_by_selector(string selector)
    {
        _foundElement = GetDocument().QuerySelector(selector);
        return Task.CompletedTask;
    }

    protected Task When_search_for_element_by_local_name(string localName)
    {
        _foundElement = GetDocument().All
            .FirstOrDefault(m => m.LocalName == localName);

        return Task.CompletedTask;
    }

    protected Task When_search_for_element_by_id(string id)
    {
        _foundElement = GetDocument().All
            .FirstOrDefault(m => m.Id == id);

        return Task.CompletedTask;
    }

    protected Task When_search_for_element_by_class(string className)
    {
        _foundElement = GetDocument().All
            .FirstOrDefault(m => m.ClassName == className);

        return Task.CompletedTask;
    }

    public Task Then_the_found_element_should_contain(string expectedContent)
    {
        _foundElement.Should().NotBeNull("You must have run a search and found matching elements");
        _foundElement!.TextContent.Should().Contain(expectedContent);
        return Task.CompletedTask;
    }

    protected Task Then_the_found_element_value_should_be(string expectedValue)
    {
        _foundElement.Should().NotBeNull("You must have run a search and found matching elements");
        _foundElement!.OuterHtml.Should().Contain(expectedValue);
        return Task.CompletedTask;
    }

    protected Task Then_the_found_element_value_should_be_greater_than_zero()
    {
        _foundElement.Should().NotBeNull("You must have run a search and found matching elements");
        var htmlInput = (IHtmlInputElement) _foundElement!;
        var data = Convert.ToInt32(htmlInput.Value);
        data.Should().BeGreaterThan(0);
        return Task.CompletedTask;
    }
}