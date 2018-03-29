

# GLOBAL APIARY DOCUMENTATION SERVICE (G.L.A.D)

*by Gennady Chernyak*

### Summary
This library will enable automation of Apiary documentation based on Swagger Json 2.0 or Swagger YAML 1.2. 

The basic functionality provided here is to either enter the source URL of a Swagger (Json) document or to provide a YAML document as the source at the entry point. 

In addition an Apiary destination URL must be provided with a valid authentication token in the header. The Documenation to obtain the token can be found here https://help.apiary.io/api_101/authentication/

The code then takes the input Swagger or YAML and converts it to Markdown and updates an Apiary API document via the Apiary API interface.

### Suggested Usage
In my opinion the best way to use this is to have this wrapped in an API call and make the call as a post build action. In this way the documentation is updated upon each successful build.

Please feel free to leave any feedback or improvement suggestions.

Note: The YAML portion will require the YamlDotNet package.

### Examples

> Swagger JSON example
> 
``` javascript
{
  "swagger": "2.0",
  "info": {
    "version": "v1",
    "title": "My API"
  },
  "host": "http://myapi.com/api",
  "basePath": "/MyAPI",
  "schemes": [
    "http",
    "https"
  ],
  "paths": {
    "/assets": {
      "get": {
        "tags": [
          "Assets"
```
> Swagger YAML Example
``` javascript
swagger: "2.0"
info:
  description: "This is a sample server Petstore server.  You can find out more about Swagger at <a href=\"http://swagger.io\">http://swagger.io</a> or on irc.freenode.net, #swagger.  For this sample, you can use the api key \"special-key\" to test the authorization filters"
  version: 1.0.0
  title: Swagger Petstore YAML
  termsOfService: "http://swagger.io/terms/"
  contact:
    email: "apiteam@swagger.io"
  license:
    name: Apache 2.0
    url: "http://www.apache.org/licenses/LICENSE-2.0.html"
basePath: /v2
tags:
  - name: pet
    description: Everything about your Pets
    externalDocs:
      description: Find out more
      url: "http://swagger.io"
```
> XML Comments And CSharp Example

``` c#
        /// <summary>
        /// Get assets by id
        /// </summary>
        /// <title>Asset Requests</title>
        /// <remarks>This call can get all assets or get asset by id.</remarks>
        /// <param name="id"></param>
        /// <returns cref="Asset"></returns>
        [HttpGet]
        [Route("assets")]
        [Route("assets/{id:int}")]
        [ValidateModelStateFilter]
        [SwaggerResponse(200, "", Type = typeof(Asset))]
        public async Task<IHttpActionResult> GetAssetsById(int id)
        {
            var result = _unitOfWork.Assets.GetAssets(id);
            return Ok(result);
        }
```


> Raw Request to Apiary
``` javascript
POST /blueprint/publish/my/apiary/target HTTP/1.1
Host: api.apiary.io
Authentication: Token xxxxxxxxxxxxxxxxxxxxxxxxxxxx
Content-Type: application/json
Cache-Control: no-cache

{ 
	"code": "FORMAT: 1A\nHOST:localhost:16081\n\n# My API ... }"
}
```
### Dependencies

- DotNet Core
- Newtonsoft.Json
- YamlDotNet
- Microsoft.Extensions.Configuration (if applicable)

### Remarks
This application was designed to function as part of an automatic build process to enable easily created documentation.

The application in essence takes a swagger document, either in Swagger YAML or in Swagger YAML JSON (via URL) and converts it to BluePrint API (Markdown) then uses the Apiary API to modify your documentation. 

*Note: not all XML comments are supported because the possible comment node names are not limited* 

#### Links

**Swashbuckle - Swagger for WebApi (.NET)** -  https://www.nuget.org/packages/Swashbuckle

**Swagger Open Source Integrations** - https://swagger.io/open-source-integrations/
