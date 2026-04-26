Skip to main content
U.S. Department of Energy - Energy Efficiency and Renewable Energy
U.S. Environmental Protection Agency - Office of Transportation and Air Quality
www.fueleconomy.gov - the official government source for fuel economy information

    Español Site Map General Links FAQ Videos

    Find a Car
    Save Money & Fuel
    Benefits
    My MPG
    Advanced Cars & Fuels
    About EPA Ratings
    More Topics

FuelEconomy.gov Web Services
Join our data users mail list to receive the latest news about FuelEconomy.gov data services including Web services and downloads. To join, send an email to FuelEconomyNews-join@elist.ornl.gov.

In order to make estimates comparable across model years, the MPG estimates for all 1984–2007 model year vehicles and some 2011–2016 model year vehicles have been revised. Learn More
Resources
Basic Data Services

Data are available in either JSON or XML format.

    /ws/rest/vehicle/{id} - Return a specific vehicle record
        Example URL: /ws/rest/vehicle/31873
    /ws/rest/vehicle/emissions/{id} - Return a list of emission records for specific vehicle record
        Example URL: /ws/rest/vehicle/emissions/31873
    /ws/rest/fuelprices - Return the fuel prices currently used by FuelEconomy.gov
        Example URL: /ws/rest/fuelprices
    /ws/rest/ympg/shared/ympgVehicle/{id} - Return a summary of shared user MPG data for a specific vehicle ID
        Example URL: /ws/rest/ympg/shared/ympgVehicle/26425
    /ws/rest/ympg/shared/ympgDriverVehicle/{id} - Return a list of user MPG records for a specific vehicle ID
        Example URL: /ws/rest/ympg/shared/ympgDriverVehicle/26425

Download the Vehicle Data

The Find a Car vehicle table contains fuel economy information for 1984-current model year vehicles. The data are available for download in CSV and XML formats:

    Vehicle
        CSV (Zipped)
        CSV (Unzipped)
        XML
    Emissions - linked to Vehicle by vehicle ID
        CSV (Zipped)
        CSV (Unzipped)
        XML
    EPA Revises MPG Estimates for 2013–17 Audi, Bentley, Porsche and Volkswagen vehicles
    EPA Revises MPG Estimates for 2014 MINI Cooper and Cooper S
    EPA Revises MPG Estimates for 2013-2014 Mercedes C300 4matic
    EPA Revises MPG Estimates for 2013-2014 Ford Vehicles
    2012-2013 Hyundai data revised
    2012-2013 Kia data revised
    Updated: Wednesday April 01 2026
    Additional data are available for download at: /feg/download.shtml

Find a Car Menus

The resources at /ws/rest/vehicle/menu/\* are used to select a vehicle using a series of four menus: year, make, model, and options. A menu consists of a list of menu items, each with two properties: text and value.

    /ws/rest/vehicle/menu/year - Returns a list of model years
        Example URL: /ws/rest/vehicle/menu/year
    /ws/rest/vehicle/menu/make?year=yyyy - Return a list of makes for a particular year
        Example URL: /ws/rest/vehicle/menu/make?year=2012
    /ws/rest/vehicle/menu/model?year=yyyy&make=mmmm - Return a list of models for a particular year and make
        Example URL: /ws/rest/vehicle/menu/model?year=2012&make=Honda
    ws/rest/vehicle/menu/options?year=2012&make=mmmm&model=nnnn - Return a list of model options and the associated vehicle ID for a particular year, make and model
        Example URL: ws/rest/vehicle/menu/options?year=2012&make=Honda&model=Fit

My MPG Shared Data

This resource provides access to My MPG shared data via vehicle make and model:

    Menu of shared make /ws/rest/ympg/shared/menu/make
    Menu of shared models for the specified make; for example /ws/rest/ympg/shared/menu/model?make=Honda
    List of vehicles with shared My MPG data; for example /ws/rest/ympg/shared/vehicles?make=Honda&model=Fit

Data Description
vehicle

    atvtype - type of alternative fuel or advanced technology vehicle
    barrels08 - annual petroleum consumption in barrels for fuelType1 (1)
    barrelsA08 - annual petroleum consumption in barrels for fuelType2 (1)
    charge120 - time to charge an electric vehicle in hours at 120 V
    charge240 - time to charge an electric vehicle in hours at 240 V
    city08 - city MPG for fuelType1 (2), (11)
    city08U - unrounded city MPG for fuelType1 (2), (3)
    cityA08 - city MPG for fuelType2 (2)
    cityA08U - unrounded city MPG for fuelType2 (2), (3)
    cityCD - city gasoline consumption (gallons/100 miles) in charge depleting mode (4)
    cityE - city electricity consumption in kWh/100 miles
    cityMpk - city miles per kilogram for hydrogen
    cityUmpk - unrounded city miles per kilogram for hydrogen
    cityUF - EPA city utility factor (share of electricity) for PHEV
    co2 - tailpipe CO2 in grams/mile for fuelType1 (5)
    co2A - tailpipe CO2 in grams/mile for fuelType2 (5)
    co2TailpipeAGpm - tailpipe CO2 in grams/mile for fuelType2 (5)
    co2TailpipeGpm- tailpipe CO2 in grams/mile for fuelType1 (5)
    comb08 - combined MPG for fuelType1 (2), (11)
    comb08U - unrounded combined MPG for fuelType1 (2), (3)
    combA08 - combined MPG for fuelType2 (2)
    combA08U - unrounded combined MPG for fuelType2 (2), (3)
    combE - combined electricity consumption in kWh/100 miles
    combMpk - combined miles per kilogram for hydrogen
    combUmpk - unrounded combined miles per kilogram for hydrogen
    combinedCD - combined gasoline consumption (gallons/100 miles) in charge depleting mode (4)
    combinedUF - EPA combined utility factor (share of electricity) for PHEV
    cylinders - engine cylinders
    displ - engine displacement in liters
    drive - drive axle type
    emissionsList
    engId - EPA model type index
    eng_dscr - engine descriptor; see http://www.fueleconomy.gov/feg/findacarhelp.shtml#engine
    evMotor - electric motor (kW)
    feScore - EPA Fuel Economy Score (-1 = Not available)
    fuelCost08 - annual fuel cost for fuelType1 ($) (7)
    fuelCostA08 - annual fuel cost for fuelType2 ($) (7)
    fuelType - fuel type with fuelType1 and fuelType2 (if applicable)
    fuelType1 - fuel type 1. For single fuel vehicles, this will be the only fuel. For dual fuel vehicles, this will be the conventional fuel.
    fuelType2 - fuel type 2. For dual fuel vehicles, this will be the alternative fuel (e.g., E85, Electricity, CNG, LPG). For single fuel vehicles, this field is not used
    ghgScore - EPA GHG score (-1 = Not available)
    ghgScoreA - EPA GHG score for dual fuel vehicle running on the alternative fuel (-1 = Not available)
    guzzler- if G or T, this vehicle is subject to the gas guzzler tax
    highway08 - highway MPG for fuelType1 (2), (11)
    highway08U - unrounded highway MPG for fuelType1 (2), (3)
    highwayA08 - highway MPG for fuelType2 (2)
    highwayA08U - unrounded highway MPG for fuelType2 (2),(3)
    highwayCD - highway gasoline consumption (gallons/100 miles) in charge depleting mode (4)
    highwayE - highway electricity consumption in kWh/100 miles
    highwayMpk - highway miles per kilogram for hydrogen
    highwayUmpk - unrounded highway miles per kilogram for hydrogen
    highwayUF - EPA highway utility factor (share of electricity) for PHEV
    hlv - hatchback luggage volume (cubic feet) (8)
    hpv - hatchback passenger volume (cubic feet) (8)
    id - vehicle record id
    lv2 - 2-door luggage volume (cubic feet) (8)
    lv4 - 4-door luggage volume (cubic feet) (8)
    make - manufacturer (division)
    mfrCode - 3-character manufacturer code
    model - model name (carline)
    mpgData - has My MPG data; see yourMpgVehicle and yourMpgDriverVehicle
    phevBlended - if true, this vehicle operates on a blend of gasoline and electricity in charge depleting mode
    pv2 - 2-door passenger volume (cubic feet) (8)
    pv4 - 4-door passenger volume (cubic feet) (8)
    rangeA - EPA range for fuelType2
    rangeCityA - EPA city range for fuelType2
    rangeHwyA - EPA highway range for fuelType2
    trans_dscr - transmission descriptor; see http://www.fueleconomy.gov/feg/findacarhelp.shtml#trany
    trany - transmission
    UCity - unadjusted city MPG for fuelType1; see the description of the EPA test procedures
    UCityA - unadjusted city MPG for fuelType2; see the description of the EPA test procedures
    UHighway - unadjusted highway MPG for fuelType1; see the description of the EPA test procedures
    UHighwayA - unadjusted highway MPG for fuelType2; see the description of the EPA test procedures
    VClass - EPA vehicle size class
    year - model year
    youSaveSpend - you save/spend over 5 years compared to an average car ($). Savings are positive; a greater amount spent yields a negative number. For dual fuel vehicles, this is the cost savings for gasoline
    sCharger - if S, this vehicle is supercharged
    tCharger - if T, this vehicle is turbocharged
    c240Dscr - electric vehicle charger description
    charge240b - time to charge an electric vehicle in hours at 240 V using the alternate charger
    c240bDscr - electric vehicle alternate charger description
    createdOn - date the vehicle record was created
    modifiedOn - date the vehicle record was last modified
    startStop - vehicle has stop-start technology (Y, N, or blank for older vehicles)
    phevCity - EPA composite gasoline-electricity city MPGe for plug-in hybrid vehicles
    phevHwy - EPA composite gasoline-electricity highway MPGe for plug-in hybrid vehicles
    phevComb - EPA composite gasoline-electricity combined city-highway MPGe for plug-in hybrid vehicles
    basemodel - base model name

emissions

    emissionsList
        emissionsInfo
            efid - engine family ID
            id - vehicle record ID (links emission data to the vehicle record)
            salesArea - EPA sales area code
            score - EPA smog rating from 1 (worst) to 10 (best) for fuelType1
            scoreAlt - EPA smog rating from 1 (worst) to 10 (best) for fuelType2
            smartwayScore - SmartWay Code
            standard - Vehicle Emission Standard Code
            stdText - Vehicle Emission Standard

fuel prices

    fuelPrices
        midgrade - dollars per gallon of midgrade gasoline(9)
        premium - dollars per gallon of premium gasoline(9)
        regular - dollars per gallon of regular gasoline(9)
        cng - dollars per gallon of gasoline equivalent (GGE) of compressed natural gas(10)
        diesel - dollars per gallon of diesel(9)
        e85 - dollars per gallon of E85(10)
        electric - dollars per kWh of electricity(10)
        lpg - dollars per gallon of propane(10)

yourMpgVehicle - summary of all My MPG data for this vehicle

    avgMpg - harmonic mean of average MPG shared by fueleconomy.gov users
    cityPercent - average percentage of city miles
    highwayPercent - average percentage of highway miles
    maxMpg - maximum user average MPG
    minMpg - minimum user average MPG
    recordCount - number of records for this vehicle
    vehicleId - vehicle record id (links My MPG data to the vehicle record)

yourMpgDriverVehicle - summary of driver data reported for this vehicle

    cityPercent - user average percentage of city miles
    highwayPercent - user average percentage of highway miles
    lastDate - date records were last updated (yyyy-mm-dd)
    mpg - average MPG
    state - state of residence
    vehicleId - vehicle record ID (links My MPG data to the vehicle record)

Footnotes:

(1) 1 barrel = 42 gallons. Petroleum consumption is estimated using the Department of Energy's GREET model and includes petroleum consumed from production and refining to distribution and final use. Vehicle manufacture is excluded.

(2) The MPG estimates for all 1985–2007 model year vehicles and some 2011–2016 model year vehicles have been updated. Learn More

(3) Unrounded MPG values are not available for some vehicles.

(4) This field is only used for blended PHEV vehicles.

(5) For model year 2013 and beyond, tailpipe CO2 is based on EPA tests. For previous model years, CO2 is estimated using an EPA emission factor. -1 = Not Available.

(6) For PHEVs this is the charge depleting range.

(7) Annual fuel cost is based on 15,000 miles, 55% city driving, and the price of fuel used by the vehicle.

(8) Interior volume dimensions are not required for two-seater passenger cars or any vehicle classified as truck which includes vans, pickups, special purpose vehicles, minivan and sport utility vehicles.

(9) Fuel prices for gasoline and diesel fuel are from the Energy Information Administration and are usually updated weekly.

(10) Fuel prices for E85, LPG, and CNG are from the Office of Energy Efficiency and Renewable Energy's Alternative Fuel Price Report and are updated quarterly.

(11) For electric and CNG vehicles this number is MPGe (gasoline equivalent miles per gallon).
Also In This Section…

    FuelEconomy.gov Web Services
    Tax Incentive Data Services

Contacts | Download EPA's MPG Ratings | Find and Compare Cars | USA.gov | Info for Auto Dealers | Privacy/Security | Feedback

This website is administered by Oak Ridge National Laboratory for the U.S. Department of Energy and the U.S. Environmental Protection Agency.
