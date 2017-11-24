# Data Manager: Automation

Azure Data Factory project. Currently using ADF V1 (Visual Studio does not have support for V2 Preview).

Data Lake Linked Service uses User Credential Authentication (Service Principal Authentication is recommended, but it requires Azure AD).
When deploying from Visual Studio, publish the ADL Store Linked Service separately.
Publish generates an error in Visual Studio, but the Linked Service is generated anyway.
To fix the Linked Service, go to the Data Factory in Azure Portal and open the Author and Deploy blade.
Choose the Data Lake Linked Service and press *Authorize* at the top of the editor.
Enter your credentials to authorize the service.
This creates new *authorization* and *sessionId* parameters for the service.
After the authorization is complete, press *Deploy* at the top of the editor to finish updating the Linked Service.
