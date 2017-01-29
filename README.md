# OrchardCollaboration
This is a project for agile collaboration.
Especially provide an api module for Orchard Collaboration project.

API containing below definition
1. Name: GetContentTypeDefinition
   Method: GET
   Example : %Your Orhard Address% + /api/AgileCollaboration/GetContentTypeDefinition?type=User
2. Name: ContentTypes
   Method: GET
   Example : %Your Orhard Address% + /api/AgileCollaboration/ContentTypes
3. Name:
   Method:POST
   Example :  %Your Orhard Address% + /api/AgileCollaboration/Login 
   
   Content-Type: application/json

   Posted Data:
   { userNameOrEmail: "admin",password:"admin888" }

   Result:
   {
     "Id": 109,
     "UserName": "david0718",
     "Email": "agilecollaboration@outlook.com"
   }
4. Name: GetMyProjects
   Method: GET
   Example:  %Your Orhard Address% + /api/AgileCollaboration/GetMyProjects?userName=admin
   Result:
   [
  {
    "Id": 146,
    "Title": "Super Rocket",
    "Description": "Super Rocket",
    "CreatedUtc": "2017-01-24T08:21:17.4163811Z",
    "PublishedUtc": "2017-01-24T08:21:18.8695056Z",
    "ModifiedUtc": "2017-01-24T08:21:18.8445855Z",
    "VersionCreatedUtc": "2017-01-24T08:21:17Z",
    "VersionModifiedUtc": "2017-01-24T08:21:18Z",
    "VersionPublishedUtc": "2017-01-24T08:21:18Z",
    "UserName": "admin"
  },
  {
    "Id": 110,
    "Title": "AgileCollaboration",
    "Description": "AgileCollaboration",
    "CreatedUtc": "2017-01-24T03:20:36.1702541Z",
    "PublishedUtc": "2017-01-24T03:20:38.3483787Z",
    "ModifiedUtc": "2017-01-24T03:20:38.3213782Z",
    "VersionCreatedUtc": "2017-01-24T03:20:36Z",
    "VersionModifiedUtc": "2017-01-24T03:20:38Z",
    "VersionPublishedUtc": "2017-01-24T03:20:38Z",
    "UserName": "admin"
  }
]
5.  Name:GetDashBoardViewModel
    Method:GET
    Example:  %Your Orhard Address% + /api/AgileCollaboration/GetDashBoardViewModel?userName=admin
    Result:
    {
  "AllItemsWithoutOwnerCount": 1,
  "CurrentUserOverrudeItemsCount": 0,
  "CurrentUserOverrudeRequestingTicketCount": 0,
  "AllOverrudeItemsCount": 2,
  "CurrentUserTickets": [
    {
      "Id": 1,
      "Name": "New",
      "OrderId": 1,
      "Count": 0
    },
    {
      "Id": 2,
      "Name": "In Progress",
      "OrderId": 2,
      "Count": 0
    },
    {
      "Id": 3,
      "Name": "Deferred",
      "OrderId": 3,
      "Count": 0
    },
    {
      "Id": 4,
      "Name": "Pending input",
      "OrderId": 4,
      "Count": 0
    },
    {
      "Id": 5,
      "Name": "Completed",
      "OrderId": 5,
      "Count": 0
    }
  ],
  "CurrentUserRequestingTickets": null,
  "IsCustomer": false,
  "IsOperator": true,
  "AllTickets": [
    {
      "Id": 1,
      "Name": "New",
      "OrderId": 1,
      "Count": 2
    },
    {
      "Id": 2,
      "Name": "In Progress",
      "OrderId": 2,
      "Count": 1
    },
    {
      "Id": 3,
      "Name": "Deferred",
      "OrderId": 3,
      "Count": 0
    },
    {
      "Id": 4,
      "Name": "Pending input",
      "OrderId": 4,
      "Count": 0
    },
    {
      "Id": 5,
      "Name": "Completed",
      "OrderId": 5,
      "Count": 0
    }
  ],
  "CurrentUserId": 2
}
6. Name:Search
   Method: GET
   Example :        
    %Your Orhard Address% + /api/AgileCollaboration/Search?DueDate=Overdue&userName=admin
    %Your Orhard Address% + /api/AgileCollaboration/Search?Status=1&userName=admin
