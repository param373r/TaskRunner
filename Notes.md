# Notes
- Cache-Control headers can be used to specify how long a client should cache a response to avoid unnecessary requests to the server.

> PUT is like updating a resource on the server. For example, if you edit your profile information on a website, your browser might send a PUT request to the server with the updated information.

WHEREAS!!

> PATCH is like updating a resource on the server, but only changing part of it. For example, if you only want to change the caption on an image on a website, your browser might send a PATCH request to the server.

- the "User-Agent" header specifies the type of client making the request
- the "Accept" header specifies the type of content that the client can handle.
- the "Content-Encoding" header specifies the encoding used to compress the content.

> Entity Headers are those that describe the payload of the data (i.e. metadata about the message body; eg. Content-Length, Content-Encoding, Content-Type, Expires etc)

> OPTIONS method is used to retrieve information about the communication options available for a resource. When a client sends an OPTIONS request, the server responds with a list of the available methods, headers, and other communication options for the specified resource.

> HEAD is a HTTP method that is used to request only headers.

- Common data format in REST APIs are JSON and XML.
## REpresentational State Transfer
- REST APIs use allow the use of "hypermedia" (the response from the server containing links helps in traversing the API easily).
- REST API is Stateless but Cacheable, which means it may not maintain a state from previous requests (without suitable headers obvio)... But can cache data for easy quick access to the same resources.
- Layered System in REST API makes it easier for the user to work with top layer without worrying of the underlying layers
> Industry web standards for authentication and authorizations are JWT and OAuth (or OAuth2)

> Optimize API by: Error Handling, implement caching and use pagination for larger datasets

## Anatomy of REST API
- Resource: It is what we retrieve from a REST API like user, article, image.
- HTTP methods: They are different actions help in easy styling and making of the REST API.
- URI: It is make up of protocol (https), sub domain and domain name (api. ; localhost.com) and a route (/rest/v1)
- Headers: Tells more about the content being sent or received
- Body: Contains data that is transferred with the request and response
- Status Codes: tells the response of a request made was it successful, redirected, ended up in a client error or server error.
## Building a REST API
### Steps to make the API
- Design the API
- Choosing right tools (what tools?)
- Implementing the API.

### Building Part
- Idenitfy the resources: What resources will API give access to, and __design the ENDPOINTS accordingly__.
    - Choose the appropriate HTTP Method for these endpoints
- Design the data model: This includes __schema for each model and relationship between each other__.
- __Choose a framework__ to build the API: NodeJS with Express, Ruby on rails, Django etc.
- Implement the endpoints __ACTUALLY CODE THEM__.
- __Test the API__
- __Document the API__.
- __DEPLOY!!__

# Security
## CSP (Content Security Policy)
- It is a security feature that allows you to specify which sources of content (such as scripts, images, and stylesheets) are allowed to be loaded and executed on your website or web application.
- This prevents loading of unauthorized scripts on the webapp.
## CORS (Cross-Origin Resource Sharing)

## Same-origin policy

# Versioning
There are multiple ways of versioning the API.

## URL Versioning
- This type of versioning is in the URL. Eg. https://host.com/v1/status
- This is the most common type.

## Query parameter versioning
- This type is where we specify the version in the parameter. Eg. https://host.com/?version=1

## Header versioning
- This is done by using `X-Api-Version` header. Eg. `X-Api-Version: 1`

## Content negotiation
- This is also a type of header versioning, but instead we use `Accept` header to negotiate for the response in a particular format defined by a version.
- It is something like: `Accept: application/vnd.example.api.v1+json`