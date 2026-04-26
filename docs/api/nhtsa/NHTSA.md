Skip to main content
National Highway Traffic Safety Administration, part of the U.S. Department of TransportationAn icon that symbolizes the United States Department of Transportation logo.
Search the site
National Highway Traffic Safety Administration Homepage

    					Ratings


    					Recalls


    					Risky Driving


    					Road Safety


    					Vehicle Safety


          More

NHTSA Datasets and APIs
In Support of Open Government Directive
Share: Facebook Twitter LinkedIn Mail
NHTSA Datasets and APIs
Ratings
Recalls
Investigations
Complaints
Manufacturer Communications
Car Seat Inspection Locator
Ratings

NHTSA's New Car Assessment Program conducts crash tests to determine how well a vehicle protects its occupants during a crash, and rollover resistance tests to determine the risk of a vehicle rolling over in a single-vehicle crash. Also, NCAP conducts advanced driver assistance system tests to determine how well the system avoids a crash.
Approach A. Get by Model Year, Make, Model

Step 1: Get available vehicle variants for a selected Model Year, Make and Model

Make the request to get the crash tested vehicle variants for the Model Year, Make and Model combination.

    /SafetyRatings/modelyear/2013/make/Acura/model/rdx

Response Fields: Response is a list of vehicle variants for Model Year, Make and Model with their Vehicle Id and description of the variant.

Working Sample: `https://api.nhtsa.gov/SafetyRatings/modelyear/2013/make/Acura/model/rdx`

Step 2: Get the Safety Ratings for the selected variant

Now that you have the Vehicle Id of the precise model that you to look up, go ahead and make the request to get the Safety Ratings for the required vehicle variant by passing in the Vehicle Id.

    /SafetyRatings/VehicleId/7520

Response Fields: Response is a list of Safety Ratings for the given vehicle variant.

Working Sample: JSON
Approach B. Get by Model Year, Make, Model

Step 1: Get all Model Years

Request a list of available Model Years for a given product type (Vehicle).

    /SafetyRatings

Response Fields: Response is a list of available Model Years in the repository. Use a Model Year from this list to use in the next step.

Working Sample: JSON

Step 2: Get all Makes for the Model Year

Request a list of vehicle Makes by providing a specific vehicle Model Year.

    /SafetyRatings/modelyear/2013

Response Fields: Response is a list of vehicle Makes for that Model Year. Use the Model Year and a Make from this list to use in the next step.

Working Sample: JSON

Step 3: Get all Model for the Make and Model Year

Request a list of vehicle Models by providing the vehicle Model Year and Make.

    /SafetyRatings/modelyear/2013/make/Acura

Response Fields: Response is a list of Safety Ratings for the given Model Year, Make and Model

Working Sample: JSON

Step 4: Get available vehicle variants for a selected Model Year, Make and Model

Make the request to get the crash tested vehicle variants for the Model Year, Make and Model combination.

    /SafetyRatings/modelyear/2013/make/Acura/model/rdx

Response Fields: Response is a list of vehicle variants for Model Year, Make and Model with their Vehicle Id and description of the variant.

Working Sample: JSON

Step 5: Get the Safety Ratings for the selected variant

Now that you have the Vehicle Id of the precise model that you to look up, go ahead and make the request to get the Safety Ratings for the required vehicle variant by passing in the Vehicle Id.

    /SafetyRatings/VehicleId/7520

Response Fields: Response is a list of Safety Ratings for the given vehicle variant.

Working Sample: JSON
Notes

Color codes to understand each part of the request URL:

    Red: Method Name
    Green: Applicable parameters

Request parameters and method names are case-sensitive.
Recalls

Manufacturers who determine that a product or piece of original equipment either has a safety defect, or is not in compliance with federal safety standards, are required to notify NHTSA within five business days. NHTSA requires that manufacturers file a defect and noncompliance report as well as quarterly recall status reports, in compliance with Federal Regulation 49 (the National Traffic and Motor Safety Act) Part 573, which identifies the requirements for safety recalls. NHTSA stores this information and the data can be used to search for recall information related to specific NHTSA campaigns and product types.  
Approach A: Get by Model Year, Make, Model

Make the request to get the recalls for the required combination of Model Year, Make and Model.

    Syntax: api.nhtsa.gov/recalls/recallsByVehicle?make={MAKE}&model={MODEL}&modelYear={MODEL_YR}
    Example:https://api.nhtsa.gov/recalls/recallsByVehicle?make=acura&model=rdx&modelYear=2012

Response Fields: Response is a list of recalls for the given Model Year, Make and Model.

Working Sample: JSON

Approach B: Get by Model Year, Make, Model

Step 1: Get all Model Years

Request a list of available Model Years for a given product type (Vehicle).

    Syntax: api.nhtsa.gov/products/vehicle/modelYears?issueType=r
    Example: https://api.nhtsa.gov/products/vehicle/modelYears?issueType=r

Working Sample: JSON

Step 2: Get all Makes for the Model Year

Request a list of vehicle Makes by providing a specific vehicle Model Year.

    Syntax: api.nhtsa.gov/products/vehicles/makes?modelYear={year}&issueType=r
    Example: https://api.nhtsa.gov/products/vehicle/makes?modelYear=2021&issueType=r

Response Fields: Response is a list of vehicle Makes for that Model Year. Use the Model Year and a Make from this list to use in the next step.

Working Sample: JSON

Step 3: Get all Model for the Make and Model Year

Request a list of vehicle Models by providing the vehicle Model Year and Make.

    Syntax: api.nhtsa.gov/products/vehicle/models?modelYear={year}&make={make}&issueType=r
    Example: https://api.nhtsa.gov/products/vehicle/models?modelYear=2010&make=acura&issueType=r

Working Sample: JSON

Step 4 : Get all Recalls for the selected Model Year, Make, Model

Now that you have all three key pieces of information (Model Year, Make, Model), go ahead and make the request to get the recalls for the required combination.

    Syntax: api.nhtsa.gov/recalls/recallsByVehicle?make={MAKE}&model={MODEL}&modelYear={MODEL_YR}
    Example: https://api.nhtsa.gov/recalls/recallsByVehicle?make=acura&model=rdx&modelYear=2012

Response Fields: Response is a list of recalls for the given Model Year, Make and Model.

Working Sample: JSON

Approach C. Get by NHTSA Campaign Number

Get all recalls as part of a NHTSA recall campaign number

Request a list of recalls by providing the NHTSA recall campaign number

    Syntax: api.nhtsa.gov/recalls/campaignNumber?campaignNumber ={campaignNumber}
    Example: https://api.nhtsa.gov/recalls/campaignNumber?campaignNumber=12V176000

Response Fields: Response is a list of recalls for the specified NHTSA recall campaign number.

Working Sample: JSON
Notes

Color codes to understand each part of the request URL:

    Red: Method Name
    Green: Applicable parameters

Request parameters and method names are case sensitive
Investigations

After a preliminary review of consumer complaints and other information related to alleged defects, NHTSA obtains information from the manufacturer(including data on complaints, crashes, injuries, warranty claims, modifications, and part sales) and determines whether further analysis is warranted. If warranted, the investigator will conduct a more detailed and complete analysis of the character and scope of the alleged defect.
Complaints

Complaint information entered into NHTSA’s Office of Defects Investigation vehicle owner's complaint database is used with other data sources to identify safety issues that warrant investigation and to determine if a safety-related defect trend exists. Complaint information is also analyzed to monitor existing recalls for proper scope and adequacy.
Approach A: Get by Model Year, Make, Model

Make the request to get the complaints for the required combination of Model Year, Make and Model.

    Syntax: api.nhtsa.gov/complaints/complaintsByVehicle?make={MAKE}&model={MODEL}&modelYear={MODEL_YR}
    Example: https://api.nhtsa.gov/complaints/complaintsByVehicle?make=acura&model=rdx&modelYear=2012

Response Fields: Response is a list of complaints for the given Model Year, Make and Model

Working Sample: JSON

Approach B: Get by Model Year, Make, Model

Step 1: Get all Model Years

Request a list of available Model Years for a given product type (Vehicle).

    Syntax: api.nhtsa.gov/products/vehicle/modelYears?issueType=c
    Example: https://api.nhtsa.gov/products/vehicle/modelYears?issueType=c

Response Fields: Response is a list of available Model Years in the repository. Use a Model Year from this list to use in the next step.

Working Sample: JSON

Step 2: Get all Makes for the Model Year

Request a list of vehicle Makes by providing a specific vehicle Model Year.

    Syntax: api.nhtsa.gov/products/vehicle/makes?modelYear={year}&issueType=c
    Example: https://api.nhtsa.gov/products/vehicle/makes?modelYear=2021&issueType=c

Response Fields: Response is a list of vehicle Makes for that Model Year. Use the Model Year and a Make from this list to use in the next step.

Working Sample: JSON

Step 3: Get all Models for the Make and Model Year

Request a list of vehicle Models by providing the vehicle Model Year and Make.

    Syntax: api.nhtsa.gov/products/vehicle/models?modelYear={year}&make={make}&issueType=c
    Example: https://api.nhtsa.gov/products/vehicle/models?modelYear=2010&make=acura&issueType=c

Working Sample: JSON

Step 4: Get all complaints for the selected Model Year, Make, Model

Now that you have all three key pieces of information (Model Year, Make, Model), make the request to get the complaints for the required combination.

    Syntax: api.nhtsa.gov/complaints/complaintsByVehicle?make={MAKE}&model={MODEL}&modelYear={MODEL_YR}
    Example: https://api.nhtsa.gov/complaints/complaintsByVehicle?make=acura&model=rdx&modelYear=2012

Response Fields: Response is a list of complaints for the given Model Year, Make and Model

Working Sample: JSON

Approach C: Get by ODI Number

Request a list of complaints by providing ODI number.

    Syntax: api.nhtsa.gov/complaints/odinumber?odinumber={odinumber}
    Example: https://api.nhtsa.gov/complaints/odinumber?odinumber=11184030

Response Fields: Response is a list of complaints for the specified ODI number.

Working Sample: JSON
Notes

Color codes to understand each part of the request URL:

    Red: Method Name
    Green: Applicable parameters

Request parameters and method names are case sensitive
Manufacturer Communications

All manufacturers of motor vehicles or motor vehicle equipment, including low volume and child restraints, must submit to NHTSA copies of their manufacturer communications sent to dealers, distributors, owners, purchasers, lessors, or lessees regarding any defect, failure or malfunction beyond normal deterioration in use, failure of performance, flaw or other unintended deviation from design specifications whether it is safety-related or not.
Car Seat Inspection Locator

Car crashes are a leading cause of death and injuries for children. Data show a high number of child car seats are not installed properly. Car seat inspection stations make it easier for parents and caregivers to check to see if their car seat is installed correctly. NHTSA provides information to help people locate a car seat inspection station. Information for each station is reported to NHTSA and we attempt to validate the station locations using a commercial geographic database so that this data will, in most cases, be able to be used for driving directions.

    About NHTSA
    Careers & Internships
    Contact Us
    Recall Information
    Report a Safety Problem

    Vehicle Manufacturers
    State Governments
    Enforcement & Justice

    SaferCar App
    TrafficSafetyMarketing.gov
    EMS.gov
    911.gov

    Web Policies & Notices
    Accessibility
    FOIA
    Privacy Policy
    Information Quality
    Vulnerability Disclosure Policy
    No Fear Act Data
    Ethics
    Civil Rights
    Office of Inspector General
    OIG Hotline
    BusinessUSA
    USA.gov
    WhiteHouse.gov

National Highway Traffic Safety Administration

National Highway Traffic Safety Administration

1200 New Jersey Avenue, SE
Washington, D.C. 20590
888-327-4236
888-275-9171 (TTY)

Twitter
LinkedIn
Facebook
Youtube
Instagram

U.S. Department of Transportation
Submit Feedback >
