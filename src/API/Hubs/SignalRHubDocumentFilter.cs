//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;
//using System.Collections.Generic;

//namespace API.Hubs;
//public class SignalRHubDocumentFilter : IDocumentFilter
//{
//    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
//    {
//        // Create negotiate path item
//        var negotiatePathItem = new OpenApiPathItem();

//        negotiatePathItem.Operations.Add(OperationType.Get, new OpenApiOperation
//        {
//            Summary = "Negotiate",
//            Description = "Negotiate SignalR connection",
//            Responses = new OpenApiResponses
//            {
//                { "200", new OpenApiResponse { Description = "OK" } }
//            }
//        });

//        swaggerDoc.Paths.Add("/chathub/negotiate", negotiatePathItem); // Add the path to your SignalR hub

//        // Create path item for SendMessageToUser
//        var sendMessageToUserPathItem = new OpenApiPathItem();

//        sendMessageToUserPathItem.Operations.Add(OperationType.Post, new OpenApiOperation
//        {
//            Summary = "Send a message to a user",
//            Description = "Send a message to a specific user",
//            Parameters = new List<OpenApiParameter>
//            {
//                new OpenApiParameter
//                {
//                    Name = "message",
//                    In = ParameterLocation.Query,
//                    Required = true,
//                    Schema = new OpenApiSchema { Type = "string" }
//                },
//                new OpenApiParameter
//                {
//                    Name = "toUserId",
//                    In = ParameterLocation.Query,
//                    Required = true,
//                    Schema = new OpenApiSchema { Type = "string" }
//                }
//            },
//            Responses = new OpenApiResponses
//            {
//                { "200", new OpenApiResponse { Description = "Message sent" } }
//            }
//        });

//        swaggerDoc.Paths.Add("/chathub/sendmessagetouser", sendMessageToUserPathItem); // Add the path for the SendMessageToUser method

//        // Create path item for SendMessageToModerators
//        var sendMessageToModeratorsPathItem = new OpenApiPathItem();

//        sendMessageToModeratorsPathItem.Operations.Add(OperationType.Post, new OpenApiOperation
//        {
//            Summary = "Send a message to moderators",
//            Description = "Send a message to all moderators",
//            Parameters = new List<OpenApiParameter>
//            {
//                new OpenApiParameter
//                {
//                    Name = "message",
//                    In = ParameterLocation.Query,
//                    Required = true,
//                    Schema = new OpenApiSchema { Type = "string" }
//                }
//            },
//            Responses = new OpenApiResponses
//            {
//                { "200", new OpenApiResponse { Description = "Message sent" } }
//            }
//        });

//        swaggerDoc.Paths.Add("/chathub/sendmessagetomoderators", sendMessageToModeratorsPathItem); // Add the path for the SendMessageToModerators method
//    }
//}
