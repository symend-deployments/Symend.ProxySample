[appsettings]: appsettings.json
[AuthWorkflow]: Authentication/AuthWorkflow.cs
[Program]: Program.cs
[ProxyRequestOverview]: ./Documentation/ProxyRequestOverview.jpg
[Startup]: Startup.cs
[SymendApiTransformer]: HttpTransformers/SymendApiTransformer.cs

## Symend Proxy Sample

### Table of Contents

 - [Introduction](#introduction)
 - [Warning, Security](#warning-security)
 - [Warning, Interaction](#warning-interaction)
 - [Prequisites](#prequisites)
 - [Making Requests to the Proxy](#making-requests-to-the-proxy)
 - [Example Illustrated](#example-illustrated) 
 - [File Overview](#file-overview)

---

### Introduction

The “View Sent Emails” feature allows Symend clients to access a copy of the actual email message that was sent to their customer. This includes all rendering of personalizations and is a reasonable representation of the actual message the end-customer would have received. It should be understood that although the copy of the email is roughly exact, factors such as different email clients mean that it can't be garaunteed that it will be 100% exactly what was viewed

Exports which include the Sent Message URL property from email messaging events can be viewed and is intended to be done via a proxy.

This Symend.ProxySample package contains an example of a .NET C# web application that can be used as a web proxy server to make requests to Symend for resources made available by its Web API for sent emails.

The Sample Proxy is designed to forward requests to obtain Symend resources (on behalf of users) even if the users do not have a Symend account.  This is based on the assumptions that:

1. Access to the Web Proxy will be restricted based on Symend client’s corporate standards.
2. The Sample Proxy authenticates on behalf of the user through a trusted machine to machine relationship with Symend’s Auth0 server. The trust relationship is established by configuring the Web Proxy with secrets provided by Symend’s IT department.

To try out the sample proxy, simply extract the package content on to your local disk drive, and open the project using your favourite .NET IDE.

---

### Warning, Security

Access to the sample proxy is not secured; once the proxy is fully configured, it can potentially retrieve resources containing PII data.  Please take caution to restrict access to the proxy as well as the secrets obtained from Symend.

---

### Warning, Interaction

It is important to be aware that the stored sent email that users will be able to view through this URL will contain the same Calls to Action (CTA) that were present in the message received by the customer. Clicking on or engaging with any CTAs in the email and following through with any offers on Landing Pages will be registered by Symend as if the customer themselves did so. Therefore it is strongly recommended that these emails are not used for testing purposes and that following any links be done with caution.

---

### Prequisites

The sample proxy must be setup properly with security and Symend API configurations before its first run.

The Security configurations contains connection information and secrets to establish the machine to machine trust between the proxy and Symend’s authentication provider (Auth0), which allows the proxy to authenticate and authorize on behalf of the users.

The API configurations specifies the URL of the Symend Web API to use as well as the unique identifier for your organization to be used to make requests for.

These configurations will be covered in more details in the Technical Overview section. Please acquire the configuration secrets from your Symend Client Representative.

---

### Making Requests to the Proxy

The sample proxy is configured to start a local web host at <https://localhost:6001> by default. It is also configured with a single catch-all endpoint (see Startup.cs) that takes a single query parameter, “url”, which is the URL to request for a Symend resource.

For example:

- If the Symend resource URL is: <https://api.symend.io/msg/v1/email-messages/111>
- Then the request to the Proxy should be: <https://localhost:6001?url=https://api.symend.io/msg/v1/email-messages/111>

---

### Example Illustrated

A picture is worth a thousand words, the following diagram illustrates a scenario where a XyzCo (fictional company) Help Desk employee needs to retrieve a copy of email that Symend sent to one of XyzCo’s customers:

!["Architecture" diagram displaying an example of a request from the proxy to Symend's API][ProxyRequestOverview]

1. Help Desk employee locates the email link for the customer, and clicks on it.
2. XyzCo’s firewall intercepts traffic for Symend Web API, and redirects the request to the Web Proxy.
3. Alternatively, the Help Desk employee can construct a request to the Web Proxy manually. The main objective is to make a request via the Web Proxy with the Symend email URL as a parameter.
4. Regardless of what the mechanism is, XyzCo’s Azure Active Directory (or equivalent) should ensure only authenticated and authorized XyzCo employees may access the Web Proxy.
5. Upon request, the Web Proxy will retrieve an authentication token from Symend’s Authentication provider, Auth0, on behalf of the Help Desk employee. This is under the assumption that the Web Proxy is configured with the secrets provided by Symend’s IT department.
6. The Web Proxy generates a request to the Symend Web API using the email link parameter, and the authentication token obtained from Auth0.
7. Symend Web API authenticates against the request, and returns the requested email content back to the Help Desk employee.

---

### File Overview

This section provides a walkthrough of each component of the sample code base. The goal is to make it easier for the readers to familiarize with the sample project by providing context around each component’s function and purpose.

| File | Description |
| ---- | ----------- |
| [Program.cs][Program] | Standard .NET web application startup file which configures and starts the proxy web hosts |
| [Startup.cs][Startup] | Standard .NET Web application startup file which registers all the components of the proxy as well as declaring a catch-all endpoint that performs the request forwarding operation |
| [appsettings.json][appsettings] | System JSON configurations. The sections `AuthConfig` (security) and `ApiProxyConfig` (API) sections will need to be completed with settings and secrets obtained from Symend |
| [AuthWorkflow.cs][AuthWorkflow] | Contains the algorithm to retrieve an authentication token from Symend’s authentication provider, Auth0. It generates a web request to Auth0 utilizing the AuthConfig settings |
| [SymedApiTransformer.cs][SymendApiTransformer] | Transformation from the proxy request to the request specified in the `url` query parameter.  It does so by utilizing the AuthWorkflow component to retrieve the authentication token, and the ApiProxyConfig settings to construct the forward request headers |
